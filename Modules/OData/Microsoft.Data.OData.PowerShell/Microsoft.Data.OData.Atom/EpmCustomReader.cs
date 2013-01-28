

namespace Microsoft.Data.OData.Atom
{
    using System;
    using System.Collections.Generic;
	using Microsoft.Data.OData.Metadata;

    internal sealed class EpmCustomReader : EpmReader
    {
        private EpmCustomReader(IODataAtomReaderEntryState entryState, ODataAtomInputContext inputContext) : base(entryState, inputContext)
        {
        }

        private void ReadEntryEpm()
        {
            foreach (KeyValuePair<EntityPropertyMappingInfo, string> pair in base.EntryState.EpmCustomReaderValueCache.CustomEpmValues)
            {
                base.SetEntryEpmValue(pair.Key, pair.Value);
            }
        }

        internal static void ReadEntryEpm(IODataAtomReaderEntryState entryState, ODataAtomInputContext inputContext)
        {
            new EpmCustomReader(entryState, inputContext).ReadEntryEpm();
        }
    }
}

