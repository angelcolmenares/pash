using Microsoft.ActiveDirectory;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class ValidateNullableRangeAttribute : ValidateArgumentsAttribute
	{
		private int _minRange;

		private int _maxRange;

		public ValidateNullableRangeAttribute(int minRange, int maxRange)
		{
			this._minRange = minRange;
			this._maxRange = maxRange;
		}

		protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
		{
			if (arguments == null || arguments == AutomationNull.Value)
			{
				return;
			}
			else
			{
				int? nullable = (int?)(arguments as int?);
				if (nullable.HasValue)
				{
					if (nullable.Value >= this._minRange)
					{
						if (nullable.Value > this._maxRange)
						{
							object[] objArray = new object[2];
							objArray[0] = nullable;
							objArray[1] = this._maxRange;
							throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.ValidateRangeGreaterThanMaxValue, objArray));
						}
					}
					else
					{
						object[] objArray1 = new object[2];
						objArray1[0] = nullable;
						objArray1[1] = this._minRange;
						throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.ValidateRangeLessThanMinValue, objArray1));
					}
				}
				return;
			}
		}
	}
}