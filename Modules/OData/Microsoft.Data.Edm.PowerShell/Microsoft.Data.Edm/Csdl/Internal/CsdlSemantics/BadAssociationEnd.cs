using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class BadAssociationEnd : BadElement, IEdmAssociationEnd, IEdmNamedElement, IEdmElement
	{
		private readonly string role;

		private readonly IEdmAssociation declaringAssociation;

		private readonly Cache<BadAssociationEnd, BadEntityType> type;

		private readonly static Func<BadAssociationEnd, BadEntityType> ComputeTypeFunc;

		public IEdmAssociation DeclaringAssociation
		{
			get
			{
				return this.declaringAssociation;
			}
		}

		public IEdmEntityType EntityType
		{
			get
			{
				return this.type.GetValue(this, BadAssociationEnd.ComputeTypeFunc, null);
			}
		}

		public EdmMultiplicity Multiplicity
		{
			get
			{
				return EdmMultiplicity.Unknown;
			}
		}

		public string Name
		{
			get
			{
				return this.role;
			}
		}

		public EdmOnDeleteAction OnDelete
		{
			get
			{
				return EdmOnDeleteAction.None;
			}
		}

		static BadAssociationEnd()
		{
			BadAssociationEnd.ComputeTypeFunc = (BadAssociationEnd me) => me.ComputeType();
		}

		public BadAssociationEnd(IEdmAssociation declaringAssociation, string role, IEnumerable<EdmError> errors) : base(errors)
		{
			this.type = new Cache<BadAssociationEnd, BadEntityType>();
			string str = role;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			this.role = empty;
			this.declaringAssociation = declaringAssociation;
		}

		private BadEntityType ComputeType()
		{
			return new BadEntityType(string.Concat(this.declaringAssociation.Name, ".", this.role), base.Errors);
		}
	}
}