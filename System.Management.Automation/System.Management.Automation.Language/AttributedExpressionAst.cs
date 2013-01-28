namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class AttributedExpressionAst : ExpressionAst, ISupportsAssignment, IAssignableValue
    {
        public AttributedExpressionAst(IScriptExtent extent, AttributeBaseAst attribute, ExpressionAst child) : base(extent)
        {
            if ((attribute == null) || (child == null))
            {
                throw PSTraceSource.NewArgumentNullException((attribute == null) ? "attribute" : "child");
            }
            this.Attribute = attribute;
            base.SetParent(attribute);
            this.Child = child;
            base.SetParent(child);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitAttributedExpression(this);
        }

        private ISupportsAssignment GetActualAssignableAst()
        {
            ExpressionAst child = this;
            for (AttributedExpressionAst ast2 = child as AttributedExpressionAst; ast2 != null; ast2 = child as AttributedExpressionAst)
            {
                child = ast2.Child;
            }
            return (ISupportsAssignment) child;
        }

        private List<AttributeBaseAst> GetAttributes()
        {
            List<AttributeBaseAst> list = new List<AttributeBaseAst>();
            for (AttributedExpressionAst ast = this; ast != null; ast = ast.Child as AttributedExpressionAst)
            {
                list.Add(ast.Attribute);
            }
            list.Reverse();
            return list;
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.Child.GetInferredType(context);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitAttributedExpression(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Attribute.InternalVisit(visitor);
                    break;
            }
            if (action == AstVisitAction.Continue)
            {
                action = this.Child.InternalVisit(visitor);
            }
            return action;
        }

        Expression IAssignableValue.GetValue(Compiler compiler, List<Expression> exprs, List<ParameterExpression> temps)
        {
            return (Expression) this.Accept(compiler);
        }

        Expression IAssignableValue.SetValue(Compiler compiler, Expression rhs)
        {
            List<AttributeBaseAst> attributes = this.GetAttributes();
            IAssignableValue assignableValue = this.GetActualAssignableAst().GetAssignableValue();
            VariableExpressionAst ast = assignableValue as VariableExpressionAst;
            if (ast == null)
            {
                return assignableValue.SetValue(compiler, Compiler.ConvertValue(rhs, attributes));
            }
            return Compiler.CallSetVariable(Expression.Constant(ast.VariablePath), rhs, Expression.Constant(attributes.ToArray()));
        }

        IAssignableValue ISupportsAssignment.GetAssignableValue()
        {
            return this;
        }

        public AttributeBaseAst Attribute { get; private set; }

        public ExpressionAst Child { get; private set; }
    }
}

