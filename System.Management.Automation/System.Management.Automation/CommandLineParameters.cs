namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class CommandLineParameters
    {
        private readonly PSBoundParametersDictionary _dictionary = new PSBoundParametersDictionary();

        internal void Add(string name, object value)
        {
            this._dictionary[name] = value;
        }

        internal bool ContainsKey(string name)
        {
            return this._dictionary.ContainsKey(name);
        }

        internal HashSet<string> CopyBoundPositionalParameters()
        {
            HashSet<string> set = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (string str in this._dictionary.BoundPositionally)
            {
                set.Add(str);
            }
            return set;
        }

        internal IList GetImplicitUsingParameters()
        {
            return this._dictionary.ImplicitUsingParameters;
        }

        internal object GetValueToBindToPSBoundParameters()
        {
            return this._dictionary;
        }

        internal void MarkAsBoundPositionally(string name)
        {
            this._dictionary.BoundPositionally.Add(name);
        }

        internal void SetImplicitUsingParameters(object obj)
        {
            this._dictionary.ImplicitUsingParameters = PSObject.Base(obj) as IList;
        }

        internal void SetPSBoundParametersVariable(ExecutionContext context)
        {
            context.SetVariable(SpecialVariables.PSBoundParametersVarPath, this._dictionary);
        }

        internal void UpdateInvocationInfo(InvocationInfo invocationInfo)
        {
            invocationInfo.BoundParameters = this._dictionary;
        }
    }
}

