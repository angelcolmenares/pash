using Microsoft.ActiveDirectory;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class ValidateMultivalueArrayAttribute : ValidateArgumentsAttribute
	{
		public ValidateMultivalueArrayAttribute()
		{
		}

		protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
		{
			if (arguments == null || arguments == AutomationNull.Value)
			{
				return;
			}
			else
			{
				object[] objArray = arguments as object[];
				if (objArray != null)
				{
					object[] objArray1 = objArray;
					int num = 0;
					while (num < (int)objArray1.Length)
					{
						object obj = objArray1[num];
						if ((obj == null || obj == AutomationNull.Value) && (int)objArray.Length > 1)
						{
							throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidNullValue, new object[0]));
						}
						else
						{
							num++;
						}
					}
					return;
				}
				else
				{
					return;
				}
			}
		}
	}
}