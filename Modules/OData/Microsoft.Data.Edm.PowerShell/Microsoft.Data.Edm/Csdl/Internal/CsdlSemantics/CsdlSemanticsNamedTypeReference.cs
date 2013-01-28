using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsNamedTypeReference : CsdlSemanticsElement, IEdmTypeReference, IEdmElement
	{
		private readonly CsdlSemanticsSchema schema;

		private readonly CsdlNamedTypeReference reference;

		private readonly Cache<CsdlSemanticsNamedTypeReference, IEdmType> definitionCache;

		private readonly static Func<CsdlSemanticsNamedTypeReference, IEdmType> ComputeDefinitionFunc;

		public IEdmType Definition
		{
			get
			{
				return this.definitionCache.GetValue(this, CsdlSemanticsNamedTypeReference.ComputeDefinitionFunc, null);
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.reference;
			}
		}

		public bool IsNullable
		{
			get
			{
				return this.reference.IsNullable;
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.schema.Model;
			}
		}

		static CsdlSemanticsNamedTypeReference()
		{
			CsdlSemanticsNamedTypeReference.ComputeDefinitionFunc = (CsdlSemanticsNamedTypeReference me) => me.ComputeDefinition();
		}

		public CsdlSemanticsNamedTypeReference(CsdlSemanticsSchema schema, CsdlNamedTypeReference reference) : base(reference)
		{
			this.definitionCache = new Cache<CsdlSemanticsNamedTypeReference, IEdmType>();
			this.schema = schema;
			this.reference = reference;
		}

		private IEdmType ComputeDefinition()
		{
			IEdmType edmType = this.schema.FindType(this.reference.FullName);
			IEdmType edmType1 = edmType;
			IEdmType unresolvedType = edmType1;
			if (edmType1 == null)
			{
				string str = this.schema.ReplaceAlias(this.reference.FullName);
				string fullName = str;
				if (str == null)
				{
					fullName = this.reference.FullName;
				}
				unresolvedType = new UnresolvedType(fullName, base.Location);
			}
			return unresolvedType;
		}

		public override string ToString()
		{
			return this.ToTraceString();
		}
	}
}