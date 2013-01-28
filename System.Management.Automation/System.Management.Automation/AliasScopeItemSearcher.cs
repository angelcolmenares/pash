namespace System.Management.Automation
{
    using System;
    using System.Runtime.InteropServices;

    internal class AliasScopeItemSearcher : ScopedItemSearcher<AliasInfo>
    {
        public AliasScopeItemSearcher(SessionStateInternal sessionState, VariablePath lookupPath) : base(sessionState, lookupPath)
        {
        }

        protected override bool GetScopeItem(SessionStateScope scope, VariablePath name, out AliasInfo alias)
        {
            bool flag = true;
            alias = scope.GetAlias(name.QualifiedName);
            return (((alias != null) && (((alias.Options & ScopedItemOptions.Private) == ScopedItemOptions.None) || (scope == base.sessionState.CurrentScope))) && flag);
        }
    }
}

