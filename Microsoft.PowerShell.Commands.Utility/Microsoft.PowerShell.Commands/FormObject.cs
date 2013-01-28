namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class FormObject
    {
        public FormObject(string id, string method, string action)
        {
            this.Id = id;
            this.Method = method;
            this.Action = action;
            this.Fields = new Dictionary<string, string>();
        }

        internal void AddField(string key, string value)
        {
            string str;
            if ((key != null) && !this.Fields.TryGetValue(key, out str))
            {
                this.Fields[key] = value;
            }
        }

        public string Action { get; private set; }

        public Dictionary<string, string> Fields { get; private set; }

        public string Id { get; private set; }

        public string Method { get; private set; }
    }
}

