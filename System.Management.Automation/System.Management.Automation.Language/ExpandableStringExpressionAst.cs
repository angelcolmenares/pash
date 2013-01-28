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

    public class ExpandableStringExpressionAst : ExpressionAst
    {
        public ExpandableStringExpressionAst(IScriptExtent extent, string value, System.Management.Automation.Language.StringConstantType type) : base(extent)
        {
            if (value == null)
            {
                throw PSTraceSource.NewArgumentNullException("value");
            }
            if (((type != System.Management.Automation.Language.StringConstantType.DoubleQuoted) && (type != System.Management.Automation.Language.StringConstantType.DoubleQuotedHereString)) && (type != System.Management.Automation.Language.StringConstantType.BareWord))
            {
                throw PSTraceSource.NewArgumentException("type");
            }
            ExpressionAst ast = Parser.ScanString(value);
            ExpandableStringExpressionAst ast2 = ast as ExpandableStringExpressionAst;
            if (ast2 != null)
            {
                this.FormatExpression = ast2.FormatExpression;
                this.NestedExpressions = ast2.NestedExpressions;
            }
            else
            {
                this.FormatExpression = "{0}";
                this.NestedExpressions = new ReadOnlyCollection<ExpressionAst>(new ExpressionAst[] { ast });
            }
            this.Value = value;
            this.StringConstantType = type;
        }

        internal ExpandableStringExpressionAst(Token token, string value, string formatString, IEnumerable<ExpressionAst> nestedExpressions) : base(token.Extent)
        {
            this.FormatExpression = formatString;
            this.Value = value;
            this.StringConstantType = StringConstantExpressionAst.MapTokenKindToStringContantKind(token);
            this.NestedExpressions = new ReadOnlyCollection<ExpressionAst>(nestedExpressions.ToArray<ExpressionAst>());
            base.SetParents((IEnumerable<Ast>) this.NestedExpressions);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitExpandableStringExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            yield return new PSTypeName(typeof(string));
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitExpandableStringExpression(this);
            if (action == AstVisitAction.SkipChildren)
            {
                return AstVisitAction.Continue;
            }
            if ((action == AstVisitAction.Continue) && (this.NestedExpressions != null))
            {
                foreach (ExpressionAst ast in this.NestedExpressions)
                {
                    action = ast.InternalVisit(visitor);
                    if (action != AstVisitAction.Continue)
                    {
                        return action;
                    }
                }
            }
            return action;
        }

        internal string FormatExpression { get; private set; }

        public ReadOnlyCollection<ExpressionAst> NestedExpressions { get; private set; }

        public override Type StaticType
        {
            get
            {
                return typeof(string);
            }
        }

        public System.Management.Automation.Language.StringConstantType StringConstantType { get; private set; }

        public string Value { get; private set; }

        
    }
}

