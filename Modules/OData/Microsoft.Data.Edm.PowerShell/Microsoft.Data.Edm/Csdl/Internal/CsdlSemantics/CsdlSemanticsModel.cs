namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	using Microsoft.Data.Edm;
	using Microsoft.Data.Edm.Annotations;
	using Microsoft.Data.Edm.Csdl;
	using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
	using Microsoft.Data.Edm.Expressions;
	using Microsoft.Data.Edm.Internal;
	using Microsoft.Data.Edm.Library;
	using Microsoft.Data.Edm.Library.Annotations;
	using Microsoft.Data.Edm.Validation;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Threading;
	
	internal class CsdlSemanticsModel : EdmModelBase, IEdmCheckable
	{
		private readonly Dictionary<string, IEdmAssociation> associationDictionary;
		private readonly CsdlModel astModel;
		private readonly Dictionary<string, List<IEdmStructuredType>> derivedTypeMappings;
		private readonly Dictionary<string, List<CsdlSemanticsAnnotations>> outOfLineAnnotations;
		private readonly List<CsdlSemanticsSchema> schemata;
		private readonly Dictionary<CsdlVocabularyAnnotationBase, CsdlSemanticsVocabularyAnnotation> wrappedAnnotations;
		
		public CsdlSemanticsModel(CsdlModel astModel, EdmDirectValueAnnotationsManager annotationsManager, IEnumerable<IEdmModel> referencedModels) : base(referencedModels, annotationsManager)
		{
			this.schemata = new List<CsdlSemanticsSchema>();
			this.outOfLineAnnotations = new Dictionary<string, List<CsdlSemanticsAnnotations>>();
			this.wrappedAnnotations = new Dictionary<CsdlVocabularyAnnotationBase, CsdlSemanticsVocabularyAnnotation>();
			this.associationDictionary = new Dictionary<string, IEdmAssociation>();
			this.derivedTypeMappings = new Dictionary<string, List<IEdmStructuredType>>();
			this.astModel = astModel;
			foreach (CsdlSchema schema in this.astModel.Schemata)
			{
				this.AddSchema(schema);
			}
		}
		
		private void AddSchema(CsdlSchema schema)
		{
			CsdlSemanticsSchema item = new CsdlSemanticsSchema(this, schema);
			this.schemata.Add(item);
			foreach (IEdmSchemaType type in item.Types)
			{
				CsdlSemanticsStructuredTypeDefinition definition = type as CsdlSemanticsStructuredTypeDefinition;
				if (definition != null)
				{
					string baseTypeName = ((CsdlNamedStructuredType) definition.Element).BaseTypeName;
					if (baseTypeName != null)
					{
						string str;
						string str2;
						EdmUtil.TryGetNamespaceNameFromQualifiedName(baseTypeName, out str, out str2);
						if (str2 != null)
						{
							List<IEdmStructuredType> list;
							if (!this.derivedTypeMappings.TryGetValue(str2, out list))
							{
								list = new List<IEdmStructuredType>();
								this.derivedTypeMappings[str2] = list;
							}
							list.Add(definition);
						}
					}
				}
				base.RegisterElement(type);
			}
			foreach (CsdlSemanticsAssociation association in item.Associations)
			{
				RegistrationHelper.AddElement<IEdmAssociation>(association, association.Namespace + "." + association.Name, this.associationDictionary, new Func<IEdmAssociation, IEdmAssociation, IEdmAssociation>(CsdlSemanticsModel.CreateAmbiguousAssociationBinding));
			}
			foreach (IEdmFunction function in item.Functions)
			{
				base.RegisterElement(function);
			}
			foreach (IEdmValueTerm term in item.ValueTerms)
			{
				base.RegisterElement(term);
			}
			foreach (IEdmEntityContainer container in item.EntityContainers)
			{
				base.RegisterElement(container);
			}
			foreach (CsdlAnnotations annotations in schema.OutOfLineAnnotations)
			{
				List<CsdlSemanticsAnnotations> list2;
				string target = annotations.Target;
				string str5 = item.ReplaceAlias(target);
				if (str5 != null)
				{
					target = str5;
				}
				if (!this.outOfLineAnnotations.TryGetValue(target, out list2))
				{
					list2 = new List<CsdlSemanticsAnnotations>();
					this.outOfLineAnnotations[target] = list2;
				}
				list2.Add(new CsdlSemanticsAnnotations(item, annotations));
			}
			foreach (CsdlUsing @using in schema.Usings)
			{
				this.SetNamespaceAlias(@using.Namespace, @using.Alias);
			}
			Version edmVersion = this.GetEdmVersion();
			if ((edmVersion == null) || (edmVersion < schema.Version))
			{
				this.SetEdmVersion(schema.Version);
			}
		}
		
		internal static IEdmAssociation CreateAmbiguousAssociationBinding(IEdmAssociation first, IEdmAssociation second)
		{
			AmbiguousAssociationBinding binding = first as AmbiguousAssociationBinding;
			if (binding != null)
			{
				binding.AddBinding(second);
				return binding;
			}
			return new AmbiguousAssociationBinding(first, second);
		}
		
		public IEdmAssociation FindAssociation(string qualifiedName)
		{
			IEdmAssociation association;
			this.associationDictionary.TryGetValue(qualifiedName, out association);
			return association;
		}
		
		public override IEnumerable<IEdmVocabularyAnnotation> FindDeclaredVocabularyAnnotations(IEdmVocabularyAnnotatable element)
		{
			List<CsdlSemanticsAnnotations> list;
			CsdlSemanticsElement element2 = element as CsdlSemanticsElement;
			IEnumerable<IEdmVocabularyAnnotation> first = ((element2 != null) && (element2.Model == this)) ? element2.InlineVocabularyAnnotations : Enumerable.Empty<IEdmVocabularyAnnotation>();
			string key = EdmUtil.FullyQualifiedName(element);
			if ((key == null) || !this.outOfLineAnnotations.TryGetValue(key, out list))
			{
				return first;
			}
			List<IEdmVocabularyAnnotation> second = new List<IEdmVocabularyAnnotation>();
			foreach (CsdlSemanticsAnnotations annotations in list)
			{
				foreach (CsdlVocabularyAnnotationBase base2 in annotations.Annotations.Annotations)
				{
					second.Add(this.WrapVocabularyAnnotation(base2, annotations.Context, null, annotations, annotations.Annotations.Qualifier));
				}
			}
			return first.Concat<IEdmVocabularyAnnotation>(second);
		}
		
		public override IEnumerable<IEdmStructuredType> FindDirectlyDerivedTypes(IEdmStructuredType baseType)
		{
			List<IEdmStructuredType> list;
			Func<IEdmStructuredType, bool> predicate = null;
			if (!this.derivedTypeMappings.TryGetValue(((IEdmSchemaType) baseType).Name, out list))
			{
				return Enumerable.Empty<IEdmStructuredType>();
			}
			if (predicate == null)
			{
				predicate = t => t.BaseType == baseType;
			}
			return list.Where<IEdmStructuredType>(predicate);
		}
		
		internal static IEdmExpression WrapExpression(CsdlExpressionBase expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema)
		{
			if (expression != null)
			{
				switch (expression.ExpressionKind)
				{
				case EdmExpressionKind.BinaryConstant:
					return new CsdlSemanticsBinaryConstantExpression((CsdlConstantExpression) expression, schema);
					
				case EdmExpressionKind.BooleanConstant:
					return new CsdlSemanticsBooleanConstantExpression((CsdlConstantExpression) expression, schema);
					
				case EdmExpressionKind.DateTimeConstant:
					return new CsdlSemanticsDateTimeConstantExpression((CsdlConstantExpression) expression, schema);
					
				case EdmExpressionKind.DateTimeOffsetConstant:
					return new CsdlSemanticsDateTimeOffsetConstantExpression((CsdlConstantExpression) expression, schema);
					
				case EdmExpressionKind.DecimalConstant:
					return new CsdlSemanticsDecimalConstantExpression((CsdlConstantExpression) expression, schema);
					
				case EdmExpressionKind.FloatingConstant:
					return new CsdlSemanticsFloatingConstantExpression((CsdlConstantExpression) expression, schema);
					
				case EdmExpressionKind.GuidConstant:
					return new CsdlSemanticsGuidConstantExpression((CsdlConstantExpression) expression, schema);
					
				case EdmExpressionKind.IntegerConstant:
					return new CsdlSemanticsIntConstantExpression((CsdlConstantExpression) expression, schema);
					
				case EdmExpressionKind.StringConstant:
					return new CsdlSemanticsStringConstantExpression((CsdlConstantExpression) expression, schema);
					
				case EdmExpressionKind.TimeConstant:
					return new CsdlSemanticsTimeConstantExpression((CsdlConstantExpression) expression, schema);
					
				case EdmExpressionKind.Null:
					return new CsdlSemanticsNullExpression((CsdlConstantExpression) expression, schema);
					
				case EdmExpressionKind.Record:
					return new CsdlSemanticsRecordExpression((CsdlRecordExpression) expression, bindingContext, schema);
					
				case EdmExpressionKind.Collection:
					return new CsdlSemanticsCollectionExpression((CsdlCollectionExpression) expression, bindingContext, schema);
					
				case EdmExpressionKind.Path:
					return new CsdlSemanticsPathExpression((CsdlPathExpression) expression, bindingContext, schema);
					
				case EdmExpressionKind.ParameterReference:
					return new CsdlSemanticsParameterReferenceExpression((CsdlParameterReferenceExpression) expression, bindingContext, schema);
					
				case EdmExpressionKind.FunctionReference:
					return new CsdlSemanticsFunctionReferenceExpression((CsdlFunctionReferenceExpression) expression, bindingContext, schema);
					
				case EdmExpressionKind.PropertyReference:
					return new CsdlSemanticsPropertyReferenceExpression((CsdlPropertyReferenceExpression) expression, bindingContext, schema);
					
				case EdmExpressionKind.EntitySetReference:
					return new CsdlSemanticsEntitySetReferenceExpression((CsdlEntitySetReferenceExpression) expression, bindingContext, schema);
					
				case EdmExpressionKind.EnumMemberReference:
					return new CsdlSemanticsEnumMemberReferenceExpression((CsdlEnumMemberReferenceExpression) expression, bindingContext, schema);
					
				case EdmExpressionKind.If:
					return new CsdlSemanticsIfExpression((CsdlIfExpression) expression, bindingContext, schema);
					
				case EdmExpressionKind.AssertType:
					return new CsdlSemanticsAssertTypeExpression((CsdlAssertTypeExpression) expression, bindingContext, schema);
					
				case EdmExpressionKind.IsType:
					return new CsdlSemanticsIsTypeExpression((CsdlIsTypeExpression) expression, bindingContext, schema);
					
				case EdmExpressionKind.FunctionApplication:
					return new CsdlSemanticsApplyExpression((CsdlApplyExpression) expression, bindingContext, schema);
					
				case EdmExpressionKind.LabeledExpressionReference:
					return new CsdlSemanticsLabeledExpressionReferenceExpression((CsdlLabeledExpressionReferenceExpression) expression, bindingContext, schema);
					
				case EdmExpressionKind.Labeled:
					return schema.WrapLabeledElement((CsdlLabeledExpression) expression, bindingContext);
				}
			}
			return null;
		}
		
		internal IEnumerable<IEdmVocabularyAnnotation> WrapInlineVocabularyAnnotations(CsdlSemanticsElement element, CsdlSemanticsSchema schema)
		{
			IEdmVocabularyAnnotatable targetContext = element as IEdmVocabularyAnnotatable;
			if (targetContext != null)
			{
				IEnumerable<CsdlVocabularyAnnotationBase> vocabularyAnnotations = element.Element.VocabularyAnnotations;
				if (vocabularyAnnotations.FirstOrDefault<CsdlVocabularyAnnotationBase>() != null)
				{
					List<IEdmVocabularyAnnotation> list = new List<IEdmVocabularyAnnotation>();
					foreach (CsdlVocabularyAnnotationBase base2 in vocabularyAnnotations)
					{
						IEdmVocabularyAnnotation annotation = this.WrapVocabularyAnnotation(base2, schema, targetContext, null, base2.Qualifier);
						annotation.SetSerializationLocation(this, 0);
						list.Add(annotation);
					}
					return list;
				}
			}
			return Enumerable.Empty<IEdmVocabularyAnnotation>();
		}
		
		internal static IEdmTypeReference WrapTypeReference(CsdlSemanticsSchema schema, CsdlTypeReference type)
		{
			CsdlNamedTypeReference reference = type as CsdlNamedTypeReference;
			if (reference != null)
			{
				CsdlPrimitiveTypeReference reference2 = reference as CsdlPrimitiveTypeReference;
				if (reference2 != null)
				{
					switch (reference2.Kind)
					{
					case EdmPrimitiveTypeKind.Binary:
						return new CsdlSemanticsBinaryTypeReference(schema, (CsdlBinaryTypeReference) reference2);
						
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
						return new CsdlSemanticsPrimitiveTypeReference(schema, reference2);
						
					case EdmPrimitiveTypeKind.DateTime:
					case EdmPrimitiveTypeKind.DateTimeOffset:
					case EdmPrimitiveTypeKind.Time:
						return new CsdlSemanticsTemporalTypeReference(schema, (CsdlTemporalTypeReference) reference2);
						
					case EdmPrimitiveTypeKind.Decimal:
						return new CsdlSemanticsDecimalTypeReference(schema, (CsdlDecimalTypeReference) reference2);
						
					case EdmPrimitiveTypeKind.String:
						return new CsdlSemanticsStringTypeReference(schema, (CsdlStringTypeReference) reference2);
						
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
						return new CsdlSemanticsSpatialTypeReference(schema, (CsdlSpatialTypeReference) reference2);
					}
				}
				return new CsdlSemanticsNamedTypeReference(schema, reference);
			}
			CsdlExpressionTypeReference expressionUsage = type as CsdlExpressionTypeReference;
			if (expressionUsage != null)
			{
				CsdlRowType typeExpression = expressionUsage.TypeExpression as CsdlRowType;
				if (typeExpression != null)
				{
					return new CsdlSemanticsRowTypeExpression(expressionUsage, new CsdlSemanticsRowTypeDefinition(schema, typeExpression));
				}
				CsdlCollectionType collection = expressionUsage.TypeExpression as CsdlCollectionType;
				if (collection != null)
				{
					return new CsdlSemanticsCollectionTypeExpression(expressionUsage, new CsdlSemanticsCollectionTypeDefinition(schema, collection));
				}
				CsdlEntityReferenceType entityTypeReference = expressionUsage.TypeExpression as CsdlEntityReferenceType;
				if (entityTypeReference != null)
				{
					return new CsdlSemanticsEntityReferenceTypeExpression(expressionUsage, new CsdlSemanticsEntityReferenceTypeDefinition(schema, entityTypeReference));
				}
			}
			return null;
		}
		
		private IEdmVocabularyAnnotation WrapVocabularyAnnotation(CsdlVocabularyAnnotationBase annotation, CsdlSemanticsSchema schema, IEdmVocabularyAnnotatable targetContext, CsdlSemanticsAnnotations annotationsContext, string qualifier)
		{
			CsdlSemanticsVocabularyAnnotation annotation2;
			if (!this.wrappedAnnotations.TryGetValue(annotation, out annotation2))
			{
				CsdlValueAnnotation annotation3 = annotation as CsdlValueAnnotation;
				annotation2 = (annotation3 != null) ? ((CsdlSemanticsVocabularyAnnotation) new CsdlSemanticsValueAnnotation(schema, targetContext, annotationsContext, annotation3, qualifier)) : ((CsdlSemanticsVocabularyAnnotation) new CsdlSemanticsTypeAnnotation(schema, targetContext, annotationsContext, (CsdlTypeAnnotation) annotation, qualifier));
				this.wrappedAnnotations[annotation] = annotation2;
			}
			return annotation2;
		}
		
		public IEnumerable<EdmError> Errors
		{
			get
			{
				List<EdmError> list = new List<EdmError>();
				foreach (IEdmAssociation association in this.associationDictionary.Values)
				{
					string str = association.Namespace + "." + association.Name;
					if (association.IsBad())
					{
						AmbiguousAssociationBinding binding = association as AmbiguousAssociationBinding;
						if (binding != null)
						{
							bool flag = true;
							foreach (IEdmAssociation association2 in binding.Bindings)
							{
								if (flag)
								{
									flag = false;
								}
								else
								{
									list.Add(new EdmError(association2.Location(), EdmErrorCode.AlreadyDefined, Microsoft.Data.Edm.Strings.EdmModel_Validator_Semantic_SchemaElementNameAlreadyDefined(str)));
								}
							}
						}
						else
						{
							list.AddRange(association.Errors());
						}
					}
					else
					{
						if (((base.FindDeclaredType(str) != null) || (base.FindDeclaredValueTerm(str) != null)) || (base.FindDeclaredFunctions(str).Count<IEdmFunction>() != 0))
						{
							list.Add(new EdmError(association.Location(), EdmErrorCode.AlreadyDefined, Microsoft.Data.Edm.Strings.EdmModel_Validator_Semantic_SchemaElementNameAlreadyDefined(str)));
						}
						list.AddRange(association.End1.Errors());
						list.AddRange(association.End2.Errors());
						if (association.ReferentialConstraint != null)
						{
							list.AddRange(association.ReferentialConstraint.Errors());
						}
					}
				}
				foreach (CsdlSemanticsSchema schema in this.schemata)
				{
					list.AddRange(schema.Errors());
				}
				return list;
			}
		}
		
		public override IEnumerable<IEdmSchemaElement> SchemaElements
		{
			get
			{
				foreach (CsdlSemanticsSchema iteratorVariable0 in this.schemata)
				{
					foreach (IEdmSchemaType iteratorVariable1 in iteratorVariable0.Types)
					{
						yield return iteratorVariable1;
					}
					foreach (IEdmSchemaElement iteratorVariable2 in iteratorVariable0.Functions)
					{
						yield return iteratorVariable2;
					}
					foreach (IEdmSchemaElement iteratorVariable3 in iteratorVariable0.ValueTerms)
					{
						yield return iteratorVariable3;
					}
					foreach (IEdmEntityContainer iteratorVariable4 in iteratorVariable0.EntityContainers)
					{
						yield return iteratorVariable4;
					}
				}
			}
		}
		
		public override IEnumerable<IEdmVocabularyAnnotation> VocabularyAnnotations
		{
			get
			{
				List<IEdmVocabularyAnnotation> list = new List<IEdmVocabularyAnnotation>();
				foreach (CsdlSemanticsSchema schema in this.schemata)
				{
					foreach (CsdlAnnotations annotations in ((CsdlSchema) schema.Element).OutOfLineAnnotations)
					{
						CsdlSemanticsAnnotations annotationsContext = new CsdlSemanticsAnnotations(schema, annotations);
						foreach (CsdlVocabularyAnnotationBase base2 in annotations.Annotations)
						{
							IEdmVocabularyAnnotation annotation = this.WrapVocabularyAnnotation(base2, schema, null, annotationsContext, annotations.Qualifier);

							annotation.SetSerializationLocation(this, EdmVocabularyAnnotationSerializationLocation.OutOfLine);
							annotation.SetSchemaNamespace(this, schema.Namespace);
							list.Add(annotation);
						}
					}
				}
				foreach (IEdmSchemaElement element in this.SchemaElements)
				{
					list.AddRange(((CsdlSemanticsElement) element).InlineVocabularyAnnotations);
					CsdlSemanticsStructuredTypeDefinition definition = element as CsdlSemanticsStructuredTypeDefinition;
					if (definition != null)
					{
						foreach (IEdmProperty property in definition.DeclaredProperties)
						{
							list.AddRange(((CsdlSemanticsElement) property).InlineVocabularyAnnotations);
						}
					}
					CsdlSemanticsFunction function = element as CsdlSemanticsFunction;
					if (function != null)
					{
						foreach (IEdmFunctionParameter parameter in function.Parameters)
						{
							list.AddRange(((CsdlSemanticsElement) parameter).InlineVocabularyAnnotations);
						}
					}
					CsdlSemanticsEntityContainer container = element as CsdlSemanticsEntityContainer;
					if (container != null)
					{
						foreach (IEdmEntityContainerElement element2 in container.Elements)
						{
							list.AddRange(((CsdlSemanticsElement) element2).InlineVocabularyAnnotations);
							CsdlSemanticsFunctionImport import = element2 as CsdlSemanticsFunctionImport;
							if (import != null)
							{
								foreach (IEdmFunctionParameter parameter2 in import.Parameters)
								{
									list.AddRange(((CsdlSemanticsElement) parameter2).InlineVocabularyAnnotations);
								}
							}
						}
					}
				}
				return list;
			}
		}

	}
}

