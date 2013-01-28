using Microsoft.Management.Odata.Schema;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;

namespace Microsoft.Management.Odata.Core
{
	internal class ReferredEntityInstance : IUpdateInstance
	{
		private DSResource resource;

		private UserContext userContext;

		private string membershipId;

		private ResourceType resourceType;

		private EntityMetadata metadata;

		public ReferredEntityInstance(DSResource resource, UserContext userContext, ResourceType type, EntityMetadata metadata, string membershipId)
		{
			this.userContext = userContext;
			this.resourceType = type;
			this.metadata = metadata;
			this.membershipId = membershipId;
			this.resource = resource;
		}

		public void Delete()
		{
			throw new NotImplementedException();
		}

		public Dictionary<string, object> GetKeyValues()
		{
			return this.resource.GetKeyValues();
		}

		public void InvokeCommand()
		{
			throw new NotImplementedException();
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public object Resolve()
		{
			return this.resource;
		}

		public void SetReference(string propertyName, IUpdateInstance instance)
		{
			throw new NotImplementedException();
		}

		public void SetValue(string propertyName, object value)
		{
			throw new NotImplementedException();
		}

		public TestHookCommandInvocationData TestHookGetInvocationData()
		{
			throw new NotImplementedException();
		}

		public void VerifyConcurrencyValues(IEnumerable<KeyValuePair<string, object>> values)
		{
			throw new NotImplementedException();
		}
	}
}