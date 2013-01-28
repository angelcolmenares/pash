namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class ConstantExpressionAst : ExpressionAst
    {
        internal ConstantExpressionAst(NumberToken token) : base(token.Extent)
        {
            this.Value = token.Value;
        }

        public ConstantExpressionAst(IScriptExtent extent, object value) : base(extent)
        {
            this.Value = value;
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitConstantExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            if (this.Value == null)
            {
                yield break;
            }
            yield return new PSTypeName(this.Value.GetType());
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitConstantExpression(this);
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
                if (this.Value == null)
                {
                    return typeof(object);
                }
                return this.Value.GetType();
            }
        }

        public object Value { get; private set; }

        
    }
}

