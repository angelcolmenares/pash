using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsAssociationSet : CsdlSemanticsElement, IEdmAssociationSet, IEdmNamedElement, IEdmElement, IEdmCheckable
	{
		private readonly CsdlSemanticsEntityContainer context;

		private readonly CsdlAssociationSet associationSet;

		private readonly Cache<CsdlSemanticsAssociationSet, TupleInternal<CsdlSemanticsAssociationSetEnd, CsdlSemanticsAssociationSetEnd>> endsCache;

		private readonly static Func<CsdlSemanticsAssociationSet, TupleInternal<CsdlSemanticsAssociationSetEnd, CsdlSemanticsAssociationSetEnd>> ComputeEndsFunc;

		private readonly Cache<CsdlSemanticsAssociationSet, IEdmAssociation> elementTypeCache;

		private readonly static Func<CsdlSemanticsAssociationSet, IEdmAssociation> ComputeElementTypeFunc;

		private readonly Cache<CsdlSemanticsAssociationSet, IEnumerable<EdmError>> errorsCache;

		private readonly static Func<CsdlSemanticsAssociationSet, IEnumerable<EdmError>> ComputeErrorsFunc;

		public IEdmAssociation Association
		{
			get
			{
				return this.elementTypeCache.GetValue(this, CsdlSemanticsAssociationSet.ComputeElementTypeFunc, null);
			}
		}

		public IEdmEntityContainer Container
		{
			get
			{
				return this.context;
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.associationSet;
			}
		}

		public IEdmAssociationSetEnd End1
		{
			get
			{
				return this.Ends.Item1;
			}
		}

		public IEdmAssociationSetEnd End2
		{
			get
			{
				return this.Ends.Item2;
			}
		}

		private TupleInternal<CsdlSemanticsAssociationSetEnd, CsdlSemanticsAssociationSetEnd> Ends
		{
			get
			{
				return this.endsCache.GetValue(this, CsdlSemanticsAssociationSet.ComputeEndsFunc, null);
			}
		}

		public IEnumerable<EdmError> Errors
		{
			get
			{
				return this.errorsCache.GetValue(this, CsdlSemanticsAssociationSet.ComputeErrorsFunc, null);
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
				return this.associationSet.Name;
			}
		}

		static CsdlSemanticsAssociationSet()
		{
			CsdlSemanticsAssociationSet.ComputeEndsFunc = (CsdlSemanticsAssociationSet me) => me.ComputeEnds();
			CsdlSemanticsAssociationSet.ComputeElementTypeFunc = (CsdlSemanticsAssociationSet me) => me.ComputeElementType();
			CsdlSemanticsAssociationSet.ComputeErrorsFunc = (CsdlSemanticsAssociationSet me) => me.ComputeErrors();
		}

		public CsdlSemanticsAssociationSet(CsdlSemanticsEntityContainer context, CsdlAssociationSet associationSet) : base(associationSet)
		{
			this.endsCache = new Cache<CsdlSemanticsAssociationSet, TupleInternal<CsdlSemanticsAssociationSetEnd, CsdlSemanticsAssociationSetEnd>>();
			this.elementTypeCache = new Cache<CsdlSemanticsAssociationSet, IEdmAssociation>();
			this.errorsCache = new Cache<CsdlSemanticsAssociationSet, IEnumerable<EdmError>>();
			this.context = context;
			this.associationSet = associationSet;
		}

		private IEdmAssociation ComputeElementType()
		{
			IEdmAssociation edmAssociation = this.context.Context.FindAssociation(this.associationSet.Association);
			IEdmAssociation unresolvedAssociation = edmAssociation;
			if (edmAssociation == null)
			{
				unresolvedAssociation = new UnresolvedAssociation(this.associationSet.Association, base.Location);
			}
			return unresolvedAssociation;
		}

		private TupleInternal<CsdlSemanticsAssociationSetEnd, CsdlSemanticsAssociationSetEnd> ComputeEnds()
		{
			IEdmAssociationEnd end1;
			IEdmAssociationEnd end2;
			CsdlAssociationSetEnd csdlAssociationSetEnd = this.associationSet.End1;
			CsdlAssociationSetEnd end21 = this.associationSet.End2;
			IEdmAssociationEnd role = null;
			IEdmAssociationEnd unresolvedAssociationEnd = null;
			bool flag = false;
			bool flag1 = false;
			if (csdlAssociationSetEnd != null)
			{
				role = this.GetRole(csdlAssociationSetEnd);
				flag = role is IUnresolvedElement;
			}
			if (end21 != null)
			{
				unresolvedAssociationEnd = this.GetRole(end21);
				flag1 = unresolvedAssociationEnd is IUnresolvedElement;
			}
			if (csdlAssociationSetEnd == null)
			{
				if (!flag1)
				{
					if (unresolvedAssociationEnd == null)
					{
						role = this.Association.End1;
					}
					else
					{
						if (unresolvedAssociationEnd != this.Association.End1)
						{
							end2 = this.Association.End1;
						}
						else
						{
							end2 = this.Association.End2;
						}
						role = end2;
					}
				}
				else
				{
					role = new UnresolvedAssociationEnd(this.Association, "End1", base.Location);
					flag = true;
				}
			}
			if (end21 == null)
			{
				if (!flag)
				{
					if (role != this.Association.End1)
					{
						end1 = this.Association.End1;
					}
					else
					{
						end1 = this.Association.End2;
					}
					unresolvedAssociationEnd = end1;
				}
				else
				{
					unresolvedAssociationEnd = new UnresolvedAssociationEnd(this.Association, "End2", base.Location);
				}
			}
			return TupleInternal.Create<CsdlSemanticsAssociationSetEnd, CsdlSemanticsAssociationSetEnd>(new CsdlSemanticsAssociationSetEnd(this, this.associationSet.End1, role), new CsdlSemanticsAssociationSetEnd(this, this.associationSet.End2, unresolvedAssociationEnd));
		}

		private IEnumerable<EdmError> ComputeErrors()
		{
			List<EdmError> edmErrors = null;
			if (this.Association as UnresolvedAssociation != null)
			{
				edmErrors = CsdlSemanticsElement.AllocateAndAdd<EdmError>(edmErrors, this.Association.Errors());
			}
			if (this.End1.Role != null && this.End2.Role != null && this.End1.Role.Name == this.End2.Role.Name)
			{
				edmErrors = CsdlSemanticsElement.AllocateAndAdd<EdmError>(edmErrors, new EdmError(this.End2.Location(), EdmErrorCode.InvalidName, Strings.EdmModel_Validator_Semantic_DuplicateEndName(this.End1.Role.Name)));
			}
			List<EdmError> edmErrors1 = edmErrors;
			IEnumerable<EdmError> edmErrors2 = edmErrors1;
			if (edmErrors1 == null)
			{
				edmErrors2 = Enumerable.Empty<EdmError>();
			}
			return edmErrors2;
		}

		private IEdmAssociationEnd GetRole(CsdlAssociationSetEnd end)
		{
			Func<IEdmAssociationEnd, bool> func = (IEdmAssociationEnd endCandidate) => {
				if (endCandidate == null)
				{
					return false;
				}
				else
				{
					return endCandidate.Name == end.Role;
				}
			}
			;
			if (!func(this.Association.End1))
			{
				if (!func(this.Association.End2))
				{
					return new UnresolvedAssociationEnd(this.Association, end.Role, end.Location);
				}
				else
				{
					return this.Association.End2;
				}
			}
			else
			{
				return this.Association.End1;
			}
		}
	}
}