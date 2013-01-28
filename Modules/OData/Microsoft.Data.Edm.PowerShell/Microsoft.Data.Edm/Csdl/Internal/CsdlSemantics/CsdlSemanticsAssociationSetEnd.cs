using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsAssociationSetEnd : CsdlSemanticsElement, IEdmAssociationSetEnd, IEdmElement, IEdmCheckable
	{
		private readonly CsdlSemanticsAssociationSet context;

		private readonly CsdlAssociationSetEnd end;

		private readonly IEdmAssociationEnd role;

		private readonly Cache<CsdlSemanticsAssociationSetEnd, IEdmEntitySet> entitySet;

		private readonly static Func<CsdlSemanticsAssociationSetEnd, IEdmEntitySet> ComputeEntitySetFunc;

		private readonly Cache<CsdlSemanticsAssociationSetEnd, IEnumerable<EdmError>> errorsCache;

		private readonly static Func<CsdlSemanticsAssociationSetEnd, IEnumerable<EdmError>> ComputeErrorsFunc;

		public override CsdlElement Element
		{
			get
			{
				return this.end;
			}
		}

		public IEdmEntitySet EntitySet
		{
			get
			{
				return this.entitySet.GetValue(this, CsdlSemanticsAssociationSetEnd.ComputeEntitySetFunc, null);
			}
		}

		public IEnumerable<EdmError> Errors
		{
			get
			{
				return this.errorsCache.GetValue(this, CsdlSemanticsAssociationSetEnd.ComputeErrorsFunc, null);
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.context.Model;
			}
		}

		public IEdmAssociationEnd Role
		{
			get
			{
				return this.role;
			}
		}

		static CsdlSemanticsAssociationSetEnd()
		{
			CsdlSemanticsAssociationSetEnd.ComputeEntitySetFunc = (CsdlSemanticsAssociationSetEnd me) => me.ComputeEntitySet();
			CsdlSemanticsAssociationSetEnd.ComputeErrorsFunc = (CsdlSemanticsAssociationSetEnd me) => me.ComputeErrors();
		}

		public CsdlSemanticsAssociationSetEnd(CsdlSemanticsAssociationSet context, CsdlAssociationSetEnd end, IEdmAssociationEnd role) : base(end)
		{
			this.entitySet = new Cache<CsdlSemanticsAssociationSetEnd, IEdmEntitySet>();
			this.errorsCache = new Cache<CsdlSemanticsAssociationSetEnd, IEnumerable<EdmError>>();
			this.context = context;
			this.end = end;
			this.role = role;
		}

		private IEdmEntitySet ComputeEntitySet()
		{
			Func<IEdmEntitySet, bool> func = null;
			if (this.end == null)
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(base.Location, EdmErrorCode.NoEntitySetsFoundForType, Strings.EdmModel_Validator_Semantic_NoEntitySetsFoundForType(string.Concat(this.context.Container.FullName(), this.context.Name), this.role.EntityType.FullName(), this.Role.Name));
				IEnumerable<EdmError> edmErrors = edmError;
				IEnumerable<IEdmEntitySet> edmEntitySets = this.context.Container.EntitySets();
				if (func == null)
				{
					func = (IEdmEntitySet set) => set.ElementType == this.role.EntityType;
				}
				IEdmEntitySet edmEntitySet = edmEntitySets.Where<IEdmEntitySet>(func).FirstOrDefault<IEdmEntitySet>();
				IEdmEntitySet badEntitySet = edmEntitySet;
				if (edmEntitySet == null)
				{
					badEntitySet = new BadEntitySet("UnresolvedEntitySet", this.context.Container, edmErrors);
				}
				return badEntitySet;
			}
			else
			{
				IEdmEntitySet edmEntitySet1 = this.context.Container.FindEntitySet(this.end.EntitySet);
				IEdmEntitySet unresolvedEntitySet = edmEntitySet1;
				if (edmEntitySet1 == null)
				{
					unresolvedEntitySet = new UnresolvedEntitySet(this.end.EntitySet, this.context.Container, base.Location);
				}
				return unresolvedEntitySet;
			}
		}

		private IEnumerable<EdmError> ComputeErrors()
		{
			Func<IEdmEntitySet, bool> func = null;
			List<EdmError> edmErrors = new List<EdmError>();
			if (this.Role as UnresolvedAssociationEnd != null)
			{
				edmErrors.AddRange(this.Role.Errors());
			}
			if (this.EntitySet as UnresolvedEntitySet != null)
			{
				edmErrors.AddRange(this.EntitySet.Errors());
			}
			if (this.end == null)
			{
				IEnumerable<IEdmEntitySet> edmEntitySets = this.context.Container.EntitySets();
				if (func == null)
				{
					func = (IEdmEntitySet set) => set.ElementType == this.role.EntityType;
				}
				if (edmEntitySets.Where<IEdmEntitySet>(func).Count<IEdmEntitySet>() > 1)
				{
					edmErrors.Add(new EdmError(base.Location, EdmErrorCode.CannotInferEntitySetWithMultipleSetsPerType, Strings.EdmModel_Validator_Semantic_CannotInferEntitySetWithMultipleSetsPerType(string.Concat(this.context.Container.FullName(), this.context.Name), this.role.EntityType.FullName(), this.Role.Name)));
				}
			}
			return edmErrors;
		}
	}
}