using Microsoft.Management.Odata.Common;
using System;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Management.Odata
{
	[Serializable]
	public class TypeLoadException : PowerShellWebServiceException
	{
		public string ApplicationBase
		{
			get;
			private set;
		}

		public string AssemblyName
		{
			get;
			private set;
		}

		public string TypeName
		{
			get;
			private set;
		}

		public TypeLoadException()
		{
			base.Trace();
		}

		public TypeLoadException(string typeName, string assemblyName, string applicationBase, Exception innerException) : base((innerException != null ? innerException.Message : string.Empty), innerException)
		{
			TraceHelper.Current.TypeLoadException(typeName, assemblyName, applicationBase);
			this.TypeName = typeName;
			this.AssemblyName = assemblyName;
			this.ApplicationBase = applicationBase;
			base.Trace();
		}

		public TypeLoadException(string message) : base(message)
		{
			base.Trace();
		}

		public TypeLoadException(string message, Exception innerException) : base(message, innerException)
		{
			base.Trace();
		}

		protected TypeLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info != null)
			{
				this.TypeName = info.GetString("TypeName");
				this.AssemblyName = info.GetString("AssemblyName");
				this.ApplicationBase = info.GetString("ApplicationBase");
			}
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			if (info != null)
			{
				info.AddValue("TypeName", this.TypeName);
				info.AddValue("AssemblyName", this.AssemblyName);
				info.AddValue("ApplicationBase", this.ApplicationBase);
			}
		}

		protected override StringBuilder ToTraceMessage(StringBuilder builder)
		{
			builder = base.ToTraceMessage(builder);
			builder.AppendLine(string.Concat("TypeName = ", this.TypeName));
			builder.AppendLine(string.Concat("Assembly Name = ", this.AssemblyName));
			builder.AppendLine(string.Concat("Application base = ", this.ApplicationBase));
			return builder;
		}
	}
}