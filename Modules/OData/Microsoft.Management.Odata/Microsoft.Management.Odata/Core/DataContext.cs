using Microsoft.Management.Odata.Schema;
using System;

namespace Microsoft.Management.Odata.Core
{
	internal class DataContext
	{
		public string MembershipId
		{
			get;
			private set;
		}

		public UserContext UserContext
		{
			get;
			private set;
		}

		public Microsoft.Management.Odata.Schema.Schema UserSchema
		{
			get;
			private set;
		}

		public DataContext(Microsoft.Management.Odata.Schema.Schema userSchema, UserContext userContext, string membershipId)
		{
			this.UserSchema = userSchema;
			this.UserContext = userContext;
			this.MembershipId = membershipId;
		}
	}
}