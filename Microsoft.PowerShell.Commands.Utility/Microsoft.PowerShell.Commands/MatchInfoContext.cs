namespace Microsoft.PowerShell.Commands
{
    using System;

    public sealed class MatchInfoContext : ICloneable
    {
        private string[] displayPostContext;
        private string[] displayPreContext;
        private string[] postContext;
        private string[] preContext;

        internal MatchInfoContext()
        {
        }

        public object Clone()
        {
            MatchInfoContext context = this;
            return new MatchInfoContext { PreContext = (context.PreContext != null) ? ((string[]) this.PreContext.Clone()) : null, PostContext = (context.PostContext != null) ? ((string[]) this.PostContext.Clone()) : null, DisplayPreContext = (context.DisplayPreContext != null) ? ((string[]) this.DisplayPreContext.Clone()) : null, DisplayPostContext = (context.DisplayPostContext != null) ? ((string[]) this.DisplayPostContext.Clone()) : null };
        }

        public string[] DisplayPostContext
        {
            get
            {
                return this.displayPostContext;
            }
            set
            {
                this.displayPostContext = value;
            }
        }

        public string[] DisplayPreContext
        {
            get
            {
                return this.displayPreContext;
            }
            set
            {
                this.displayPreContext = value;
            }
        }

        public string[] PostContext
        {
            get
            {
                return this.postContext;
            }
            set
            {
                this.postContext = value;
            }
        }

        public string[] PreContext
        {
            get
            {
                return this.preContext;
            }
            set
            {
                this.preContext = value;
            }
        }
    }
}

