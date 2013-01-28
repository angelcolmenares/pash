namespace System.Management.Automation.Language
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class CommandAst : CommandBaseAst
    {
        public CommandAst(IScriptExtent extent, IEnumerable<CommandElementAst> commandElements, TokenKind invocationOperator, IEnumerable<RedirectionAst> redirections) : base(extent, redirections)
        {
            if ((commandElements == null) || !commandElements.Any<CommandElementAst>())
            {
                throw PSTraceSource.NewArgumentException("commandElements");
            }
            if (((invocationOperator != TokenKind.Dot) && (invocationOperator != TokenKind.Ampersand)) && (invocationOperator != TokenKind.Unknown))
            {
                throw PSTraceSource.NewArgumentException("invocationOperator");
            }
            this.CommandElements = new ReadOnlyCollection<CommandElementAst>(commandElements.ToArray<CommandElementAst>());
            base.SetParents((IEnumerable<Ast>) this.CommandElements);
            this.InvocationOperator = invocationOperator;
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitCommand(this);
        }

        public string GetCommandName()
        {
            StringConstantExpressionAst ast = this.CommandElements[0] as StringConstantExpressionAst;
            if (ast == null)
            {
                return null;
            }
            return ast.Value;
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            PseudoBindingInfo iteratorVariable0 = new PseudoParameterBinder().DoPseudoParameterBinding(this, null, null, false);
            if (iteratorVariable0.CommandInfo != null)
            {
                AstParameterArgumentPair iteratorVariable1;
                string key = "Path";
                if (!iteratorVariable0.BoundArguments.TryGetValue(key, out iteratorVariable1))
                {
                    key = "LiteralPath";
                    iteratorVariable0.BoundArguments.TryGetValue(key, out iteratorVariable1);
                }
                CommandInfo commandInfo = iteratorVariable0.CommandInfo;
                AstPair iteratorVariable4 = iteratorVariable1 as AstPair;
                if ((iteratorVariable4 != null) && (iteratorVariable4.Argument is StringConstantExpressionAst))
                {
                    string str = ((StringConstantExpressionAst) iteratorVariable4.Argument).Value;
                    try
                    {
                        commandInfo = commandInfo.CreateGetCommandCopy(new string[] { "-" + key, str });
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
                CmdletInfo iteratorVariable5 = commandInfo as CmdletInfo;
                if (iteratorVariable5 != null)
                {
                    if (iteratorVariable5.ImplementingType.FullName.Equals("Microsoft.PowerShell.Commands.NewObjectCommand", StringComparison.Ordinal))
                    {
                        AstParameterArgumentPair iteratorVariable6;
                        if (iteratorVariable0.BoundArguments.TryGetValue("TypeName", out iteratorVariable6))
                        {
                            AstPair iteratorVariable7 = iteratorVariable6 as AstPair;
                            if ((iteratorVariable7 != null) && (iteratorVariable7.Argument is StringConstantExpressionAst))
                            {
                                yield return new PSTypeName(((StringConstantExpressionAst) iteratorVariable7.Argument).Value);
                            }
                        }
                        goto Label_0579;
                    }
                    if (iteratorVariable5.ImplementingType.Equals(typeof(WhereObjectCommand)) || iteratorVariable5.ImplementingType.FullName.Equals("Microsoft.PowerShell.Commands.SortObjectCommand", StringComparison.Ordinal))
                    {
                        PipelineAst parent = this.Parent as PipelineAst;
                        if (parent != null)
                        {
                            int iteratorVariable9 = 0;
                            while (iteratorVariable9 < parent.PipelineElements.Count)
                            {
                                if (parent.PipelineElements[iteratorVariable9] == this)
                                {
                                    break;
                                }
                                iteratorVariable9++;
                            }
                            if (iteratorVariable9 > 0)
                            {
                                foreach (PSTypeName iteratorVariable10 in parent.PipelineElements[iteratorVariable9 - 1].GetInferredType(context))
                                {
                                    yield return iteratorVariable10;
                                }
                            }
                        }
                        goto Label_0579;
                    }
                    if (iteratorVariable5.ImplementingType.Equals(typeof(ForEachObjectCommand)))
                    {
                        AstParameterArgumentPair iteratorVariable11;
                        if (iteratorVariable0.BoundArguments.TryGetValue("Begin", out iteratorVariable11))
                        {
                            foreach (PSTypeName iteratorVariable12 in this.GetInferredTypeFromScriptBlockParameter(iteratorVariable11, context))
                            {
                                yield return iteratorVariable12;
                            }
                        }
                        if (iteratorVariable0.BoundArguments.TryGetValue("Process", out iteratorVariable11))
                        {
                            foreach (PSTypeName iteratorVariable13 in this.GetInferredTypeFromScriptBlockParameter(iteratorVariable11, context))
                            {
                                yield return iteratorVariable13;
                            }
                        }
                        if (iteratorVariable0.BoundArguments.TryGetValue("End", out iteratorVariable11))
                        {
                            foreach (PSTypeName iteratorVariable14 in this.GetInferredTypeFromScriptBlockParameter(iteratorVariable11, context))
                            {
                                yield return iteratorVariable14;
                            }
                        }
                    }
                }
                foreach (PSTypeName iteratorVariable15 in commandInfo.OutputType)
                {
                    yield return iteratorVariable15;
                }
            }
        Label_0579:
            yield break;
        }

        private IEnumerable<PSTypeName> GetInferredTypeFromScriptBlockParameter(AstParameterArgumentPair argument, CompletionContext context)
        {
            AstPair iteratorVariable0 = argument as AstPair;
            if ((iteratorVariable0 != null) && (iteratorVariable0.Argument is ScriptBlockExpressionAst))
            {
                ScriptBlockExpressionAst iteratorVariable1 = (ScriptBlockExpressionAst) iteratorVariable0.Argument;
                foreach (PSTypeName iteratorVariable2 in iteratorVariable1.ScriptBlock.GetInferredType(context))
                {
                    yield return iteratorVariable2;
                }
            }
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitCommand(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    foreach (CommandElementAst ast in this.CommandElements)
                    {
                        action = ast.InternalVisit(visitor);
                        if (action != AstVisitAction.Continue)
                        {
                            break;
                        }
                    }
                    break;
            }
            if (action == AstVisitAction.Continue)
            {
                foreach (RedirectionAst ast2 in base.Redirections)
                {
                    if (action == AstVisitAction.Continue)
                    {
                        action = ast2.InternalVisit(visitor);
                    }
                }
            }
            return action;
        }

        public ReadOnlyCollection<CommandElementAst> CommandElements { get; private set; }

        public TokenKind InvocationOperator { get; private set; }

        
    }
}

