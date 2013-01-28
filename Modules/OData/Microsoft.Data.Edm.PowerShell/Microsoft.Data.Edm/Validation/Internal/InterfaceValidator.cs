using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Validation.Internal
{
	internal class InterfaceValidator
	{
		private readonly static Dictionary<Type, InterfaceValidator.VisitorBase> InterfaceVisitors;

		private readonly static Memoizer<Type, IEnumerable<InterfaceValidator.VisitorBase>> ConcreteTypeInterfaceVisitors;

		private readonly HashSetInternal<object> visited;

		private readonly HashSetInternal<object> visitedBad;

		private readonly HashSetInternal<object> danglingReferences;

		private readonly HashSetInternal<object> skipVisitation;

		private readonly bool validateDirectValueAnnotations;

		private readonly IEdmModel model;

		static InterfaceValidator()
		{
			InterfaceValidator.InterfaceVisitors = InterfaceValidator.CreateInterfaceVisitorsMap();
			InterfaceValidator.ConcreteTypeInterfaceVisitors = new Memoizer<Type, IEnumerable<InterfaceValidator.VisitorBase>>(new Func<Type, IEnumerable<InterfaceValidator.VisitorBase>>(InterfaceValidator.ComputeInterfaceVisitorsForObject), null);
		}

		private InterfaceValidator(HashSetInternal<object> skipVisitation, IEdmModel model, bool validateDirectValueAnnotations)
		{
			this.visited = new HashSetInternal<object>();
			this.visitedBad = new HashSetInternal<object>();
			this.danglingReferences = new HashSetInternal<object>();
			this.skipVisitation = skipVisitation;
			this.model = model;
			this.validateDirectValueAnnotations = validateDirectValueAnnotations;
		}

		private static EdmError CheckForInterfaceKindValueMismatchError<T, K, I>(T item, K kind, string propertyName)
		{
			if (!(item is I))
			{
				return new EdmError(InterfaceValidator.GetLocation(item), EdmErrorCode.InterfaceCriticalKindValueMismatch, Strings.EdmModel_Validator_Syntactic_InterfaceKindValueMismatch(kind, typeof(T).Name, propertyName, typeof(I).Name));
			}
			else
			{
				return null;
			}
		}

		private static void CollectErrors(EdmError newError, ref List<EdmError> errors)
		{
			if (newError != null)
			{
				if (errors == null)
				{
					errors = new List<EdmError>();
				}
				errors.Add(newError);
			}
		}

		private void CollectReference(object reference)
		{
			if (reference as IEdmValidCoreModelElement == null && !this.visited.Contains(reference) && (this.skipVisitation == null || !this.skipVisitation.Contains(reference)))
			{
				this.danglingReferences.Add(reference);
			}
		}

		private static IEnumerable<InterfaceValidator.VisitorBase> ComputeInterfaceVisitorsForObject(Type objectType)
		{
			InterfaceValidator.VisitorBase visitorBase = null;
			List<InterfaceValidator.VisitorBase> visitorBases = new List<InterfaceValidator.VisitorBase>();
			Type[] interfaces = objectType.GetInterfaces();
			for (int i = 0; i < (int)interfaces.Length; i++)
			{
				Type type = interfaces[i];
				if (InterfaceValidator.InterfaceVisitors.TryGetValue(type, out visitorBase))
				{
					visitorBases.Add(visitorBase);
				}
			}
			return visitorBases;
		}

		private static EdmError CreateEnumPropertyOutOfRangeError<T, E>(T item, E enumValue, string propertyName)
		{
			return new EdmError(InterfaceValidator.GetLocation(item), EdmErrorCode.InterfaceCriticalEnumPropertyValueOutOfRange, Strings.EdmModel_Validator_Syntactic_EnumPropertyValueOutOfRange(typeof(T).Name, propertyName, typeof(E).Name, enumValue));
		}

		private static EdmError CreateInterfaceKindValueUnexpectedError<T, K>(T item, K kind, string propertyName)
		{
			return new EdmError(InterfaceValidator.GetLocation(item), EdmErrorCode.InterfaceCriticalKindValueUnexpected, Strings.EdmModel_Validator_Syntactic_InterfaceKindValueUnexpected(kind, typeof(T).Name, propertyName));
		}

		private static Dictionary<Type, InterfaceValidator.VisitorBase> CreateInterfaceVisitorsMap()
		{
			Dictionary<Type, InterfaceValidator.VisitorBase> types = new Dictionary<Type, InterfaceValidator.VisitorBase>();
			foreach (Type nonPublicNestedType in typeof(InterfaceValidator).GetNonPublicNestedTypes())
			{
				if (!nonPublicNestedType.IsClass())
				{
					continue;
				}
				Type baseType = nonPublicNestedType.GetBaseType();
				if (!baseType.IsGenericType() || !(baseType.GetBaseType() == typeof(InterfaceValidator.VisitorBase)))
				{
					continue;
				}
				types.Add(baseType.GetGenericArguments()[0], (InterfaceValidator.VisitorBase)Activator.CreateInstance(nonPublicNestedType));
			}
			return types;
		}

		private static EdmError CreatePrimitiveTypeRefInterfaceTypeKindValueMismatchError<T>(T item)
		where T : IEdmPrimitiveTypeReference
		{
			return new EdmError(InterfaceValidator.GetLocation(item), EdmErrorCode.InterfaceCriticalKindValueMismatch, Strings.EdmModel_Validator_Syntactic_TypeRefInterfaceTypeKindValueMismatch(typeof(T).Name, ((IEdmPrimitiveType)item.Definition).PrimitiveKind));
		}

		private static EdmError CreatePropertyMustNotBeNullError<T>(T item, string propertyName)
		{
			return new EdmError(InterfaceValidator.GetLocation(item), EdmErrorCode.InterfaceCriticalPropertyValueMustNotBeNull, Strings.EdmModel_Validator_Syntactic_PropertyMustNotBeNull(typeof(T).Name, propertyName));
		}

		private static EdmError CreateTypeRefInterfaceTypeKindValueMismatchError<T>(T item)
		where T : IEdmTypeReference
		{
			return new EdmError(InterfaceValidator.GetLocation(item), EdmErrorCode.InterfaceCriticalKindValueMismatch, Strings.EdmModel_Validator_Syntactic_TypeRefInterfaceTypeKindValueMismatch(typeof(T).Name, item.Definition.TypeKind));
		}

		private static EdmLocation GetLocation(object item)
		{
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

		private static IEnumerable<ValidationRule> GetSemanticInterfaceVisitorsForObject(Type objectType, ValidationRuleSet ruleSet, Dictionary<Type, List<ValidationRule>> concreteTypeSemanticInterfaceVisitors)
		{
			List<ValidationRule> validationRules = null;
			if (!concreteTypeSemanticInterfaceVisitors.TryGetValue(objectType, out validationRules))
			{
				validationRules = new List<ValidationRule>();
				Type[] interfaces = objectType.GetInterfaces();
				for (int i = 0; i < (int)interfaces.Length; i++)
				{
					Type type = interfaces[i];
					validationRules.AddRange(ruleSet.GetRules(type));
				}
				concreteTypeSemanticInterfaceVisitors.Add(objectType, validationRules);
			}
			return validationRules;
		}

		public static IEnumerable<EdmError> GetStructuralErrors(IEdmElement item)
		{
			IEdmModel edmModel = item as IEdmModel;
			InterfaceValidator interfaceValidator = new InterfaceValidator(null, edmModel, edmModel != null);
			return interfaceValidator.ValidateStructure(item);
		}

		private static bool IsCheckableBad(object element)
		{
			IEdmCheckable edmCheckable = element as IEdmCheckable;
			if (edmCheckable == null || edmCheckable.Errors == null)
			{
				return false;
			}
			else
			{
				return edmCheckable.Errors.Count<EdmError>() > 0;
			}
		}

		private static void ProcessEnumerable<T, E>(T item, IEnumerable<E> enumerable, string propertyName, IList targetList, ref List<EdmError> errors)
		{
			if (enumerable != null)
			{
				foreach (E e in enumerable)
				{
					if (e == null)
					{
						InterfaceValidator.CollectErrors(new EdmError(InterfaceValidator.GetLocation(item), EdmErrorCode.InterfaceCriticalEnumerableMustNotHaveNullElements, Strings.EdmModel_Validator_Syntactic_EnumerableMustNotHaveNullElements(typeof(T).Name, propertyName)), ref errors);
						break;
					}
					else
					{
						targetList.Add(e);
					}
				}
				return;
			}
			else
			{
				InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<T>(item, propertyName), ref errors);
				return;
			}
		}

		public static IEnumerable<EdmError> ValidateModelStructureAndSemantics(IEdmModel model, ValidationRuleSet semanticRuleSet)
		{
			InterfaceValidator interfaceValidator = new InterfaceValidator(null, model, true);
			List<EdmError> edmErrors = new List<EdmError>(interfaceValidator.ValidateStructure(model));
			InterfaceValidator interfaceValidator1 = new InterfaceValidator(interfaceValidator.visited, model, false);
			for (IEnumerable<object> i = interfaceValidator.danglingReferences; i.FirstOrDefault<object>() != null; i = interfaceValidator1.danglingReferences.ToArray<object>())
			{
				foreach (object obj in i)
				{
					edmErrors.AddRange(interfaceValidator1.ValidateStructure(obj));
				}
			}
			if (!edmErrors.Any<EdmError>(new Func<EdmError, bool>(ValidationHelper.IsInterfaceCritical)))
			{
				ValidationContext validationContext = new ValidationContext(model, (object item) => {
					if (interfaceValidator.visitedBad.Contains(item))
					{
						return true;
					}
					else
					{
						return interfaceValidator1.visitedBad.Contains(item);
					}
				}
				);
				Dictionary<Type, List<ValidationRule>> types = new Dictionary<Type, List<ValidationRule>>();
				foreach (object semanticInterfaceVisitorsForObject in interfaceValidator.visited)
				{
					if (interfaceValidator.visitedBad.Contains(semanticInterfaceVisitorsForObject))
					{
						continue;
					}
					IEnumerator<ValidationRule> enumerator = InterfaceValidator.GetSemanticInterfaceVisitorsForObject(semanticInterfaceVisitorsForObject.GetType(), semanticRuleSet, types).GetEnumerator();
					using (enumerator)
					{
						while (enumerator.MoveNext())
						{
							ValidationRule validationRule = enumerator.Current;
							validationRule.Evaluate(validationContext, semanticInterfaceVisitorsForObject);
						}
					}
				}
				edmErrors.AddRange(validationContext.Errors);
				return edmErrors;
			}
			else
			{
				return edmErrors;
			}
		}

		private IEnumerable<EdmError> ValidateStructure(object item)
		{
			IEnumerable<EdmError> edmErrors = null;
			if (item as IEdmValidCoreModelElement != null || this.visited.Contains(item) || this.skipVisitation != null && this.skipVisitation.Contains(item))
			{
				return Enumerable.Empty<EdmError>();
			}
			else
			{
				this.visited.Add(item);
				if (this.danglingReferences.Contains(item))
				{
					this.danglingReferences.Remove(item);
				}
				List<EdmError> edmErrors1 = null;
				List<object> objs = new List<object>();
				List<object> objs1 = new List<object>();
				IEnumerable<InterfaceValidator.VisitorBase> visitorBases = InterfaceValidator.ConcreteTypeInterfaceVisitors.Evaluate(item.GetType());
				foreach (InterfaceValidator.VisitorBase visitorBase in visitorBases)
				{
					edmErrors = visitorBase.Visit(item, objs, objs1);
					if (edmErrors == null)
					{
						continue;
					}
					IEnumerator<EdmError> enumerator = edmErrors.GetEnumerator();
					using (enumerator)
					{
						while (enumerator.MoveNext())
						{
							EdmError edmError = enumerator.Current;
							if (edmErrors1 == null)
							{
								edmErrors1 = new List<EdmError>();
							}
							edmErrors1.Add(edmError);
						}
					}
				}
				if (edmErrors1 == null)
				{
					List<EdmError> edmErrors2 = new List<EdmError>();
					if (this.validateDirectValueAnnotations)
					{
						IEdmElement edmElement = item as IEdmElement;
						if (edmElement != null)
						{
							foreach (IEdmDirectValueAnnotation edmDirectValueAnnotation in this.model.DirectValueAnnotations(edmElement))
							{
								edmErrors2.AddRange(this.ValidateStructure(edmDirectValueAnnotation));
							}
						}
					}
					foreach (object obj in objs)
					{
						edmErrors2.AddRange(this.ValidateStructure(obj));
					}
					foreach (object obj1 in objs1)
					{
						this.CollectReference(obj1);
					}
					return edmErrors2;
				}
				else
				{
					this.visitedBad.Add(item);
					return edmErrors1;
				}
			}
		}

		private abstract class VisitorBase
		{
			protected VisitorBase()
			{
			}

			public abstract IEnumerable<EdmError> Visit(object item, List<object> followup, List<object> references);
		}

		private sealed class VisitorOfIEdmBinaryTypeReference : InterfaceValidator.VisitorOfT<IEdmBinaryTypeReference>
		{
			public VisitorOfIEdmBinaryTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmBinaryTypeReference typeRef, List<object> followup, List<object> references)
			{
				IEdmPrimitiveType definition = typeRef.Definition as IEdmPrimitiveType;
				if (definition == null || definition.PrimitiveKind == EdmPrimitiveTypeKind.Binary)
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePrimitiveTypeRefInterfaceTypeKindValueMismatchError<IEdmBinaryTypeReference>(typeRef);
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmBinaryValue : InterfaceValidator.VisitorOfT<IEdmBinaryValue>
		{
			public VisitorOfIEdmBinaryValue()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmBinaryValue value, List<object> followup, List<object> references)
			{
				if (value.Value == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmBinaryValue>(value, "Value");
					return edmErrorArray;
				}
				else
				{
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmCheckable : InterfaceValidator.VisitorOfT<IEdmCheckable>
		{
			public VisitorOfIEdmCheckable()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmCheckable checkable, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = new List<EdmError>();
				List<EdmError> edmErrors1 = null;
				InterfaceValidator.ProcessEnumerable<IEdmCheckable, EdmError>(checkable, checkable.Errors, "Errors", edmErrors, ref edmErrors1);
				List<EdmError> edmErrors2 = edmErrors1;
				IEnumerable<EdmError> edmErrors3 = edmErrors2;
				if (edmErrors2 == null)
				{
					edmErrors3 = edmErrors;
				}
				return edmErrors3;
			}
		}

		private sealed class VisitorOfIEdmCollectionExpression : InterfaceValidator.VisitorOfT<IEdmCollectionExpression>
		{
			public VisitorOfIEdmCollectionExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmCollectionExpression expression, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				InterfaceValidator.ProcessEnumerable<IEdmCollectionExpression, IEdmExpression>(expression, expression.Elements, "Elements", followup, ref edmErrors);
				if (expression.DeclaredType != null)
				{
					followup.Add(expression.DeclaredType);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmCollectionType : InterfaceValidator.VisitorOfT<IEdmCollectionType>
		{
			public VisitorOfIEdmCollectionType()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmCollectionType type, List<object> followup, List<object> references)
			{
				if (type.ElementType == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmCollectionType>(type, "ElementType");
					return edmErrorArray;
				}
				else
				{
					followup.Add(type.ElementType);
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmCollectionTypeReference : InterfaceValidator.VisitorOfT<IEdmCollectionTypeReference>
		{
			public VisitorOfIEdmCollectionTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmCollectionTypeReference typeRef, List<object> followup, List<object> references)
			{
				if (typeRef.Definition == null || typeRef.Definition.TypeKind == EdmTypeKind.Collection)
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreateTypeRefInterfaceTypeKindValueMismatchError<IEdmCollectionTypeReference>(typeRef);
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmCollectionValue : InterfaceValidator.VisitorOfT<IEdmCollectionValue>
		{
			public VisitorOfIEdmCollectionValue()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmCollectionValue value, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				InterfaceValidator.ProcessEnumerable<IEdmCollectionValue, IEdmDelayedValue>(value, value.Elements, "Elements", followup, ref edmErrors);
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmComplexTypeReference : InterfaceValidator.VisitorOfT<IEdmComplexTypeReference>
		{
			public VisitorOfIEdmComplexTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmComplexTypeReference typeRef, List<object> followup, List<object> references)
			{
				if (typeRef.Definition == null || typeRef.Definition.TypeKind == EdmTypeKind.Complex)
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreateTypeRefInterfaceTypeKindValueMismatchError<IEdmComplexTypeReference>(typeRef);
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmDecimalTypeReference : InterfaceValidator.VisitorOfT<IEdmDecimalTypeReference>
		{
			public VisitorOfIEdmDecimalTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmDecimalTypeReference typeRef, List<object> followup, List<object> references)
			{
				IEdmPrimitiveType definition = typeRef.Definition as IEdmPrimitiveType;
				if (definition == null || definition.PrimitiveKind == EdmPrimitiveTypeKind.Decimal)
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePrimitiveTypeRefInterfaceTypeKindValueMismatchError<IEdmDecimalTypeReference>(typeRef);
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmDelayedValue : InterfaceValidator.VisitorOfT<IEdmDelayedValue>
		{
			public VisitorOfIEdmDelayedValue()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmDelayedValue value, List<object> followup, List<object> references)
			{
				if (value.Value == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmDelayedValue>(value, "Value");
					return edmErrorArray;
				}
				else
				{
					followup.Add(value.Value);
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmDirectValueAnnotation : InterfaceValidator.VisitorOfT<IEdmDirectValueAnnotation>
		{
			public VisitorOfIEdmDirectValueAnnotation()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmDirectValueAnnotation annotation, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (annotation.NamespaceUri == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmDirectValueAnnotation>(annotation, "NamespaceUri"), ref edmErrors);
				}
				if (annotation.Value == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmDirectValueAnnotation>(annotation, "Value"), ref edmErrors);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmElement : InterfaceValidator.VisitorOfT<IEdmElement>
		{
			public VisitorOfIEdmElement()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmElement element, List<object> followup, List<object> references)
			{
				return null;
			}
		}

		private sealed class VisitorOfIEdmEntityContainer : InterfaceValidator.VisitorOfT<IEdmEntityContainer>
		{
			public VisitorOfIEdmEntityContainer()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmEntityContainer container, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				InterfaceValidator.ProcessEnumerable<IEdmEntityContainer, IEdmEntityContainerElement>(container, container.Elements, "Elements", followup, ref edmErrors);
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmEntityContainerElement : InterfaceValidator.VisitorOfT<IEdmEntityContainerElement>
		{
			public VisitorOfIEdmEntityContainerElement()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmEntityContainerElement element, List<object> followup, List<object> references)
			{
				EdmError edmError = null;
				EdmContainerElementKind containerElementKind = element.ContainerElementKind;
				switch (containerElementKind)
				{
					case EdmContainerElementKind.None:
					{
						if (edmError != null)
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = edmError;
							return edmErrorArray;
						}
						else
						{
							return null;
						}
					}
					case EdmContainerElementKind.EntitySet:
					{
						edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmEntityContainerElement, EdmContainerElementKind, IEdmEntitySet>(element, element.ContainerElementKind, "ContainerElementKind");
						if (edmError != null)
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = edmError;
							return edmErrorArray;
						}
						else
						{
							return null;
						}
					}
					case EdmContainerElementKind.FunctionImport:
					{
						edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmEntityContainerElement, EdmContainerElementKind, IEdmFunctionImport>(element, element.ContainerElementKind, "ContainerElementKind");
						if (edmError != null)
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = edmError;
							return edmErrorArray;
						}
						else
						{
							return null;
						}
					}
					default:
					{
						edmError = InterfaceValidator.CreateEnumPropertyOutOfRangeError<IEdmEntityContainerElement, EdmContainerElementKind>(element, element.ContainerElementKind, "ContainerElementKind");
						if (edmError != null)
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = edmError;
							return edmErrorArray;
						}
						else
						{
							return null;
						}
					}
				}
			}
		}

		private sealed class VisitorOfIEdmEntityReferenceType : InterfaceValidator.VisitorOfT<IEdmEntityReferenceType>
		{
			public VisitorOfIEdmEntityReferenceType()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmEntityReferenceType type, List<object> followup, List<object> references)
			{
				if (type.EntityType == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmEntityReferenceType>(type, "EntityType");
					return edmErrorArray;
				}
				else
				{
					references.Add(type.EntityType);
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmEntityReferenceTypeReference : InterfaceValidator.VisitorOfT<IEdmEntityReferenceTypeReference>
		{
			public VisitorOfIEdmEntityReferenceTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmEntityReferenceTypeReference typeRef, List<object> followup, List<object> references)
			{
				if (typeRef.Definition == null || typeRef.Definition.TypeKind == EdmTypeKind.EntityReference)
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreateTypeRefInterfaceTypeKindValueMismatchError<IEdmEntityReferenceTypeReference>(typeRef);
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmEntitySet : InterfaceValidator.VisitorOfT<IEdmEntitySet>
		{
			public VisitorOfIEdmEntitySet()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmEntitySet set, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (set.ElementType == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmEntitySet>(set, "ElementType"), ref edmErrors);
				}
				else
				{
					references.Add(set.ElementType);
				}
				List<IEdmNavigationTargetMapping> edmNavigationTargetMappings = new List<IEdmNavigationTargetMapping>();
				InterfaceValidator.ProcessEnumerable<IEdmEntitySet, IEdmNavigationTargetMapping>(set, set.NavigationTargets, "NavigationTargets", edmNavigationTargetMappings, ref edmErrors);
				foreach (IEdmNavigationTargetMapping edmNavigationTargetMapping in edmNavigationTargetMappings)
				{
					if (edmNavigationTargetMapping.NavigationProperty == null)
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmNavigationTargetMapping>(edmNavigationTargetMapping, "NavigationProperty"), ref edmErrors);
					}
					else
					{
						references.Add(edmNavigationTargetMapping.NavigationProperty);
					}
					if (edmNavigationTargetMapping.TargetEntitySet == null)
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmNavigationTargetMapping>(edmNavigationTargetMapping, "TargetEntitySet"), ref edmErrors);
					}
					else
					{
						references.Add(edmNavigationTargetMapping.TargetEntitySet);
					}
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmEntityType : InterfaceValidator.VisitorOfT<IEdmEntityType>
		{
			public VisitorOfIEdmEntityType()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmEntityType type, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (type.DeclaredKey != null)
				{
					InterfaceValidator.ProcessEnumerable<IEdmEntityType, IEdmStructuralProperty>(type, type.DeclaredKey, "DeclaredKey", references, ref edmErrors);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmEntityTypeReference : InterfaceValidator.VisitorOfT<IEdmEntityTypeReference>
		{
			public VisitorOfIEdmEntityTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmEntityTypeReference typeRef, List<object> followup, List<object> references)
			{
				if (typeRef.Definition == null || typeRef.Definition.TypeKind == EdmTypeKind.Entity)
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreateTypeRefInterfaceTypeKindValueMismatchError<IEdmEntityTypeReference>(typeRef);
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmEnumMember : InterfaceValidator.VisitorOfT<IEdmEnumMember>
		{
			public VisitorOfIEdmEnumMember()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmEnumMember member, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (member.DeclaringType == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmEnumMember>(member, "DeclaringType"), ref edmErrors);
				}
				else
				{
					references.Add(member.DeclaringType);
				}
				if (member.Value == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmEnumMember>(member, "Value"), ref edmErrors);
				}
				else
				{
					followup.Add(member.Value);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmEnumType : InterfaceValidator.VisitorOfT<IEdmEnumType>
		{
			public VisitorOfIEdmEnumType()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmEnumType type, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				InterfaceValidator.ProcessEnumerable<IEdmEnumType, IEdmEnumMember>(type, type.Members, "Members", followup, ref edmErrors);
				if (type.UnderlyingType == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmEnumType>(type, "UnderlyingType"), ref edmErrors);
				}
				else
				{
					references.Add(type.UnderlyingType);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmEnumTypeReference : InterfaceValidator.VisitorOfT<IEdmEnumTypeReference>
		{
			public VisitorOfIEdmEnumTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmEnumTypeReference typeRef, List<object> followup, List<object> references)
			{
				if (typeRef.Definition == null || typeRef.Definition.TypeKind == EdmTypeKind.Enum)
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreateTypeRefInterfaceTypeKindValueMismatchError<IEdmEnumTypeReference>(typeRef);
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmEnumValue : InterfaceValidator.VisitorOfT<IEdmEnumValue>
		{
			public VisitorOfIEdmEnumValue()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmEnumValue value, List<object> followup, List<object> references)
			{
				if (value.Value == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmEnumValue>(value, "Value");
					return edmErrorArray;
				}
				else
				{
					followup.Add(value.Value);
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmExpression : InterfaceValidator.VisitorOfT<IEdmExpression>
		{
			public VisitorOfIEdmExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmExpression expression, List<object> followup, List<object> references)
			{
				EdmError edmError = null;
				if (!InterfaceValidator.IsCheckableBad(expression))
				{
					EdmExpressionKind expressionKind = expression.ExpressionKind;
					switch (expressionKind)
					{
						case EdmExpressionKind.BinaryConstant:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmBinaryConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.BooleanConstant:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmBooleanConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.DateTimeConstant:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmDateTimeConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.DateTimeOffsetConstant:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmDateTimeOffsetConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.DecimalConstant:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmDecimalConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.FloatingConstant:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmFloatingConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.GuidConstant:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmGuidConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.IntegerConstant:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmIntegerConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.StringConstant:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmStringConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.TimeConstant:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmTimeConstantExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.Null:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmNullExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.Record:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmRecordExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.Collection:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmCollectionExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.Path:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmPathExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.ParameterReference:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmParameterReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.FunctionReference:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmFunctionReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.PropertyReference:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmPropertyReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.ValueTermReference:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmValueTermReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.EntitySetReference:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmEntitySetReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.EnumMemberReference:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmEnumMemberReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.If:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmIfExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.AssertType:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmAssertTypeExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.IsType:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmIsTypeExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.FunctionApplication:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmApplyExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.LabeledExpressionReference:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmLabeledExpressionReferenceExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						case EdmExpressionKind.Labeled:
						{
							edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmExpression, EdmExpressionKind, IEdmLabeledExpression>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
						default:
						{
							edmError = InterfaceValidator.CreateInterfaceKindValueUnexpectedError<IEdmExpression, EdmExpressionKind>(expression, expression.ExpressionKind, "ExpressionKind");
							break;
						}
					}
				}
				if (edmError != null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = edmError;
					return edmErrorArray;
				}
				else
				{
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmFunction : InterfaceValidator.VisitorOfT<IEdmFunction>
		{
			public VisitorOfIEdmFunction()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmFunction function, List<object> followup, List<object> references)
			{
				if (function.ReturnType == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmFunction>(function, "ReturnType");
					return edmErrorArray;
				}
				else
				{
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmFunctionBase : InterfaceValidator.VisitorOfT<IEdmFunctionBase>
		{
			public VisitorOfIEdmFunctionBase()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmFunctionBase function, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				InterfaceValidator.ProcessEnumerable<IEdmFunctionBase, IEdmFunctionParameter>(function, function.Parameters, "Parameters", followup, ref edmErrors);
				if (function.ReturnType != null)
				{
					followup.Add(function.ReturnType);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmFunctionImport : InterfaceValidator.VisitorOfT<IEdmFunctionImport>
		{
			public VisitorOfIEdmFunctionImport()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmFunctionImport functionImport, List<object> followup, List<object> references)
			{
				if (functionImport.EntitySet != null)
				{
					followup.Add(functionImport.EntitySet);
				}
				return null;
			}
		}

		private sealed class VisitorOfIEdmFunctionParameter : InterfaceValidator.VisitorOfT<IEdmFunctionParameter>
		{
			public VisitorOfIEdmFunctionParameter()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmFunctionParameter parameter, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (parameter.Type == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmFunctionParameter>(parameter, "Type"), ref edmErrors);
				}
				else
				{
					followup.Add(parameter.Type);
				}
				if (parameter.DeclaringFunction == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmFunctionParameter>(parameter, "DeclaringFunction"), ref edmErrors);
				}
				else
				{
					references.Add(parameter.DeclaringFunction);
				}
				if (parameter.Mode < EdmFunctionParameterMode.None || parameter.Mode > EdmFunctionParameterMode.InOut)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreateEnumPropertyOutOfRangeError<IEdmFunctionParameter, EdmFunctionParameterMode>(parameter, parameter.Mode, "Mode"), ref edmErrors);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmFunctionReferenceExpression : InterfaceValidator.VisitorOfT<IEdmFunctionReferenceExpression>
		{
			public VisitorOfIEdmFunctionReferenceExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmFunctionReferenceExpression expression, List<object> followup, List<object> references)
			{
				if (expression.ReferencedFunction == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmFunctionReferenceExpression>(expression, "ReferencedFunction");
					return edmErrorArray;
				}
				else
				{
					references.Add(expression.ReferencedFunction);
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmLabeledElement : InterfaceValidator.VisitorOfT<IEdmLabeledExpression>
		{
			public VisitorOfIEdmLabeledElement()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmLabeledExpression expression, List<object> followup, List<object> references)
			{
				if (expression.Expression == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmLabeledExpression>(expression, "Expression");
					return edmErrorArray;
				}
				else
				{
					followup.Add(expression.Expression);
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmModel : InterfaceValidator.VisitorOfT<IEdmModel>
		{
			public VisitorOfIEdmModel()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmModel model, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				InterfaceValidator.ProcessEnumerable<IEdmModel, IEdmSchemaElement>(model, model.SchemaElements, "SchemaElements", followup, ref edmErrors);
				InterfaceValidator.ProcessEnumerable<IEdmModel, IEdmVocabularyAnnotation>(model, model.VocabularyAnnotations, "VocabularyAnnotations", followup, ref edmErrors);
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmNamedElement : InterfaceValidator.VisitorOfT<IEdmNamedElement>
		{
			public VisitorOfIEdmNamedElement()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmNamedElement element, List<object> followup, List<object> references)
			{
				if (element.Name != null)
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmNamedElement>(element, "Name");
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmNavigationProperty : InterfaceValidator.VisitorOfT<IEdmNavigationProperty>
		{
			public VisitorOfIEdmNavigationProperty()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmNavigationProperty property, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (property.Partner == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmNavigationProperty>(property, "Partner"), ref edmErrors);
				}
				else
				{
					if (property.Partner.DeclaringType.DeclaredProperties.Contains<IEdmProperty>(property.Partner))
					{
						references.Add(property.Partner);
					}
					else
					{
						followup.Add(property.Partner);
					}
					if (property.Partner.Partner != property || property.Partner == property)
					{
						InterfaceValidator.CollectErrors(new EdmError(InterfaceValidator.GetLocation(property), EdmErrorCode.InterfaceCriticalNavigationPartnerInvalid, Strings.EdmModel_Validator_Syntactic_NavigationPartnerInvalid(property.Name)), ref edmErrors);
					}
				}
				if (property.DependentProperties != null)
				{
					InterfaceValidator.ProcessEnumerable<IEdmNavigationProperty, IEdmStructuralProperty>(property, property.DependentProperties, "DependentProperties", references, ref edmErrors);
				}
				if (property.OnDelete < EdmOnDeleteAction.None || property.OnDelete > EdmOnDeleteAction.Cascade)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreateEnumPropertyOutOfRangeError<IEdmNavigationProperty, EdmOnDeleteAction>(property, property.OnDelete, "OnDelete"), ref edmErrors);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmParameterReferenceExpression : InterfaceValidator.VisitorOfT<IEdmParameterReferenceExpression>
		{
			public VisitorOfIEdmParameterReferenceExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmParameterReferenceExpression expression, List<object> followup, List<object> references)
			{
				if (expression.ReferencedParameter == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmParameterReferenceExpression>(expression, "ReferencedParameter");
					return edmErrorArray;
				}
				else
				{
					references.Add(expression.ReferencedParameter);
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmPathExpression : InterfaceValidator.VisitorOfT<IEdmPathExpression>
		{
			public VisitorOfIEdmPathExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmPathExpression expression, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				List<string> strs = new List<string>();
				InterfaceValidator.ProcessEnumerable<IEdmPathExpression, string>(expression, expression.Path, "Path", strs, ref edmErrors);
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmPrimitiveType : InterfaceValidator.VisitorOfT<IEdmPrimitiveType>
		{
			public VisitorOfIEdmPrimitiveType()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmPrimitiveType type, List<object> followup, List<object> references)
			{
				if (InterfaceValidator.IsCheckableBad(type) || type.PrimitiveKind >= EdmPrimitiveTypeKind.None && type.PrimitiveKind <= EdmPrimitiveTypeKind.GeometryMultiPoint)
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreateInterfaceKindValueUnexpectedError<IEdmPrimitiveType, EdmPrimitiveTypeKind>(type, type.PrimitiveKind, "PrimitiveKind");
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmPrimitiveTypeReference : InterfaceValidator.VisitorOfT<IEdmPrimitiveTypeReference>
		{
			public VisitorOfIEdmPrimitiveTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmPrimitiveTypeReference typeRef, List<object> followup, List<object> references)
			{
				if (typeRef.Definition == null || typeRef.Definition.TypeKind == EdmTypeKind.Primitive)
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreateTypeRefInterfaceTypeKindValueMismatchError<IEdmPrimitiveTypeReference>(typeRef);
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmProperty : InterfaceValidator.VisitorOfT<IEdmProperty>
		{
			public VisitorOfIEdmProperty()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmProperty property, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				EdmPropertyKind propertyKind = property.PropertyKind;
				switch (propertyKind)
				{
					case EdmPropertyKind.Structural:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmProperty, EdmPropertyKind, IEdmStructuralProperty>(property, property.PropertyKind, "PropertyKind"), ref edmErrors);
						if (property.Type == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmProperty>(property, "Type"), ref edmErrors);
						}
						else
						{
							followup.Add(property.Type);
						}
						if (property.DeclaringType == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmProperty>(property, "DeclaringType"), ref edmErrors);
						}
						else
						{
							references.Add(property.DeclaringType);
						}
						return edmErrors;
					}
					case EdmPropertyKind.Navigation:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmProperty, EdmPropertyKind, IEdmNavigationProperty>(property, property.PropertyKind, "PropertyKind"), ref edmErrors);
						if (property.Type == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmProperty>(property, "Type"), ref edmErrors);
						}
						else
						{
							followup.Add(property.Type);
						}
						if (property.DeclaringType == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmProperty>(property, "DeclaringType"), ref edmErrors);
						}
						else
						{
							references.Add(property.DeclaringType);
						}
						return edmErrors;
					}
					case EdmPropertyKind.None:
					{
						if (property.Type == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmProperty>(property, "Type"), ref edmErrors);
						}
						else
						{
							followup.Add(property.Type);
						}
						if (property.DeclaringType == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmProperty>(property, "DeclaringType"), ref edmErrors);
						}
						else
						{
							references.Add(property.DeclaringType);
						}
						return edmErrors;
					}
					default:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CreateInterfaceKindValueUnexpectedError<IEdmProperty, EdmPropertyKind>(property, property.PropertyKind, "PropertyKind"), ref edmErrors);
						if (property.Type == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmProperty>(property, "Type"), ref edmErrors);
						}
						else
						{
							followup.Add(property.Type);
						}
						if (property.DeclaringType == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmProperty>(property, "DeclaringType"), ref edmErrors);
						}
						else
						{
							references.Add(property.DeclaringType);
						}
						return edmErrors;
					}
				}
			}
		}

		private sealed class VisitorOfIEdmPropertyConstructor : InterfaceValidator.VisitorOfT<IEdmPropertyConstructor>
		{
			public VisitorOfIEdmPropertyConstructor()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmPropertyConstructor expression, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (expression.Name == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmPropertyConstructor>(expression, "Name"), ref edmErrors);
				}
				if (expression.Value == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmPropertyConstructor>(expression, "Value"), ref edmErrors);
				}
				else
				{
					followup.Add(expression.Value);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmPropertyReferenceExpression : InterfaceValidator.VisitorOfT<IEdmPropertyReferenceExpression>
		{
			public VisitorOfIEdmPropertyReferenceExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmPropertyReferenceExpression expression, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (expression.Base == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmPropertyReferenceExpression>(expression, "Base"), ref edmErrors);
				}
				else
				{
					followup.Add(expression.Base);
				}
				if (expression.ReferencedProperty == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmPropertyReferenceExpression>(expression, "ReferencedProperty"), ref edmErrors);
				}
				else
				{
					references.Add(expression.ReferencedProperty);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmPropertyValue : InterfaceValidator.VisitorOfT<IEdmPropertyValue>
		{
			public VisitorOfIEdmPropertyValue()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmPropertyValue value, List<object> followup, List<object> references)
			{
				if (value.Name == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmPropertyValue>(value, "Name");
					return edmErrorArray;
				}
				else
				{
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmPropertyValueBinding : InterfaceValidator.VisitorOfT<IEdmPropertyValueBinding>
		{
			public VisitorOfIEdmPropertyValueBinding()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmPropertyValueBinding binding, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (binding.Value == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmPropertyValueBinding>(binding, "Value"), ref edmErrors);
				}
				else
				{
					followup.Add(binding.Value);
				}
				if (binding.BoundProperty == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmPropertyValueBinding>(binding, "BoundProperty"), ref edmErrors);
				}
				else
				{
					references.Add(binding.BoundProperty);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmRecordExpression : InterfaceValidator.VisitorOfT<IEdmRecordExpression>
		{
			public VisitorOfIEdmRecordExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmRecordExpression expression, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				InterfaceValidator.ProcessEnumerable<IEdmRecordExpression, IEdmPropertyConstructor>(expression, expression.Properties, "Properties", followup, ref edmErrors);
				if (expression.DeclaredType != null)
				{
					followup.Add(expression.DeclaredType);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmRowTypeReference : InterfaceValidator.VisitorOfT<IEdmRowTypeReference>
		{
			public VisitorOfIEdmRowTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmRowTypeReference typeRef, List<object> followup, List<object> references)
			{
				if (typeRef.Definition == null || typeRef.Definition.TypeKind == EdmTypeKind.Row)
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreateTypeRefInterfaceTypeKindValueMismatchError<IEdmRowTypeReference>(typeRef);
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmSchemaElement : InterfaceValidator.VisitorOfT<IEdmSchemaElement>
		{
			public VisitorOfIEdmSchemaElement()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmSchemaElement element, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = new List<EdmError>();
				EdmSchemaElementKind schemaElementKind = element.SchemaElementKind;
				switch (schemaElementKind)
				{
					case EdmSchemaElementKind.None:
					{
						if (element.Namespace == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmSchemaElement>(element, "Namespace"), ref edmErrors);
						}
						return edmErrors;
					}
					case EdmSchemaElementKind.TypeDefinition:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmSchemaElement, EdmSchemaElementKind, IEdmSchemaType>(element, element.SchemaElementKind, "SchemaElementKind"), ref edmErrors);
						if (element.Namespace == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmSchemaElement>(element, "Namespace"), ref edmErrors);
						}
						return edmErrors;
					}
					case EdmSchemaElementKind.Function:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmSchemaElement, EdmSchemaElementKind, IEdmFunction>(element, element.SchemaElementKind, "SchemaElementKind"), ref edmErrors);
						if (element.Namespace == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmSchemaElement>(element, "Namespace"), ref edmErrors);
						}
						return edmErrors;
					}
					case EdmSchemaElementKind.ValueTerm:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmSchemaElement, EdmSchemaElementKind, IEdmValueTerm>(element, element.SchemaElementKind, "SchemaElementKind"), ref edmErrors);
						if (element.Namespace == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmSchemaElement>(element, "Namespace"), ref edmErrors);
						}
						return edmErrors;
					}
					case EdmSchemaElementKind.EntityContainer:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmSchemaElement, EdmSchemaElementKind, IEdmEntityContainer>(element, element.SchemaElementKind, "SchemaElementKind"), ref edmErrors);
						if (element.Namespace == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmSchemaElement>(element, "Namespace"), ref edmErrors);
						}
						return edmErrors;
					}
					default:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CreateEnumPropertyOutOfRangeError<IEdmSchemaElement, EdmSchemaElementKind>(element, element.SchemaElementKind, "SchemaElementKind"), ref edmErrors);
						if (element.Namespace == null)
						{
							InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmSchemaElement>(element, "Namespace"), ref edmErrors);
						}
						return edmErrors;
					}
				}
			}
		}

		private sealed class VisitorOfIEdmSpatialTypeReference : InterfaceValidator.VisitorOfT<IEdmSpatialTypeReference>
		{
			public VisitorOfIEdmSpatialTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmSpatialTypeReference typeRef, List<object> followup, List<object> references)
			{
				IEdmPrimitiveType definition = typeRef.Definition as IEdmPrimitiveType;
				if (definition == null || definition.PrimitiveKind.IsSpatial())
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePrimitiveTypeRefInterfaceTypeKindValueMismatchError<IEdmSpatialTypeReference>(typeRef);
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmStringTypeReference : InterfaceValidator.VisitorOfT<IEdmStringTypeReference>
		{
			public VisitorOfIEdmStringTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmStringTypeReference typeRef, List<object> followup, List<object> references)
			{
				IEdmPrimitiveType definition = typeRef.Definition as IEdmPrimitiveType;
				if (definition == null || definition.PrimitiveKind == EdmPrimitiveTypeKind.String)
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePrimitiveTypeRefInterfaceTypeKindValueMismatchError<IEdmStringTypeReference>(typeRef);
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmStringValue : InterfaceValidator.VisitorOfT<IEdmStringValue>
		{
			public VisitorOfIEdmStringValue()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmStringValue value, List<object> followup, List<object> references)
			{
				if (value.Value == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmStringValue>(value, "Value");
					return edmErrorArray;
				}
				else
				{
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmStructuralProperty : InterfaceValidator.VisitorOfT<IEdmStructuralProperty>
		{
			public VisitorOfIEdmStructuralProperty()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmStructuralProperty property, List<object> followup, List<object> references)
			{
				if (property.ConcurrencyMode < EdmConcurrencyMode.None || property.ConcurrencyMode > EdmConcurrencyMode.Fixed)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreateEnumPropertyOutOfRangeError<IEdmStructuralProperty, EdmConcurrencyMode>(property, property.ConcurrencyMode, "ConcurrencyMode");
					return edmErrorArray;
				}
				else
				{
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmStructuredType : InterfaceValidator.VisitorOfT<IEdmStructuredType>
		{
			public VisitorOfIEdmStructuredType()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmStructuredType type, List<object> followup, List<object> references)
			{
				string name;
				List<EdmError> edmErrors = null;
				InterfaceValidator.ProcessEnumerable<IEdmStructuredType, IEdmProperty>(type, type.DeclaredProperties, "DeclaredProperties", followup, ref edmErrors);
				if (type.BaseType != null)
				{
					HashSetInternal<IEdmStructuredType> edmStructuredTypes = new HashSetInternal<IEdmStructuredType>();
					edmStructuredTypes.Add(type);
					IEdmStructuredType baseType = type.BaseType;
					IEdmStructuredType edmStructuredType = baseType;
					edmStructuredType = baseType;
					while (edmStructuredType != null)
					{
						if (!edmStructuredTypes.Contains(edmStructuredType))
						{
							edmStructuredType = edmStructuredType.BaseType;
						}
						else
						{
							IEdmSchemaType edmSchemaType = type as IEdmSchemaType;
							if (edmSchemaType != null)
							{
								name = edmSchemaType.FullName();
							}
							else
							{
								name = typeof(Type).Name;
							}
							string str = name;
							InterfaceValidator.CollectErrors(new EdmError(InterfaceValidator.GetLocation(type), EdmErrorCode.InterfaceCriticalCycleInTypeHierarchy, Strings.EdmModel_Validator_Syntactic_InterfaceCriticalCycleInTypeHierarchy(str)), ref edmErrors);
							break;
						}
					}
					references.Add(type.BaseType);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmStructuredTypeReference : InterfaceValidator.VisitorOfT<IEdmStructuredTypeReference>
		{
			public VisitorOfIEdmStructuredTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmStructuredTypeReference typeRef, List<object> followup, List<object> references)
			{
				if (typeRef.Definition == null || typeRef.Definition.TypeKind.IsStructured())
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreateTypeRefInterfaceTypeKindValueMismatchError<IEdmStructuredTypeReference>(typeRef);
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmStructuredValue : InterfaceValidator.VisitorOfT<IEdmStructuredValue>
		{
			public VisitorOfIEdmStructuredValue()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmStructuredValue value, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				InterfaceValidator.ProcessEnumerable<IEdmStructuredValue, IEdmPropertyValue>(value, value.PropertyValues, "PropertyValues", followup, ref edmErrors);
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmTemporalTypeReference : InterfaceValidator.VisitorOfT<IEdmTemporalTypeReference>
		{
			public VisitorOfIEdmTemporalTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmTemporalTypeReference typeRef, List<object> followup, List<object> references)
			{
				IEdmPrimitiveType definition = typeRef.Definition as IEdmPrimitiveType;
				if (definition == null || definition.PrimitiveKind.IsTemporal())
				{
					return null;
				}
				else
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePrimitiveTypeRefInterfaceTypeKindValueMismatchError<IEdmTemporalTypeReference>(typeRef);
					return edmErrorArray;
				}
			}
		}

		private sealed class VisitorOfIEdmTerm : InterfaceValidator.VisitorOfT<IEdmTerm>
		{
			public VisitorOfIEdmTerm()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmTerm term, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				EdmTermKind termKind = term.TermKind;
				switch (termKind)
				{
					case EdmTermKind.None:
					{
						return edmErrors;
					}
					case EdmTermKind.Type:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmTerm, EdmTermKind, IEdmSchemaType>(term, term.TermKind, "TermKind"), ref edmErrors);
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmTerm, EdmTermKind, IEdmStructuredType>(term, term.TermKind, "TermKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmTermKind.Value:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmTerm, EdmTermKind, IEdmValueTerm>(term, term.TermKind, "TermKind"), ref edmErrors);
						return edmErrors;
					}
					default:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CreateInterfaceKindValueUnexpectedError<IEdmTerm, EdmTermKind>(term, term.TermKind, "TermKind"), ref edmErrors);
						return edmErrors;
					}
				}
			}
		}

		private sealed class VisitorOfIEdmType : InterfaceValidator.VisitorOfT<IEdmType>
		{
			public VisitorOfIEdmType()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmType type, List<object> followup, List<object> references)
			{
				EdmError edmError = null;
				EdmTypeKind typeKind = type.TypeKind;
				switch (typeKind)
				{
					case EdmTypeKind.None:
					{
						if (edmError != null)
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = edmError;
							return edmErrorArray;
						}
						else
						{
							return null;
						}
					}
					case EdmTypeKind.Primitive:
					{
						edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmPrimitiveType>(type, type.TypeKind, "TypeKind");
						if (edmError != null)
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = edmError;
							return edmErrorArray;
						}
						else
						{
							return null;
						}
					}
					case EdmTypeKind.Entity:
					{
						edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmEntityType>(type, type.TypeKind, "TypeKind");
						if (edmError != null)
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = edmError;
							return edmErrorArray;
						}
						else
						{
							return null;
						}
					}
					case EdmTypeKind.Complex:
					{
						edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmComplexType>(type, type.TypeKind, "TypeKind");
						if (edmError != null)
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = edmError;
							return edmErrorArray;
						}
						else
						{
							return null;
						}
					}
					case EdmTypeKind.Row:
					{
						edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmRowType>(type, type.TypeKind, "TypeKind");
						if (edmError != null)
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = edmError;
							return edmErrorArray;
						}
						else
						{
							return null;
						}
					}
					case EdmTypeKind.Collection:
					{
						edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmCollectionType>(type, type.TypeKind, "TypeKind");
						if (edmError != null)
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = edmError;
							return edmErrorArray;
						}
						else
						{
							return null;
						}
					}
					case EdmTypeKind.EntityReference:
					{
						edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmEntityReferenceType>(type, type.TypeKind, "TypeKind");
						if (edmError != null)
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = edmError;
							return edmErrorArray;
						}
						else
						{
							return null;
						}
					}
					case EdmTypeKind.Enum:
					{
						edmError = InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmType, EdmTypeKind, IEdmEnumType>(type, type.TypeKind, "TypeKind");
						if (edmError != null)
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = edmError;
							return edmErrorArray;
						}
						else
						{
							return null;
						}
					}
					default:
					{
						edmError = InterfaceValidator.CreateInterfaceKindValueUnexpectedError<IEdmType, EdmTypeKind>(type, type.TypeKind, "TypeKind");
						if (edmError != null)
						{
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = edmError;
							return edmErrorArray;
						}
						else
						{
							return null;
						}
					}
				}
			}
		}

		private sealed class VisitorOfIEdmTypeAnnotation : InterfaceValidator.VisitorOfT<IEdmTypeAnnotation>
		{
			public VisitorOfIEdmTypeAnnotation()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmTypeAnnotation annotation, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				InterfaceValidator.ProcessEnumerable<IEdmTypeAnnotation, IEdmPropertyValueBinding>(annotation, annotation.PropertyValueBindings, "PropertyValueBindings", followup, ref edmErrors);
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmTypeReference : InterfaceValidator.VisitorOfT<IEdmTypeReference>
		{
			public VisitorOfIEdmTypeReference()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmTypeReference type, List<object> followup, List<object> references)
			{
				if (type.Definition == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmTypeReference>(type, "Definition");
					return edmErrorArray;
				}
				else
				{
					if (type.Definition as IEdmSchemaType == null)
					{
						followup.Add(type.Definition);
					}
					else
					{
						references.Add(type.Definition);
					}
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmValue : InterfaceValidator.VisitorOfT<IEdmValue>
		{
			public VisitorOfIEdmValue()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmValue value, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (value.Type != null)
				{
					followup.Add(value.Type);
				}
				EdmValueKind valueKind = value.ValueKind;
				switch (valueKind)
				{
					case EdmValueKind.Binary:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmBinaryValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.Boolean:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmBooleanValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.Collection:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmCollectionValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.DateTimeOffset:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmDateTimeOffsetValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.DateTime:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmDateTimeValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.Decimal:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmDecimalValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.Enum:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmEnumValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.Floating:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmFloatingValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.Guid:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmGuidValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.Integer:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmIntegerValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.Null:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmNullValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.String:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmStringValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.Structured:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmStructuredValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.Time:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CheckForInterfaceKindValueMismatchError<IEdmValue, EdmValueKind, IEdmTimeValue>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
					case EdmValueKind.None:
					{
						return edmErrors;
					}
					default:
					{
						InterfaceValidator.CollectErrors(InterfaceValidator.CreateInterfaceKindValueUnexpectedError<IEdmValue, EdmValueKind>(value, value.ValueKind, "ValueKind"), ref edmErrors);
						return edmErrors;
					}
				}
			}
		}

		private sealed class VisitorOfIEdmValueAnnotation : InterfaceValidator.VisitorOfT<IEdmValueAnnotation>
		{
			public VisitorOfIEdmValueAnnotation()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmValueAnnotation annotation, List<object> followup, List<object> references)
			{
				if (annotation.Value == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmValueAnnotation>(annotation, "Value");
					return edmErrorArray;
				}
				else
				{
					followup.Add(annotation.Value);
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmValueTerm : InterfaceValidator.VisitorOfT<IEdmValueTerm>
		{
			public VisitorOfIEdmValueTerm()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmValueTerm term, List<object> followup, List<object> references)
			{
				if (term.Type == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmValueTerm>(term, "Type");
					return edmErrorArray;
				}
				else
				{
					followup.Add(term.Type);
					return null;
				}
			}
		}

		private sealed class VisitorOfIEdmValueTermReferenceExpression : InterfaceValidator.VisitorOfT<IEdmValueTermReferenceExpression>
		{
			public VisitorOfIEdmValueTermReferenceExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmValueTermReferenceExpression expression, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (expression.Base == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmValueTermReferenceExpression>(expression, "Base"), ref edmErrors);
				}
				else
				{
					followup.Add(expression.Base);
				}
				if (expression.Term == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmValueTermReferenceExpression>(expression, "Term"), ref edmErrors);
				}
				else
				{
					references.Add(expression.Term);
				}
				return edmErrors;
			}
		}

		private sealed class VisitorOfIEdmVocabularyAnnotation : InterfaceValidator.VisitorOfT<IEdmVocabularyAnnotation>
		{
			public VisitorOfIEdmVocabularyAnnotation()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmVocabularyAnnotation annotation, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (annotation.Term == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmVocabularyAnnotation>(annotation, "Term"), ref edmErrors);
				}
				else
				{
					references.Add(annotation.Term);
				}
				if (annotation.Target == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmVocabularyAnnotation>(annotation, "Target"), ref edmErrors);
				}
				else
				{
					references.Add(annotation.Target);
				}
				return edmErrors;
			}
		}

		private abstract class VisitorOfT<T> : InterfaceValidator.VisitorBase
		{
			protected VisitorOfT()
			{
			}

			public override IEnumerable<EdmError> Visit(object item, List<object> followup, List<object> references)
			{
				return this.VisitT((T)item, followup, references);
			}

			protected abstract IEnumerable<EdmError> VisitT(T item, List<object> followup, List<object> references);
		}

		private sealed class VistorOfIEdmAssertTypeExpression : InterfaceValidator.VisitorOfT<IEdmAssertTypeExpression>
		{
			public VistorOfIEdmAssertTypeExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmAssertTypeExpression expression, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (expression.Operand == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmAssertTypeExpression>(expression, "Operand"), ref edmErrors);
				}
				else
				{
					followup.Add(expression.Operand);
				}
				if (expression.Type == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmAssertTypeExpression>(expression, "Type"), ref edmErrors);
				}
				else
				{
					followup.Add(expression.Type);
				}
				return edmErrors;
			}
		}

		private sealed class VistorOfIEdmEntitySetReferenceExpression : InterfaceValidator.VisitorOfT<IEdmEntitySetReferenceExpression>
		{
			public VistorOfIEdmEntitySetReferenceExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmEntitySetReferenceExpression expression, List<object> followup, List<object> references)
			{
				if (expression.ReferencedEntitySet == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmEntitySetReferenceExpression>(expression, "ReferencedEntitySet");
					return edmErrorArray;
				}
				else
				{
					references.Add(expression.ReferencedEntitySet);
					return null;
				}
			}
		}

		private sealed class VistorOfIEdmEnumMemberReferenceExpression : InterfaceValidator.VisitorOfT<IEdmEnumMemberReferenceExpression>
		{
			public VistorOfIEdmEnumMemberReferenceExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmEnumMemberReferenceExpression expression, List<object> followup, List<object> references)
			{
				if (expression.ReferencedEnumMember == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmEnumMemberReferenceExpression>(expression, "ReferencedEnumMember");
					return edmErrorArray;
				}
				else
				{
					references.Add(expression.ReferencedEnumMember);
					return null;
				}
			}
		}

		private sealed class VistorOfIEdmFunctionApplicationExpression : InterfaceValidator.VisitorOfT<IEdmApplyExpression>
		{
			public VistorOfIEdmFunctionApplicationExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmApplyExpression expression, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (expression.AppliedFunction == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmApplyExpression>(expression, "AppliedFunction"), ref edmErrors);
				}
				else
				{
					followup.Add(expression.AppliedFunction);
				}
				InterfaceValidator.ProcessEnumerable<IEdmApplyExpression, IEdmExpression>(expression, expression.Arguments, "Arguments", followup, ref edmErrors);
				return edmErrors;
			}
		}

		private sealed class VistorOfIEdmIfExpression : InterfaceValidator.VisitorOfT<IEdmIfExpression>
		{
			public VistorOfIEdmIfExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmIfExpression expression, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (expression.TestExpression == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmIfExpression>(expression, "TestExpression"), ref edmErrors);
				}
				else
				{
					followup.Add(expression.TestExpression);
				}
				if (expression.TrueExpression == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmIfExpression>(expression, "TrueExpression"), ref edmErrors);
				}
				else
				{
					followup.Add(expression.TrueExpression);
				}
				if (expression.FalseExpression == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmIfExpression>(expression, "FalseExpression"), ref edmErrors);
				}
				else
				{
					followup.Add(expression.FalseExpression);
				}
				return edmErrors;
			}
		}

		private sealed class VistorOfIEdmIsTypeExpression : InterfaceValidator.VisitorOfT<IEdmIsTypeExpression>
		{
			public VistorOfIEdmIsTypeExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmIsTypeExpression expression, List<object> followup, List<object> references)
			{
				List<EdmError> edmErrors = null;
				if (expression.Operand == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmIsTypeExpression>(expression, "Operand"), ref edmErrors);
				}
				else
				{
					followup.Add(expression.Operand);
				}
				if (expression.Type == null)
				{
					InterfaceValidator.CollectErrors(InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmIsTypeExpression>(expression, "Type"), ref edmErrors);
				}
				else
				{
					followup.Add(expression.Type);
				}
				return edmErrors;
			}
		}

		private sealed class VistorOfIEdmLabeledElementReferenceExpression : InterfaceValidator.VisitorOfT<IEdmLabeledExpressionReferenceExpression>
		{
			public VistorOfIEdmLabeledElementReferenceExpression()
			{
			}

			protected override IEnumerable<EdmError> VisitT(IEdmLabeledExpressionReferenceExpression expression, List<object> followup, List<object> references)
			{
				if (expression.ReferencedLabeledExpression == null)
				{
					EdmError[] edmErrorArray = new EdmError[1];
					edmErrorArray[0] = InterfaceValidator.CreatePropertyMustNotBeNullError<IEdmLabeledExpressionReferenceExpression>(expression, "ReferencedLabeledExpression");
					return edmErrorArray;
				}
				else
				{
					references.Add(expression.ReferencedLabeledExpression);
					return null;
				}
			}
		}
	}
}