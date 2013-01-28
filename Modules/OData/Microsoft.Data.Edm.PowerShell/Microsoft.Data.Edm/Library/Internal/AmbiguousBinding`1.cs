using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class AmbiguousBinding<TElement> : BadElement
		where TElement : class, IEdmNamedElement
	{
		private readonly List<TElement> bindings;

		public IEnumerable<TElement> Bindings
		{
			get
			{
				return this.bindings;
			}
		}

		public string Name
		{
			get
			{
				TElement tElement = this.bindings.First<TElement>();
				string name = tElement.Name;
				string empty = name;
				if (name == null)
				{
					empty = string.Empty;
				}
				return empty;
			}
		}

		public AmbiguousBinding(TElement first, TElement second)
			: base(new EdmError[] { new EdmError(null, EdmErrorCode.BadAmbiguousElementBinding, Strings.Bad_AmbiguousElementBinding(first.Name)) })
		{
			this.bindings = new List<TElement>();
			this.AddBinding(first);
			this.AddBinding(second);
		}

		public void AddBinding(TElement binding)
		{
			if (!this.bindings.Contains(binding))
			{
				this.bindings.Add(binding);
			}
		}
	}
}