using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsAssociation : CsdlSemanticsElement, IEdmAssociation, IEdmNamedElement, IEdmElement, IEdmCheckable
	{
		private readonly CsdlAssociation association;

		private readonly CsdlSemanticsSchema context;

		private readonly Cache<CsdlSemanticsAssociation, CsdlSemanticsReferentialConstraint> referentialConstraintCache;

		private readonly static Func<CsdlSemanticsAssociation, CsdlSemanticsReferentialConstraint> ComputeReferentialConstraintFunc;

		private readonly Cache<CsdlSemanticsAssociation, TupleInternal<IEdmAssociationEnd, IEdmAssociationEnd>> endsCache;

		private readonly static Func<CsdlSemanticsAssociation, TupleInternal<IEdmAssociationEnd, IEdmAssociationEnd>> ComputeEndsFunc;

		private readonly Cache<CsdlSemanticsAssociation, IEnumerable<EdmError>> errorsCache;

		private readonly static Func<CsdlSemanticsAssociation, IEnumerable<EdmError>> ComputeErrorsFunc;

		public override CsdlElement Element
		{
			get
			{
				return this.association;
			}
		}

		public IEdmAssociationEnd End1
		{
			get
			{
				return this.endsCache.GetValue(this, CsdlSemanticsAssociation.ComputeEndsFunc, null).Item1;
			}
		}

		public IEdmAssociationEnd End2
		{
			get
			{
				return this.endsCache.GetValue(this, CsdlSemanticsAssociation.ComputeEndsFunc, null).Item2;
			}
		}

		public IEnumerable<EdmError> Errors
		{
			get
			{
				return this.errorsCache.GetValue(this, CsdlSemanticsAssociation.ComputeErrorsFunc, null);
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.context.Model;
			}
		}

		public string Name
		{
			get
			{
				return this.association.Name;
			}
		}

		public string Namespace
		{
			get
			{
				return this.context.Namespace;
			}
		}

		public CsdlSemanticsReferentialConstraint ReferentialConstraint
		{
			get
			{
				return this.referentialConstraintCache.GetValue(this, CsdlSemanticsAssociation.ComputeReferentialConstraintFunc, null);
			}
		}

		static CsdlSemanticsAssociation()
		{
			CsdlSemanticsAssociation.ComputeReferentialConstraintFunc = (CsdlSemanticsAssociation me) => me.ComputeReferentialConstraint();
			CsdlSemanticsAssociation.ComputeEndsFunc = (CsdlSemanticsAssociation me) => me.ComputeEnds();
			CsdlSemanticsAssociation.ComputeErrorsFunc = (CsdlSemanticsAssociation me) => me.ComputeErrors();
		}

		public CsdlSemanticsAssociation(CsdlSemanticsSchema context, CsdlAssociation association) : base(association)
		{
			this.referentialConstraintCache = new Cache<CsdlSemanticsAssociation, CsdlSemanticsReferentialConstraint>();
			this.endsCache = new Cache<CsdlSemanticsAssociation, TupleInternal<IEdmAssociationEnd, IEdmAssociationEnd>>();
			this.errorsCache = new Cache<CsdlSemanticsAssociation, IEnumerable<EdmError>>();
			this.association = association;
			this.context = context;
		}

		private TupleInternal<IEdmAssociationEnd, IEdmAssociationEnd> ComputeEnds()
		{
			IEdmAssociationEnd csdlSemanticsAssociationEnd;
			IEdmAssociationEnd edmAssociationEnd;
			if (this.association.End1 != null)
			{
				csdlSemanticsAssociationEnd = new CsdlSemanticsAssociationEnd(this.context, this, this.association.End1);
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(base.Location, EdmErrorCode.InvalidAssociation, Strings.CsdlParser_InvalidAssociationIncorrectNumberOfEnds(string.Concat(this.Namespace, ".", this.Name)));
				IEdmAssociationEnd badAssociationEnd = new BadAssociationEnd(this, "End1", edmError);
				csdlSemanticsAssociationEnd = badAssociationEnd;
			}
			if (this.association.End2 != null)
			{
				edmAssociationEnd = new CsdlSemanticsAssociationEnd(this.context, this, this.association.End2);
			}
			else
			{
				EdmError[] edmErrorArray = new EdmError[1];
				edmErrorArray[0] = new EdmError(base.Location, EdmErrorCode.InvalidAssociation, Strings.CsdlParser_InvalidAssociationIncorrectNumberOfEnds(string.Concat(this.Namespace, ".", this.Name)));
				IEdmAssociationEnd badAssociationEnd1 = new BadAssociationEnd(this, "End2", edmErrorArray);
				edmAssociationEnd = badAssociationEnd1;
			}
			return TupleInternal.Create<IEdmAssociationEnd, IEdmAssociationEnd>(csdlSemanticsAssociationEnd, edmAssociationEnd);
		}

		private IEnumerable<EdmError> ComputeErrors()
		{
			List<EdmError> edmErrors = null;
			if (this.association.End1.Name == this.association.End2.Name)
			{
				List<EdmError> edmErrors1 = edmErrors;
				EdmLocation location = this.association.End2.Location;
				EdmLocation edmLocation = location;
				if (location == null)
				{
					edmLocation = base.Location;
				}
				edmErrors = CsdlSemanticsElement.AllocateAndAdd<EdmError>(edmErrors1, new EdmError(edmLocation, EdmErrorCode.AlreadyDefined, Strings.EdmModel_Validator_Semantic_EndNameAlreadyDefinedDuplicate(this.association.End1.Name)));
			}
			List<EdmError> edmErrors2 = edmErrors;
			IEnumerable<EdmError> edmErrors3 = edmErrors2;
			if (edmErrors2 == null)
			{
				edmErrors3 = Enumerable.Empty<EdmError>();
			}
			return edmErrors3;
		}

		private CsdlSemanticsReferentialConstraint ComputeReferentialConstraint()
		{
			if (this.association.Constraint != null)
			{
				return new CsdlSemanticsReferentialConstraint(this, this.association.Constraint);
			}
			else
			{
				return null;
			}
		}
	}
}