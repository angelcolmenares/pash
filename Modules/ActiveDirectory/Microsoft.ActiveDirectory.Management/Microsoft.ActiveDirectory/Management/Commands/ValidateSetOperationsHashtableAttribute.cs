using Microsoft.ActiveDirectory;
using System;
using System.Collections;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class ValidateSetOperationsHashtableAttribute : ValidateArgumentsAttribute
	{
		private Type _expectedValueType;

		public ValidateSetOperationsHashtableAttribute(Type expectedValueType)
		{
			this._expectedValueType = expectedValueType;
		}

		private bool IsObjectOfExpectedType(object o)
		{
			object baseObject;
			if (o.GetType() != typeof(PSObject))
			{
				baseObject = o;
			}
			else
			{
				baseObject = ((PSObject)o).BaseObject;
			}
			return this._expectedValueType.IsAssignableFrom(baseObject.GetType());
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
							if ("Replace".Equals((string)key, StringComparison.OrdinalIgnoreCase) || "Add".Equals((string)key, StringComparison.OrdinalIgnoreCase) || "Remove".Equals((string)key, StringComparison.OrdinalIgnoreCase))
							{
								object item = hashtables[key];
								if ((item == null || item == AutomationNull.Value) && !"Replace".Equals((string)key, StringComparison.OrdinalIgnoreCase))
								{
									throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidNullValue, new object[0]));
								}
								else
								{
									if (item == null || item == AutomationNull.Value)
									{
										continue;
									}
									if (item.GetType() != typeof(object[]))
									{
										if (this.IsObjectOfExpectedType(item))
										{
											continue;
										}
										object[] objArray = new object[1];
										objArray[0] = this._expectedValueType;
										throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.ObjectTypeNotEqualToExpectedType, objArray));
									}
									else
									{
										object[] objArray1 = (object[])item;
										object[] objArray2 = objArray1;
										int num = 0;
										while (num < (int)objArray2.Length)
										{
											object obj = objArray2[num];
											if (obj == null || obj == AutomationNull.Value)
											{
												throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidNullValue, new object[0]));
											}
											else
											{
												if (this.IsObjectOfExpectedType(obj))
												{
													num++;
												}
												else
												{
													object[] objArray3 = new object[1];
													objArray3[0] = this._expectedValueType;
													throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.ObjectTypeNotEqualToExpectedType, objArray3));
												}
											}
										}
									}
								}
							}
							else
							{
								throw new ValidationMetadataException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidHashtableKey, new object[0]));
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