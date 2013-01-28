using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class BadElement : IEdmCheckable, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly IEnumerable<EdmError> errors;

		public IEnumerable<EdmError> Errors
		{
			get
			{
				return this.errors;
			}
		}

		public BadElement(IEnumerable<EdmError> errors)
		{
			this.errors = errors;
		}
	}
}