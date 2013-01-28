using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public abstract class ADFactory<T> : ADFactoryBase<T>
	where T : ADEntity, new()
	{
		private const string _debugCategory = "ADFactory";

		private readonly static IdentityResolverDelegate[] _identityResolvers;

		private ADFactory<T>.FactoryCommitSubroutinePipeline _preCommitPipeline;

		private ADFactory<T>.FactoryCommitSubroutinePipeline _postCommitPipeline;

		internal virtual IdentityResolverDelegate[] IdentityResolvers
		{
			get
			{
				return ADFactory<T>._identityResolvers;
			}
		}

		internal ADFactory<T>.FactoryCommitSubroutinePipeline PostCommitPipeline
		{
			get
			{
				return this._postCommitPipeline;
			}
		}

		internal ADFactory<T>.FactoryCommitSubroutinePipeline PreCommitPipeline
		{
			get
			{
				return this._preCommitPipeline;
			}
		}

		internal abstract string RDNPrefix
		{
			get;
		}

		internal abstract string StructuralObjectClass
		{
			get;
		}

		internal abstract IADOPathNode StructuralObjectFilter
		{
			get;
		}

		static ADFactory()
		{
			ADFactory<T>._identityResolvers = new IdentityResolverDelegate[0];
		}

		internal ADFactory()
		{
			this._preCommitPipeline = new ADFactory<T>.FactoryCommitSubroutinePipeline();
			this._postCommitPipeline = new ADFactory<T>.FactoryCommitSubroutinePipeline();
		}

		internal virtual IADOPathNode BuildIdentityFilter(T identityObj)
		{
			IADOPathNode aDOPathNode;
			if (identityObj != null)
			{
				if (base.CmdletSessionInfo != null)
				{
					//ADFactoryBase<T>.AttributeTable[base.ConnectedStore];
					if (identityObj.Identity == null)
					{
						aDOPathNode = this.IdentitySearchConverter(identityObj);
					}
					else
					{
						aDOPathNode = this.IdentitySearchConverter(identityObj.Identity);
					}
					return aDOPathNode;
				}
				else
				{
					throw new ArgumentNullException(StringResources.SessionRequired);
				}
			}
			else
			{
				throw new ArgumentNullException("Identity");
			}
		}

		private void CheckInstanceForUpdateCollisions(T instance, string extendedAttribute, IEnumerable<string> directoryAttributes)
		{
			if (directoryAttributes != null)
			{
				foreach (string directoryAttribute in directoryAttributes)
				{
					if (extendedAttribute.Equals(directoryAttribute, StringComparison.OrdinalIgnoreCase) || !this.PropertyHasChange(directoryAttribute, instance, null, ADFactory<T>.DirectoryOperation.Update))
					{
						continue;
					}
					object[] objArray = new object[2];
					objArray[0] = directoryAttribute;
					objArray[1] = extendedAttribute;
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.CustomAttributeCollision, objArray));
				}
			}
		}

		internal void ClearProperty(string parameter, T instance, ADParameterSet paramSet)
		{
			if (paramSet.Contains(parameter))
			{
				paramSet.RemoveParameter(parameter);
			}
			if (instance != null)
			{
				if (instance.ModifiedProperties.Contains(parameter))
				{
					instance.ModifiedProperties.Remove(parameter);
				}
				if (instance.AddedProperties.Contains(parameter))
				{
					instance.AddedProperties.Remove(parameter);
				}
				if (instance.RemovedProperties.Contains(parameter))
				{
					instance.RemovedProperties.Remove(parameter);
				}
				if (instance.Contains(parameter))
				{
					instance.Remove(parameter);
				}
			}
		}

		internal virtual T Construct(ADEntity directoryObj, AttributeSetRequest requestedAttributes)
		{
			AttributeConverterEntry attributeConverterEntry = null;
			if (base.CmdletSessionInfo != null)
			{
				T sessionInfo = Activator.CreateInstance<T>();
				sessionInfo.IsSearchResult = true;
				sessionInfo.SessionInfo = directoryObj.SessionInfo;
				MappingTable<AttributeConverterEntry> item = ADFactoryBase<T>.AttributeTable[base.ConnectedStore];
				if (requestedAttributes.ReturnAll || requestedAttributes.ReturnDefault)
				{
					foreach (AttributeConverterEntry value in item.Values)
					{
						if (!value.IsExtendedConverterDefined || requestedAttributes.ReturnDefault && (!requestedAttributes.ReturnDefault || value.AttributeSet != AttributeSet.Default))
						{
							continue;
						}
						value.InvokeToExtendedConverter(sessionInfo, directoryObj, base.CmdletSessionInfo);
					}
				}
				if (requestedAttributes.ReturnDefault)
				{
					foreach (string extendedAttribute in requestedAttributes.ExtendedAttributes)
					{
						if (!item.TryGetValue(extendedAttribute, out attributeConverterEntry) || !attributeConverterEntry.IsExtendedConverterDefined)
						{
							continue;
						}
						attributeConverterEntry.InvokeToExtendedConverter(sessionInfo, directoryObj, base.CmdletSessionInfo);
					}
				}
				foreach (string customAttribute in requestedAttributes.CustomAttributes)
				{
					if (sessionInfo.Contains(customAttribute) || !directoryObj.Contains(customAttribute))
					{
						continue;
					}
					ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(directoryObj[customAttribute]);
					sessionInfo.Add(customAttribute, aDPropertyValueCollection);
				}
				if (requestedAttributes.ReturnAll)
				{
					foreach (string propertyName in directoryObj.PropertyNames)
					{
						if (sessionInfo.Contains(propertyName))
						{
							continue;
						}
						ADPropertyValueCollection aDPropertyValueCollection1 = new ADPropertyValueCollection(directoryObj[propertyName]);
						sessionInfo.Add(propertyName, aDPropertyValueCollection1);
					}
				}
				string[] strArrays = DefaultAttributes.attributes;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					if (directoryObj.Contains(str))
					{
						if (sessionInfo.Contains(str))
						{
							sessionInfo[str].Value = directoryObj[str].Value;
						}
						else
						{
							ADPropertyValueCollection aDPropertyValueCollection2 = new ADPropertyValueCollection(directoryObj[str]);
							sessionInfo.Add(str, aDPropertyValueCollection2);
						}
					}
				}
				foreach (string propertyName1 in directoryObj.InternalProperties.PropertyNames)
				{
					if (sessionInfo.InternalProperties.Contains(propertyName1))
					{
						continue;
					}
					sessionInfo.InternalProperties.SetValue(propertyName1, directoryObj.InternalProperties[propertyName1]);
				}
				sessionInfo.TrackChanges = true;
				return sessionInfo;
			}
			else
			{
				throw new ArgumentNullException(StringResources.SessionRequired);
			}
		}

		internal virtual AttributeSetRequest ConstructAttributeSetRequest(AttributeSet set)
		{
			MappingTable<AttributeConverterEntry> item = ADFactoryBase<T>.AttributeTable[base.ConnectedStore];
			AttributeSetRequest attributeSetRequest = new AttributeSetRequest();
			foreach (AttributeConverterEntry value in item.Values)
			{
				if (value.AttributeSet != set)
				{
					continue;
				}
				attributeSetRequest.ExtendedAttributes.Add(value.ExtendedAttribute);
				if (value.DirectoryAttributes == null)
				{
					continue;
				}
				string[] directoryAttributes = value.DirectoryAttributes;
				for (int i = 0; i < (int)directoryAttributes.Length; i++)
				{
					string str = directoryAttributes[i];
					if (str != null && !attributeSetRequest.DirectoryAttributes.Contains(str))
					{
						attributeSetRequest.DirectoryAttributes.Add(str);
					}
				}
			}
			return attributeSetRequest;
		}

		internal virtual AttributeSetRequest ConstructAttributeSetRequest(ICollection<string> requestedExtendedAttr)
		{
			AttributeConverterEntry attributeConverterEntry = null;
			if (base.CmdletSessionInfo != null)
			{
				MappingTable<AttributeConverterEntry> item = ADFactoryBase<T>.AttributeTable[base.ConnectedStore];
				AttributeSetRequest attributeSetRequest = new AttributeSetRequest();
				HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				if (requestedExtendedAttr == null || requestedExtendedAttr.Count == 0)
				{
					attributeSetRequest.ReturnDefault = true;
				}
				else
				{
					bool flag = false;
					foreach (string str in requestedExtendedAttr)
					{
						if (!item.TryGetValue(str, out attributeConverterEntry))
						{
							if (!string.Equals(str, "*", StringComparison.OrdinalIgnoreCase))
							{
								strs.Add(str);
								attributeSetRequest.CustomAttributes.Add(str);
							}
							else
							{
								if (!flag)
								{
									flag = true;
									foreach (AttributeConverterEntry value in item.Values)
									{
										if (value.DirectoryAttributes == null)
										{
											continue;
										}
										string[] directoryAttributes = value.DirectoryAttributes;
										for (int i = 0; i < (int)directoryAttributes.Length; i++)
										{
											string str1 = directoryAttributes[i];
											if (str1 != null && !strs.Contains(str1))
											{
												strs.Add(str1);
											}
										}
									}
									strs.Add("*");
									attributeSetRequest.ReturnAll = true;
								}
								else
								{
									throw new ArgumentException(StringResources.MultipleKeywords);
								}
							}
						}
						else
						{
							attributeSetRequest.ExtendedAttributes.Add(str);
							if (attributeConverterEntry.DirectoryAttributes == null)
							{
								continue;
							}
							string[] strArrays = attributeConverterEntry.DirectoryAttributes;
							for (int j = 0; j < (int)strArrays.Length; j++)
							{
								string str2 = strArrays[j];
								strs.Add(str2);
							}
						}
					}
					if (!attributeSetRequest.ReturnAll)
					{
						attributeSetRequest.ReturnDefault = true;
					}
				}
				if (attributeSetRequest.ReturnDefault)
				{
					foreach (AttributeConverterEntry value1 in item.Values)
					{
						if (value1.AttributeSet != AttributeSet.Default || value1.DirectoryAttributes == null)
						{
							continue;
						}
						string[] directoryAttributes1 = value1.DirectoryAttributes;
						for (int k = 0; k < (int)directoryAttributes1.Length; k++)
						{
							string str3 = directoryAttributes1[k];
							strs.Add(str3);
						}
						if (!attributeSetRequest.ExtendedAttributes.Contains(value1.ExtendedAttribute))
						{
							continue;
						}
						attributeSetRequest.ExtendedAttributes.Remove(value1.ExtendedAttribute);
					}
				}
				string[] strArrays1 = DefaultAttributes.attributes;
				int num = 0;
				while (num < (int)strArrays1.Length)
				{
					string str4 = strArrays1[num];
					strs.Add(str4);
					if (attributeSetRequest.ExtendedAttributes.Contains(str4))
					{
						attributeSetRequest.ExtendedAttributes.Remove(str4);
					}
					num++;
				}
				attributeSetRequest.DirectoryAttributes = new List<string>(strs);
				return attributeSetRequest;
			}
			else
			{
				throw new ArgumentNullException(StringResources.SessionRequired);
			}
		}

		internal virtual string GenerateObjectName(ADParameterSet parameters)
		{
			if (parameters.Contains("Name"))
			{
				return parameters["Name"] as string;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = "Name";
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray));
			}
		}

		private static object GetCustomPropertyValue(Hashtable ht, string propertyName)
		{
			object item = ht[propertyName];
			PSObject pSObject = item as PSObject;
			if (pSObject != null)
			{
				item = pSObject.BaseObject;
			}
			return item;
		}

		internal virtual ADObject GetDirectoryObjectFromIdentity(T identityObj, string searchRoot, bool showDeleted)
		{
			ADObject objectFromIdentitySearcher;
			ADObject aDObject = (object)identityObj as ADObject;
			if (aDObject != null)
			{
				if (base.CmdletSessionInfo != null)
				{
					if (aDObject.IsSearchResult && aDObject.Contains("isDeleted"))
					{
						showDeleted = true;
					}
					if (!aDObject.IsSearchResult || !aDObject.Contains("distinguishedName"))
					{
						ADObjectSearcher aDObjectSearcherFromIdentity = ADFactoryUtil.GetADObjectSearcherFromIdentity(identityObj, searchRoot, showDeleted, this.StructuralObjectFilter, this.BuildIdentityFilter(identityObj), this.IdentityResolvers, base.CmdletSessionInfo);
						AttributeSetRequest attributeSetRequest = this.ConstructAttributeSetRequest(null);
						string[] strArrays = new string[0];
						objectFromIdentitySearcher = ADFactoryUtil.GetObjectFromIdentitySearcher(aDObjectSearcherFromIdentity, identityObj, searchRoot, attributeSetRequest, base.CmdletSessionInfo, out strArrays);
						objectFromIdentitySearcher.TrackChanges = true;
						objectFromIdentitySearcher.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
						string[] strArrays1 = strArrays;
						for (int i = 0; i < (int)strArrays1.Length; i++)
						{
							string str = strArrays1[i];
							base.CmdletSessionInfo.CmdletMessageWriter.WriteWarningBuffered(str);
						}
						return objectFromIdentitySearcher;
					}
					else
					{
						DebugLogger.LogInfo("ADFactory", string.Format("GetDirectoryObjectFromIdentity: Creating directory object: {0} by copying from identity", aDObject.DistinguishedName));
						this.ValidateObjectClass(identityObj);
						objectFromIdentitySearcher = Utils.CreateIdentityCopy(aDObject);
						string[] internalLdapAttributes = DirectoryAttrConstants.InternalLdapAttributes;
						for (int j = 0; j < (int)internalLdapAttributes.Length; j++)
						{
							string str1 = internalLdapAttributes[j];
							ADPropertyValueCollection aDPropertyValueCollection = null;
							if (!objectFromIdentitySearcher.Contains(str1) && aDObject.InternalProperties.TryGetValue(str1, out aDPropertyValueCollection) && aDPropertyValueCollection.Value != null)
							{
								objectFromIdentitySearcher.SetValue(str1, aDPropertyValueCollection.Value);
							}
						}
						objectFromIdentitySearcher.TrackChanges = true;
						objectFromIdentitySearcher.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
						objectFromIdentitySearcher.IsSearchResult = true;
						return objectFromIdentitySearcher;
					}
				}
				else
				{
					throw new ArgumentNullException(StringResources.SessionRequired);
				}
			}
			else
			{
				object[] type = new object[2];
				type[0] = "GetDirectoryObjectFromIdentity";
				type[1] = identityObj.GetType();
				throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.MethodNotSupportedForObjectType, type));
			}
		}

		internal virtual IEnumerable<T> GetExtendedObjectFromFilter(IADOPathNode filter, string searchBase, ADSearchScope searchScope, ICollection<string> propertiesToFetch, int? resultSetSize, int? pageSize, bool showDeleted)
		{
			T iteratorVariable0 = Activator.CreateInstance<T>();
			ADObject iteratorVariable1 = iteratorVariable0 as ADObject;
			if (iteratorVariable1 == null)
			{
				throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.MethodNotSupportedForObjectType, new object[] { "GetExtendedObjectFromFilter", iteratorVariable0.GetType() }));
			}
			if (this.CmdletSessionInfo == null)
			{
				throw new ArgumentNullException(StringResources.SessionRequired);
			}
			int iteratorVariable2 = 0;
			AttributeSetRequest requestedAttributes = this.ConstructAttributeSetRequest(propertiesToFetch);
			using (ADObjectSearcher iteratorVariable4 = SearchUtility.BuildSearcher(this.CmdletSessionInfo.ADSessionInfo, searchBase, searchScope))
			{
				if (ADObjectSearcher.IsDefaultSearchFilter(this.StructuralObjectFilter.GetLdapFilterString()))
				{
					iteratorVariable4.Filter = filter;
				}
				else
				{
					iteratorVariable4.Filter = ADOPathUtil.CreateAndClause(new IADOPathNode[] { filter, this.StructuralObjectFilter });
				}
				if (pageSize.HasValue && pageSize.HasValue)
				{
					iteratorVariable4.PageSize = pageSize.Value;
				}
				if (resultSetSize.HasValue && resultSetSize.HasValue)
				{
					iteratorVariable4.SizeLimit = resultSetSize.Value;
				}
				if (showDeleted)
				{
					iteratorVariable4.ShowDeleted = true;
					iteratorVariable4.ShowDeactivatedLink = true;
				}
				iteratorVariable4.Properties.AddRange(requestedAttributes.DirectoryAttributes);
				DebugLogger.LogInfo("ADFactory", string.Format("ADFactory: GetExtendedObjectFromFilter: Searching for identities using filter: {0} searchbase: {1} scope: {2}", iteratorVariable4.Filter.GetLdapFilterString(), iteratorVariable4.SearchRoot, iteratorVariable4.Scope));
				foreach (ADObject iteratorVariable5 in iteratorVariable4.FindAll())
				{
					T iteratorVariable6 = this.Construct(iteratorVariable5, requestedAttributes);
					iteratorVariable6.SessionInfo = this.CmdletSessionInfo.ADSessionInfo;
					yield return iteratorVariable6;
					if ((resultSetSize.HasValue && resultSetSize.HasValue) && (++iteratorVariable2 == resultSetSize.Value))
					{
						break;
					}
				}
			}
		}
		
		
		


		internal ADObject GetDirectoryObjectFromIdentity(T identityObj, string searchRoot)
		{
			return this.GetDirectoryObjectFromIdentity(identityObj, searchRoot, false);
		}

		internal T GetExtendedObjectFromDN(string distinguishedName)
		{
			return this.GetExtendedObjectFromDN(distinguishedName, null, false);
		}

		internal T GetExtendedObjectFromDN(string distinguishedName, bool showDeleted)
		{
			return this.GetExtendedObjectFromDN(distinguishedName, null, showDeleted);
		}

		internal T GetExtendedObjectFromDN(string distinguishedName, ICollection<string> propertiesToFetch)
		{
			return this.GetExtendedObjectFromDN(distinguishedName, propertiesToFetch, false);
		}

		internal virtual T GetExtendedObjectFromDN(string distinguishedName, ICollection<string> propertiesToFetch, bool showDeleted)
		{
			ADObject aDObject;
			T t = Activator.CreateInstance<T>();
			if ((object)t as ADObject == null)
			{
				t.Identity = distinguishedName;
				return this.GetExtendedObjectFromIdentity(t, distinguishedName, propertiesToFetch);
			}
			else
			{
				AttributeSetRequest attributeSetRequest = this.ConstructAttributeSetRequest(propertiesToFetch);
				ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(base.CmdletSessionInfo.ADSessionInfo, distinguishedName, ADSearchScope.Base, showDeleted);
				using (aDObjectSearcher)
				{
					aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
					aDObjectSearcher.Properties.AddRange(attributeSetRequest.DirectoryAttributes);
					DebugLogger.LogInfo("ADFactory", string.Format("ADFactory: GetExtendedObjectFromDN: Searching for identity using filter: {0} searchbase: {1} scope: {2}", aDObjectSearcher.Filter.GetLdapFilterString(), aDObjectSearcher.SearchRoot, aDObjectSearcher.Scope));
					aDObject = aDObjectSearcher.FindOne();
				}
				if (aDObject == null)
				{
					DebugLogger.LogInfo("ADFactory", string.Format("ADFactory: GetExtendedObjectFromDN: Identity not found", new object[0]));
					object[] objArray = new object[2];
					objArray[0] = distinguishedName;
					objArray[1] = distinguishedName;
					throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityNotFound, objArray));
				}
				else
				{
					T aDSessionInfo = this.Construct(aDObject, attributeSetRequest);
					aDSessionInfo.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
					return aDSessionInfo;
				}
			}
		}

		internal T GetExtendedObjectFromIdentity(T identityObj, string identityQueryPath)
		{
			return this.GetExtendedObjectFromIdentity(identityObj, identityQueryPath, null, false);
		}

		internal T GetExtendedObjectFromIdentity(T identityObj, string identityQueryPath, bool showDeleted)
		{
			return this.GetExtendedObjectFromIdentity(identityObj, identityQueryPath, null, showDeleted);
		}

		internal T GetExtendedObjectFromIdentity(T identityObj, string identityQueryPath, ICollection<string> propertiesToFetch)
		{
			return this.GetExtendedObjectFromIdentity(identityObj, identityQueryPath, propertiesToFetch, false);
		}

		internal virtual T GetExtendedObjectFromIdentity(T identityObj, string identityQueryPath, ICollection<string> propertiesToFetch, bool showDeleted)
		{
			ADObject aDObject = (object)identityObj as ADObject;
			if (aDObject != null)
			{
				if (base.CmdletSessionInfo != null)
				{
					AttributeSetRequest attributeSetRequest = this.ConstructAttributeSetRequest(propertiesToFetch);
					ADObjectSearcher aDObjectSearcherFromIdentity = null;
					if (!aDObject.IsSearchResult)
					{
						aDObjectSearcherFromIdentity = ADFactoryUtil.GetADObjectSearcherFromIdentity(identityObj, identityQueryPath, showDeleted, this.StructuralObjectFilter, this.BuildIdentityFilter(identityObj), this.IdentityResolvers, base.CmdletSessionInfo);
					}
					else
					{
						this.ValidateObjectClass(identityObj);
						if (!string.IsNullOrEmpty(aDObject.DistinguishedName))
						{
							aDObjectSearcherFromIdentity = SearchUtility.BuildSearcher(base.CmdletSessionInfo.ADSessionInfo, aDObject.DistinguishedName, ADSearchScope.Base, showDeleted);
							aDObjectSearcherFromIdentity.Filter = this.StructuralObjectFilter;
							aDObjectSearcherFromIdentity.ShowDeleted = showDeleted;
						}
						else
						{
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.DistinguishedNameCannotBeNull, new object[0]));
						}
					}
					string[] strArrays = new string[0];
					ADObject objectFromIdentitySearcher = ADFactoryUtil.GetObjectFromIdentitySearcher(aDObjectSearcherFromIdentity, identityObj, identityQueryPath, attributeSetRequest, base.CmdletSessionInfo, out strArrays);
					T aDSessionInfo = this.Construct(objectFromIdentitySearcher, attributeSetRequest);
					aDSessionInfo.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
					string[] strArrays1 = strArrays;
					for (int i = 0; i < (int)strArrays1.Length; i++)
					{
						string str = strArrays1[i];
						base.CmdletSessionInfo.CmdletMessageWriter.WriteWarningBuffered(str);
					}
					return aDSessionInfo;
				}
				else
				{
					throw new ArgumentNullException(StringResources.SessionRequired);
				}
			}
			else
			{
				object[] type = new object[2];
				type[0] = "GetExtendedObjectFromIdentity";
				type[1] = identityObj.GetType();
				throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.MethodNotSupportedForObjectType, type));
			}
		}

		internal RVal GetSingleValueProperty<RVal>(string parameter, T instance, ADParameterSet paramSet, ADFactory<T>.DirectoryOperation operation)
		{
			if (!paramSet.Contains(parameter))
			{
				if (instance != null)
				{
					if (!instance.ModifiedProperties.Contains(parameter))
					{
						if (!instance.AddedProperties.Contains(parameter))
						{
							if (!instance.RemovedProperties.Contains(parameter) || operation != ADFactory<T>.DirectoryOperation.Update)
							{
								if (instance.Contains(parameter) && operation == ADFactory<T>.DirectoryOperation.Create)
								{
									return (RVal)instance[parameter].Value;
								}
							}
							else
							{
								return (RVal)instance[parameter].Value;
							}
						}
						else
						{
							return (RVal)instance[parameter].Value;
						}
					}
					else
					{
						return (RVal)instance[parameter].Value;
					}
				}
				RVal rVal = default(RVal);
				return rVal;
			}
			else
			{
				return (RVal)paramSet[parameter];
			}
		}

		internal abstract IADOPathNode IdentitySearchConverter(object identity);

		internal bool PostCommitProcesing(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			return this.PostCommitPipeline.Invoke(operation, instance, parameters, directoryObj);
		}

		internal bool PreCommitProcesing(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			return this.PreCommitPipeline.Invoke(operation, instance, parameters, directoryObj);
		}

		protected internal virtual bool ProcessParameter(string paramter, ADPropertyValueCollection parameterValue, ADObject directoryObj)
		{
			return true;
		}

		internal bool PropertyHasChange(string parameter, T instance, ADParameterSet paramSet, ADFactory<T>.DirectoryOperation operation)
		{
			if ((paramSet == null || !paramSet.Contains(parameter)) && (instance == null || !instance.AddedProperties.Contains(parameter) && (!instance.RemovedProperties.Contains(parameter) || operation != ADFactory<T>.DirectoryOperation.Update) && !instance.ModifiedProperties.Contains(parameter) && (!instance.Contains(parameter) || operation != ADFactory<T>.DirectoryOperation.Create)))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		internal virtual void UpdateFromObject(T modifiedObject, ADObject directoryObj)
		{
			AttributeConverterEntry attributeConverterEntry = null;
			AttributeConverterEntry attributeConverterEntry1 = null;
			AttributeConverterEntry attributeConverterEntry2 = null;
			MappingTable<AttributeConverterEntry> item = ADFactoryBase<T>.AttributeTable[base.ConnectedStore];
			foreach (string removedProperty in modifiedObject.RemovedProperties)
			{
				if (!item.TryGetValue(removedProperty, out attributeConverterEntry))
				{
					directoryObj.ForceRemove(removedProperty);
				}
				else
				{
					this.CheckInstanceForUpdateCollisions(modifiedObject, removedProperty, attributeConverterEntry.DirectoryAttributes);
					if (!attributeConverterEntry.IsDirectoryConverterDefined)
					{
						continue;
					}
					attributeConverterEntry.InvokeToDirectoryConverter(null, directoryObj, base.CmdletSessionInfo);
				}
			}
			foreach (string addedProperty in modifiedObject.AddedProperties)
			{
				if (!item.TryGetValue(addedProperty, out attributeConverterEntry1))
				{
					directoryObj.Add(addedProperty, modifiedObject[addedProperty]);
				}
				else
				{
					this.CheckInstanceForUpdateCollisions(modifiedObject, addedProperty, attributeConverterEntry1.DirectoryAttributes);
					if (!attributeConverterEntry1.IsDirectoryConverterDefined)
					{
						continue;
					}
					attributeConverterEntry1.InvokeToDirectoryConverter(modifiedObject[addedProperty], directoryObj, base.CmdletSessionInfo);
				}
			}
			foreach (string modifiedProperty in modifiedObject.ModifiedProperties)
			{
				if (!item.TryGetValue(modifiedProperty, out attributeConverterEntry2))
				{
					directoryObj.SetValue(modifiedProperty, modifiedObject[modifiedProperty]);
				}
				else
				{
					this.CheckInstanceForUpdateCollisions(modifiedObject, modifiedProperty, attributeConverterEntry2.DirectoryAttributes);
					if (!attributeConverterEntry2.IsDirectoryConverterDefined)
					{
						continue;
					}
					attributeConverterEntry2.InvokeToDirectoryConverter(modifiedObject[modifiedProperty], directoryObj, base.CmdletSessionInfo);
				}
			}
		}

		internal virtual void UpdateFromParameters(ADParameterSet parameters, Hashtable custPropsToAdd, Hashtable custPropsToReplace, Hashtable custPropsToRemove, string[] custPropsToClear, ADObject directoryObj)
		{
			AttributeConverterEntry attributeConverterEntry = null;
			ADPropertyValueCollection aDPropertyValueCollection;
			ADPropertyValueCollection item;
			ADPropertyValueCollection aDPropertyValueCollection1;
			bool flag;
			if (base.CmdletSessionInfo != null)
			{
				MappingTable<AttributeConverterEntry> mappingTable = ADFactoryBase<T>.AttributeTable[base.ConnectedStore];
				if (custPropsToRemove != null || custPropsToAdd != null || custPropsToReplace != null)
				{
					flag = true;
				}
				else
				{
					flag = null != custPropsToClear;
				}
				bool flag1 = flag;
				IDictionary<string, ADPropertyValueCollection> aDPVCDictionary = parameters.GetADPVCDictionary();
				foreach (string key in aDPVCDictionary.Keys)
				{
					if (!mappingTable.TryGetValue(key, out attributeConverterEntry))
					{
						this.ProcessParameter(key, aDPVCDictionary[key], directoryObj);
					}
					else
					{
						if (!attributeConverterEntry.IsDirectoryConverterDefined)
						{
							continue;
						}
						if (flag1 && attributeConverterEntry.DirectoryAttributes != null)
						{
							string[] directoryAttributes = attributeConverterEntry.DirectoryAttributes;
							for (int i = 0; i < (int)directoryAttributes.Length; i++)
							{
								string str = directoryAttributes[i];
								if (custPropsToRemove != null)
								{
									foreach (string key1 in custPropsToRemove.Keys)
									{
										if (string.Compare(key1, str, StringComparison.OrdinalIgnoreCase) != 0)
										{
											continue;
										}
										object[] objArray = new object[2];
										objArray[0] = key1;
										objArray[1] = str;
										throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.CustomAttributeCollision, objArray));
									}
								}
								if (custPropsToAdd != null)
								{
									foreach (string str1 in custPropsToAdd.Keys)
									{
										if (string.Compare(str1, str, StringComparison.OrdinalIgnoreCase) != 0)
										{
											continue;
										}
										object[] objArray1 = new object[2];
										objArray1[0] = str1;
										objArray1[1] = str;
										throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.CustomAttributeCollision, objArray1));
									}
								}
								if (custPropsToReplace != null)
								{
									foreach (string key2 in custPropsToReplace.Keys)
									{
										if (string.Compare(key2, str, StringComparison.OrdinalIgnoreCase) != 0)
										{
											continue;
										}
										object[] objArray2 = new object[2];
										objArray2[0] = key2;
										objArray2[1] = str;
										throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.CustomAttributeCollision, objArray2));
									}
								}
								if (custPropsToClear != null)
								{
									string[] strArrays = custPropsToClear;
									int num = 0;
									while (num < (int)strArrays.Length)
									{
										string str2 = strArrays[num];
										if (string.Compare(str2, str, StringComparison.OrdinalIgnoreCase) != 0)
										{
											num++;
										}
										else
										{
											object[] objArray3 = new object[2];
											objArray3[0] = str2;
											objArray3[1] = str;
											throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.CustomAttributeCollision, objArray3));
										}
									}
								}
							}
						}
						attributeConverterEntry.InvokeToDirectoryConverter(aDPVCDictionary[key], directoryObj, base.CmdletSessionInfo);
					}
				}
				if (custPropsToRemove != null)
				{
					foreach (string key3 in custPropsToRemove.Keys)
					{
						object customPropertyValue = ADFactory<T>.GetCustomPropertyValue(custPropsToRemove, key3);
						if (!directoryObj.Contains(key3))
						{
							aDPropertyValueCollection = new ADPropertyValueCollection();
							aDPropertyValueCollection.TrackChanges = true;
							directoryObj.Add(key3, aDPropertyValueCollection);
						}
						else
						{
							aDPropertyValueCollection = directoryObj[key3];
						}
						if (customPropertyValue as object[] == null)
						{
							aDPropertyValueCollection.ForceRemove(customPropertyValue);
						}
						else
						{
							object[] objArray4 = (object[])customPropertyValue;
							for (int j = 0; j < (int)objArray4.Length; j++)
							{
								object obj = objArray4[j];
								aDPropertyValueCollection.ForceRemove(obj);
							}
						}
					}
				}
				if (custPropsToAdd != null)
				{
					foreach (string str3 in custPropsToAdd.Keys)
					{
						object customPropertyValue1 = ADFactory<T>.GetCustomPropertyValue(custPropsToAdd, str3);
						if (!directoryObj.Contains(str3))
						{
							item = new ADPropertyValueCollection();
							item.TrackChanges = true;
							directoryObj.Add(str3, item);
						}
						else
						{
							item = directoryObj[str3];
						}
						if (customPropertyValue1 as object[] == null)
						{
							item.Add(custPropsToAdd[str3]);
						}
						else
						{
							object[] objArray5 = (object[])customPropertyValue1;
							for (int k = 0; k < (int)objArray5.Length; k++)
							{
								object obj1 = objArray5[k];
								item.Add(obj1);
							}
						}
					}
				}
				if (custPropsToReplace != null)
				{
					foreach (string key4 in custPropsToReplace.Keys)
					{
						object customPropertyValue2 = ADFactory<T>.GetCustomPropertyValue(custPropsToReplace, key4);
						if (!directoryObj.Contains(key4))
						{
							aDPropertyValueCollection1 = new ADPropertyValueCollection();
							aDPropertyValueCollection1.TrackChanges = true;
							directoryObj.Add(key4, aDPropertyValueCollection1);
						}
						else
						{
							aDPropertyValueCollection1 = directoryObj[key4];
						}
						if (customPropertyValue2 as object[] == null)
						{
							aDPropertyValueCollection1.Value = customPropertyValue2;
						}
						else
						{
							aDPropertyValueCollection1.Clear();
							aDPropertyValueCollection1.AddRange((object[])customPropertyValue2);
						}
					}
				}
				if (custPropsToClear != null)
				{
					string[] strArrays1 = custPropsToClear;
					for (int l = 0; l < (int)strArrays1.Length; l++)
					{
						string str4 = strArrays1[l];
						directoryObj.ForceRemove(str4);
					}
				}
				return;
			}
			else
			{
				throw new ArgumentNullException(StringResources.SessionRequired);
			}
		}

		internal virtual void UpdateFromTemplate(T template, ADObject directoryObj)
		{
			AttributeConverterEntry attributeConverterEntry = null;
			if (base.CmdletSessionInfo != null)
			{
				MappingTable<AttributeConverterEntry> item = ADFactoryBase<T>.AttributeTable[base.ConnectedStore];
				foreach (string propertyName in template.PropertyNames)
				{
					if (template[propertyName].Value == null)
					{
						continue;
					}
					if (!item.TryGetValue(propertyName, out attributeConverterEntry))
					{
						if (!directoryObj.Contains(propertyName))
						{
							directoryObj.Add(propertyName, template[propertyName]);
						}
						else
						{
							ADPropertyValueCollection aDPropertyValueCollection = template[propertyName];
							aDPropertyValueCollection.TrackChanges = true;
							directoryObj[propertyName].Clear();
							foreach (object obj in aDPropertyValueCollection)
							{
								directoryObj[propertyName].Add(obj);
							}
						}
					}
					else
					{
						if (!attributeConverterEntry.IsDirectoryConverterDefined || attributeConverterEntry.AdapterAccessLevel != TypeAdapterAccess.ReadWrite && attributeConverterEntry.AdapterAccessLevel != TypeAdapterAccess.Write)
						{
							continue;
						}
						attributeConverterEntry.InvokeToDirectoryConverter(template[propertyName], directoryObj, base.CmdletSessionInfo);
					}
				}
				return;
			}
			else
			{
				throw new ArgumentNullException(StringResources.SessionRequired);
			}
		}

		internal virtual void ValidateObjectClass(T identityObj)
		{
			ADObject aDObject = (object)identityObj as ADObject;
			if (aDObject == null || !aDObject.IsSearchResult)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.OnlySearchResultsSupported, new object[0]));
			}
			else
			{
				bool? nullable = aDObject.IsOfType(this.StructuralObjectClass);
				if (!nullable.HasValue || !nullable.Value)
				{
					object[] structuralObjectClass = new object[1];
					structuralObjectClass[0] = this.StructuralObjectClass;
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidObjectClass, structuralObjectClass));
				}
				else
				{
					return;
				}
			}
		}

		internal enum DirectoryOperation
		{
			Create,
			Update,
			Delete,
			Rename,
			Move,
			Restore
		}

		internal delegate bool FactoryCommitSubroutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj);

		internal class FactoryCommitSubroutinePipeline : DelegatePipeline
		{
			private const string _debugCategory = "FactoryCommitSubroutinePipeline";

			protected override string DelegateMethodSuffix
			{
				get
				{
					return "FSRoutine";
				}
			}

			public FactoryCommitSubroutinePipeline() : base(typeof(ADFactory<T>.FactoryCommitSubroutine))
			{
			}

			public void InsertAfter(ADFactory<T>.FactoryCommitSubroutine referenceDelegate, ADFactory<T>.FactoryCommitSubroutine newDelegate)
			{
				base.InsertAfter(referenceDelegate, newDelegate);
			}

			public void InsertAtEnd(ADFactory<T>.FactoryCommitSubroutine newDelegate)
			{
				base.InsertAtEnd(newDelegate);
			}

			public void InsertAtStart(ADFactory<T>.FactoryCommitSubroutine newDelegate)
			{
				base.InsertAtStart(newDelegate);
			}

			public void InsertBefore(ADFactory<T>.FactoryCommitSubroutine referenceDelegate, ADFactory<T>.FactoryCommitSubroutine newDelegate)
			{
				base.InsertBefore(referenceDelegate, newDelegate);
			}

			public bool Invoke(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
			{
				if (base.GetDelegate() == null || (int)base.GetDelegate().GetInvocationList().Length == 0)
				{
					DebugLogger.LogInfo("FactoryCommitSubroutinePipeline", "No CmdletSubroutine delegates found");
					return false;
				}
				else
				{
					bool flag = false;
					Delegate[] invocationList = base.GetDelegate().GetInvocationList();
					for (int i = 0; i < (int)invocationList.Length; i++)
					{
						Delegate @delegate = invocationList[i];
						object[] method = new object[4];
						method[0] = "Invoking Method: ";
						method[1] = @delegate.Method;
						method[2] = " on Target: ";
						method[3] = @delegate.Target;
						DebugLogger.LogInfo("FactoryCommitSubroutinePipeline", string.Concat(method));
						bool flag1 = ((ADFactory<T>.FactoryCommitSubroutine)@delegate)(operation, instance, parameters, directoryObj);
						flag = flag | flag1;
						object[] target = new object[6];
						target[0] = "Exiting Method: ";
						target[1] = @delegate.Method;
						target[2] = " on Target: ";
						target[3] = @delegate.Target;
						target[4] = "DirObjModified = ";
						target[5] = flag1;
						DebugLogger.LogInfo("FactoryCommitSubroutinePipeline", string.Concat(target));
					}
					return flag;
				}
			}

			public void Remove(ADFactory<T>.FactoryCommitSubroutine existingDelegate)
			{
				base.Remove(existingDelegate);
			}

			public void Replace(ADFactory<T>.FactoryCommitSubroutine existingDelegate, ADFactory<T>.FactoryCommitSubroutine newDelegate)
			{
				base.Replace(existingDelegate, newDelegate);
			}
		}
	}
}