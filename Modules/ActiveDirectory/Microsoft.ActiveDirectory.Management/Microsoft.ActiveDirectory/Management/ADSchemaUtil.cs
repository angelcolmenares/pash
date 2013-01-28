using Microsoft.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADSchemaUtil
	{
		private const string _debugCategory = "ADSchemaUtil";

		private ADSessionInfo _sessionInfo;

		private ADSchema _adSchema;

		private ADTypeConverter _adTypeConverter;

		private ADSchemaUtil()
		{
		}

		internal ADSchemaUtil(ADSessionInfo sessionInfo)
		{
			this._sessionInfo = sessionInfo;
			this.Init();
		}

		internal bool AttributeIsSingleValue(string attribute)
		{
			Dictionary<string, ADSchemaAttribute> schemaProperties = this._adSchema.SchemaProperties;
			if (schemaProperties == null || !schemaProperties.ContainsKey(attribute))
			{
				return false;
			}
			else
			{
				return schemaProperties[attribute].IsSingleValued;
			}
		}

		internal bool AttributeIsWritable(string attribute)
		{
			Dictionary<string, ADSchemaAttribute> schemaProperties = this._adSchema.SchemaProperties;
			if (schemaProperties == null || !schemaProperties.ContainsKey(attribute))
			{
				return true;
			}
			else
			{
				if (schemaProperties[attribute].IsSystemOnly || schemaProperties[attribute].IsBackLink)
				{
					return false;
				}
				else
				{
					return !schemaProperties[attribute].IsConstructed;
				}
			}
		}

		internal HashSet<string> GetAllParentClassesForSchemaClass(string schemaClassName)
		{
			Dictionary<string, ADObject> schemaClasses = this._adSchema.SchemaClasses;
			if (schemaClasses.ContainsKey(schemaClassName))
			{
				ADObject item = schemaClasses[schemaClassName];
				if (!item.Contains(SchemaConstants.ParentClasslist))
				{
					HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					HashSet<string> strs1 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					if (item.Contains("subClassOf"))
					{
						string value = (string)item["subClassOf"].Value;
						if (string.Compare(value, schemaClassName, StringComparison.OrdinalIgnoreCase) != 0)
						{
							strs1.Add(value);
						}
					}
					if (item.Contains("auxiliaryClass"))
					{
						string[] valueList = item["auxiliaryClass"].ValueList as string[];
						strs1.UnionWith(valueList);
					}
					if (item.Contains("systemAuxiliaryClass"))
					{
						string[] strArrays = item["systemAuxiliaryClass"].ValueList as string[];
						strs1.UnionWith(strArrays);
					}
					strs.UnionWith(strs1);
					foreach (string str in strs1)
					{
						if (schemaClasses.ContainsKey(str))
						{
							HashSet<string> allParentClassesForSchemaClass = this.GetAllParentClassesForSchemaClass(str);
							strs.UnionWith(allParentClassesForSchemaClass);
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = str;
							throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.NoSchemaClassInSchemaCache, objArray));
						}
					}
					strs.Add(schemaClassName);
					ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(new HashSet<string>(strs, StringComparer.OrdinalIgnoreCase));
					item[SchemaConstants.ParentClasslist].Value = aDPropertyValueCollection;
					return strs;
				}
				else
				{
					return new HashSet<string>(item[SchemaConstants.ParentClasslist].Value as HashSet<string>, StringComparer.OrdinalIgnoreCase);
				}
			}
			else
			{
				object[] objArray1 = new object[1];
				objArray1[0] = schemaClassName;
				throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.NoSchemaClassInSchemaCache, objArray1));
			}
		}

		internal HashSet<string> GetAllParentClassesForSchemaClassDN(string schemaClassDN)
		{
			Dictionary<string, string> schemaClassesDnHash = this._adSchema.SchemaClassesDnHash;
			if (schemaClassesDnHash.ContainsKey(schemaClassDN))
			{
				return this.GetAllParentClassesForSchemaClass(schemaClassesDnHash[schemaClassDN]);
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = schemaClassDN;
				throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.NoSchemaClassInSchemaCache, objArray));
			}
		}

		internal Type GetAttributeDotNetType(string attribute, Type defaultType)
		{
			if (!this._adSchema.SchemaProperties.ContainsKey(attribute))
			{
				return defaultType;
			}
			else
			{
				return this._adTypeConverter.GetDotNetPropertyType(attribute);
			}
		}

		internal HashSet<string> GetAttributeListForSchemaClass(string schemaClassName)
		{
			Dictionary<string, ADObject> schemaClasses = this._adSchema.SchemaClasses;
			if (schemaClasses.ContainsKey(schemaClassName))
			{
				ADObject item = schemaClasses[schemaClassName];
				if (!item.Contains(SchemaConstants.Attributelist))
				{
					HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					HashSet<string> allParentClassesForSchemaClass = this.GetAllParentClassesForSchemaClass(schemaClassName);
					allParentClassesForSchemaClass.Add(schemaClassName);
					string[] strArrays = new string[4];
					strArrays[0] = "systemMayContain";
					strArrays[1] = "mayContain";
					strArrays[2] = "mustContain";
					strArrays[3] = "systemMustContain";
					string[] strArrays1 = strArrays;
					foreach (string str in allParentClassesForSchemaClass)
					{
						ADObject aDObject = schemaClasses[str];
						string[] strArrays2 = strArrays1;
						for (int i = 0; i < (int)strArrays2.Length; i++)
						{
							string str1 = strArrays2[i];
							if (aDObject.Contains(str1))
							{
								string[] valueList = aDObject[str1].ValueList as string[];
								strs.UnionWith(valueList);
							}
						}
					}
					ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(new HashSet<string>(strs, StringComparer.OrdinalIgnoreCase));
					item[SchemaConstants.Attributelist].Value = aDPropertyValueCollection;
					return strs;
				}
				else
				{
					return new HashSet<string>(item[SchemaConstants.Attributelist].Value as HashSet<string>, StringComparer.OrdinalIgnoreCase);
				}
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = schemaClassName;
				throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.NoSchemaClassInSchemaCache, objArray));
			}
		}

		internal HashSet<string> GetAttributeListForSchemaClassDN(string schemaClassDN)
		{
			Dictionary<string, string> schemaClassesDnHash = this._adSchema.SchemaClassesDnHash;
			if (schemaClassesDnHash.ContainsKey(schemaClassDN))
			{
				return this.GetAttributeListForSchemaClass(schemaClassesDnHash[schemaClassDN]);
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = schemaClassDN;
				throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.NoSchemaClassInSchemaCache, objArray));
			}
		}

		internal string GetRDNPrefix(string objectClass)
		{
			return this._adSchema.GetRDNPrefix(objectClass);
		}

		internal HashSet<string> GetUserSubClasses()
		{
			return this._adSchema.GetUserSubClasses();
		}

		private void Init()
		{
			if (this._adSchema == null)
			{
				this._adSchema = new ADSchema(this._sessionInfo);
			}
			if (this._adTypeConverter == null)
			{
				this._adTypeConverter = new ADTypeConverter(this._sessionInfo);
			}
		}
	}
}