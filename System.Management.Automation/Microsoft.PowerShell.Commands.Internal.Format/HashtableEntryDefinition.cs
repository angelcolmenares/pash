namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal class HashtableEntryDefinition
    {
        private Type[] allowedTypes;
        private string key;
        private bool mandatory;
        private IEnumerable<string> secondaryNames;

        internal HashtableEntryDefinition(string name, Type[] types) : this(name, types, false)
        {
        }

        internal HashtableEntryDefinition(string name, Type[] types, bool mandatory)
        {
            this.key = name;
            this.allowedTypes = types;
            this.mandatory = mandatory;
        }

        internal HashtableEntryDefinition(string name, IEnumerable<string> secondaryNames, Type[] types, bool mandatory) : this(name, types, mandatory)
        {
            this.secondaryNames = secondaryNames;
        }

        internal virtual object ComputeDefaultValue()
        {
            return AutomationNull.Value;
        }

        internal virtual Hashtable CreateHashtableFromSingleType(object val)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        internal bool IsKeyMatch(string key)
        {
            if (CommandParameterDefinition.FindPartialMatch(key, this.KeyName))
            {
                return true;
            }
            if (this.SecondaryNames != null)
            {
                foreach (string str in this.SecondaryNames)
                {
                    if (CommandParameterDefinition.FindPartialMatch(key, str))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal virtual object Verify(object val, TerminatingErrorContext invocationContext, bool originalParameterWasHashTable)
        {
            return null;
        }

        internal Type[] AllowedTypes
        {
            get
            {
                return this.allowedTypes;
            }
        }

        internal string KeyName
        {
            get
            {
                return this.key;
            }
        }

        internal bool Mandatory
        {
            get
            {
                return this.mandatory;
            }
        }

        internal IEnumerable<string> SecondaryNames
        {
            get
            {
                return this.secondaryNames;
            }
        }
    }
}

