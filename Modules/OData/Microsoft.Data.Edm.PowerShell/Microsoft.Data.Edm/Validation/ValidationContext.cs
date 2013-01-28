using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Validation
{
	internal sealed class ValidationContext
	{
		private readonly List<EdmError> errors;

		private readonly IEdmModel model;

		private readonly Func<object, bool> isBad;

		internal IEnumerable<EdmError> Errors
		{
			get
			{
				return this.errors;
			}
		}

		public IEdmModel Model
		{
			get
			{
				return this.model;
			}
		}

		internal ValidationContext(IEdmModel model, Func<object, bool> isBad)
		{
			this.errors = new List<EdmError>();
			this.model = model;
			this.isBad = isBad;
		}

		public void AddError(EdmLocation location, EdmErrorCode errorCode, string errorMessage)
		{
			this.AddError(new EdmError(location, errorCode, errorMessage));
		}

		public void AddError(EdmError error)
		{
			this.errors.Add(error);
		}

		public bool IsBad(IEdmElement element)
		{
			return this.isBad(element);
		}
	}
}