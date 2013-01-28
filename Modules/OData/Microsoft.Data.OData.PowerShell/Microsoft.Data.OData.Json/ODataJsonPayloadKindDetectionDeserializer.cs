namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ODataJsonPayloadKindDetectionDeserializer : ODataJsonPropertyAndValueDeserializer
    {
        private readonly HashSet<ODataPayloadKind> detectedPayloadKinds;

        internal ODataJsonPayloadKindDetectionDeserializer(ODataJsonInputContext jsonInputContext) : base(jsonInputContext)
        {
            this.detectedPayloadKinds = new HashSet<ODataPayloadKind>(EqualityComparer<ODataPayloadKind>.Default);
        }

        private void AddOrRemovePayloadKinds(Func<ODataPayloadKind, bool> addOrRemoveAction, params ODataPayloadKind[] payloadKinds)
        {
            for (int i = 0; i < payloadKinds.Length; i++)
            {
                ODataPayloadKind payloadKind = payloadKinds[i];
                if (ODataUtilsInternal.IsPayloadKindSupported(payloadKind, !base.ReadingResponse))
                {
                    addOrRemoveAction(payloadKind);
                }
            }
        }

        private void AddPayloadKinds(params ODataPayloadKind[] payloadKinds)
        {
            this.AddOrRemovePayloadKinds(new Func<ODataPayloadKind, bool>(this.detectedPayloadKinds.Add), payloadKinds);
        }

        internal IEnumerable<ODataPayloadKind> DetectPayloadKind()
        {
            IEnumerable<ODataPayloadKind> detectedPayloadKinds;
            this.detectedPayloadKinds.Clear();
            base.JsonReader.DisableInStreamErrorDetection = true;
            try
            {
                base.ReadPayloadStart(false);
                switch (base.JsonReader.NodeType)
                {
                    case JsonNodeType.StartObject:
                    {
                        base.JsonReader.ReadStartObject();
                        int num = 0;
                        while (base.JsonReader.NodeType == JsonNodeType.Property)
                        {
                            string strB = base.JsonReader.ReadPropertyName();
                            num++;
                            if (string.CompareOrdinal("__metadata", strB) == 0)
                            {
                                this.ProcessMetadataPropertyValue();
                                break;
                            }
                            if (num == 1)
                            {
                                this.AddPayloadKinds(new ODataPayloadKind[] { ODataPayloadKind.Property, ODataPayloadKind.Entry, ODataPayloadKind.Parameter });
                                if ((string.CompareOrdinal("uri", strB) == 0) && (base.JsonReader.NodeType == JsonNodeType.PrimitiveValue))
                                {
                                    this.AddPayloadKinds(new ODataPayloadKind[] { ODataPayloadKind.EntityReferenceLink });
                                }
                                else
                                {
                                    ODataError error;
                                    if ((string.CompareOrdinal("error", strB) == 0) && base.JsonReader.TryReadInStreamErrorPropertyValue(out error))
                                    {
                                        this.AddPayloadKinds(new ODataPayloadKind[] { ODataPayloadKind.Error });
                                    }
                                }
                            }
                            else if (num == 2)
                            {
                                this.RemovePayloadKinds(new ODataPayloadKind[] { ODataPayloadKind.Property, ODataPayloadKind.EntityReferenceLink, ODataPayloadKind.Error });
                            }
                            if ((string.CompareOrdinal("results", strB) == 0) && (base.JsonReader.NodeType == JsonNodeType.StartArray))
                            {
                                this.DetectStartArrayPayloadKind(false);
                            }
                            else if ((base.ReadingResponse && (string.CompareOrdinal("EntitySets", strB) == 0)) && (base.JsonReader.NodeType == JsonNodeType.StartArray))
                            {
                                this.ProcessEntitySetsArray();
                            }
                            base.JsonReader.SkipValue();
                        }
                        if (num == 0)
                        {
                            this.AddPayloadKinds(new ODataPayloadKind[] { ODataPayloadKind.Entry, ODataPayloadKind.Parameter });
                        }
                        break;
                    }
                    case JsonNodeType.StartArray:
                        this.DetectStartArrayPayloadKind(true);
                        break;
                }
                detectedPayloadKinds = this.detectedPayloadKinds;
            }
            catch (ODataException)
            {
                detectedPayloadKinds = Enumerable.Empty<ODataPayloadKind>();
            }
            finally
            {
                base.JsonReader.DisableInStreamErrorDetection = false;
            }
            return detectedPayloadKinds;
        }

        private void DetectStartArrayPayloadKind(bool isTopLevel)
        {
            if (!isTopLevel)
            {
                this.AddPayloadKinds(new ODataPayloadKind[] { ODataPayloadKind.Property });
            }
            base.JsonReader.StartBuffering();
            try
            {
                base.JsonReader.ReadStartArray();
                switch (base.JsonReader.NodeType)
                {
                    case JsonNodeType.EndArray:
                    {
                        ODataPayloadKind[] kindArray2 = new ODataPayloadKind[3];
                        kindArray2[1] = ODataPayloadKind.Collection;
                        kindArray2[2] = ODataPayloadKind.EntityReferenceLinks;
                        this.AddPayloadKinds(kindArray2);
                        return;
                    }
                    case JsonNodeType.Property:
                        return;

                    case JsonNodeType.PrimitiveValue:
                        this.AddPayloadKinds(new ODataPayloadKind[] { ODataPayloadKind.Collection });
                        return;

                    case JsonNodeType.StartObject:
                        break;

                    default:
                        return;
                }
                base.JsonReader.ReadStartObject();
                bool flag = false;
                int num = 0;
                while (base.JsonReader.NodeType == JsonNodeType.Property)
                {
                    string strB = base.JsonReader.ReadPropertyName();
                    num++;
                    if (num > 1)
                    {
                        break;
                    }
                    if ((string.CompareOrdinal("uri", strB) == 0) && (base.JsonReader.NodeType == JsonNodeType.PrimitiveValue))
                    {
                        flag = true;
                    }
                    base.JsonReader.SkipValue();
                }
                ODataPayloadKind[] payloadKinds = new ODataPayloadKind[2];
                payloadKinds[1] = ODataPayloadKind.Collection;
                this.AddPayloadKinds(payloadKinds);
                if (flag && (num == 1))
                {
                    this.AddPayloadKinds(new ODataPayloadKind[] { ODataPayloadKind.EntityReferenceLinks });
                }
            }
            finally
            {
                base.JsonReader.StopBuffering();
            }
        }

        private void ProcessEntitySetsArray()
        {
            base.JsonReader.StartBuffering();
            try
            {
                base.JsonReader.ReadStartArray();
                if ((base.JsonReader.NodeType == JsonNodeType.EndArray) || (base.JsonReader.NodeType == JsonNodeType.PrimitiveValue))
                {
                    this.AddPayloadKinds(new ODataPayloadKind[] { ODataPayloadKind.ServiceDocument });
                }
            }
            finally
            {
                base.JsonReader.StopBuffering();
            }
        }

        private void ProcessMetadataPropertyValue()
        {
            this.detectedPayloadKinds.Clear();
            string typeName = base.ReadTypeNameFromMetadataPropertyValue();
            EdmTypeKind none = EdmTypeKind.None;
            if (typeName != null)
            {
                MetadataUtils.ResolveTypeNameForRead(EdmCoreModel.Instance, null, typeName, base.MessageReaderSettings.ReaderBehavior, base.Version, out none);
            }
            if ((none != EdmTypeKind.Primitive) && (none != EdmTypeKind.Collection))
            {
                this.detectedPayloadKinds.Add(ODataPayloadKind.Entry);
            }
        }

        private void RemovePayloadKinds(params ODataPayloadKind[] payloadKinds)
        {
            this.AddOrRemovePayloadKinds(new Func<ODataPayloadKind, bool>(this.detectedPayloadKinds.Remove), payloadKinds);
        }
    }
}

