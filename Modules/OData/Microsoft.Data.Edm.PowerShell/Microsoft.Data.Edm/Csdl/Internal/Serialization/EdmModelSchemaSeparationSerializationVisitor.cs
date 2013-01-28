using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.Serialization
{
	internal class EdmModelSchemaSeparationSerializationVisitor : EdmModelVisitor
	{
		private bool visitCompleted;

		private Dictionary<string, EdmSchema> modelSchemas;

		private EdmSchema activeSchema;

		public EdmModelSchemaSeparationSerializationVisitor(IEdmModel visitedModel) : base(visitedModel)
		{
			this.modelSchemas = new Dictionary<string, EdmSchema>();
		}

		private void CheckSchemaElementReference(IEdmSchemaElement element)
		{
			this.CheckSchemaElementReference(element.Namespace);
		}

		private void CheckSchemaElementReference(string namespaceName)
		{
			if (this.activeSchema != null)
			{
				this.activeSchema.AddNamespaceUsing(namespaceName);
			}
		}

		public IEnumerable<EdmSchema> GetSchemas()
		{
			if (!this.visitCompleted)
			{
				this.Visit();
			}
			return this.modelSchemas.Values;
		}

		protected override void ProcessComplexType(IEdmComplexType element)
		{
			base.ProcessComplexType(element);
			if (element.BaseComplexType() != null)
			{
				this.CheckSchemaElementReference(element.BaseComplexType());
			}
		}

		protected override void ProcessComplexTypeReference(IEdmComplexTypeReference element)
		{
			this.CheckSchemaElementReference(element.ComplexDefinition());
		}

		protected override void ProcessEntityContainer(IEdmEntityContainer element)
		{
			EdmSchema edmSchema = null;
			string @namespace = element.Namespace;
			if (!this.modelSchemas.TryGetValue(@namespace, out edmSchema))
			{
				edmSchema = new EdmSchema(@namespace);
				this.modelSchemas.Add(edmSchema.Namespace, edmSchema);
			}
			edmSchema.AddEntityContainer(element);
			this.activeSchema = edmSchema;
			base.ProcessEntityContainer(element);
		}

		protected override void ProcessEntityReferenceTypeReference(IEdmEntityReferenceTypeReference element)
		{
			this.CheckSchemaElementReference(element.EntityType());
		}

		protected override void ProcessEntityType(IEdmEntityType element)
		{
			base.ProcessEntityType(element);
			if (element.BaseEntityType() != null)
			{
				this.CheckSchemaElementReference(element.BaseEntityType());
			}
		}

		protected override void ProcessEntityTypeReference(IEdmEntityTypeReference element)
		{
			this.CheckSchemaElementReference(element.EntityDefinition());
		}

		protected override void ProcessEnumType(IEdmEnumType element)
		{
			base.ProcessEnumType(element);
			this.CheckSchemaElementReference(element.UnderlyingType);
		}

		protected override void ProcessEnumTypeReference(IEdmEnumTypeReference element)
		{
			this.CheckSchemaElementReference(element.EnumDefinition());
		}

		protected override void ProcessModel(IEdmModel model)
		{
			this.ProcessElement(model);
			base.VisitSchemaElements(model.SchemaElements);
			base.VisitVocabularyAnnotations(model.VocabularyAnnotations.Where<IEdmVocabularyAnnotation>((IEdmVocabularyAnnotation a) => !a.IsInline(this.Model)));
		}

		protected override void ProcessNavigationProperty(IEdmNavigationProperty property)
		{
			EdmSchema edmSchema = null;
			string associationNamespace = this.Model.GetAssociationNamespace(property);
			if (!this.modelSchemas.TryGetValue(associationNamespace, out edmSchema))
			{
				edmSchema = new EdmSchema(associationNamespace);
				this.modelSchemas.Add(edmSchema.Namespace, edmSchema);
			}
			edmSchema.AddAssociatedNavigationProperty(property);
			edmSchema.AddNamespaceUsing(property.DeclaringEntityType().Namespace);
			edmSchema.AddNamespaceUsing(property.Partner.DeclaringEntityType().Namespace);
			this.activeSchema.AddNamespaceUsing(associationNamespace);
			base.ProcessNavigationProperty(property);
		}

		protected override void ProcessSchemaElement(IEdmSchemaElement element)
		{
			EdmSchema edmSchema = null;
			string @namespace = element.Namespace;
			if (EdmUtil.IsNullOrWhiteSpaceInternal(@namespace))
			{
				@namespace = string.Empty;
			}
			if (!this.modelSchemas.TryGetValue(@namespace, out edmSchema))
			{
				edmSchema = new EdmSchema(@namespace);
				this.modelSchemas.Add(@namespace, edmSchema);
			}
			edmSchema.AddSchemaElement(element);
			this.activeSchema = edmSchema;
			base.ProcessSchemaElement(element);
		}

		protected override void ProcessVocabularyAnnotatable(IEdmVocabularyAnnotatable element)
		{
			base.VisitAnnotations(this.Model.DirectValueAnnotations(element));
			base.VisitVocabularyAnnotations(this.Model.FindDeclaredVocabularyAnnotations(element).Where<IEdmVocabularyAnnotation>((IEdmVocabularyAnnotation a) => a.IsInline(this.Model)));
		}

		protected override void ProcessVocabularyAnnotation(IEdmVocabularyAnnotation annotation)
		{
			EdmSchema edmSchema = null;
			if (!annotation.IsInline(this.Model))
			{
				string schemaNamespace = annotation.GetSchemaNamespace(this.Model);
				string empty = schemaNamespace;
				if (schemaNamespace == null)
				{
					Dictionary<string, EdmSchema> strs = this.modelSchemas;
					string str = strs.Select<KeyValuePair<string, EdmSchema>, string>((KeyValuePair<string, EdmSchema> s) => s.Key).FirstOrDefault<string>();
					empty = str;
					if (str == null)
					{
						empty = string.Empty;
					}
				}
				string str1 = empty;
				if (!this.modelSchemas.TryGetValue(str1, out edmSchema))
				{
					edmSchema = new EdmSchema(str1);
					this.modelSchemas.Add(edmSchema.Namespace, edmSchema);
				}
				edmSchema.AddVocabularyAnnotation(annotation);
				this.activeSchema = edmSchema;
			}
			if (annotation.Term != null)
			{
				this.CheckSchemaElementReference(annotation.Term);
			}
			base.ProcessVocabularyAnnotation(annotation);
		}

		protected void Visit()
		{
			base.VisitEdmModel();
			this.visitCompleted = true;
		}
	}
}