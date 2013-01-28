namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public abstract class CommandBaseAst : StatementAst
    {
        private static readonly ReadOnlyCollection<RedirectionAst> EmptyRedirections = new ReadOnlyCollection<RedirectionAst>(new RedirectionAst[0]);
        internal const int MaxRedirections = 7;

        protected CommandBaseAst(IScriptExtent extent, IEnumerable<RedirectionAst> redirections) : base(extent)
        {
            if (redirections != null)
            {
                base.SetParents(redirections);
                this.Redirections = new ReadOnlyCollection<RedirectionAst>(redirections.ToArray<RedirectionAst>());
            }
            else
            {
                this.Redirections = EmptyRedirections;
            }
        }

        public ReadOnlyCollection<RedirectionAst> Redirections { get; private set; }
    }
}

