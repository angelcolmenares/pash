using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimGetCimClassContext : XOperationContextBase
	{
		private string className;

		private string methodName;

		private string propertyName;

		private string qualifierName;

		public string ClassName
		{
			get
			{
				return this.className;
			}
			set
			{
				this.className = value;
			}
		}

		internal string MethodName
		{
			get
			{
				return this.methodName;
			}
		}

		internal string PropertyName
		{
			get
			{
				return this.propertyName;
			}
		}

		internal string QualifierName
		{
			get
			{
				return this.qualifierName;
			}
		}

		internal CimGetCimClassContext(string theClassName, string theMethodName, string thePropertyName, string theQualifierName)
		{
			this.className = theClassName;
			this.methodName = theMethodName;
			this.propertyName = thePropertyName;
			this.qualifierName = theQualifierName;
		}
	}
}