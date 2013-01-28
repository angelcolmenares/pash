namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal sealed class LightCompiler
    {
        private readonly int _compilationThreshold;
        private readonly List<DebugInfo> _debugInfos;
        private readonly Stack<ParameterExpression> _exceptionForRethrowStack;
        private bool _forceCompile;
        private readonly InstructionList _instructions;
        private LabelScopeInfo _labelBlock;
        private readonly LocalVariables _locals;
        private readonly LightCompiler _parent;
        private readonly HybridReferenceDictionary<LabelTarget, LabelInfo> _treeLabels;
        internal const int DefaultCompilationThreshold = 0x20;
        private static LocalDefinition[] EmptyLocals = new LocalDefinition[0];

        public LightCompiler(int compilationThreshold)
        {
            this._locals = new LocalVariables();
            this._debugInfos = new List<DebugInfo>();
            this._treeLabels = new HybridReferenceDictionary<LabelTarget, LabelInfo>();
            this._labelBlock = new LabelScopeInfo(null, LabelScopeKind.Lambda);
            this._exceptionForRethrowStack = new Stack<ParameterExpression>();
            this._instructions = new InstructionList();
            this._compilationThreshold = (compilationThreshold < 0) ? 0x20 : compilationThreshold;
        }

        private LightCompiler(LightCompiler parent) : this(parent._compilationThreshold)
        {
            this._parent = parent;
        }

        public void Compile(Expression expr)
        {
            bool flag = this.TryPushLabelBlock(expr);
            this.CompileNoLabelPush(expr);
            if (flag)
            {
                this.PopLabelBlock(this._labelBlock.Kind);
            }
        }

        internal void Compile(Expression expr, bool asVoid)
        {
            if (asVoid)
            {
                this.CompileAsVoid(expr);
            }
            else
            {
                this.Compile(expr);
            }
        }

        private void CompileAndAlsoBinaryExpression(Expression expr)
        {
            this.CompileLogicalBinaryExpression(expr, true);
        }

        private void CompileArithmetic(ExpressionType nodeType, Expression left, Expression right)
        {
            this.Compile(left);
            this.Compile(right);
            switch (nodeType)
            {
                case ExpressionType.Add:
                    this._instructions.EmitAdd(left.Type, false);
                    return;

                case ExpressionType.AddChecked:
                    this._instructions.EmitAdd(left.Type, true);
                    return;

                case ExpressionType.Divide:
                    this._instructions.EmitDiv(left.Type);
                    return;

                case ExpressionType.Multiply:
                    this._instructions.EmitMul(left.Type, false);
                    return;

                case ExpressionType.MultiplyChecked:
                    this._instructions.EmitMul(left.Type, true);
                    return;

                case ExpressionType.Subtract:
                    this._instructions.EmitSub(left.Type, false);
                    return;

                case ExpressionType.SubtractChecked:
                    this._instructions.EmitSub(left.Type, true);
                    return;
            }
            throw Assert.Unreachable;
        }

        private void CompileAssignBinaryExpression(Expression expr, bool asVoid)
        {
            BinaryExpression node = (BinaryExpression) expr;
            switch (node.Left.NodeType)
            {
                case ExpressionType.Extension:
                case ExpressionType.Parameter:
                    this.CompileVariableAssignment(node, asVoid);
                    return;

                case ExpressionType.Index:
                    this.CompileIndexAssignment(node, asVoid);
                    return;

                case ExpressionType.MemberAccess:
                    this.CompileMemberAssignment(node, asVoid);
                    return;
            }
            throw new InvalidOperationException("Invalid lvalue for assignment: " + node.Left.NodeType);
        }

        internal void CompileAsVoid(Expression expr)
        {
            bool flag = this.TryPushLabelBlock(expr);
            int currentStackDepth = this._instructions.CurrentStackDepth;
            switch (expr.NodeType)
            {
                case ExpressionType.Assign:
                    this.CompileAssignBinaryExpression(expr, true);
                    break;

                case ExpressionType.Block:
                    this.CompileBlockExpression(expr, true);
                    break;

                case ExpressionType.Default:
                case ExpressionType.Constant:
                case ExpressionType.Parameter:
                    break;

                case ExpressionType.Throw:
                    this.CompileThrowUnaryExpression(expr, true);
                    break;

                default:
                    this.CompileNoLabelPush(expr);
                    if (expr.Type != typeof(void))
                    {
                        this._instructions.EmitPop();
                    }
                    break;
            }
            if (flag)
            {
                this.PopLabelBlock(this._labelBlock.Kind);
            }
        }

        private void CompileAsVoidRemoveRethrow(Expression expr)
        {
            int currentStackDepth = this._instructions.CurrentStackDepth;
            if (expr.NodeType != ExpressionType.Throw)
            {
                BlockExpression node = (BlockExpression) expr;
                LocalDefinition[] locals = this.CompileBlockStart(node);
                this.CompileAsVoidRemoveRethrow(node.Expressions[node.Expressions.Count - 1]);
                this.CompileBlockEnd(locals);
            }
        }

        private void CompileBinaryExpression(Expression expr)
        {
            BinaryExpression expression = (BinaryExpression) expr;
            if (expression.Method != null)
            {
                this.Compile(expression.Left);
                this.Compile(expression.Right);
                this._instructions.EmitCall(expression.Method);
            }
            else
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                        this.CompileArithmetic(expression.NodeType, expression.Left, expression.Right);
                        return;

                    case ExpressionType.ArrayIndex:
                        this.Compile(expression.Left);
                        this.Compile(expression.Right);
                        this._instructions.EmitGetArrayItem(expression.Left.Type);
                        return;

                    case ExpressionType.Equal:
                        this.CompileEqual(expression.Left, expression.Right);
                        return;

                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                        this.CompileComparison(expression.NodeType, expression.Left, expression.Right);
                        return;

                    case ExpressionType.NotEqual:
                        this.CompileNotEqual(expression.Left, expression.Right);
                        return;
                }
                throw new NotImplementedException(expression.NodeType.ToString());
            }
        }

        private void CompileBlockEnd(LocalDefinition[] locals)
        {
            foreach (LocalDefinition definition in locals)
            {
                this._locals.UndefineLocal(definition, this._instructions.Count);
            }
        }

        private void CompileBlockExpression(Expression expr, bool asVoid)
        {
            BlockExpression node = (BlockExpression) expr;
            LocalDefinition[] locals = this.CompileBlockStart(node);
            Expression expression2 = node.Expressions[node.Expressions.Count - 1];
            this.Compile(expression2, asVoid);
            this.CompileBlockEnd(locals);
        }

        private LocalDefinition[] CompileBlockStart(BlockExpression node)
        {
            LocalDefinition[] emptyLocals;
            int count = this._instructions.Count;
            ReadOnlyCollection<ParameterExpression> variables = node.Variables;
            if (variables.Count != 0)
            {
                emptyLocals = new LocalDefinition[variables.Count];
                int num2 = 0;
                foreach (ParameterExpression expression in variables)
                {
                    LocalDefinition definition = this._locals.DefineLocal(expression, count);
                    emptyLocals[num2++] = definition;
                    this._instructions.EmitInitializeLocal(definition.Index, expression.Type);
                }
            }
            else
            {
                emptyLocals = EmptyLocals;
            }
            for (int i = 0; i < (node.Expressions.Count - 1); i++)
            {
                this.CompileAsVoid(node.Expressions[i]);
            }
            return emptyLocals;
        }

        private void CompileCoalesceBinaryExpression(Expression expr)
        {
            BinaryExpression expression = (BinaryExpression) expr;
            if (expression.Left.Type.IsNullableType())
            {
                throw new NotImplementedException();
            }
            if (expression.Conversion != null)
            {
                throw new NotImplementedException();
            }
            BranchLabel leftNotNull = this._instructions.MakeLabel();
            this.Compile(expression.Left);
            this._instructions.EmitCoalescingBranch(leftNotNull);
            this._instructions.EmitPop();
            this.Compile(expression.Right);
            this._instructions.MarkLabel(leftNotNull);
        }

        private void CompileComparison(ExpressionType nodeType, Expression left, Expression right)
        {
            this.Compile(left);
            this.Compile(right);
            switch (nodeType)
            {
                case ExpressionType.GreaterThan:
                    this._instructions.EmitGreaterThan(left.Type);
                    return;

                case ExpressionType.GreaterThanOrEqual:
                    this._instructions.EmitGreaterThanOrEqual(left.Type);
                    return;

                case ExpressionType.LessThan:
                    this._instructions.EmitLessThan(left.Type);
                    return;

                case ExpressionType.LessThanOrEqual:
                    this._instructions.EmitLessThanOrEqual(left.Type);
                    return;
            }
            throw Assert.Unreachable;
        }

        private void CompileConditionalExpression(Expression expr, bool asVoid)
        {
            ConditionalExpression expression = (ConditionalExpression) expr;
            this.Compile(expression.Test);
            if (expression.IfTrue == Utils.Empty())
            {
                BranchLabel elseLabel = this._instructions.MakeLabel();
                this._instructions.EmitBranchTrue(elseLabel);
                this.Compile(expression.IfFalse, asVoid);
                this._instructions.MarkLabel(elseLabel);
            }
            else
            {
                BranchLabel label2 = this._instructions.MakeLabel();
                this._instructions.EmitBranchFalse(label2);
                this.Compile(expression.IfTrue, asVoid);
                if (expression.IfFalse != Utils.Empty())
                {
                    BranchLabel label = this._instructions.MakeLabel();
                    this._instructions.EmitBranch(label, false, !asVoid);
                    this._instructions.MarkLabel(label2);
                    this.Compile(expression.IfFalse, asVoid);
                    this._instructions.MarkLabel(label);
                }
                else
                {
                    this._instructions.MarkLabel(label2);
                }
            }
        }

        private void CompileConstantExpression(Expression expr)
        {
            ConstantExpression expression = (ConstantExpression) expr;
            this._instructions.EmitLoad(expression.Value, expression.Type);
        }

        private void CompileConvertToType(Type typeFrom, Type typeTo, bool isChecked)
        {
            if (!typeTo.Equals(typeFrom))
            {
                TypeCode typeCode = Type.GetTypeCode(typeFrom);
                TypeCode code2 = Type.GetTypeCode(typeTo);
                if (TypeUtils.IsNumeric(typeCode) && TypeUtils.IsNumeric(code2))
                {
                    if (isChecked)
                    {
                        this._instructions.EmitNumericConvertChecked(typeCode, code2);
                    }
                    else
                    {
                        this._instructions.EmitNumericConvertUnchecked(typeCode, code2);
                    }
                }
            }
        }

        private void CompileConvertUnaryExpression(Expression expr)
        {
            UnaryExpression expression = (UnaryExpression) expr;
            if (expression.Method != null)
            {
                this.Compile(expression.Operand);
                if (expression.Method != ScriptingRuntimeHelpers.Int32ToObjectMethod)
                {
                    this._instructions.EmitCall(expression.Method);
                }
            }
            else if (expression.Type == typeof(void))
            {
                this.CompileAsVoid(expression.Operand);
            }
            else
            {
                this.Compile(expression.Operand);
                this.CompileConvertToType(expression.Operand.Type, expression.Type, expression.NodeType == ExpressionType.ConvertChecked);
            }
        }

        private void CompileDebugInfoExpression(Expression expr)
        {
            DebugInfoExpression expression = (DebugInfoExpression) expr;
            int count = this._instructions.Count;
            DebugInfo item = new DebugInfo {
                Index = count,
                FileName = expression.Document.FileName,
                StartLine = expression.StartLine,
                EndLine = expression.EndLine,
                IsClear = expression.IsClear
            };
            this._debugInfos.Add(item);
        }

        private void CompileDefaultExpression(Expression expr)
        {
            this.CompileDefaultExpression(expr.Type);
        }

        private void CompileDefaultExpression(Type type)
        {
            if (type != typeof(void))
            {
                if (type.IsValueType)
                {
                    object primitiveDefaultValue = ScriptingRuntimeHelpers.GetPrimitiveDefaultValue(type);
                    if (primitiveDefaultValue != null)
                    {
                        this._instructions.EmitLoad(primitiveDefaultValue);
                    }
                    else
                    {
                        this._instructions.EmitDefaultValue(type);
                    }
                }
                else
                {
                    this._instructions.EmitLoad(null);
                }
            }
        }

        private void CompileDynamicExpression(Expression expr)
        {
            DynamicExpression expression = (DynamicExpression) expr;
            foreach (Expression expression2 in expression.Arguments)
            {
                this.Compile(expression2);
            }
            this._instructions.EmitDynamic(expression.DelegateType, expression.Binder);
        }

        private void CompileEqual(Expression left, Expression right)
        {
            this.Compile(left);
            this.Compile(right);
            this._instructions.EmitEqual(left.Type);
        }

        private void CompileExtensionExpression(Expression expr)
        {
            IInstructionProvider provider = expr as IInstructionProvider;
            if (provider != null)
            {
                provider.AddInstructions(this);
            }
            else
            {
                if (!expr.CanReduce)
                {
                    throw new NotImplementedException();
                }
                this.Compile(expr.Reduce());
            }
        }

        public void CompileGetBoxedVariable(ParameterExpression variable)
        {
            LocalVariable variable2 = this.ResolveLocal(variable);
            if (variable2.InClosure)
            {
                this._instructions.EmitLoadLocalFromClosureBoxed(variable2.Index);
            }
            else
            {
                this._instructions.EmitLoadLocal(variable2.Index);
            }
        }

        public void CompileGetVariable(ParameterExpression variable)
        {
            LocalVariable variable2 = this.ResolveLocal(variable);
            if (variable2.InClosure)
            {
                this._instructions.EmitLoadLocalFromClosure(variable2.Index);
            }
            else if (variable2.IsBoxed)
            {
                this._instructions.EmitLoadLocalBoxed(variable2.Index);
            }
            else
            {
                this._instructions.EmitLoadLocal(variable2.Index);
            }
        }

        private void CompileGotoExpression(Expression expr)
        {
            GotoExpression expression = (GotoExpression) expr;
            LabelInfo info = this.ReferenceLabel(expression.Target);
            if (expression.Value != null)
            {
                this.Compile(expression.Value);
            }
            this._instructions.EmitGoto(info.GetLabel(this), expression.Type != typeof(void), (expression.Value != null) && (expression.Value.Type != typeof(void)));
        }

        private void CompileIndexAssignment(BinaryExpression node, bool asVoid)
        {
            IndexExpression left = (IndexExpression) node.Left;
            if (!asVoid)
            {
                throw new NotImplementedException();
            }
            if (left.Object != null)
            {
                this.Compile(left.Object);
            }
            foreach (Expression expression2 in left.Arguments)
            {
                this.Compile(expression2);
            }
            this.Compile(node.Right);
            if (left.Indexer != null)
            {
                this._instructions.EmitCall(left.Indexer.GetSetMethod(true));
            }
            else if (left.Arguments.Count != 1)
            {
                this._instructions.EmitCall(left.Object.Type.GetMethod("Set", BindingFlags.Public | BindingFlags.Instance));
            }
            else
            {
                this._instructions.EmitSetArrayItem(left.Object.Type);
            }
        }

        private void CompileIndexExpression(Expression expr)
        {
            IndexExpression expression = (IndexExpression) expr;
            if (expression.Object != null)
            {
                this.Compile(expression.Object);
            }
            foreach (Expression expression2 in expression.Arguments)
            {
                this.Compile(expression2);
            }
            if (expression.Indexer != null)
            {
                this._instructions.EmitCall(expression.Indexer.GetGetMethod(true));
            }
            else if (expression.Arguments.Count != 1)
            {
                this._instructions.EmitCall(expression.Object.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance));
            }
            else
            {
                this._instructions.EmitGetArrayItem(expression.Object.Type);
            }
        }

        private void CompileInvocationExpression(Expression expr)
        {
            InvocationExpression expression = (InvocationExpression) expr;
            if (typeof(LambdaExpression).IsAssignableFrom(expression.Expression.Type))
            {
                throw new NotImplementedException();
            }
            this.CompileMethodCallExpression(Expression.Call(expression.Expression, expression.Expression.Type.GetMethod("Invoke"), expression.Arguments));
        }

        private void CompileLabelExpression(Expression expr)
        {
            LabelExpression expression = (LabelExpression) expr;
            LabelInfo info = null;
            if (this._labelBlock.Kind == LabelScopeKind.Block)
            {
                this._labelBlock.TryGetLabelInfo(expression.Target, out info);
                if ((info == null) && (this._labelBlock.Parent.Kind == LabelScopeKind.Switch))
                {
                    this._labelBlock.Parent.TryGetLabelInfo(expression.Target, out info);
                }
            }
            if (info == null)
            {
                info = this.DefineLabel(expression.Target);
            }
            if (expression.DefaultValue != null)
            {
                if (expression.Target.Type == typeof(void))
                {
                    this.CompileAsVoid(expression.DefaultValue);
                }
                else
                {
                    this.Compile(expression.DefaultValue);
                }
            }
            this._instructions.MarkLabel(info.GetLabel(this));
        }

        private void CompileLambdaExpression(Expression expr)
        {
            LambdaExpression node = (LambdaExpression) expr;
            LightCompiler compiler = new LightCompiler(this);
            LightDelegateCreator creator = compiler.CompileTop(node);
            if (compiler._locals.ClosureVariables != null)
            {
                foreach (ParameterExpression expression2 in compiler._locals.ClosureVariables.Keys)
                {
                    this.CompileGetBoxedVariable(expression2);
                }
            }
            this._instructions.EmitCreateDelegate(creator);
        }

        private void CompileListInitExpression(Expression expr)
        {
            throw new NotImplementedException();
        }

        private void CompileLogicalBinaryExpression(Expression expr, bool andAlso)
        {
            BinaryExpression expression = (BinaryExpression) expr;
            if (expression.Method != null)
            {
                throw new NotImplementedException();
            }
            if (!(expression.Left.Type == typeof(bool)))
            {
                throw new NotImplementedException();
            }
            BranchLabel elseLabel = this._instructions.MakeLabel();
            BranchLabel label = this._instructions.MakeLabel();
            this.Compile(expression.Left);
            if (andAlso)
            {
                this._instructions.EmitBranchFalse(elseLabel);
            }
            else
            {
                this._instructions.EmitBranchTrue(elseLabel);
            }
            this.Compile(expression.Right);
            this._instructions.EmitBranch(label, false, true);
            this._instructions.MarkLabel(elseLabel);
            this._instructions.EmitLoad(!andAlso);
            this._instructions.MarkLabel(label);
        }

        private void CompileLoopExpression(Expression expr)
        {
        }

        private void CompileMemberAssignment(BinaryExpression node, bool asVoid)
        {
            MemberExpression left = (MemberExpression) node.Left;
            PropertyInfo member = left.Member as PropertyInfo;
            if (member != null)
            {
                MethodInfo setMethod = member.GetSetMethod(true);
                this.Compile(left.Expression);
                this.Compile(node.Right);
                int count = this._instructions.Count;
                if (!asVoid)
                {
                    LocalDefinition definition = this._locals.DefineLocal(Expression.Parameter(node.Right.Type), count);
                    this._instructions.EmitAssignLocal(definition.Index);
                    this._instructions.EmitCall(setMethod);
                    this._instructions.EmitLoadLocal(definition.Index);
                    this._locals.UndefineLocal(definition, this._instructions.Count);
                }
                else
                {
                    this._instructions.EmitCall(setMethod);
                }
            }
            else
            {
                FieldInfo field = left.Member as FieldInfo;
                if (field == null)
                {
                    throw new NotImplementedException();
                }
                if (left.Expression != null)
                {
                    this.Compile(left.Expression);
                }
                this.Compile(node.Right);
                int start = this._instructions.Count;
                if (!asVoid)
                {
                    LocalDefinition definition2 = this._locals.DefineLocal(Expression.Parameter(node.Right.Type), start);
                    this._instructions.EmitAssignLocal(definition2.Index);
                    this._instructions.EmitStoreField(field);
                    this._instructions.EmitLoadLocal(definition2.Index);
                    this._locals.UndefineLocal(definition2, this._instructions.Count);
                }
                else
                {
                    this._instructions.EmitStoreField(field);
                }
            }
        }

        private void CompileMemberExpression(Expression expr)
        {
            MemberExpression expression = (MemberExpression) expr;
            MemberInfo member = expression.Member;
            FieldInfo field = member as FieldInfo;
            if (field != null)
            {
                if (field.IsLiteral)
                {
                    this._instructions.EmitLoad(field.GetRawConstantValue(), field.FieldType);
                }
                else if (field.IsStatic)
                {
                    if (field.IsInitOnly)
                    {
                        this._instructions.EmitLoad(field.GetValue(null), field.FieldType);
                    }
                    else
                    {
                        this._instructions.EmitLoadField(field);
                    }
                }
                else
                {
                    this.Compile(expression.Expression);
                    this._instructions.EmitLoadField(field);
                }
            }
            else
            {
                PropertyInfo info3 = member as PropertyInfo;
                if (info3 == null)
                {
                    throw new NotImplementedException();
                }
                MethodInfo getMethod = info3.GetGetMethod(true);
                if (expression.Expression != null)
                {
                    this.Compile(expression.Expression);
                }
                this._instructions.EmitCall(getMethod);
            }
        }

        private void CompileMemberInitExpression(Expression expr)
        {
            throw new NotImplementedException();
        }

        private void CompileMethodCallExpression(Expression expr)
        {
            MethodCallExpression expression = (MethodCallExpression) expr;
            ParameterInfo[] parameters = expression.Method.GetParameters();
            if (!parameters.TrueForAll<ParameterInfo>(p => !p.ParameterType.IsByRef) || ((!expression.Method.IsStatic && expression.Method.DeclaringType.IsValueType) && !expression.Method.DeclaringType.IsPrimitive))
            {
                this._forceCompile = true;
            }
            if (!expression.Method.IsStatic)
            {
                this.Compile(expression.Object);
            }
            foreach (Expression expression2 in expression.Arguments)
            {
                this.Compile(expression2);
            }
            this._instructions.EmitCall(expression.Method, parameters);
        }

        private void CompileNewArrayExpression(Expression expr)
        {
            NewArrayExpression expression = (NewArrayExpression) expr;
            foreach (Expression expression2 in expression.Expressions)
            {
                this.Compile(expression2);
            }
            Type elementType = expression.Type.GetElementType();
            int count = expression.Expressions.Count;
            if (expression.NodeType == ExpressionType.NewArrayInit)
            {
                this._instructions.EmitNewArrayInit(elementType, count);
            }
            else
            {
                if (expression.NodeType != ExpressionType.NewArrayBounds)
                {
                    throw new NotImplementedException();
                }
                if (count == 1)
                {
                    this._instructions.EmitNewArray(elementType);
                }
                else
                {
                    this._instructions.EmitNewArrayBounds(elementType, count);
                }
            }
        }

        private void CompileNewExpression(Expression expr)
        {
            NewExpression expression = (NewExpression) expr;
            if ((expression.Constructor != null) && !expression.Constructor.GetParameters().TrueForAll<ParameterInfo>(p => !p.ParameterType.IsByRef))
            {
                this._forceCompile = true;
            }
            if (expression.Constructor != null)
            {
                foreach (Expression expression2 in expression.Arguments)
                {
                    this.Compile(expression2);
                }
                this._instructions.EmitNew(expression.Constructor);
            }
            else
            {
                this._instructions.EmitDefaultValue(expression.Type);
            }
        }

        private void CompileNoLabelPush(Expression expr)
        {
            int currentStackDepth = this._instructions.CurrentStackDepth;
            switch (expr.NodeType)
            {
                case ExpressionType.Add:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.AddChecked:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.And:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.AndAlso:
                    this.CompileAndAlsoBinaryExpression(expr);
                    return;

                case ExpressionType.ArrayLength:
                    this.CompileUnaryExpression(expr);
                    return;

                case ExpressionType.ArrayIndex:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.Call:
                    this.CompileMethodCallExpression(expr);
                    return;

                case ExpressionType.Coalesce:
                    this.CompileCoalesceBinaryExpression(expr);
                    return;

                case ExpressionType.Conditional:
                    this.CompileConditionalExpression(expr, expr.Type == typeof(void));
                    return;

                case ExpressionType.Constant:
                    this.CompileConstantExpression(expr);
                    return;

                case ExpressionType.Convert:
                    this.CompileConvertUnaryExpression(expr);
                    return;

                case ExpressionType.ConvertChecked:
                    this.CompileConvertUnaryExpression(expr);
                    return;

                case ExpressionType.Divide:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.Equal:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.ExclusiveOr:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.GreaterThan:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.GreaterThanOrEqual:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.Invoke:
                    this.CompileInvocationExpression(expr);
                    return;

                case ExpressionType.Lambda:
                    this.CompileLambdaExpression(expr);
                    return;

                case ExpressionType.LeftShift:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.LessThan:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.LessThanOrEqual:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.ListInit:
                    this.CompileListInitExpression(expr);
                    return;

                case ExpressionType.MemberAccess:
                    this.CompileMemberExpression(expr);
                    return;

                case ExpressionType.MemberInit:
                    this.CompileMemberInitExpression(expr);
                    return;

                case ExpressionType.Modulo:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.Multiply:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.MultiplyChecked:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.Negate:
                    this.CompileUnaryExpression(expr);
                    return;

                case ExpressionType.UnaryPlus:
                    this.CompileUnaryExpression(expr);
                    return;

                case ExpressionType.NegateChecked:
                    this.CompileUnaryExpression(expr);
                    return;

                case ExpressionType.New:
                    this.CompileNewExpression(expr);
                    return;

                case ExpressionType.NewArrayInit:
                    this.CompileNewArrayExpression(expr);
                    return;

                case ExpressionType.NewArrayBounds:
                    this.CompileNewArrayExpression(expr);
                    return;

                case ExpressionType.Not:
                    this.CompileUnaryExpression(expr);
                    return;

                case ExpressionType.NotEqual:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.Or:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.OrElse:
                    this.CompileOrElseBinaryExpression(expr);
                    return;

                case ExpressionType.Parameter:
                    this.CompileParameterExpression(expr);
                    return;

                case ExpressionType.Power:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.Quote:
                    this.CompileQuoteUnaryExpression(expr);
                    return;

                case ExpressionType.RightShift:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.Subtract:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.SubtractChecked:
                    this.CompileBinaryExpression(expr);
                    return;

                case ExpressionType.TypeAs:
                    this.CompileUnaryExpression(expr);
                    return;

                case ExpressionType.TypeIs:
                    this.CompileTypeIsExpression(expr);
                    return;

                case ExpressionType.Assign:
                    this.CompileAssignBinaryExpression(expr, expr.Type == typeof(void));
                    return;

                case ExpressionType.Block:
                    this.CompileBlockExpression(expr, expr.Type == typeof(void));
                    return;

                case ExpressionType.DebugInfo:
                    this.CompileDebugInfoExpression(expr);
                    return;

                case ExpressionType.Decrement:
                    this.CompileUnaryExpression(expr);
                    return;

                case ExpressionType.Dynamic:
                    this.CompileDynamicExpression(expr);
                    return;

                case ExpressionType.Default:
                    this.CompileDefaultExpression(expr);
                    return;

                case ExpressionType.Extension:
                    this.CompileExtensionExpression(expr);
                    return;

                case ExpressionType.Goto:
                    this.CompileGotoExpression(expr);
                    return;

                case ExpressionType.Increment:
                    this.CompileUnaryExpression(expr);
                    return;

                case ExpressionType.Index:
                    this.CompileIndexExpression(expr);
                    return;

                case ExpressionType.Label:
                    this.CompileLabelExpression(expr);
                    return;

                case ExpressionType.RuntimeVariables:
                    this.CompileRuntimeVariablesExpression(expr);
                    return;

                case ExpressionType.Loop:
                    this.CompileLoopExpression(expr);
                    return;

                case ExpressionType.Switch:
                    this.CompileSwitchExpression(expr);
                    return;

                case ExpressionType.Throw:
                    this.CompileThrowUnaryExpression(expr, expr.Type == typeof(void));
                    return;

                case ExpressionType.Try:
                    this.CompileTryExpression(expr);
                    return;

                case ExpressionType.Unbox:
                    this.CompileUnboxUnaryExpression(expr);
                    return;

                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.PostDecrementAssign:
                    this.CompileReducibleExpression(expr);
                    return;

                case ExpressionType.TypeEqual:
                    this.CompileTypeEqualExpression(expr);
                    return;

                case ExpressionType.OnesComplement:
                    this.CompileUnaryExpression(expr);
                    return;

                case ExpressionType.IsTrue:
                    this.CompileUnaryExpression(expr);
                    return;

                case ExpressionType.IsFalse:
                    this.CompileUnaryExpression(expr);
                    return;
            }
            throw Assert.Unreachable;
        }

        private void CompileNotEqual(Expression left, Expression right)
        {
            this.Compile(left);
            this.Compile(right);
            this._instructions.EmitNotEqual(left.Type);
        }

        private void CompileNotExpression(UnaryExpression node)
        {
            if (node.Operand.Type != typeof(bool))
            {
                throw new NotImplementedException();
            }
            this.Compile(node.Operand);
            this._instructions.EmitNot();
        }

        private void CompileOrElseBinaryExpression(Expression expr)
        {
            this.CompileLogicalBinaryExpression(expr, false);
        }

        public void CompileParameterExpression(Expression expr)
        {
            ParameterExpression variable = (ParameterExpression) expr;
            this.CompileGetVariable(variable);
        }

        private void CompileQuoteUnaryExpression(Expression expr)
        {
            throw new NotImplementedException();
        }

        private void CompileReducibleExpression(Expression expr)
        {
            throw new NotImplementedException();
        }

        private void CompileRuntimeVariablesExpression(Expression expr)
        {
            RuntimeVariablesExpression expression = (RuntimeVariablesExpression) expr;
            foreach (ParameterExpression expression2 in expression.Variables)
            {
                this.EnsureAvailableForClosure(expression2);
                this.CompileGetBoxedVariable(expression2);
            }
            this._instructions.EmitNewRuntimeVariables(expression.Variables.Count);
        }

        public void CompileSetVariable(ParameterExpression variable, bool isVoid)
        {
            LocalVariable variable2 = this.ResolveLocal(variable);
            if (variable2.InClosure)
            {
                if (isVoid)
                {
                    this._instructions.EmitStoreLocalToClosure(variable2.Index);
                }
                else
                {
                    this._instructions.EmitAssignLocalToClosure(variable2.Index);
                }
            }
            else if (variable2.IsBoxed)
            {
                if (isVoid)
                {
                    this._instructions.EmitStoreLocalBoxed(variable2.Index);
                }
                else
                {
                    this._instructions.EmitAssignLocalBoxed(variable2.Index);
                }
            }
            else if (isVoid)
            {
                this._instructions.EmitStoreLocal(variable2.Index);
            }
            else
            {
                this._instructions.EmitAssignLocal(variable2.Index);
            }
        }

        private void CompileSwitchExpression(Expression expr)
        {
            SwitchExpression expression = (SwitchExpression) expr;
            if ((expression.SwitchValue.Type != typeof(int)) || (expression.Comparison != null))
            {
                throw new NotImplementedException();
            }
            if (!expression.Cases.All<SwitchCase>(c => c.TestValues.All<Expression>(t => (t is ConstantExpression))))
            {
                throw new NotImplementedException();
            }
            LabelInfo info = this.DefineLabel(null);
            bool hasValue = expression.Type != typeof(void);
            this.Compile(expression.SwitchValue);
            Dictionary<int, int> cases = new Dictionary<int, int>();
            int count = this._instructions.Count;
            this._instructions.EmitSwitch(cases);
            if (expression.DefaultBody != null)
            {
                this.Compile(expression.DefaultBody);
            }
            this._instructions.EmitBranch(info.GetLabel(this), false, hasValue);
            for (int i = 0; i < expression.Cases.Count; i++)
            {
                SwitchCase @case = expression.Cases[i];
                int num3 = this._instructions.Count - count;
                foreach (ConstantExpression expression2 in @case.TestValues)
                {
                    cases[(int) expression2.Value] = num3;
                }
                this.Compile(@case.Body);
                if (i < (expression.Cases.Count - 1))
                {
                    this._instructions.EmitBranch(info.GetLabel(this), false, hasValue);
                }
            }
            this._instructions.MarkLabel(info.GetLabel(this));
        }

        private void CompileThrowUnaryExpression(Expression expr, bool asVoid)
        {
            UnaryExpression expression = (UnaryExpression) expr;
            if (expression.Operand == null)
            {
                this.CompileParameterExpression(this._exceptionForRethrowStack.Peek());
                if (asVoid)
                {
                    this._instructions.EmitRethrowVoid();
                }
                else
                {
                    this._instructions.EmitRethrow();
                }
            }
            else
            {
                this.Compile(expression.Operand);
                if (asVoid)
                {
                    this._instructions.EmitThrowVoid();
                }
                else
                {
                    this._instructions.EmitThrow();
                }
            }
        }

        public LightDelegateCreator CompileTop(LambdaExpression node)
        {
            foreach (ParameterExpression expression in node.Parameters)
            {
                LocalDefinition definition = this._locals.DefineLocal(expression, 0);
                this._instructions.EmitInitializeParameter(definition.Index);
            }
            this.Compile(node.Body);
            if ((node.Body.Type != typeof(void)) && (node.ReturnType == typeof(void)))
            {
                this._instructions.EmitPop();
            }
            return new LightDelegateCreator(this.MakeInterpreter(node.Name), node);
        }

        private void CompileTryExpression(Expression expr)
        {
            TryExpression expression = (TryExpression) expr;
            BranchLabel label = this._instructions.MakeLabel();
            BranchLabel label2 = this._instructions.MakeLabel();
            int count = this._instructions.Count;
            BranchLabel finallyStartLabel = null;
            if (expression.Finally != null)
            {
                finallyStartLabel = this._instructions.MakeLabel();
                this._instructions.EmitEnterTryFinally(finallyStartLabel);
            }
            else
            {
                this._instructions.EmitEnterTryCatch();
            }
            List<ExceptionHandler> list = null;
            EnterTryCatchFinallyInstruction instruction = this._instructions.GetInstruction(count) as EnterTryCatchFinallyInstruction;
            this.PushLabelBlock(LabelScopeKind.Try);
            this.Compile(expression.Body);
            bool hasResult = expression.Body.Type != typeof(void);
            int end = this._instructions.Count;
            this._instructions.MarkLabel(label2);
            this._instructions.EmitGoto(label, hasResult, hasResult);
            if (expression.Handlers.Count > 0)
            {
                list = new List<ExceptionHandler>();
                if ((expression.Finally == null) && (expression.Handlers.Count == 1))
                {
                    CatchBlock block = expression.Handlers[0];
                    if (((block.Filter == null) && (block.Test == typeof(Exception))) && ((block.Variable == null) && this.EndsWithRethrow(block.Body)))
                    {
                        if (hasResult)
                        {
                            this._instructions.EmitEnterExceptionHandlerNonVoid();
                        }
                        else
                        {
                            this._instructions.EmitEnterExceptionHandlerVoid();
                        }
                        int labelIndex = this._instructions.MarkRuntimeLabel();
                        int handlerStartIndex = this._instructions.Count;
                        this.CompileAsVoidRemoveRethrow(block.Body);
                        this._instructions.EmitLeaveFault(hasResult);
                        this._instructions.MarkLabel(label);
                        list.Add(new ExceptionHandler(count, end, labelIndex, handlerStartIndex, this._instructions.Count, null));
                        instruction.SetTryHandler(new TryCatchFinallyHandler(count, end, label2.TargetIndex, list.ToArray()));
                        this.PopLabelBlock(LabelScopeKind.Try);
                        return;
                    }
                }
                foreach (CatchBlock block2 in expression.Handlers)
                {
                    this.PushLabelBlock(LabelScopeKind.Catch);
                    if (block2.Filter != null)
                    {
                        throw new NotImplementedException();
                    }
                    ParameterExpression variable = block2.Variable ?? Expression.Parameter(block2.Test);
                    LocalDefinition definition = this._locals.DefineLocal(variable, this._instructions.Count);
                    this._exceptionForRethrowStack.Push(variable);
                    if (hasResult)
                    {
                        this._instructions.EmitEnterExceptionHandlerNonVoid();
                    }
                    else
                    {
                        this._instructions.EmitEnterExceptionHandlerVoid();
                    }
                    int num5 = this._instructions.MarkRuntimeLabel();
                    int num6 = this._instructions.Count;
                    this.CompileSetVariable(variable, true);
                    this.Compile(block2.Body);
                    this._exceptionForRethrowStack.Pop();
                    this._instructions.EmitLeaveExceptionHandler(hasResult, label2);
                    list.Add(new ExceptionHandler(count, end, num5, num6, this._instructions.Count, block2.Test));
                    this.PopLabelBlock(LabelScopeKind.Catch);
                    this._locals.UndefineLocal(definition, this._instructions.Count);
                }
                if (expression.Fault != null)
                {
                    throw new NotImplementedException();
                }
            }
            if (expression.Finally != null)
            {
                this.PushLabelBlock(LabelScopeKind.Finally);
                this._instructions.MarkLabel(finallyStartLabel);
                this._instructions.EmitEnterFinally(finallyStartLabel);
                this.CompileAsVoid(expression.Finally);
                this._instructions.EmitLeaveFinally();
                instruction.SetTryHandler(new TryCatchFinallyHandler(count, end, label2.TargetIndex, finallyStartLabel.TargetIndex, this._instructions.Count, (list != null) ? list.ToArray() : null));
                this.PopLabelBlock(LabelScopeKind.Finally);
            }
            else
            {
                instruction.SetTryHandler(new TryCatchFinallyHandler(count, end, label2.TargetIndex, list.ToArray()));
            }
            this._instructions.MarkLabel(label);
            this.PopLabelBlock(LabelScopeKind.Try);
        }

        private void CompileTypeAsExpression(UnaryExpression node)
        {
            this.Compile(node.Operand);
            this._instructions.EmitTypeAs(node.Type);
        }

        private void CompileTypeEqualExpression(Expression expr)
        {
            TypeBinaryExpression expression = (TypeBinaryExpression) expr;
            this.Compile(expression.Expression);
            this._instructions.EmitLoad(expression.TypeOperand);
            this._instructions.EmitTypeEquals();
        }

        private void CompileTypeIsExpression(Expression expr)
        {
            TypeBinaryExpression expression = (TypeBinaryExpression) expr;
            this.Compile(expression.Expression);
            if (expression.TypeOperand.IsSealed)
            {
                this._instructions.EmitLoad(expression.TypeOperand);
                this._instructions.EmitTypeEquals();
            }
            else
            {
                this._instructions.EmitTypeIs(expression.TypeOperand);
            }
        }

        private void CompileUnaryExpression(Expression expr)
        {
            UnaryExpression node = (UnaryExpression) expr;
            if (node.Method != null)
            {
                this.Compile(node.Operand);
                this._instructions.EmitCall(node.Method);
            }
            else
            {
                ExpressionType nodeType = node.NodeType;
                if (nodeType != ExpressionType.Not)
                {
                    if (nodeType != ExpressionType.TypeAs)
                    {
                        throw new NotImplementedException(node.NodeType.ToString());
                    }
                }
                else
                {
                    this.CompileNotExpression(node);
                    return;
                }
                this.CompileTypeAsExpression(node);
            }
        }

        private void CompileUnboxUnaryExpression(Expression expr)
        {
            UnaryExpression expression = (UnaryExpression) expr;
            this.Compile(expression.Operand);
        }

        private void CompileVariableAssignment(BinaryExpression node, bool asVoid)
        {
            this.Compile(node.Right);
            ParameterExpression left = (ParameterExpression) node.Left;
            this.CompileSetVariable(left, asVoid);
        }

        private void DefineBlockLabels(Expression node)
        {
            BlockExpression expression = node as BlockExpression;
            if (expression != null)
            {
                int num = 0;
                int count = expression.Expressions.Count;
                while (num < count)
                {
                    Expression expression2 = expression.Expressions[num];
                    LabelExpression expression3 = expression2 as LabelExpression;
                    if (expression3 != null)
                    {
                        this.DefineLabel(expression3.Target);
                    }
                    num++;
                }
            }
        }

        internal LabelInfo DefineLabel(LabelTarget node)
        {
            if (node == null)
            {
                return new LabelInfo(null);
            }
            LabelInfo info = this.EnsureLabel(node);
            info.Define(this._labelBlock);
            return info;
        }

        private bool EndsWithRethrow(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Throw)
            {
                UnaryExpression expression = (UnaryExpression) expr;
                return (expression.Operand == null);
            }
            BlockExpression expression2 = expr as BlockExpression;
            return ((expression2 != null) && this.EndsWithRethrow(expression2.Expressions[expression2.Expressions.Count - 1]));
        }

        private LocalVariable EnsureAvailableForClosure(ParameterExpression expr)
        {
            LocalVariable variable;
            if (this._locals.TryGetLocalOrClosure(expr, out variable))
            {
                if (!variable.InClosure && !variable.IsBoxed)
                {
                    this._locals.Box(expr, this._instructions);
                }
                return variable;
            }
            if (this._parent == null)
            {
                throw new InvalidOperationException("unbound variable: " + expr);
            }
            this._parent.EnsureAvailableForClosure(expr);
            return this._locals.AddClosureVariable(expr);
        }

        private LabelInfo EnsureLabel(LabelTarget node)
        {
            LabelInfo info;
            if (!this._treeLabels.TryGetValue(node, out info))
            {
                this._treeLabels[node] = info = new LabelInfo(node);
            }
            return info;
        }

        public BranchLabel GetBranchLabel(LabelTarget target)
        {
            return this.ReferenceLabel(target).GetLabel(this);
        }

        private HybridReferenceDictionary<LabelTarget, BranchLabel> GetBranchMapping()
        {
            HybridReferenceDictionary<LabelTarget, BranchLabel> dictionary = new HybridReferenceDictionary<LabelTarget, BranchLabel>(this._treeLabels.Count);
            foreach (KeyValuePair<LabelTarget, LabelInfo> pair in this._treeLabels)
            {
                dictionary[pair.Key] = pair.Value.GetLabel(this);
            }
            return dictionary;
        }

        private System.Management.Automation.Interpreter.Interpreter MakeInterpreter(string lambdaName)
        {
            if (this._forceCompile)
            {
                return null;
            }
            return new System.Management.Automation.Interpreter.Interpreter(lambdaName, this._locals, this.GetBranchMapping(), this._instructions.ToArray(), this._debugInfos.ToArray(), this._compilationThreshold);
        }

        public void PopLabelBlock(LabelScopeKind kind)
        {
            this._labelBlock = this._labelBlock.Parent;
        }

        public void PushLabelBlock(LabelScopeKind type)
        {
            this._labelBlock = new LabelScopeInfo(this._labelBlock, type);
        }

        private LabelInfo ReferenceLabel(LabelTarget node)
        {
            LabelInfo info = this.EnsureLabel(node);
            info.Reference(this._labelBlock);
            return info;
        }

        private LocalVariable ResolveLocal(ParameterExpression variable)
        {
            LocalVariable variable2;
            if (!this._locals.TryGetLocalOrClosure(variable, out variable2))
            {
                variable2 = this.EnsureAvailableForClosure(variable);
            }
            return variable2;
        }

        private bool TryPushLabelBlock(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Conditional:
                case ExpressionType.Goto:
                case ExpressionType.Loop:
                    this.PushLabelBlock(LabelScopeKind.Statement);
                    return true;

                case ExpressionType.Convert:
                    if (node.Type != typeof(void))
                    {
                        break;
                    }
                    this.PushLabelBlock(LabelScopeKind.Statement);
                    return true;

                case ExpressionType.Block:
                    this.PushLabelBlock(LabelScopeKind.Block);
                    if (this._labelBlock.Parent.Kind != LabelScopeKind.Switch)
                    {
                        this.DefineBlockLabels(node);
                    }
                    return true;

                case ExpressionType.Label:
                {
                    if (this._labelBlock.Kind != LabelScopeKind.Block)
                    {
                        goto Label_00B3;
                    }
                    LabelTarget target = ((LabelExpression) node).Target;
                    if (!this._labelBlock.ContainsTarget(target))
                    {
                        if ((this._labelBlock.Parent.Kind == LabelScopeKind.Switch) && this._labelBlock.Parent.ContainsTarget(target))
                        {
                            return false;
                        }
                        goto Label_00B3;
                    }
                    return false;
                }
                case ExpressionType.Switch:
                {
                    this.PushLabelBlock(LabelScopeKind.Switch);
                    SwitchExpression expression = (SwitchExpression) node;
                    foreach (SwitchCase @case in expression.Cases)
                    {
                        this.DefineBlockLabels(@case.Body);
                    }
                    this.DefineBlockLabels(expression.DefaultBody);
                    return true;
                }
            }
            if (this._labelBlock.Kind != LabelScopeKind.Expression)
            {
                this.PushLabelBlock(LabelScopeKind.Expression);
                return true;
            }
            return false;
        Label_00B3:
            this.PushLabelBlock(LabelScopeKind.Statement);
            return true;
        }

        internal static Expression Unbox(Expression strongBoxExpression)
        {
            return Expression.Field(strongBoxExpression, typeof(StrongBox<object>).GetField("Value"));
        }

        public InstructionList Instructions
        {
            get
            {
                return this._instructions;
            }
        }

        public LocalVariables Locals
        {
            get
            {
                return this._locals;
            }
        }
    }
}

