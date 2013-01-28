namespace System.Management.Automation.Language
{
    using System;

    public class TypeConstraintAst : AttributeBaseAst
    {
        public TypeConstraintAst(IScriptExtent extent, ITypeName typeName) : base(extent, typeName)
        {
        }

        public TypeConstraintAst(IScriptExtent extent, Type type) : base(extent, new ReflectionTypeName(type))
        {
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitTypeConstraint(this);
        }

        internal override Attribute GetAttribute()
        {
            return Compiler.GetAttribute(this);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitTypeConstraint(this);
            if (action != AstVisitAction.SkipChildren)
            {
                return action;
            }
            return AstVisitAction.Continue;
        }
    }
}

