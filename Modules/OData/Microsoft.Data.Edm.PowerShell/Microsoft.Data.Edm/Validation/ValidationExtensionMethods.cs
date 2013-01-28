using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Validation
{
	internal static class ValidationExtensionMethods
	{
		public static IEnumerable<EdmError> Errors(this IEdmElement element)
		{
			EdmUtil.CheckArgumentNull<IEdmElement>(element, "element");
			return InterfaceValidator.GetStructuralErrors(element);
		}

		public static bool IsBad(this IEdmElement element)
		{
			EdmUtil.CheckArgumentNull<IEdmElement>(element, "element");
			return element.Errors().FirstOrDefault<EdmError>() != null;
		}

		public static IEnumerable<EdmError> TypeErrors(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			return InterfaceValidator.GetStructuralErrors(type).Concat<EdmError>(InterfaceValidator.GetStructuralErrors(type.Definition));
		}
	}
}