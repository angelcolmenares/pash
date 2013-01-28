using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Annotations
{
	internal class EdmTypeAnnotation : EdmVocabularyAnnotation, IEdmTypeAnnotation, IEdmVocabularyAnnotation, IEdmElement
	{
		private readonly IEnumerable<IEdmPropertyValueBinding> propertyValueBindings;

		public IEnumerable<IEdmPropertyValueBinding> PropertyValueBindings
		{
			get
			{
				return this.propertyValueBindings;
			}
		}

		public EdmTypeAnnotation(IEdmVocabularyAnnotatable target, IEdmTerm term, IEdmPropertyValueBinding[] propertyValueBindings) : this(target, term, null, propertyValueBindings)
		{
		}

		public EdmTypeAnnotation(IEdmVocabularyAnnotatable target, IEdmTerm term, string qualifier, IEdmPropertyValueBinding[] propertyValueBindings) : this(target, term, qualifier, (IEnumerable<IEdmPropertyValueBinding>)propertyValueBindings)
		{
		}

		public EdmTypeAnnotation(IEdmVocabularyAnnotatable target, IEdmTerm term, string qualifier, IEnumerable<IEdmPropertyValueBinding> propertyValueBindings) : base(target, term, qualifier)
		{
			EdmUtil.CheckArgumentNull<IEnumerable<IEdmPropertyValueBinding>>(propertyValueBindings, "propertyValueBindings");
			this.propertyValueBindings = propertyValueBindings;
		}
	}
}