using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.Schema;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;

namespace Microsoft.Management.Odata.PS
{
	internal class PSReferencedResourceSet : IReferencedResourceSet
	{
		private ResourceProperty referenceResourceProperty;

		private ResourceType referenceeResourceType;

		internal static PSReferencedResourceSet.CommandFactory TestHookCommandFactory
		{
			get;set;
		}

		internal static DataContext TestHookDataContext
		{
			get;set;
		}

		public PSReferencedResourceSet(ResourceProperty referencedResourceProperty, ResourceType referenceeResourceType)
		{
			this.referenceResourceProperty = referencedResourceProperty;
			this.referenceeResourceType = referenceeResourceType;
		}

		public List<DSResource> Get(Dictionary<string, object> parameters)
		{
			List<DSResource> dSResources;
			DataContext currentContext;
			PSReferencedResourceSet.CommandFactory commandFactory;
			if (PSReferencedResourceSet.TestHookDataContext == null)
			{
				currentContext = DataServiceController.Current.GetCurrentContext();
			}
			else
			{
				currentContext = PSReferencedResourceSet.TestHookDataContext;
			}
			DataContext dataContext = currentContext;
			if (PSReferencedResourceSet.TestHookCommandFactory == null)
			{
				commandFactory = new PSReferencedResourceSet.CommandFactory(DataServiceController.Current.GetReferenceSetCommand);
			}
			else
			{
				commandFactory = PSReferencedResourceSet.TestHookCommandFactory;
			}
			PSReferencedResourceSet.CommandFactory commandFactory1 = commandFactory;
			IReferenceSetCommand userContext = commandFactory1((CommandType)5, dataContext.UserContext, this.referenceResourceProperty, dataContext.UserSchema.GetEntityMetadata(this.referenceeResourceType), dataContext.MembershipId, this.referenceResourceProperty.ResourceType);
			using (userContext)
			{
				UriParametersHelper.AddParametersToCommand(userContext, DataServiceController.Current.GetCurrentResourceUri());
				userContext.AddReferringObject(parameters);
				IEnumerator<DSResource> enumerator = userContext.InvokeAsync(null, false);
				List<DSResource> dSResources1 = new List<DSResource>();
				while (enumerator.MoveNext())
				{
					dSResources1.Add(enumerator.Current);
				}
				dSResources = dSResources1;
			}
			return dSResources;
		}

		public delegate IReferenceSetCommand CommandFactory(CommandType commandType, UserContext userContext, ResourceProperty property, EntityMetadata entityMetadata, string membershipId, ResourceType entityType);
	}
}