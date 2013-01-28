using Microsoft.ActiveDirectory;
using System;
using System.Collections;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class ValidateAttributeValueHashtableAttribute : ValidateArgumentsAttribute
	{
		public ValidateAttributeValueHashtableAttribute()
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
				Hashtable hashtables = arguments as Hashtable;
				if (hashtables != null)
				{
					foreach (object key in hashtables.Keys)
					{
						if (key.GetType() == typeof(string))
						{
							object item = hashtables[key];
							if (item == null || item == AutomationNull.Value)
							{
								throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidNullValue, new object[0]));
							}
							else
							{
								if (item.GetType() != typeof(object[]))
								{
									continue;
								}
								object[] objArray = (object[])item;
								if ((int)objArray.Length != 0)
								{
									Type type = objArray[0].GetType();
									object[] objArray1 = objArray;
									int num = 0;
									while (num < (int)objArray1.Length)
									{
										object obj = objArray1[num];
										if (obj.GetType() == type)
										{
											num++;
										}
										else
										{
											throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidTypeInCollection, new object[0]));
										}
									}
								}
								else
								{
									throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidEmptyCollection, new object[0]));
								}
							}
						}
						else
						{
							throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidHashtableKeyType, new object[0]));
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