namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
	using System.Linq;
    using System.Text;

    [DebuggerDisplay("MediaType [{type}/{subType}]")]
    internal sealed class MediaType
    {
        private readonly IList<KeyValuePair<string, string>> parameters;
        private readonly string subType;
        private readonly string type;

        internal MediaType(string type, string subType) : this(type, subType, (IList<KeyValuePair<string, string>>) null)
        {
        }

        internal MediaType(string type, string subType, IList<KeyValuePair<string, string>> parameters)
        {
            this.type = type;
            this.subType = subType;
            this.parameters = parameters;
        }

		internal MediaType(string type, string subType, KeyValuePair<string, string> parameter) : this(type, subType, new KeyValuePair<string, string>[] { parameter })
		{
        }

        private static Encoding EncodingFromName(string name)
        {
            Encoding encodingFromCharsetName = HttpUtils.GetEncodingFromCharsetName(name);
            if (encodingFromCharsetName == null)
            {
                throw new ODataException(Strings.MediaType_EncodingNotSupported(name));
            }
            return encodingFromCharsetName;
        }

        internal Encoding SelectEncoding()
        {
            if (this.parameters != null)
            {
                foreach (string str in from parameter in this.parameters
                    where HttpUtils.CompareMediaTypeParameterNames("charset", parameter.Key)
                    select parameter.Value.Trim() into encodingName
                    where encodingName.Length > 0
                    select encodingName)
                {
                    return EncodingFromName(str);
                }
            }
            if (HttpUtils.CompareMediaTypeNames("text", this.type))
            {
                if (!HttpUtils.CompareMediaTypeNames("xml", this.subType))
                {
                    return MissingEncoding;
                }
                return null;
            }
            if (HttpUtils.CompareMediaTypeNames("application", this.type) && HttpUtils.CompareMediaTypeNames("json", this.subType))
            {
                return FallbackEncoding;
            }
            return null;
        }

        internal static Encoding FallbackEncoding
        {
            get
            {
                return MediaTypeUtils.EncodingUtf8NoPreamble;
            }
        }

        internal string FullTypeName
        {
            get
            {
                return (this.type + "/" + this.subType);
            }
        }

        internal static Encoding MissingEncoding
        {
            get
            {
                return Encoding.GetEncoding("ISO-8859-1", new EncoderExceptionFallback(), new DecoderExceptionFallback());
            }
        }

        internal IList<KeyValuePair<string, string>> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        internal string SubTypeName
        {
            get
            {
                return this.subType;
            }
        }

        internal string TypeName
        {
            get
            {
                return this.type;
            }
        }
    }
}

