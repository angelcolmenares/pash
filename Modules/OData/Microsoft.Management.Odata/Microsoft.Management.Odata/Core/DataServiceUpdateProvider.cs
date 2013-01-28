using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Schema;
using Microsoft.Management.Odata.Tracing;
using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Linq;

namespace Microsoft.Management.Odata.Core
{
	internal class DataServiceUpdateProvider : IDataServiceUpdateProvider, IUpdatable
	{
		private DataContext dataContext;

		private List<IUpdateInstance> instances;

		private IUpdateInstance entityInstance;

		public DataServiceUpdateProvider(DataContext cxt)
		{
			this.instances = new List<IUpdateInstance>();
			this.dataContext = cxt;
		}

		public void AddReferenceToCollection(object targetResource, string propertyName, object resourceToBeAdded)
		{
			TraceHelper.Current.MethodCall0("DataServiceUpdateProvider", "AddReferenceToCollection");
			IUpdateInstance instanceFromHandle = this.GetInstanceFromHandle(targetResource);
			IUpdateInstance updateInstance = this.GetInstanceFromHandle(resourceToBeAdded);
			(instanceFromHandle as EntityUpdate).AddReference(propertyName, updateInstance);
			this.entityInstance = instanceFromHandle;
		}

		public void ClearChanges()
		{
			TraceHelper.Current.MethodCall0("DataServiceUpdateProvider", "ClearChanges");
			this.instances.Clear();
		}

