using Microsoft.Management.Odata.Common;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Management.Odata
{
	[Serializable]
	public class InvalidSchemaException : PowerShellWebServiceException
	{
		public string SchemaFileName
		{
			get;
			set;
		}

		public InvalidSchemaException()
		{
			base.Trace();
		}

		public InvalidSchemaException(string message) : base(message)
		{
			TraceHelper.Current.InvalidSchemaException(message);
		}

		public InvalidSchemaException(string message, Exception innerException) : base(message, innerException)
		{
			TraceHelper.Current.InvalidSchemaException(message);
			base.Trace();
		}

		public InvalidSchemaException(string schemaFileName, string message, Exception innerException) : base(message, innerException)
		{
			TraceHelper.Current.InvalidSchemaException(message);
			this.SchemaFileName = schemaFileName;
			base.Trace();
		}

		protected InvalidSchemaException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			base.Trace();
			if (info != null)
			{
				this.SchemaFileName = info.GetString("SchemaFileName");
			}
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			if (info != null)
			{
				info.AddValue("SchemaFileName", this.SchemaFileName);
			}
		}
	}
}