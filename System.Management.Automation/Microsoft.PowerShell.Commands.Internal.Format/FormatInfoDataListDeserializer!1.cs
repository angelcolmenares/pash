namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;

    internal static class FormatInfoDataListDeserializer<T> where T: FormatInfoData
    {
        internal static void ReadList(PSObject so, string property, List<T> lst, FormatObjectDeserializer deserializer)
        {
            if (lst == null)
            {
                throw PSTraceSource.NewArgumentNullException("lst");
            }
            FormatInfoDataListDeserializer<T>.ReadListHelper(PSObjectHelper.GetEnumerable(FormatObjectDeserializer.GetProperty(so, property)), lst, deserializer);
        }

        private static void ReadListHelper(IEnumerable en, List<T> lst, FormatObjectDeserializer deserializer)
        {
            deserializer.VerifyDataNotNull(en, "enumerable");
            foreach (object obj2 in en)
            {
                T local = deserializer.DeserializeObject(PSObjectHelper.AsPSObject(obj2)) as T;
                deserializer.VerifyDataNotNull(local, "entry");
                lst.Add(local);
            }
        }
    }
}

