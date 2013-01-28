using Microsoft.Management.Infrastructure.Internal;
using System;

namespace Microsoft.Management.Infrastructure
{
	public abstract class CimQualifier
	{
		public abstract CimType CimType
		{
			get;
		}

		public abstract CimFlags Flags
		{
			get;
		}

		public abstract string Name
		{
			get;
		}

		public abstract object Value
		{
			get;
		}

		internal CimQualifier()
		{
		}

		public override string ToString()
		{
			return Helpers.ToStringFromNameAndValue(this.Name, this.Value);
		}
	}
}