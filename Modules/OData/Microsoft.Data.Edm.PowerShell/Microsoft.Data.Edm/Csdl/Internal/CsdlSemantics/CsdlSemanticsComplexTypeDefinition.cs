using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Internal;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsComplexTypeDefinition : CsdlSemanticsStructuredTypeDefinition, IEdmComplexType, IEdmStructuredType, IEdmSchemaType, IEdmType, IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly CsdlComplexType complex;

		private readonly Cache<CsdlSemanticsComplexTypeDefinition, IEdmComplexType> baseTypeCache;

		private readonly static Func<CsdlSemanticsComplexTypeDefinition, IEdmComplexType> ComputeBaseTypeFunc;

		private readonly static Func<CsdlSemanticsComplexTypeDefinition, IEdmComplexType> OnCycleBaseTypeFunc;

		public override IEdmStructuredType BaseType
		{
			get
			{
				return this.baseTypeCache.GetValue(this, CsdlSemanticsComplexTypeDefinition.ComputeBaseTypeFunc, CsdlSemanticsComplexTypeDefinition.OnCycleBaseTypeFunc);
			}
		}

		public override bool IsAbstract
		{
			get
			{
				return this.complex.IsAbstract;
			}
		}

		protected override CsdlStructuredType MyStructured
		{
			get
			{
				return this.complex;
			}
		}

		public string Name
		{
			get
			{
				return this.complex.Name;
			}
		}

		public EdmTermKind TermKind
		{
			get
			{
				return EdmTermKind.Type;
			}
		}

		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.Complex;
			}
		}

		static CsdlSemanticsComplexTypeDefinition()
		{
			CsdlSemanticsComplexTypeDefinition.ComputeBaseTypeFunc = (CsdlSemanticsComplexTypeDefinition me) => me.ComputeBaseType();
			CsdlSemanticsComplexTypeDefinition.OnCycleBaseTypeFunc = (CsdlSemanticsComplexTypeDefinition me) => new CyclicComplexType(me.GetCyclicBaseTypeName(me.complex.BaseTypeName), me.Location);
		}

		public CsdlSemanticsComplexTypeDefinition(CsdlSemanticsSchema context, CsdlComplexType complex) : base(context, complex)
		{
			this.baseTypeCache = new Cache<CsdlSemanticsComplexTypeDefinition, IEdmComplexType>();
			this.complex = complex;
		}

		private IEdmComplexType ComputeBaseType()
		{
			if (this.complex.BaseTypeName == null)
			{
				return null;
			}
			else
			{
				IEdmComplexType edmComplexType = base.Context.FindType(this.complex.BaseTypeName) as IEdmComplexType;
				if (edmComplexType != null)
				{
					var baseType = edmComplexType.BaseType;
					if (baseType == null) { }
				}
				IEdmComplexType edmComplexType1 = edmComplexType;
				IEdmComplexType unresolvedComplexType = edmComplexType1;
				if (edmComplexType1 == null)
				{
					unresolvedComplexType = new UnresolvedComplexType(base.Context.UnresolvedName(this.complex.BaseTypeName), base.Location);
				}
				return unresolvedComplexType;
			}
		}
	}
}