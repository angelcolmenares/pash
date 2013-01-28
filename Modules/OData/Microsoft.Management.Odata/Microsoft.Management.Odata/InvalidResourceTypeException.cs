using Microsoft.Management.Odata.Common;
using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Management.Odata
{
	[Serializable]
	public class InvalidResourceTypeException : PowerShellWebServiceException
	{
		public string ActualResourceName
		{
			get;
			private set;
		}

		public string ActualResourceType
		{
			get;
			private set;
		}

		public string ExpectedResourceType
		{
			get;
			private set;
		}

		public InvalidResourceTypeException()
		{
			base.Trace();
		}

		public InvalidResourceTypeException(string actualResourceName, string actualResourceType, string expectedResourceType)
			: base(string.Format(CultureInfo.CurrentCulture, Resources.ResourceTypeIsInvalid, new object[] { actualResourceName, actualResourceType, expectedResourceType }))
		{
			TraceHelper.Current.WrongResourceTypeUsedException(actualResourceName, actualResourceName, expectedResourceType);
			this.ActualResourceName = actualResourceName;
			this.ActualResourceType = actualResourceType;
			this.ExpectedResourceType = expectedResourceType;
			base.Trace();
		}

		public InvalidResourceTypeException(string message) : base(message)
		{
			base.Trace();
		}

		public InvalidResourceTypeException(string message, Exception innerException) : base(message, innerException)
		{
			base.Trace();
		}

		protected InvalidResourceTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info != null)
			{
				this.ActualResourceName = info.GetString("ActualResourceName");
				this.ActualResourceType = info.GetString("ActualResourceType");
				this.ExpectedResourceType = info.GetString("ExpectedResourceType");
			}
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			if (info != null)
			{
				info.AddValue("ActualResourceName", this.ActualResourceName);
				info.AddValue("ActualResourceType", this.ActualResourceType);
				info.AddValue("ExpectedResourceType", this.ExpectedResourceType);
			}
		}

		protected override StringBuilder ToTraceMessage(StringBuilder builder)
		{
			builder = base.ToTraceMessage(builder);
			builder.AppendLine(string.Concat("Actual Resource Name = ", this.ActualResourceName));
			builder.AppendLine(string.Concat("Actual Resource Type = ", this.ActualResourceType));
			builder.AppendLine(string.Concat("Expected Resource Type = ", this.ExpectedResourceType));
			return builder;
		}
	}
}