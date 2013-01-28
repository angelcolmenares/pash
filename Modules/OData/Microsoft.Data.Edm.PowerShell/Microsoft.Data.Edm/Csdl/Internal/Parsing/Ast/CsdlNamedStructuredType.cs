using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal abstract class CsdlNamedStructuredType : CsdlStructuredType
	{
		protected string baseTypeName;

		protected bool isAbstract;

		protected string name;

		public string BaseTypeName
		{
			get
			{
				return this.baseTypeName;
			}
		}

		public bool IsAbstract
		{
			get
			{
				return this.isAbstract;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		protected CsdlNamedStructuredType(string name, string baseTypeName, bool isAbstract, IEnumerable<CsdlProperty> properties, CsdlDocumentation documentation, CsdlLocation location) : base(properties, documentation, location)
		{
			this.isAbstract = isAbstract;
			this.name = name;
			this.baseTypeName = baseTypeName;
		}
	}
}