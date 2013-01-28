using Microsoft.Management.Infrastructure;
using System;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class FormatPartialCimInstance : IObjectPreProcess
	{
		internal const string PartialPSTypeName = "Microsoft.Management.Infrastructure.CimInstance#__PartialCIMInstance";

		public FormatPartialCimInstance()
		{
		}

		public object Process(object resultObject)
		{
			if (resultObject as CimInstance == null)
			{
				return resultObject;
			}
			else
			{
				PSObject pSObject = PSObject.AsPSObject(resultObject);
				pSObject.TypeNames.Insert(0, "Microsoft.Management.Infrastructure.CimInstance#__PartialCIMInstance");
				return pSObject;
			}
		}
	}
}