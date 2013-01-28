namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    internal class ReferenceIdHandlerForSerializer<T> where T: class
    {
        private readonly IDictionary<T, ulong> object2refId;
        private ulong seed;

        internal ReferenceIdHandlerForSerializer(IDictionary<T, ulong> dictionary)
        {
            this.object2refId = dictionary;
        }

        private ulong GetNewReferenceId()
        {
            ulong num2;
            this.seed = (num2 = this.seed) + ((ulong) 1L);
            return num2;
        }

        internal string GetRefId(T t)
        {
            ulong num;
            if ((this.object2refId != null) && this.object2refId.TryGetValue(t, out num))
            {
                return num.ToString(CultureInfo.InvariantCulture);
            }
            return null;
        }

        internal string SetRefId(T t)
        {
            if (this.object2refId != null)
            {
                ulong newReferenceId = this.GetNewReferenceId();
                this.object2refId.Add(t, newReferenceId);
                return newReferenceId.ToString(CultureInfo.InvariantCulture);
            }
            return null;
        }
    }
}

