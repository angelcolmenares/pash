using Microsoft.Data.Edm.Validation;
using System.Collections.Generic;

namespace Microsoft.Data.Edm
{
	internal interface IEdmCheckable
	{
		IEnumerable<EdmError> Errors
		{
			get;
		}

	}
}