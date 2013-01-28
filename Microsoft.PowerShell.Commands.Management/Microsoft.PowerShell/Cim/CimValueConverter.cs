using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Commands;
using Microsoft.PowerShell.Commands.Management;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace Microsoft.PowerShell.Cim
{
	internal static class CimValueConverter
	{
		[Conditional("DEBUG")]
		internal static void AssertIntrinsicCimType(Type type)
		{
		}

		[Conditional("DEBUG")]
		internal static void AssertIntrinsicCimValue(object value)
		{
			value.GetType();
		}

		internal static object ConvertFromCimToDotNet(object cimObject, Type expectedDotNetType)
		{
			if (expectedDotNetType != null)
			{
				if (cimObject != null)
				{
					if (expectedDotNetType.IsGenericType && expectedDotNetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
					{
						expectedDotNetType = expectedDotNetType.GetGenericArguments()[0];
					}
					if (!LanguagePrimitives.IsCimIntrinsicScalarType(expectedDotNetType))
					{
						if (!expectedDotNetType.Equals(typeof(CimInstance)))
						{
							if (expectedDotNetType.IsArray)
							{
								Type elementType = CimValueConverter.GetElementType(expectedDotNetType);
								if (elementType != null)
								{
									Array arrays = (Array)LanguagePrimitives.ConvertTo(cimObject, typeof(Array), CultureInfo.InvariantCulture);
									Array arrays1 = Array.CreateInstance(elementType, arrays.Length);
									for (int i = 0; i < arrays1.Length; i++)
									{
										object dotNet = CimValueConverter.ConvertFromCimToDotNet(arrays.GetValue(i), elementType);
										arrays1.SetValue(dotNet, i);
									}
									return arrays1;
								}
							}
							Type convertibleCimType = CimValueConverter.GetConvertibleCimType(expectedDotNetType);
							if (convertibleCimType == null)
							{
								Func<Func<object>, object> func = (Func<object> innerAction) => {
									object obj;
									try
									{
										obj = innerAction();
									}
									catch (Exception exception1)
									{
										Exception exception = exception1;
										CommandProcessorBase.CheckForSevereException(exception);
										throw CimValueConverter.GetInvalidCastException(exception, "InvalidCimToDotNetCast", cimObject, expectedDotNetType.FullName);
									}
									return obj;
								}
								;
								if (!typeof(ObjectSecurity).IsAssignableFrom(expectedDotNetType))
								{
									if (!typeof(X509Certificate2).Equals(expectedDotNetType))
									{
										if (!typeof(X500DistinguishedName).Equals(expectedDotNetType))
										{
											if (!typeof(PhysicalAddress).Equals(expectedDotNetType))
											{
												if (!typeof(IPEndPoint).Equals(expectedDotNetType))
												{
													if (!typeof(XmlDocument).Equals(expectedDotNetType))
													{
														throw CimValueConverter.GetInvalidCastException(null, "InvalidCimToDotNetCast", cimObject, expectedDotNetType.FullName);
													}
													else
													{
														return func(() => {
															int? nullable = null;
															XmlDocument xmlDocument = InternalDeserializer.LoadUnsafeXmlDocument((string)LanguagePrimitives.ConvertTo(cimObject, typeof(string), CultureInfo.InvariantCulture), true, nullable);
															return xmlDocument;
														}
														);
													}
												}
												else
												{
													return func(() => {
														int num = ((string)LanguagePrimitives.ConvertTo(cimObject, typeof(string), CultureInfo.InvariantCulture)).LastIndexOf(':');
														int num1 = int.Parse(((string)LanguagePrimitives.ConvertTo(cimObject, typeof(string), CultureInfo.InvariantCulture)).Substring(num + 1), NumberStyles.Integer, CultureInfo.InvariantCulture);
														IPAddress pAddress = IPAddress.Parse(((string)LanguagePrimitives.ConvertTo(cimObject, typeof(string), CultureInfo.InvariantCulture)).Substring(0, num));
														return new IPEndPoint(pAddress, num1);
													}
													);
												}
											}
											else
											{
												return func(() => PhysicalAddress.Parse((string)LanguagePrimitives.ConvertTo(cimObject, typeof(string), CultureInfo.InvariantCulture)));
											}
										}
										else
										{
											return func(() => new X500DistinguishedName((byte[])LanguagePrimitives.ConvertTo(cimObject, typeof(byte[]), CultureInfo.InvariantCulture)));
										}
									}
									else
									{
										return func(() => new X509Certificate2((byte[])LanguagePrimitives.ConvertTo(cimObject, typeof(byte[]), CultureInfo.InvariantCulture)));
									}
								}
								else
								{
									return func(() => {
										ObjectSecurity objectSecurity = (ObjectSecurity)Activator.CreateInstance(expectedDotNetType);
										objectSecurity.SetSecurityDescriptorSddlForm((string)LanguagePrimitives.ConvertTo(cimObject, typeof(string), CultureInfo.InvariantCulture));
										return objectSecurity;
									}
									);
								}
							}
							else
							{
								object obj1 = LanguagePrimitives.ConvertTo(cimObject, convertibleCimType, CultureInfo.InvariantCulture);
								object obj2 = LanguagePrimitives.ConvertTo(obj1, expectedDotNetType, CultureInfo.InvariantCulture);
								return obj2;
							}
						}
						else
						{
							return LanguagePrimitives.ConvertTo(cimObject, expectedDotNetType, CultureInfo.InvariantCulture);
						}
					}
					else
					{
						return LanguagePrimitives.ConvertTo(cimObject, expectedDotNetType, CultureInfo.InvariantCulture);
					}
				}
				else
				{
					return null;
				}
			}
			else
			{
				throw new ArgumentNullException("expectedDotNetType");
			}
		}

		internal static object ConvertFromDotNetToCim(object dotNetObject)
		{
			if (dotNetObject != null)
			{
				PSObject pSObject = PSObject.AsPSObject(dotNetObject);
				Type type = pSObject.BaseObject.GetType();
				if (!LanguagePrimitives.IsCimIntrinsicScalarType(type))
				{
					if (!typeof(CimInstance).IsAssignableFrom(type))
					{
						if (!typeof(PSReference).IsAssignableFrom(type))
						{
							if (type.IsArray)
							{
								Type elementType = CimValueConverter.GetElementType(type);
								if (elementType != null)
								{
									Array baseObject = (Array)pSObject.BaseObject;
									Type cimType = CimValueConverter.GetCimType(elementType);
									Array arrays = Array.CreateInstance(cimType, baseObject.Length);
									for (int i = 0; i < arrays.Length; i++)
									{
										object cim = CimValueConverter.ConvertFromDotNetToCim(baseObject.GetValue(i));
										arrays.SetValue(cim, i);
									}
									return arrays;
								}
							}
							Type convertibleCimType = CimValueConverter.GetConvertibleCimType(type);
							if (convertibleCimType == null)
							{
								if (!typeof(ObjectSecurity).IsAssignableFrom(type))
								{
									if (!typeof(X509Certificate2).IsAssignableFrom(type))
									{
										if (!typeof(X500DistinguishedName).IsAssignableFrom(type))
										{
											if (!typeof(PhysicalAddress).IsAssignableFrom(type))
											{
												if (!typeof(IPEndPoint).IsAssignableFrom(type))
												{
													if (!typeof(WildcardPattern).IsAssignableFrom(type))
													{
														if (!typeof(XmlDocument).IsAssignableFrom(type))
														{
															throw CimValueConverter.GetInvalidCastException(null, "InvalidDotNetToCimCast", dotNetObject, CmdletizationResources.CimConversion_CimIntrinsicValue);
														}
														else
														{
															XmlDocument xmlDocument = (XmlDocument)pSObject.BaseObject;
															string outerXml = xmlDocument.OuterXml;
															return outerXml;
														}
													}
													else
													{
														WildcardPattern wildcardPattern = (WildcardPattern)pSObject.BaseObject;
														return wildcardPattern.ToWql();
													}
												}
												else
												{
													object obj = LanguagePrimitives.ConvertTo(dotNetObject, typeof(string), CultureInfo.InvariantCulture);
													return obj;
												}
											}
											else
											{
												object obj1 = LanguagePrimitives.ConvertTo(dotNetObject, typeof(string), CultureInfo.InvariantCulture);
												return obj1;
											}
										}
										else
										{
											X500DistinguishedName x500DistinguishedName = (X500DistinguishedName)pSObject.BaseObject;
											byte[] rawData = x500DistinguishedName.RawData;
											return rawData;
										}
									}
									else
									{
										X509Certificate2 x509Certificate2 = (X509Certificate2)pSObject.BaseObject;
										byte[] numArray = x509Certificate2.RawData;
										return numArray;
									}
								}
								else
								{
									string sddl = SecurityDescriptorCommandsBase.GetSddl(pSObject);
									return sddl;
								}
							}
							else
							{
								object obj2 = LanguagePrimitives.ConvertTo(dotNetObject, convertibleCimType, CultureInfo.InvariantCulture);
								return obj2;
							}
						}
						else
						{
							PSReference pSReference = (PSReference)pSObject.BaseObject;
							if (pSReference.Value != null)
							{
								PSObject pSObject1 = PSObject.AsPSObject(pSReference.Value);
								return CimValueConverter.ConvertFromDotNetToCim(pSObject1.BaseObject);
							}
							else
							{
								return null;
							}
						}
					}
					else
					{
						return pSObject.BaseObject;
					}
				}
				else
				{
					return pSObject.BaseObject;
				}
			}
			else
			{
				return null;
			}
		}

		internal static Type GetCimType(Type dotNetType)
		{
			if (!dotNetType.IsArray)
			{
				if (!dotNetType.IsGenericType || !dotNetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
				{
					if (!LanguagePrimitives.IsCimIntrinsicScalarType(dotNetType))
					{
						if (!dotNetType.Equals(typeof(CimInstance)))
						{
							if (!dotNetType.Equals(typeof(PSReference)))
							{
								Type convertibleCimType = CimValueConverter.GetConvertibleCimType(dotNetType);
								if (convertibleCimType == null)
								{
									if (!typeof(ObjectSecurity).IsAssignableFrom(dotNetType))
									{
										if (!typeof(X509Certificate2).Equals(dotNetType))
										{
											if (!typeof(X500DistinguishedName).Equals(dotNetType))
											{
												if (!typeof(PhysicalAddress).Equals(dotNetType))
												{
													if (!typeof(IPEndPoint).Equals(dotNetType))
													{
														if (!typeof(WildcardPattern).Equals(dotNetType))
														{
															if (!typeof(XmlDocument).Equals(dotNetType))
															{
																return null;
															}
															else
															{
																return typeof(string);
															}
														}
														else
														{
															return typeof(string);
														}
													}
													else
													{
														return typeof(string);
													}
												}
												else
												{
													return typeof(string);
												}
											}
											else
											{
												return typeof(byte[]);
											}
										}
										else
										{
											return typeof(byte[]);
										}
									}
									else
									{
										return typeof(string);
									}
								}
								else
								{
									return convertibleCimType;
								}
							}
							else
							{
								return dotNetType;
							}
						}
						else
						{
							return dotNetType;
						}
					}
					else
					{
						return dotNetType;
					}
				}
				else
				{
					return CimValueConverter.GetCimType(dotNetType.GetGenericArguments()[0]);
				}
			}
			else
			{
				return CimValueConverter.GetCimType(CimValueConverter.GetElementType(dotNetType)).MakeArrayType();
			}
		}

		internal static CimType GetCimTypeEnum(Type dotNetType)
		{
			if (!typeof(PSReference).IsAssignableFrom(dotNetType))
			{
				if (!typeof(PSReference[]).IsAssignableFrom(dotNetType))
				{
					return CimConverter.GetCimType(dotNetType);
				}
				else
				{
					return CimType.ReferenceArray;
				}
			}
			else
			{
				return CimType.Reference;
			}
		}

		private static Type GetConvertibleCimType(Type dotNetType)
		{
			if (!dotNetType.IsEnum)
			{
				if (!dotNetType.Equals(typeof(SwitchParameter)))
				{
					if (dotNetType.Equals(typeof(Guid)) || dotNetType.Equals(typeof(Uri)) || dotNetType.Equals(typeof(Version)) || dotNetType.Equals(typeof(IPAddress)) || dotNetType.Equals(typeof(MailAddress)))
					{
						return typeof(string);
					}
					else
					{
						return null;
					}
				}
				else
				{
					return typeof(bool);
				}
			}
			else
			{
				return Enum.GetUnderlyingType(dotNetType);
			}
		}

		private static Type GetElementType(Type arrayType)
		{
			if (arrayType.GetArrayRank() == 1)
			{
				Type elementType = arrayType.GetElementType();
				if (!elementType.IsArray)
				{
					return elementType;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		internal static PSInvalidCastException GetInvalidCastException(Exception innerException, string errorId, object sourceValue, string descriptionOfTargetType)
		{
			object[] fullName = new object[3];
			fullName[0] = sourceValue;
			fullName[1] = PSObject.AsPSObject(sourceValue).BaseObject.GetType().FullName;
			fullName[2] = descriptionOfTargetType;
			throw new PSInvalidCastException(errorId, innerException, ExtendedTypeSystem.InvalidCastException, fullName);
		}
	}
}