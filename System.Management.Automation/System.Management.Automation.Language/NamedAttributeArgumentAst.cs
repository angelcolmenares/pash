namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class NamedAttributeArgumentAst : Ast
    {
        public NamedAttributeArgumentAst(IScriptExtent extent, string argumentName, ExpressionAst argument, bool expressionOmitted) : base(extent)
        {
            if (string.IsNullOrEmpty(argumentName))
            {
                throw PSTraceSource.NewArgumentNullException("argumentName");
            }
            if (argument == null)
            {
                throw PSTraceSource.NewArgumentNullException("argument");
            }
            this.Argument = argument;
            base.SetParent(argument);
            this.ArgumentName = argumentName;
            this.ExpressionOmitted = expressionOmitted;
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitNamedAttributeArgument(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return Ast.EmptyPSTypeNameArray;
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitNamedAttributeArgument(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Argument.InternalVisit(visitor);
                    break;
            }
            return action;
        }

        public ExpressionAst Argument { get; private set; }

        public string ArgumentName { get; private set; }

        public bool ExpressionOmitted { get; private set; }
    }
}

