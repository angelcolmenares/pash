using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class AttributeConverters
	{
		private AttributeConverters()
		{
		}

		internal static void BuildToExtendedFromStringConverter(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo, AttributeConverters.StringParserDelegate parser)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				new ADPropertyValueCollection(null);
				userObj.Add(extendedAttribute, new ADPropertyValueCollection(null));
				return;
			}
			else
			{
				string value = (string)directoryObj[directoryAttributes[0]].Value;
				if (!string.IsNullOrEmpty(value))
				{
					userObj.Add(extendedAttribute, parser(value));
					return;
				}
				else
				{
					ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(null);
					userObj.Add(extendedAttribute, aDPropertyValueCollection);
					return;
				}
			}
		}

		internal static object GetAttributeValueFromObjectName<F, O>(object entity, string searchBase, string extendedAttributeName, string extendedAttribute, CmdletSessionInfo cmdletSessionInfo)
		where F : ADFactory<O>, new()
		where O : ADEntity, new()
		{
			O o;
			ADEntity extendedObjectFromIdentity;
			object value = null;
			string str = entity as string;
			if (str == null)
			{
				o = (O)(entity as O);
			}
			else
			{
				o = Activator.CreateInstance<O>();
				o.Identity = str;
			}
			if (o != null)
			{
				F f = Activator.CreateInstance<F>();
				f.SetCmdletSessionInfo(cmdletSessionInfo);
				try
				{
					if (extendedAttributeName != null)
					{
						HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						strs.Add(extendedAttributeName);
						extendedObjectFromIdentity = f.GetExtendedObjectFromIdentity(o, searchBase, strs, false);
						value = extendedObjectFromIdentity[extendedAttributeName].Value;
					}
					else
					{
						extendedObjectFromIdentity = f.GetDirectoryObjectFromIdentity(o, searchBase);
						value = extendedObjectFromIdentity["DistinguishedName"].Value;
					}
				}
				catch (ADIdentityNotFoundException aDIdentityNotFoundException1)
				{
					ADIdentityNotFoundException aDIdentityNotFoundException = aDIdentityNotFoundException1;
					object[] message = new object[2];
					message[0] = extendedAttribute;
					message[1] = aDIdentityNotFoundException.Message;
					throw new ADIdentityResolutionException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityInExtendedAttributeCannotBeResolved, message), aDIdentityNotFoundException);
				}
				catch (ADIdentityResolutionException aDIdentityResolutionException1)
				{
					ADIdentityResolutionException aDIdentityResolutionException = aDIdentityResolutionException1;
					object[] objArray = new object[2];
					objArray[0] = extendedAttribute;
					objArray[1] = aDIdentityResolutionException.Message;
					throw new ADIdentityResolutionException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityInExtendedAttributeCannotBeResolved, objArray), aDIdentityResolutionException);
				}
			}
			return value;
		}

		internal static ToDirectoryFormatDelegate GetDelegateToDirectoryIntFromFlag(int bit, bool isInverted)
		{
			return (string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo) => AttributeConverters.ToDirectoryIntFromFlag(bit, isInverted, extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
		}

		internal static ToExtendedFormatDelegate GetDelegateToExtendedFlagFromInt(int bit, bool isInverted)
		{
			return (string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo) => AttributeConverters.ToExtendedFlagFromInt(bit, isInverted, extendedAttribute, directoryAttributes, userObj, directoryObj, cmdletSessionInfo);
		}

		internal static ToExtendedFormatDelegate GetToExtendedFromStringConverter(AttributeConverters.StringParserDelegate parser)
		{
			return (string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo) => AttributeConverters.BuildToExtendedFromStringConverter(extendedAttribute, directoryAttributes, userObj, directoryObj, cmdletSessionInfo, parser);
		}

		internal static object MultiValueNoOpConvertor(object entity, string extendedAttribute, CmdletSessionInfo cmdletSessionInfo)
		{
			return entity;
		}

		internal static object ParseDateTimeFromString(string stringVal)
		{
			DateTime dateTime = DateTime.Parse(stringVal, DateTimeFormatInfo.InvariantInfo);
			return dateTime.ToLocalTime();
		}

		internal static object ParseGuidFromString(string stringVal)
		{
			Guid guid = new Guid(stringVal);
			if (guid != Guid.Empty)
			{
				return guid;
			}
			else
			{
				return null;
			}
		}

		internal static object ParseIntFromString(string stringVal)
		{
			return int.Parse(stringVal, NumberFormatInfo.InvariantInfo);
		}

		internal static object ParseLongFromString(string stringVal)
		{
			return long.Parse(stringVal, NumberFormatInfo.InvariantInfo);
		}

		internal static void ToDirectoryBlobFromADReplicationSchedule(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData == null || extendedData.Value == null)
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
			else
			{
				ActiveDirectorySchedule value = (ActiveDirectorySchedule)extendedData.Value;
				bool[,,] rawSchedule = value.RawSchedule;
				TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
				long ticks = utcOffset.Ticks / 0x861c46800L;
				int num = 188;
				int num1 = 20;
				byte[] numArray = new byte[num];
				numArray[0] = (byte)num;
				numArray[8] = 1;
				numArray[16] = (byte)num1;
				byte num2 = 0;
				int num3 = num1;
				int num4 = 0;
				while (num3 < num)
				{
					int num5 = num4 / 24;
					int num6 = num4 % 24;
					num2 = 0;
					if (rawSchedule[num5, num6, 0])
					{
						num2 = (byte)(num2 | 1);
					}
					if (rawSchedule[num5, num6, 1])
					{
						num2 = (byte)(num2 | 2);
					}
					if (rawSchedule[num5, num6, 2])
					{
						num2 = (byte)(num2 | 4);
					}
					if (rawSchedule[num5, num6, 3])
					{
						num2 = (byte)(num2 | 8);
					}
					int num7 = num3 - (int)ticks;
					if (num7 < num)
					{
						if (num7 < num1)
						{
							num7 = num - num1 - num7;
						}
					}
					else
					{
						num7 = num7 - num + num1;
					}
					numArray[num7] = num2;
					num3++;
					num4++;
				}
				directoryObj.SetValue(directoryAttributes[0], numArray);
				return;
			}
		}

		internal static void ToDirectoryDateTime(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData == null || extendedData.Value == null)
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
			else
			{
				DateTime value = (DateTime)extendedData.Value;
				directoryObj.SetValue(directoryAttributes[0], value.ToFileTime());
				return;
			}
		}

		internal static void ToDirectoryDaysFromTimeSpan(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData == null || extendedData.Value == null)
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
			else
			{
				TimeSpan value = (TimeSpan)extendedData.Value;
				if (Math.Round(value.TotalDays) == value.TotalDays)
				{
					directoryObj.SetValue(directoryAttributes[0], value.Days);
					return;
				}
				else
				{
					throw new ArgumentException(StringResources.ADInvalidQuantizationDays, extendedAttribute);
				}
			}
		}

		internal static void ToDirectoryFromADEntityToAttributeValue<F, O>(string searchBase, string extendedAttributeName, string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		where F : ADFactory<O>, new()
		where O : ADEntity, new()
		{
			string attributeValueFromObjectName;
			if (extendedData == null || extendedData.Value == null)
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
			else
			{
				string value = extendedData.Value as string;
				if (value == null)
				{
					attributeValueFromObjectName = (string)AttributeConverters.GetAttributeValueFromObjectName<F, O>(extendedData.Value, searchBase, extendedAttributeName, extendedAttribute, cmdletSessionInfo);
				}
				else
				{
					attributeValueFromObjectName = value;
				}
				if (attributeValueFromObjectName != null)
				{
					if (!directoryObj.Contains(directoryAttributes[0]))
					{
						directoryObj.Add(directoryAttributes[0], attributeValueFromObjectName);
						return;
					}
					else
					{
						directoryObj[directoryAttributes[0]].Value = attributeValueFromObjectName;
						return;
					}
				}
				else
				{
					object[] type = new object[2];
					type[0] = extendedData.Value.GetType();
					type[1] = extendedAttribute;
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.AttributeConverterUnrecognizedObjectType, type));
				}
			}
		}

		internal static void ToDirectoryFromADObjectToDN<F, O>(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		where F : ADFactory<O>, new()
		where O : ADObject, new()
		{
			AttributeConverters.ToDirectoryFromADEntityToAttributeValue<F, O>(cmdletSessionInfo.DefaultPartitionPath, null, extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
		}

		internal static void ToDirectoryIntFromFlag(int bit, bool isInverted, string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			int value = 0;
			if (directoryObj.Contains(directoryAttributes[0]))
			{
				value = (int)directoryObj[directoryAttributes[0]].Value;
			}
			bool item = (bool)extendedData[0];
			if (isInverted)
			{
				item = !item;
			}
			if (!item)
			{
				value = value & ~bit;
			}
			else
			{
				value = value | bit;
			}
			directoryObj.SetValue(directoryAttributes[0], value);
		}

		internal static void ToDirectoryIntFromFlagEnumeration(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			int value = 0;
			if (directoryObj.Contains(directoryAttributes[0]))
			{
				value = (int)directoryObj[directoryAttributes[0]].Value;
			}
			Type type = extendedData[0].GetType();
			int item = (int)extendedData[0];
			foreach (int num in Enum.GetValues(type))
			{
				bool flag = (item & num) == num;
				if (!flag)
				{
					value = value & ~num;
				}
				else
				{
					value = value | num;
				}
			}
			directoryObj.SetValue(directoryAttributes[0], value);
		}

		internal static void ToDirectoryInvertBool(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData == null || extendedData.Value == null)
			{
				AttributeConverters.ToDirectoryObject(extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
				return;
			}
			else
			{
				bool value = !(bool)extendedData.Value;
				AttributeConverters.ToDirectoryObject(extendedAttribute, directoryAttributes, new ADPropertyValueCollection((object)value), directoryObj, cmdletSessionInfo);
				return;
			}
		}

		internal static void ToDirectoryMultivalueCertificate(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			ADPropertyValueCollection aDPropertyValueCollection = null;
			if (extendedData != null)
			{
				aDPropertyValueCollection = new ADPropertyValueCollection();
				if (!extendedData.TrackChanges)
				{
					foreach (X509Certificate extendedDatum in extendedData)
					{
						aDPropertyValueCollection.Add(extendedDatum.GetRawCertData());
					}
				}
				else
				{
					aDPropertyValueCollection.TrackChanges = true;
					if (!extendedData.IsValuesCleared)
					{
						if (extendedData.ReplacedValues.Count != 0)
						{
							aDPropertyValueCollection.Clear();
							foreach (object replacedValue in extendedData.ReplacedValues)
							{
								aDPropertyValueCollection.Add(((X509Certificate)replacedValue).GetRawCertData());
							}
						}
						else
						{
							foreach (object addedValue in extendedData.AddedValues)
							{
								aDPropertyValueCollection.Add(((X509Certificate)addedValue).GetRawCertData());
							}
							foreach (object deletedValue in extendedData.DeletedValues)
							{
								aDPropertyValueCollection.ForceRemove(((X509Certificate)deletedValue).GetRawCertData());
							}
						}
					}
					else
					{
						aDPropertyValueCollection.Clear();
					}
				}
			}
			AttributeConverters.ToDirectoryMultivalueObject(extendedAttribute, directoryAttributes, aDPropertyValueCollection, directoryObj, cmdletSessionInfo);
		}

		internal static void ToDirectoryMultivalueObject(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			AttributeConverters.ToDirectoryMultivalueObjectConvertor(extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo, new MultiValueAttributeConvertorDelegate(AttributeConverters.MultiValueNoOpConvertor));
		}

		internal static void ToDirectoryMultivalueObjectConvertor(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo, MultiValueAttributeConvertorDelegate _delegate)
		{
			ADPropertyValueCollection aDPropertyValueCollection;
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				aDPropertyValueCollection = new ADPropertyValueCollection();
				aDPropertyValueCollection.TrackChanges = true;
				directoryObj.Add(directoryAttributes[0], aDPropertyValueCollection);
			}
			if (extendedData != null)
			{
				aDPropertyValueCollection = directoryObj[directoryAttributes[0]];
				if (!extendedData.TrackChanges)
				{
					aDPropertyValueCollection.Clear();
					foreach (object extendedDatum in extendedData)
					{
						aDPropertyValueCollection.Add(_delegate(extendedDatum, extendedAttribute, cmdletSessionInfo));
					}
				}
				else
				{
					if (!extendedData.IsValuesCleared)
					{
						if (extendedData.ReplacedValues.Count != 0)
						{
							aDPropertyValueCollection.Clear();
							foreach (object replacedValue in extendedData.ReplacedValues)
							{
								aDPropertyValueCollection.Add(_delegate(replacedValue, extendedAttribute, cmdletSessionInfo));
							}
						}
						else
						{
							foreach (object addedValue in extendedData.AddedValues)
							{
								aDPropertyValueCollection.Add(_delegate(addedValue, extendedAttribute, cmdletSessionInfo));
							}
							foreach (object deletedValue in extendedData.DeletedValues)
							{
								aDPropertyValueCollection.ForceRemove(_delegate(deletedValue, extendedAttribute, cmdletSessionInfo));
							}
						}
					}
					else
					{
						directoryObj.ForceRemove(directoryAttributes[0]);
					}
					if (extendedData.ReplacedValues.Count == 0 && extendedData.AddedValues.Count == 0 && extendedData.DeletedValues.Count == 0)
					{
						aDPropertyValueCollection.Clear();
						foreach (object obj in extendedData)
						{
							aDPropertyValueCollection.Add(_delegate(obj, extendedAttribute, cmdletSessionInfo));
						}
					}
				}
				return;
			}
			else
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
		}

		internal static void ToDirectoryNegativeTimeSpan(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			AttributeConverters.ToDirectoryNegativeTimeSpanHelper(false, extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
		}

		internal static void ToDirectoryNegativeTimeSpanHelper(bool performNoExpirationConversion, string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			long ticks;
			if (extendedData == null || extendedData.Value == null)
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
			else
			{
				TimeSpan value = (TimeSpan)extendedData.Value;
				if ((long)0 < value.Ticks)
				{
					ticks = -value.Ticks;
				}
				else
				{
					ticks = value.Ticks;
				}
				if (performNoExpirationConversion && ticks == (long)0)
				{
					ticks = -9223372036854775808L;
				}
				directoryObj.SetValue(directoryAttributes[0], ticks);
				return;
			}
		}

		internal static void ToDirectoryNoExpirationTimeSpan(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			AttributeConverters.ToDirectoryNegativeTimeSpanHelper(true, extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
		}

		internal static void ToDirectoryObject(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData == null || extendedData.Value == null)
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
			else
			{
				directoryObj.SetValue(directoryAttributes[0], extendedData.Value);
				return;
			}
		}

		internal static void ToDirectoryObjectWithCast<T>(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData == null || extendedData.Value == null)
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
			else
			{
				directoryObj[directoryAttributes[0]].Value = (T)extendedData.Value;
				return;
			}
		}

		internal static void ToDirectorySDDLStringFromString(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData == null || extendedData.Value == null)
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
			else
			{
				int num = AttributeConverters.ValidateSDDLString(extendedData.Value as string, cmdletSessionInfo);
				if (num != 0)
				{
					object[] objArray = new object[1];
					objArray[0] = extendedAttribute;
					Win32Exception win32Exception = new Win32Exception(num, string.Format(CultureInfo.CurrentCulture, StringResources.SDDLValidationFailed, objArray));
					cmdletSessionInfo.CmdletBase.ThrowTerminatingError(new ErrorRecord(win32Exception, num.ToString(CultureInfo.InvariantCulture), ErrorCategory.InvalidArgument, directoryObj));
				}
				directoryObj.SetValue(directoryAttributes[0], extendedData.Value);
				return;
			}
		}

		internal static void ToDirectorySecDescFromPrincipal(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			SecurityIdentifier attributeValueFromObjectName;
			if (extendedData == null || extendedData.Value == null)
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
			else
			{
				ActiveDirectorySecurity activeDirectorySecurity = new ActiveDirectorySecurity();
				SecurityIdentifier securityIdentifier = new SecurityIdentifier(ACLConstants.DomainAdministratorsGroupSID);
				activeDirectorySecurity.SetOwner(securityIdentifier);
				foreach (ADPrincipal extendedDatum in extendedData)
				{
					if (!extendedDatum.IsSearchResult || !(null != extendedDatum.SID))
					{
						attributeValueFromObjectName = (SecurityIdentifier)AttributeConverters.GetAttributeValueFromObjectName<ADPrincipalFactory<ADPrincipal>, ADPrincipal>(extendedDatum, cmdletSessionInfo.DefaultPartitionPath, "SID", extendedAttribute, cmdletSessionInfo);
					}
					else
					{
						attributeValueFromObjectName = extendedDatum.SID;
					}
					activeDirectorySecurity.AddAccessRule(new ActiveDirectoryAccessRule(attributeValueFromObjectName, ActiveDirectoryRights.GenericAll, AccessControlType.Allow, ActiveDirectorySecurityInheritance.None));
				}
				directoryObj.Add(directoryAttributes[0], activeDirectorySecurity);
				return;
			}
		}

		internal static void ToExtendedADReplicationScheduleFromBlob(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(null);
				userObj.Add(extendedAttribute, aDPropertyValueCollection);
				return;
			}
			else
			{
				if (directoryObj[directoryAttributes[0]].Value != null)
				{
					ActiveDirectorySchedule activeDirectorySchedule = new ActiveDirectorySchedule();
					bool[,,] flagArray = new bool[7, 24, 4];
					int num = 188;
					int num1 = 20;
					byte[] value = (byte[])directoryObj[directoryAttributes[0]].Value;
					TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
					long ticks = utcOffset.Ticks / 0x861c46800L;
					int num2 = num1;
					int num3 = 0;
					while (num2 < num)
					{
						int num4 = num3 / 24;
						int num5 = num3 % 24;
						int num6 = num2 - (int)ticks;
						if (num6 < num)
						{
							if (num6 < num1)
							{
								num6 = num - num1 - num6;
							}
						}
						else
						{
							num6 = num6 - num + num1;
						}
						int num7 = value[num6];
						if ((num7 & 1) != 0)
						{
							flagArray[num4, num5, 0] = true;
						}
						if ((num7 & 2) != 0)
						{
							flagArray[num4, num5, 1] = true;
						}
						if ((num7 & 4) != 0)
						{
							flagArray[num4, num5, 2] = true;
						}
						if ((num7 & 8) != 0)
						{
							flagArray[num4, num5, 3] = true;
						}
						num2++;
						num3++;
					}
					activeDirectorySchedule.RawSchedule = flagArray;
					userObj.Add(extendedAttribute, activeDirectorySchedule);
					return;
				}
				else
				{
					ADPropertyValueCollection aDPropertyValueCollection1 = new ADPropertyValueCollection(null);
					userObj.Add(extendedAttribute, aDPropertyValueCollection1);
					return;
				}
			}
		}

		internal static void ToExtendedDateTimeFromDateTime(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(null);
				userObj.Add(extendedAttribute, aDPropertyValueCollection);
				return;
			}
			else
			{
				if (directoryObj[directoryAttributes[0]].Value != null)
				{
					DateTime value = (DateTime)directoryObj[directoryAttributes[0]].Value;
					userObj.Add(extendedAttribute, value.ToLocalTime());
					return;
				}
				else
				{
					ADPropertyValueCollection aDPropertyValueCollection1 = new ADPropertyValueCollection(null);
					userObj.Add(extendedAttribute, aDPropertyValueCollection1);
					return;
				}
			}
		}

		internal static void ToExtendedDateTimeFromLong(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				new ADPropertyValueCollection(null);
				userObj.Add(extendedAttribute, new ADPropertyValueCollection(null));
				return;
			}
			else
			{
				long value = (long)directoryObj[directoryAttributes[0]].Value;
				if ((long)0 != value)
				{
					DateTime dateTime = DateTime.FromFileTimeUtc(value);
					userObj.Add(extendedAttribute, dateTime.ToLocalTime());
					return;
				}
				else
				{
					ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(null);
					userObj.Add(extendedAttribute, aDPropertyValueCollection);
					return;
				}
			}
		}

		internal static void ToExtendedFlagEnumerationFromInt<T>(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (directoryObj.Contains(directoryAttributes[0]))
			{
				int value = (int)directoryObj[directoryAttributes[0]].Value;
				int num = 0;
				foreach (int value1 in Enum.GetValues(typeof(T)))
				{
					if ((value & value1) != value1)
					{
						continue;
					}
					num = num | value1;
				}
				userObj.Add(extendedAttribute, (T)(object)num);
				return;
			}
			else
			{
				userObj.Add(extendedAttribute, null);
				return;
			}
		}

		internal static void ToExtendedFlagFromInt(int bit, bool isInverted, string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			int value;
			bool flag;
			uint num = 0;
			if (directoryObj.Contains(directoryAttributes[0]))
			{
				string str = directoryObj[directoryAttributes[0]].Value as string;
				if (string.IsNullOrEmpty(str))
				{
					value = (int)directoryObj[directoryAttributes[0]].Value;
				}
				else
				{
					if (!int.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out value))
					{
						if (!uint.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out num))
						{
							return;
						}
						else
						{
							value = Convert.ToInt32 (num);
						}
					}
				}
				if (!isInverted)
				{
					flag = (value & bit) != 0;
				}
				else
				{
					flag = (value & bit) == 0;
				}
				userObj.Add(extendedAttribute, flag);
				return;
			}
			else
			{
				userObj.Add(extendedAttribute, null);
				return;
			}
		}

		internal static void ToExtendedFromFirstAttributeOnly(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			string[] strArrays = new string[1];
			strArrays[0] = directoryAttributes[0];
			AttributeConverters.ToExtendedObject(extendedAttribute, strArrays, userObj, directoryObj, cmdletSessionInfo);
		}

		internal static void ToExtendedGuid(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				userObj.Add(extendedAttribute, null);
				return;
			}
			else
			{
				byte[] value = (byte[])directoryObj[directoryAttributes[0]].Value;
				Guid guid = new Guid(value);
				userObj.Add(extendedAttribute, guid);
				return;
			}
		}

		internal static void ToExtendedInvertBool(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			AttributeConverters.ToExtendedObject(extendedAttribute, directoryAttributes, userObj, directoryObj, cmdletSessionInfo);
			if (userObj[extendedAttribute].Value is bool)
			{
				userObj[extendedAttribute].Value = !(bool)userObj[extendedAttribute].Value;
			}
		}

		internal static void ToExtendedMultivalueCertificate(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection();
			foreach (byte[] item in directoryObj[directoryAttributes[0]])
			{
				X509Certificate x509Certificate = new X509Certificate(item);
				aDPropertyValueCollection.Add(x509Certificate);
			}
			userObj.Add(extendedAttribute, aDPropertyValueCollection);
		}

		internal static void ToExtendedMultivalueObject(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				userObj.Add(extendedAttribute, new ADPropertyValueCollection());
				return;
			}
			else
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(directoryObj[directoryAttributes[0]].Value);
				userObj.Add(extendedAttribute, aDPropertyValueCollection);
				return;
			}
		}

		internal static void ToExtendedMultivalueStringFromMultiAttribute(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			List<string> strs = new List<string>();
			string[] strArrays = directoryAttributes;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				if (directoryObj.Contains(str))
				{
					string[] value = directoryObj[str].Value as string[];
					string[] strArrays1 = value;
					for (int j = 0; j < (int)strArrays1.Length; j++)
					{
						string str1 = strArrays1[j];
						strs.Add(str1);
					}
				}
			}
			if (strs.Count <= 0)
			{
				userObj.Add(extendedAttribute, new ADPropertyValueCollection());
				return;
			}
			else
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(strs.ToArray());
				userObj.Add(extendedAttribute, aDPropertyValueCollection);
				return;
			}
		}

		internal static void ToExtendedNoExpirationTimeSpan(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(null);
				userObj.Add(extendedAttribute, aDPropertyValueCollection);
				return;
			}
			else
			{
				long value = (long)directoryObj[directoryAttributes[0]].Value;
				if (-9223372036854775808L != value)
				{
					value = Math.Abs(value);
					TimeSpan timeSpan = TimeSpan.FromTicks(value);
					userObj.Add(extendedAttribute, timeSpan);
					return;
				}
				else
				{
					TimeSpan timeSpan1 = TimeSpan.FromTicks((long)0);
					userObj.Add(extendedAttribute, timeSpan1);
					return;
				}
			}
		}

		internal static void ToExtendedObject(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				userObj.Add(extendedAttribute, new ADPropertyValueCollection());
				return;
			}
			else
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(directoryObj[directoryAttributes[0]].Value);
				userObj.Add(extendedAttribute, aDPropertyValueCollection);
				return;
			}
		}

		internal static void ToExtendedObjectWithCast<T>(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				userObj.Add(extendedAttribute, new ADPropertyValueCollection());
				return;
			}
			else
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection((object)((T)directoryObj[directoryAttributes[0]].Value));
				userObj.Add(extendedAttribute, aDPropertyValueCollection);
				return;
			}
		}

		public static void ToExtendedPrincipalFromSecDesc(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				userObj.Add(extendedAttribute, new ADPropertyValueCollection(null));
			}
			else
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection();
				userObj.Add(extendedAttribute, aDPropertyValueCollection);
				ActiveDirectorySecurity value = (ActiveDirectorySecurity)directoryObj[directoryAttributes[0]].Value;
				bool flag = false;
				SecurityIdentifier securityIdentifier = null;
				List<string> strs = new List<string>();
				IADOPathNode aDOPathNode = null;
				bool flag1 = false;
				if (!directoryObj.Contains("objectSid") || directoryObj["objectSid"].Value == null)
				{
					flag1 = true;
				}
				else
				{
					securityIdentifier = (SecurityIdentifier)directoryObj["objectSid"].Value;
				}
				foreach (ActiveDirectoryAccessRule accessRule in value.GetAccessRules(true, true, typeof(SecurityIdentifier)))
				{
					if (accessRule.AccessControlType != AccessControlType.Allow || accessRule.IsInherited || accessRule.InheritanceType != ActiveDirectorySecurityInheritance.None)
					{
						flag = true;
					}
					else
					{
						string str = accessRule.IdentityReference.Value;
						SecurityIdentifier securityIdentifier1 = new SecurityIdentifier(str);
						if (null != securityIdentifier && !securityIdentifier.IsEqualDomainSid(securityIdentifier1))
						{
							flag1 = true;
						}
						strs.Add(str);
						IADOPathNode aDOPathNode1 = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectSid", str);
						if (aDOPathNode != null)
						{
							IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
							aDOPathNodeArray[0] = aDOPathNode;
							aDOPathNodeArray[1] = aDOPathNode1;
							aDOPathNode = ADOPathUtil.CreateOrClause(aDOPathNodeArray);
						}
						else
						{
							aDOPathNode = aDOPathNode1;
						}
					}
				}
				if (strs.Count != 0)
				{
					ADSessionInfo aDSessionInfo = cmdletSessionInfo.ADSessionInfo;
					if (flag1)
					{
						ADTopologyManagement aDTopologyManagement = new ADTopologyManagement(cmdletSessionInfo.ADSessionInfo);
						ADObject domain = aDTopologyManagement.GetDomain();
						if (domain != null)
						{
							if (!domain.Contains("DNSRoot") || domain["DNSRoot"].Value == null)
							{
								aDSessionInfo = new ADSessionInfo(domain.Name);
							}
							else
							{
								aDSessionInfo = new ADSessionInfo(domain["DNSRoot"].Value as string);
							}
							aDSessionInfo.SetDefaultPort(LdapConstants.LDAP_GC_PORT);
							if (cmdletSessionInfo.ADSessionInfo != null)
							{
								aDSessionInfo.Credential = cmdletSessionInfo.ADSessionInfo.Credential;
							}
						}
					}
					try
					{
						using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(aDSessionInfo))
						{
							IADOPathNode[] aDOPathNodeArray1 = new IADOPathNode[2];
							aDOPathNodeArray1[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "group");
							aDOPathNodeArray1[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "user");
							IADOPathNode aDOPathNode2 = ADOPathUtil.CreateOrClause(aDOPathNodeArray1);
							aDObjectSearcher.SearchRoot = cmdletSessionInfo.ADRootDSE.DefaultNamingContext;
							IADOPathNode[] aDOPathNodeArray2 = new IADOPathNode[2];
							aDOPathNodeArray2[0] = aDOPathNode2;
							aDOPathNodeArray2[1] = aDOPathNode;
							aDObjectSearcher.Filter = ADOPathUtil.CreateAndClause(aDOPathNodeArray2);
							aDObjectSearcher.Properties.Add("objectSid");
							foreach (ADObject aDObject in aDObjectSearcher.FindAll())
							{
								SecurityIdentifier value1 = (SecurityIdentifier)aDObject["objectSid"].Value;
								if (!strs.Contains(value1.ToString()))
								{
									continue;
								}
								userObj[extendedAttribute].Add(aDObject.DistinguishedName);
								strs.Remove(value1.ToString());
							}
						}
					}
					catch (ADServerDownException aDServerDownException)
					{
					}
					foreach (string str1 in strs)
					{
						userObj[extendedAttribute].Add(str1);
					}
				}
				if (flag)
				{
					object[] objArray = new object[2];
					objArray[0] = directoryObj["distinguishedName"].Value;
					objArray[1] = extendedAttribute;
					cmdletSessionInfo.CmdletMessageWriter.WriteWarningBuffered(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidACEInSecDesc, objArray));
					return;
				}
			}
		}

		internal static void ToExtendedTimeSpanFromDays(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(null);
				userObj.Add(extendedAttribute, aDPropertyValueCollection);
				return;
			}
			else
			{
				int value = (int)directoryObj[directoryAttributes[0]].Value;
				if (value != 0)
				{
					TimeSpan timeSpan = TimeSpan.FromDays((double)value);
					userObj.Add(extendedAttribute, timeSpan);
					return;
				}
				else
				{
					ADPropertyValueCollection aDPropertyValueCollection1 = new ADPropertyValueCollection(null);
					userObj.Add(extendedAttribute, aDPropertyValueCollection1);
					return;
				}
			}
		}

		internal static int ValidateSDDLString(string sddlString, CmdletSessionInfo cmdletSessionInfo)
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			int lastWin32Error = 0;
			bool securityDescriptor = UnsafeNativeMethods.ConvertStringSecurityDescriptorToSecurityDescriptor(sddlString, 1, out zero, intPtr);
			if (!securityDescriptor)
			{
				lastWin32Error = Marshal.GetLastWin32Error();
			}
			if (zero != IntPtr.Zero)
			{
				UnsafeNativeMethods.LocalFree(zero);
			}
			return lastWin32Error;
		}

		internal delegate object StringParserDelegate(string stringVal);
	}
}