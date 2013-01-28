namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Text;

    internal class ImportCsvHelper
    {
        private bool _alreadyWarnedUnspecifiedName;
        private readonly PSCmdlet _cmdlet;
        private readonly char _delimiter;
        private IList<string> _header;
        private readonly StreamReader _sr;
        private string _typeName;
        private const string UnspecifiedName = "H";

        internal ImportCsvHelper(PSCmdlet cmdlet, char delimiter, IList<string> header, string typeName, StreamReader streamReader)
        {
            this._cmdlet = cmdlet;
            this._delimiter = delimiter;
            this._header = header;
            this._typeName = typeName;
            this._sr = streamReader;
        }

        private PSObject BuildMshobject(string type, IList<string> names, Collection<string> values, char delimiter)
        {
            PSObject obj2 = new PSObject();
            char ch = delimiter;
            int num = 1;
            if ((type != null) && (type.Length > 0))
            {
                obj2.TypeNames.Clear();
                obj2.TypeNames.Add(type);
            }
            for (int i = 0; i <= (names.Count - 1); i++)
            {
                string str = names[i];
                string str2 = null;
                if ((str.Length != 0) || (ch != '"'))
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        str = "H" + num;
                        num++;
                    }
                    if (i < values.Count)
                    {
                        str2 = values[i];
                    }
                    obj2.Properties.Add(new PSNoteProperty(str, str2));
                }
            }
            if (!this._alreadyWarnedUnspecifiedName && (num != 1))
            {
                this._cmdlet.WriteWarning(CsvCommandStrings.UseDefaultNameForUnspecifiedHeader);
                this._alreadyWarnedUnspecifiedName = true;
            }
            return obj2;
        }

        internal void Import(ref bool alreadyWriteOutWarning)
        {
            this._alreadyWarnedUnspecifiedName = alreadyWriteOutWarning;
            this.ReadHeader();
            while (true)
            {
                Collection<string> values = this.ParseNextRecord(false);
                if (values.Count == 0)
                {
                    break;
                }
                PSObject sendToPipeline = this.BuildMshobject(this._typeName, this._header, values, this._delimiter);
                this._cmdlet.WriteObject(sendToPipeline);
            }
            alreadyWriteOutWarning = this._alreadyWarnedUnspecifiedName;
        }

        private bool IsNewLine(char ch)
        {
            bool flag = false;
            if (ch == '\n')
            {
                return true;
            }
            if ((ch == '\r') && this.PeekNextChar('\n'))
            {
                flag = true;
            }
            return flag;
        }

        private Collection<string> ParseNextRecord(bool isHeaderRow)
        {
            Collection<string> collection = new Collection<string>();
            StringBuilder current = new StringBuilder();
            bool flag = false;
            while (!this.EOF)
            {
                char ch = this.ReadChar();
                if (ch == this._delimiter)
                {
                    if (flag)
                    {
                        current.Append(ch);
                    }
                    else
                    {
                        collection.Add(current.ToString());
                        current.Remove(0, current.Length);
                    }
                }
                else
                {
                    if (ch == '"')
                    {
                        if (flag)
                        {
                            if (this.PeekNextChar('"'))
                            {
                                this.ReadChar();
                                current.Append('"');
                                continue;
                            }
                            flag = false;
                            bool endOfRecord = false;
                            this.ReadTillNextDelimiter(current, ref endOfRecord, true);
                            collection.Add(current.ToString());
                            current.Remove(0, current.Length);
                            if (!endOfRecord)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (current.Length == 0)
                            {
                                flag = true;
                                continue;
                            }
                            bool flag3 = false;
                            current.Append(ch);
                            this.ReadTillNextDelimiter(current, ref flag3, false);
                            collection.Add(current.ToString());
                            current.Remove(0, current.Length);
                            if (!flag3)
                            {
                                continue;
                            }
                        }
                        break;
                    }
                    if ((ch == ' ') || (ch == '\t'))
                    {
                        if (flag)
                        {
                            current.Append(ch);
                            continue;
                        }
                        if (current.Length == 0)
                        {
                            continue;
                        }
                        bool flag4 = false;
                        current.Append(ch);
                        this.ReadTillNextDelimiter(current, ref flag4, true);
                        collection.Add(current.ToString());
                        current.Remove(0, current.Length);
                        if (!flag4)
                        {
                            continue;
                        }
                        break;
                    }
                    if (this.IsNewLine(ch))
                    {
                        if (ch == '\r')
                        {
                            this.ReadChar();
                        }
                        if (flag)
                        {
                            current.Append(ch);
                            if (ch == '\r')
                            {
                                current.Append('\n');
                            }
                            continue;
                        }
                        collection.Add(current.ToString());
                        current.Remove(0, current.Length);
                        break;
                    }
                    current.Append(ch);
                }
            }
            if (current.Length != 0)
            {
                collection.Add(current.ToString());
            }
            if (isHeaderRow)
            {
                while ((collection.Count > 1) && collection[collection.Count - 1].Equals(string.Empty))
                {
                    collection.RemoveAt(collection.Count - 1);
                }
            }
            return collection;
        }

        private bool PeekNextChar(char c)
        {
            int num = this._sr.Peek();
            if (num == -1)
            {
                return false;
            }
            return (c == ((char) num));
        }

        private char ReadChar()
        {
            return (char) this._sr.Read();
        }

        internal void ReadHeader()
        {
            if ((this._typeName == null) && !this.EOF)
            {
                this._typeName = this.ReadTypeInformation();
            }
            if ((this._header == null) && !this.EOF)
            {
                Collection<string> collection = this.ParseNextRecord(true);
                if (collection.Count != 0)
                {
                    this._header = collection;
                }
            }
            if ((this._header != null) && (this._header.Count > 0))
            {
                ValidatePropertyNames(this._header);
            }
        }

        private string ReadLine()
        {
            return this._sr.ReadLine();
        }

        private void ReadTillNextDelimiter(StringBuilder current, ref bool endOfRecord, bool eatTrailingBlanks)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            while (true)
            {
                if (this.EOF)
                {
                    endOfRecord = true;
                    break;
                }
                char ch = this.ReadChar();
                if (ch == this._delimiter)
                {
                    break;
                }
                if (this.IsNewLine(ch))
                {
                    endOfRecord = true;
                    if (ch == '\r')
                    {
                        this.ReadChar();
                    }
                    break;
                }
                builder.Append(ch);
                if ((ch != ' ') && (ch != '\t'))
                {
                    flag = true;
                }
            }
            if (eatTrailingBlanks && !flag)
            {
                string str = builder.ToString().Trim();
                current.Append(str);
            }
            else
            {
                current.Append(builder);
            }
        }

        private string ReadTypeInformation()
        {
            string str = null;
            if (!this.PeekNextChar('#'))
            {
                return str;
            }
            string str2 = this.ReadLine();
            if (!str2.StartsWith("#Type", StringComparison.OrdinalIgnoreCase))
            {
                return str;
            }
            str = str2.Substring(5).Trim();
            if (str.Length == 0)
            {
                return null;
            }
            return ("CSV:" + str);
        }

        private static void ValidatePropertyNames(IList<string> names)
        {
            if ((names != null) && (names.Count != 0))
            {
                HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string str in names)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        if (set.Contains(str))
                        {
                            ExtendedTypeSystemException exception = new ExtendedTypeSystemException(string.Format(CultureInfo.InvariantCulture, ExtendedTypeSystem.ResourceManager.GetString("MemberAlreadyPresent"), new object[] { str }));
                            throw exception;
                        }
                        set.Add(str);
                    }
                }
            }
        }

        private bool EOF
        {
            get
            {
                return this._sr.EndOfStream;
            }
        }

        internal IList<string> Header
        {
            get
            {
                return this._header;
            }
        }

        internal string TypeName
        {
            get
            {
                return this._typeName;
            }
        }
    }
}

