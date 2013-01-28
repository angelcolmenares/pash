namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class ModuleSpecification
    {
        public ModuleSpecification(Hashtable moduleSpecification)
        {
            if (moduleSpecification == null)
            {
                throw new ArgumentNullException("moduleSpecification");
            }
            StringBuilder builder = new StringBuilder();
            foreach (DictionaryEntry entry in moduleSpecification)
            {
                if (entry.Key.ToString().Equals("ModuleName", StringComparison.OrdinalIgnoreCase))
                {
                    this.Name = LanguagePrimitives.ConvertTo<string>(entry.Value);
                }
                else if (entry.Key.ToString().Equals("ModuleVersion", StringComparison.OrdinalIgnoreCase))
                {
                    this.Version = LanguagePrimitives.ConvertTo<System.Version>(entry.Value);
                }
                else if (entry.Key.ToString().Equals("GUID", StringComparison.OrdinalIgnoreCase))
                {
                    this.Guid = LanguagePrimitives.ConvertTo<System.Guid?>(entry.Value);
                }
                else
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append("'");
                    builder.Append(entry.Key.ToString());
                    builder.Append("'");
                }
            }
            if (builder.Length != 0)
            {
                throw new ArgumentException(StringUtil.Format(Modules.InvalidModuleSpecificationMember, "ModuleName, ModuleVersion, GUID", builder));
            }
            if (string.IsNullOrEmpty(this.Name))
            {
                throw new MissingMemberException(StringUtil.Format(Modules.RequiredModuleMissingModuleName, new object[0]));
            }
            if (this.Version == null)
            {
                throw new MissingMemberException(StringUtil.Format(Modules.RequiredModuleMissingModuleVersion, new object[0]));
            }
        }

        internal ModuleSpecification(PSModuleInfo moduleInfo)
        {
            if (moduleInfo == null)
            {
                throw new ArgumentNullException("moduleInfo");
            }
            this.Name = moduleInfo.Name;
            this.Version = moduleInfo.Version;
            this.Guid = new System.Guid?(moduleInfo.Guid);
        }

        public ModuleSpecification(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                throw new ArgumentNullException("moduleName");
            }
            this.Name = moduleName;
            this.Version = null;
            this.Guid = null;
        }

        public System.Guid? Guid { get; internal set; }

        public string Name { get; private set; }

        public System.Version Version { get; internal set; }
    }
}

