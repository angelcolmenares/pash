using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Library.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsDirectValueAnnotationsManager : EdmDirectValueAnnotationsManager
	{
		public CsdlSemanticsDirectValueAnnotationsManager()
		{
		}

		protected override IEnumerable<IEdmDirectValueAnnotation> GetAttachedAnnotations(IEdmElement element)
		{
			CsdlSemanticsElement csdlSemanticsElement = element as CsdlSemanticsElement;
			if (csdlSemanticsElement == null)
			{
				return Enumerable.Empty<IEdmDirectValueAnnotation>();
			}
			else
			{
				return csdlSemanticsElement.DirectValueAnnotations;
			}
		}
	}
}