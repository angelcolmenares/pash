using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library.Internal;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class AmbiguousAssociationBinding : AmbiguousBinding<IEdmAssociation>, IEdmAssociation, IEdmNamedElement, IEdmElement
	{
		private readonly string namespaceName;

		public IEdmAssociationEnd End1
		{
			get
			{
				return new BadAssociationEnd(this, "End1", base.Errors);
			}
		}

		public IEdmAssociationEnd End2
		{
			get
			{
				return new BadAssociationEnd(this, "End2", base.Errors);
			}
		}

		public string Namespace
		{
			get
			{
				return this.namespaceName;
			}
		}

		public CsdlSemanticsReferentialConstraint ReferentialConstraint
		{
			get
			{
				return null;
			}
		}

		public AmbiguousAssociationBinding(IEdmAssociation first, IEdmAssociation second) : base(first, second)
		{
			string @namespace = first.Namespace;
			string empty = @namespace;
			if (@namespace == null)
			{
				empty = string.Empty;
			}
			this.namespaceName = empty;
		}
	}
}