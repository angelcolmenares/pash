using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Serialization
{
	internal class EdmSchema
	{
		private readonly string schemaNamespace;

		private readonly List<IEdmSchemaElement> schemaElements;

		private readonly List<IEdmNavigationProperty> associationNavigationProperties;

		private readonly List<IEdmEntityContainer> entityContainers;

		private readonly Dictionary<string, List<IEdmVocabularyAnnotation>> annotations;

		private readonly List<string> usedNamespaces;

		public List<IEdmNavigationProperty> AssociationNavigationProperties
		{
			get
			{
				return this.associationNavigationProperties;
			}
		}

		public List<IEdmEntityContainer> EntityContainers
		{
			get
			{
				return this.entityContainers;
			}
		}

		public string Namespace
		{
			get
			{
				return this.schemaNamespace;
			}
		}

		public IEnumerable<string> NamespaceUsings
		{
			get
			{
				return this.usedNamespaces;
			}
		}

		public IEnumerable<KeyValuePair<string, List<IEdmVocabularyAnnotation>>> OutOfLineAnnotations
		{
			get
			{
				return this.annotations;
			}
		}

		public List<IEdmSchemaElement> SchemaElements
		{
			get
			{
				return this.schemaElements;
			}
		}

		public EdmSchema(string namespaceString)
		{
			this.schemaNamespace = namespaceString;
			this.schemaElements = new List<IEdmSchemaElement>();
			this.entityContainers = new List<IEdmEntityContainer>();
			this.associationNavigationProperties = new List<IEdmNavigationProperty>();
			this.annotations = new Dictionary<string, List<IEdmVocabularyAnnotation>>();
			this.usedNamespaces = new List<string>();
		}

		internal void AddAssociatedNavigationProperty(IEdmNavigationProperty property)
		{
			this.associationNavigationProperties.Add(property);
		}

		public void AddEntityContainer(IEdmEntityContainer container)
		{
			this.entityContainers.Add(container);
		}

		public void AddNamespaceUsing(string usedNamespace)
		{
			if (usedNamespace != "Edm" && !this.usedNamespaces.Contains(usedNamespace))
			{
				this.usedNamespaces.Add(usedNamespace);
			}
		}

		public void AddSchemaElement(IEdmSchemaElement element)
		{
			this.schemaElements.Add(element);
		}

		public void AddVocabularyAnnotation(IEdmVocabularyAnnotation annotation)
		{
			List<IEdmVocabularyAnnotation> edmVocabularyAnnotations = null;
			if (!this.annotations.TryGetValue(annotation.TargetString(), out edmVocabularyAnnotations))
			{
				edmVocabularyAnnotations = new List<IEdmVocabularyAnnotation>();
				this.annotations[annotation.TargetString()] = edmVocabularyAnnotations;
			}
			edmVocabularyAnnotations.Add(annotation);
		}
	}
}