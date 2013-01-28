namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;

    public class FormObjectCollection : Collection<FormObject>
    {
        public FormObject this[string key]
        {
            get
            {
                foreach (FormObject obj3 in this)
                {
                    if (string.Equals(key, obj3.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        return obj3;
                    }
                }
                return null;
            }
        }
    }
}

