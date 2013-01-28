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
    using System.Threading;

    public class ParameterAst : Ast
    {
        private static readonly ReadOnlyCollection<AttributeBaseAst> EmptyAttributeList = new ReadOnlyCollection<AttributeBaseAst>(new AttributeBaseAst[0]);

        public ParameterAst(IScriptExtent extent, VariableExpressionAst name, IEnumerable<AttributeBaseAst> attributes, ExpressionAst defaultValue) : base(extent)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            if ((attributes != null) && attributes.Any<AttributeBaseAst>())
            {
                this.Attributes = new ReadOnlyCollection<AttributeBaseAst>(attributes.ToArray<AttributeBaseAst>());
                base.SetParents(attributes);
            }
            else
            {
                this.Attributes = EmptyAttributeList;
            }
            this.Name = name;
            base.SetParent(name);
            if (defaultValue != null)
            {
                this.DefaultValue = defaultValue;
                base.SetParent(defaultValue);
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitParameter(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            TypeConstraintAst iteratorVariable0 = this.Attributes.OfType<TypeConstraintAst>().FirstOrDefault<TypeConstraintAst>();
            if (iteratorVariable0 != null)
            {
                Type reflectionType = iteratorVariable0.TypeName.GetReflectionType();
                if (reflectionType != null)
                {
                    yield return new PSTypeName(reflectionType);
                }
                else
                {
                    yield return new PSTypeName(iteratorVariable0.TypeName.FullName);
                }
            }
            IEnumerator<AttributeAst> enumerator = this.Attributes.OfType<AttributeAst>().GetEnumerator();
            while (enumerator.MoveNext())
            {
                AttributeAst current = enumerator.Current;
                PSTypeNameAttribute attribute = null;
                try
                {
                    attribute = current.GetAttribute() as PSTypeNameAttribute;
                }
                catch (RuntimeException)
                {
                }
                if (attribute != null)
                {
                    yield return new PSTypeName(attribute.PSTypeName);
                }
            }
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitParameter(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    foreach (AttributeBaseAst ast in this.Attributes)
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
                action = this.Name.InternalVisit(visitor);
            }
            if ((action == AstVisitAction.Continue) && (this.DefaultValue != null))
            {
                action = this.DefaultValue.InternalVisit(visitor);
            }
            return action;
        }

        public ReadOnlyCollection<AttributeBaseAst> Attributes { get; private set; }

        public ExpressionAst DefaultValue { get; private set; }

        public VariableExpressionAst Name { get; private set; }

        public Type StaticType
        {
            get
            {
                Type reflectionType = null;
                TypeConstraintAst ast = this.Attributes.OfType<TypeConstraintAst>().FirstOrDefault<TypeConstraintAst>();
                if (ast != null)
                {
                    reflectionType = ast.TypeName.GetReflectionType();
                }
                return (reflectionType ?? typeof(object));
            }
        }

        
    }
}

