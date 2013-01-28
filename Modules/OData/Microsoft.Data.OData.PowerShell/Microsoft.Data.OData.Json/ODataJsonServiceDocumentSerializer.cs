namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;

    internal sealed class ODataJsonServiceDocumentSerializer : ODataJsonSerializer
    {
        internal ODataJsonServiceDocumentSerializer(ODataJsonOutputContext jsonOutputContext) : base(jsonOutputContext)
        {
        }

        internal void WriteServiceDocument(ODataWorkspace defaultWorkspace)
        {
            IEnumerable<ODataResourceCollectionInfo> collections = defaultWorkspace.Collections;
            base.WriteTopLevelPayload(delegate {
                this.JsonWriter.StartObjectScope();
                this.JsonWriter.WriteName("EntitySets");
                this.JsonWriter.StartArrayScope();
                if (collections != null)
                {
                    foreach (ODataResourceCollectionInfo info in collections)
                    {
                        ValidationUtils.ValidateResourceCollectionInfo(info);
                        this.JsonWriter.WriteValue(UriUtilsCommon.UriToString(info.Url));
                    }
                }
                this.JsonWriter.EndArrayScope();
                this.JsonWriter.EndObjectScope();
            });
        }
    }
}

