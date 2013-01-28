using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class ValidateNotNullOrEmptyADEntityAttribute : ValidateArgumentsAttribute
	{
		public ValidateNotNullOrEmptyADEntityAttribute()
		{
		}

		protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
		{
			if (arguments == null || arguments == AutomationNull.Value)
			{
				throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidNullValue, new object[0]));
			}
			else
			{
				ADEntity aDEntity = arguments as ADEntity;
				if (aDEntity != null)
				{
					if (!aDEntity.IsSearchResult)
					{
						if (aDEntity.Identity == null || aDEntity.Identity == AutomationNull.Value || aDEntity.Identity as string == string.Empty)
						{
							throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.NullOrEmptyIdentityPropertyArgument, new object[0]));
						}
						else
						{
							ADEntity identity = aDEntity.Identity as ADEntity;
							if (identity != null)
							{
								this.Validate(identity, engineIntrinsics);
							}
							return;
						}
					}
					else
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
		}
	}
}