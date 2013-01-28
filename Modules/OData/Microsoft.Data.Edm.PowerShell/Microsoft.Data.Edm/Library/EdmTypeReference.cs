using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal abstract class EdmTypeReference : EdmElement, IEdmTypeReference, IEdmElement
	{
		private readonly IEdmType definition;

		private readonly bool isNullable;

		public IEdmType Definition
		{
			get
			{
				return this.definition;
			}
		}

		public bool IsNullable
		{
			get
			{
				return this.isNullable;
			}
		}

		protected EdmTypeReference(IEdmType definition, bool isNullable)
		{
			EdmUtil.CheckArgumentNull<IEdmType>(definition, "definition");
			this.definition = definition;
			this.isNullable = isNullable;
		}

		public override string ToString()
		{
			return this.ToTraceString();
		}
	}
}