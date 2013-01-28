using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal abstract class CsdlSemanticsTypeDefinition : CsdlSemanticsElement, IEdmType, IEdmElement
	{
		public EdmSchemaElementKind SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.TypeDefinition;
			}
		}

		public abstract EdmTypeKind TypeKind
		{
			get;
		}

		protected CsdlSemanticsTypeDefinition(CsdlElement element) : base(element)
		{
		}

		public override string ToString()
		{
			return this.ToTraceString();
		}
	}
}