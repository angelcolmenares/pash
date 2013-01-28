using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADObject", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219323", SupportsShouldProcess=true)]
	public class NewADObject : ADNewCmdletBase<NewADObjectParameterSet, ADObjectFactory<ADObject>, ADObject>
	{
		public NewADObject()
		{
		}

		protected internal override string GenerateObjectClass(ADObjectFactory<ADObject> factory, ADParameterSet cmdletParameters, NewADObjectParameterSet dynamicParameters)
		{
			return dynamicParameters.Type;
		}

		protected internal override string GenerateRDNPrefix(ADObjectFactory<ADObject> factory, ADParameterSet cmdletParameters, NewADObjectParameterSet dynamicParameters)
		{
			ADSessionInfo sessionInfo = this.GetSessionInfo();
			ADSchemaUtil aDSchemaUtil = new ADSchemaUtil(sessionInfo);
			string rDNPrefix = aDSchemaUtil.GetRDNPrefix(dynamicParameters.Type);
			if (rDNPrefix == null)
			{
				rDNPrefix = factory.RDNPrefix;
			}
			return rDNPrefix;
		}
	}
}