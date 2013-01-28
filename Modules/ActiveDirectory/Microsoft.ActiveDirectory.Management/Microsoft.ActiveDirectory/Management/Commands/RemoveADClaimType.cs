using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADClaimType", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216389", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADClaimType : ADRemoveCmdletBase<RemoveADClaimTypeParameterSet, ADClaimTypeFactory<ADClaimType>, ADClaimType>
	{
		public RemoveADClaimType()
		{
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.RemoveADClaimTypeCmdletValidationCSRoutine));
		}

		protected internal override string GetDefaultPartitionPath()
		{
			string str = ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			return ADPathModule.MakePath(str, "CN=Claim Types,", ADPathFormat.X500);
		}

		private bool RemoveADClaimTypeCmdletValidationCSRoutine()
		{
			string[] strArrays;
			SwitchParameter force = this._cmdletParameters.Force;
			if (!force.ToBool())
			{
				ADClaimType identity = this._cmdletParameters.Identity;
				object attributeValueFromObjectName = AttributeConverters.GetAttributeValueFromObjectName<ADClaimTypeFactory<ADClaimType>, ADClaimType>(identity, this.GetDefaultPartitionPath(), "msDS-ClaimSharesPossibleValuesWithBL", "msDS-ClaimSharesPossibleValuesWithBL", this.GetCmdletSessionInfo());
				if (attributeValueFromObjectName != null)
				{
					string str = attributeValueFromObjectName as string;
					if (str == null)
					{
						strArrays = attributeValueFromObjectName as string[];
					}
					else
					{
						string[] strArrays1 = new string[1];
						strArrays1[0] = str;
						strArrays = strArrays1;
					}
					object[] identifyingString = new object[1];
					identifyingString[0] = identity.IdentifyingString;
					string str1 = string.Format(CultureInfo.CurrentCulture, StringResources.RemoveClaimTypeSharesValueWithError, identifyingString);
					if (strArrays != null)
					{
						for (int i = 0; i < 5 && i < (int)strArrays.Length; i++)
						{
							str1 = string.Concat(str1, Environment.NewLine, strArrays[i]);
						}
					}
					base.WriteError(new ErrorRecord(new ADException(str1), "RemoveADClaimType:RemoveADClaimTypeCmdletValidationCSRoutine", ErrorCategory.InvalidData, null));
					return false;
				}
			}
			return true;
		}
	}
}