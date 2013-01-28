using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADSchema
	{
		private static Dictionary<string, ADSchema> _schemaTable;

		private static object _schemaTableLock;

		private ADSessionInfo _sessionInfo;

		private string _subSchemaDN;

		private Dictionary<string, ADSchemaAttribute> _schemaProperties;

		private Dictionary<string, ADObject> _schemaClasses;

		private Dictionary<string, string> _schemaClassesDnHash;

		private HashSet<string> _userSubClasses;

		public Dictionary<string, ADObject> SchemaClasses
		{
			get
			{
				if (this._schemaClasses == null)
				{
					this.PopulateSchemaClasses();
				}
				return this._schemaClasses;
			}
		}

		public Dictionary<string, string> SchemaClassesDnHash
		{
			get
			{
				if (this._schemaClassesDnHash == null)
				{
					this.PopulateSchemaClasses();
				}
				return this._schemaClassesDnHash;
			}
		}

		public Dictionary<string, ADSchemaAttribute> SchemaProperties
		{
			get
			{
				return this._schemaProperties;
			}
		}

		static ADSchema()
		{
			ADSchema._schemaTable = new Dictionary<string, ADSchema>(StringComparer.OrdinalIgnoreCase);
			ADSchema._schemaTableLock = new object();
		}

		private ADSchema()
		{
		}

		public ADSchema(ADSessionInfo sessionInfo)
		{
			this._sessionInfo = sessionInfo.Copy();
			this.Init();
		}

		private void AddSchemaClassObjects(ADObjectSearcher searcher, ADSchema adSchema)
		{
			searcher.SchemaTranslation = false;
			ADRootDSE rootDSE = searcher.GetRootDSE();
			searcher.SearchRoot = rootDSE.SchemaNamingContext;
			IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "classSchema");
			IADOPathNode aDOPathNode1 = ADOPathUtil.CreateFilterClause(ADOperator.NotLike, "isDefunct", "*");
			IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
			aDOPathNodeArray[0] = aDOPathNode;
			aDOPathNodeArray[1] = aDOPathNode1;
			searcher.Filter = ADOPathUtil.CreateAndClause(aDOPathNodeArray);
			searcher.Scope = ADSearchScope.Subtree;
			searcher.PageSize = 0x100;
			searcher.Properties.Clear();
			searcher.Properties.Add("lDAPDisplayName");
			searcher.Properties.Add("subClassOf");
			searcher.Properties.Add("systemMayContain");
			searcher.Properties.Add("mayContain");
			searcher.Properties.Add("mustContain");
			searcher.Properties.Add("systemMustContain");
			searcher.Properties.Add("auxiliaryClass");
			searcher.Properties.Add("systemAuxiliaryClass");
			IEnumerable<ADObject> aDObjects = searcher.FindAll();
			foreach (ADObject aDObject in aDObjects)
			{
				if (!aDObject.Contains("lDAPDisplayName") || aDObject["lDAPDisplayName"].Value == null)
				{
					continue;
				}
				adSchema._schemaClasses.Add((string)aDObject["lDAPDisplayName"].Value, aDObject);
				adSchema._schemaClassesDnHash.Add((string)aDObject["distinguishedName"].Value, (string)aDObject["lDAPDisplayName"].Value);
			}
		}

		public ADAttributeSyntax GetPropertyType(string propertyName)
		{
			return this.GetPropertyType(propertyName, ADAttributeSyntax.DirectoryString);
		}

		public ADAttributeSyntax GetPropertyType(string propertyName, ADAttributeSyntax defaultSyntax)
		{
			ADSchemaAttribute aDSchemaAttribute = null;
			this._schemaProperties.TryGetValue(propertyName, out aDSchemaAttribute);
			if (aDSchemaAttribute == null)
			{
				return defaultSyntax;
			}
			else
			{
				return aDSchemaAttribute.Syntax;
			}
		}

		internal string GetRDNPrefix(string objectClass)
		{
			ADObject aDObject;
			ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(this._sessionInfo);
			using (aDObjectSearcher)
			{
				ADRootDSE rootDSE = aDObjectSearcher.GetRootDSE();
				aDObjectSearcher.SearchRoot = rootDSE.SchemaNamingContext;
				aDObjectSearcher.Properties.Add("rDNAttID");
				IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "classSchema");
				IADOPathNode aDOPathNode1 = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "lDAPDisplayName", objectClass);
				IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
				aDOPathNodeArray[0] = aDOPathNode;
				aDOPathNodeArray[1] = aDOPathNode1;
				IADOPathNode aDOPathNode2 = ADOPathUtil.CreateAndClause(aDOPathNodeArray);
				aDObjectSearcher.Filter = aDOPathNode2;
				aDObject = aDObjectSearcher.FindOne();
			}
			if (aDObject == null)
			{
				return null;
			}
			else
			{
				return (string)aDObject["rDNAttID"][0];
			}
		}

		internal HashSet<string> GetUserSubClasses()
		{
			return this._userSubClasses;
		}

		private HashSet<string> GetUserSubClasses(ADObjectSearcher searcher, ADRootDSE rootDSE)
		{
			HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			searcher.SearchRoot = rootDSE.SchemaNamingContext;
			searcher.Properties.Add("lDAPDisplayName");
			string str = string.Concat("CN=Person,", rootDSE.SchemaNamingContext);
			IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "classSchema");
			IADOPathNode aDOPathNode1 = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "subClassOf", "user");
			IADOPathNode aDOPathNode2 = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "defaultObjectCategory", str);
			IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
			aDOPathNodeArray[0] = aDOPathNode;
			IADOPathNode[] aDOPathNodeArray1 = new IADOPathNode[2];
			aDOPathNodeArray1[0] = aDOPathNode1;
			aDOPathNodeArray1[1] = aDOPathNode2;
			aDOPathNodeArray[1] = ADOPathUtil.CreateAndClause(aDOPathNodeArray1);
			IADOPathNode aDOPathNode3 = ADOPathUtil.CreateAndClause(aDOPathNodeArray);
			searcher.Filter = aDOPathNode3;
			IEnumerable<ADObject> aDObjects = searcher.FindAll();
			foreach (ADObject aDObject in aDObjects)
			{
				var ldapDisplayName = aDObject["lDAPDisplayName"];
				if (ldapDisplayName != null)
				{
					if (ldapDisplayName.Count > 0)
					{
						strs.Add((string)ldapDisplayName[0]);
					}
				}
			}
			strs.Add("user");
			return strs;
		}

		private void Init()
		{
			if (this._schemaProperties == null)
			{
				ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(this._sessionInfo);

				{
					ADRootDSE rootDSE = aDObjectSearcher.GetRootDSE();
					this._subSchemaDN = rootDSE.SubSchemaSubEntry;
					ADSchema aDSchema = null;
					lock (ADSchema._schemaTableLock)
					{
						ADSchema._schemaTable.TryGetValue(this._subSchemaDN, out aDSchema);
					}
					if (aDSchema != null)
					{
						this._schemaProperties = aDSchema._schemaProperties;
						this._userSubClasses = aDSchema._userSubClasses;
						this._schemaClasses = aDSchema._schemaClasses;
						this._schemaClassesDnHash = aDSchema._schemaClassesDnHash;
					}
					if (this._schemaProperties == null)
					{
						if (rootDSE.ServerType == ADServerType.ADDS && this._sessionInfo.ConnectedToGC)
						{
							this._sessionInfo.SetEffectivePort(LdapConstants.LDAP_PORT);
							aDObjectSearcher.Dispose();
							aDObjectSearcher = new ADObjectSearcher(this._sessionInfo);
						}
						this._schemaProperties = new Dictionary<string, ADSchemaAttribute>(1, StringComparer.OrdinalIgnoreCase);
						this.ReadConstructedSchema(aDObjectSearcher, this);
						this.ReadObjectSchema(aDObjectSearcher, this);
						this._userSubClasses = this.GetUserSubClasses(aDObjectSearcher, rootDSE);
					}
					else
					{
						return;
					}
				}
				aDObjectSearcher.Dispose ();
				lock (ADSchema._schemaTableLock)
				{
					if (ADSchema._schemaTable.ContainsKey(this._subSchemaDN))
					{
						ADSchema._schemaTable.Remove(this._subSchemaDN);
					}
					ADSchema._schemaTable.Add(this._subSchemaDN, this);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void PopulateSchemaClasses()
		{
			if (this._schemaClasses == null)
			{
				ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(this._sessionInfo);
				{
					ADRootDSE rootDSE = aDObjectSearcher.GetRootDSE();
					this._subSchemaDN = rootDSE.SubSchemaSubEntry;
					if (rootDSE.ServerType == ADServerType.ADDS && this._sessionInfo.ConnectedToGC)
					{
						this._sessionInfo.SetEffectivePort(LdapConstants.LDAP_PORT);
						aDObjectSearcher.Dispose();
						aDObjectSearcher = new ADObjectSearcher(this._sessionInfo);
					}
					this._schemaClasses = new Dictionary<string, ADObject>(1, StringComparer.OrdinalIgnoreCase);
					this._schemaClassesDnHash = new Dictionary<string, string>(1, StringComparer.OrdinalIgnoreCase);
					this.AddSchemaClassObjects(aDObjectSearcher, this);
				}
				aDObjectSearcher.Dispose ();
				lock (ADSchema._schemaTableLock)
				{
					ADSchema aDSchema = null;
					lock (ADSchema._schemaTableLock)
					{
						ADSchema._schemaTable.TryGetValue(this._subSchemaDN, out aDSchema);
						if (aDSchema != null)
						{
							aDSchema._schemaClasses = this._schemaClasses;
							aDSchema._schemaClassesDnHash = this._schemaClassesDnHash;
						}
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void ReadConstructedSchema(ADObjectSearcher searcher, ADSchema adSchema)
		{
			searcher.SchemaTranslation = false;
			ADRootDSE rootDSE = searcher.GetRootDSE();
			searcher.SearchRoot = rootDSE.SubSchemaSubEntry;
			searcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
			searcher.Scope = ADSearchScope.Base;
			searcher.Properties.Clear();
			searcher.Properties.Add("extendedAttributeInfo");
			searcher.Properties.Add("attributeTypes");
			ADObject aDObject = searcher.FindOne();
			int success = SchemaConstants.AttributeTypesRegex.GroupNumberFromName(SchemaConstants.NameGroup);
			int num = SchemaConstants.AttributeTypesRegex.GroupNumberFromName(SchemaConstants.SyntaxGroup);
			int num1 = SchemaConstants.AttributeTypesRegex.GroupNumberFromName(SchemaConstants.SingleValueGroup);
			adSchema._schemaProperties = new Dictionary<string, ADSchemaAttribute>(1, StringComparer.OrdinalIgnoreCase);
			foreach (string item in aDObject["attributeTypes"])
			{
				Match match = SchemaConstants.AttributeTypesRegex.Match(item);
				if (match != null)
				{
					if (!match.Groups[success].Success)
					{
						DebugLogger.LogError("adschema", string.Format("AttributeType {0} no match on Name", item));
					}
					if (!match.Groups[num].Success)
					{
						DebugLogger.LogError("adschema", string.Format("AttributeType {0} no match on Syntax", item));
					}
					adSchema._schemaProperties.Add(match.Groups[success].Value, new ADSchemaAttribute(ADSyntax.OIDToSyntax(match.Groups[num].Value), match.Groups[num1].Success, false));
				}
				else
				{
					DebugLogger.LogError("adschema", string.Format("unable to match AttributeType {0}", item));
					throw new ADException();
				}
			}
			success = SchemaConstants.ExtendedAttrInfoRegex.GroupNumberFromName(SchemaConstants.NameGroup);
			int num2 = SchemaConstants.ExtendedAttrInfoRegex.GroupNumberFromName(SchemaConstants.SystemOnlyGroup);
			foreach (string str in aDObject["extendedAttributeInfo"])
			{
				Match match1 = SchemaConstants.ExtendedAttrInfoRegex.Match(str);
				adSchema._schemaProperties[match1.Groups[success].Value].IsSystemOnly = match1.Groups[num2].Success;
			}
		}

		private void ReadObjectSchema(ADObjectSearcher searcher, ADSchema adSchema)
		{
			searcher.SchemaTranslation = false;
			ADRootDSE rootDSE = searcher.GetRootDSE();
			searcher.SearchRoot = rootDSE.SchemaNamingContext;
			IADOPathNode[] aDOPathNodeArray = new IADOPathNode[3];
			aDOPathNodeArray[0] = ADOPathUtil.CreateNotClause(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "isDefunct", true));
			aDOPathNodeArray[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "attributeSchema");
			IADOPathNode[] aDOPathNodeArray1 = new IADOPathNode[3];
			aDOPathNodeArray1[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "attributeSyntax", SchemaConstants.SidAttributeSyntax);
			aDOPathNodeArray1[1] = ADOPathUtil.CreateFilterClause(ADOperator.Like, "linkID", "*");
			aDOPathNodeArray1[2] = ADOPathUtil.CreateFilterClause(ADOperator.Band, "systemFlags", SchemaConstants.systemFlagsConstructedBitMask);
			aDOPathNodeArray[2] = ADOPathUtil.CreateOrClause(aDOPathNodeArray1);
			searcher.Filter = ADOPathUtil.CreateAndClause(aDOPathNodeArray);
			searcher.Scope = ADSearchScope.Subtree;
			searcher.PageSize = 0x100;
			searcher.Properties.Clear();
			searcher.Properties.Add("lDAPDisplayName");
			searcher.Properties.Add("linkID");
			searcher.Properties.Add("systemFlags");
			searcher.Properties.Add("attributeSyntax");
			IEnumerable<ADObject> aDObjects = searcher.FindAll();

			foreach (ADObject nullable in aDObjects)
			{
				if (adSchema._schemaProperties.ContainsKey ((string)nullable["lDAPDisplayName"].Value))
				{
					if (nullable.Contains("linkID"))
					{
						adSchema._schemaProperties[(string)nullable["lDAPDisplayName"].Value].LinkID = new int?(int.Parse(nullable["linkID"].Value as string, NumberFormatInfo.InvariantInfo));
					}
					if (nullable.Contains("systemFlags") && (long)0 != (ulong.Parse(nullable["systemFlags"].Value as string, NumberFormatInfo.InvariantInfo) & SchemaConstants.systemFlagsConstructedBitMask))
					{
						adSchema._schemaProperties[(string)nullable["lDAPDisplayName"].Value].IsConstructed = true;
					}
					if (!nullable.Contains("attributeSyntax") || string.Compare(nullable["attributeSyntax"].Value as string, SchemaConstants.SidAttributeSyntax, true) != 0)
					{
						continue;
					}
					adSchema._schemaProperties[(string)nullable["lDAPDisplayName"].Value].Syntax = ADAttributeSyntax.Sid;
				}
			}
		}
	}
}