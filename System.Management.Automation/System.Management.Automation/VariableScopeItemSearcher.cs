namespace System.Management.Automation
{
    using System;
    using System.Runtime.InteropServices;

    internal class VariableScopeItemSearcher : ScopedItemSearcher<PSVariable>
    {
        private readonly CommandOrigin _origin;

        public VariableScopeItemSearcher(SessionStateInternal sessionState, VariablePath lookupPath, CommandOrigin origin) : base(sessionState, lookupPath)
        {
            this._origin = origin;
        }

        protected override bool GetScopeItem(SessionStateScope scope, VariablePath name, out PSVariable variable)
        {
            bool flag = true;
            variable = scope.GetVariable(name.QualifiedName, this._origin);
            return (((variable != null) && (!variable.IsPrivate || (scope == base.sessionState.CurrentScope))) && flag);
        }
    }
}

