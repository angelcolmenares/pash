namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation.Internal;
    using System.Xml;

    public sealed class SelectXmlInfo
    {
        private const string inputStream = "InputStream";
        private const string MatchFormat = "{0}:{1}";
        private XmlNode node;
        private string path;
        private string pattern;
        private const string SimpleFormat = "{0}";

        private string FormatLine(string text, string displaypath)
        {
            if (this.path.Equals("InputStream"))
            {
                return StringUtil.Format("{0}", text);
            }
            return StringUtil.Format("{0}:{1}", text, displaypath);
        }

        internal string GetNodeText()
        {
            string str = string.Empty;
            if (this.node == null)
            {
                return str;
            }
            if (this.node.Value != null)
            {
                return this.node.Value.Trim();
            }
            return this.node.InnerXml.Trim();
        }

        private string RelativePath(string directory)
        {
            string path = this.path;
            if (path.Equals("InputStream") || !path.StartsWith(directory, StringComparison.CurrentCultureIgnoreCase))
            {
                return path;
            }
            int length = directory.Length;
            if (length >= path.Length)
            {
                return path;
            }
            if ((directory[length - 1] == '\\') || (directory[length - 1] == '/'))
            {
                return path.Substring(length);
            }
            if ((path[length] != '\\') && (path[length] != '/'))
            {
                return path;
            }
            return path.Substring(length + 1);
        }

        public override string ToString()
        {
            return this.ToString(null);
        }

        private string ToString(string directory)
        {
            string displaypath = (directory != null) ? this.RelativePath(directory) : this.path;
            return this.FormatLine(this.GetNodeText(), displaypath);
        }

        public XmlNode Node
        {
            get
            {
                return this.node;
            }
            set
            {
                this.node = value;
            }
        }

        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.path = "InputStream";
                }
                else
                {
                    this.path = value;
                }
            }
        }

        public string Pattern
        {
            get
            {
                return this.pattern;
            }
            set
            {
                this.pattern = value;
            }
        }
    }
}

