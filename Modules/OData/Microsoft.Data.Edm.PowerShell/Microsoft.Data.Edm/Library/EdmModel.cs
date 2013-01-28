using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Library.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmModel : EdmModelBase
	{
		private readonly List<IEdmSchemaElement> elements;

		private readonly Dictionary<IEdmVocabularyAnnotatable, List<IEdmVocabularyAnnotation>> vocabularyAnnotationsDictionary;

		private readonly Dictionary<IEdmStructuredType, List<IEdmStructuredType>> derivedTypeMappings;

		public override IEnumerable<IEdmSchemaElement> SchemaElements
		{
			get
			{
				return this.elements;
			}
		}

		public override IEnumerable<IEdmVocabularyAnnotation> VocabularyAnnotations
		{
			get
			{
				Dictionary<IEdmVocabularyAnnotatable, List<IEdmVocabularyAnnotation>> edmVocabularyAnnotatables = this.vocabularyAnnotationsDictionary;
				return edmVocabularyAnnotatables.SelectMany<KeyValuePair<IEdmVocabularyAnnotatable, List<IEdmVocabularyAnnotation>>, IEdmVocabularyAnnotation>((KeyValuePair<IEdmVocabularyAnnotatable, List<IEdmVocabularyAnnotation>> kvp) => kvp.Value);
			}
		}

		public EdmModel() : base(Enumerable.Empty<IEdmModel>(), new EdmDirectValueAnnotationsManager())
		{
			this.elements = new List<IEdmSchemaElement>();
			this.vocabularyAnnotationsDictionary = new Dictionary<IEdmVocabularyAnnotatable, List<IEdmVocabularyAnnotation>>();
			this.derivedTypeMappings = new Dictionary<IEdmStructuredType, List<IEdmStructuredType>>();
		}

		public void AddElement(IEdmSchemaElement element)
		{
			List<IEdmStructuredType> edmStructuredTypes = null;
			EdmUtil.CheckArgumentNull<IEdmSchemaElement>(element, "element");
			this.elements.Add(element);
			IEdmStructuredType edmStructuredType = element as IEdmStructuredType;
			if (edmStructuredType != null && edmStructuredType.BaseType != null)
			{
				if (!this.derivedTypeMappings.TryGetValue(edmStructuredType.BaseType, out edmStructuredTypes))
				{
					edmStructuredTypes = new List<IEdmStructuredType>();
					this.derivedTypeMappings[edmStructuredType.BaseType] = edmStructuredTypes;
				}
				edmStructuredTypes.Add(edmStructuredType);
			}
			base.RegisterElement(element);
		}

		public void AddElements(IEnumerable<IEdmSchemaElement> newElements)
		{
			EdmUtil.CheckArgumentNull<IEnumerable<IEdmSchemaElement>>(newElements, "newElements");
			foreach (IEdmSchemaElement newElement in newElements)
			{
				this.AddElement(newElement);
			}
		}

		public void AddReferencedModel(IEdmModel model)
		{
			base.AddReferencedModel(model);
		}

		public void AddVocabularyAnnotation(IEdmVocabularyAnnotation annotation)
		{
			List<IEdmVocabularyAnnotation> edmVocabularyAnnotations = null;
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotation>(annotation, "annotation");
			if (annotation.Target != null)
			{
				if (!this.vocabularyAnnotationsDictionary.TryGetValue(annotation.Target, out edmVocabularyAnnotations))
				{
					edmVocabularyAnnotations = new List<IEdmVocabularyAnnotation>();
					this.vocabularyAnnotationsDictionary.Add(annotation.Target, edmVocabularyAnnotations);
				}
				edmVocabularyAnnotations.Add(annotation);
				return;
			}
			else
			{
				throw new InvalidOperationException(Strings.Constructable_VocabularyAnnotationMustHaveTarget);
			}
		}

		public override IEnumerable<IEdmVocabularyAnnotation> FindDeclaredVocabularyAnnotations(IEdmVocabularyAnnotatable element)
		{
			List<IEdmVocabularyAnnotation> edmVocabularyAnnotations = null;
			if (this.vocabularyAnnotationsDictionary.TryGetValue(element, out edmVocabularyAnnotations))
			{
				return edmVocabularyAnnotations;
			}
			else
			{
				return Enumerable.Empty<IEdmVocabularyAnnotation>();
			}
		}

		public override IEnumerable<IEdmStructuredType> FindDirectlyDerivedTypes(IEdmStructuredType baseType)
		{
			List<IEdmStructuredType> edmStructuredTypes = null;
			if (!this.derivedTypeMappings.TryGetValue(baseType, out edmStructuredTypes))
			{
				return Enumerable.Empty<IEdmStructuredType>();
			}
			else
			{
				return edmStructuredTypes;
			}
		}
	}
}