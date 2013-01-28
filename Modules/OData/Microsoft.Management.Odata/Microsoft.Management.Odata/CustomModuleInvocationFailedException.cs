using Microsoft.Management.Odata.Common;
using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Management.Odata
{
	[Serializable]
	public class CustomModuleInvocationFailedException : PowerShellWebServiceException
	{
		public string MethodName
		{
			get;
			private set;
		}

		public string ModuleName
		{
			get;
			private set;
		}

		public CustomModuleInvocationFailedException()
		{
			base.Trace();
		}

		public CustomModuleInvocationFailedException(string moduleName, string methodName, Exception innerException)

			: base(string.Format(CultureInfo.CurrentCulture, Resources.CustomModuleMethodInvocationFailed, new object[] {  methodName, moduleName, innerException == null ? string.Empty : innerException.Message }), innerException)
		{
			TraceHelper.Current.CustomModuleInvocationFailedException(moduleName, methodName, innerException.ToTraceMessage("Exception"));
			this.ModuleName = moduleName;
			this.MethodName = methodName;
			base.Trace();
		}

		public CustomModuleInvocationFailedException(string message) : base(message)
		{
			base.Trace();
		}

		public CustomModuleInvocationFailedException(string message, Exception innerException) : base(message, innerException)
		{
			base.Trace();
		}

		protected CustomModuleInvocationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info != null)
			{
				this.ModuleName = info.GetString("ModuleName");
				this.MethodName = info.GetString("MethodName");
			}
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			if (info != null)
			{
				info.AddValue("ModuleName", this.ModuleName);
				info.AddValue("MethodName", this.MethodName);
			}
		}

		protected override StringBuilder ToTraceMessage(StringBuilder builder)
		{
			builder = base.ToTraceMessage(builder);
			builder.AppendLine(string.Concat("Module Name = ", this.ModuleName));
			builder.AppendLine(string.Concat("Method Name = ", this.MethodName));
			return builder;
		}
	}
}