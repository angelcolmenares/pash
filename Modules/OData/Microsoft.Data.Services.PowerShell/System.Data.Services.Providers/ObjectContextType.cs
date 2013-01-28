namespace System.Data.Services.Providers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class ObjectContextType : IProviderType
    {
        private readonly StructuralType structuralType;

        internal ObjectContextType(StructuralType structuralType)
        {
            this.structuralType = structuralType;
        }

        public IEnumerable<IProviderMember> Members
        {
            get
            {
                foreach (EdmMember iteratorVariable0 in from m in this.structuralType.Members
                    where m.DeclaringType == this.structuralType
                    select m)
                {
                    yield return new ObjectContextMember(iteratorVariable0);
                }
            }
        }

        public string Name
        {
            get
            {
                return this.structuralType.Name;
            }
        }

        
    }
}

