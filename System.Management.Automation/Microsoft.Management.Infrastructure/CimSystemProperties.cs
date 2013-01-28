namespace Microsoft.Management.Infrastructure
{
    using System;

    public class CimSystemProperties
    {
        private string _className;
        private string _namespace;
        private string _path;
        private string _serverName;

        internal CimSystemProperties()
        {

        }

        internal void UpdateCimSystemProperties(string systemNamespace, string serverName, string className)
        {
            this._namespace = systemNamespace;
            this._serverName = serverName;
            this._className = className;
        }

        internal void UpdateSystemPath(string Path)
        {
            this._path = Path;
        }

        public string ClassName
        {
            get
            {
                return this._className;
            }
        }

        public string Namespace
        {
            get
            {
                return this._namespace;
            }
        }

        public string Path
        {
            get
            {
                return this._path;
            }
        }

        public string ServerName
        {
            get
            {
                return this._serverName;
            }
        }

		public override string ToString ()
		{
			return string.Format ("[ClassName={0}, Namespace={1}, Path={2}, ServerName={3}]", ClassName, Namespace, Path, ServerName);
		}

    }
}

