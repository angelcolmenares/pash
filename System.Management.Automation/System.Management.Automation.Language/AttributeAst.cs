namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class AttributeAst : AttributeBaseAst
    {
        private static readonly ReadOnlyCollection<NamedAttributeArgumentAst> EmptyNamedAttributeArguments = new ReadOnlyCollection<NamedAttributeArgumentAst>(new NamedAttributeArgumentAst[0]);
        private static readonly ReadOnlyCollection<ExpressionAst> EmptyPositionalArguments = new ReadOnlyCollection<ExpressionAst>(new ExpressionAst[0]);

        public AttributeAst(IScriptExtent extent, ITypeName typeName, IEnumerable<ExpressionAst> positionalArguments, IEnumerable<NamedAttributeArgumentAst> namedArguments) : base(extent, typeName)
        {
            if ((positionalArguments != null) && positionalArguments.Any<ExpressionAst>())
            {
                this.PositionalArguments = new ReadOnlyCollection<ExpressionAst>(positionalArguments.ToArray<ExpressionAst>());
                base.SetParents(positionalArguments);
            }
            else
            {
                this.PositionalArguments = EmptyPositionalArguments;
            }
            if ((namedArguments != null) && namedArguments.Any<NamedAttributeArgumentAst>())
            {
                this.NamedArguments = new ReadOnlyCollection<NamedAttributeArgumentAst>(namedArguments.ToArray<NamedAttributeArgumentAst>());
                base.SetParents(namedArguments);
            }
            else
            {
                this.NamedArguments = EmptyNamedAttributeArguments;
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitAttribute(this);
        }

        internal override Attribute GetAttribute()
        {
            return Compiler.GetAttribute(this);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitAttribute(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    foreach (ExpressionAst ast in this.PositionalArguments)
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
                foreach (NamedAttributeArgumentAst ast2 in this.NamedArguments)
                {
                    action = ast2.InternalVisit(visitor);
                    if (action != AstVisitAction.Continue)
                    {
                        return action;
                    }
                }
            }
            return action;
        }

        public ReadOnlyCollection<NamedAttributeArgumentAst> NamedArguments { get; private set; }

        public ReadOnlyCollection<ExpressionAst> PositionalArguments { get; private set; }
    }
}

