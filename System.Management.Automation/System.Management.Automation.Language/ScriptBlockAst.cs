namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    public class ScriptBlockAst : Ast, IParameterMetadataProvider
    {
        public ScriptBlockAst(IScriptExtent extent, ParamBlockAst paramBlock, StatementBlockAst statements, bool isFilter) : base(extent)
        {
            if (statements == null)
            {
                throw PSTraceSource.NewArgumentNullException("statements");
            }
            if (paramBlock != null)
            {
                this.ParamBlock = paramBlock;
                base.SetParent(paramBlock);
            }
            if (isFilter)
            {
                this.ProcessBlock = new NamedBlockAst(statements.Extent, TokenKind.Process, statements, true);
                base.SetParent(this.ProcessBlock);
            }
            else
            {
                this.EndBlock = new NamedBlockAst(statements.Extent, TokenKind.End, statements, true);
                base.SetParent(this.EndBlock);
            }
        }

        public ScriptBlockAst(IScriptExtent extent, ParamBlockAst paramBlock, NamedBlockAst beginBlock, NamedBlockAst processBlock, NamedBlockAst endBlock, NamedBlockAst dynamicParamBlock) : base(extent)
        {
            if (paramBlock != null)
            {
                this.ParamBlock = paramBlock;
                base.SetParent(paramBlock);
            }
            if (beginBlock != null)
            {
                this.BeginBlock = beginBlock;
                base.SetParent(beginBlock);
            }
            if (processBlock != null)
            {
                this.ProcessBlock = processBlock;
                base.SetParent(processBlock);
            }
            if (endBlock != null)
            {
                this.EndBlock = endBlock;
                base.SetParent(endBlock);
            }
            if (dynamicParamBlock != null)
            {
                this.DynamicParamBlock = dynamicParamBlock;
                base.SetParent(dynamicParamBlock);
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitScriptBlock(this);
        }

        public CommentHelpInfo GetHelpContent()
        {
            Dictionary<Ast, Token[]> scriptBlockTokenCache = new Dictionary<Ast, Token[]>();
            Tuple<List<Token>, List<string>> helpCommentTokens = HelpCommentsParser.GetHelpCommentTokens(this, scriptBlockTokenCache);
            if (helpCommentTokens != null)
            {
                return HelpCommentsParser.GetHelpContents(helpCommentTokens.Item1, helpCommentTokens.Item2);
            }
            return null;
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            if (this.BeginBlock != null)
            {
                foreach (PSTypeName iteratorVariable0 in this.BeginBlock.GetInferredType(context))
                {
                    yield return iteratorVariable0;
                }
            }
            if (this.ProcessBlock != null)
            {
                foreach (PSTypeName iteratorVariable1 in this.ProcessBlock.GetInferredType(context))
                {
                    yield return iteratorVariable1;
                }
            }
            if (this.EndBlock != null)
            {
                foreach (PSTypeName iteratorVariable2 in this.EndBlock.GetInferredType(context))
                {
                    yield return iteratorVariable2;
                }
            }
        }

        public ScriptBlock GetScriptBlock()
        {
            Parser parser = new Parser();
            SemanticChecks.CheckAst(parser, this);
            if (parser.ErrorList.Any<ParseError>())
            {
                throw new ParseException(parser.ErrorList.ToArray());
            }
            return new ScriptBlock(this, false);
        }

        internal PipelineAst GetSimplePipeline(bool allowMultiplePipelines, out string errorId, out string errorMsg)
        {
            if (((this.BeginBlock != null) || (this.ProcessBlock != null)) || (this.DynamicParamBlock != null))
            {
                errorId = "CanConvertOneClauseOnly";
                errorMsg = AutomationExceptions.CanConvertOneClauseOnly;
                return null;
            }
            if ((this.EndBlock == null) || (this.EndBlock.Statements.Count < 1))
            {
                errorId = "CantConvertEmptyPipeline";
                errorMsg = AutomationExceptions.CantConvertEmptyPipeline;
                return null;
            }
            if ((this.EndBlock.Traps != null) && this.EndBlock.Traps.Any<TrapStatementAst>())
            {
                errorId = "CantConvertScriptBlockWithTrap";
                errorMsg = AutomationExceptions.CantConvertScriptBlockWithTrap;
                return null;
            }
            if ((from ast in this.EndBlock.Statements
                where !(ast is PipelineAst)
                select ast).Any<StatementAst>())
            {
                errorId = "CanOnlyConvertOnePipeline";
                errorMsg = AutomationExceptions.CanOnlyConvertOnePipeline;
                return null;
            }
            if ((this.EndBlock.Statements.Count != 1) && !allowMultiplePipelines)
            {
                errorId = "CanOnlyConvertOnePipeline";
                errorMsg = AutomationExceptions.CanOnlyConvertOnePipeline;
                return null;
            }
            errorId = null;
            errorMsg = null;
            return (this.EndBlock.Statements[0] as PipelineAst);
        }

        internal string GetWithInputHandlingForInvokeCommandImpl(Tuple<List<VariableExpressionAst>, string> usingVariablesTuple)
        {
            string str;
            string str2;
            PipelineAst ast = this.GetSimplePipeline(false, out str, out str2);
            if (ast == null)
            {
                if (usingVariablesTuple != null)
                {
                    return this.ToStringForSerialization(usingVariablesTuple, base.Extent.StartOffset, base.Extent.EndOffset);
                }
                return this.ToStringForSerialization();
            }
            if (ast.PipelineElements[0] is CommandExpressionAst)
            {
                if (usingVariablesTuple != null)
                {
                    return this.ToStringForSerialization(usingVariablesTuple, base.Extent.StartOffset, base.Extent.EndOffset);
                }
                return this.ToStringForSerialization();
            }
            if (AstSearcher.IsUsingDollarInput(this))
            {
                if (usingVariablesTuple != null)
                {
                    return this.ToStringForSerialization(usingVariablesTuple, base.Extent.StartOffset, base.Extent.EndOffset);
                }
                return this.ToStringForSerialization();
            }
            StringBuilder builder = new StringBuilder();
            if (this.ParamBlock != null)
            {
                string str3 = (usingVariablesTuple == null) ? this.ParamBlock.ToString() : this.ToStringForSerialization(usingVariablesTuple, this.ParamBlock.Extent.StartOffset, this.ParamBlock.Extent.EndOffset);
                builder.Append(str3);
            }
            builder.Append("$input |");
            string str4 = (usingVariablesTuple == null) ? ast.ToString() : this.ToStringForSerialization(usingVariablesTuple, ast.Extent.StartOffset, ast.Extent.EndOffset);
            builder.Append(str4);
            return builder.ToString();
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitScriptBlock(this);
            if (action == AstVisitAction.SkipChildren)
            {
                return AstVisitAction.Continue;
            }
            if ((action == AstVisitAction.Continue) && (this.ParamBlock != null))
            {
                action = this.ParamBlock.InternalVisit(visitor);
            }
            if ((action == AstVisitAction.Continue) && (this.DynamicParamBlock != null))
            {
                action = this.DynamicParamBlock.InternalVisit(visitor);
            }
            if ((action == AstVisitAction.Continue) && (this.BeginBlock != null))
            {
                action = this.BeginBlock.InternalVisit(visitor);
            }
            if ((action == AstVisitAction.Continue) && (this.ProcessBlock != null))
            {
                action = this.ProcessBlock.InternalVisit(visitor);
            }
            if ((action == AstVisitAction.Continue) && (this.EndBlock != null))
            {
                action = this.EndBlock.InternalVisit(visitor);
            }
            return action;
        }

        RuntimeDefinedParameterDictionary IParameterMetadataProvider.GetParameterMetadata(bool automaticPositions, ref bool usesCmdletBinding)
        {
            if (this.ParamBlock != null)
            {
                return Compiler.GetParameterMetaData(this.ParamBlock.Parameters, automaticPositions, ref usesCmdletBinding);
            }
            return new RuntimeDefinedParameterDictionary { Data = RuntimeDefinedParameterDictionary.EmptyParameterArray };
        }

        PowerShell IParameterMetadataProvider.GetPowerShell(System.Management.Automation.ExecutionContext context, Dictionary<string, object> variables, bool filterNonUsingVariables, bool? createLocalScope, params object[] args)
        {
            System.Management.Automation.ExecutionContext.CheckStackDepth();
            return ScriptBlockToPowerShellConverter.Convert(this, null, context, variables, filterNonUsingVariables, createLocalScope, args);
        }

        IEnumerable<Attribute> IParameterMetadataProvider.GetScriptBlockAttributes()
        {
            if (this.ParamBlock != null)
            {
                foreach (AttributeAst iteratorVariable0 in this.ParamBlock.Attributes)
                {
                    yield return Compiler.GetAttribute(iteratorVariable0);
                }
            }
        }

        string IParameterMetadataProvider.GetWithInputHandlingForInvokeCommand()
        {
            return this.GetWithInputHandlingForInvokeCommandImpl(null);
        }

        bool IParameterMetadataProvider.UsesCmdletBinding()
        {
            bool flag = false;
            if (this.ParamBlock != null)
            {
                flag = (from attribute in this.ParamBlock.Attributes
                    where typeof(CmdletBindingAttribute).Equals(attribute.TypeName.GetReflectionAttributeType())
                    select attribute).Any<AttributeAst>();
                if (!flag)
                {
                    flag = ParamBlockAst.UsesCmdletBinding(this.ParamBlock.Parameters);
                }
            }
            return flag;
        }

        internal string ToStringForSerialization()
        {
            string str = this.ToString();
            if ((base.Parent is FunctionDefinitionAst) || (base.Parent is ScriptBlockExpressionAst))
            {
                return str.Substring(1, str.Length - 2);
            }
            return str;
        }

        internal string ToStringForSerialization(Tuple<List<VariableExpressionAst>, string> usingVariablesTuple, int initialStartOffset, int initialEndOffset)
        {
            List<VariableExpressionAst> collection = usingVariablesTuple.Item1;
            string str = usingVariablesTuple.Item2;
            List<Ast> list2 = new List<Ast>(collection);
            if (this.ParamBlock != null)
            {
                list2.Add(this.ParamBlock);
            }
            int startOffset = base.Extent.StartOffset;
            int startIndex = initialStartOffset - startOffset;
            int num3 = initialEndOffset - startOffset;
            string str2 = this.ToString();
            StringBuilder builder = new StringBuilder();
            foreach (Ast ast in from ast in list2
                orderby ast.Extent.StartOffset
                select ast)
            {
                int num4 = ast.Extent.StartOffset - startOffset;
                int num5 = ast.Extent.EndOffset - startOffset;
                if (num4 >= startIndex)
                {
                    if (num4 >= num3)
                    {
                        break;
                    }
                    VariableExpressionAst ast2 = ast as VariableExpressionAst;
                    if (ast2 != null)
                    {
                        string userPath = ast2.VariablePath.UserPath;
                        string str4 = ast2.Splatted ? "@" : "$";
                        string str5 = str4 + "__using_" + userPath;
                        builder.Append(str2.Substring(startIndex, num4 - startIndex));
                        builder.Append(str5);
                        startIndex = num5;
                    }
                    else
                    {
                        int num6;
                        ParamBlockAst ast3 = ast as ParamBlockAst;
                        if (ast3.Parameters.Count == 0)
                        {
                            num6 = num5 - 1;
                        }
                        else
                        {
                            ParameterAst ast4 = ast3.Parameters[0];
                            num6 = (ast4.Attributes.Count == 0) ? (ast4.Name.Extent.StartOffset - startOffset) : (ast4.Attributes[0].Extent.StartOffset - startOffset);
                            str = str + ",\n";
                        }
                        builder.Append(str2.Substring(startIndex, num6 - startIndex));
                        builder.Append(str);
                        startIndex = num6;
                    }
                }
            }
            builder.Append(str2.Substring(startIndex, num3 - startIndex));
            string str6 = builder.ToString();
            if (((base.Parent is ScriptBlockExpressionAst) && (initialStartOffset == base.Extent.StartOffset)) && (initialEndOffset == base.Extent.EndOffset))
            {
                str6 = str6.Substring(1, str6.Length - 2);
            }
            return str6;
        }

        public NamedBlockAst BeginBlock { get; private set; }

        public NamedBlockAst DynamicParamBlock { get; private set; }

        public NamedBlockAst EndBlock { get; private set; }

        public ParamBlockAst ParamBlock { get; private set; }

        public NamedBlockAst ProcessBlock { get; private set; }

        public System.Management.Automation.Language.ScriptRequirements ScriptRequirements { get; internal set; }

        ScriptBlockAst IParameterMetadataProvider.Body
        {
            get
            {
                return this;
            }
        }

        ReadOnlyCollection<ParameterAst> IParameterMetadataProvider.Parameters
        {
            get
            {
                if (this.ParamBlock == null)
                {
                    return null;
                }
                return this.ParamBlock.Parameters;
            }
        }

        
    }
}

