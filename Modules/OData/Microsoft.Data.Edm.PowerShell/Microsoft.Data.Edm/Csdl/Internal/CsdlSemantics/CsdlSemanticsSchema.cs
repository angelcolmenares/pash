namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	using Microsoft.Data.Edm;
	using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
	using Microsoft.Data.Edm.Expressions;
	using Microsoft.Data.Edm.Internal;
	using Microsoft.Data.Edm.Library.Internal;
	using Microsoft.Data.Edm.Validation;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;
	
	internal class CsdlSemanticsSchema : CsdlSemanticsElement, IEdmCheckable
	{
		private readonly Dictionary<List<CsdlLabeledExpression>, IEdmLabeledExpression> ambiguousLabeledExpressions;
		private readonly Cache<CsdlSemanticsSchema, IEnumerable<CsdlSemanticsAssociation>> associationsCache;
		private static readonly Func<CsdlSemanticsSchema, IEnumerable<CsdlSemanticsAssociation>> ComputeAssociationsFunc = me => me.ComputeAssociations();
		private static readonly Func<CsdlSemanticsSchema, IEnumerable<IEdmEntityContainer>> ComputeEntityContainersFunc = me => me.ComputeEntityContainers();
		private static readonly Func<CsdlSemanticsSchema, IEnumerable<IEdmFunction>> ComputeFunctionsFunc = me => me.ComputeFunctions();
		private static readonly Func<CsdlSemanticsSchema, Dictionary<string, object>> ComputeLabeledExpressionsFunc = me => me.ComputeLabeledExpressions();
		private static readonly Func<CsdlSemanticsSchema, IEnumerable<IEdmSchemaType>> ComputeTypesFunc = me => me.ComputeTypes();
		private static readonly Func<CsdlSemanticsSchema, IEnumerable<IEdmValueTerm>> ComputeValueTermsFunc = me => me.ComputeValueTerms();
		private readonly Cache<CsdlSemanticsSchema, IEnumerable<IEdmEntityContainer>> entityContainersCache;
		private readonly Cache<CsdlSemanticsSchema, IEnumerable<IEdmFunction>> functionsCache;
		private readonly Cache<CsdlSemanticsSchema, Dictionary<string, object>> labeledExpressionsCache;
		private readonly CsdlSemanticsModel model;
		private readonly CsdlSchema schema;
		private readonly Dictionary<CsdlLabeledExpression, IEdmLabeledExpression> semanticsLabeledElements;
		private readonly Cache<CsdlSemanticsSchema, IEnumerable<IEdmSchemaType>> typesCache;
		private readonly Cache<CsdlSemanticsSchema, IEnumerable<IEdmValueTerm>> valueTermsCache;
		
		public CsdlSemanticsSchema(CsdlSemanticsModel model, CsdlSchema schema) : base(schema)
		{
			this.typesCache = new Cache<CsdlSemanticsSchema, IEnumerable<IEdmSchemaType>>();
			this.associationsCache = new Cache<CsdlSemanticsSchema, IEnumerable<CsdlSemanticsAssociation>>();
			this.functionsCache = new Cache<CsdlSemanticsSchema, IEnumerable<IEdmFunction>>();
			this.entityContainersCache = new Cache<CsdlSemanticsSchema, IEnumerable<IEdmEntityContainer>>();
			this.valueTermsCache = new Cache<CsdlSemanticsSchema, IEnumerable<IEdmValueTerm>>();
			this.labeledExpressionsCache = new Cache<CsdlSemanticsSchema, Dictionary<string, object>>();
			this.semanticsLabeledElements = new Dictionary<CsdlLabeledExpression, IEdmLabeledExpression>();
			this.ambiguousLabeledExpressions = new Dictionary<List<CsdlLabeledExpression>, IEdmLabeledExpression>();
			this.model = model;
			this.schema = schema;
		}
		
		private static void AddLabeledExpressions(CsdlExpressionBase expression, Dictionary<string, object> result)
		{
			if (expression != null)
			{
				switch (expression.ExpressionKind)
				{
				case EdmExpressionKind.Record:
					foreach (CsdlPropertyValue value2 in ((CsdlRecordExpression) expression).PropertyValues)
					{
						AddLabeledExpressions(value2.Expression, result);
					}
					return;
					
				case EdmExpressionKind.Collection:
					foreach (CsdlExpressionBase base2 in ((CsdlCollectionExpression) expression).ElementValues)
					{
						AddLabeledExpressions(base2, result);
					}
					return;
					
				case EdmExpressionKind.If:
				{
					CsdlIfExpression expression3 = (CsdlIfExpression) expression;
					AddLabeledExpressions(expression3.Test, result);
					AddLabeledExpressions(expression3.IfTrue, result);
					AddLabeledExpressions(expression3.IfFalse, result);
					return;
				}
				case EdmExpressionKind.AssertType:
					AddLabeledExpressions(((CsdlAssertTypeExpression) expression).Operand, result);
					return;
					
				case EdmExpressionKind.IsType:
					AddLabeledExpressions(((CsdlIsTypeExpression) expression).Operand, result);
					return;
					
				case EdmExpressionKind.FunctionApplication:
					foreach (CsdlExpressionBase base3 in ((CsdlApplyExpression) expression).Arguments)
					{
						AddLabeledExpressions(base3, result);
					}
					return;
					
				case EdmExpressionKind.LabeledExpressionReference:
					return;
					
				case EdmExpressionKind.Labeled:
				{
					object obj2;
					CsdlLabeledExpression item = (CsdlLabeledExpression) expression;
					string label = item.Label;
					if (!result.TryGetValue(label, out obj2))
					{
						result[label] = item;
					}
					else
					{
						List<CsdlLabeledExpression> list = obj2 as List<CsdlLabeledExpression>;
						if (list == null)
						{
							list = new List<CsdlLabeledExpression> {
								(CsdlLabeledExpression) obj2
							};
							result[label] = list;
						}
						list.Add(item);
					}
					AddLabeledExpressions(item.Element, result);
					return;
				}
				}
			}
		}
		
		private static void AddLabeledExpressions(IEnumerable<CsdlVocabularyAnnotationBase> annotations, Dictionary<string, object> result)
		{
			foreach (CsdlVocabularyAnnotationBase base2 in annotations)
			{
				CsdlValueAnnotation annotation = base2 as CsdlValueAnnotation;
				if (annotation != null)
				{
					AddLabeledExpressions(annotation.Expression, result);
				}
				else
				{
					CsdlTypeAnnotation annotation2 = base2 as CsdlTypeAnnotation;
					if (annotation2 != null)
					{
						foreach (CsdlPropertyValue value2 in annotation2.Properties)
						{
							AddLabeledExpressions(value2.Expression, result);
						}
					}
				}
			}
		}
		
		private IEnumerable<CsdlSemanticsAssociation> ComputeAssociations()
		{
			List<CsdlSemanticsAssociation> list = new List<CsdlSemanticsAssociation>();
			foreach (CsdlAssociation association in this.schema.Associations)
			{
				list.Add(new CsdlSemanticsAssociation(this, association));
			}
			return list;
		}
		
		private IEnumerable<IEdmEntityContainer> ComputeEntityContainers()
		{
			List<IEdmEntityContainer> list = new List<IEdmEntityContainer>();
			foreach (CsdlEntityContainer container in this.schema.EntityContainers)
			{
				list.Add(new CsdlSemanticsEntityContainer(this, container));
			}
			return list;
		}
		
		private IEnumerable<IEdmFunction> ComputeFunctions()
		{
			List<IEdmFunction> list = new List<IEdmFunction>();
			foreach (CsdlFunction function in this.schema.Functions)
			{
				list.Add(new CsdlSemanticsFunction(this, function));
			}
			return list;
		}
		
		private Dictionary<string, object> ComputeLabeledExpressions()
		{
			Dictionary<string, object> result = new Dictionary<string, object>();
			foreach (CsdlAnnotations annotations in this.schema.OutOfLineAnnotations)
			{
				AddLabeledExpressions(annotations.Annotations, result);
			}
			foreach (CsdlStructuredType type in this.schema.StructuredTypes)
			{
				AddLabeledExpressions(type.VocabularyAnnotations, result);
				foreach (CsdlProperty property in type.Properties)
				{
					AddLabeledExpressions(property.VocabularyAnnotations, result);
				}
			}
			foreach (CsdlFunction function in this.schema.Functions)
			{
				AddLabeledExpressions(function.VocabularyAnnotations, result);
				foreach (CsdlFunctionParameter parameter in function.Parameters)
				{
					AddLabeledExpressions(parameter.VocabularyAnnotations, result);
				}
			}
			foreach (CsdlValueTerm term in this.schema.ValueTerms)
			{
				AddLabeledExpressions(term.VocabularyAnnotations, result);
			}
			foreach (CsdlEntityContainer container in this.schema.EntityContainers)
			{
				AddLabeledExpressions(container.VocabularyAnnotations, result);
				foreach (CsdlEntitySet set in container.EntitySets)
				{
					AddLabeledExpressions(set.VocabularyAnnotations, result);
				}
				foreach (CsdlFunctionImport import in container.FunctionImports)
				{
					AddLabeledExpressions(import.VocabularyAnnotations, result);
					foreach (CsdlFunctionParameter parameter2 in import.Parameters)
					{
						AddLabeledExpressions(parameter2.VocabularyAnnotations, result);
					}
				}
			}
			return result;
		}
		
		private IEnumerable<IEdmSchemaType> ComputeTypes()
		{
			List<IEdmSchemaType> list = new List<IEdmSchemaType>();
			foreach (CsdlStructuredType type in this.schema.StructuredTypes)
			{
				CsdlEntityType entity = type as CsdlEntityType;
				if (entity != null)
				{
					list.Add(new CsdlSemanticsEntityTypeDefinition(this, entity));
				}
				else
				{
					CsdlComplexType complex = type as CsdlComplexType;
					if (complex != null)
					{
						list.Add(new CsdlSemanticsComplexTypeDefinition(this, complex));
					}
				}
			}
			foreach (CsdlEnumType type4 in this.schema.EnumTypes)
			{
				list.Add(new CsdlSemanticsEnumTypeDefinition(this, type4));
			}
			return list;
		}
		
		private IEnumerable<IEdmValueTerm> ComputeValueTerms()
		{
			List<IEdmValueTerm> list = new List<IEdmValueTerm>();
			foreach (CsdlValueTerm term in this.schema.ValueTerms)
			{
				list.Add(new CsdlSemanticsValueTerm(this, term));
			}
			return list;
		}
		
		public IEdmAssociation FindAssociation(string name)
		{
			return this.FindSchemaElement<IEdmAssociation>(name, new Func<IEdmModel, string, IEdmAssociation>(CsdlSemanticsSchema.FindAssociation));
		}
		
		private static IEdmAssociation FindAssociation(IEdmModel model, string name)
		{
			return ((CsdlSemanticsModel) model).FindAssociation(name);
		}
		
		public IEdmEntityContainer FindEntityContainer(string name)
		{
			return this.FindSchemaElement<IEdmEntityContainer>(name, new Func<IEdmModel, string, IEdmEntityContainer>(CsdlSemanticsSchema.FindEntityContainer));
		}
		
		private static IEdmEntityContainer FindEntityContainer(IEdmModel model, string name)
		{
			return model.FindEntityContainer(name);
		}
		
		public IEnumerable<IEdmFunction> FindFunctions(string name)
		{
			return this.FindSchemaElement<IEnumerable<IEdmFunction>>(name, new Func<IEdmModel, string, IEnumerable<IEdmFunction>>(CsdlSemanticsSchema.FindFunctions));
		}
		
		private static IEnumerable<IEdmFunction> FindFunctions(IEdmModel model, string name)
		{
			return model.FindFunctions(name);
		}
		
		public IEdmLabeledExpression FindLabeledElement(string label, IEdmEntityType bindingContext)
		{
			object obj2;
			if (!this.LabeledExpressions.TryGetValue(label, out obj2))
			{
				return null;
			}
			CsdlLabeledExpression labeledElement = obj2 as CsdlLabeledExpression;
			if (labeledElement != null)
			{
				return this.WrapLabeledElement(labeledElement, bindingContext);
			}
			return this.WrapLabeledElementList((List<CsdlLabeledExpression>) obj2, bindingContext);
		}
		
		public T FindSchemaElement<T>(string name, Func<IEdmModel, string, T> modelFinder)
		{
			string str = this.ReplaceAlias(name);
			if (str == null)
			{
				str = name;
			}
			return modelFinder(this.model, str);
		}
		
		public IEdmSchemaType FindType(string name)
		{
			return this.FindSchemaElement<IEdmSchemaType>(name, new Func<IEdmModel, string, IEdmSchemaType>(CsdlSemanticsSchema.FindType));
		}
		
		private static IEdmSchemaType FindType(IEdmModel model, string name)
		{
			return model.FindType(name);
		}
		
		public IEdmValueTerm FindValueTerm(string name)
		{
			return this.FindSchemaElement<IEdmValueTerm>(name, new Func<IEdmModel, string, IEdmValueTerm>(CsdlSemanticsSchema.FindValueTerm));
		}
		
		private static IEdmValueTerm FindValueTerm(IEdmModel model, string name)
		{
			return model.FindValueTerm(name);
		}
		
		public string ReplaceAlias(string name)
		{
			string str = ReplaceAlias(this.Namespace, this.schema.Alias, name);
			if (str == null)
			{
				foreach (CsdlUsing @using in this.schema.Usings)
				{
					str = ReplaceAlias(@using.Namespace, @using.Alias, name);
					if (str != null)
					{
						return str;
					}
				}
			}
			return str;
		}
		
		private static string ReplaceAlias(string namespaceName, string namespaceAlias, string name)
		{
			if (((namespaceAlias == null) || (name.Length <= namespaceAlias.Length)) || (!name.StartsWith(namespaceAlias, StringComparison.Ordinal) || (name[namespaceAlias.Length] != '.')))
			{
				return null;
			}
			return ((namespaceName ?? string.Empty) + name.Substring(namespaceAlias.Length));
		}
		
		public string UnresolvedName(string qualifiedName)
		{
			if (qualifiedName == null)
			{
				return null;
			}
			return (this.ReplaceAlias(qualifiedName) ?? qualifiedName);
		}
		
		public IEdmLabeledExpression WrapLabeledElement(CsdlLabeledExpression labeledElement, IEdmEntityType bindingContext)
		{
			IEdmLabeledExpression expression;
			if (!this.semanticsLabeledElements.TryGetValue(labeledElement, out expression))
			{
				expression = new CsdlSemanticsLabeledExpression(labeledElement.Label, labeledElement.Element, bindingContext, this);
				this.semanticsLabeledElements[labeledElement] = expression;
			}
			return expression;
		}
		
		private IEdmLabeledExpression WrapLabeledElementList(List<CsdlLabeledExpression> labeledExpressions, IEdmEntityType bindingContext)
		{
			IEdmLabeledExpression expression;
			if (!this.ambiguousLabeledExpressions.TryGetValue(labeledExpressions, out expression))
			{
				foreach (CsdlLabeledExpression expression2 in labeledExpressions)
				{
					IEdmLabeledExpression second = this.WrapLabeledElement(expression2, bindingContext);
					expression = (expression == null) ? second : new AmbiguousLabeledExpressionBinding(expression, second);
				}
				this.ambiguousLabeledExpressions[labeledExpressions] = expression;
			}
			return expression;
		}
		
		public IEnumerable<CsdlSemanticsAssociation> Associations
		{
			get
			{
				return this.associationsCache.GetValue(this, ComputeAssociationsFunc, null);
			}
		}
		
		public override CsdlElement Element
		{
			get
			{
				return this.schema;
			}
		}
		
		public IEnumerable<IEdmEntityContainer> EntityContainers
		{
			get
			{
				return this.entityContainersCache.GetValue(this, ComputeEntityContainersFunc, null);
			}
		}
		
		public IEnumerable<EdmError> Errors
		{
			get
			{
				HashSetInternal<string> internal2 = new HashSetInternal<string>();
				if (this.schema.Alias != null)
				{
					internal2.Add(this.schema.Alias);
				}
				foreach (CsdlUsing @using in this.schema.Usings)
				{
					if (!internal2.Add(@using.Alias))
					{
						return new EdmError[] { new EdmError(base.Location, EdmErrorCode.DuplicateAlias, Microsoft.Data.Edm.Strings.CsdlSemantics_DuplicateAlias(this.Namespace, @using.Alias)) };
					}
				}
				return Enumerable.Empty<EdmError>();
			}
		}
		
		public IEnumerable<IEdmFunction> Functions
		{
			get
			{
				return this.functionsCache.GetValue(this, ComputeFunctionsFunc, null);
			}
		}
		
		private Dictionary<string, object> LabeledExpressions
		{
			get
			{
				return this.labeledExpressionsCache.GetValue(this, ComputeLabeledExpressionsFunc, null);
			}
		}
		
		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.model;
			}
		}
		
		public string Namespace
		{
			get
			{
				return this.schema.Namespace;
			}
		}
		
		public IEnumerable<IEdmSchemaType> Types
		{
			get
			{
				return this.typesCache.GetValue(this, ComputeTypesFunc, null);
			}
		}
		
		public IEnumerable<IEdmValueTerm> ValueTerms
		{
			get
			{
				return this.valueTermsCache.GetValue(this, ComputeValueTermsFunc, null);
			}
		}
	}
}

