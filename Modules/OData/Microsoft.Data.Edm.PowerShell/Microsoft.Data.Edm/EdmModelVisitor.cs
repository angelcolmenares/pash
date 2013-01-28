using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Expressions;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm
{
	internal abstract class EdmModelVisitor
	{
		protected readonly IEdmModel Model;

		protected EdmModelVisitor(IEdmModel model)
		{
			this.Model = model;
		}

		protected virtual void ProcessAssertTypeExpression(IEdmAssertTypeExpression expression)
		{
			this.ProcessExpression(expression);
			this.VisitTypeReference(expression.Type);
			this.VisitExpression(expression.Operand);
		}

		protected virtual void ProcessBinaryConstantExpression(IEdmBinaryConstantExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessBinaryTypeReference(IEdmBinaryTypeReference reference)
		{
			this.ProcessPrimitiveTypeReference(reference);
		}

		protected virtual void ProcessBooleanConstantExpression(IEdmBooleanConstantExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessCollectionExpression(IEdmCollectionExpression expression)
		{
			this.ProcessExpression(expression);
			this.VisitExpressions(expression.Elements);
		}

		protected virtual void ProcessCollectionType(IEdmCollectionType definition)
		{
			this.ProcessElement(definition);
			this.ProcessType(definition);
			this.VisitTypeReference(definition.ElementType);
		}

		protected virtual void ProcessCollectionTypeReference(IEdmCollectionTypeReference reference)
		{
			this.ProcessTypeReference(reference);
			this.ProcessCollectionType(reference.CollectionDefinition());
		}

		protected virtual void ProcessComplexType(IEdmComplexType definition)
		{
			this.ProcessSchemaElement(definition);
			this.ProcessStructuredType(definition);
			this.ProcessSchemaType(definition);
		}

		protected virtual void ProcessComplexTypeReference(IEdmComplexTypeReference reference)
		{
			this.ProcessStructuredTypeReference(reference);
		}

		protected virtual void ProcessDateTimeConstantExpression(IEdmDateTimeConstantExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessDateTimeOffsetConstantExpression(IEdmDateTimeOffsetConstantExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessDecimalConstantExpression(IEdmDecimalConstantExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessDecimalTypeReference(IEdmDecimalTypeReference reference)
		{
			this.ProcessPrimitiveTypeReference(reference);
		}

		protected virtual void ProcessElement(IEdmElement element)
		{
			this.VisitAnnotations(this.Model.DirectValueAnnotations(element));
		}

		protected virtual void ProcessEntityContainer(IEdmEntityContainer container)
		{
			this.ProcessVocabularyAnnotatable(container);
			this.ProcessNamedElement(container);
			this.VisitEntityContainerElements(container.Elements);
		}

		protected virtual void ProcessEntityContainerElement(IEdmEntityContainerElement element)
		{
			this.ProcessNamedElement(element);
		}

		protected virtual void ProcessEntityReferenceType(IEdmEntityReferenceType definition)
		{
			this.ProcessElement(definition);
			this.ProcessType(definition);
		}

		protected virtual void ProcessEntityReferenceTypeReference(IEdmEntityReferenceTypeReference reference)
		{
			this.ProcessTypeReference(reference);
			this.ProcessEntityReferenceType(reference.EntityReferenceDefinition());
		}

		protected virtual void ProcessEntitySet(IEdmEntitySet set)
		{
			this.ProcessEntityContainerElement(set);
		}

		protected virtual void ProcessEntitySetReferenceExpression(IEdmEntitySetReferenceExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessEntityType(IEdmEntityType definition)
		{
			this.ProcessSchemaElement(definition);
			this.ProcessTerm(definition);
			this.ProcessStructuredType(definition);
			this.ProcessSchemaType(definition);
		}

		protected virtual void ProcessEntityTypeReference(IEdmEntityTypeReference reference)
		{
			this.ProcessStructuredTypeReference(reference);
		}

		protected virtual void ProcessEnumMember(IEdmEnumMember enumMember)
		{
			this.ProcessNamedElement(enumMember);
		}

		protected virtual void ProcessEnumMemberReferenceExpression(IEdmEnumMemberReferenceExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessEnumType(IEdmEnumType definition)
		{
			this.ProcessSchemaElement(definition);
			this.ProcessType(definition);
			this.ProcessSchemaType(definition);
			this.VisitEnumMembers(definition.Members);
		}

		protected virtual void ProcessEnumTypeReference(IEdmEnumTypeReference reference)
		{
			this.ProcessTypeReference(reference);
		}

		protected virtual void ProcessExpression(IEdmExpression expression)
		{
		}

		protected virtual void ProcessFloatingConstantExpression(IEdmFloatingConstantExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessFunction(IEdmFunction function)
		{
			this.ProcessSchemaElement(function);
			this.ProcessFunctionBase(function);
		}

		protected virtual void ProcessFunctionApplicationExpression(IEdmApplyExpression expression)
		{
			this.ProcessExpression(expression);
			this.VisitExpression(expression.AppliedFunction);
			this.VisitExpressions(expression.Arguments);
		}

		protected virtual void ProcessFunctionBase(IEdmFunctionBase functionBase)
		{
			if (functionBase.ReturnType != null)
			{
				this.VisitTypeReference(functionBase.ReturnType);
			}
			this.VisitFunctionParameters(functionBase.Parameters);
		}

		protected virtual void ProcessFunctionImport(IEdmFunctionImport functionImport)
		{
			this.ProcessEntityContainerElement(functionImport);
			this.ProcessFunctionBase(functionImport);
		}

		protected virtual void ProcessFunctionParameter(IEdmFunctionParameter parameter)
		{
			this.ProcessVocabularyAnnotatable(parameter);
			this.ProcessNamedElement(parameter);
			this.VisitTypeReference(parameter.Type);
		}

		protected virtual void ProcessFunctionReferenceExpression(IEdmFunctionReferenceExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessGuidConstantExpression(IEdmGuidConstantExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessIfExpression(IEdmIfExpression expression)
		{
			this.ProcessExpression(expression);
			this.VisitExpression(expression.TestExpression);
			this.VisitExpression(expression.TrueExpression);
			this.VisitExpression(expression.FalseExpression);
		}

		protected virtual void ProcessImmediateValueAnnotation(IEdmDirectValueAnnotation annotation)
		{
			this.ProcessNamedElement(annotation);
		}

		protected virtual void ProcessIntegerConstantExpression(IEdmIntegerConstantExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessIsTypeExpression(IEdmIsTypeExpression expression)
		{
			this.ProcessExpression(expression);
			this.VisitTypeReference(expression.Type);
			this.VisitExpression(expression.Operand);
		}

		protected virtual void ProcessLabeledExpression(IEdmLabeledExpression element)
		{
			this.VisitExpression(element.Expression);
		}

		protected virtual void ProcessLabeledExpressionReferenceExpression(IEdmLabeledExpressionReferenceExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessModel(IEdmModel model)
		{
			this.ProcessElement(model);
			this.VisitSchemaElements(model.SchemaElements);
			this.VisitVocabularyAnnotations(model.VocabularyAnnotations);
		}

		protected virtual void ProcessNamedElement(IEdmNamedElement element)
		{
			this.ProcessElement(element);
		}

		protected virtual void ProcessNavigationProperty(IEdmNavigationProperty property)
		{
			this.ProcessProperty(property);
		}

		protected virtual void ProcessNullConstantExpression(IEdmNullExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessParameterReferenceExpression(IEdmParameterReferenceExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessPathExpression(IEdmPathExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessPrimitiveTypeReference(IEdmPrimitiveTypeReference reference)
		{
			this.ProcessTypeReference(reference);
		}

		protected virtual void ProcessProperty(IEdmProperty property)
		{
			this.ProcessVocabularyAnnotatable(property);
			this.ProcessNamedElement(property);
			this.VisitTypeReference(property.Type);
		}

		protected virtual void ProcessPropertyConstructor(IEdmPropertyConstructor constructor)
		{
			this.VisitExpression(constructor.Value);
		}

		protected virtual void ProcessPropertyReferenceExpression(IEdmPropertyReferenceExpression expression)
		{
			this.ProcessExpression(expression);
			if (expression.Base != null)
			{
				this.VisitExpression(expression.Base);
			}
		}

		protected virtual void ProcessPropertyValueBinding(IEdmPropertyValueBinding binding)
		{
			this.VisitExpression(binding.Value);
		}

		protected virtual void ProcessRecordExpression(IEdmRecordExpression expression)
		{
			this.ProcessExpression(expression);
			if (expression.DeclaredType != null)
			{
				this.VisitTypeReference(expression.DeclaredType);
			}
			this.VisitPropertyConstructors(expression.Properties);
		}

		protected virtual void ProcessRowType(IEdmRowType definition)
		{
			this.ProcessElement(definition);
			this.ProcessStructuredType(definition);
		}

		protected virtual void ProcessRowTypeReference(IEdmRowTypeReference reference)
		{
			this.ProcessStructuredTypeReference(reference);
			this.ProcessRowType(reference.RowDefinition());
		}

		protected virtual void ProcessSchemaElement(IEdmSchemaElement element)
		{
			this.ProcessVocabularyAnnotatable(element);
			this.ProcessNamedElement(element);
		}

		protected virtual void ProcessSchemaType(IEdmSchemaType type)
		{
		}

		protected virtual void ProcessSpatialTypeReference(IEdmSpatialTypeReference reference)
		{
			this.ProcessPrimitiveTypeReference(reference);
		}

		protected virtual void ProcessStringConstantExpression(IEdmStringConstantExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessStringTypeReference(IEdmStringTypeReference reference)
		{
			this.ProcessPrimitiveTypeReference(reference);
		}

		protected virtual void ProcessStructuralProperty(IEdmStructuralProperty property)
		{
			this.ProcessProperty(property);
		}

		protected virtual void ProcessStructuredType(IEdmStructuredType definition)
		{
			this.ProcessType(definition);
			this.VisitProperties(definition.DeclaredProperties);
		}

		protected virtual void ProcessStructuredTypeReference(IEdmStructuredTypeReference reference)
		{
			this.ProcessTypeReference(reference);
		}

		protected virtual void ProcessTemporalTypeReference(IEdmTemporalTypeReference reference)
		{
			this.ProcessPrimitiveTypeReference(reference);
		}

		protected virtual void ProcessTerm(IEdmTerm term)
		{
		}

		protected virtual void ProcessTimeConstantExpression(IEdmTimeConstantExpression expression)
		{
			this.ProcessExpression(expression);
		}

		protected virtual void ProcessType(IEdmType definition)
		{
		}

		protected virtual void ProcessTypeAnnotation(IEdmTypeAnnotation annotation)
		{
			this.ProcessVocabularyAnnotation(annotation);
			this.VisitPropertyValueBindings(annotation.PropertyValueBindings);
		}

		protected virtual void ProcessTypeReference(IEdmTypeReference element)
		{
			this.ProcessElement(element);
		}

		protected virtual void ProcessValueAnnotation(IEdmValueAnnotation annotation)
		{
			this.ProcessVocabularyAnnotation(annotation);
			this.VisitExpression(annotation.Value);
		}

		protected virtual void ProcessValueTerm(IEdmValueTerm term)
		{
			this.ProcessSchemaElement(term);
			this.ProcessTerm(term);
			this.VisitTypeReference(term.Type);
		}

		protected virtual void ProcessVocabularyAnnotatable(IEdmVocabularyAnnotatable annotatable)
		{
		}

		protected virtual void ProcessVocabularyAnnotation(IEdmVocabularyAnnotation annotation)
		{
			this.ProcessElement(annotation);
		}

		public void VisitAnnotation(IEdmDirectValueAnnotation annotation)
		{
			this.ProcessImmediateValueAnnotation(annotation);
		}

		public void VisitAnnotations(IEnumerable<IEdmDirectValueAnnotation> annotations)
		{
			EdmModelVisitor.VisitCollection<IEdmDirectValueAnnotation>(annotations, new Action<IEdmDirectValueAnnotation>(this.VisitAnnotation));
		}

		protected static void VisitCollection<T>(IEnumerable<T> collection, Action<T> visitMethod)
		{
			foreach (T t in collection)
			{
				visitMethod(t);
			}
		}

		public void VisitEdmModel()
		{
			this.ProcessModel(this.Model);
		}

		public void VisitEntityContainerElements(IEnumerable<IEdmEntityContainerElement> elements)
		{
			foreach (IEdmEntityContainerElement element in elements)
			{
				EdmContainerElementKind containerElementKind = element.ContainerElementKind;
				switch (containerElementKind)
				{
					case EdmContainerElementKind.None:
					{
						this.ProcessEntityContainerElement(element);
						continue;
					}
					case EdmContainerElementKind.EntitySet:
					{
						this.ProcessEntitySet((IEdmEntitySet)element);
						continue;
					}
					case EdmContainerElementKind.FunctionImport:
					{
						this.ProcessFunctionImport((IEdmFunctionImport)element);
						continue;
					}
				}
				throw new InvalidOperationException(Strings.UnknownEnumVal_ContainerElementKind(element.ContainerElementKind.ToString()));
			}
		}

		public void VisitEnumMember(IEdmEnumMember enumMember)
		{
			this.ProcessEnumMember(enumMember);
		}

		public void VisitEnumMembers(IEnumerable<IEdmEnumMember> enumMembers)
		{
			EdmModelVisitor.VisitCollection<IEdmEnumMember>(enumMembers, new Action<IEdmEnumMember>(this.VisitEnumMember));
		}

		public void VisitExpression(IEdmExpression expression)
		{
			EdmExpressionKind expressionKind = expression.ExpressionKind;
			switch (expressionKind)
			{
				case EdmExpressionKind.None:
				{
					this.ProcessExpression(expression);
					return;
				}
				case EdmExpressionKind.BinaryConstant:
				{
					this.ProcessBinaryConstantExpression((IEdmBinaryConstantExpression)expression);
					return;
				}
				case EdmExpressionKind.BooleanConstant:
				{
					this.ProcessBooleanConstantExpression((IEdmBooleanConstantExpression)expression);
					return;
				}
				case EdmExpressionKind.DateTimeConstant:
				{
					this.ProcessDateTimeConstantExpression((IEdmDateTimeConstantExpression)expression);
					return;
				}
				case EdmExpressionKind.DateTimeOffsetConstant:
				{
					this.ProcessDateTimeOffsetConstantExpression((IEdmDateTimeOffsetConstantExpression)expression);
					return;
				}
				case EdmExpressionKind.DecimalConstant:
				{
					this.ProcessDecimalConstantExpression((IEdmDecimalConstantExpression)expression);
					return;
				}
				case EdmExpressionKind.FloatingConstant:
				{
					this.ProcessFloatingConstantExpression((IEdmFloatingConstantExpression)expression);
					return;
				}
				case EdmExpressionKind.GuidConstant:
				{
					this.ProcessGuidConstantExpression((IEdmGuidConstantExpression)expression);
					return;
				}
				case EdmExpressionKind.IntegerConstant:
				{
					this.ProcessIntegerConstantExpression((IEdmIntegerConstantExpression)expression);
					return;
				}
				case EdmExpressionKind.StringConstant:
				{
					this.ProcessStringConstantExpression((IEdmStringConstantExpression)expression);
					return;
				}
				case EdmExpressionKind.TimeConstant:
				{
					this.ProcessTimeConstantExpression((IEdmTimeConstantExpression)expression);
					return;
				}
				case EdmExpressionKind.Null:
				{
					this.ProcessNullConstantExpression((IEdmNullExpression)expression);
					return;
				}
				case EdmExpressionKind.Record:
				{
					this.ProcessRecordExpression((IEdmRecordExpression)expression);
					return;
				}
				case EdmExpressionKind.Collection:
				{
					this.ProcessCollectionExpression((IEdmCollectionExpression)expression);
					return;
				}
				case EdmExpressionKind.Path:
				{
					this.ProcessPathExpression((IEdmPathExpression)expression);
					return;
				}
				case EdmExpressionKind.ParameterReference:
				{
					this.ProcessParameterReferenceExpression((IEdmParameterReferenceExpression)expression);
					return;
				}
				case EdmExpressionKind.FunctionReference:
				{
					this.ProcessFunctionReferenceExpression((IEdmFunctionReferenceExpression)expression);
					return;
				}
				case EdmExpressionKind.PropertyReference:
				{
					this.ProcessPropertyReferenceExpression((IEdmPropertyReferenceExpression)expression);
					return;
				}
				case EdmExpressionKind.ValueTermReference:
				{
					this.ProcessPropertyReferenceExpression((IEdmPropertyReferenceExpression)expression);
					return;
				}
				case EdmExpressionKind.EntitySetReference:
				{
					this.ProcessEntitySetReferenceExpression((IEdmEntitySetReferenceExpression)expression);
					return;
				}
				case EdmExpressionKind.EnumMemberReference:
				{
					this.ProcessEnumMemberReferenceExpression((IEdmEnumMemberReferenceExpression)expression);
					return;
				}
				case EdmExpressionKind.If:
				{
					this.ProcessIfExpression((IEdmIfExpression)expression);
					return;
				}
				case EdmExpressionKind.AssertType:
				{
					this.ProcessAssertTypeExpression((IEdmAssertTypeExpression)expression);
					return;
				}
				case EdmExpressionKind.IsType:
				{
					this.ProcessIsTypeExpression((IEdmIsTypeExpression)expression);
					return;
				}
				case EdmExpressionKind.FunctionApplication:
				{
					this.ProcessFunctionApplicationExpression((IEdmApplyExpression)expression);
					return;
				}
				case EdmExpressionKind.LabeledExpressionReference:
				{
					this.ProcessLabeledExpressionReferenceExpression((IEdmLabeledExpressionReferenceExpression)expression);
					return;
				}
				case EdmExpressionKind.Labeled:
				{
					this.ProcessLabeledExpression((IEdmLabeledExpression)expression);
					return;
				}
			}
			throw new InvalidOperationException(Strings.UnknownEnumVal_ExpressionKind(expression.ExpressionKind));
		}

		public void VisitExpressions(IEnumerable<IEdmExpression> expressions)
		{
			EdmModelVisitor.VisitCollection<IEdmExpression>(expressions, new Action<IEdmExpression>(this.VisitExpression));
		}

		public void VisitFunctionParameters(IEnumerable<IEdmFunctionParameter> parameters)
		{
			EdmModelVisitor edmModelVisitor = this;
			EdmModelVisitor.VisitCollection<IEdmFunctionParameter>(parameters, new Action<IEdmFunctionParameter>(edmModelVisitor.ProcessFunctionParameter));
		}

		public void VisitPrimitiveTypeReference(IEdmPrimitiveTypeReference reference)
		{
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = reference.PrimitiveKind();
			switch (edmPrimitiveTypeKind)
			{
				case EdmPrimitiveTypeKind.None:
				case EdmPrimitiveTypeKind.Boolean:
				case EdmPrimitiveTypeKind.Byte:
				case EdmPrimitiveTypeKind.Double:
				case EdmPrimitiveTypeKind.Guid:
				case EdmPrimitiveTypeKind.Int16:
				case EdmPrimitiveTypeKind.Int32:
				case EdmPrimitiveTypeKind.Int64:
				case EdmPrimitiveTypeKind.SByte:
				case EdmPrimitiveTypeKind.Single:
				case EdmPrimitiveTypeKind.Stream:
				{
					this.ProcessPrimitiveTypeReference(reference);
					return;
				}
				case EdmPrimitiveTypeKind.Binary:
				{
					this.ProcessBinaryTypeReference(reference.AsBinary());
					return;
				}
				case EdmPrimitiveTypeKind.DateTime:
				case EdmPrimitiveTypeKind.DateTimeOffset:
				case EdmPrimitiveTypeKind.Time:
				{
					this.ProcessTemporalTypeReference(reference.AsTemporal());
					return;
				}
				case EdmPrimitiveTypeKind.Decimal:
				{
					this.ProcessDecimalTypeReference(reference.AsDecimal());
					return;
				}
				case EdmPrimitiveTypeKind.String:
				{
					this.ProcessStringTypeReference(reference.AsString());
					return;
				}
				case EdmPrimitiveTypeKind.Geography:
				case EdmPrimitiveTypeKind.GeographyPoint:
				case EdmPrimitiveTypeKind.GeographyLineString:
				case EdmPrimitiveTypeKind.GeographyPolygon:
				case EdmPrimitiveTypeKind.GeographyCollection:
				case EdmPrimitiveTypeKind.GeographyMultiPolygon:
				case EdmPrimitiveTypeKind.GeographyMultiLineString:
				case EdmPrimitiveTypeKind.GeographyMultiPoint:
				case EdmPrimitiveTypeKind.Geometry:
				case EdmPrimitiveTypeKind.GeometryPoint:
				case EdmPrimitiveTypeKind.GeometryLineString:
				case EdmPrimitiveTypeKind.GeometryPolygon:
				case EdmPrimitiveTypeKind.GeometryCollection:
				case EdmPrimitiveTypeKind.GeometryMultiPolygon:
				case EdmPrimitiveTypeKind.GeometryMultiLineString:
				case EdmPrimitiveTypeKind.GeometryMultiPoint:
				{
					this.ProcessSpatialTypeReference(reference.AsSpatial());
					return;
				}
			}
			throw new InvalidOperationException(Strings.UnknownEnumVal_PrimitiveKind(reference.PrimitiveKind().ToString()));
		}

		public void VisitProperties(IEnumerable<IEdmProperty> properties)
		{
			EdmModelVisitor.VisitCollection<IEdmProperty>(properties, new Action<IEdmProperty>(this.VisitProperty));
		}

		public void VisitProperty(IEdmProperty property)
		{
			EdmPropertyKind propertyKind = property.PropertyKind;
			switch (propertyKind)
			{
				case EdmPropertyKind.Structural:
				{
					this.ProcessStructuralProperty((IEdmStructuralProperty)property);
					return;
				}
				case EdmPropertyKind.Navigation:
				{
					this.ProcessNavigationProperty((IEdmNavigationProperty)property);
					return;
				}
				case EdmPropertyKind.None:
				{
					this.ProcessProperty(property);
					return;
				}
			}
			throw new InvalidOperationException(Strings.UnknownEnumVal_PropertyKind(property.PropertyKind.ToString()));
		}

		public void VisitPropertyConstructors(IEnumerable<IEdmPropertyConstructor> constructor)
		{
			EdmModelVisitor edmModelVisitor = this;
			EdmModelVisitor.VisitCollection<IEdmPropertyConstructor>(constructor, new Action<IEdmPropertyConstructor>(edmModelVisitor.ProcessPropertyConstructor));
		}

		public void VisitPropertyValueBindings(IEnumerable<IEdmPropertyValueBinding> bindings)
		{
			EdmModelVisitor edmModelVisitor = this;
			EdmModelVisitor.VisitCollection<IEdmPropertyValueBinding>(bindings, new Action<IEdmPropertyValueBinding>(edmModelVisitor.ProcessPropertyValueBinding));
		}

		public void VisitSchemaElement(IEdmSchemaElement element)
		{
			EdmSchemaElementKind schemaElementKind = element.SchemaElementKind;
			switch (schemaElementKind)
			{
				case EdmSchemaElementKind.None:
				{
					this.ProcessSchemaElement(element);
					return;
				}
				case EdmSchemaElementKind.TypeDefinition:
				{
					this.VisitSchemaType((IEdmType)element);
					return;
				}
				case EdmSchemaElementKind.Function:
				{
					this.ProcessFunction((IEdmFunction)element);
					return;
				}
				case EdmSchemaElementKind.ValueTerm:
				{
					this.ProcessValueTerm((IEdmValueTerm)element);
					return;
				}
				case EdmSchemaElementKind.EntityContainer:
				{
					this.ProcessEntityContainer((IEdmEntityContainer)element);
					return;
				}
			}
			throw new InvalidOperationException(Strings.UnknownEnumVal_SchemaElementKind(element.SchemaElementKind));
		}

		public void VisitSchemaElements(IEnumerable<IEdmSchemaElement> elements)
		{
			EdmModelVisitor.VisitCollection<IEdmSchemaElement>(elements, new Action<IEdmSchemaElement>(this.VisitSchemaElement));
		}

		public void VisitSchemaType(IEdmType definition)
		{
			EdmTypeKind typeKind = definition.TypeKind;
			switch (typeKind)
			{
				case EdmTypeKind.None:
				{
					this.VisitSchemaType(definition);
					return;
				}
				case EdmTypeKind.Primitive:
				{
					throw new InvalidOperationException(Strings.UnknownEnumVal_TypeKind(definition.TypeKind));
				}
				case EdmTypeKind.Entity:
				{
					this.ProcessEntityType((IEdmEntityType)definition);
					return;
				}
				case EdmTypeKind.Complex:
				{
					this.ProcessComplexType((IEdmComplexType)definition);
					return;
				}
				default:
				{
					if (typeKind == EdmTypeKind.Enum)
					{
						this.ProcessEnumType((IEdmEnumType)definition);
						return;
					}
					else
					{
						throw new InvalidOperationException(Strings.UnknownEnumVal_TypeKind((object)definition.TypeKind));
					}
				}
			}
		}

		public void VisitTypeReference(IEdmTypeReference reference)
		{
			EdmTypeKind edmTypeKind = reference.TypeKind();
			switch (edmTypeKind)
			{
				case EdmTypeKind.None:
				{
					this.ProcessTypeReference(reference);
					return;
				}
				case EdmTypeKind.Primitive:
				{
					this.VisitPrimitiveTypeReference(reference.AsPrimitive());
					return;
				}
				case EdmTypeKind.Entity:
				{
					this.ProcessEntityTypeReference(reference.AsEntity());
					return;
				}
				case EdmTypeKind.Complex:
				{
					this.ProcessComplexTypeReference(reference.AsComplex());
					return;
				}
				case EdmTypeKind.Row:
				{
					this.ProcessRowTypeReference(reference.AsRow());
					return;
				}
				case EdmTypeKind.Collection:
				{
					this.ProcessCollectionTypeReference(reference.AsCollection());
					return;
				}
				case EdmTypeKind.EntityReference:
				{
					this.ProcessEntityReferenceTypeReference(reference.AsEntityReference());
					return;
				}
				case EdmTypeKind.Enum:
				{
					this.ProcessEnumTypeReference(reference.AsEnum());
					return;
				}
			}
			throw new InvalidOperationException(Strings.UnknownEnumVal_TypeKind(reference.TypeKind().ToString()));
		}

		public void VisitVocabularyAnnotation(IEdmVocabularyAnnotation annotation)
		{
			if (annotation.Term == null)
			{
				this.ProcessVocabularyAnnotation(annotation);
				return;
			}
			else
			{
				EdmTermKind termKind = annotation.Term.TermKind;
				switch (termKind)
				{
					case EdmTermKind.None:
					{
						this.ProcessVocabularyAnnotation(annotation);
						return;
					}
					case EdmTermKind.Type:
					{
						this.ProcessTypeAnnotation((IEdmTypeAnnotation)annotation);
						return;
					}
					case EdmTermKind.Value:
					{
						this.ProcessValueAnnotation((IEdmValueAnnotation)annotation);
						return;
					}
				}
				throw new InvalidOperationException(Strings.UnknownEnumVal_TermKind(annotation.Term.TermKind));
			}
		}

		public void VisitVocabularyAnnotations(IEnumerable<IEdmVocabularyAnnotation> annotations)
		{
			EdmModelVisitor.VisitCollection<IEdmVocabularyAnnotation>(annotations, new Action<IEdmVocabularyAnnotation>(this.VisitVocabularyAnnotation));
		}
	}
}