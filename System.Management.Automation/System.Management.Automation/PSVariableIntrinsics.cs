namespace System.Management.Automation
{
    using System;

    public sealed class PSVariableIntrinsics
    {
        private SessionStateInternal sessionState;

        private PSVariableIntrinsics()
        {
        }

        internal PSVariableIntrinsics(SessionStateInternal sessionState)
        {
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentException("sessionState");
            }
            this.sessionState = sessionState;
        }

        public PSVariable Get(string name)
        {
            if ((name != null) && name.Equals(string.Empty))
            {
                return null;
            }
            return this.sessionState.GetVariable(name);
        }

        internal PSVariable GetAtScope(string name, string scope)
        {
            return this.sessionState.GetVariableAtScope(name, scope);
        }

        public object GetValue(string name)
        {
            return this.sessionState.GetVariableValue(name);
        }

        public object GetValue(string name, object defaultValue)
        {
            return (this.sessionState.GetVariableValue(name) ?? defaultValue);
        }

        internal object GetValueAtScope(string name, string scope)
        {
            return this.sessionState.GetVariableValueAtScope(name, scope);
        }

        public void Remove(PSVariable variable)
        {
            this.sessionState.RemoveVariable(variable);
        }

        public void Remove(string name)
        {
            this.sessionState.RemoveVariable(name);
        }

        internal void RemoveAtScope(PSVariable variable, string scope)
        {
            this.sessionState.RemoveVariableAtScope(variable, scope);
        }

        internal void RemoveAtScope(string name, string scope)
        {
            this.sessionState.RemoveVariableAtScope(name, scope);
        }

        public void Set(PSVariable variable)
        {
            this.sessionState.SetVariable(variable, false, CommandOrigin.Internal);
        }

        public void Set(string name, object value)
        {
            this.sessionState.SetVariableValue(name, value, CommandOrigin.Internal);
        }
    }
}

