namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    public class WebCmdletElementCollection : ReadOnlyCollection<PSObject>
    {
        internal WebCmdletElementCollection(IList<PSObject> list) : base(list)
        {
        }

        public PSObject Find(string nameOrId)
        {
            PSObject obj2 = this.FindById(nameOrId);
            if (obj2 == null)
            {
                obj2 = this.FindByName(nameOrId);
            }
            return obj2;
        }

        private PSObject Find(string nameOrId, bool findById)
        {
            foreach (PSObject obj2 in this)
            {
                PSPropertyInfo info = obj2.Properties[findById ? "id" : "name"];
                if ((info != null) && (((string) info.Value) == nameOrId))
                {
                    return obj2;
                }
            }
            return null;
        }

        public PSObject FindById(string id)
        {
            return this.Find(id, true);
        }

        public PSObject FindByName(string name)
        {
            return this.Find(name, false);
        }
    }
}

