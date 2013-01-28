namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;

    internal sealed class MediaTypeResolver
    {
        private static readonly Dictionary<ODataPayloadKind, MediaTypeWithFormat[]> additionalClientV2MediaTypes;
        private static readonly MediaType ApplicationAtomXmlMediaType = new MediaType("application", "atom+xml");
        private static readonly MediaType ApplicationJsonMediaType = new MediaType("application", "json");
        private static readonly MediaType ApplicationJsonVerboseMediaType = new MediaType("application", "json", new KeyValuePair<string, string>("odata", "verbose"));
        private static readonly MediaType ApplicationXmlMediaType = new MediaType("application", "xml");
        private static MediaTypeResolver defaultMediaTypeResolver;
        private static readonly MediaTypeWithFormat[][] defaultMediaTypes;
        private MediaTypeWithFormat[][] mediaTypesForPayloadKind = new MediaTypeWithFormat[defaultMediaTypes.Length][];
        private static readonly Dictionary<ODataPayloadKind, MediaTypeWithFormat[]> plainAppJsonMeansVerbose;
        private static readonly MediaType TextXmlMediaType = new MediaType("text", "xml");

        static MediaTypeResolver()
        {
            MediaTypeWithFormat[][] formatArray = new MediaTypeWithFormat[13][];
            MediaTypeWithFormat[] formatArray2 = new MediaTypeWithFormat[3];
            MediaTypeWithFormat format = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = new MediaType("application", "atom+xml", new KeyValuePair<string, string>("type", "feed"))
            };
            formatArray2[0] = format;
            MediaTypeWithFormat format2 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationAtomXmlMediaType
            };
            formatArray2[1] = format2;
            MediaTypeWithFormat format3 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonVerboseMediaType
            };
            formatArray2[2] = format3;
            formatArray[0] = formatArray2;
            MediaTypeWithFormat[] formatArray3 = new MediaTypeWithFormat[3];
            MediaTypeWithFormat format4 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = new MediaType("application", "atom+xml", new KeyValuePair<string, string>("type", "entry"))
            };
            formatArray3[0] = format4;
            MediaTypeWithFormat format5 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationAtomXmlMediaType
            };
            formatArray3[1] = format5;
            MediaTypeWithFormat format6 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonVerboseMediaType
            };
            formatArray3[2] = format6;
            formatArray[1] = formatArray3;
            MediaTypeWithFormat[] formatArray4 = new MediaTypeWithFormat[3];
            MediaTypeWithFormat format7 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationXmlMediaType
            };
            formatArray4[0] = format7;
            MediaTypeWithFormat format8 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = TextXmlMediaType
            };
            formatArray4[1] = format8;
            MediaTypeWithFormat format9 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonVerboseMediaType
            };
            formatArray4[2] = format9;
            formatArray[2] = formatArray4;
            MediaTypeWithFormat[] formatArray5 = new MediaTypeWithFormat[3];
            MediaTypeWithFormat format10 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationXmlMediaType
            };
            formatArray5[0] = format10;
            MediaTypeWithFormat format11 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = TextXmlMediaType
            };
            formatArray5[1] = format11;
            MediaTypeWithFormat format12 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonVerboseMediaType
            };
            formatArray5[2] = format12;
            formatArray[3] = formatArray5;
            MediaTypeWithFormat[] formatArray6 = new MediaTypeWithFormat[3];
            MediaTypeWithFormat format13 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationXmlMediaType
            };
            formatArray6[0] = format13;
            MediaTypeWithFormat format14 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = TextXmlMediaType
            };
            formatArray6[1] = format14;
            MediaTypeWithFormat format15 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonVerboseMediaType
            };
            formatArray6[2] = format15;
            formatArray[4] = formatArray6;
            MediaTypeWithFormat[] formatArray7 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format16 = new MediaTypeWithFormat {
                Format = ODataFormat.RawValue,
                MediaType = new MediaType("text", "plain")
            };
            formatArray7[0] = format16;
            formatArray[5] = formatArray7;
            MediaTypeWithFormat[] formatArray8 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format17 = new MediaTypeWithFormat {
                Format = ODataFormat.RawValue,
                MediaType = new MediaType("application", "octet-stream")
            };
            formatArray8[0] = format17;
            formatArray[6] = formatArray8;
            MediaTypeWithFormat[] formatArray9 = new MediaTypeWithFormat[3];
            MediaTypeWithFormat format18 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationXmlMediaType
            };
            formatArray9[0] = format18;
            MediaTypeWithFormat format19 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = TextXmlMediaType
            };
            formatArray9[1] = format19;
            MediaTypeWithFormat format20 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonVerboseMediaType
            };
            formatArray9[2] = format20;
            formatArray[7] = formatArray9;
            MediaTypeWithFormat[] formatArray10 = new MediaTypeWithFormat[3];
            MediaTypeWithFormat format21 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationXmlMediaType
            };
            formatArray10[0] = format21;
            MediaTypeWithFormat format22 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = new MediaType("application", "atomsvc+xml")
            };
            formatArray10[1] = format22;
            MediaTypeWithFormat format23 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonVerboseMediaType
            };
            formatArray10[2] = format23;
            formatArray[8] = formatArray10;
            MediaTypeWithFormat[] formatArray11 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format24 = new MediaTypeWithFormat {
                Format = ODataFormat.Metadata,
                MediaType = ApplicationXmlMediaType
            };
            formatArray11[0] = format24;
            formatArray[9] = formatArray11;
            formatArray2 = new MediaTypeWithFormat[2];
            MediaTypeWithFormat format25 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationXmlMediaType
            };
            formatArray2[0] = format25;
            MediaTypeWithFormat format26 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonVerboseMediaType
            };
            formatArray2[1] = format26;
            formatArray[10] = formatArray2;
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format27 = new MediaTypeWithFormat {
                Format = ODataFormat.Batch,
                MediaType = new MediaType("multipart", "mixed")
            };
            formatArray2[0] = format27;
            formatArray[11] = formatArray2;
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format28 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonVerboseMediaType
            };
            formatArray2[0] = format28;
            formatArray[12] = formatArray2;
            defaultMediaTypes = formatArray;
            Dictionary<ODataPayloadKind, MediaTypeWithFormat[]> dictionary = new Dictionary<ODataPayloadKind, MediaTypeWithFormat[]>();
            formatArray2 = new MediaTypeWithFormat[2];
            MediaTypeWithFormat format29 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = new MediaType("application", "xml", new KeyValuePair<string, string>("type", "feed"))
            };
            formatArray2[0] = format29;
            MediaTypeWithFormat format30 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationXmlMediaType
            };
            formatArray2[1] = format30;
            dictionary.Add(ODataPayloadKind.Feed, formatArray2);
            formatArray2 = new MediaTypeWithFormat[2];
            MediaTypeWithFormat format31 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = new MediaType("application", "xml", new KeyValuePair<string, string>("type", "entry"))
            };
            formatArray2[0] = format31;
            MediaTypeWithFormat format32 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationXmlMediaType
            };
            formatArray2[1] = format32;
            dictionary.Add(ODataPayloadKind.Entry, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format33 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationAtomXmlMediaType
            };
            formatArray2[0] = format33;
            dictionary.Add(ODataPayloadKind.Property, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format34 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationAtomXmlMediaType
            };
            formatArray2[0] = format34;
            dictionary.Add(ODataPayloadKind.EntityReferenceLink, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format35 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationAtomXmlMediaType
            };
            formatArray2[0] = format35;
            dictionary.Add(ODataPayloadKind.EntityReferenceLinks, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format36 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationAtomXmlMediaType
            };
            formatArray2[0] = format36;
            dictionary.Add(ODataPayloadKind.Collection, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format37 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationAtomXmlMediaType
            };
            formatArray2[0] = format37;
            dictionary.Add(ODataPayloadKind.ServiceDocument, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format38 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationAtomXmlMediaType
            };
            formatArray2[0] = format38;
            dictionary.Add(ODataPayloadKind.MetadataDocument, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format39 = new MediaTypeWithFormat {
                Format = ODataFormat.Atom,
                MediaType = ApplicationAtomXmlMediaType
            };
            formatArray2[0] = format39;
            dictionary.Add(ODataPayloadKind.Error, formatArray2);
            additionalClientV2MediaTypes = dictionary;
            Dictionary<ODataPayloadKind, MediaTypeWithFormat[]> dictionary2 = new Dictionary<ODataPayloadKind, MediaTypeWithFormat[]>();
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format40 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonMediaType
            };
            formatArray2[0] = format40;
            dictionary2.Add(ODataPayloadKind.Feed, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format41 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonMediaType
            };
            formatArray2[0] = format41;
            dictionary2.Add(ODataPayloadKind.Entry, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format42 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonMediaType
            };
            formatArray2[0] = format42;
            dictionary2.Add(ODataPayloadKind.Property, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format43 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonMediaType
            };
            formatArray2[0] = format43;
            dictionary2.Add(ODataPayloadKind.EntityReferenceLink, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format44 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonMediaType
            };
            formatArray2[0] = format44;
            dictionary2.Add(ODataPayloadKind.EntityReferenceLinks, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format45 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonMediaType
            };
            formatArray2[0] = format45;
            dictionary2.Add(ODataPayloadKind.Collection, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format46 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonMediaType
            };
            formatArray2[0] = format46;
            dictionary2.Add(ODataPayloadKind.ServiceDocument, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format47 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonMediaType
            };
            formatArray2[0] = format47;
            dictionary2.Add(ODataPayloadKind.Error, formatArray2);
            formatArray2 = new MediaTypeWithFormat[1];
            MediaTypeWithFormat format48 = new MediaTypeWithFormat {
                Format = ODataFormat.VerboseJson,
                MediaType = ApplicationJsonMediaType
            };
            formatArray2[0] = format48;
            dictionary2.Add(ODataPayloadKind.Parameter, formatArray2);
            plainAppJsonMeansVerbose = dictionary2;
        }

        internal MediaTypeResolver(bool shouldPlainAppJsonImplyVerboseJson, bool shouldAppXmlAndAppAtomXmlBeInterchangeable)
        {
            for (int i = 0; i < defaultMediaTypes.Length; i++)
            {
                this.mediaTypesForPayloadKind[i] = new MediaTypeWithFormat[defaultMediaTypes[i].Length];
                defaultMediaTypes[i].CopyTo(this.mediaTypesForPayloadKind[i], 0);
            }
            if (shouldPlainAppJsonImplyVerboseJson)
            {
                this.AddCustomMediaTypes(plainAppJsonMeansVerbose);
            }
            if (shouldAppXmlAndAppAtomXmlBeInterchangeable)
            {
                this.AddCustomMediaTypes(additionalClientV2MediaTypes);
            }
        }

        private void AddCustomMediaTypes(Dictionary<ODataPayloadKind, MediaTypeWithFormat[]> customMediaTypes)
        {
            foreach (KeyValuePair<ODataPayloadKind, MediaTypeWithFormat[]> pair in customMediaTypes)
            {
                MediaTypeWithFormat[] formatArray = pair.Value;
                MediaTypeWithFormat[] formatArray2 = this.mediaTypesForPayloadKind[(int)pair.Key];
                MediaTypeWithFormat[] array = new MediaTypeWithFormat[formatArray2.Length + formatArray.Length];
                formatArray2.CopyTo(array, 0);
                formatArray.CopyTo(array, formatArray2.Length);
                this.mediaTypesForPayloadKind[(int)pair.Key] = array;
            }
        }

        internal MediaTypeWithFormat[] GetMediaTypesForPayloadKind(ODataPayloadKind payloadKind)
        {
            return this.mediaTypesForPayloadKind[(int) payloadKind];
        }

        public static MediaTypeResolver DefaultMediaTypeResolver
        {
            get
            {
                if (defaultMediaTypeResolver == null)
                {
                    defaultMediaTypeResolver = new MediaTypeResolver(false, false);
                }
                return defaultMediaTypeResolver;
            }
        }
    }
}

