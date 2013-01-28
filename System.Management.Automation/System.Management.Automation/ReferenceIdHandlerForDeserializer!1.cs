namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    internal class ReferenceIdHandlerForDeserializer<T> where T: class
    {
        private readonly Dictionary<string, T> refId2object;

        public ReferenceIdHandlerForDeserializer()
        {
            this.refId2object = new Dictionary<string, T>();
        }

        internal T GetReferencedObject(string refId)
        {
            T local;
            if (this.refId2object.TryGetValue(refId, out local))
            {
                return local;
            }
            return default(T);
        }

        internal void SetRefId(T o, string refId, bool duplicateRefIdsAllowed)
        {
            this.refId2object[refId] = o;
        }
    }
}

