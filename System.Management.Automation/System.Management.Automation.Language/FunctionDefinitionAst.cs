namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class FunctionDefinitionAst : StatementAst, IParameterMetadataProvider
    {
        public FunctionDefinitionAst(IScriptExtent extent, bool isFilter, bool isWorkflow, string name, IEnumerable<ParameterAst> parameters, ScriptBlockAst body) : base(extent)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            if (body == null)
            {
                throw PSTraceSource.NewArgumentNullException("body");
            }
            if (isFilter && isWorkflow)
            {
                throw PSTraceSource.NewArgumentException("isFilter");
            }
            this.IsFilter = isFilter;
            this.IsWorkflow = isWorkflow;
            this.Name = name;
            if ((parameters != null) && parameters.Any<ParameterAst>())
            {
                this.Parameters = new ReadOnlyCollection<ParameterAst>(parameters.ToArray<ParameterAst>());
                base.SetParents((IEnumerable<Ast>) this.Parameters);
            }
            this.Body = body;
            base.SetParent(body);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitFunctionDefinition(this);
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
            return Ast.EmptyPSTypeNameArray;
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitFunctionDefinition(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    if (this.Parameters != null)
                    {
                        foreach (ParameterAst ast in this.Parameters)
                        {
                            action = ast.InternalVisit(visitor);
                            if (action != AstVisitAction.Continue)
                            {
                                break;
                            }
                        }
                    }
                    if (action == AstVisitAction.Continue)
                    {
                        action = this.Body.InternalVisit(visitor);
                    }
                    break;
            }
            return action;
        }

        RuntimeDefinedParameterDictionary IParameterMetadataProvider.GetParameterMetadata(bool automaticPositions, ref bool usesCmdletBinding)
        {
            if (this.Parameters != null)
            {
                return Compiler.GetParameterMetaData(this.Parameters, automaticPositions, ref usesCmdletBinding);
            }
            if (this.Body.ParamBlock != null)
            {
                return Compiler.GetParameterMetaData(this.Body.ParamBlock.Parameters, automaticPositions, ref usesCmdletBinding);
            }
            return new RuntimeDefinedParameterDictionary { Data = RuntimeDefinedParameterDictionary.EmptyParameterArray };
        }

        PowerShell IParameterMetadataProvider.GetPowerShell(ExecutionContext context, Dictionary<string, object> variables, bool filterNonUsingVariables, bool? createLocalScope, params object[] args)
        {
            ExecutionContext.CheckStackDepth();
            return ScriptBlockToPowerShellConverter.Convert(this.Body, this.Parameters, context, variables, filterNonUsingVariables, createLocalScope, args);
        }

        IEnumerable<Attribute> IParameterMetadataProvider.GetScriptBlockAttributes()
        {
            return ((IParameterMetadataProvider) this.Body).GetScriptBlockAttributes();
        }

        string IParameterMetadataProvider.GetWithInputHandlingForInvokeCommand()
        {
            return ((IParameterMetadataProvider) this.Body).GetWithInputHandlingForInvokeCommand();
        }

        bool IParameterMetadataProvider.UsesCmdletBinding()
        {
            bool flag = false;
            if (this.Parameters != null)
            {
                return ParamBlockAst.UsesCmdletBinding(this.Parameters);
            }
            if (this.Body.ParamBlock != null)
            {
                flag = ((IParameterMetadataProvider) this.Body).UsesCmdletBinding();
            }
            return flag;
        }

        public ScriptBlockAst Body { get; private set; }

        public bool IsFilter { get; private set; }

        public bool IsWorkflow { get; private set; }

        public string Name { get; private set; }

        public ReadOnlyCollection<ParameterAst> Parameters { get; private set; }

        ReadOnlyCollection<ParameterAst> IParameterMetadataProvider.Parameters
        {
            get
            {
                // This item is obfuscated and can not be translated.
                ReadOnlyCollection<ParameterAst> expressionStack_9_0;
                ReadOnlyCollection<ParameterAst> parameters = this.Parameters;
                if (parameters != null)
                {
                    return parameters;
                }
                else
                {
                    expressionStack_9_0 = parameters;
                }
                expressionStack_9_0 = this.Body.ParamBlock.Parameters;
                if (this.Body.ParamBlock == null)
                {
                    return null;
                }
                return this.Body.ParamBlock.Parameters;
            }
        }
    }
}

