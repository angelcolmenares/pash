using Microsoft.ActiveDirectory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class ValidateCollectionIsUniqueAttribute : ValidateArgumentsAttribute
	{
		public ValidateCollectionIsUniqueAttribute()
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
				IEnumerable enumerable = arguments as IEnumerable;
				if (enumerable != null)
				{
					HashSet<object> objs = new HashSet<object>();
					foreach (object obj in enumerable)
					{
						if (!objs.Contains(obj))
						{
							objs.Add(obj);
						}
						else
						{
							object[] str = new object[1];
							str[0] = obj.ToString();
							throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.DuplicateValuesSpecified, str));
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