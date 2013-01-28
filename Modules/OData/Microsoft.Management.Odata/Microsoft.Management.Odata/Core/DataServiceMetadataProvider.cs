using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Schema;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Security.Principal;
using System.ServiceModel;

namespace Microsoft.Management.Odata.Core
{
	internal class DataServiceMetadataProvider : IDataServiceMetadataProvider, IDisposable
	{
		private bool disposed;

		private DataContext dataContext;

		public string ContainerName
		{
			get
			{
				return this.dataContext.UserSchema.ContainerName;
			}
		}

		public string ContainerNamespace
		{
			get
			{
				return this.dataContext.UserSchema.ContainerNamespace;
			}
		}

		public IEnumerable<ResourceSet> ResourceSets
		{
			get
			{
				return this.dataContext.UserSchema.ResourceSets.Values;
			}
		}

		public IEnumerable<ServiceOperation> ServiceOperations
		{
			get
			{
				return null;
			}
		}

		public IEnumerable<ResourceType> Types
		{
			get
			{
				return this.dataContext.UserSchema.ResourceTypes.Values;
			}
		}

		public DataServiceMetadataProvider(DataContext dataContext)
		{
			this.dataContext = dataContext;
		}

		public void Dispose()
		{
			OperationContext.Current.TraceOutgoingMessage();
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposeManagedResources)
		{
			if (!this.disposed && disposeManagedResources)
			{
				try
				{
					SafeRefCountedContainer<WindowsIdentity> authorizedUserIdentity = DataServiceController.Current.GetAuthorizedUserIdentity(this.dataContext.UserContext);
					authorizedUserIdentity.Release();
				}
				catch (UnauthorizedAccessException unauthorizedAccessException)
				{
				}
			}
			this.disposed = true;
		}

		public IEnumerable<ResourceType> GetDerivedTypes(ResourceType resourceType)
		{
			TraceHelper.Current.MethodCall0("DataServiceMetadataProvider", "GetDerivedTypes");
			if (resourceType != null)
			{
				return resourceType.GetDerivedTypes();
			}
			else
			{
				return null;
			}
		}

		public ResourceAssociationSet GetResourceAssociationSet(ResourceSet resourceSet, ResourceType resourceType, ResourceProperty resourceProperty)
		{
			ResourceAssociationSet resourceAssociationSet;
			if (resourceSet == null || resourceType == null)
			{
				return null;
			}
			else
			{
				if (resourceType.Properties.Contains(resourceProperty))
				{
					Microsoft.Management.Odata.Schema.Schema.AssociationType associationType = (resourceProperty.GetCustomState() as ReferenceCustomState).AssociationType;
					if (associationType != null)
					{
						List<ResourceAssociationSet>.Enumerator enumerator = associationType.WcfClass.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								ResourceAssociationSet current = enumerator.Current;
								if (current.End1.ResourceSet != resourceSet || current.End1.ResourceType != resourceType || current.End1.ResourceProperty != resourceProperty)
								{
									if (current.End2.ResourceSet != resourceSet || current.End2.ResourceType != resourceType || current.End2.ResourceProperty != resourceProperty)
									{
										continue;
									}
									resourceAssociationSet = current;
									return resourceAssociationSet;
								}
								else
								{
									resourceAssociationSet = current;
									return resourceAssociationSet;
								}
							}
							return null;
						}
						finally
						{
							enumerator.Dispose();
						}
						return resourceAssociationSet;
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
		}

		public bool HasDerivedTypes(ResourceType resourceType)
		{
			TraceHelper.Current.MethodCall0("DataServiceMetadataProvider", "HasDerivedTypes");
			if (resourceType != null)
			{
				return resourceType.GetDerivedTypes().Count > 0;
			}
			else
			{
				return false;
			}
		}

		public bool TryResolveResourceSet(string name, out ResourceSet resourceSet)
		{
			TraceHelper.Current.MethodCall1("DataServiceMetadataProvider", "TryResolveResourceSet", name);
			bool flag = this.dataContext.UserSchema.ResourceSets.TryGetValue(name, out resourceSet);
			if (!flag)
			{
				TraceHelper.Current.ResourceSetNotFound(name);
			}
			return flag;
		}

		public bool TryResolveResourceType(string name, out ResourceType resourceType)
		{
			TraceHelper.Current.MethodCall1("DataServiceMetadataProvider", "TryResolveResourceType", name);
			bool flag = this.dataContext.UserSchema.ResourceTypes.TryGetValue(name, out resourceType);
			if (!flag)
			{
				TraceHelper.Current.ResourceTypeNotFound(name);
			}
			return flag;
		}

		public bool TryResolveServiceOperation(string name, out ServiceOperation serviceOperation)
		{
			serviceOperation = null;
			return false;
		}
	}
}