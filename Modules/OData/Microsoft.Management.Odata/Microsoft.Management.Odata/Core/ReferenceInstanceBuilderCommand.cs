using Microsoft.Management.Odata.Schema;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata.Core
{
	internal class ReferenceInstanceBuilderCommand : ICommand, IDisposable
	{
		private ResourceType resourceType;

		private Dictionary<string, object> properties;

		private EntityMetadata entityMetadata;

		public ReferenceInstanceBuilderCommand(ResourceType type, EntityMetadata entityMetadata)
		{
			this.entityMetadata = entityMetadata;
			this.resourceType = type;
			this.properties = new Dictionary<string, object>();
		}

		public bool AddArrayFieldParameter(string parameter, IEnumerable<object> values)
		{
			throw new NotImplementedException();
		}

		public bool AddFieldParameter(string parameter, object value)
		{
			this.properties.Add(parameter, value);
			return true;
		}

		public void AddParameter(string parameter, object value, bool isOption = true)
		{
			throw new NotImplementedException();
		}

		public bool CanFieldBeAdded(string fieldName)
		{
			return true;
		}

		public void Dispose()
		{
		}

		public IEnumerator<DSResource> InvokeAsync(Expression expression, bool noStreamingResponse)
		{
			List<DSResource> dSResources = new List<DSResource>();
			DSResource dSResource = ResourceTypeExtensions.CreateResourceWithKeyAndReferenceSetCmdlets(this.resourceType, this.properties, this.entityMetadata);
			if (dSResource != null)
			{
				dSResources.Add(dSResource);
			}
			return dSResources.GetEnumerator();
		}
	}
}