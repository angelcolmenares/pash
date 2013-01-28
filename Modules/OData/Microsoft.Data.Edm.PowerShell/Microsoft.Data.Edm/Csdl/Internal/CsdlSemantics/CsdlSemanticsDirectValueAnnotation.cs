using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Library.Values;
using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsDirectValueAnnotation : CsdlSemanticsElement, IEdmDirectValueAnnotation, IEdmNamedElement, IEdmElement
	{
		private readonly CsdlDirectValueAnnotation annotation;

		private readonly CsdlSemanticsModel model;

		private readonly Cache<CsdlSemanticsDirectValueAnnotation, IEdmValue> valueCache;

		private readonly static Func<CsdlSemanticsDirectValueAnnotation, IEdmValue> ComputeValueFunc;

		public override CsdlElement Element
		{
			get
			{
				return this.annotation;
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.model;
			}
		}

		public string Name
		{
			get
			{
				return this.annotation.Name;
			}
		}

		public string NamespaceUri
		{
			get
			{
				return this.annotation.NamespaceName;
			}
		}

		public object Value
		{
			get
			{
				return this.valueCache.GetValue(this, CsdlSemanticsDirectValueAnnotation.ComputeValueFunc, null);
			}
		}

		static CsdlSemanticsDirectValueAnnotation()
		{
			CsdlSemanticsDirectValueAnnotation.ComputeValueFunc = (CsdlSemanticsDirectValueAnnotation me) => me.ComputeValue();
		}

		public CsdlSemanticsDirectValueAnnotation(CsdlDirectValueAnnotation annotation, CsdlSemanticsModel model) : base(annotation)
		{
			this.valueCache = new Cache<CsdlSemanticsDirectValueAnnotation, IEdmValue>();
			this.annotation = annotation;
			this.model = model;
		}

		private IEdmValue ComputeValue()
		{
			IEdmStringValue edmStringConstant = new EdmStringConstant(new EdmStringTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.String), false), this.annotation.Value);
			edmStringConstant.SetIsSerializedAsElement(this.model, !this.annotation.IsAttribute);
			return edmStringConstant;
		}
	}
}