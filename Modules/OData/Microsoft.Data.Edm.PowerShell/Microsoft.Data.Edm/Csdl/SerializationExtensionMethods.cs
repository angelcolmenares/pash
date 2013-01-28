using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Validation.Internal;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl
{
	internal static class SerializationExtensionMethods
	{
		private const char AssociationNameEscapeChar = '\u005F';

		private const string AssociationNameEscapeString = "_";

		private const string AssociationNameEscapeStringEscaped = "__";

		private static string EscapeName(string name)
		{
			return name.Replace("_", "__");
		}

		public static void GetAssociationAnnotations(this IEdmModel model, IEdmNavigationProperty property, out IEnumerable<IEdmDirectValueAnnotation> annotations, out IEnumerable<IEdmDirectValueAnnotation> end1Annotations, out IEnumerable<IEdmDirectValueAnnotation> end2Annotations, out IEnumerable<IEdmDirectValueAnnotation> constraintAnnotations)
		{
			annotations = null;
			end1Annotations = null;
			end2Annotations = null;
			constraintAnnotations = null;
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			property.PopulateCaches();
			SerializationExtensionMethods.AssociationAnnotations annotationValue = model.GetAnnotationValue<SerializationExtensionMethods.AssociationAnnotations>(property, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationAnnotations");
			if (annotationValue == null)
			{
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations = Enumerable.Empty<IEdmDirectValueAnnotation>();
				annotations = edmDirectValueAnnotations;
				end1Annotations = edmDirectValueAnnotations;
				end2Annotations = edmDirectValueAnnotations;
				constraintAnnotations = edmDirectValueAnnotations;
				return;
			}
			else
			{
				IEnumerable<IEdmDirectValueAnnotation> enumerablePointers = annotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations1 = annotationValue.Annotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations2 = edmDirectValueAnnotations1;
				if (edmDirectValueAnnotations1 == null)
				{
					edmDirectValueAnnotations2 = Enumerable.Empty<IEdmDirectValueAnnotation>();
				}
				(enumerablePointers) = edmDirectValueAnnotations2;
				IEnumerable<IEdmDirectValueAnnotation> enumerablePointers1 = end1Annotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations3 = annotationValue.End1Annotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations4 = edmDirectValueAnnotations3;
				if (edmDirectValueAnnotations3 == null)
				{
					edmDirectValueAnnotations4 = Enumerable.Empty<IEdmDirectValueAnnotation>();
				}
				(enumerablePointers1) = edmDirectValueAnnotations4;
				IEnumerable<IEdmDirectValueAnnotation> enumerablePointers2 = end2Annotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations5 = annotationValue.End2Annotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations6 = edmDirectValueAnnotations5;
				if (edmDirectValueAnnotations5 == null)
				{
					edmDirectValueAnnotations6 = Enumerable.Empty<IEdmDirectValueAnnotation>();
				}
				(enumerablePointers2) = edmDirectValueAnnotations6;
				IEnumerable<IEdmDirectValueAnnotation> enumerablePointers3 = constraintAnnotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations7 = annotationValue.ConstraintAnnotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations8 = edmDirectValueAnnotations7;
				if (edmDirectValueAnnotations7 == null)
				{
					edmDirectValueAnnotations8 = Enumerable.Empty<IEdmDirectValueAnnotation>();
				}
				(enumerablePointers3) = edmDirectValueAnnotations8;
				return;
			}
		}

		public static string GetAssociationEndName(this IEdmModel model, IEdmNavigationProperty property)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			property.PopulateCaches();
			string annotationValue = model.GetAnnotationValue<string>(property, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationEndName");
			string name = annotationValue;
			if (annotationValue == null)
			{
				name = property.Partner.Name;
			}
			return name;
		}

		public static string GetAssociationFullName(this IEdmModel model, IEdmNavigationProperty property)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			property.PopulateCaches();
			return string.Concat(model.GetAssociationNamespace(property), ".", model.GetAssociationName(property));
		}

		public static string GetAssociationName(this IEdmModel model, IEdmNavigationProperty property)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			property.PopulateCaches();
			string annotationValue = model.GetAnnotationValue<string>(property, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationName");
			if (annotationValue == null)
			{
				IEdmNavigationProperty primary = property.GetPrimary();
				IEdmNavigationProperty partner = primary.Partner;
				annotationValue = string.Concat(SerializationExtensionMethods.GetQualifiedAndEscapedPropertyName(partner), (char)95, SerializationExtensionMethods.GetQualifiedAndEscapedPropertyName(primary));
			}
			return annotationValue;
		}

		public static string GetAssociationNamespace(this IEdmModel model, IEdmNavigationProperty property)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			property.PopulateCaches();
			string annotationValue = model.GetAnnotationValue<string>(property, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationNamespace");
			if (annotationValue == null)
			{
				annotationValue = property.GetPrimary().DeclaringEntityType().Namespace;
			}
			return annotationValue;
		}

		public static void GetAssociationSetAnnotations(this IEdmModel model, IEdmEntitySet entitySet, IEdmNavigationProperty property, out IEnumerable<IEdmDirectValueAnnotation> annotations, out IEnumerable<IEdmDirectValueAnnotation> end1Annotations, out IEnumerable<IEdmDirectValueAnnotation> end2Annotations)
		{
			annotations = null;
			end1Annotations = null;
			end2Annotations = null;
			SerializationExtensionMethods.AssociationSetAnnotations associationSetAnnotation = null;
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmEntitySet>(entitySet, "entitySet");
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			Dictionary<string, SerializationExtensionMethods.AssociationSetAnnotations> annotationValue = model.GetAnnotationValue<Dictionary<string, SerializationExtensionMethods.AssociationSetAnnotations>>(entitySet, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationSetAnnotations");
			if (annotationValue == null || !annotationValue.TryGetValue(property.Name, out associationSetAnnotation))
			{
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations = Enumerable.Empty<IEdmDirectValueAnnotation>();
				annotations = edmDirectValueAnnotations;
				end1Annotations = edmDirectValueAnnotations;
				end2Annotations = edmDirectValueAnnotations;
				return;
			}
			else
			{
				IEnumerable<IEdmDirectValueAnnotation> enumerablePointers = annotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations1 = associationSetAnnotation.Annotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations2 = edmDirectValueAnnotations1;
				if (edmDirectValueAnnotations1 == null)
				{
					edmDirectValueAnnotations2 = Enumerable.Empty<IEdmDirectValueAnnotation>();
				}
				(enumerablePointers) = edmDirectValueAnnotations2;
				IEnumerable<IEdmDirectValueAnnotation> enumerablePointers1 = end1Annotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations3 = associationSetAnnotation.End1Annotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations4 = edmDirectValueAnnotations3;
				if (edmDirectValueAnnotations3 == null)
				{
					edmDirectValueAnnotations4 = Enumerable.Empty<IEdmDirectValueAnnotation>();
				}
				(enumerablePointers1) = edmDirectValueAnnotations4;
				IEnumerable<IEdmDirectValueAnnotation> enumerablePointers2 = end2Annotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations5 = associationSetAnnotation.End2Annotations;
				IEnumerable<IEdmDirectValueAnnotation> edmDirectValueAnnotations6 = edmDirectValueAnnotations5;
				if (edmDirectValueAnnotations5 == null)
				{
					edmDirectValueAnnotations6 = Enumerable.Empty<IEdmDirectValueAnnotation>();
				}
				(enumerablePointers2) = edmDirectValueAnnotations6;
				return;
			}
		}

		public static string GetAssociationSetName(this IEdmModel model, IEdmEntitySet entitySet, IEdmNavigationProperty property)
		{
			string str = null;
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmEntitySet>(entitySet, "entitySet");
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			Dictionary<string, string> annotationValue = model.GetAnnotationValue<Dictionary<string, string>>(entitySet, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationSetName");
			if (annotationValue == null || !annotationValue.TryGetValue(property.Name, out str))
			{
				str = string.Concat(model.GetAssociationName(property), "Set");
			}
			return str;
		}

		public static Version GetDataServiceVersion(this IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			return model.GetAnnotationValue<Version>(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "DataServiceVersion");
		}

		public static Version GetEdmxVersion(this IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			return model.GetAnnotationValue<Version>(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "EdmxVersion");
		}

		public static Version GetMaxDataServiceVersion(this IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			return model.GetAnnotationValue<Version>(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "MaxDataServiceVersion");
		}

		public static string GetNamespaceAlias(this IEdmModel model, string namespaceName)
		{
			VersioningDictionary<string, string> annotationValue = model.GetAnnotationValue<VersioningDictionary<string, string>>(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "NamespaceAlias");
			return annotationValue.Get(namespaceName);
		}

		internal static VersioningDictionary<string, string> GetNamespaceAliases(this IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			return model.GetAnnotationValue<VersioningDictionary<string, string>>(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "NamespaceAlias");
		}

		public static IEnumerable<KeyValuePair<string, string>> GetNamespacePrefixMappings(this IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			return model.GetAnnotationValue<IEnumerable<KeyValuePair<string, string>>>(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "NamespacePrefix");
		}

		public static IEdmNavigationProperty GetPrimary(this IEdmNavigationProperty property)
		{
			if (!property.IsPrincipal)
			{
				IEdmNavigationProperty partner = property.Partner;
				if (!partner.IsPrincipal)
				{
					int num = string.Compare(property.Name, partner.Name, StringComparison.Ordinal);
					if (num == 0)
					{
						num = string.Compare(property.DeclaringEntityType().FullName(), partner.DeclaringEntityType().FullName(), StringComparison.Ordinal);
					}
					if (num > 0)
					{
						return property;
					}
					else
					{
						return partner;
					}
				}
				else
				{
					return partner;
				}
			}
			else
			{
				return property;
			}
		}

		private static string GetQualifiedAndEscapedPropertyName(IEdmNavigationProperty property)
		{
			object[] objArray = new object[5];
			objArray[0] = SerializationExtensionMethods.EscapeName(property.DeclaringEntityType().Namespace).Replace('.', '\u005F');
			objArray[1] = (char)95;
			objArray[2] = SerializationExtensionMethods.EscapeName(property.DeclaringEntityType().Name);
			objArray[3] = (char)95;
			objArray[4] = SerializationExtensionMethods.EscapeName(property.Name);
			return string.Concat(objArray);
		}

		public static string GetSchemaNamespace(this IEdmVocabularyAnnotation annotation, IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotation>(annotation, "annotation");
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			return model.GetAnnotationValue<string>(annotation, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "SchemaNamespace");
		}

		public static EdmVocabularyAnnotationSerializationLocation? GetSerializationLocation(this IEdmVocabularyAnnotation annotation, IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotation>(annotation, "annotation");
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			return (EdmVocabularyAnnotationSerializationLocation?)(model.GetAnnotationValue(annotation, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AnnotationSerializationLocation") as EdmVocabularyAnnotationSerializationLocation?);
		}

		internal static bool IsInline(this IEdmVocabularyAnnotation annotation, IEdmModel model)
		{
			bool hasValue;
			EdmVocabularyAnnotationSerializationLocation? serializationLocation = annotation.GetSerializationLocation(model);
			if (serializationLocation.GetValueOrDefault() != EdmVocabularyAnnotationSerializationLocation.Inline)
			{
				hasValue = false;
			}
			else
			{
				hasValue = serializationLocation.HasValue;
			}
			if (hasValue)
			{
				return true;
			}
			else
			{
				return annotation.TargetString() == null;
			}
		}

		public static bool IsSerializedAsElement(this IEdmValue value, IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmValue>(value, "value");
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			bool? annotationValue = (bool?)(model.GetAnnotationValue(value, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "IsSerializedAsElement") as bool?);
			if (annotationValue.HasValue)
			{
				return annotationValue.GetValueOrDefault();
			}
			else
			{
				return false;
			}
		}

		public static bool? IsValueExplicit(this IEdmEnumMember member, IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmEnumMember>(member, "member");
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			return (bool?)(model.GetAnnotationValue(member, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "IsEnumMemberValueExplicit") as bool?);
		}

		private static void PopulateCaches (this IEdmNavigationProperty property)
		{
			var p1 = property.Partner;
			var p2 = property.IsPrincipal;
			var p3 = property.DependentProperties;

			if (p1 == null) {
			}
			if (p2) {
			}
			if (p3 == null) {
			}
		}

		public static void SetAssociationAnnotations(this IEdmModel model, IEdmNavigationProperty property, IEnumerable<IEdmDirectValueAnnotation> annotations, IEnumerable<IEdmDirectValueAnnotation> end1Annotations, IEnumerable<IEdmDirectValueAnnotation> end2Annotations, IEnumerable<IEdmDirectValueAnnotation> constraintAnnotations)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			if (annotations != null && annotations.FirstOrDefault<IEdmDirectValueAnnotation>() != null || end1Annotations != null && end1Annotations.FirstOrDefault<IEdmDirectValueAnnotation>() != null || end2Annotations != null && end2Annotations.FirstOrDefault<IEdmDirectValueAnnotation>() != null || constraintAnnotations != null && constraintAnnotations.FirstOrDefault<IEdmDirectValueAnnotation>() != null)
			{
				SerializationExtensionMethods.AssociationAnnotations associationAnnotation = new SerializationExtensionMethods.AssociationAnnotations();
				associationAnnotation.Annotations = annotations;
				associationAnnotation.End1Annotations = end1Annotations;
				associationAnnotation.End2Annotations = end2Annotations;
				associationAnnotation.ConstraintAnnotations = constraintAnnotations;
				model.SetAnnotationValue(property, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationAnnotations", associationAnnotation);
			}
		}

		public static void SetAssociationEndName(this IEdmModel model, IEdmNavigationProperty property, string association)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			model.SetAnnotationValue(property, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationEndName", association);
		}

		public static void SetAssociationName(this IEdmModel model, IEdmNavigationProperty property, string associationName)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			EdmUtil.CheckArgumentNull<string>(associationName, "associationName");
			model.SetAnnotationValue(property, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationName", associationName);
		}

		public static void SetAssociationNamespace(this IEdmModel model, IEdmNavigationProperty property, string associationNamespace)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			EdmUtil.CheckArgumentNull<string>(associationNamespace, "associationNamespace");
			model.SetAnnotationValue(property, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationNamespace", associationNamespace);
		}

		public static void SetAssociationSetAnnotations(this IEdmModel model, IEdmEntitySet entitySet, IEdmNavigationProperty property, IEnumerable<IEdmDirectValueAnnotation> annotations, IEnumerable<IEdmDirectValueAnnotation> end1Annotations, IEnumerable<IEdmDirectValueAnnotation> end2Annotations)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmEntitySet>(entitySet, "property");
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			if (annotations != null && annotations.FirstOrDefault<IEdmDirectValueAnnotation>() != null || end1Annotations != null && end1Annotations.FirstOrDefault<IEdmDirectValueAnnotation>() != null || end2Annotations != null && end2Annotations.FirstOrDefault<IEdmDirectValueAnnotation>() != null)
			{
				Dictionary<string, SerializationExtensionMethods.AssociationSetAnnotations> annotationValue = model.GetAnnotationValue<Dictionary<string, SerializationExtensionMethods.AssociationSetAnnotations>>(entitySet, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationSetAnnotations");
				if (annotationValue == null)
				{
					annotationValue = new Dictionary<string, SerializationExtensionMethods.AssociationSetAnnotations>();
					model.SetAnnotationValue(entitySet, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationSetAnnotations", annotationValue);
				}
				SerializationExtensionMethods.AssociationSetAnnotations associationSetAnnotation = new SerializationExtensionMethods.AssociationSetAnnotations();
				associationSetAnnotation.Annotations = annotations;
				associationSetAnnotation.End1Annotations = end1Annotations;
				associationSetAnnotation.End2Annotations = end2Annotations;
				annotationValue[property.Name] = associationSetAnnotation;
			}
		}

		public static void SetAssociationSetName(this IEdmModel model, IEdmEntitySet entitySet, IEdmNavigationProperty property, string associationSet)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmEntitySet>(entitySet, "entitySet");
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			Dictionary<string, string> annotationValue = model.GetAnnotationValue<Dictionary<string, string>>(entitySet, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationSetName");
			if (annotationValue == null)
			{
				annotationValue = new Dictionary<string, string>();
				model.SetAnnotationValue(entitySet, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AssociationSetName", annotationValue);
			}
			annotationValue[property.Name] = associationSet;
		}

		public static void SetDataServiceVersion(this IEdmModel model, Version version)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			model.SetAnnotationValue(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "DataServiceVersion", version);
		}

		public static void SetEdmxVersion(this IEdmModel model, Version version)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			model.SetAnnotationValue(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "EdmxVersion", version);
		}

		public static void SetIsSerializedAsElement(this IEdmValue value, IEdmModel model, bool isSerializedAsElement)
		{
			EdmError edmError = null;
			EdmUtil.CheckArgumentNull<IEdmValue>(value, "value");
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			if (!isSerializedAsElement || ValidationHelper.ValidateValueCanBeWrittenAsXmlElementAnnotation(value, null, null, out edmError))
			{
				model.SetAnnotationValue(value, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "IsSerializedAsElement", isSerializedAsElement);
				return;
			}
			else
			{
				throw new InvalidOperationException(edmError.ToString());
			}
		}

		public static void SetIsValueExplicit(this IEdmEnumMember member, IEdmModel model, bool? isExplicit)
		{
			EdmUtil.CheckArgumentNull<IEdmEnumMember>(member, "member");
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			model.SetAnnotationValue(member, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "IsEnumMemberValueExplicit", isExplicit);
		}

		public static void SetMaxDataServiceVersion(this IEdmModel model, Version version)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			model.SetAnnotationValue(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "MaxDataServiceVersion", version);
		}

		public static void SetNamespaceAlias(this IEdmModel model, string namespaceName, string alias)
		{
			VersioningDictionary<string, string> annotationValue = model.GetAnnotationValue<VersioningDictionary<string, string>>(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "NamespaceAlias");
			if (annotationValue == null)
			{
				annotationValue = VersioningDictionary<string, string>.Create(new Func<string, string, int>(string.CompareOrdinal));
			}
			if (alias != null)
			{
				annotationValue = annotationValue.Set(namespaceName, alias);
			}
			else
			{
				annotationValue = annotationValue.Remove(namespaceName);
			}
			model.SetAnnotationValue(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "NamespaceAlias", annotationValue);
		}

		public static void SetNamespacePrefixMappings(this IEdmModel model, IEnumerable<KeyValuePair<string, string>> mappings)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			model.SetAnnotationValue(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "NamespacePrefix", mappings);
		}

		public static void SetSchemaNamespace(this IEdmVocabularyAnnotation annotation, IEdmModel model, string schemaNamespace)
		{
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotation>(annotation, "annotation");
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			model.SetAnnotationValue(annotation, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "SchemaNamespace", schemaNamespace);
		}

		public static void SetSerializationLocation(this IEdmVocabularyAnnotation annotation, IEdmModel model, EdmVocabularyAnnotationSerializationLocation? location)
		{
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotation>(annotation, "annotation");
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			model.SetAnnotationValue(annotation, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "AnnotationSerializationLocation", location);
		}

		internal static string TargetString(this IEdmVocabularyAnnotation annotation)
		{
			return EdmUtil.FullyQualifiedName(annotation.Target);
		}

		private class AssociationAnnotations
		{
			public IEnumerable<IEdmDirectValueAnnotation> Annotations
			{
				get;
				set;
			}

			public IEnumerable<IEdmDirectValueAnnotation> ConstraintAnnotations
			{
				get;
				set;
			}

			public IEnumerable<IEdmDirectValueAnnotation> End1Annotations
			{
				get;
				set;
			}

			public IEnumerable<IEdmDirectValueAnnotation> End2Annotations
			{
				get;
				set;
			}

			public AssociationAnnotations()
			{
			}
		}

		private class AssociationSetAnnotations
		{
			public IEnumerable<IEdmDirectValueAnnotation> Annotations
			{
				get;
				set;
			}

			public IEnumerable<IEdmDirectValueAnnotation> End1Annotations
			{
				get;
				set;
			}

			public IEnumerable<IEdmDirectValueAnnotation> End2Annotations
			{
				get;
				set;
			}

			public AssociationSetAnnotations()
			{
			}
		}
	}
}