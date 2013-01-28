namespace Microsoft.Data.Edm.Csdl.Internal.Serialization
{
	using Microsoft.Data.Edm;
	using Microsoft.Data.Edm.Annotations;
	using Microsoft.Data.Edm.Csdl;
	using Microsoft.Data.Edm.Expressions;
	using Microsoft.Data.Edm.Internal;
	using Microsoft.Data.Edm.Values;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml;
	
	internal sealed class EdmModelCsdlSerializationVisitor : EdmModelVisitor
	{
		private readonly Dictionary<string, List<IEdmNavigationProperty>> associations;
		private readonly Dictionary<string, List<TupleInternal<IEdmEntitySet, IEdmNavigationProperty>>> associationSets;
		private readonly Version edmVersion;
		private readonly VersioningDictionary<string, string> namespaceAliasMappings;
		private readonly List<IEdmNavigationProperty> navigationProperties;
		private readonly EdmModelCsdlSchemaWriter schemaWriter;
		
		internal EdmModelCsdlSerializationVisitor(IEdmModel model, XmlWriter xmlWriter, Version edmVersion) : base(model)
		{
			this.navigationProperties = new List<IEdmNavigationProperty>();
			this.associations = new Dictionary<string, List<IEdmNavigationProperty>>();
			this.associationSets = new Dictionary<string, List<TupleInternal<IEdmEntitySet, IEdmNavigationProperty>>>();
			this.edmVersion = edmVersion;
			this.namespaceAliasMappings = model.GetNamespaceAliases();
			this.schemaWriter = new EdmModelCsdlSchemaWriter(model, this.namespaceAliasMappings, xmlWriter, this.edmVersion);
		}
		
		private void BeginElement<TElement>(TElement element, Action<TElement> elementHeaderWriter, params Action<TElement>[] additionalAttributeWriters) where TElement: IEdmElement
		{
			elementHeaderWriter(element);
			if (additionalAttributeWriters != null)
			{
				foreach (Action<TElement> action in additionalAttributeWriters)
				{
					action(element);
				}
			}
			this.VisitAttributeAnnotations(base.Model.DirectValueAnnotations(element));
			IEdmDocumentation documentation = base.Model.GetDocumentation(element);
			if (documentation != null)
			{
				this.ProcessEdmDocumentation(documentation);
			}
		}
		
		private void EndElement(IEdmElement element)
		{
			Func<IEdmVocabularyAnnotation, bool> predicate = null;
			this.VisitPrimitiveElementAnnotations(base.Model.DirectValueAnnotations(element));
			IEdmVocabularyAnnotatable annotatable = element as IEdmVocabularyAnnotatable;
			if (annotatable != null)
			{
				if (predicate == null)
				{
					predicate = a => a.IsInline(base.Model);
				}
				this.VisitElementVocabularyAnnotations(base.Model.FindDeclaredVocabularyAnnotations(annotatable).Where<IEdmVocabularyAnnotation>(predicate));
			}
			this.schemaWriter.WriteEndElement();
		}
		
		private static bool IsInlineExpression(IEdmExpression expression)
		{
			switch (expression.ExpressionKind)
			{
			case EdmExpressionKind.BinaryConstant:
			case EdmExpressionKind.BooleanConstant:
			case EdmExpressionKind.DateTimeConstant:
			case EdmExpressionKind.DateTimeOffsetConstant:
			case EdmExpressionKind.DecimalConstant:
			case EdmExpressionKind.FloatingConstant:
			case EdmExpressionKind.GuidConstant:
			case EdmExpressionKind.IntegerConstant:
			case EdmExpressionKind.StringConstant:
			case EdmExpressionKind.TimeConstant:
			case EdmExpressionKind.Path:
				return true;
			}
			return false;
		}
		
		private static bool IsInlineType(IEdmTypeReference reference)
		{
			return (((reference.Definition is IEdmSchemaElement) || reference.IsEntityReference()) || (reference.IsCollection() && (reference.AsCollection().CollectionDefinition().ElementType.Definition is IEdmSchemaElement)));
		}
		
		private void ProcessAnnotations(IEnumerable<IEdmDirectValueAnnotation> annotations)
		{
			this.VisitAttributeAnnotations(annotations);
			foreach (IEdmDirectValueAnnotation annotation in annotations)
			{
				if ((annotation.NamespaceUri == "http://schemas.microsoft.com/ado/2011/04/edm/documentation") && (annotation.Name == "Documentation"))
				{
					this.ProcessEdmDocumentation((IEdmDocumentation) annotation.Value);
				}
			}
		}
		
		protected override void ProcessAssertTypeExpression(IEdmAssertTypeExpression expression)
		{
			bool inlineType = IsInlineType(expression.Type);
			this.BeginElement<IEdmAssertTypeExpression>(expression, t => this.schemaWriter.WriteAssertTypeExpressionElementHeader(t, inlineType), new Action<IEdmAssertTypeExpression>[] { e => this.ProcessFacets(e.Type, inlineType) });
			if (!inlineType)
			{
				base.VisitTypeReference(expression.Type);
			}
			base.VisitExpression(expression.Operand);
			this.EndElement(expression);
		}
		
		private void ProcessAssociation(IEdmNavigationProperty element)
		{
			IEnumerable<IEdmDirectValueAnnotation> enumerable;
			IEnumerable<IEdmDirectValueAnnotation> enumerable2;
			IEnumerable<IEdmDirectValueAnnotation> enumerable3;
			IEnumerable<IEdmDirectValueAnnotation> enumerable4;
			IEdmNavigationProperty primary = element.GetPrimary();
			IEdmNavigationProperty partner = primary.Partner;
			base.Model.GetAssociationAnnotations(element, out enumerable, out enumerable2, out enumerable3, out enumerable4);
			this.schemaWriter.WriteAssociationElementHeader(primary);
			this.ProcessAnnotations(enumerable);
			this.ProcessAssociationEnd(primary, (primary == element) ? enumerable2 : enumerable3);
			this.ProcessAssociationEnd(partner, (primary == element) ? enumerable3 : enumerable2);
			this.ProcessReferentialConstraint(primary, enumerable4);
			this.VisitPrimitiveElementAnnotations(enumerable);
			this.schemaWriter.WriteEndElement();
		}
		
		private void ProcessAssociationEnd(IEdmNavigationProperty element, IEnumerable<IEdmDirectValueAnnotation> annotations)
		{
			this.schemaWriter.WriteAssociationEndElementHeader(element);
			this.ProcessAnnotations(annotations);
			if (element.OnDelete != EdmOnDeleteAction.None)
			{
				this.schemaWriter.WriteOperationActionElement("OnDelete", element.OnDelete);
			}
			this.VisitPrimitiveElementAnnotations(annotations);
			this.schemaWriter.WriteEndElement();
		}
		
		private void ProcessAssociationSet(IEdmEntitySet entitySet, IEdmNavigationProperty property)
		{
			IEnumerable<IEdmDirectValueAnnotation> enumerable;
			IEnumerable<IEdmDirectValueAnnotation> enumerable2;
			IEnumerable<IEdmDirectValueAnnotation> enumerable3;
			base.Model.GetAssociationSetAnnotations(entitySet, property, out enumerable, out enumerable2, out enumerable3);
			this.schemaWriter.WriteAssociationSetElementHeader(entitySet, property);
			this.ProcessAnnotations(enumerable);
			this.ProcessAssociationSetEnd(entitySet, property, enumerable2);
			IEdmEntitySet set = entitySet.FindNavigationTarget(property);
			if (set != null)
			{
				this.ProcessAssociationSetEnd(set, property.Partner, enumerable3);
			}
			this.VisitPrimitiveElementAnnotations(enumerable);
			this.schemaWriter.WriteEndElement();
		}
		
		private void ProcessAssociationSetEnd(IEdmEntitySet entitySet, IEdmNavigationProperty property, IEnumerable<IEdmDirectValueAnnotation> annotations)
		{
			this.schemaWriter.WriteAssociationSetEndElementHeader(entitySet, property);
			this.ProcessAnnotations(annotations);
			this.VisitPrimitiveElementAnnotations(annotations);
			this.schemaWriter.WriteEndElement();
		}
		
		private void ProcessAttributeAnnotation(IEdmDirectValueAnnotation annotation)
		{
			this.schemaWriter.WriteAnnotationStringAttribute(annotation);
		}
		
		protected override void ProcessBinaryConstantExpression(IEdmBinaryConstantExpression expression)
		{
			this.schemaWriter.WriteBinaryConstantExpressionElement(expression);
		}
		
		protected override void ProcessBinaryTypeReference(IEdmBinaryTypeReference element)
		{
			this.schemaWriter.WriteBinaryTypeAttributes(element);
		}
		
		protected override void ProcessBooleanConstantExpression(IEdmBooleanConstantExpression expression)
		{
			this.schemaWriter.WriteBooleanConstantExpressionElement(expression);
		}
		
		protected override void ProcessCollectionExpression(IEdmCollectionExpression expression)
		{
			this.BeginElement<IEdmCollectionExpression>(expression, new Action<IEdmCollectionExpression>(this.schemaWriter.WriteCollectionExpressionElementHeader), new Action<IEdmCollectionExpression>[0]);
			base.VisitExpressions(expression.Elements);
			this.EndElement(expression);
		}
		
		protected override void ProcessCollectionType(IEdmCollectionType element)
		{
			bool inlineType = IsInlineType(element.ElementType);
			this.BeginElement<IEdmCollectionType>(element, t => this.schemaWriter.WriteCollectionTypeElementHeader(t, inlineType), new Action<IEdmCollectionType>[] { e => this.ProcessFacets(e.ElementType, inlineType) });
			if (!inlineType)
			{
				base.VisitTypeReference(element.ElementType);
			}
			this.EndElement(element);
		}
		
		protected override void ProcessComplexType(IEdmComplexType element)
		{
			this.BeginElement<IEdmComplexType>(element, new Action<IEdmComplexType>(this.schemaWriter.WriteComplexTypeElementHeader), new Action<IEdmComplexType>[0]);
			base.ProcessComplexType(element);
			this.EndElement(element);
		}
		
		protected override void ProcessDateTimeConstantExpression(IEdmDateTimeConstantExpression expression)
		{
			this.schemaWriter.WriteDateTimeConstantExpressionElement(expression);
		}
		
		protected override void ProcessDateTimeOffsetConstantExpression(IEdmDateTimeOffsetConstantExpression expression)
		{
			this.schemaWriter.WriteDateTimeOffsetConstantExpressionElement(expression);
		}
		
		protected override void ProcessDecimalConstantExpression(IEdmDecimalConstantExpression expression)
		{
			this.schemaWriter.WriteDecimalConstantExpressionElement(expression);
		}
		
		protected override void ProcessDecimalTypeReference(IEdmDecimalTypeReference element)
		{
			this.schemaWriter.WriteDecimalTypeAttributes(element);
		}
		
		private void ProcessEdmDocumentation(IEdmDocumentation element)
		{
			this.schemaWriter.WriteDocumentationElement(element);
		}
		
		private void ProcessElementAnnotation(IEdmDirectValueAnnotation annotation)
		{
			this.schemaWriter.WriteAnnotationStringElement(annotation);
		}
		
		protected override void ProcessEntityContainer(IEdmEntityContainer element)
		{
			this.BeginElement<IEdmEntityContainer>(element, new Action<IEdmEntityContainer>(this.schemaWriter.WriteEntityContainerElementHeader), new Action<IEdmEntityContainer>[0]);
			base.ProcessEntityContainer(element);
			using (IEnumerator<IEdmEntitySet> enumerator = element.EntitySets().GetEnumerator())
			{
				IEdmEntitySet entitySet;
				while (enumerator.MoveNext())
				{
					entitySet = enumerator.Current;
					using (IEnumerator<IEdmNavigationTargetMapping> enumerator2 = entitySet.NavigationTargets.GetEnumerator())
					{
						Func<TupleInternal<IEdmEntitySet, IEdmNavigationProperty>, bool> predicate = null;
						IEdmNavigationTargetMapping mapping;
						while (enumerator2.MoveNext())
						{
							List<TupleInternal<IEdmEntitySet, IEdmNavigationProperty>> list;
							mapping = enumerator2.Current;
							string associationFullName = base.Model.GetAssociationFullName(mapping.NavigationProperty);
							if (!this.associationSets.TryGetValue(associationFullName, out list))
							{
								list = new List<TupleInternal<IEdmEntitySet, IEdmNavigationProperty>>();
								this.associationSets[associationFullName] = list;
							}
							if (predicate == null)
							{
								predicate = set => this.SharesAssociationSet(set.Item1, set.Item2, entitySet, mapping.NavigationProperty);
							}
							if (!list.Any<TupleInternal<IEdmEntitySet, IEdmNavigationProperty>>(predicate))
							{
								list.Add(new TupleInternal<IEdmEntitySet, IEdmNavigationProperty>(entitySet, mapping.NavigationProperty));
								list.Add(new TupleInternal<IEdmEntitySet, IEdmNavigationProperty>(mapping.TargetEntitySet, mapping.NavigationProperty.Partner));
								this.ProcessAssociationSet(entitySet, mapping.NavigationProperty);
							}
						}
						continue;
					}
				}
			}
			this.associationSets.Clear();
			this.EndElement(element);
		}
		
		protected override void ProcessEntitySet(IEdmEntitySet element)
		{
			this.BeginElement<IEdmEntitySet>(element, new Action<IEdmEntitySet>(this.schemaWriter.WriteEntitySetElementHeader), new Action<IEdmEntitySet>[0]);
			base.ProcessEntitySet(element);
			this.EndElement(element);
		}
		
		protected override void ProcessEntitySetReferenceExpression(IEdmEntitySetReferenceExpression expression)
		{
			this.schemaWriter.WriteEntitySetReferenceExpressionElement(expression);
		}
		
		protected override void ProcessEntityType(IEdmEntityType element)
		{
			this.BeginElement<IEdmEntityType>(element, new Action<IEdmEntityType>(this.schemaWriter.WriteEntityTypeElementHeader), new Action<IEdmEntityType>[0]);
			if (((element.DeclaredKey != null) && (element.DeclaredKey.Count<IEdmStructuralProperty>() > 0)) && (element.BaseType == null))
			{
				this.VisitEntityTypeDeclaredKey(element.DeclaredKey);
			}
			base.VisitProperties(element.DeclaredStructuralProperties().Cast<IEdmProperty>());
			base.VisitProperties(element.DeclaredNavigationProperties().Cast<IEdmProperty>());
			this.EndElement(element);
		}
		
		protected override void ProcessEnumMember(IEdmEnumMember element)
		{
			this.BeginElement<IEdmEnumMember>(element, new Action<IEdmEnumMember>(this.schemaWriter.WriteEnumMemberElementHeader), new Action<IEdmEnumMember>[0]);
			this.EndElement(element);
		}
		
		protected override void ProcessEnumMemberReferenceExpression(IEdmEnumMemberReferenceExpression expression)
		{
			this.schemaWriter.WriteEnumMemberReferenceExpressionElement(expression);
		}
		
		protected override void ProcessEnumType(IEdmEnumType element)
		{
			this.BeginElement<IEdmEnumType>(element, new Action<IEdmEnumType>(this.schemaWriter.WriteEnumTypeElementHeader), new Action<IEdmEnumType>[0]);
			base.ProcessEnumType(element);
			this.EndElement(element);
		}
		
		private void ProcessFacets(IEdmTypeReference element, bool inlineType)
		{
			if (((element != null) && !element.IsEntityReference()) && inlineType)
			{
				if (element.TypeKind() == EdmTypeKind.Collection)
				{
					IEdmCollectionTypeReference type = element.AsCollection();
					this.schemaWriter.WriteNullableAttribute(type.CollectionDefinition().ElementType);
					base.VisitTypeReference(type.CollectionDefinition().ElementType);
				}
				else
				{
					this.schemaWriter.WriteNullableAttribute(element);
					base.VisitTypeReference(element);
				}
			}
		}
		
		protected override void ProcessFloatingConstantExpression(IEdmFloatingConstantExpression expression)
		{
			this.schemaWriter.WriteFloatingConstantExpressionElement(expression);
		}
		
		protected override void ProcessFunction(IEdmFunction element)
		{
			Action<IEdmFunction> elementHeaderWriter = null;
			if (element.ReturnType != null)
			{
				bool inlineReturnType = IsInlineType(element.ReturnType);
				this.BeginElement<IEdmFunction>(element, f => this.schemaWriter.WriteFunctionElementHeader(f, inlineReturnType), new Action<IEdmFunction>[] { f => this.ProcessFacets(f.ReturnType, inlineReturnType) });
				if (!inlineReturnType)
				{
					this.schemaWriter.WriteReturnTypeElementHeader();
					base.VisitTypeReference(element.ReturnType);
					this.schemaWriter.WriteEndElement();
				}
			}
			else
			{
				if (elementHeaderWriter == null)
				{
					elementHeaderWriter = t => this.schemaWriter.WriteFunctionElementHeader(t, false);
				}
				this.BeginElement<IEdmFunction>(element, elementHeaderWriter, new Action<IEdmFunction>[0]);
			}
			if (element.DefiningExpression != null)
			{
				this.schemaWriter.WriteDefiningExpressionElement(element.DefiningExpression);
			}
			base.VisitFunctionParameters(element.Parameters);
			this.EndElement(element);
		}
		
		protected override void ProcessFunctionApplicationExpression(IEdmApplyExpression expression)
		{
			bool isFunction = expression.AppliedFunction.ExpressionKind == EdmExpressionKind.FunctionReference;
			this.BeginElement<IEdmApplyExpression>(expression, e => this.schemaWriter.WriteFunctionApplicationElementHeader(e, isFunction), new Action<IEdmApplyExpression>[0]);
			if (!isFunction)
			{
				base.VisitExpression(expression.AppliedFunction);
			}
			base.VisitExpressions(expression.Arguments);
			this.EndElement(expression);
		}
		
		protected override void ProcessFunctionImport(IEdmFunctionImport functionImport)
		{
			if ((functionImport.ReturnType != null) && !IsInlineType(functionImport.ReturnType))
			{
				throw new InvalidOperationException(Microsoft.Data.Edm.Strings.Serializer_NonInlineFunctionImportReturnType(functionImport.Container.FullName() + "/" + functionImport.Name));
			}
			this.BeginElement<IEdmFunctionImport>(functionImport, new Action<IEdmFunctionImport>(this.schemaWriter.WriteFunctionImportElementHeader), new Action<IEdmFunctionImport>[0]);
			base.VisitFunctionParameters(functionImport.Parameters);
			this.EndElement(functionImport);
		}
		
		protected override void ProcessFunctionParameter(IEdmFunctionParameter element)
		{
			bool inlineType = IsInlineType(element.Type);
			this.BeginElement<IEdmFunctionParameter>(element, t => this.schemaWriter.WriteFunctionParameterElementHeader(t, inlineType), new Action<IEdmFunctionParameter>[] { e => this.ProcessFacets(e.Type, inlineType) });
			if (!inlineType)
			{
				base.VisitTypeReference(element.Type);
			}
			this.EndElement(element);
		}
		
		protected override void ProcessFunctionReferenceExpression(IEdmFunctionReferenceExpression expression)
		{
			this.schemaWriter.WriteFunctionReferenceExpressionElement(expression);
		}
		
		protected override void ProcessGuidConstantExpression(IEdmGuidConstantExpression expression)
		{
			this.schemaWriter.WriteGuidConstantExpressionElement(expression);
		}
		
		protected override void ProcessIfExpression(IEdmIfExpression expression)
		{
			this.BeginElement<IEdmIfExpression>(expression, new Action<IEdmIfExpression>(this.schemaWriter.WriteIfExpressionElementHeader), new Action<IEdmIfExpression>[0]);
			base.ProcessIfExpression(expression);
			this.EndElement(expression);
		}
		
		protected override void ProcessIntegerConstantExpression(IEdmIntegerConstantExpression expression)
		{
			this.schemaWriter.WriteIntegerConstantExpressionElement(expression);
		}
		
		protected override void ProcessIsTypeExpression(IEdmIsTypeExpression expression)
		{
			bool inlineType = IsInlineType(expression.Type);
			this.BeginElement<IEdmIsTypeExpression>(expression, t => this.schemaWriter.WriteIsTypeExpressionElementHeader(t, inlineType), new Action<IEdmIsTypeExpression>[] { e => this.ProcessFacets(e.Type, inlineType) });
			if (!inlineType)
			{
				base.VisitTypeReference(expression.Type);
			}
			base.VisitExpression(expression.Operand);
			this.EndElement(expression);
		}
		
		protected override void ProcessLabeledExpression(IEdmLabeledExpression element)
		{
			if (element.Name == null)
			{
				base.ProcessLabeledExpression(element);
			}
			else
			{
				this.BeginElement<IEdmLabeledExpression>(element, new Action<IEdmLabeledExpression>(this.schemaWriter.WriteLabeledElementHeader), new Action<IEdmLabeledExpression>[0]);
				base.ProcessLabeledExpression(element);
				this.EndElement(element);
			}
		}
		
		protected override void ProcessNavigationProperty(IEdmNavigationProperty element)
		{
			this.BeginElement<IEdmNavigationProperty>(element, new Action<IEdmNavigationProperty>(this.schemaWriter.WriteNavigationPropertyElementHeader), new Action<IEdmNavigationProperty>[0]);
			this.EndElement(element);
			this.navigationProperties.Add(element);
		}
		
		protected override void ProcessNullConstantExpression(IEdmNullExpression expression)
		{
			this.schemaWriter.WriteNullConstantExpressionElement(expression);
		}
		
		protected override void ProcessParameterReferenceExpression(IEdmParameterReferenceExpression expression)
		{
			this.schemaWriter.WriteParameterReferenceExpressionElement(expression);
		}
		
		protected override void ProcessPathExpression(IEdmPathExpression expression)
		{
			this.schemaWriter.WritePathExpressionElement(expression);
		}
		
		protected override void ProcessPropertyConstructor(IEdmPropertyConstructor constructor)
		{
			bool isInline = IsInlineExpression(constructor.Value);
			this.BeginElement<IEdmPropertyConstructor>(constructor, t => this.schemaWriter.WritePropertyConstructorElementHeader(t, isInline), new Action<IEdmPropertyConstructor>[0]);
			if (!isInline)
			{
				base.ProcessPropertyConstructor(constructor);
			}
			this.EndElement(constructor);
		}
		
		protected override void ProcessPropertyReferenceExpression(IEdmPropertyReferenceExpression expression)
		{
			this.BeginElement<IEdmPropertyReferenceExpression>(expression, new Action<IEdmPropertyReferenceExpression>(this.schemaWriter.WritePropertyReferenceExpressionElementHeader), new Action<IEdmPropertyReferenceExpression>[0]);
			if (expression.Base != null)
			{
				base.VisitExpression(expression.Base);
			}
			this.EndElement(expression);
		}
		
		protected override void ProcessPropertyValueBinding(IEdmPropertyValueBinding binding)
		{
			bool isInline = IsInlineExpression(binding.Value);
			this.BeginElement<IEdmPropertyValueBinding>(binding, t => this.schemaWriter.WritePropertyValueElementHeader(t, isInline), new Action<IEdmPropertyValueBinding>[0]);
			if (!isInline)
			{
				base.ProcessPropertyValueBinding(binding);
			}
			this.EndElement(binding);
		}
		
		protected override void ProcessRecordExpression(IEdmRecordExpression expression)
		{
			this.BeginElement<IEdmRecordExpression>(expression, new Action<IEdmRecordExpression>(this.schemaWriter.WriteRecordExpressionElementHeader), new Action<IEdmRecordExpression>[0]);
			base.VisitPropertyConstructors(expression.Properties);
			this.EndElement(expression);
		}
		
		private void ProcessReferentialConstraint(IEdmNavigationProperty element, IEnumerable<IEdmDirectValueAnnotation> annotations)
		{
			IEdmNavigationProperty partner;
			if (element.DependentProperties != null)
			{
				partner = element.Partner;
			}
			else if (element.Partner.DependentProperties != null)
			{
				partner = element;
			}
			else
			{
				return;
			}
			this.schemaWriter.WriteReferentialConstraintElementHeader(partner);
			this.ProcessAnnotations(annotations);
			this.schemaWriter.WriteReferentialConstraintPrincipalEndElementHeader(partner);
			this.VisitPropertyRefs(((IEdmEntityType) partner.DeclaringType).Key());
			this.schemaWriter.WriteEndElement();
			this.schemaWriter.WriteReferentialConstraintDependentEndElementHeader(partner.Partner);
			this.VisitPropertyRefs(partner.Partner.DependentProperties);
			this.schemaWriter.WriteEndElement();
			this.VisitPrimitiveElementAnnotations(annotations);
			this.schemaWriter.WriteEndElement();
		}
		
		protected override void ProcessRowType(IEdmRowType element)
		{
			this.schemaWriter.WriteRowTypeElementHeader();
			base.ProcessRowType(element);
			this.schemaWriter.WriteEndElement();
		}
		
		protected override void ProcessSpatialTypeReference(IEdmSpatialTypeReference element)
		{
			this.schemaWriter.WriteSpatialTypeAttributes(element);
		}
		
		protected override void ProcessStringConstantExpression(IEdmStringConstantExpression expression)
		{
			this.schemaWriter.WriteStringConstantExpressionElement(expression);
		}
		
		protected override void ProcessStringTypeReference(IEdmStringTypeReference element)
		{
			this.schemaWriter.WriteStringTypeAttributes(element);
		}
		
		protected override void ProcessStructuralProperty(IEdmStructuralProperty element)
		{
			bool inlineType = IsInlineType(element.Type);
			this.BeginElement<IEdmStructuralProperty>(element, t => this.schemaWriter.WriteStructuralPropertyElementHeader(t, inlineType), new Action<IEdmStructuralProperty>[] { e => this.ProcessFacets(e.Type, inlineType) });
			if (!inlineType)
			{
				base.VisitTypeReference(element.Type);
			}
			this.EndElement(element);
		}
		
		protected override void ProcessTemporalTypeReference(IEdmTemporalTypeReference element)
		{
			this.schemaWriter.WriteTemporalTypeAttributes(element);
		}
		
		protected override void ProcessTypeAnnotation(IEdmTypeAnnotation annotation)
		{
			this.BeginElement<IEdmTypeAnnotation>(annotation, new Action<IEdmTypeAnnotation>(this.schemaWriter.WriteTypeAnnotationElementHeader), new Action<IEdmTypeAnnotation>[0]);
			base.ProcessTypeAnnotation(annotation);
			this.EndElement(annotation);
		}
		
		protected override void ProcessValueAnnotation(IEdmValueAnnotation annotation)
		{
			bool isInline = IsInlineExpression(annotation.Value);
			this.BeginElement<IEdmValueAnnotation>(annotation, t => this.schemaWriter.WriteValueAnnotationElementHeader(t, isInline), new Action<IEdmValueAnnotation>[0]);
			if (!isInline)
			{
				base.ProcessValueAnnotation(annotation);
			}
			this.EndElement(annotation);
		}
		
		protected override void ProcessValueTerm(IEdmValueTerm term)
		{
			bool inlineType = (term.Type != null) && IsInlineType(term.Type);
			this.BeginElement<IEdmValueTerm>(term, t => this.schemaWriter.WriteValueTermElementHeader(t, inlineType), new Action<IEdmValueTerm>[] { e => this.ProcessFacets(e.Type, inlineType) });
			if (!inlineType && (term.Type != null))
			{
				base.VisitTypeReference(term.Type);
			}
			this.EndElement(term);
		}
		
		private bool SharesAssociation(IEdmNavigationProperty thisNavprop, IEdmNavigationProperty thatNavprop)
		{
			IEnumerable<IEdmDirectValueAnnotation> enumerable5;
			IEnumerable<IEdmDirectValueAnnotation> enumerable6;
			IEnumerable<IEdmDirectValueAnnotation> enumerable7;
			IEnumerable<IEdmDirectValueAnnotation> enumerable8;
			IEnumerable<IEdmDirectValueAnnotation> enumerable9;
			IEnumerable<IEdmDirectValueAnnotation> enumerable10;
			IEnumerable<IEdmDirectValueAnnotation> enumerable11;
			IEnumerable<IEdmDirectValueAnnotation> enumerable12;
			if (thisNavprop == thatNavprop)
			{
				return true;
			}
			if (base.Model.GetAssociationName(thisNavprop) != base.Model.GetAssociationName(thatNavprop))
			{
				return false;
			}
			IEdmNavigationProperty primary = thisNavprop.GetPrimary();
			IEdmNavigationProperty property2 = thatNavprop.GetPrimary();
			if (!this.SharesEnd(primary, property2))
			{
				return false;
			}
			IEdmNavigationProperty partner = primary.Partner;
			IEdmNavigationProperty property4 = property2.Partner;
			if (!this.SharesEnd(partner, property4))
			{
				return false;
			}
			IEnumerable<IEdmStructuralProperty> theseProperties = ((IEdmEntityType) primary.DeclaringType).Key();
			IEnumerable<IEdmStructuralProperty> thoseProperties = ((IEdmEntityType) property2.DeclaringType).Key();
			if (!this.SharesReferentialConstraintEnd(theseProperties, thoseProperties))
			{
				return false;
			}
			IEnumerable<IEdmStructuralProperty> dependentProperties = partner.DependentProperties;
			IEnumerable<IEdmStructuralProperty> enumerable4 = partner.DependentProperties;
			if (((dependentProperties != null) && (enumerable4 != null)) && !this.SharesReferentialConstraintEnd(dependentProperties, enumerable4))
			{
				return false;
			}
			base.Model.GetAssociationAnnotations(primary, out enumerable5, out enumerable6, out enumerable7, out enumerable8);
			base.Model.GetAssociationAnnotations(property2, out enumerable9, out enumerable10, out enumerable11, out enumerable12);
			return (((enumerable5 == enumerable9) && (enumerable6 == enumerable10)) && ((enumerable7 == enumerable11) && (enumerable8 == enumerable12)));
		}
		
		private bool SharesAssociationSet(IEdmEntitySet thisEntitySet, IEdmNavigationProperty thisNavprop, IEdmEntitySet thatEntitySet, IEdmNavigationProperty thatNavprop)
		{
			IEnumerable<IEdmDirectValueAnnotation> enumerable;
			IEnumerable<IEdmDirectValueAnnotation> enumerable2;
			IEnumerable<IEdmDirectValueAnnotation> enumerable3;
			IEnumerable<IEdmDirectValueAnnotation> enumerable4;
			IEnumerable<IEdmDirectValueAnnotation> enumerable5;
			IEnumerable<IEdmDirectValueAnnotation> enumerable6;
			if ((thisEntitySet == thatEntitySet) && (thisNavprop == thatNavprop))
			{
				return true;
			}
			if ((base.Model.GetAssociationSetName(thisEntitySet, thisNavprop) != base.Model.GetAssociationSetName(thatEntitySet, thatNavprop)) || (base.Model.GetAssociationFullName(thisNavprop) != base.Model.GetAssociationFullName(thatNavprop)))
			{
				return false;
			}
			if ((base.Model.GetAssociationEndName(thisNavprop) != base.Model.GetAssociationEndName(thatNavprop)) || (thisEntitySet.Name != thatEntitySet.Name))
			{
				return false;
			}
			IEdmEntitySet set = thisEntitySet.FindNavigationTarget(thisNavprop);
			IEdmEntitySet set2 = thatEntitySet.FindNavigationTarget(thatNavprop);
			if (set == null)
			{
				if (set2 != null)
				{
					return false;
				}
			}
			else
			{
				if (set2 == null)
				{
					return false;
				}
				if ((base.Model.GetAssociationEndName(thisNavprop.Partner) != base.Model.GetAssociationEndName(thatNavprop.Partner)) || (set.Name != set2.Name))
				{
					return false;
				}
			}
			base.Model.GetAssociationSetAnnotations(thisEntitySet, thisNavprop, out enumerable, out enumerable2, out enumerable3);
			base.Model.GetAssociationSetAnnotations(thatEntitySet, thatNavprop, out enumerable4, out enumerable5, out enumerable6);
			return (((enumerable == enumerable4) && (enumerable2 == enumerable5)) && (enumerable3 == enumerable6));
		}
		
		private bool SharesEnd(IEdmNavigationProperty end1, IEdmNavigationProperty end2)
		{
			return (((((IEdmEntityType) end1.DeclaringType).FullName() == ((IEdmEntityType) end2.DeclaringType).FullName()) && (base.Model.GetAssociationEndName(end1) == base.Model.GetAssociationEndName(end2))) && ((end1.Multiplicity() == end2.Multiplicity()) && (end1.OnDelete == end2.OnDelete)));
		}
		
		private bool SharesReferentialConstraintEnd(IEnumerable<IEdmStructuralProperty> theseProperties, IEnumerable<IEdmStructuralProperty> thoseProperties)
		{
			if (theseProperties.Count<IEdmStructuralProperty>() != thoseProperties.Count<IEdmStructuralProperty>())
			{
				return false;
			}
			IEnumerator<IEdmStructuralProperty> enumerator = theseProperties.GetEnumerator();
			foreach (IEdmStructuralProperty property in thoseProperties)
			{
				enumerator.MoveNext();
				if (!(enumerator.Current.Name == property.Name))
				{
					return false;
				}
			}
			return true;
		}
		
		private void VisitAttributeAnnotations(IEnumerable<IEdmDirectValueAnnotation> annotations)
		{
			foreach (IEdmDirectValueAnnotation annotation in annotations)
			{
				if (annotation.NamespaceUri != "http://schemas.microsoft.com/ado/2011/04/edm/internal")
				{
					IEdmValue value2 = annotation.Value as IEdmValue;
					if (((value2 != null) && !value2.IsSerializedAsElement(base.Model)) && (value2.Type.TypeKind() == EdmTypeKind.Primitive))
					{
						this.ProcessAttributeAnnotation(annotation);
					}
				}
			}
		}
		
		internal void VisitEdmSchema(EdmSchema element, IEnumerable<KeyValuePair<string, string>> mappings)
		{
			string str = null;
			if (this.namespaceAliasMappings != null)
			{
				this.namespaceAliasMappings.TryGetValue(element.Namespace, out str);
			}
			this.schemaWriter.WriteSchemaElementHeader(element, str, mappings);
			foreach (string str2 in element.NamespaceUsings)
			{
				if (((str2 != element.Namespace) && (this.namespaceAliasMappings != null)) && this.namespaceAliasMappings.TryGetValue(str2, out str))
				{
					this.schemaWriter.WriteNamespaceUsingElement(str2, str);
				}
			}
			base.VisitSchemaElements(element.SchemaElements);
			using (List<IEdmNavigationProperty>.Enumerator enumerator2 = element.AssociationNavigationProperties.GetEnumerator())
			{
				Func<IEdmNavigationProperty, bool> predicate = null;
				IEdmNavigationProperty navigationProperty;
				while (enumerator2.MoveNext())
				{
					List<IEdmNavigationProperty> list;
					navigationProperty = enumerator2.Current;
					string associationFullName = base.Model.GetAssociationFullName(navigationProperty);
					if (!this.associations.TryGetValue(associationFullName, out list))
					{
						list = new List<IEdmNavigationProperty>();
						this.associations.Add(associationFullName, list);
					}
					if (predicate == null)
					{
						predicate = np => this.SharesAssociation(np, navigationProperty);
					}
					if (!list.Any<IEdmNavigationProperty>(predicate))
					{
						list.Add(navigationProperty);
						list.Add(navigationProperty.Partner);
						this.ProcessAssociation(navigationProperty);
					}
				}
			}
			EdmModelVisitor.VisitCollection<IEdmEntityContainer>(element.EntityContainers, new Action<IEdmEntityContainer>(this.ProcessEntityContainer));
			foreach (KeyValuePair<string, List<IEdmVocabularyAnnotation>> pair in element.OutOfLineAnnotations)
			{
				this.schemaWriter.WriteAnnotationsElementHeader(pair.Key);
				base.VisitVocabularyAnnotations(pair.Value);
				this.schemaWriter.WriteEndElement();
			}
			this.schemaWriter.WriteEndElement();
		}
		
		private void VisitElementVocabularyAnnotations(IEnumerable<IEdmVocabularyAnnotation> annotations)
		{
			foreach (IEdmVocabularyAnnotation annotation in annotations)
			{
				switch (annotation.Term.TermKind)
				{
				case EdmTermKind.None:
					this.ProcessVocabularyAnnotation(annotation);
					break;
					
				case EdmTermKind.Type:
					this.ProcessTypeAnnotation((IEdmTypeAnnotation) annotation);
					break;
					
				case EdmTermKind.Value:
					this.ProcessValueAnnotation((IEdmValueAnnotation) annotation);
					break;
					
				default:
					throw new InvalidOperationException(Microsoft.Data.Edm.Strings.UnknownEnumVal_TermKind(annotation.Term.TermKind));
				}
			}
		}
		
		private void VisitEntityTypeDeclaredKey(IEnumerable<IEdmStructuralProperty> keyProperties)
		{
			this.schemaWriter.WriteDelaredKeyPropertiesElementHeader();
			this.VisitPropertyRefs(keyProperties);
			this.schemaWriter.WriteEndElement();
		}
		
		private void VisitPrimitiveElementAnnotations(IEnumerable<IEdmDirectValueAnnotation> annotations)
		{
			foreach (IEdmDirectValueAnnotation annotation in annotations)
			{
				if (annotation.NamespaceUri != "http://schemas.microsoft.com/ado/2011/04/edm/internal")
				{
					IEdmValue value2 = annotation.Value as IEdmValue;
					if (((value2 != null) && value2.IsSerializedAsElement(base.Model)) && (value2.Type.TypeKind() == EdmTypeKind.Primitive))
					{
						this.ProcessElementAnnotation(annotation);
					}
				}
			}
		}
		
		private void VisitPropertyRefs(IEnumerable<IEdmStructuralProperty> properties)
		{
			foreach (IEdmStructuralProperty property in properties)
			{
				this.schemaWriter.WritePropertyRefElement(property);
			}
		}
	}
}

