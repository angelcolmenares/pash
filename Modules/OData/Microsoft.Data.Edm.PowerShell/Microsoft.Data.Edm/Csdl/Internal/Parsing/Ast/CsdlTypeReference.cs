using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal abstract class CsdlTypeReference : CsdlElement
	{
		private readonly bool isNullable;

		public bool IsNullable
		{
			get
			{
				return this.isNullable;
			}
		}

		protected CsdlTypeReference(bool isNullable, CsdlLocation location) : base(location)
		{
			this.isNullable = isNullable;
		}
	}
}