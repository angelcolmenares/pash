namespace System.Data.Services.Serializers
{
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Atom;
    using System;
    using System.Data.Services.Providers;
    using System.Linq;

    internal sealed class ServiceDocumentSerializer
    {
        private readonly ODataMessageWriter writer;

        internal ServiceDocumentSerializer(ODataMessageWriter writer)
        {
            this.writer = writer;
        }

        internal void WriteServiceDocument(DataServiceProviderWrapper provider)
        {
            ODataWorkspace defaultWorkspace = new ODataWorkspace {
                Collections = provider.GetResourceSets().Select<ResourceSetWrapper, ODataResourceCollectionInfo>(delegate (ResourceSetWrapper rs) {
                    ODataResourceCollectionInfo info = new ODataResourceCollectionInfo {
                        Url = new Uri(rs.Name, UriKind.RelativeOrAbsolute)
                    };
                    AtomResourceCollectionMetadata annotation = new AtomResourceCollectionMetadata();
                    AtomTextConstruct construct = new AtomTextConstruct {
                        Text = rs.Name
                    };
                    annotation.Title = construct;
                    info.SetAnnotation<AtomResourceCollectionMetadata>(annotation);
                    return info;
                })
            };
            this.writer.WriteServiceDocument(defaultWorkspace);
        }
    }
}

