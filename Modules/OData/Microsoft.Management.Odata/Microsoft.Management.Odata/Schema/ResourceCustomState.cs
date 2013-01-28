using Microsoft.Management.Odata.Common;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;

namespace Microsoft.Management.Odata.Schema
{
	internal class ResourceCustomState
	{
		public Type ClrType
		{
			get
			{
				if (!string.IsNullOrEmpty(this.ClrTypeString))
				{
					return (new TypeWrapper(this.ClrTypeString)).Value;
				}
				else
				{
					return typeof(object);
				}
			}
		}

		public string ClrTypeString
		{
			get;
			set;
		}

		public HashSet<ResourceType> DerivedTypes
		{
			get;
			private set;
		}

		public ResourceCustomState() : this(null)
		{
		}

		public ResourceCustomState(string clrType)
		{
			this.DerivedTypes = new HashSet<ResourceType>();
			this.ClrTypeString = clrType;
		}
	}
}