namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;

    public class PSMemberSet : PSMemberInfo
    {
        private PSObject constructorPSObject;
        private static Collection<CollectionEntry<PSMemberInfo>> emptyMemberCollection = new Collection<CollectionEntry<PSMemberInfo>>();
        private static Collection<CollectionEntry<PSMethodInfo>> emptyMethodCollection = new Collection<CollectionEntry<PSMethodInfo>>();
        private static Collection<CollectionEntry<PSPropertyInfo>> emptyPropertyCollection = new Collection<CollectionEntry<PSPropertyInfo>>();
        internal bool inheritMembers;
        internal PSMemberInfoInternalCollection<PSMemberInfo> internalMembers;
        private PSMemberInfoIntegratingCollection<PSMemberInfo> members;
        private PSMemberInfoIntegratingCollection<PSMethodInfo> methods;
        private PSMemberInfoIntegratingCollection<PSPropertyInfo> properties;
        private static Collection<CollectionEntry<PSMemberInfo>> typeMemberCollection = GetTypeMemberCollection();
        private static Collection<CollectionEntry<PSMethodInfo>> typeMethodCollection = GetTypeMethodCollection();
        private static Collection<CollectionEntry<PSPropertyInfo>> typePropertyCollection = GetTypePropertyCollection();

        public PSMemberSet(string name)
        {
            this.inheritMembers = true;
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            this.internalMembers = new PSMemberInfoInternalCollection<PSMemberInfo>();
            this.members = new PSMemberInfoIntegratingCollection<PSMemberInfo>(this, emptyMemberCollection);
            this.properties = new PSMemberInfoIntegratingCollection<PSPropertyInfo>(this, emptyPropertyCollection);
            this.methods = new PSMemberInfoIntegratingCollection<PSMethodInfo>(this, emptyMethodCollection);
        }

        public PSMemberSet(string name, IEnumerable<PSMemberInfo> members)
        {
            this.inheritMembers = true;
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            if (members == null)
            {
                throw PSTraceSource.NewArgumentNullException("members");
            }
            this.internalMembers = new PSMemberInfoInternalCollection<PSMemberInfo>();
            foreach (PSMemberInfo info in members)
            {
                if (info == null)
                {
                    throw PSTraceSource.NewArgumentNullException("members");
                }
                this.internalMembers.Add(info.Copy());
            }
            this.members = new PSMemberInfoIntegratingCollection<PSMemberInfo>(this, emptyMemberCollection);
            this.properties = new PSMemberInfoIntegratingCollection<PSPropertyInfo>(this, emptyPropertyCollection);
            this.methods = new PSMemberInfoIntegratingCollection<PSMethodInfo>(this, emptyMethodCollection);
        }

        internal PSMemberSet(string name, PSObject mshObject)
        {
            this.inheritMembers = true;
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            if (mshObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("mshObject");
            }
            this.constructorPSObject = mshObject;
            this.internalMembers = mshObject.InstanceMembers;
            this.members = new PSMemberInfoIntegratingCollection<PSMemberInfo>(this, typeMemberCollection);
            this.properties = new PSMemberInfoIntegratingCollection<PSPropertyInfo>(this, typePropertyCollection);
            this.methods = new PSMemberInfoIntegratingCollection<PSMethodInfo>(this, typeMethodCollection);
        }

        public override PSMemberInfo Copy()
        {
            if (this.constructorPSObject != null)
            {
                return new PSMemberSet(base.name, this.constructorPSObject);
            }
            PSMemberSet destiny = new PSMemberSet(base.name);
            foreach (PSMemberInfo info in this.Members)
            {
                destiny.Members.Add(info);
            }
            base.CloneBaseProperties(destiny);
            return destiny;
        }

        private static Collection<CollectionEntry<PSMemberInfo>> GetTypeMemberCollection()
        {
            return new Collection<CollectionEntry<PSMemberInfo>> { new CollectionEntry<PSMemberInfo>(new CollectionEntry<PSMemberInfo>.GetMembersDelegate(PSObject.TypeTableGetMembersDelegate<PSMemberInfo>), new CollectionEntry<PSMemberInfo>.GetMemberDelegate(PSObject.TypeTableGetMemberDelegate<PSMemberInfo>), true, true, "type table members") };
        }

        private static Collection<CollectionEntry<PSMethodInfo>> GetTypeMethodCollection()
        {
            return new Collection<CollectionEntry<PSMethodInfo>> { new CollectionEntry<PSMethodInfo>(new CollectionEntry<PSMethodInfo>.GetMembersDelegate(PSObject.TypeTableGetMembersDelegate<PSMethodInfo>), new CollectionEntry<PSMethodInfo>.GetMemberDelegate(PSObject.TypeTableGetMemberDelegate<PSMethodInfo>), true, true, "type table members") };
        }

        private static Collection<CollectionEntry<PSPropertyInfo>> GetTypePropertyCollection()
        {
            return new Collection<CollectionEntry<PSPropertyInfo>> { new CollectionEntry<PSPropertyInfo>(new CollectionEntry<PSPropertyInfo>.GetMembersDelegate(PSObject.TypeTableGetMembersDelegate<PSPropertyInfo>), new CollectionEntry<PSPropertyInfo>.GetMemberDelegate(PSObject.TypeTableGetMemberDelegate<PSPropertyInfo>), true, true, "type table members") };
        }

        internal override void ReplicateInstance(object particularInstance)
        {
            base.ReplicateInstance(particularInstance);
            foreach (PSMemberInfo info in this.Members)
            {
                info.ReplicateInstance(particularInstance);
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(" {");
            foreach (PSMemberInfo info in this.Members)
            {
                builder.Append(info.Name);
                builder.Append(", ");
            }
            if (builder.Length > 2)
            {
                builder.Remove(builder.Length - 2, 2);
            }
            builder.Insert(0, base.Name);
            builder.Append("}");
            return builder.ToString();
        }

        public bool InheritMembers
        {
            get
            {
                return this.inheritMembers;
            }
        }

        internal virtual PSMemberInfoInternalCollection<PSMemberInfo> InternalMembers
        {
            get
            {
                return this.internalMembers;
            }
        }

        public PSMemberInfoCollection<PSMemberInfo> Members
        {
            get
            {
                return this.members;
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.MemberSet;
            }
        }

        public PSMemberInfoCollection<PSMethodInfo> Methods
        {
            get
            {
                return this.methods;
            }
        }

        public PSMemberInfoCollection<PSPropertyInfo> Properties
        {
            get
            {
                return this.properties;
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                return typeof(PSMemberSet).FullName;
            }
        }

        public override object Value
        {
            get
            {
                return this;
            }
            set
            {
                throw new ExtendedTypeSystemException("CannotChangePSMemberSetValue", null, ExtendedTypeSystem.CannotSetValueForMemberType, new object[] { base.GetType().FullName });
            }
        }
    }
}

