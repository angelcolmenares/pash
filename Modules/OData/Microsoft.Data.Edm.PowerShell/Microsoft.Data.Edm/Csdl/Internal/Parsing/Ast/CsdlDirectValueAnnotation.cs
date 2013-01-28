using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlDirectValueAnnotation : CsdlElement
	{
		private readonly string namespaceName;

		private readonly string name;

		private readonly string @value;

		private readonly bool isAttribute;

		public bool IsAttribute
		{
			get
			{
				return this.isAttribute;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public string NamespaceName
		{
			get
			{
				return this.namespaceName;
			}
		}

		public string Value
		{
			get
			{
				return this.@value;
			}
		}

		public CsdlDirectValueAnnotation(string namespaceName, string name, string value, bool isAttribute, CsdlLocation location) : base(location)
		{
			this.namespaceName = namespaceName;
			this.name = name;
			this.@value = value;
			this.isAttribute = isAttribute;
		}
	}
}