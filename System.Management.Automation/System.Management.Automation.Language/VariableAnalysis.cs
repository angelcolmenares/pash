namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class VariableAnalysis : ICustomAstVisitor
    {
        private static readonly ConcurrentDictionary<string, bool> _allScopeVariables = new ConcurrentDictionary<string, bool>(1, 0x10, StringComparer.OrdinalIgnoreCase);
        private Block _currentBlock;
        private bool _disableOptimizations;
        private Block _entryBlock;
        private Block _exitBlock;
        private int _localsAllocated;
        private readonly List<LoopGotoTargets> _loopTargets = new List<LoopGotoTargets>();
        private Dictionary<string, VariableAnalysisDetails> _variables;
        internal const int ForceDynamic = -2;
        internal const int Unanalyzed = -1;

        internal static Tuple<Type, Dictionary<string, int>> Analyze(IParameterMetadataProvider ast, bool disableOptimizations, bool scriptCmdlet)
        {
            return new VariableAnalysis().AnalyzeImpl(ast, disableOptimizations, scriptCmdlet);
        }

        private void AnalyzeBlock(BitArray assignedBitArray, Block block)
        {
            foreach (Ast ast in block._asts)
            {
                VariableExpressionAst ast2 = ast as VariableExpressionAst;
                if (ast2 != null)
                {
                    VariablePath variablePath = ast2.VariablePath;
                    if (variablePath.IsAnyLocal())
                    {
                        string unaliasedVariableName = GetUnaliasedVariableName(variablePath);
                        VariableAnalysisDetails details = this._variables[unaliasedVariableName];
                        if (details.Automatic)
                        {
                            ast2.TupleIndex = details.LocalTupleIndex;
                            ast2.Automatic = true;
                        }
                        else
                        {
                            ast2.TupleIndex = (assignedBitArray[details.BitIndex] && !details.PreferenceVariable) ? details.LocalTupleIndex : -2;
                        }
                    }
                }
                else
                {
                    AssignmentTarget target = ast as AssignmentTarget;
                    if (target != null)
                    {
                        if (target._targetAst != null)
                        {
                            this.CheckLHSAssign(target._targetAst, assignedBitArray);
                        }
                        else
                        {
                            this.CheckLHSAssignVar(target._variableName, assignedBitArray, target._type);
                        }
                    }
                    else
                    {
                        DataStatementAst item = ast as DataStatementAst;
                        if (item != null)
                        {
                            VariableAnalysisDetails details2 = this.CheckLHSAssignVar(item.Variable, assignedBitArray, typeof(object));
                            item.TupleIndex = details2.LocalTupleIndex;
                            details2.AssociatedAsts.Add(item);
                        }
                    }
                }
            }
        }

        internal static Tuple<Type, Dictionary<string, int>> AnalyzeExpression(ExpressionAst exprAst)
        {
            return new VariableAnalysis().AnalyzeImpl(exprAst);
        }

        private Tuple<Type, Dictionary<string, int>> AnalyzeImpl(ExpressionAst exprAst)
        {
            this._variables = FindAllVariablesVisitor.Visit(exprAst);
            this._disableOptimizations = true;
            this.Init();
            this._localsAllocated = SpecialVariables.AutomaticVariables.Length;
            this._currentBlock = this._entryBlock;
            exprAst.Accept(this);
            this._currentBlock.FlowsTo(this._exitBlock);
            return this.FinishAnalysis(false);
        }

        private Tuple<Type, Dictionary<string, int>> AnalyzeImpl(TrapStatementAst trap)
        {
            this._variables = FindAllVariablesVisitor.Visit(trap);
            this._disableOptimizations = true;
            this.Init();
            this._localsAllocated = SpecialVariables.AutomaticVariables.Length;
            this._currentBlock = this._entryBlock;
            trap.Body.Accept(this);
            this._currentBlock.FlowsTo(this._exitBlock);
            return this.FinishAnalysis(false);
        }

        private Tuple<Type, Dictionary<string, int>> AnalyzeImpl(IParameterMetadataProvider ast, bool disableOptimizations, bool scriptCmdlet)
        {
            this._variables = FindAllVariablesVisitor.Visit(ast, disableOptimizations, scriptCmdlet, out this._localsAllocated, out this._disableOptimizations);
            this.Init();
            if (ast.Parameters != null)
            {
                foreach (ParameterAst ast2 in ast.Parameters)
                {
                    VariablePath variablePath = ast2.Name.VariablePath;
                    if (variablePath.IsAnyLocal())
                    {
                        bool flag = false;
                        int num = -1;
                        Type c = null;
                        foreach (AttributeBaseAst ast3 in ast2.Attributes)
                        {
                            if (ast3 is TypeConstraintAst)
                            {
                                num++;
                                if (c == null)
                                {
                                    c = ast3.TypeName.GetReflectionType();
                                }
                            }
                            else
                            {
                                Type reflectionAttributeType = ast3.TypeName.GetReflectionAttributeType();
                                if (typeof(ValidateArgumentsAttribute).IsAssignableFrom(reflectionAttributeType) || typeof(ArgumentTransformationAttribute).IsAssignableFrom(reflectionAttributeType))
                                {
                                    flag = true;
                                }
                            }
                        }
                        string unaliasedVariableName = GetUnaliasedVariableName(variablePath);
                        VariableAnalysisDetails details = this._variables[unaliasedVariableName];
                        c = c ?? (details.Type ?? typeof(object));
                        if (((flag || (num > 0)) || (typeof(PSReference).IsAssignableFrom(c) || MustBeBoxed(c))) && (!details.Automatic && !details.PreferenceVariable))
                        {
                            details.LocalTupleIndex = -2;
                        }
                        this._entryBlock.AddAst(new AssignmentTarget(unaliasedVariableName, c));
                    }
                }
            }
            ast.Body.Accept(this);
            return this.FinishAnalysis(scriptCmdlet);
        }

        internal static Tuple<Type, Dictionary<string, int>> AnalyzeTrap(TrapStatementAst trap)
        {
            return new VariableAnalysis().AnalyzeImpl(trap);
        }

        internal static bool AnyVariablesCouldBeAllScope(Dictionary<string, int> variableNames)
        {
            return variableNames.Any<KeyValuePair<string, int>>(keyValuePair => _allScopeVariables.ContainsKey(keyValuePair.Key));
        }

        private void BreakOrContinue(ExpressionAst label, Func<LoopGotoTargets, Block> fieldSelector)
        {
            Func<LoopGotoTargets, Block> selector = null;
            Block next = null;
            if (label != null)
            {
                label.Accept(this);
                if (this._loopTargets.Any<LoopGotoTargets>())
                {
                    Func<LoopGotoTargets, bool> predicate = null;
                    StringConstantExpressionAst labelStrAst = label as StringConstantExpressionAst;
                    if (labelStrAst != null)
                    {
                        if (predicate == null)
                        {
                            predicate = t => t.Label.Equals(labelStrAst.Value, StringComparison.OrdinalIgnoreCase);
                        }
                        if (selector == null)
                        {
                            selector = t => fieldSelector(t);
                        }
                        next = this._loopTargets.Where<LoopGotoTargets>(predicate).Select<LoopGotoTargets, Block>(selector).LastOrDefault<Block>();
                    }
                }
            }
            else if (this._loopTargets.Count > 0)
            {
                next = fieldSelector(this._loopTargets.Last<LoopGotoTargets>());
            }
            if (next != null)
            {
                this._currentBlock.FlowsTo(next);
            }
            this._currentBlock = new Block();
        }

        private void CheckLHSAssign(ExpressionAst lhs, BitArray assignedBitArray)
        {
            ConvertExpressionAst ast = lhs as ConvertExpressionAst;
            Type convertType = null;
            if (ast != null)
            {
                lhs = ast.Child;
                convertType = ast.StaticType;
            }
            VariableExpressionAst item = lhs as VariableExpressionAst;
            VariablePath variablePath = item.VariablePath;
            if (variablePath.IsAnyLocal())
            {
                string unaliasedVariableName = GetUnaliasedVariableName(variablePath);
                if ((convertType == null) && (unaliasedVariableName.Equals("foreach", StringComparison.OrdinalIgnoreCase) || unaliasedVariableName.Equals("switch", StringComparison.OrdinalIgnoreCase)))
                {
                    convertType = typeof(object);
                }
                VariableAnalysisDetails details = this.CheckLHSAssignVar(unaliasedVariableName, assignedBitArray, convertType);
                details.AssociatedAsts.Add(item);
                item.TupleIndex = details.LocalTupleIndex;
                item.Automatic = details.Automatic;
            }
            else
            {
                item.TupleIndex = -2;
            }
        }

        private VariableAnalysisDetails CheckLHSAssignVar(string variableName, BitArray assignedBitArray, Type convertType)
        {
            VariableAnalysisDetails details = this._variables[variableName];
            if (details.LocalTupleIndex == -1)
            {
                details.LocalTupleIndex = (this._disableOptimizations || _allScopeVariables.ContainsKey(variableName)) ? -2 : this._localsAllocated++;
            }
            if ((convertType != null) && MustBeBoxed(convertType))
            {
                details.LocalTupleIndex = -2;
            }
            Type o = details.Type;
            if (o == null)
            {
                details.Type = convertType ?? typeof(object);
            }
            else
            {
                if (!assignedBitArray[details.BitIndex] && (convertType == null))
                {
                    convertType = typeof(object);
                }
                if ((convertType != null) && !convertType.Equals(o))
                {
                    if (details.Automatic || details.PreferenceVariable)
                    {
                        details.Type = typeof(object);
                    }
                    else
                    {
                        details.LocalTupleIndex = -2;
                    }
                }
            }
            assignedBitArray.Set(details.BitIndex, true);
            return details;
        }

        private void ControlFlowStatement(PipelineBaseAst pipelineAst)
        {
            if (pipelineAst != null)
            {
                pipelineAst.Accept(this);
            }
            this._currentBlock.FlowsTo(this._exitBlock);
            this._currentBlock = new Block();
        }

        private Tuple<Type, Dictionary<string, int>> FinishAnalysis(bool scriptCmdlet = false)
        {
            List<Block> list = Block.GenerateReverseDepthFirstOrder(this._entryBlock);
            BitArray assignedBitArray = new BitArray(this._variables.Count);
            list[0]._visitData = assignedBitArray;
            this.AnalyzeBlock(assignedBitArray, list[0]);
            for (int i = 1; i < list.Count; i++)
            {
                Block block = list[i];
                assignedBitArray = new BitArray(this._variables.Count);
                assignedBitArray.SetAll(true);
                block._visitData = assignedBitArray;
                int num2 = 0;
                foreach (Block block2 in block._predecessors)
                {
                    if (block2._visitData != null)
                    {
                        num2++;
                        assignedBitArray.And((BitArray) block2._visitData);
                    }
                }
                this.AnalyzeBlock(assignedBitArray, block);
            }
            var v = this._variables.Values.Where(x => x.LocalTupleIndex == -2).SelectMany(x => x.AssociatedAsts);
            foreach (Ast ast in v)
            {
                FixTupleIndex(ast, -2);
            }
            VariableAnalysisDetails[] detailsArray = (from details in this._variables.Values
                where details.LocalTupleIndex >= 0
                orderby details.LocalTupleIndex
                select details).ToArray<VariableAnalysisDetails>();
            Dictionary<string, int> dictionary = new Dictionary<string, int>(0, StringComparer.OrdinalIgnoreCase);
            for (int j = 0; j < detailsArray.Length; j++)
            {
                VariableAnalysisDetails details = detailsArray[j];
                string name = details.Name;
                dictionary.Add(name, j);
                if (details.LocalTupleIndex != j)
                {
                    foreach (Ast ast2 in details.AssociatedAsts)
                    {
                        FixTupleIndex(ast2, j);
                    }
                }
            }
            return Tuple.Create<Type, Dictionary<string, int>>(MutableTuple.MakeTupleType((from l in detailsArray select l.Type).ToArray<Type>()), dictionary);
        }

        private static void FixTupleIndex(Ast ast, int newIndex)
        {
            VariableExpressionAst ast2 = ast as VariableExpressionAst;
            if (ast2 != null)
            {
                if (ast2.TupleIndex != -2)
                {
                    ast2.TupleIndex = newIndex;
                }
            }
            else
            {
                DataStatementAst ast3 = ast as DataStatementAst;
                if ((ast3 != null) && (ast3.TupleIndex != -2))
                {
                    ast3.TupleIndex = newIndex;
                }
            }
        }

        private void GenerateDoLoop(LoopStatementAst loopStatement)
        {
            Block continueTarget = new Block();
            Block next = new Block();
            Block breakTarget = new Block();
            Block block4 = new Block();
            this._loopTargets.Add(new LoopGotoTargets(loopStatement.Label ?? "", breakTarget, continueTarget));
            this._currentBlock.FlowsTo(next);
            this._currentBlock = next;
            loopStatement.Body.Accept(this);
            this._currentBlock.FlowsTo(continueTarget);
            this._currentBlock = continueTarget;
            loopStatement.Condition.Accept(this);
            this._currentBlock.FlowsTo(breakTarget);
            this._currentBlock.FlowsTo(block4);
            this._currentBlock = block4;
            this._currentBlock.FlowsTo(next);
            this._currentBlock = breakTarget;
            this._loopTargets.RemoveAt(this._loopTargets.Count - 1);
        }

        private void GenerateWhileLoop(string loopLabel, Action generateCondition, Action generateLoopBody, Ast continueAction = null)
        {
            Block next = new Block();
            if (continueAction != null)
            {
                Block block2 = new Block();
                this._currentBlock.FlowsTo(block2);
                this._currentBlock = next;
                continueAction.Accept(this);
                this._currentBlock.FlowsTo(block2);
                this._currentBlock = block2;
            }
            else
            {
                this._currentBlock.FlowsTo(next);
                this._currentBlock = next;
            }
            Block block3 = new Block();
            Block block4 = new Block();
            if (generateCondition != null)
            {
                generateCondition();
                this._currentBlock.FlowsTo(block4);
            }
            this._loopTargets.Add(new LoopGotoTargets(loopLabel ?? "", block4, next));
            this._currentBlock.FlowsTo(block3);
            this._currentBlock = block3;
            generateLoopBody();
            this._currentBlock.FlowsTo(next);
            this._currentBlock = block4;
            this._loopTargets.RemoveAt(this._loopTargets.Count - 1);
        }

        private static IEnumerable<ExpressionAst> GetAssignmentTargets(ExpressionAst expressionAst)
        {
            ParenExpressionAst iteratorVariable0 = expressionAst as ParenExpressionAst;
            if (iteratorVariable0 != null)
            {
                foreach (ExpressionAst iteratorVariable1 in GetAssignmentTargets(iteratorVariable0.Pipeline.GetPureExpression()))
                {
                    yield return iteratorVariable1;
                }
            }
            else
            {
                ArrayLiteralAst iteratorVariable2 = expressionAst as ArrayLiteralAst;
                if (iteratorVariable2 != null)
                {
                    foreach (ExpressionAst iteratorVariable3 in iteratorVariable2.Elements.SelectMany<ExpressionAst, ExpressionAst>(new Func<ExpressionAst, IEnumerable<ExpressionAst>>(VariableAnalysis.GetAssignmentTargets)))
                    {
                        yield return iteratorVariable3;
                    }
                }
                else
                {
                    yield return expressionAst;
                }
            }
        }

        internal static string GetUnaliasedVariableName(VariablePath varPath)
        {
            return GetUnaliasedVariableName(varPath.UnqualifiedPath);
        }

        internal static string GetUnaliasedVariableName(string varName)
        {
            if (!varName.Equals("PSItem", StringComparison.OrdinalIgnoreCase))
            {
                return varName;
            }
            return "_";
        }

        private void Init()
        {
            this._entryBlock = new Block();
            this._exitBlock = new Block();
        }

        private static bool MustBeBoxed(Type type)
        {
            return ((type.IsValueType && PSVariableAssignmentBinder.IsValueTypeMutable(type)) && !typeof(SwitchParameter).Equals(type));
        }

        internal static void NoteAllScopeVariable(string variableName)
        {
            _allScopeVariables.GetOrAdd(variableName, true);
        }

        public object VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
        {
            arrayExpressionAst.SubExpression.Accept(this);
            return null;
        }

        public object VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
            foreach (ExpressionAst ast in arrayLiteralAst.Elements)
            {
                ast.Accept(this);
            }
            return null;
        }

        public object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            assignmentStatementAst.Right.Accept(this);
            foreach (ExpressionAst ast in GetAssignmentTargets(assignmentStatementAst.Left))
            {
                bool flag = false;
                int num = 0;
                ExpressionAst child = ast;
                while (child is AttributedExpressionAst)
                {
                    num++;
                    if (!(child is ConvertExpressionAst))
                    {
                        flag = true;
                    }
                    child = ((AttributedExpressionAst) child).Child;
                }
                if (child is VariableExpressionAst)
                {
                    if (flag || (num > 1))
                    {
                        VariablePath variablePath = ((VariableExpressionAst) child).VariablePath;
                        if (variablePath.IsAnyLocal())
                        {
                            VariableAnalysisDetails details = this._variables[GetUnaliasedVariableName(variablePath)];
                            details.LocalTupleIndex = -2;
                        }
                    }
                    else
                    {
                        this._currentBlock.AddAst(new AssignmentTarget(ast));
                    }
                }
                else
                {
                    ast.Accept(this);
                }
            }
            return null;
        }

        public object VisitAttribute(AttributeAst attributeAst)
        {
            return null;
        }

        public object VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
        {
            attributedExpressionAst.Child.Accept(this);
            return null;
        }

        public object VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            if ((binaryExpressionAst.Operator == TokenKind.And) || (binaryExpressionAst.Operator == TokenKind.Or))
            {
                binaryExpressionAst.Left.Accept(this);
                Block next = new Block();
                Block block2 = new Block();
                this._currentBlock.FlowsTo(next);
                this._currentBlock.FlowsTo(block2);
                this._currentBlock = block2;
                binaryExpressionAst.Right.Accept(this);
                this._currentBlock.FlowsTo(next);
                this._currentBlock = next;
            }
            else
            {
                binaryExpressionAst.Left.Accept(this);
                binaryExpressionAst.Right.Accept(this);
            }
            return null;
        }

        public object VisitBlockStatement(BlockStatementAst blockStatementAst)
        {
            blockStatementAst.Body.Accept(this);
            return null;
        }

        public object VisitBreakStatement(BreakStatementAst breakStatementAst)
        {
            this.BreakOrContinue(breakStatementAst.Label, t => t.BreakTarget);
            return null;
        }

        public object VisitCatchClause(CatchClauseAst catchClauseAst)
        {
            catchClauseAst.Body.Accept(this);
            return null;
        }

        public object VisitCommand(CommandAst commandAst)
        {
            foreach (CommandElementAst ast in commandAst.CommandElements)
            {
                ast.Accept(this);
            }
            return null;
        }

        public object VisitCommandExpression(CommandExpressionAst commandExpressionAst)
        {
            commandExpressionAst.Expression.Accept(this);
            return null;
        }

        public object VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            if (commandParameterAst.Argument != null)
            {
                commandParameterAst.Argument.Accept(this);
            }
            return null;
        }

        public object VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            return null;
        }

        public object VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            this.BreakOrContinue(continueStatementAst.Label, t => t.ContinueTarget);
            return null;
        }

        public object VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
        {
            convertExpressionAst.Child.Accept(this);
            return null;
        }

        public object VisitDataStatement(DataStatementAst dataStatementAst)
        {
            dataStatementAst.Body.Accept(this);
            if (dataStatementAst.Variable != null)
            {
                this._currentBlock.AddAst(dataStatementAst);
            }
            return null;
        }

        public object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
        {
            this.GenerateDoLoop(doUntilStatementAst);
            return null;
        }

        public object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
        {
            this.GenerateDoLoop(doWhileStatementAst);
            return null;
        }

        public object VisitErrorExpression(ErrorExpressionAst errorExpressionAst)
        {
            return null;
        }

        public object VisitErrorStatement(ErrorStatementAst errorStatementAst)
        {
            return null;
        }

        public object VisitExitStatement(ExitStatementAst exitStatementAst)
        {
            this.ControlFlowStatement(exitStatementAst.Pipeline);
            return null;
        }

        public object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
        {
            foreach (ExpressionAst ast in expandableStringExpressionAst.NestedExpressions)
            {
                ast.Accept(this);
            }
            return null;
        }

        public object VisitFileRedirection(FileRedirectionAst fileRedirectionAst)
        {
            fileRedirectionAst.Location.Accept(this);
            return null;
        }

        public object VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            VariableAnalysisDetails details = this._variables["foreach"];
            if ((details.LocalTupleIndex == -1) && !this._disableOptimizations)
            {
                details.LocalTupleIndex = this._localsAllocated++;
            }
            Block afterFor = new Block();
            Action generateCondition = delegate {
                forEachStatementAst.Condition.Accept(this);
                this._currentBlock.FlowsTo(afterFor);
                this._currentBlock.AddAst(new AssignmentTarget("foreach", typeof(IEnumerator)));
                this._currentBlock.AddAst(new AssignmentTarget(forEachStatementAst.Variable));
            };
            this.GenerateWhileLoop(forEachStatementAst.Label, generateCondition, delegate {
                forEachStatementAst.Body.Accept(this);
            }, null);
            this._currentBlock.FlowsTo(afterFor);
            this._currentBlock = afterFor;
            return null;
        }

        public object VisitForStatement(ForStatementAst forStatementAst)
        {
            if (forStatementAst.Initializer != null)
            {
                forStatementAst.Initializer.Accept(this);
            }
            Action generateCondition = null;
            if (forStatementAst.Condition != null) 
            {
                generateCondition = delegate
                {
                    forStatementAst.Condition.Accept(this);
                };
            }
            this.GenerateWhileLoop(forStatementAst.Label, generateCondition, delegate {
                forStatementAst.Body.Accept(this);
            }, forStatementAst.Iterator);
            return null;
        }

        public object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            return null;
        }

        public object VisitHashtable(HashtableAst hashtableAst)
        {
            foreach (Tuple<ExpressionAst, StatementAst> tuple in hashtableAst.KeyValuePairs)
            {
                tuple.Item1.Accept(this);
                tuple.Item2.Accept(this);
            }
            return null;
        }

        public object VisitIfStatement(IfStatementAst ifStmtAst)
        {
            Block next = new Block();
            int count = ifStmtAst.Clauses.Count;
            for (int i = 0; i < count; i++)
            {
                Tuple<PipelineBaseAst, StatementBlockAst> tuple = ifStmtAst.Clauses[i];
                bool flag = (i == (count - 1)) && (ifStmtAst.ElseClause == null);
                Block block2 = new Block();
                Block block3 = flag ? next : new Block();
                tuple.Item1.Accept(this);
                this._currentBlock.FlowsTo(block2);
                this._currentBlock.FlowsTo(block3);
                this._currentBlock = block2;
                tuple.Item2.Accept(this);
                this._currentBlock.FlowsTo(next);
                this._currentBlock = block3;
            }
            if (ifStmtAst.ElseClause != null)
            {
                ifStmtAst.ElseClause.Accept(this);
                this._currentBlock.FlowsTo(next);
            }
            this._currentBlock = next;
            return null;
        }

        public object VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            indexExpressionAst.Target.Accept(this);
            indexExpressionAst.Index.Accept(this);
            return null;
        }

        public object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst)
        {
            invokeMemberExpressionAst.Expression.Accept(this);
            invokeMemberExpressionAst.Member.Accept(this);
            if (invokeMemberExpressionAst.Arguments != null)
            {
                foreach (ExpressionAst ast in invokeMemberExpressionAst.Arguments)
                {
                    ast.Accept(this);
                }
            }
            return null;
        }

        public object VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            memberExpressionAst.Expression.Accept(this);
            memberExpressionAst.Member.Accept(this);
            return null;
        }

        public object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst)
        {
            return null;
        }

        public object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst)
        {
            return null;
        }

        public object VisitNamedBlock(NamedBlockAst namedBlockAst)
        {
            return this.VisitStatementBlock(namedBlockAst.Statements);
        }

        public object VisitParamBlock(ParamBlockAst paramBlockAst)
        {
            return null;
        }

        public object VisitParameter(ParameterAst parameterAst)
        {
            return null;
        }

        public object VisitParenExpression(ParenExpressionAst parenExpressionAst)
        {
            parenExpressionAst.Pipeline.Accept(this);
            return null;
        }

        public object VisitPipeline(PipelineAst pipelineAst)
        {
            bool flag = false;
            foreach (CommandBaseAst ast in pipelineAst.PipelineElements)
            {
                ast.Accept(this);
                if (ast is CommandAst)
                {
                    flag = true;
                }
                foreach (RedirectionAst ast2 in ast.Redirections)
                {
                    ast2.Accept(this);
                }
            }
            if (flag && this._loopTargets.Any<LoopGotoTargets>())
            {
                foreach (LoopGotoTargets targets in this._loopTargets)
                {
                    this._currentBlock.FlowsTo(targets.BreakTarget);
                    this._currentBlock.FlowsTo(targets.ContinueTarget);
                }
                Block next = new Block();
                this._currentBlock.FlowsTo(next);
                this._currentBlock = next;
            }
            return null;
        }

        public object VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            this.ControlFlowStatement(returnStatementAst.Pipeline);
            return null;
        }

        public object VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            this._currentBlock = this._entryBlock;
            if (scriptBlockAst.DynamicParamBlock != null)
            {
                scriptBlockAst.DynamicParamBlock.Accept(this);
            }
            if (scriptBlockAst.BeginBlock != null)
            {
                scriptBlockAst.BeginBlock.Accept(this);
            }
            if (scriptBlockAst.ProcessBlock != null)
            {
                scriptBlockAst.ProcessBlock.Accept(this);
            }
            if (scriptBlockAst.EndBlock != null)
            {
                scriptBlockAst.EndBlock.Accept(this);
            }
            this._currentBlock.FlowsTo(this._exitBlock);
            return null;
        }

        public object VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            return null;
        }

        private object VisitStatementBlock(ReadOnlyCollection<StatementAst> statements)
        {
            foreach (StatementAst ast in statements)
            {
                ast.Accept(this);
            }
            return null;
        }

        public object VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            return this.VisitStatementBlock(statementBlockAst.Statements);
        }

        public object VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            return null;
        }

        public object VisitSubExpression(SubExpressionAst subExpressionAst)
        {
            subExpressionAst.SubExpression.Accept(this);
            return null;
        }

        public object VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            VariableAnalysisDetails details = this._variables["switch"];
            if ((details.LocalTupleIndex == -1) && !this._disableOptimizations)
            {
                details.LocalTupleIndex = this._localsAllocated++;
            }
            Action generateCondition = delegate {
                switchStatementAst.Condition.Accept(this);
                this._currentBlock.AddAst(new AssignmentTarget("switch", typeof(IEnumerator)));
            };
            Action generateLoopBody = delegate {
                bool flag = switchStatementAst.Default != null;
                Block next = new Block();
                int count = switchStatementAst.Clauses.Count;
                for (int j = 0; j < count; j++)
                {
                    Tuple<ExpressionAst, StatementBlockAst> tuple = switchStatementAst.Clauses[j];
                    Block block2 = new Block();
                    bool flag2 = (j == (count - 1)) && !flag;
                    Block block3 = flag2 ? next : new Block();
                    tuple.Item1.Accept(this);
                    this._currentBlock.FlowsTo(block3);
                    this._currentBlock.FlowsTo(block2);
                    this._currentBlock = block2;
                    tuple.Item2.Accept(this);
                    if (!flag2)
                    {
                        this._currentBlock.FlowsTo(block3);
                        this._currentBlock = block3;
                    }
                }
                if (flag)
                {
                    this._currentBlock.FlowsTo(next);
                    switchStatementAst.Default.Accept(this);
                }
                this._currentBlock.FlowsTo(next);
                this._currentBlock = next;
            };
            this.GenerateWhileLoop(switchStatementAst.Label, generateCondition, generateLoopBody, null);
            return null;
        }

        public object VisitThrowStatement(ThrowStatementAst throwStatementAst)
        {
            this.ControlFlowStatement(throwStatementAst.Pipeline);
            return null;
        }

        public object VisitTrap(TrapStatementAst trapStatementAst)
        {
            trapStatementAst.Body.Accept(this);
            return null;
        }

        public object VisitTryStatement(TryStatementAst tryStatementAst)
        {
            Block block = this._currentBlock;
            this._currentBlock = new Block();
            block.FlowsTo(this._currentBlock);
            tryStatementAst.Body.Accept(this);
            Block block2 = new Block();
            foreach (CatchClauseAst ast in tryStatementAst.CatchClauses)
            {
                this._currentBlock = new Block();
                block.FlowsTo(this._currentBlock);
                ast.Accept(this);
            }
            this._currentBlock = block2;
            block.FlowsTo(this._currentBlock);
            if (tryStatementAst.Finally != null)
            {
                tryStatementAst.Finally.Accept(this);
                Block next = new Block();
                this._currentBlock.FlowsTo(next);
                this._currentBlock = next;
            }
            return null;
        }

        public object VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            return null;
        }

        public object VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            return null;
        }

        public object VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            unaryExpressionAst.Child.Accept(this);
            return null;
        }

        public object VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            return null;
        }

        public object VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            VariablePath variablePath = variableExpressionAst.VariablePath;
            if (variablePath.IsAnyLocal())
            {
                VariableAnalysisDetails details = this._variables[GetUnaliasedVariableName(variablePath)];
                if (details.LocalTupleIndex != -1)
                {
                    variableExpressionAst.TupleIndex = details.PreferenceVariable ? -2 : details.LocalTupleIndex;
                    variableExpressionAst.Automatic = details.Automatic;
                }
                else
                {
                    this._currentBlock.AddAst(variableExpressionAst);
                }
                details.AssociatedAsts.Add(variableExpressionAst);
            }
            else
            {
                variableExpressionAst.TupleIndex = -2;
            }
            return null;
        }

        public object VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            this.GenerateWhileLoop(whileStatementAst.Label, delegate {
                whileStatementAst.Condition.Accept(this);
            }, delegate {
                whileStatementAst.Body.Accept(this);
            }, null);
            return null;
        }

        

        private class AssignmentTarget : Ast
        {
            internal readonly ExpressionAst _targetAst;
            internal readonly Type _type;
            internal readonly string _variableName;

            public AssignmentTarget(ExpressionAst targetExpressionAst) : base(PositionUtilities.EmptyExtent)
            {
                this._targetAst = targetExpressionAst;
            }

            public AssignmentTarget(string variableName, Type type) : base(PositionUtilities.EmptyExtent)
            {
                this._variableName = variableName;
                this._type = type;
            }

            internal override object Accept(ICustomAstVisitor visitor)
            {
                return null;
            }

            internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
            {
                return Ast.EmptyPSTypeNameArray;
            }

            internal override AstVisitAction InternalVisit(AstVisitor visitor)
            {
                return AstVisitAction.Continue;
            }
        }

        private class Block
        {
            internal readonly List<Ast> _asts = new List<Ast>();
            internal readonly List<VariableAnalysis.Block> _predecessors = new List<VariableAnalysis.Block>();
            private readonly List<VariableAnalysis.Block> _successors = new List<VariableAnalysis.Block>();
            internal object _visitData;

            internal void AddAst(Ast ast)
            {
                this._asts.Add(ast);
            }

            internal void FlowsTo(VariableAnalysis.Block next)
            {
                if (this._successors.IndexOf(next) < 0)
                {
                    this._successors.Add(next);
                    next._predecessors.Add(this);
                }
            }

            internal static List<VariableAnalysis.Block> GenerateReverseDepthFirstOrder(VariableAnalysis.Block block)
            {
                List<VariableAnalysis.Block> visitData = new List<VariableAnalysis.Block>();
                VisitDepthFirstOrder(block, visitData);
                visitData.Reverse();
                visitData.ForEach(delegate (VariableAnalysis.Block b) {
                    b._visitData = null;
                });
                return visitData;
            }

            private static void VisitDepthFirstOrder(VariableAnalysis.Block block, List<VariableAnalysis.Block> visitData)
            {
                if (!object.ReferenceEquals(block._visitData, visitData))
                {
                    block._visitData = visitData;
                    foreach (VariableAnalysis.Block block2 in block._successors)
                    {
                        VisitDepthFirstOrder(block2, visitData);
                    }
                    visitData.Add(block);
                }
            }
        }

        private class LoopGotoTargets
        {
            internal LoopGotoTargets(string label, VariableAnalysis.Block breakTarget, VariableAnalysis.Block continueTarget)
            {
                this.Label = label;
                this.BreakTarget = breakTarget;
                this.ContinueTarget = continueTarget;
            }

            internal VariableAnalysis.Block BreakTarget { get; private set; }

            internal VariableAnalysis.Block ContinueTarget { get; private set; }

            internal string Label { get; private set; }
        }
    }
}

