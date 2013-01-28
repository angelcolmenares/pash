namespace Microsoft.Data.OData.Metadata
{
    using System;
    using System.Globalization;

    internal sealed class EpmAttributeNameBuilder
    {
        private int index;
        private const string Separator = "_";
        private string suffix = string.Empty;

        internal EpmAttributeNameBuilder()
        {
        }

        internal void MoveNext()
        {
            this.index++;
            this.suffix = "_" + this.index.ToString(CultureInfo.InvariantCulture);
        }

        internal string EpmContentKind
        {
            get
            {
                return ("FC_ContentKind" + this.suffix);
            }
        }

        internal string EpmKeepInContent
        {
            get
            {
                return ("FC_KeepInContent" + this.suffix);
            }
        }

        internal string EpmNsPrefix
        {
            get
            {
                return ("FC_NsPrefix" + this.suffix);
            }
        }

        internal string EpmNsUri
        {
            get
            {
                return ("FC_NsUri" + this.suffix);
            }
        }

        internal string EpmSourcePath
        {
            get
            {
                return ("FC_SourcePath" + this.suffix);
            }
        }

        internal string EpmTargetPath
        {
            get
            {
                return ("FC_TargetPath" + this.suffix);
            }
        }
    }
}

