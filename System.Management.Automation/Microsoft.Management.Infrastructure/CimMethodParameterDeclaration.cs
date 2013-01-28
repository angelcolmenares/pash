using Microsoft.Management.Infrastructure.Generic;
using System;

namespace Microsoft.Management.Infrastructure
{
	public abstract class CimMethodParameterDeclaration
	{
		public abstract CimType CimType
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

		internal CimMethodParameterDeclaration()
		{
		}
	}
}