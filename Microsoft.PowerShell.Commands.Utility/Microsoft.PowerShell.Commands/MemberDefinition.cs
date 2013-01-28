namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    public class MemberDefinition
    {
        private string definition;
        private PSMemberTypes memberType;
        private string name;
        private string typeName;

        public MemberDefinition(string typeName, string name, PSMemberTypes memberType, string definition)
        {
            this.name = name;
            this.definition = definition;
            this.memberType = memberType;
            this.typeName = typeName;
        }

        public override string ToString()
        {
            return this.definition;
        }

        public string Definition
        {
            get
            {
                return this.definition;
            }
        }

        public PSMemberTypes MemberType
        {
            get
            {
                return this.memberType;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public string TypeName
        {
            get
            {
                return this.typeName;
            }
        }
    }
}

