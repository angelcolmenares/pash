namespace System.Management.Automation.Language
{
    using System;
    using System.Runtime.CompilerServices;

    public class MergingRedirectionAst : RedirectionAst
    {
        public MergingRedirectionAst(IScriptExtent extent, RedirectionStream from, RedirectionStream to) : base(extent, from)
        {
            this.ToStream = to;
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitMergingRedirection(this);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitMergingRedirection(this);
            if (action != AstVisitAction.SkipChildren)
            {
                return action;
            }
            return AstVisitAction.Continue;
        }

        public RedirectionStream ToStream { get; private set; }
    }
}

