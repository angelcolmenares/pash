namespace System.Data.Services.Providers
{
    using System;
    using System.Globalization;

    internal sealed class EpmAttributeNameBuilder
    {
        private int index;
        private string postFix = string.Empty;

        internal EpmAttributeNameBuilder()
        {
        }

        internal void MoveNext()
        {
            this.index++;
            this.postFix = "_" + this.index.ToString(CultureInfo.InvariantCulture);
        }

        internal string EpmContentKind
        {
            get
            {
                return ("FC_ContentKind" + this.postFix);
            }
        }

        internal string EpmKeepInContent
        {
            get
            {
                return ("FC_KeepInContent" + this.postFix);
            }
        }

        internal string EpmNsPrefix
        {
            get
            {
                return ("FC_NsPrefix" + this.postFix);
            }
        }

        internal string EpmNsUri
        {
            get
            {
                return ("FC_NsUri" + this.postFix);
            }
        }

        internal string EpmSourcePath
        {
            get
            {
                return ("FC_SourcePath" + this.postFix);
            }
        }

        internal string EpmTargetPath
        {
            get
            {
                return ("FC_TargetPath" + this.postFix);
            }
        }
    }
}

