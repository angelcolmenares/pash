using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class TransformFilterAttribute : ArgumentTransformationAttribute
	{
		public TransformFilterAttribute()
		{

		}


		public string AttributeName
		{
			get;set;
		}

		public override object Transform (EngineIntrinsics engineIntrinsics, object inputData)
		{
			PSObject pSObject = inputData as PSObject;
			if (pSObject != null) {
				inputData = pSObject.BaseObject;
			}
			string attributeName = AttributeName;
			if (string.IsNullOrEmpty (attributeName)) {
				attributeName = "objectClass";
			}
			string str = inputData as string;
			if (str != "*")
			{
				return inputData;
			}
			else
			{
				return attributeName + " -like \"*\"";
			}
		}
	}
}