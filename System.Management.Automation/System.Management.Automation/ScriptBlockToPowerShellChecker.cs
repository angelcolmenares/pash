namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;

    internal class ScriptBlockToPowerShellChecker : AstVisitor
    {
        private readonly HashSet<string> _validVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal static void ThrowError(ScriptBlockToPowerShellNotSupportedException ex, Ast ast)
        {
            InterpreterError.UpdateExceptionErrorRecordPosition(ex, ast.Extent);
            throw ex;
        }

        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            if (commandAst.InvocationOperator == TokenKind.Dot)
            {
                ThrowError(new ScriptBlockToPowerShellNotSupportedException("CantConvertWithDotSourcing", null, AutomationExceptions.CantConvertWithDotSourcing, new object[0]), commandAst);
            }
            if (commandAst.Parent.Parent.Parent != this.ScriptBeingConverted)
            {
                ThrowError(new ScriptBlockToPowerShellNotSupportedException("CantConvertWithCommandInvocations", null, AutomationExceptions.CantConvertWithCommandInvocations, new object[0]), commandAst);
            }
            if (commandAst.CommandElements[0] is ScriptBlockExpressionAst)
            {
                ThrowError(new ScriptBlockToPowerShellNotSupportedException("CantConvertWithScriptBlockInvocation", null, AutomationExceptions.CantConvertWithScriptBlockInvocation, new object[0]), commandAst);
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitFileRedirection(FileRedirectionAst redirectionAst)
        {
            ThrowError(new ScriptBlockToPowerShellNotSupportedException("CanConvertOneOutputErrorRedir", null, AutomationExceptions.CanConvertOneOutputErrorRedir, new object[0]), redirectionAst);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitMergingRedirection(MergingRedirectionAst redirectionAst)
        {
            if (redirectionAst.ToStream != RedirectionStream.Output)
            {
                ThrowError(new ScriptBlockToPowerShellNotSupportedException("CanConvertOneOutputErrorRedir", null, AutomationExceptions.CanConvertOneOutputErrorRedir, new object[0]), redirectionAst);
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitParameter(ParameterAst parameterAst)
        {
            if (parameterAst.Name.VariablePath.IsAnyLocal())
            {
                this._validVariables.Add(parameterAst.Name.VariablePath.UnqualifiedPath);
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitPipeline(PipelineAst pipelineAst)
        {
            if ((pipelineAst.PipelineElements[0] is CommandExpressionAst) && ((pipelineAst.GetPureExpression() == null) || (pipelineAst.Parent.Parent == this.ScriptBeingConverted)))
            {
                ThrowError(new ScriptBlockToPowerShellNotSupportedException("CantConvertPipelineStartsWithExpression", null, AutomationExceptions.CantConvertPipelineStartsWithExpression, new object[0]), pipelineAst);
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            ThrowError(new ScriptBlockToPowerShellNotSupportedException("CantConvertWithScriptBlocks", null, AutomationExceptions.CantConvertWithScriptBlocks, new object[0]), scriptBlockExpressionAst);
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            this.HasUsingExpr = true;
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            bool flag = false;
            if (variableExpressionAst.VariablePath.IsAnyLocal())
            {
                string unqualifiedPath = variableExpressionAst.VariablePath.UnqualifiedPath;
                if (this._validVariables.Contains(unqualifiedPath) || unqualifiedPath.Equals("args", StringComparison.OrdinalIgnoreCase))
                {
                    flag = true;
                    this.UsesParameter = true;
                }
                else
                {
                    flag = !variableExpressionAst.Splatted && variableExpressionAst.IsConstantVariable();
                }
            }
            if (!flag)
            {
                ThrowError(new ScriptBlockToPowerShellNotSupportedException("CantConvertWithUndeclaredVariables", null, AutomationExceptions.CantConvertWithUndeclaredVariables, new object[] { variableExpressionAst.VariablePath }), variableExpressionAst);
            }
            return AstVisitAction.Continue;
        }

        internal bool HasUsingExpr { get; private set; }

        internal ScriptBlockAst ScriptBeingConverted { get; set; }

        internal bool UsesParameter { get; private set; }
    }
}

