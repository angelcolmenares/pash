using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Reflection;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADEntityAdapter : PSPropertyAdapter
	{
		private const string ParseMethodName = "Parse";

		private const string _debugCategory = "ADEntityAdapter";

		private readonly static string DefaultObjectTypeName;

		private readonly static string MultiValueTypeName;

		private readonly static string TypeAdapterTypeName;

		private readonly static string ADEntityTypeName;

		static ADEntityAdapter()
		{
			ADEntityAdapter.DefaultObjectTypeName = typeof(object).FullName;
			ADEntityAdapter.MultiValueTypeName = typeof(ADPropertyValueCollection).FullName;
			ADEntityAdapter.TypeAdapterTypeName = typeof(ADEntityAdapter).FullName;
			ADEntityAdapter.ADEntityTypeName = typeof(ADEntity).FullName;
		}

		public ADEntityAdapter()
		{
		}

		private object ConvertValue(PSAdaptedProperty property, object valueObject, Type attrType, Type valType)
		{
			Exception exception = null;
			object obj = null;
			if (!this.TryConvertValue(property, valueObject, attrType, valType, out obj, out exception))
			{
				if (exception == null)
				{
					object[] name = new object[3];
					name[0] = property.Name;
					name[1] = valType;
					name[2] = attrType;
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.NoConversionExists, name));
				}
				else
				{
					object[] message = new object[3];
					message[0] = valType;
					message[1] = attrType;
					message[2] = exception.Message;
					throw new FormatException(string.Format(CultureInfo.CurrentCulture, StringResources.TypeConversionError, message), exception);
				}
			}
			else
			{
				return obj;
			}
		}

		public override Collection<PSAdaptedProperty> GetProperties(object baseObject)
		{
			ADEntity aDEntity = baseObject as ADEntity;
			if (aDEntity != null)
			{
				Collection<PSAdaptedProperty> pSAdaptedProperties = new Collection<PSAdaptedProperty>();
				foreach (string propertyName in aDEntity.PropertyNames)
				{
					pSAdaptedProperties.Add(new PSAdaptedProperty(propertyName, null));
				}
				return pSAdaptedProperties;
			}
			else
			{
				object[] typeAdapterTypeName = new object[2];
				typeAdapterTypeName[0] = ADEntityAdapter.TypeAdapterTypeName;
				typeAdapterTypeName[1] = ADEntityAdapter.ADEntityTypeName;
				throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.TypeAdapterForADEntityOnly, typeAdapterTypeName));
			}
		}

		public override PSAdaptedProperty GetProperty(object baseObject, string propertyName)
		{
			ADEntity aDEntity = baseObject as ADEntity;
			if (aDEntity != null)
			{
				if (!aDEntity.HasMethod(propertyName))
				{
					return new PSAdaptedProperty(propertyName, null);
				}
				else
				{
					return null;
				}
			}
			else
			{
				object[] typeAdapterTypeName = new object[2];
				typeAdapterTypeName[0] = ADEntityAdapter.TypeAdapterTypeName;
				typeAdapterTypeName[1] = ADEntityAdapter.ADEntityTypeName;
				throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.TypeAdapterForADEntityOnly, typeAdapterTypeName));
			}
		}

		public override string GetPropertyTypeName(PSAdaptedProperty property)
		{
			if (property != null)
			{
				ADEntity baseObject = property.BaseObject as ADEntity;
				if (baseObject != null)
				{
					PropertyInfo dotNetProperty = baseObject.GetDotNetProperty(property.Name);
					if (dotNetProperty == null)
					{
						if (!baseObject.PropertyIsSingleValue(property.Name))
						{
							return ADEntityAdapter.MultiValueTypeName;
						}
						else
						{
							return baseObject.GetPropertyType(property.Name).FullName;
						}
					}
					else
					{
						return dotNetProperty.PropertyType.FullName;
					}
				}
				else
				{
					object[] typeAdapterTypeName = new object[2];
					typeAdapterTypeName[0] = ADEntityAdapter.TypeAdapterTypeName;
					typeAdapterTypeName[1] = ADEntityAdapter.ADEntityTypeName;
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.TypeAdapterForADEntityOnly, typeAdapterTypeName));
				}
			}
			else
			{
				throw new ArgumentNullException("property");
			}
		}

		public override object GetPropertyValue(PSAdaptedProperty property)
		{
			if (property != null)
			{
				if (property.BaseObject != null)
				{
					if (property.Name != null)
					{
						ADEntity baseObject = property.BaseObject as ADEntity;
						if (baseObject != null)
						{
							return ADEntityAdapter.GetPropertyValue(baseObject, property.Name);
						}
						else
						{
							object[] typeAdapterTypeName = new object[2];
							typeAdapterTypeName[0] = ADEntityAdapter.TypeAdapterTypeName;
							typeAdapterTypeName[1] = ADEntityAdapter.ADEntityTypeName;
							throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.TypeAdapterForADEntityOnly, typeAdapterTypeName));
						}
					}
					else
					{
						throw new ArgumentNullException("property.Name");
					}
				}
				else
				{
					throw new ArgumentNullException("property.BaseObject");
				}
			}
			else
			{
				throw new ArgumentNullException("property");
			}
		}

		internal static object GetPropertyValue(ADEntity targetEntity, string propertyName)
		{
			PropertyInfo dotNetProperty = targetEntity.GetDotNetProperty(propertyName);
			if (dotNetProperty == null)
			{
				if (targetEntity.PropertyIsReadable(propertyName))
				{
					ADPropertyValueCollection item = targetEntity[propertyName];
					if (targetEntity.PropertyIsSingleValue(propertyName))
					{
						if (item == null || item.Count == 0)
						{
							return null;
						}
						else
						{
							return item[0];
						}
					}
					else
					{
						return item;
					}
				}
				else
				{
					return null;
				}
			}
			else
			{
				return dotNetProperty.GetValue(targetEntity, null);
			}
		}

		private Type GetRealType(Type inputType)
		{
			Type underlyingType = Nullable.GetUnderlyingType(inputType);
			if (underlyingType == null)
			{
				return inputType;
			}
			else
			{
				return underlyingType;
			}
		}

		public override bool IsGettable(PSAdaptedProperty property)
		{
			if (property != null)
			{
				ADEntity baseObject = property.BaseObject as ADEntity;
				if (baseObject != null)
				{
					PropertyInfo dotNetProperty = baseObject.GetDotNetProperty(property.Name);
					if (dotNetProperty == null)
					{
						return baseObject.PropertyIsReadable(property.Name);
					}
					else
					{
						return dotNetProperty.CanRead;
					}
				}
				else
				{
					object[] typeAdapterTypeName = new object[2];
					typeAdapterTypeName[0] = ADEntityAdapter.TypeAdapterTypeName;
					typeAdapterTypeName[1] = ADEntityAdapter.ADEntityTypeName;
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.TypeAdapterForADEntityOnly, typeAdapterTypeName));
				}
			}
			else
			{
				throw new ArgumentNullException("property");
			}
		}

		public override bool IsSettable(PSAdaptedProperty property)
		{
			if (property != null)
			{
				ADEntity baseObject = property.BaseObject as ADEntity;
				if (baseObject != null)
				{
					PropertyInfo dotNetProperty = baseObject.GetDotNetProperty(property.Name);
					if (dotNetProperty == null)
					{
						return baseObject.PropertyIsWritable(property.Name);
					}
					else
					{
						return dotNetProperty.CanWrite;
					}
				}
				else
				{
					object[] typeAdapterTypeName = new object[2];
					typeAdapterTypeName[0] = ADEntityAdapter.TypeAdapterTypeName;
					typeAdapterTypeName[1] = ADEntityAdapter.ADEntityTypeName;
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.TypeAdapterForADEntityOnly, typeAdapterTypeName));
				}
			}
			else
			{
				throw new ArgumentNullException("property");
			}
		}

		public override void SetPropertyValue(PSAdaptedProperty property, object valueObject)
		{
			Type realType;
			Type type;
			object obj = null;
			Exception exception = null;
			if (property != null)
			{
				ADEntity baseObject = property.BaseObject as ADEntity;
				if (baseObject != null)
				{
					PSObject pSObject = valueObject as PSObject;
					if (pSObject != null)
					{
						valueObject = pSObject.BaseObject;
					}
					PropertyInfo dotNetProperty = baseObject.GetDotNetProperty(property.Name);
					if (dotNetProperty == null)
					{
						if (this.IsSettable(property))
						{
							if (valueObject != null)
							{
								realType = this.GetRealType(baseObject.GetPropertyType(property.Name));
								type = this.GetRealType(valueObject.GetType());
								if (realType == typeof(object) || realType == type)
								{
									baseObject[property.Name].Value = valueObject;
									return;
								}
								else
								{
									if (baseObject.PropertyIsSingleValue(property.Name) || valueObject as ICollection == null)
									{
										baseObject[property.Name].Value = this.ConvertValue(property, valueObject, realType, type);
										return;
									}
									else
									{
										if (!this.TryConvertValue(property, valueObject, realType, type, out obj, out exception))
										{
											ICollection collections = valueObject as ICollection;
											ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection();
											foreach (object obj1 in collections)
											{
												if (obj1 == null)
												{
													continue;
												}
												Type type1 = obj1.GetType();
												if (type1 != realType)
												{
													aDPropertyValueCollection.Add(this.ConvertValue(property, obj1, realType, type1));
												}
												else
												{
													aDPropertyValueCollection.Add(obj1);
												}
											}
											baseObject[property.Name].Value = aDPropertyValueCollection;
											return;
										}
										else
										{
											baseObject[property.Name].Value = obj;
											return;
										}
									}
								}
							}
							else
							{
								baseObject[property.Name].Value = null;
								return;
							}
						}
						else
						{
							object[] name = new object[1];
							name[0] = property.Name;
							throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.PropertyIsReadonly, name));
						}
					}
					else
					{
						if (dotNetProperty.CanWrite)
						{
							if (valueObject != null)
							{
								realType = this.GetRealType(dotNetProperty.PropertyType);
								type = this.GetRealType(valueObject.GetType());
								if (realType == typeof(object) || realType == type)
								{
									dotNetProperty.SetValue(baseObject, valueObject, null);
									return;
								}
								else
								{
									dotNetProperty.SetValue(baseObject, this.ConvertValue(property, valueObject, realType, type), null);
									return;
								}
							}
							else
							{
								dotNetProperty.SetValue(baseObject, null, null);
								return;
							}
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = property.Name;
							throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.PropertyIsReadonly, objArray));
						}
					}
				}
				else
				{
					object[] typeAdapterTypeName = new object[2];
					typeAdapterTypeName[0] = ADEntityAdapter.TypeAdapterTypeName;
					typeAdapterTypeName[1] = ADEntityAdapter.ADEntityTypeName;
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.TypeAdapterForADEntityOnly, typeAdapterTypeName));
				}
			}
			else
			{
				throw new ArgumentNullException("property");
			}
		}

		private bool TryConvertValue(PSAdaptedProperty property, object valueObject, Type attrType, Type valType, out object newValue, out Exception conversionException)
		{
			bool flag;
			newValue = null;
			conversionException = null;
			if (!attrType.IsEnum)
			{
				if (!(attrType == typeof(string)) || !valType.IsPrimitive && !(valueObject is Guid) && valueObject as ADObject == null)
				{
					Type[] typeArray = new Type[1];
					typeArray[0] = valType;
					Type[] typeArray1 = typeArray;
					object[] objArray = new object[1];
					objArray[0] = valueObject;
					object[] objArray1 = objArray;
					ConstructorInfo constructor = attrType.GetConstructor(typeArray1);
					if (constructor != null)
					{
						try
						{
							newValue = constructor.Invoke(objArray1);
							flag = true;
							return flag;
						}
						catch (TargetInvocationException targetInvocationException1)
						{
							TargetInvocationException targetInvocationException = targetInvocationException1;
							conversionException = targetInvocationException.InnerException;
						}
						catch (TargetException targetException1)
						{
							TargetException targetException = targetException1;
							conversionException = targetException;
						}
					}
					if (attrType.IsPrimitive && valType.IsPrimitive)
					{
						object[] str = new object[1];
						str[0] = valueObject.ToString();
						objArray1 = str;
						Type[] typeArray2 = new Type[1];
						typeArray2[0] = typeof(string);
						typeArray1 = typeArray2;
					}
					MethodInfo method = attrType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, typeArray1, null);
					if (method != null)
					{
						try
						{
							newValue = method.Invoke(null, objArray1);
							flag = true;
							return flag;
						}
						catch (TargetInvocationException targetInvocationException3)
						{
							TargetInvocationException targetInvocationException2 = targetInvocationException3;
							conversionException = targetInvocationException2.InnerException;
						}
						catch (TargetException targetException3)
						{
							TargetException targetException2 = targetException3;
							conversionException = targetException2;
						}
					}
					return false;
				}
				else
				{
					newValue = valueObject.ToString();
					return true;
				}
			}
			else
			{
				if (!Utils.TryParseEnum(attrType, valueObject, out newValue))
				{
					object[] objArray2 = new object[2];
					objArray2[0] = valueObject;
					objArray2[1] = attrType;
					conversionException = new FormatException(string.Format(CultureInfo.CurrentCulture, StringResources.EnumConversionError, objArray2));
					return false;
				}
				else
				{
					return true;
				}
			}
		}
	}
}