using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal abstract class EdmType : EdmElement, IEdmType, IEdmElement
	{
		public abstract EdmTypeKind TypeKind
		{
			get;
		}

		protected EdmType()
		{
		}

		public override string ToString()
		{
			return this.ToTraceString();
		}
	}
}