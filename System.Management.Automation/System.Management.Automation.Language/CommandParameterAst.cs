namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class CommandParameterAst : CommandElementAst
    {
        public CommandParameterAst(IScriptExtent extent, string parameterName, ExpressionAst argument, IScriptExtent errorPosition) : base(extent)
        {
            if ((errorPosition == null) || string.IsNullOrEmpty(parameterName))
            {
                throw PSTraceSource.NewArgumentNullException((errorPosition == null) ? "errorPosition" : "parameterName");
            }
            this.ParameterName = parameterName;
            if (argument != null)
            {
                this.Argument = argument;
                base.SetParent(argument);
            }
            this.ErrorPosition = errorPosition;
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitCommandParameter(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return Ast.EmptyPSTypeNameArray;
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitCommandParameter(this);
            if (action == AstVisitAction.SkipChildren)
            {
                return AstVisitAction.Continue;
            }
            if ((this.Argument != null) && (action == AstVisitAction.Continue))
            {
                action = this.Argument.InternalVisit(visitor);
            }
            return action;
        }

        public ExpressionAst Argument { get; private set; }

        public IScriptExtent ErrorPosition { get; private set; }

        public string ParameterName { get; private set; }
    }
}

