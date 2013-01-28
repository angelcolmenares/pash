using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Evaluation;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm
{
	internal static class ExtensionMethods
	{
		private readonly static Func<IEdmModel, string, IEdmSchemaType> findType;

		private readonly static Func<IEdmModel, string, IEdmValueTerm> findValueTerm;

		private readonly static Func<IEdmModel, string, IEnumerable<IEdmFunction>> findFunctions;

		private readonly static Func<IEdmModel, string, IEdmEntityContainer> findEntityContainer;

		private readonly static Func<IEnumerable<IEdmFunction>, IEnumerable<IEdmFunction>, IEnumerable<IEdmFunction>> mergeFunctions;

		static ExtensionMethods()
		{
			ExtensionMethods.findType = (IEdmModel model, string qualifiedName) => model.FindDeclaredType(qualifiedName);
			ExtensionMethods.findValueTerm = (IEdmModel model, string qualifiedName) => model.FindDeclaredValueTerm(qualifiedName);
			ExtensionMethods.findFunctions = (IEdmModel model, string qualifiedName) => model.FindDeclaredFunctions(qualifiedName);
			ExtensionMethods.findEntityContainer = (IEdmModel model, string qualifiedName) => model.FindDeclaredEntityContainer(qualifiedName);
			ExtensionMethods.mergeFunctions = (IEnumerable<IEdmFunction> f1, IEnumerable<IEdmFunction> f2) => f1.Concat<IEdmFunction>(f2);
		}

		private static T AnnotationValue<T>(object annotation)
		where T : class
		{
			if (annotation == null)
			{
				T t = default(T);
				return t;
			}
			else
			{
				T t1 = (T)(annotation as T);
				if (t1 == null)
				{
					throw new InvalidOperationException(Strings.Annotations_TypeMismatch(annotation.GetType().Name, typeof(T).Name));
				}
				else
				{
					return t1;
				}
			}
		}

		public static IEdmComplexType BaseComplexType(this IEdmComplexType type)
		{
			EdmUtil.CheckArgumentNull<IEdmComplexType>(type, "type");
			return type.BaseType as IEdmComplexType;
		}

		public static IEdmComplexType BaseComplexType(this IEdmComplexTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmComplexTypeReference>(type, "type");
			return type.ComplexDefinition().BaseComplexType();
		}

		public static IEdmEntityType BaseEntityType(this IEdmEntityType type)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityType>(type, "type");
			return type.BaseType as IEdmEntityType;
		}

		public static IEdmEntityType BaseEntityType(this IEdmEntityTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityTypeReference>(type, "type");
			return type.EntityDefinition().BaseEntityType();
		}

		public static IEdmStructuredType BaseType(this IEdmStructuredTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmStructuredTypeReference>(type, "type");
			return type.StructuredDefinition().BaseType;
		}

		public static IEdmCollectionType CollectionDefinition(this IEdmCollectionTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmCollectionTypeReference>(type, "type");
			return (IEdmCollectionType)type.Definition;
		}

		public static IEdmComplexType ComplexDefinition(this IEdmComplexTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmComplexTypeReference>(type, "type");
			return (IEdmComplexType)type.Definition;
		}

		public static IEnumerable<IEdmNavigationProperty> DeclaredNavigationProperties(this IEdmEntityType type)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityType>(type, "type");
			return type.DeclaredProperties.OfType<IEdmNavigationProperty>();
		}

		public static IEnumerable<IEdmNavigationProperty> DeclaredNavigationProperties(this IEdmEntityTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityTypeReference>(type, "type");
			return type.EntityDefinition().DeclaredNavigationProperties();
		}

		public static IEnumerable<IEdmStructuralProperty> DeclaredStructuralProperties(this IEdmStructuredType type)
		{
			EdmUtil.CheckArgumentNull<IEdmStructuredType>(type, "type");
			return type.DeclaredProperties.OfType<IEdmStructuralProperty>();
		}

		public static IEnumerable<IEdmStructuralProperty> DeclaredStructuralProperties(this IEdmStructuredTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmStructuredTypeReference>(type, "type");
			return type.StructuredDefinition().DeclaredStructuralProperties();
		}

		public static IEdmEntityType DeclaringEntityType(this IEdmNavigationProperty property)
		{
			return (IEdmEntityType)property.DeclaringType;
		}

		private static void DerivedFrom(this IEdmModel model, IEdmStructuredType baseType, HashSetInternal<IEdmStructuredType> visited, List<IEdmStructuredType> derivedTypes)
		{
			if (visited.Add(baseType))
			{
				IEnumerable<IEdmStructuredType> edmStructuredTypes = model.FindDirectlyDerivedTypes(baseType);
				if (edmStructuredTypes != null && edmStructuredTypes.Any<IEdmStructuredType>())
				{
					foreach (IEdmStructuredType edmStructuredType in edmStructuredTypes)
					{
						derivedTypes.Add(edmStructuredType);
						model.DerivedFrom(edmStructuredType, visited, derivedTypes);
					}
				}
				foreach (IEdmModel edmModel in edmStructuredTypes)
				{
					edmStructuredTypes = edmModel.FindDirectlyDerivedTypes(baseType);
					if (edmStructuredTypes == null || !edmStructuredTypes.Any<IEdmStructuredType>())
					{
						continue;
					}
					IEnumerator<IEdmStructuredType> enumerator = edmStructuredTypes.GetEnumerator();
					using (enumerator)
					{
						while (enumerator.MoveNext())
						{
							IEdmStructuredType edmStructuredType1 = enumerator.Current;
							derivedTypes.Add(edmStructuredType1);
							model.DerivedFrom(edmStructuredType1, visited, derivedTypes);
						}
					}
				}
			}
		}

		public static IEnumerable<IEdmDirectValueAnnotation> DirectValueAnnotations(this IEdmModel model, IEdmElement element)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmElement>(element, "element");
			return model.DirectValueAnnotationsManager.GetDirectValueAnnotations(element);
		}

		public static IEdmTypeReference ElementType(this IEdmCollectionTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmCollectionTypeReference>(type, "type");
			return type.CollectionDefinition().ElementType;
		}

		public static IEnumerable<IEdmEntityContainer> EntityContainers(this IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			return model.SchemaElements.OfType<IEdmEntityContainer>();
		}

		public static IEdmEntityType EntityDefinition(this IEdmEntityTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityTypeReference>(type, "type");
			return (IEdmEntityType)type.Definition;
		}

		public static IEdmEntityReferenceType EntityReferenceDefinition(this IEdmEntityReferenceTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityReferenceTypeReference>(type, "type");
			return (IEdmEntityReferenceType)type.Definition;
		}

		public static IEnumerable<IEdmEntitySet> EntitySets(this IEdmEntityContainer container)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityContainer>(container, "container");
			return container.Elements.OfType<IEdmEntitySet>();
		}

		public static IEdmEntityType EntityType(this IEdmEntityReferenceTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityReferenceTypeReference>(type, "type");
			return type.EntityReferenceDefinition().EntityType;
		}

		public static IEdmEnumType EnumDefinition(this IEdmEnumTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmEnumTypeReference>(type, "type");
			return (IEdmEnumType)type.Definition;
		}

		private static T FindAcrossModels<T>(this IEdmModel model, string qualifiedName, Func<IEdmModel, string, T> finder, Func<T, T, T> ambiguousCreator)
		{
			T t;
			T t1 = finder(model, qualifiedName);
			foreach (IEdmModel referencedModel in model.ReferencedModels)
			{
				T t2 = finder(referencedModel, qualifiedName);
				if (t2 == null)
				{
					continue;
				}
				if (t1 == null)
				{
					t = t2;
				}
				else
				{
					t = ambiguousCreator(t1, t2);
				}
				t1 = t;
			}
			return t1;
		}

		public static IEnumerable<IEdmStructuredType> FindAllDerivedTypes(this IEdmModel model, IEdmStructuredType baseType)
		{
			List<IEdmStructuredType> edmStructuredTypes = new List<IEdmStructuredType>();
			if (baseType as IEdmSchemaElement != null)
			{
				model.DerivedFrom(baseType, new HashSetInternal<IEdmStructuredType>(), edmStructuredTypes);
			}
			return edmStructuredTypes;
		}

		public static IEdmEntityContainer FindEntityContainer(this IEdmModel model, string qualifiedName)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<string>(qualifiedName, "qualifiedName");
			return model.FindAcrossModels<IEdmEntityContainer>(qualifiedName, ExtensionMethods.findEntityContainer, new Func<IEdmEntityContainer, IEdmEntityContainer, IEdmEntityContainer>(RegistrationHelper.CreateAmbiguousEntityContainerBinding));
		}

		public static IEnumerable<IEdmFunction> FindFunctions(this IEdmModel model, string qualifiedName)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<string>(qualifiedName, "qualifiedName");
			return model.FindAcrossModels<IEnumerable<IEdmFunction>>(qualifiedName, ExtensionMethods.findFunctions, ExtensionMethods.mergeFunctions);
		}

		public static IEdmNavigationProperty FindNavigationProperty(this IEdmEntityTypeReference type, string name)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityTypeReference>(type, "type");
			EdmUtil.CheckArgumentNull<string>(name, "name");
			return type.EntityDefinition().FindProperty(name) as IEdmNavigationProperty;
		}

		public static IEdmProperty FindProperty(this IEdmStructuredTypeReference type, string name)
		{
			EdmUtil.CheckArgumentNull<IEdmStructuredTypeReference>(type, "type");
			EdmUtil.CheckArgumentNull<string>(name, "name");
			return type.StructuredDefinition().FindProperty(name);
		}

		public static IEdmPropertyConstructor FindProperty(this IEdmRecordExpression expression, string name)
		{
			IEdmPropertyConstructor edmPropertyConstructor;
			IEnumerator<IEdmPropertyConstructor> enumerator = expression.Properties.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					IEdmPropertyConstructor current = enumerator.Current;
					if (current.Name != name)
					{
						continue;
					}
					edmPropertyConstructor = current;
					return edmPropertyConstructor;
				}
				return null;
			}
			return edmPropertyConstructor;
		}

		public static IEdmPropertyValueBinding FindPropertyBinding(this IEdmTypeAnnotation annotation, IEdmProperty property)
		{
			IEdmPropertyValueBinding edmPropertyValueBinding;
			EdmUtil.CheckArgumentNull<IEdmTypeAnnotation>(annotation, "annotation");
			EdmUtil.CheckArgumentNull<IEdmProperty>(property, "property");
			IEnumerator<IEdmPropertyValueBinding> enumerator = annotation.PropertyValueBindings.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					IEdmPropertyValueBinding current = enumerator.Current;
					if (current.BoundProperty != property)
					{
						continue;
					}
					edmPropertyValueBinding = current;
					return edmPropertyValueBinding;
				}
				return null;
			}
			return edmPropertyValueBinding;
		}

		public static IEdmPropertyValueBinding FindPropertyBinding(this IEdmTypeAnnotation annotation, string propertyName)
		{
			IEdmPropertyValueBinding edmPropertyValueBinding;
			EdmUtil.CheckArgumentNull<IEdmTypeAnnotation>(annotation, "annotation");
			EdmUtil.CheckArgumentNull<string>(propertyName, "propertyName");
			IEnumerator<IEdmPropertyValueBinding> enumerator = annotation.PropertyValueBindings.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					IEdmPropertyValueBinding current = enumerator.Current;
					if (!current.BoundProperty.Name.EqualsOrdinal(propertyName))
					{
						continue;
					}
					edmPropertyValueBinding = current;
					return edmPropertyValueBinding;
				}
				return null;
			}
			return edmPropertyValueBinding;
		}

		public static IEdmSchemaType FindType(this IEdmModel model, string qualifiedName)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<string>(qualifiedName, "qualifiedName");
			return model.FindAcrossModels<IEdmSchemaType>(qualifiedName, ExtensionMethods.findType, new Func<IEdmSchemaType, IEdmSchemaType, IEdmSchemaType>(RegistrationHelper.CreateAmbiguousTypeBinding));
		}

		public static IEdmValueTerm FindValueTerm(this IEdmModel model, string qualifiedName)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<string>(qualifiedName, "qualifiedName");
			return model.FindAcrossModels<IEdmValueTerm>(qualifiedName, ExtensionMethods.findValueTerm, new Func<IEdmValueTerm, IEdmValueTerm, IEdmValueTerm>(RegistrationHelper.CreateAmbiguousValueTermBinding));
		}

		public static IEnumerable<IEdmVocabularyAnnotation> FindVocabularyAnnotations(this IEdmModel model, IEdmVocabularyAnnotatable element)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			IEnumerable<IEdmVocabularyAnnotation> edmVocabularyAnnotations = model.FindVocabularyAnnotationsIncludingInheritedAnnotations(element);
			foreach (IEdmModel referencedModel in model.ReferencedModels)
			{
				edmVocabularyAnnotations = edmVocabularyAnnotations.Concat<IEdmVocabularyAnnotation>(referencedModel.FindVocabularyAnnotationsIncludingInheritedAnnotations(element));
			}
			return edmVocabularyAnnotations;
		}

		public static IEnumerable<T> FindVocabularyAnnotations<T>(this IEdmModel model, IEdmVocabularyAnnotatable element, IEdmTerm term)
		where T : IEdmVocabularyAnnotation
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			EdmUtil.CheckArgumentNull<IEdmTerm>(term, "term");
			return model.FindVocabularyAnnotations<T>(element, term, null);
		}

		public static IEnumerable<T> FindVocabularyAnnotations<T>(this IEdmModel model, IEdmVocabularyAnnotatable element, IEdmTerm term, string qualifier)
		where T : IEdmVocabularyAnnotation
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			EdmUtil.CheckArgumentNull<IEdmTerm>(term, "term");
			List<T> ts = null;
			foreach (T t in model.FindVocabularyAnnotations(element).OfType<T>())
			{
				if (t.Term != term || qualifier != null && !(qualifier == t.Qualifier))
				{
					continue;
				}
				if (ts == null)
				{
					ts = new List<T>();
				}
				ts.Add(t);
			}
			List<T> ts1 = ts;
			IEnumerable<T> ts2 = ts1;
			if (ts1 == null)
			{
				ts2 = Enumerable.Empty<T>();
			}
			return ts2;
		}

		public static IEnumerable<T> FindVocabularyAnnotations<T>(this IEdmModel model, IEdmVocabularyAnnotatable element, string termName)
		where T : IEdmVocabularyAnnotation
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			EdmUtil.CheckArgumentNull<string>(termName, "termName");
			return model.FindVocabularyAnnotations<T>(element, termName, null);
		}

		public static IEnumerable<T> FindVocabularyAnnotations<T>(this IEdmModel model, IEdmVocabularyAnnotatable element, string termName, string qualifier)
		where T : IEdmVocabularyAnnotation
		{
			string str = null;
			string str1 = null;
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			EdmUtil.CheckArgumentNull<string>(termName, "termName");
			List<T> ts = null;
			if (EdmUtil.TryGetNamespaceNameFromQualifiedName(termName, out str1, out str))
			{
				foreach (T t in model.FindVocabularyAnnotations(element).OfType<T>())
				{
					IEdmTerm term = t.Term;
					if (!(term.Namespace == str1) || !(term.Name == str) || qualifier != null && !(qualifier == t.Qualifier))
					{
						continue;
					}
					if (ts == null)
					{
						ts = new List<T>();
					}
					ts.Add(t);
				}
			}
			List<T> ts1 = ts;
			IEnumerable<T> ts2 = ts1;
			if (ts1 == null)
			{
				ts2 = Enumerable.Empty<T>();
			}
			return ts2;
		}

		public static IEnumerable<IEdmVocabularyAnnotation> FindVocabularyAnnotationsIncludingInheritedAnnotations(this IEdmModel model, IEdmVocabularyAnnotatable element)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			IEnumerable<IEdmVocabularyAnnotation> edmVocabularyAnnotations = model.FindDeclaredVocabularyAnnotations(element);
			IEdmStructuredType i = element as IEdmStructuredType;
			if (i != null)
			{
				for (i = i.BaseType; i != null; i = i.BaseType)
				{
					IEdmVocabularyAnnotatable edmVocabularyAnnotatable = i as IEdmVocabularyAnnotatable;
					if (edmVocabularyAnnotatable != null)
					{
						edmVocabularyAnnotations = edmVocabularyAnnotations.Concat<IEdmVocabularyAnnotation>(model.FindDeclaredVocabularyAnnotations(edmVocabularyAnnotatable));
					}
				}
			}
			return edmVocabularyAnnotations;
		}

		public static string FullName(this IEdmSchemaElement element)
		{
			EdmUtil.CheckArgumentNull<IEdmSchemaElement>(element, "element");
			string @namespace = element.Namespace;
			string empty = @namespace;
			if (@namespace == null)
			{
				empty = string.Empty;
			}
			string str = ".";
			string name = element.Name;
			string empty1 = name;
			if (name == null)
			{
				empty1 = string.Empty;
			}
			return string.Concat(empty, str, empty1);
		}

		public static string FullName(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmSchemaElement definition = type.Definition as IEdmSchemaElement;
			if (definition != null)
			{
				return definition.FullName();
			}
			else
			{
				return null;
			}
		}

		public static IEnumerable<IEdmFunctionImport> FunctionImports(this IEdmEntityContainer container)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityContainer>(container, "container");
			return container.Elements.OfType<IEdmFunctionImport>();
		}

		public static object GetAnnotationValue(this IEdmModel model, IEdmElement element, string namespaceName, string localName)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmElement>(element, "element");
			return model.DirectValueAnnotationsManager.GetAnnotationValue(element, namespaceName, localName);
		}

		public static T GetAnnotationValue<T>(this IEdmModel model, IEdmElement element, string namespaceName, string localName)
		where T : class
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmElement>(element, "element");
			return ExtensionMethods.AnnotationValue<T>(model.GetAnnotationValue(element, namespaceName, localName));
		}

		public static T GetAnnotationValue<T>(this IEdmModel model, IEdmElement element)
		where T : class
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmElement>(element, "element");
			return model.GetAnnotationValue<T>(element, "http://schemas.microsoft.com/ado/2011/04/edm/internal", ExtensionMethods.TypeName<T>.LocalName);
		}

		public static object[] GetAnnotationValues(this IEdmModel model, IEnumerable<IEdmDirectValueAnnotationBinding> annotations)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEnumerable<IEdmDirectValueAnnotationBinding>>(annotations, "annotations");
			return model.DirectValueAnnotationsManager.GetAnnotationValues(annotations);
		}

		public static IEdmDocumentation GetDocumentation(this IEdmModel model, IEdmElement element)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmElement>(element, "element");
			return (IEdmDocumentation)model.GetAnnotationValue(element, "http://schemas.microsoft.com/ado/2011/04/edm/documentation", "Documentation");
		}

		public static Version GetEdmVersion(this IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			return model.GetAnnotationValue<Version>(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "EdmVersion");
		}

		internal static IEdmEntityType GetPathSegmentEntityType(IEdmTypeReference segmentType)
		{
			IEdmTypeReference edmTypeReference;
			if (segmentType.IsCollection())
			{
				edmTypeReference = segmentType.AsCollection().ElementType();
			}
			else
			{
				edmTypeReference = segmentType;
			}
			return edmTypeReference.AsEntity().EntityDefinition();
		}

		public static IEdmValue GetPropertyValue(this IEdmModel model, IEdmStructuredValue context, IEdmProperty property, EdmExpressionEvaluator expressionEvaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmStructuredValue>(context, "context");
			EdmUtil.CheckArgumentNull<IEdmProperty>(property, "property");
			EdmUtil.CheckArgumentNull<EdmExpressionEvaluator>(expressionEvaluator, "expressionEvaluator");
			return model.GetPropertyValue<IEdmValue>(context, context.Type.AsEntity().EntityDefinition(), property, null, new Func<IEdmExpression, IEdmStructuredValue, IEdmValue>(expressionEvaluator.Evaluate));
		}

		public static IEdmValue GetPropertyValue(this IEdmModel model, IEdmStructuredValue context, IEdmProperty property, string qualifier, EdmExpressionEvaluator expressionEvaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmStructuredValue>(context, "context");
			EdmUtil.CheckArgumentNull<IEdmProperty>(property, "property");
			EdmUtil.CheckArgumentNull<EdmExpressionEvaluator>(expressionEvaluator, "expressionEvaluator");
			return model.GetPropertyValue<IEdmValue>(context, context.Type.AsEntity().EntityDefinition(), property, qualifier, new Func<IEdmExpression, IEdmStructuredValue, IEdmValue>(expressionEvaluator.Evaluate));
		}

		public static T GetPropertyValue<T>(this IEdmModel model, IEdmStructuredValue context, IEdmProperty property, EdmToClrEvaluator evaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmStructuredValue>(context, "context");
			EdmUtil.CheckArgumentNull<IEdmProperty>(property, "property");
			EdmUtil.CheckArgumentNull<EdmToClrEvaluator>(evaluator, "evaluator");
			return model.GetPropertyValue<T>(context, context.Type.AsEntity().EntityDefinition(), property, null, new Func<IEdmExpression, IEdmStructuredValue, T>(evaluator.EvaluateToClrValue<T>));
		}

		public static T GetPropertyValue<T>(this IEdmModel model, IEdmStructuredValue context, IEdmProperty property, string qualifier, EdmToClrEvaluator evaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmStructuredValue>(context, "context");
			EdmUtil.CheckArgumentNull<IEdmProperty>(property, "property");
			EdmUtil.CheckArgumentNull<EdmToClrEvaluator>(evaluator, "evaluator");
			return model.GetPropertyValue<T>(context, context.Type.AsEntity().EntityDefinition(), property, qualifier, new Func<IEdmExpression, IEdmStructuredValue, T>(evaluator.EvaluateToClrValue<T>));
		}

		private static T GetPropertyValue<T>(this IEdmModel model, IEdmStructuredValue context, IEdmEntityType contextType, IEdmProperty property, string qualifier, Func<IEdmExpression, IEdmStructuredValue, T> evaluator)
		{
			IEdmEntityType declaringType = (IEdmEntityType)property.DeclaringType;
			IEnumerable<IEdmTypeAnnotation> edmTypeAnnotations = model.FindVocabularyAnnotations<IEdmTypeAnnotation>(contextType, declaringType, qualifier);
			if (edmTypeAnnotations.Count<IEdmTypeAnnotation>() == 1)
			{
				IEdmPropertyValueBinding edmPropertyValueBinding = edmTypeAnnotations.Single<IEdmTypeAnnotation>().FindPropertyBinding(property);
				if (edmPropertyValueBinding != null)
				{
					return evaluator(edmPropertyValueBinding.Value, context);
				}
				else
				{
					T t = default(T);
					return t;
				}
			}
			else
			{
				throw new InvalidOperationException(Strings.Edm_Evaluator_NoTermTypeAnnotationOnType(contextType.ToTraceString(), declaringType.ToTraceString()));
			}
		}

		public static IEdmValue GetTermValue(this IEdmModel model, IEdmStructuredValue context, string termName, EdmExpressionEvaluator expressionEvaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmStructuredValue>(context, "context");
			EdmUtil.CheckArgumentNull<string>(termName, "termName");
			EdmUtil.CheckArgumentNull<EdmExpressionEvaluator>(expressionEvaluator, "expressionEvaluator");
			return model.GetTermValue<IEdmValue>(context, context.Type.AsEntity().EntityDefinition(), termName, null, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, IEdmValue>(expressionEvaluator.Evaluate));
		}

		public static IEdmValue GetTermValue(this IEdmModel model, IEdmStructuredValue context, string termName, string qualifier, EdmExpressionEvaluator expressionEvaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmStructuredValue>(context, "context");
			EdmUtil.CheckArgumentNull<string>(termName, "termName");
			EdmUtil.CheckArgumentNull<EdmExpressionEvaluator>(expressionEvaluator, "expressionEvaluator");
			return model.GetTermValue<IEdmValue>(context, context.Type.AsEntity().EntityDefinition(), termName, qualifier, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, IEdmValue>(expressionEvaluator.Evaluate));
		}

		public static IEdmValue GetTermValue(this IEdmModel model, IEdmStructuredValue context, IEdmValueTerm term, EdmExpressionEvaluator expressionEvaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmStructuredValue>(context, "context");
			EdmUtil.CheckArgumentNull<IEdmValueTerm>(term, "term");
			EdmUtil.CheckArgumentNull<EdmExpressionEvaluator>(expressionEvaluator, "expressionEvaluator");
			return model.GetTermValue<IEdmValue>(context, context.Type.AsEntity().EntityDefinition(), term, null, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, IEdmValue>(expressionEvaluator.Evaluate));
		}

		public static IEdmValue GetTermValue(this IEdmModel model, IEdmStructuredValue context, IEdmValueTerm term, string qualifier, EdmExpressionEvaluator expressionEvaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmStructuredValue>(context, "context");
			EdmUtil.CheckArgumentNull<IEdmValueTerm>(term, "term");
			EdmUtil.CheckArgumentNull<EdmExpressionEvaluator>(expressionEvaluator, "expressionEvaluator");
			return model.GetTermValue<IEdmValue>(context, context.Type.AsEntity().EntityDefinition(), term, qualifier, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, IEdmValue>(expressionEvaluator.Evaluate));
		}

		public static T GetTermValue<T>(this IEdmModel model, IEdmStructuredValue context, string termName, EdmToClrEvaluator evaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmStructuredValue>(context, "context");
			EdmUtil.CheckArgumentNull<string>(termName, "termName");
			EdmUtil.CheckArgumentNull<EdmToClrEvaluator>(evaluator, "evaluator");
			return model.GetTermValue<T>(context, context.Type.AsEntity().EntityDefinition(), termName, null, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, T>(evaluator.EvaluateToClrValue<T>));
		}

		public static T GetTermValue<T>(this IEdmModel model, IEdmStructuredValue context, string termName, string qualifier, EdmToClrEvaluator evaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmStructuredValue>(context, "context");
			EdmUtil.CheckArgumentNull<string>(termName, "termName");
			EdmUtil.CheckArgumentNull<EdmToClrEvaluator>(evaluator, "evaluator");
			return model.GetTermValue<T>(context, context.Type.AsEntity().EntityDefinition(), termName, qualifier, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, T>(evaluator.EvaluateToClrValue<T>));
		}

		public static T GetTermValue<T>(this IEdmModel model, IEdmStructuredValue context, IEdmValueTerm term, EdmToClrEvaluator evaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmStructuredValue>(context, "context");
			EdmUtil.CheckArgumentNull<IEdmValueTerm>(term, "term");
			EdmUtil.CheckArgumentNull<EdmToClrEvaluator>(evaluator, "evaluator");
			return model.GetTermValue<T>(context, context.Type.AsEntity().EntityDefinition(), term, null, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, T>(evaluator.EvaluateToClrValue<T>));
		}

		public static T GetTermValue<T>(this IEdmModel model, IEdmStructuredValue context, IEdmValueTerm term, string qualifier, EdmToClrEvaluator evaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmStructuredValue>(context, "context");
			EdmUtil.CheckArgumentNull<IEdmValueTerm>(term, "term");
			EdmUtil.CheckArgumentNull<EdmToClrEvaluator>(evaluator, "evaluator");
			return model.GetTermValue<T>(context, context.Type.AsEntity().EntityDefinition(), term, qualifier, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, T>(evaluator.EvaluateToClrValue<T>));
		}

		public static IEdmValue GetTermValue(this IEdmModel model, IEdmVocabularyAnnotatable element, string termName, EdmExpressionEvaluator expressionEvaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			EdmUtil.CheckArgumentNull<string>(termName, "termName");
			EdmUtil.CheckArgumentNull<EdmExpressionEvaluator>(expressionEvaluator, "evaluator");
			return model.GetTermValue<IEdmValue>(element, termName, null, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, IEdmValue>(expressionEvaluator.Evaluate));
		}

		public static IEdmValue GetTermValue(this IEdmModel model, IEdmVocabularyAnnotatable element, string termName, string qualifier, EdmExpressionEvaluator expressionEvaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			EdmUtil.CheckArgumentNull<string>(termName, "termName");
			EdmUtil.CheckArgumentNull<EdmExpressionEvaluator>(expressionEvaluator, "evaluator");
			return model.GetTermValue<IEdmValue>(element, termName, qualifier, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, IEdmValue>(expressionEvaluator.Evaluate));
		}

		public static IEdmValue GetTermValue(this IEdmModel model, IEdmVocabularyAnnotatable element, IEdmValueTerm term, EdmExpressionEvaluator expressionEvaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			EdmUtil.CheckArgumentNull<IEdmValueTerm>(term, "term");
			EdmUtil.CheckArgumentNull<EdmExpressionEvaluator>(expressionEvaluator, "evaluator");
			return model.GetTermValue<IEdmValue>(element, term, null, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, IEdmValue>(expressionEvaluator.Evaluate));
		}

		public static IEdmValue GetTermValue(this IEdmModel model, IEdmVocabularyAnnotatable element, IEdmValueTerm term, string qualifier, EdmExpressionEvaluator expressionEvaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			EdmUtil.CheckArgumentNull<IEdmValueTerm>(term, "term");
			EdmUtil.CheckArgumentNull<EdmExpressionEvaluator>(expressionEvaluator, "evaluator");
			return model.GetTermValue<IEdmValue>(element, term, qualifier, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, IEdmValue>(expressionEvaluator.Evaluate));
		}

		public static T GetTermValue<T>(this IEdmModel model, IEdmVocabularyAnnotatable element, string termName, EdmToClrEvaluator evaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			EdmUtil.CheckArgumentNull<string>(termName, "termName");
			EdmUtil.CheckArgumentNull<EdmToClrEvaluator>(evaluator, "evaluator");
			return model.GetTermValue<T>(element, termName, null, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, T>(evaluator.EvaluateToClrValue<T>));
		}

		public static T GetTermValue<T>(this IEdmModel model, IEdmVocabularyAnnotatable element, string termName, string qualifier, EdmToClrEvaluator evaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			EdmUtil.CheckArgumentNull<string>(termName, "termName");
			EdmUtil.CheckArgumentNull<EdmToClrEvaluator>(evaluator, "evaluator");
			return model.GetTermValue<T>(element, termName, qualifier, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, T>(evaluator.EvaluateToClrValue<T>));
		}

		public static T GetTermValue<T>(this IEdmModel model, IEdmVocabularyAnnotatable element, IEdmValueTerm term, EdmToClrEvaluator evaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			EdmUtil.CheckArgumentNull<IEdmValueTerm>(term, "term");
			EdmUtil.CheckArgumentNull<EdmToClrEvaluator>(evaluator, "evaluator");
			return model.GetTermValue<T>(element, term, null, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, T>(evaluator.EvaluateToClrValue<T>));
		}

		public static T GetTermValue<T>(this IEdmModel model, IEdmVocabularyAnnotatable element, IEdmValueTerm term, string qualifier, EdmToClrEvaluator evaluator)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			EdmUtil.CheckArgumentNull<IEdmValueTerm>(term, "term");
			EdmUtil.CheckArgumentNull<EdmToClrEvaluator>(evaluator, "evaluator");
			return model.GetTermValue<T>(element, term, qualifier, new Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, T>(evaluator.EvaluateToClrValue<T>));
		}

		private static T GetTermValue<T>(this IEdmModel model, IEdmStructuredValue context, IEdmEntityType contextType, IEdmValueTerm term, string qualifier, Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, T> evaluator)
		{
			IEnumerable<IEdmValueAnnotation> edmValueAnnotations = model.FindVocabularyAnnotations<IEdmValueAnnotation>(contextType, term, qualifier);
			if (edmValueAnnotations.Count<IEdmValueAnnotation>() == 1)
			{
				return evaluator(edmValueAnnotations.Single<IEdmValueAnnotation>().Value, context, term.Type);
			}
			else
			{
				throw new InvalidOperationException(Strings.Edm_Evaluator_NoValueAnnotationOnType(contextType.ToTraceString(), term.ToTraceString()));
			}
		}

		private static T GetTermValue<T>(this IEdmModel model, IEdmStructuredValue context, IEdmEntityType contextType, string termName, string qualifier, Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, T> evaluator)
		{
			IEnumerable<IEdmValueAnnotation> edmValueAnnotations = model.FindVocabularyAnnotations<IEdmValueAnnotation>(contextType, termName, qualifier);
			if (edmValueAnnotations.Count<IEdmValueAnnotation>() == 1)
			{
				IEdmValueAnnotation edmValueAnnotation = edmValueAnnotations.Single<IEdmValueAnnotation>();
				return evaluator(edmValueAnnotation.Value, context, edmValueAnnotation.ValueTerm().Type);
			}
			else
			{
				throw new InvalidOperationException(Strings.Edm_Evaluator_NoValueAnnotationOnType(contextType.ToTraceString(), termName));
			}
		}

		private static T GetTermValue<T>(this IEdmModel model, IEdmVocabularyAnnotatable element, IEdmValueTerm term, string qualifier, Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, T> evaluator)
		{
			IEnumerable<IEdmValueAnnotation> edmValueAnnotations = model.FindVocabularyAnnotations<IEdmValueAnnotation>(element, term, qualifier);
			if (edmValueAnnotations.Count<IEdmValueAnnotation>() == 1)
			{
				return evaluator(edmValueAnnotations.Single<IEdmValueAnnotation>().Value, null, term.Type);
			}
			else
			{
				throw new InvalidOperationException(Strings.Edm_Evaluator_NoValueAnnotationOnElement(term.ToTraceString()));
			}
		}

		private static T GetTermValue<T>(this IEdmModel model, IEdmVocabularyAnnotatable element, string termName, string qualifier, Func<IEdmExpression, IEdmStructuredValue, IEdmTypeReference, T> evaluator)
		{
			IEnumerable<IEdmValueAnnotation> edmValueAnnotations = model.FindVocabularyAnnotations<IEdmValueAnnotation>(element, termName, qualifier);
			if (edmValueAnnotations.Count<IEdmValueAnnotation>() == 1)
			{
				IEdmValueAnnotation edmValueAnnotation = edmValueAnnotations.Single<IEdmValueAnnotation>();
				return evaluator(edmValueAnnotation.Value, null, edmValueAnnotation.ValueTerm().Type);
			}
			else
			{
				throw new InvalidOperationException(Strings.Edm_Evaluator_NoValueAnnotationOnElement(termName));
			}
		}

		public static bool IsAbstract(this IEdmStructuredTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmStructuredTypeReference>(type, "type");
			return type.StructuredDefinition().IsAbstract;
		}

		public static bool IsOpen(this IEdmStructuredTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmStructuredTypeReference>(type, "type");
			return type.StructuredDefinition().IsOpen;
		}

		public static IEnumerable<IEdmStructuralProperty> Key(this IEdmEntityType type)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityType>(type, "type");
			IEdmEntityType edmEntityType = type;
			while (edmEntityType != null)
			{
				if (edmEntityType.DeclaredKey == null)
				{
					edmEntityType = edmEntityType.BaseEntityType();
				}
				else
				{
					return edmEntityType.DeclaredKey;
				}
			}
			return Enumerable.Empty<IEdmStructuralProperty>();
		}

		public static IEnumerable<IEdmStructuralProperty> Key(this IEdmEntityTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityTypeReference>(type, "type");
			return type.EntityDefinition().Key();
		}

		public static EdmLocation Location(this IEdmElement item)
		{
			EdmUtil.CheckArgumentNull<IEdmElement>(item, "item");
			IEdmLocatable edmLocatable = item as IEdmLocatable;
			if (edmLocatable == null || edmLocatable.Location == null)
			{
				return new ObjectLocation(item);
			}
			else
			{
				return edmLocatable.Location;
			}
		}

		public static EdmMultiplicity Multiplicity(this IEdmNavigationProperty property)
		{
			EdmUtil.CheckArgumentNull<IEdmNavigationProperty>(property, "property");
			IEdmNavigationProperty partner = property.Partner;
			if (partner == null)
			{
				return EdmMultiplicity.One;
			}
			else
			{
				IEdmTypeReference type = partner.Type;
				if (!type.IsCollection())
				{
					if (type.IsNullable)
					{
						return EdmMultiplicity.ZeroOrOne;
					}
					else
					{
						return EdmMultiplicity.One;
					}
				}
				else
				{
					return EdmMultiplicity.Many;
				}
			}
		}

		public static IEnumerable<IEdmNavigationProperty> NavigationProperties(this IEdmEntityType type)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityType>(type, "type");
			return type.Properties().OfType<IEdmNavigationProperty>();
		}

		public static IEnumerable<IEdmNavigationProperty> NavigationProperties(this IEdmEntityTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityTypeReference>(type, "type");
			return type.EntityDefinition().NavigationProperties();
		}

		public static IEdmPrimitiveType PrimitiveDefinition(this IEdmPrimitiveTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmPrimitiveTypeReference>(type, "type");
			return (IEdmPrimitiveType)type.Definition;
		}

		public static EdmPrimitiveTypeKind PrimitiveKind(this IEdmPrimitiveTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmPrimitiveTypeReference>(type, "type");
			IEdmPrimitiveType edmPrimitiveType = type.PrimitiveDefinition();
			if (edmPrimitiveType != null)
			{
				return edmPrimitiveType.PrimitiveKind;
			}
			else
			{
				return EdmPrimitiveTypeKind.None;
			}
		}

		public static IEnumerable<IEdmProperty> Properties(this IEdmStructuredType type)
		{
			EdmUtil.CheckArgumentNull<IEdmStructuredType>(type, "type");
			if (type.BaseType != null)
			{
				foreach (IEdmProperty edmProperty in type.BaseType.Properties())
				{
					yield return edmProperty;
				}
			}
			if (type.DeclaredProperties != null)
			{
				foreach (IEdmProperty edmProperty1 in type.DeclaredProperties)
				{
					yield return edmProperty1;
				}
			}
		}

		public static IEdmRowType RowDefinition(this IEdmRowTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmRowTypeReference>(type, "type");
			return (IEdmRowType)type.Definition;
		}

		public static IEnumerable<IEdmSchemaElement> SchemaElementsAcrossModels(this IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			IEnumerable<IEdmSchemaElement> edmSchemaElements = new IEdmSchemaElement[0];
			foreach (IEdmModel referencedModel in model.ReferencedModels)
			{
				edmSchemaElements = edmSchemaElements.Concat<IEdmSchemaElement>(referencedModel.SchemaElements);
			}
			edmSchemaElements = edmSchemaElements.Concat<IEdmSchemaElement>(model.SchemaElements);
			return edmSchemaElements;
		}

		public static void SetAnnotationValue(this IEdmModel model, IEdmElement element, string namespaceName, string localName, object value)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmElement>(element, "element");
			model.DirectValueAnnotationsManager.SetAnnotationValue(element, namespaceName, localName, value);
		}

		public static void SetAnnotationValue<T>(this IEdmModel model, IEdmElement element, T value)
		where T : class
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmElement>(element, "element");
			model.SetAnnotationValue(element, "http://schemas.microsoft.com/ado/2011/04/edm/internal", ExtensionMethods.TypeName<T>.LocalName, value);
		}

		public static void SetAnnotationValues(this IEdmModel model, IEnumerable<IEdmDirectValueAnnotationBinding> annotations)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEnumerable<IEdmDirectValueAnnotationBinding>>(annotations, "annotations");
			model.DirectValueAnnotationsManager.SetAnnotationValues(annotations);
		}

		public static void SetDocumentation(this IEdmModel model, IEdmElement element, IEdmDocumentation documentation)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			EdmUtil.CheckArgumentNull<IEdmElement>(element, "element");
			model.SetAnnotationValue(element, "http://schemas.microsoft.com/ado/2011/04/edm/documentation", "Documentation", documentation);
		}

		public static void SetEdmVersion(this IEdmModel model, Version version)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			model.SetAnnotationValue(model, "http://schemas.microsoft.com/ado/2011/04/edm/internal", "EdmVersion", version);
		}

		public static IEnumerable<IEdmStructuralProperty> StructuralProperties(this IEdmStructuredType type)
		{
			EdmUtil.CheckArgumentNull<IEdmStructuredType>(type, "type");
			return type.Properties().OfType<IEdmStructuralProperty>();
		}

		public static IEnumerable<IEdmStructuralProperty> StructuralProperties(this IEdmStructuredTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmStructuredTypeReference>(type, "type");
			return type.StructuredDefinition().StructuralProperties();
		}

		public static IEdmStructuredType StructuredDefinition(this IEdmStructuredTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmStructuredTypeReference>(type, "type");
			return (IEdmStructuredType)type.Definition;
		}

		public static IEdmEntityType ToEntityType(this IEdmNavigationProperty property)
		{
			IEdmType definition = property.Type.Definition;
			if (definition.TypeKind == EdmTypeKind.Collection)
			{
				definition = ((IEdmCollectionType)definition).ElementType.Definition;
			}
			if (definition.TypeKind == EdmTypeKind.EntityReference)
			{
				definition = ((IEdmEntityReferenceType)definition).EntityType;
			}
			return definition as IEdmEntityType;
		}

		public static bool TryGetRelativeEntitySetPath(this IEdmFunctionImport functionImport, IEdmModel model, out IEdmFunctionParameter parameter, out IEnumerable<IEdmNavigationProperty> path)
		{
			parameter = null;
			path = null;
			IEdmPathExpression entitySet = functionImport.EntitySet as IEdmPathExpression;
			if (entitySet != null)
			{
				List<string> list = entitySet.Path.ToList<string>();
				if (list.Count != 0)
				{
					parameter = functionImport.FindParameter(list[0]);
					if (parameter != null)
					{
						if (list.Count != 1)
						{
							IEdmEntityType pathSegmentEntityType = ExtensionMethods.GetPathSegmentEntityType(parameter.Type);
							List<IEdmNavigationProperty> edmNavigationProperties = new List<IEdmNavigationProperty>();
							for (int i = 1; i < list.Count; i++)
							{
								string item = list[i];
								if (!EdmUtil.IsQualifiedName(item))
								{
									IEdmNavigationProperty edmNavigationProperty = pathSegmentEntityType.FindProperty(item) as IEdmNavigationProperty;
									if (edmNavigationProperty != null)
									{
										edmNavigationProperties.Add(edmNavigationProperty);
										pathSegmentEntityType = ExtensionMethods.GetPathSegmentEntityType(edmNavigationProperty.Type);
									}
									else
									{
										return false;
									}
								}
								else
								{
									if (i != list.Count - 1)
									{
										IEdmEntityType edmEntityType = model.FindDeclaredType(item) as IEdmEntityType;
										if (edmEntityType == null || !edmEntityType.IsOrInheritsFrom(pathSegmentEntityType))
										{
											return false;
										}
										else
										{
											pathSegmentEntityType = edmEntityType;
										}
									}
									else
									{
										return false;
									}
								}
							}
							path = edmNavigationProperties;
							return true;
						}
						else
						{
							path = Enumerable.Empty<IEdmNavigationProperty>();
							return true;
						}
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public static bool TryGetStaticEntitySet(this IEdmFunctionImport functionImport, out IEdmEntitySet entitySet)
		{
			entitySet = null;
			IEdmEntitySet referencedEntitySet;
			IEdmEntitySetReferenceExpression edmEntitySetReferenceExpression = functionImport.EntitySet as IEdmEntitySetReferenceExpression;
			IEdmEntitySet edmEntitySetPointer = entitySet;
			if (edmEntitySetReferenceExpression != null)
			{
				referencedEntitySet = edmEntitySetReferenceExpression.ReferencedEntitySet;
			}
			else
			{
				referencedEntitySet = null;
			}
			(edmEntitySetPointer) = referencedEntitySet;
			return entitySet != null;
		}

		public static EdmTypeKind TypeKind(this IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			IEdmType definition = type.Definition;
			if (definition != null)
			{
				return definition.TypeKind;
			}
			else
			{
				return EdmTypeKind.None;
			}
		}

		public static IEdmValueTerm ValueTerm(this IEdmValueAnnotation annotation)
		{
			EdmUtil.CheckArgumentNull<IEdmValueAnnotation>(annotation, "annotation");
			return (IEdmValueTerm)annotation.Term;
		}

		public static IEnumerable<IEdmVocabularyAnnotation> VocabularyAnnotations(this IEdmVocabularyAnnotatable element, IEdmModel model)
		{
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(element, "element");
			EdmUtil.CheckArgumentNull<IEdmModel>(model, "model");
			return model.FindVocabularyAnnotations(element);
		}

		internal static class TypeName<T>
		{
			public readonly static string LocalName;

			static TypeName()
			{
				ExtensionMethods.TypeName<T>.LocalName = typeof(T).ToString().Replace("_", "_____").Replace('.', '\u005F').Replace("[", "").Replace("]", "").Replace(",", "__").Replace("`", "___").Replace("+", "____");
			}
		}
	}
}