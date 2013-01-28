using Microsoft.Management.Odata.Common;
using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Management.Odata
{
	[Serializable]
	public class ResourcePropertyNotFoundException : PowerShellWebServiceException
	{
		public string PropertyName
		{
			get;
			private set;
		}

		public string ResourceTypeName
		{
			get;
			private set;
		}

		public ResourcePropertyNotFoundException()
		{
			base.Trace();
		}

		public ResourcePropertyNotFoundException(string resourceTypeName, string propertyName)
			: base(string.Format(CultureInfo.CurrentCulture, Resources.PropertyNotFoundInODataResource, new object[] { propertyName, resourceTypeName }))
		{
			TraceHelper.Current.ResourcePropertyNotFoundException(resourceTypeName, propertyName);
			this.ResourceTypeName = resourceTypeName;
			this.PropertyName = propertyName;
			base.Trace();
		}

		public ResourcePropertyNotFoundException(string message) : base(message)
		{
			base.Trace();
		}

		public ResourcePropertyNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
			base.Trace();
		}

		protected ResourcePropertyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info != null)
			{
				this.ResourceTypeName = info.GetString("ResourceTypeName");
				this.PropertyName = info.GetString("PropertyName");
			}
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			if (info != null)
			{
				info.AddValue("ResourceTypeName", this.ResourceTypeName);
				info.AddValue("PropertyName", this.PropertyName);
			}
		}

		protected override StringBuilder ToTraceMessage(StringBuilder builder)
		{
			builder = base.ToTraceMessage(builder);
			builder.AppendLine(string.Concat("Resource Type Name = ", this.ResourceTypeName));
			builder.AppendLine(string.Concat("Property Name = ", this.PropertyName));
			return builder;
		}
	}
}