		public object CreateResource(string containerName, string fullTypeName)
		{
			TraceHelper.Current.MethodCall2("DataServiceUpdateProvider", "AddReferenceToCollection", containerName, fullTypeName);
			ResourceType resourceType = null;
			if (containerName != null)
			{
				ResourceSet resourceSet = null;
				this.dataContext.UserSchema.ResourceSets.TryGetValue(containerName, out resourceSet);
				object[] objArray = new object[1];
				objArray[0] = containerName;
				ExceptionHelpers.ThrowArgumentExceptionIf("set", resourceSet == null, Resources.MissingResourceSet, objArray);
				if (this.entityInstance == null)
				{
					resourceType = resourceSet.ResourceType;
					EntityMetadata item = this.dataContext.UserSchema.EntityMetadataDictionary[resourceSet.ResourceType.FullName];
					this.entityInstance = new EntityUpdate(this.dataContext.UserContext, resourceType, item, this.dataContext.MembershipId);
					this.instances.Add(this.entityInstance);
					return this.entityInstance;
				}
				else
				{
					throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.BatchUpdatesNotSupported, new object[0]));
				}
			}
			else
			{
				this.dataContext.UserSchema.ResourceTypes.TryGetValue(fullTypeName, out resourceType);
				if (resourceType != null)
				{
					object[] objArray1 = new object[1];
					objArray1[0] = fullTypeName;
					ExceptionHelpers.ThrowArgumentExceptionIf("fullTypeName", resourceType.ResourceTypeKind != ResourceTypeKind.ComplexType, Resources.NotAComplexType, objArray1);
					IUpdateInstance complexTypeInstance = new ComplexTypeInstance(resourceType);
					this.instances.Add(complexTypeInstance);
					return complexTypeInstance;
				}
				else
				{
					TraceHelper.Current.ResourceTypeNotFound(fullTypeName);
					object[] objArray2 = new object[1];
					objArray2[0] = fullTypeName;
					throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.MissingResourceType, objArray2));
				}
			}
		}

		public void DeleteResource(object resource)
		{
			string str;
			Tracer current = TraceHelper.Current;
			string str1 = "DataServiceUpdateProvider";
			string str2 = "DeleteResource";
			if (resource == null)
			{
				str = "null";
			}
			else
			{
				str = resource.ToString();
			}
			current.MethodCall1(str1, str2, str);
			IUpdateInstance instanceFromHandle = this.GetInstanceFromHandle(resource);
			instanceFromHandle.Delete();
		}

		internal IUpdateInstance GetInstanceFromHandle(object resource)
		{
			object str;
			IUpdateInstance updateInstance = resource as IUpdateInstance;
			string str1 = "resource";
			bool flag = updateInstance == null;
			string invalidUpdateHandle = Resources.InvalidUpdateHandle;
			object[] objArray = new object[1];
			object[] objArray1 = objArray;
			int num = 0;
			if (resource != null)
			{
				str = resource.ToString();
			}
			else
			{
				str = string.Empty;
			}
			objArray1[num] = str;
			ExceptionHelpers.ThrowArgumentExceptionIf(str1, flag, invalidUpdateHandle, objArray);
			ExceptionHelpers.ThrowArgumentExceptionIf("resource", !this.instances.Contains(updateInstance), Resources.NoLongerValidUpdateHandle, new object[0]);
			return updateInstance;
		}

		public object GetResource(IQueryable query, string fullTypeName)
		{
			object obj;
			TraceHelper.Current.MethodCall2("DataServiceUpdateProvider", "GetResource", query.ToString(), fullTypeName);
			try
			{
				DSLinqQueryProvider provider = query.Provider as DSLinqQueryProvider;
				ExceptionHelpers.ThrowArgumentExceptionIf("query", provider == null, Resources.QueryDoesNotIncludePswsProvider, new object[0]);
				if (!provider.IsFilterOverResourceRoot(query.Expression))
				{
					ReferredResourceExtractor referredResourceExtractor = new ReferredResourceExtractor();
					DSResource current = null;
					if (!referredResourceExtractor.Extract(query.Expression, provider.GetInitialQueryable(), provider.GetInitialResourceType(), provider.GetEntityMetadata(provider.GetInitialResourceType())))
					{
						IEnumerator<DSResource> enumerator = (query.GetEnumerator() as IEnumerable<DSResource>).GetEnumerator();
						if (enumerator.MoveNext())
						{
							current = enumerator.Current;
							if (enumerator.MoveNext())
							{
								throw new ArgumentException("multiple instances");
							}
						}
						else
						{
							throw new ArgumentException("no instances");
						}
					}
					else
					{
						current = referredResourceExtractor.ReferredResource;
					}
					ResourceType resourceType = current.ResourceType;
					EntityMetadata entityMetadata = provider.GetEntityMetadata(resourceType);
					IUpdateInstance referredEntityInstance = new ReferredEntityInstance(current, this.dataContext.UserContext, resourceType, entityMetadata, this.dataContext.MembershipId);
					this.instances.Add(referredEntityInstance);
					obj = referredEntityInstance;
				}
				else
				{
					ResourceType initialResourceType = provider.GetInitialResourceType();
					EntityMetadata entityMetadatum = provider.GetEntityMetadata(initialResourceType);
					this.entityInstance = new EntityUpdate(CommandType.Update, this.dataContext.UserContext, initialResourceType, entityMetadatum, query, this.dataContext.MembershipId);
					this.instances.Add(this.entityInstance);
					obj = this.entityInstance;
				}
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				TraceHelper.Current.InvalidUpdateQuery(fullTypeName, query.ToString(), argumentException.GetType().ToString(), argumentException.Message);
				obj = null;
			}
			return obj;
		}

		public object GetValue(object resource, string propertyName)
		{
			string str;
			Tracer current = TraceHelper.Current;
			string str1 = "DataServiceUpdateProvider";
			string str2 = "GetValue";
			if (resource == null)
			{
				str = "null";
			}
			else
			{
				str = resource.ToString();
			}
			current.MethodCall2(str1, str2, str, propertyName);
			object[] objArray = new object[2];
			objArray[0] = "GetValue";
			objArray[1] = "DataServiceUpdateProvider";
			throw new NotImplementedException(ExceptionHelpers.GetExceptionMessage(Resources.NotImplementedExceptionMessage, objArray));
		}

		public void RemoveReferenceFromCollection(object targetResource, string propertyName, object resourceToBeRemoved)
		{
			TraceHelper.Current.MethodCall0("DataServiceUpdateProvider", "RemoveReference");
			IUpdateInstance instanceFromHandle = this.GetInstanceFromHandle(targetResource);
			IUpdateInstance updateInstance = this.GetInstanceFromHandle(resourceToBeRemoved);
			(instanceFromHandle as EntityUpdate).RemoveReference(propertyName, updateInstance);
			this.entityInstance = instanceFromHandle;
		}

		public object ResetResource(object resource)
		{
			string str;
			Tracer current = TraceHelper.Current;
			string str1 = "DataServiceUpdateProvider";
			string str2 = "ResetResource";
			if (resource == null)
			{
				str = "null";
			}
			else
			{
				str = resource.ToString();
			}
			current.MethodCall1(str1, str2, str);
			IUpdateInstance instanceFromHandle = this.GetInstanceFromHandle(resource);
			instanceFromHandle.Reset();
			return instanceFromHandle;
		}

		public object ResolveResource(object resource)
		{
			string str;
			Tracer current = TraceHelper.Current;
			string str1 = "DataServiceUpdateProvider";
			string str2 = "ResolveResource";
			if (resource == null)
			{
				str = "null";
			}
			else
			{
				str = resource.ToString();
			}
			current.MethodCall1(str1, str2, str);
			IUpdateInstance instanceFromHandle = this.GetInstanceFromHandle(resource);
			return instanceFromHandle.Resolve();
		}

		public void SaveChanges()
		{
			TraceHelper.Current.MethodCall0("DataServiceUpdateProvider", "SaveChanges");
			this.entityInstance.InvokeCommand();
		}

		public void SetConcurrencyValues(object resourceCookie, bool? checkForEquality, IEnumerable<KeyValuePair<string, object>> concurrencyValues)
		{
			string str;
			string str1;
			bool hasValue;
			Tracer current = TraceHelper.Current;
			string str2 = "DataServiceUpdateProvider";
			string str3 = "SetConcurrencyValues";
			if (!checkForEquality.HasValue)
			{
				str = "null";
			}
			else
			{
				str = checkForEquality.ToString();
			}
			if (concurrencyValues == null)
			{
				str1 = "null";
			}
			else
			{
				str1 = concurrencyValues.ToString();
			}
			current.MethodCall2(str2, str3, str, str1);
			if (checkForEquality.HasValue)
			{
				bool? nullable = checkForEquality;
				if (nullable.GetValueOrDefault())
				{
					hasValue = false;
				}
				else
				{
					hasValue = nullable.HasValue;
				}
				if (!hasValue)
				{
					IUpdateInstance instanceFromHandle = this.GetInstanceFromHandle(resourceCookie);
					instanceFromHandle.VerifyConcurrencyValues(concurrencyValues);
					return;
				}
				else
				{
					throw new NotImplementedException();
				}
			}
			else
			{
				return;
			}
		}

		public void SetReference(object resource, string propertyName, object propertyValue)
		{
			string str;
			string str1;
			IUpdateInstance instanceFromHandle;
			Tracer current = TraceHelper.Current;
			string str2 = "DataServiceUpdateProvider";
			string str3 = "SetReference";
			if (resource == null)
			{
				str = "null";
			}
			else
			{
				str = resource.ToString();
			}
			string str4 = propertyName;
			if (propertyValue == null)
			{
				str1 = "null";
			}
			else
			{
				str1 = propertyValue.ToString();
			}
			current.MethodCall3(str2, str3, str, str4, str1);
			IUpdateInstance updateInstance = this.GetInstanceFromHandle(resource);
			if (propertyValue == null)
			{
				instanceFromHandle = null;
			}
			else
			{
				instanceFromHandle = this.GetInstanceFromHandle(propertyValue);
			}
			IUpdateInstance updateInstance1 = instanceFromHandle;
			updateInstance.SetReference(propertyName, updateInstance1);
		}

		public void SetValue(object resource, string propertyName, object propertyValue)
		{
			string str;
			string str1;
			Tracer current = TraceHelper.Current;
			string str2 = "DataServiceUpdateProvider";
			string str3 = "SetValue";
			if (resource == null)
			{
				str = "null";
			}
			else
			{
				str = resource.ToString();
			}
			string str4 = propertyName;
			if (propertyValue == null)
			{
				str1 = "null";
			}
			else
			{
				str1 = propertyValue.ToString();
			}
			current.MethodCall3(str2, str3, str, str4, str1);
			IUpdateInstance instanceFromHandle = this.GetInstanceFromHandle(resource);
			instanceFromHandle.SetValue(propertyName, propertyValue);
		}
	}
}