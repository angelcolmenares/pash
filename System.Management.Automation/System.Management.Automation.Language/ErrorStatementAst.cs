namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class ErrorStatementAst : PipelineBaseAst
    {
        internal ErrorStatementAst(IScriptExtent extent, IEnumerable<Ast> nestedAsts = null) : base(extent)
        {
            if ((nestedAsts != null) && nestedAsts.Any<Ast>())
            {
                this.NestedAst = new ReadOnlyCollection<Ast>(nestedAsts.ToArray<Ast>());
                base.SetParents(this.NestedAst);
            }
        }

        internal ErrorStatementAst(IScriptExtent extent, Token kind, IEnumerable<Ast> nestedAsts = null) : base(extent)
        {
            if (kind == null)
            {
                throw PSTraceSource.NewArgumentNullException("kind");
            }
            this.Kind = kind;
            if ((nestedAsts != null) && nestedAsts.Any<Ast>())
            {
                this.NestedAst = new ReadOnlyCollection<Ast>(nestedAsts.ToArray<Ast>());
                base.SetParents(this.NestedAst);
            }
        }

        internal ErrorStatementAst(IScriptExtent extent, Token kind, IEnumerable<KeyValuePair<string, Tuple<Token, Ast>>> flags, IEnumerable<Ast> conditions, IEnumerable<Ast> bodies) : base(extent)
        {
            if (kind == null)
            {
                throw PSTraceSource.NewArgumentNullException("kind");
            }
            this.Kind = kind;
            if ((flags != null) && flags.Any<KeyValuePair<string, Tuple<Token, Ast>>>())
            {
                this.Flags = new Dictionary<string, Tuple<Token, Ast>>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, Tuple<Token, Ast>> pair in flags)
                {
                    if (!this.Flags.ContainsKey(pair.Key))
                    {
                        this.Flags.Add(pair.Key, pair.Value);
                        if (pair.Value.Item2 != null)
                        {
                            base.SetParent(pair.Value.Item2);
                        }
                    }
                }
            }
            if ((conditions != null) && conditions.Any<Ast>())
            {
                this.Conditions = new ReadOnlyCollection<Ast>(conditions.ToArray<Ast>());
                base.SetParents(conditions);
            }
            if ((bodies != null) && bodies.Any<Ast>())
            {
                this.Bodies = new ReadOnlyCollection<Ast>(bodies.ToArray<Ast>());
                base.SetParents(bodies);
            }
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitErrorStatement(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.Conditions.Concat<Ast>(this.Bodies).Concat<Ast>(this.NestedAst).SelectMany(x => x.GetInferredType(context));
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitErrorStatement(this);
            if (action == AstVisitAction.SkipChildren)
            {
                return AstVisitAction.Continue;
            }
            if ((action == AstVisitAction.Continue) && (this.NestedAst != null))
            {
                foreach (Ast ast in this.NestedAst)
                {
                    action = ast.InternalVisit(visitor);
                    if (action != AstVisitAction.Continue)
                    {
                        break;
                    }
                }
            }
            if ((action == AstVisitAction.Continue) && (this.Flags != null))
            {
                foreach (Tuple<Token, Ast> tuple in this.Flags.Values)
                {
                    if (tuple.Item2 != null)
                    {
                        action = tuple.Item2.InternalVisit(visitor);
                        if (action != AstVisitAction.Continue)
                        {
                            break;
                        }
                    }
                }
            }
            if ((action == AstVisitAction.Continue) && (this.Conditions != null))
            {
                foreach (Ast ast2 in this.Conditions)
                {
                    action = ast2.InternalVisit(visitor);
                    if (action != AstVisitAction.Continue)
                    {
                        break;
                    }
                }
            }
            if ((action == AstVisitAction.Continue) && (this.Bodies != null))
            {
                foreach (Ast ast3 in this.Bodies)
                {
                    action = ast3.InternalVisit(visitor);
                    if (action != AstVisitAction.Continue)
                    {
                        return action;
                    }
                }
            }
            return action;
        }

        public ReadOnlyCollection<Ast> Bodies { get; private set; }

        public ReadOnlyCollection<Ast> Conditions { get; private set; }

        public Dictionary<string, Tuple<Token, Ast>> Flags { get; private set; }

        public Token Kind { get; private set; }

        public ReadOnlyCollection<Ast> NestedAst { get; private set; }
    }
}

