namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class TypeExpressionAst : ExpressionAst
    {
        public TypeExpressionAst(IScriptExtent extent, ITypeName typeName) : base(extent)
        {
            if (typeName == null)
            {
                throw PSTraceSource.NewArgumentNullException("typeName");
            }
            this.TypeName = typeName;
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitTypeExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            yield return new PSTypeName(this.StaticType);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitTypeExpression(this);
            if (action != AstVisitAction.SkipChildren)
            {
                return action;
            }
            return AstVisitAction.Continue;
        }

        public override Type StaticType
        {
            get
            {
                return typeof(Type);
            }
        }

        public ITypeName TypeName { get; private set; }

        
    }
}

