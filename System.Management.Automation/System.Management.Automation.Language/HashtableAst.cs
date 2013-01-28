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

    public class HashtableAst : ExpressionAst
    {
        private static readonly ReadOnlyCollection<Tuple<ExpressionAst, StatementAst>> EmptyKeyValuePairs = new ReadOnlyCollection<Tuple<ExpressionAst, StatementAst>>(new Tuple<ExpressionAst, StatementAst>[0]);

        public HashtableAst(IScriptExtent extent, IEnumerable<Tuple<ExpressionAst, StatementAst>> keyValuePairs) : base(extent)
        {
            if ((keyValuePairs != null) && keyValuePairs.Any<Tuple<ExpressionAst, StatementAst>>())
            {
                this.KeyValuePairs = new ReadOnlyCollection<Tuple<ExpressionAst, StatementAst>>(keyValuePairs.ToArray<Tuple<ExpressionAst, StatementAst>>());
                base.SetParents<ExpressionAst, StatementAst>(this.KeyValuePairs);
            }
            else
            {
                this.KeyValuePairs = EmptyKeyValuePairs;
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitHashtable(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            yield return new PSTypeName(typeof(Hashtable));
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitHashtable(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    foreach (Tuple<ExpressionAst, StatementAst> tuple in this.KeyValuePairs)
                    {
                        action = tuple.Item1.InternalVisit(visitor);
                        if (action != AstVisitAction.Continue)
                        {
                            return action;
                        }
                        action = tuple.Item2.InternalVisit(visitor);
                        if (action != AstVisitAction.Continue)
                        {
                            return action;
                        }
                    }
                    break;
            }
            return action;
        }

        public ReadOnlyCollection<Tuple<ExpressionAst, StatementAst>> KeyValuePairs { get; private set; }

        public override Type StaticType
        {
            get
            {
                return typeof(Hashtable);
            }
        }

        
    }
}

