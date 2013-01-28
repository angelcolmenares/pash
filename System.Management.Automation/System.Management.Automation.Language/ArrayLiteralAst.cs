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

    public class ArrayLiteralAst : ExpressionAst, ISupportsAssignment
    {
        public ArrayLiteralAst(IScriptExtent extent, IList<ExpressionAst> elements) : base(extent)
        {
            if ((elements == null) || !elements.Any<ExpressionAst>())
            {
                throw PSTraceSource.NewArgumentException("elements");
            }
            this.Elements = new ReadOnlyCollection<ExpressionAst>(elements);
            base.SetParents((IEnumerable<Ast>) this.Elements);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitArrayLiteral(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            yield return new PSTypeName(typeof(object[]));
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitArrayLiteral(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    foreach (ExpressionAst ast in this.Elements)
                    {
                        action = ast.InternalVisit(visitor);
                        if (action != AstVisitAction.Continue)
                        {
                            return action;
                        }
                    }
                    break;
            }
            return action;
        }

        IAssignableValue ISupportsAssignment.GetAssignableValue()
        {
            return new ArrayAssignableValue { ArrayLiteral = this };
        }

        public ReadOnlyCollection<ExpressionAst> Elements { get; private set; }

        public override Type StaticType
        {
            get
            {
                return typeof(object[]);
            }
        }

        
    }
}

