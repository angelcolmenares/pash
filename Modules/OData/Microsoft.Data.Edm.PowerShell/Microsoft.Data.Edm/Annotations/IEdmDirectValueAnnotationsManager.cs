using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Annotations
{
	internal interface IEdmDirectValueAnnotationsManager
	{
		object GetAnnotationValue(IEdmElement element, string namespaceName, string localName);

		object[] GetAnnotationValues(IEnumerable<IEdmDirectValueAnnotationBinding> annotations);

		IEnumerable<IEdmDirectValueAnnotation> GetDirectValueAnnotations(IEdmElement element);

		void SetAnnotationValue(IEdmElement element, string namespaceName, string localName, object value);

		void SetAnnotationValues(IEnumerable<IEdmDirectValueAnnotationBinding> annotations);
	}
}