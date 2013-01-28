using System;
using System.Globalization;
using System.IO;
using System.Management.Automation.Tracing;
using System.Xml.Linq;

namespace Microsoft.PowerShell.Workflow
{
	internal class PersistenceVersion
	{
		private readonly PowerShellTraceSource Tracer;

		private bool saved;

		private object syncLock;

		internal Version CLRVersion
		{
			get;
			set;
		}

		internal bool EnableCompression
		{
			get;
			set;
		}

		internal bool EnableEncryption
		{
			get;
			set;
		}

		internal Version StoreVersion
		{
			get;
			set;
		}

		internal PersistenceVersion(bool enableEncryption, bool enableCompression)
		{
			this.Tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this.syncLock = new object();
			this.StoreVersion = new Version(1, 0);
			this.CLRVersion = Environment.Version;
			this.EnableEncryption = enableEncryption;
			this.EnableCompression = enableCompression;
		}

		internal void load(string versionFileName)
		{
			try
			{
				if (File.Exists(versionFileName))
				{
					XElement xElement = XElement.Load(versionFileName);
					if (xElement.Name.LocalName.Equals("PersistenceVersion", StringComparison.OrdinalIgnoreCase))
					{
						foreach (XElement xElement1 in xElement.Elements())
						{
							if (!xElement1.Name.LocalName.Equals("StoreVersion", StringComparison.OrdinalIgnoreCase))
							{
								if (!xElement1.Name.LocalName.Equals("CLRVersion", StringComparison.OrdinalIgnoreCase))
								{
									if (!xElement1.Name.LocalName.Equals("EnableEncryption", StringComparison.OrdinalIgnoreCase))
									{
										if (!xElement1.Name.LocalName.Equals("EnableCompression", StringComparison.OrdinalIgnoreCase))
										{
											continue;
										}
										this.EnableCompression = Convert.ToBoolean(xElement1.Value, CultureInfo.InvariantCulture);
									}
									else
									{
										this.EnableEncryption = Convert.ToBoolean(xElement1.Value, CultureInfo.InvariantCulture);
									}
								}
								else
								{
									this.CLRVersion = new Version(xElement1.Value);
								}
							}
							else
							{
								this.StoreVersion = new Version(xElement1.Value);
							}
						}
					}
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				object[] objArray = new object[1];
				objArray[0] = versionFileName;
				this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "Exception while reading or parsing the version file: {0}", objArray));
				this.Tracer.TraceException(exception);
			}
		}

		internal void save(string versionFileName)
		{
			if (!this.saved)
			{
				lock (this.syncLock)
				{
					if (!this.saved)
					{
						this.saved = true;
						if (!File.Exists(versionFileName))
						{
							object[] xElement = new object[4];
							xElement[0] = new XElement("StoreVersion", this.StoreVersion);
							xElement[1] = new XElement("CLRVersion", this.CLRVersion);
							xElement[2] = new XElement("EnableEncryption", (object)this.EnableEncryption);
							xElement[3] = new XElement("EnableCompression", (object)this.EnableCompression);
							XElement xElement1 = new XElement("PersistenceVersion", xElement);
							xElement1.Save(versionFileName);
						}
					}
				}
				return;
			}
			else
			{
				return;
			}
		}
	}
}