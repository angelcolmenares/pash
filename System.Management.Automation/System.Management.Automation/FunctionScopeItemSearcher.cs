namespace System.Management.Automation
{
    using System;
    using System.Runtime.InteropServices;

    internal class FunctionScopeItemSearcher : ScopedItemSearcher<FunctionInfo>
    {
        private readonly CommandOrigin _origin;
        private string name;

        public FunctionScopeItemSearcher(SessionStateInternal sessionState, VariablePath lookupPath, CommandOrigin origin) : base(sessionState, lookupPath)
        {
            this.name = string.Empty;
            this._origin = origin;
        }

        protected override bool GetScopeItem(SessionStateScope scope, VariablePath path, out FunctionInfo script)
        {
            bool flag = true;
            this.name = path.IsFunction ? path.UnqualifiedPath : path.QualifiedName;
            script = scope.GetFunction(this.name);
            if (script != null)
            {
                bool flag2;
                FilterInfo info = script as FilterInfo;
                if (info != null)
                {
                    flag2 = (info.Options & ScopedItemOptions.Private) != ScopedItemOptions.None;
                }
                else
                {
                    flag2 = (script.Options & ScopedItemOptions.Private) != ScopedItemOptions.None;
                }
                if (flag2 && (scope != base.sessionState.CurrentScope))
                {
                    return false;
                }
                SessionState.ThrowIfNotVisible(this._origin, script);
                return flag;
            }
            return false;
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

