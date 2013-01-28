namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Text.RegularExpressions;

    public class MatchInfo
    {
        private MatchInfoContext context;
        private const string ContextPrefix = "  ";
        private const string EmptyPrefix = "";
        private string filename;
        private bool ignoreCase;
        private static string inputStream = "InputStream";
        private string line = "";
        private int lineNumber;
        private Match[] matches = new Match[0];
        private const string MatchFormat = "{0}{1}:{2}:{3}";
        private const string MatchPrefix = "> ";
        private string path = inputStream;
        private bool pathSet;
        private string pattern;
        private const string SimpleFormat = "{0}{1}";

        internal MatchInfo Clone()
        {
            MatchInfo info = (MatchInfo) base.MemberwiseClone();
            if (info.Context != null)
            {
                info.Context = (MatchInfoContext) info.Context.Clone();
            }
            info.Matches = (Match[]) info.Matches.Clone();
            return info;
        }

        private string FormatLine(string lineStr, int displayLineNumber, string displayPath, string prefix)
        {
            if (this.pathSet)
            {
                return StringUtil.Format("{0}{1}:{2}:{3}", new object[] { prefix, displayPath, displayLineNumber, lineStr });
            }
            return StringUtil.Format("{0}{1}", prefix, lineStr);
        }

        public string RelativePath(string directory)
        {
            if (!this.pathSet)
            {
                return this.Path;
            }
            string path = this.path;
            if (string.IsNullOrEmpty(directory) || !path.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
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

        public string ToString(string directory)
        {
            string displayPath = (directory != null) ? this.RelativePath(directory) : this.path;
            if (this.Context == null)
            {
                return this.FormatLine(this.line, this.LineNumber, displayPath, "");
            }
            List<string> list = new List<string>((this.Context.DisplayPreContext.Length + this.Context.DisplayPostContext.Length) + 1);
            int num = this.LineNumber - this.Context.DisplayPreContext.Length;
            foreach (string str2 in this.Context.DisplayPreContext)
            {
                list.Add(this.FormatLine(str2, num++, displayPath, "  "));
            }
            list.Add(this.FormatLine(this.line, num++, displayPath, "> "));
            foreach (string str3 in this.Context.DisplayPostContext)
            {
                list.Add(this.FormatLine(str3, num++, displayPath, "  "));
            }
            return string.Join(Environment.NewLine, list.ToArray());
        }

        public MatchInfoContext Context
        {
            get
            {
                return this.context;
            }
            set
            {
                this.context = value;
            }
        }

        public string Filename
        {
            get
            {
                if (!this.pathSet)
                {
                    return inputStream;
                }
                if (this.filename == null)
                {
                    this.filename = System.IO.Path.GetFileName(this.path);
                }
                return this.filename;
            }
        }

        public bool IgnoreCase
        {
            get
            {
                return this.ignoreCase;
            }
            set
            {
                this.ignoreCase = value;
            }
        }

        public string Line
        {
            get
            {
                return this.line;
            }
            set
            {
                this.line = value;
            }
        }

        public int LineNumber
        {
            get
            {
                return this.lineNumber;
            }
            set
            {
                this.lineNumber = value;
            }
        }

        public Match[] Matches
        {
            get
            {
                return this.matches;
            }
            set
            {
                this.matches = value;
            }
        }

        public string Path
        {
            get
            {
                if (!this.pathSet)
                {
                    return inputStream;
                }
                return this.path;
            }
            set
            {
                this.path = value;
                this.pathSet = true;
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

