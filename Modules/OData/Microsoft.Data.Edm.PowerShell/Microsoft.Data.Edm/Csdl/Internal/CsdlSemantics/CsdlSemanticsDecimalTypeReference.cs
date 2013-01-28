using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsDecimalTypeReference : CsdlSemanticsPrimitiveTypeReference, IEdmDecimalTypeReference, IEdmPrimitiveTypeReference, IEdmTypeReference, IEdmElement
	{
		public int? Precision
		{
			get
			{
				return ((CsdlDecimalTypeReference)this.Reference).Precision;
			}
		}

		public int? Scale
		{
			get
			{
				return ((CsdlDecimalTypeReference)this.Reference).Scale;
			}
		}

		public CsdlSemanticsDecimalTypeReference(CsdlSemanticsSchema schema, CsdlDecimalTypeReference reference) : base(schema, reference)
		{
		}
	}
}