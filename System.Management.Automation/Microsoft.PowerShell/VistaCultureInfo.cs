namespace Microsoft.PowerShell
{
    using System;
    using System.Globalization;

    internal class VistaCultureInfo : CultureInfo
    {
        private string[] m_fallbacks;
        private VistaCultureInfo parentCI;
        private object syncObject;

        public VistaCultureInfo(string name, string[] fallbacks) : base(name)
        {
            this.syncObject = new object();
            this.m_fallbacks = fallbacks;
        }

        public override object Clone()
        {
            return new VistaCultureInfo(base.Name, this.m_fallbacks);
        }

        private VistaCultureInfo ImmediateParent
        {
            get
            {
                if (this.parentCI == null)
                {
                    lock (this.syncObject)
                    {
                        if (this.parentCI == null)
                        {
                            string name = base.Parent.Name;
                            string[] array = null;
                            if (this.m_fallbacks != null)
                            {
                                array = new string[this.m_fallbacks.Length];
                                int index = 0;
                                foreach (string str2 in this.m_fallbacks)
                                {
                                    if (!name.Equals(str2, StringComparison.OrdinalIgnoreCase))
                                    {
                                        array[index] = str2;
                                        index++;
                                    }
                                }
                                if (this.m_fallbacks.Length != index)
                                {
                                    Array.Resize<string>(ref array, index);
                                }
                            }
                            this.parentCI = new VistaCultureInfo(name, array);
                        }
                    }
                }
                return this.parentCI;
            }
        }

        public override CultureInfo Parent
        {
            get
            {
                if ((base.Parent == null) || string.IsNullOrEmpty(base.Parent.Name))
                {
                    while ((this.m_fallbacks != null) && (this.m_fallbacks.Length > 0))
                    {
                        string name = this.m_fallbacks[0];
                        string[] destinationArray = null;
                        if (this.m_fallbacks.Length > 1)
                        {
                            destinationArray = new string[this.m_fallbacks.Length - 1];
                            Array.Copy(this.m_fallbacks, 1, destinationArray, 0, this.m_fallbacks.Length - 1);
                        }
                        try
                        {
                            return new VistaCultureInfo(name, destinationArray);
                        }
                        catch (ArgumentException)
                        {
                            this.m_fallbacks = destinationArray;
                        }
                    }
                    return base.Parent;
                }
                return this.ImmediateParent;
            }
        }
    }
}

