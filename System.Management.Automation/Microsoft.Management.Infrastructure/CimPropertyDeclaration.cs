using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Internal;
using System;

namespace Microsoft.Management.Infrastructure
{
	public abstract class CimPropertyDeclaration
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

		public abstract CimReadOnlyKeyedCollection<CimQualifier> Qualifiers
		{
			get;
		}

		public abstract string ReferenceClassName
		{
			get;
		}

		public abstract object Value
		{
			get;
		}

		internal CimPropertyDeclaration()
		{
		}

		public override string ToString()
		{
			return Helpers.ToStringFromNameAndValue(this.Name, this.Value);
		}
	}
}