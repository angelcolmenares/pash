namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;

    internal class CSVHelper
    {
        private char delimiter = ',';

        internal CSVHelper(char delimiter)
        {
            this.delimiter = delimiter;
        }

        internal Collection<string> ParseCsv(string csv)
        {
            Collection<string> collection = new Collection<string>();
            string item = "";
            csv = csv.Trim();
            if ((csv.Length != 0) && (csv[0] != '#'))
            {
                bool flag = false;
                for (int i = 0; i < csv.Length; i++)
                {
                    char ch = csv[i];
                    if (ch == this.Delimiter)
                    {
                        if (!flag)
                        {
                            collection.Add(item);
                            item = "";
                        }
                        else
                        {
                            item = item + ch;
                        }
                    }
                    else if (ch == '"')
                    {
                        if (flag)
                        {
                            if (i == (csv.Length - 1))
                            {
                                collection.Add(item);
                                item = "";
                                flag = false;
                            }
                            else if (csv[i + 1] == this.Delimiter)
                            {
                                collection.Add(item);
                                item = "";
                                flag = false;
                                i++;
                            }
                            else if (csv[i + 1] == '"')
                            {
                                item = item + '"';
                                i++;
                            }
                            else
                            {
                                flag = false;
                            }
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    else
                    {
                        item = item + ch;
                    }
                }
                if (item.Length > 0)
                {
                    collection.Add(item);
                }
            }
            return collection;
        }

        internal char Delimiter
        {
            get
            {
                return this.delimiter;
            }
        }
    }
}

