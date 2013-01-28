using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Library;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsPrimitiveTypeReference : CsdlSemanticsElement, IEdmPrimitiveTypeReference, IEdmTypeReference, IEdmElement
	{
		internal readonly CsdlPrimitiveTypeReference Reference;

		private readonly CsdlSemanticsSchema schema;

		private readonly IEdmPrimitiveType definition;

		public IEdmType Definition
		{
			get
			{
				return this.definition;
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.Reference;
			}
		}

		public bool IsNullable
		{
			get
			{
				return this.Reference.IsNullable;
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.schema.Model;
			}
		}

		public CsdlSemanticsPrimitiveTypeReference(CsdlSemanticsSchema schema, CsdlPrimitiveTypeReference reference) : base(reference)
		{
			this.schema = schema;
			this.Reference = reference;
			this.definition = EdmCoreModel.Instance.GetPrimitiveType(this.Reference.Kind);
		}

		public override string ToString()
		{
			return this.ToTraceString();
		}
	}
}