using Microsoft.Management.Infrastructure.Generic;
using System;

namespace Microsoft.Management.Infrastructure
{
	public abstract class CimMethodDeclaration
	{
		public abstract string Name
		{
			get;
		}

		public abstract CimReadOnlyKeyedCollection<CimMethodParameterDeclaration> Parameters
		{
			get;
		}

		public abstract CimReadOnlyKeyedCollection<CimQualifier> Qualifiers
		{
			get;
		}

		public abstract CimType ReturnType
		{
			get;
		}

		internal CimMethodDeclaration()
		{
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}