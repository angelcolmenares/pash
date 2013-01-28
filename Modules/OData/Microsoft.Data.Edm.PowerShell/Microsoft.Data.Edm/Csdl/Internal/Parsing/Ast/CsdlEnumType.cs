using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlEnumType : CsdlNamedElement
	{
		private readonly string underlyingTypeName;

		private readonly bool isFlags;

		private readonly List<CsdlEnumMember> members;

		public bool IsFlags
		{
			get
			{
				return this.isFlags;
			}
		}

		public IEnumerable<CsdlEnumMember> Members
		{
			get
			{
				return this.members;
			}
		}

		public string UnderlyingTypeName
		{
			get
			{
				return this.underlyingTypeName;
			}
		}

		public CsdlEnumType(string name, string underlyingTypeName, bool isFlags, IEnumerable<CsdlEnumMember> members, CsdlDocumentation documentation, CsdlLocation location) : base(name, documentation, location)
		{
			this.underlyingTypeName = underlyingTypeName;
			this.isFlags = isFlags;
			this.members = new List<CsdlEnumMember>(members);
		}
	}
}