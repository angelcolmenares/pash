namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class ConvertExpressionAst : AttributedExpressionAst, ISupportsAssignment
    {
        public ConvertExpressionAst(IScriptExtent extent, TypeConstraintAst typeConstraint, ExpressionAst child) : base(extent, typeConstraint, child)
        {
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitConvertExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            System.Type reflectionType = this.Type.TypeName.GetReflectionType();
            if (reflectionType == null)
            {
                yield return new PSTypeName(this.Type.TypeName.FullName);
            }
            else
            {
                yield return new PSTypeName(reflectionType);
            }
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitConvertExpression(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Type.InternalVisit(visitor);
                    break;
            }
            if (action == AstVisitAction.Continue)
            {
                action = base.Child.InternalVisit(visitor);
            }
            return action;
        }

        internal bool IsRef()
        {
            return this.Type.TypeName.Name.Equals("ref", StringComparison.OrdinalIgnoreCase);
        }

        IAssignableValue ISupportsAssignment.GetAssignableValue()
        {
            VariableExpressionAst child = base.Child as VariableExpressionAst;
            if ((child != null) && (child.TupleIndex >= 0))
            {
                return child;
            }
            return this;
        }

        public override System.Type StaticType
        {
            get
            {
                return (this.Type.TypeName.GetReflectionType() ?? typeof(object));
            }
        }

        public TypeConstraintAst Type
        {
            get
            {
                return (TypeConstraintAst) base.Attribute;
            }
        }

        
    }
}

