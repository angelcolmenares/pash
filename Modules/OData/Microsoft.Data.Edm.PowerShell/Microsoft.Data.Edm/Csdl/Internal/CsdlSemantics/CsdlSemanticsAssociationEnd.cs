using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsAssociationEnd : CsdlSemanticsElement, IEdmAssociationEnd, IEdmNamedElement, IEdmElement, IEdmCheckable
	{
		private readonly CsdlAssociationEnd end;

		private readonly CsdlSemanticsAssociation definingAssociation;

		private readonly CsdlSemanticsSchema context;

		private readonly Cache<CsdlSemanticsAssociationEnd, IEdmEntityType> typeCache;

		private readonly static Func<CsdlSemanticsAssociationEnd, IEdmEntityType> ComputeTypeFunc;

		private readonly Cache<CsdlSemanticsAssociationEnd, IEnumerable<EdmError>> errorsCache;

		private readonly static Func<CsdlSemanticsAssociationEnd, IEnumerable<EdmError>> ComputeErrorsFunc;

		public IEdmAssociation DeclaringAssociation
		{
			get
			{
				return this.definingAssociation;
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.end;
			}
		}

		public IEdmEntityType EntityType
		{
			get
			{
				return this.typeCache.GetValue(this, CsdlSemanticsAssociationEnd.ComputeTypeFunc, null);
			}
		}

		public IEnumerable<EdmError> Errors
		{
			get
			{
				return this.errorsCache.GetValue(this, CsdlSemanticsAssociationEnd.ComputeErrorsFunc, null);
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.context.Model;
			}
		}

		public EdmMultiplicity Multiplicity
		{
			get
			{
				return this.end.Multiplicity;
			}
		}

		public string Name
		{
			get
			{
				string name = this.end.Name;
				string empty = name;
				if (name == null)
				{
					empty = string.Empty;
				}
				return empty;
			}
		}

		public EdmOnDeleteAction OnDelete
		{
			get
			{
				if (this.end.OnDelete != null)
				{
					return this.end.OnDelete.Action;
				}
				else
				{
					return EdmOnDeleteAction.None;
				}
			}
		}

		static CsdlSemanticsAssociationEnd()
		{
			CsdlSemanticsAssociationEnd.ComputeTypeFunc = (CsdlSemanticsAssociationEnd me) => me.ComputeType();
			CsdlSemanticsAssociationEnd.ComputeErrorsFunc = (CsdlSemanticsAssociationEnd me) => me.ComputeErrors();
		}

		public CsdlSemanticsAssociationEnd(CsdlSemanticsSchema context, CsdlSemanticsAssociation association, CsdlAssociationEnd end) : base(end)
		{
			this.typeCache = new Cache<CsdlSemanticsAssociationEnd, IEdmEntityType>();
			this.errorsCache = new Cache<CsdlSemanticsAssociationEnd, IEnumerable<EdmError>>();
			this.end = end;
			this.definingAssociation = association;
			this.context = context;
		}

		private IEnumerable<EdmError> ComputeErrors()
		{
			List<EdmError> edmErrors = null;
			if (this.EntityType as UnresolvedEntityType != null)
			{
				edmErrors = CsdlSemanticsElement.AllocateAndAdd<EdmError>(edmErrors, this.EntityType.Errors());
			}
			List<EdmError> edmErrors1 = edmErrors;
			IEnumerable<EdmError> edmErrors2 = edmErrors1;
			if (edmErrors1 == null)
			{
				edmErrors2 = Enumerable.Empty<EdmError>();
			}
			return edmErrors2;
		}

		private IEdmEntityType ComputeType()
		{
			IEdmTypeReference edmTypeReference = CsdlSemanticsModel.WrapTypeReference(this.context, this.end.Type);
			if (edmTypeReference.TypeKind() == EdmTypeKind.Entity)
			{
				return edmTypeReference.AsEntity().EntityDefinition();
			}
			else
			{
				return new UnresolvedEntityType(edmTypeReference.FullName(), base.Location);
			}
		}
	}
}