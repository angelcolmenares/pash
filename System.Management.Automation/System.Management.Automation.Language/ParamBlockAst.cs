namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class ParamBlockAst : Ast
    {
        private static readonly ReadOnlyCollection<AttributeAst> EmptyAttributeList = new ReadOnlyCollection<AttributeAst>(new AttributeAst[0]);
        private static readonly ReadOnlyCollection<ParameterAst> EmptyParameterList = new ReadOnlyCollection<ParameterAst>(new ParameterAst[0]);

        public ParamBlockAst(IScriptExtent extent, IEnumerable<AttributeAst> attributes, IEnumerable<ParameterAst> parameters) : base(extent)
        {
            if ((attributes != null) && attributes.Any<AttributeAst>())
            {
                this.Attributes = new ReadOnlyCollection<AttributeAst>(attributes.ToArray<AttributeAst>());
                base.SetParents(attributes);
            }
            else
            {
                this.Attributes = EmptyAttributeList;
            }
            if ((parameters != null) && parameters.Any<ParameterAst>())
            {
                this.Parameters = new ReadOnlyCollection<ParameterAst>(parameters.ToArray<ParameterAst>());
                base.SetParents(parameters);
            }
            else
            {
                this.Parameters = EmptyParameterList;
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitParamBlock(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return Ast.EmptyPSTypeNameArray;
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitParamBlock(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    foreach (AttributeAst ast in this.Attributes)
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
                foreach (ParameterAst ast2 in this.Parameters)
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

        internal static bool UsesCmdletBinding(IEnumerable<ParameterAst> parameters)
        {
            bool flag = false;
            foreach (ParameterAst ast in parameters)
            {
                flag = (from attribute in ast.Attributes
                    where (attribute.TypeName.GetReflectionAttributeType() != null) && attribute.TypeName.GetReflectionAttributeType().Equals(typeof(ParameterAttribute))
                    select attribute).Any<AttributeBaseAst>();
                if (flag)
                {
                    return flag;
                }
            }
            return flag;
        }

        public ReadOnlyCollection<AttributeAst> Attributes { get; private set; }

        public ReadOnlyCollection<ParameterAst> Parameters { get; private set; }
    }
}

