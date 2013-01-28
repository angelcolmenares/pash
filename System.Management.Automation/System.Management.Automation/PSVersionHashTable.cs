namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Remoting.Client;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class PSVersionHashTable : Hashtable, IEnumerable
    {
        private bool shouldInitializeWSManStackVersion;
        private object syncObject;

        public PSVersionHashTable(IDictionary listToInitialize) : base(listToInitialize)
        {
            this.syncObject = new object();
            this.shouldInitializeWSManStackVersion = true;
        }

        private void AssignWSManStackVersion()
        {
            if (this.shouldInitializeWSManStackVersion)
            {
                lock (this.syncObject)
                {
                    if (this.shouldInitializeWSManStackVersion)
                    {
                        this.shouldInitializeWSManStackVersion = false;
                        this["WSManStackVersion"] = this.GetWSManStackVersion();
                    }
                }
            }
        }

        public override object Clone()
        {
            this.AssignWSManStackVersion();
            return base.Clone();
        }

        public override bool ContainsValue(object value)
        {
            this.AssignWSManStackVersion();
            return base.ContainsValue(value);
        }

        public override void CopyTo(Array array, int arrayIndex)
        {
            this.AssignWSManStackVersion();
            base.CopyTo(array, arrayIndex);
        }

        public override bool Equals(object obj)
        {
            this.AssignWSManStackVersion();
            return base.Equals(obj);
        }

        public override IDictionaryEnumerator GetEnumerator()
        {
            this.AssignWSManStackVersion();
            return base.GetEnumerator();
        }

        public override int GetHashCode()
        {
            this.AssignWSManStackVersion();
            return base.GetHashCode();
        }

        private Version GetWSManStackVersion()
        {
            Version version = WSManNativeApi.WSMAN_STACK_VERSION;
            try
            {
                IWSManEx ex = (IWSManEx) new WSManClass();
                int flags = 0x8000;
                object connectionOptions = ex.CreateConnectionOptions();
				if (connectionOptions != null) {;
	                string str = ((IWSManSession) ex.CreateSession(null, flags, connectionOptions)).Identify(0);
	                if (!string.IsNullOrEmpty(str))
	                {
	                    XmlDocument document = new XmlDocument();
	                    document.LoadXml(str);
	                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
	                    nsmgr.AddNamespace("wsmid", "http://schemas.dmtf.org/wbem/wsman/identity/1/wsmanidentity.xsd");
	                    System.Xml.XmlNode node = document.SelectSingleNode("/wsmid:IdentifyResponse/wsmid:ProductVersion", nsmgr);
	                    if (node != null)
	                    {
	                        string innerText = node.InnerText;
	                        version = new Version(innerText.Substring(innerText.IndexOf("Stack:", StringComparison.OrdinalIgnoreCase) + 6).Trim());
	                    }
	                }
				}
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            return version;
        }

        public override void OnDeserialization(object sender)
        {
            this.AssignWSManStackVersion();
            base.OnDeserialization(sender);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            this.AssignWSManStackVersion();
            return base.GetEnumerator();
        }

        public override string ToString()
        {
            this.AssignWSManStackVersion();
            return base.ToString();
        }

        public override int Count
        {
            get
            {
                this.AssignWSManStackVersion();
                return base.Count;
            }
        }

        public override object this[object key]
        {
            get
            {
                if (string.Equals("WSManStackVersion", key as string, StringComparison.OrdinalIgnoreCase))
                {
                    this.AssignWSManStackVersion();
                }
                return base[key];
            }
            set
            {
                base[key] = value;
            }
        }

        public override ICollection Keys
        {
            get
            {
                this.AssignWSManStackVersion();
                return base.Keys;
            }
        }

        public override ICollection Values
        {
            get
            {
                this.AssignWSManStackVersion();
                return base.Values;
            }
        }

        //[ComImport, Guid("2D53BDAA-798E-49E6-A1AA-74D01256F411"), TypeLibType((short) 0x10d0), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        private interface IWSManEx
        {
            [return: MarshalAs(UnmanagedType.IDispatch)]
            [DispId(1)]
            object CreateSession([MarshalAs(UnmanagedType.BStr)] string connection, int flags, [MarshalAs(UnmanagedType.IDispatch)] object connectionOptions);
            [return: MarshalAs(UnmanagedType.IDispatch)]
            [DispId(2)]
            object CreateConnectionOptions();
        }

        //[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("FC84FC58-1286-40C4-9DA0-C8EF6EC241E0"), TypeLibType((short) 0x10c0)]
        private interface IWSManSession
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            [DispId(1)]
            string Get(object resourceUri, int flags);
            [return: MarshalAs(UnmanagedType.BStr)]
            [DispId(2)]
            string Put(object resourceUri, [MarshalAs(UnmanagedType.BStr)] string resource, int flags);
            [return: MarshalAs(UnmanagedType.BStr)]
            [DispId(3)]
            string Create(object resourceUri, [MarshalAs(UnmanagedType.BStr)] string resource, int flags);
            [DispId(4)]
            void Delete(object resourceUri, int flags);
            [DispId(5)]
            string Invoke([MarshalAs(UnmanagedType.BStr)] string actionURI, [In] object resourceUri, [MarshalAs(UnmanagedType.BStr)] string parameters, [In] int flags);
            [return: MarshalAs(UnmanagedType.IDispatch)]
            [DispId(6)]
            object Enumerate(object resourceUri, [MarshalAs(UnmanagedType.BStr)] string filter, [MarshalAs(UnmanagedType.BStr)] string dialect, int flags);
            [return: MarshalAs(UnmanagedType.BStr)]
            [DispId(7)]
            string Identify(int flags);
        }

        //[ComImport, ClassInterface(ClassInterfaceType.AutoDual), Guid("BCED617B-EC03-420b-8508-977DC7A686BD")]
		private class WSManClass : IWSManEx
        {
			#region IWSManEx implementation
			public object CreateSession (string connection, int flags, object connectionOptions)
			{
				return null;
			}
			public object CreateConnectionOptions ()
			{
				return null;
			}
			#endregion
        }
    }
}

