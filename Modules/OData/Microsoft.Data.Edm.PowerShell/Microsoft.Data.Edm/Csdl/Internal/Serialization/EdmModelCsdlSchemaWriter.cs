using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Csdl.Internal;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Microsoft.Data.Edm.Csdl.Internal.Serialization
{
	internal class EdmModelCsdlSchemaWriter
	{
		protected XmlWriter xmlWriter;

		protected Version version;

		private readonly VersioningDictionary<string, string> namespaceAliasMappings;

		private readonly IEdmModel model;

		internal EdmModelCsdlSchemaWriter(IEdmModel model, VersioningDictionary<string, string> namespaceAliasMappings, XmlWriter xmlWriter, Version edmVersion)
		{
			this.xmlWriter = xmlWriter;
			this.version = edmVersion;
			this.model = model;
			this.namespaceAliasMappings = namespaceAliasMappings;
		}

		private static string ConcurrencyModeAsXml(EdmConcurrencyMode mode)
		{
			EdmConcurrencyMode edmConcurrencyMode = mode;
			switch (edmConcurrencyMode)
			{
				case EdmConcurrencyMode.None:
				{
					return "None";
				}
				case EdmConcurrencyMode.Fixed:
				{
					return "Fixed";
				}
			}
			throw new InvalidOperationException(Strings.UnknownEnumVal_ConcurrencyMode(mode.ToString()));
		}

		private static string EntitySetAsXml(IEdmEntitySet set)
		{
			return string.Concat(set.Container.FullName(), "/", set.Name);
		}

		private static string EnumMemberAsXml(IEdmEnumMember member)
		{
			return string.Concat(member.DeclaringType.FullName(), "/", member.Name);
		}

		private string FunctionAsXml(IEdmFunction function)
		{
			return this.SerializationName(function);
		}

		private static string FunctionParameterModeAsXml(EdmFunctionParameterMode mode)
		{
			EdmFunctionParameterMode edmFunctionParameterMode = mode;
			switch (edmFunctionParameterMode)
			{
				case EdmFunctionParameterMode.In:
				{
					return "In";
				}
				case EdmFunctionParameterMode.Out:
				{
					return "Out";
				}
				case EdmFunctionParameterMode.InOut:
				{
					return "InOut";
				}
			}
			throw new InvalidOperationException(Strings.UnknownEnumVal_FunctionParameterMode(mode.ToString()));
		}

		private static string GetCsdlNamespace(Version edmVersion)
		{
			string[] strArrays = null;
			if (!CsdlConstants.SupportedVersions.TryGetValue(edmVersion, out strArrays))
			{
				throw new InvalidOperationException(Strings.Serializer_UnknownEdmVersion);
			}
			else
			{
				return strArrays[0];
			}
		}

		private static string MultiplicityAsXml(EdmMultiplicity endKind)
		{
			EdmMultiplicity edmMultiplicity = endKind;
			switch (edmMultiplicity)
			{
				case EdmMultiplicity.ZeroOrOne:
				{
					return "0..1";
				}
				case EdmMultiplicity.One:
				{
					return "1";
				}
				case EdmMultiplicity.Many:
				{
					return "*";
				}
			}
			throw new InvalidOperationException(Strings.UnknownEnumVal_Multiplicity(endKind.ToString()));
		}

		private static string ParameterAsXml(IEdmFunctionParameter parameter)
		{
			return parameter.Name;
		}

		private static string PathAsXml(IEnumerable<string> path)
		{
			return EdmUtil.JoinInternal<string>("/", path);
		}

		private static string PropertyAsXml(IEdmProperty property)
		{
			return property.Name;
		}

		private string SerializationName(IEdmSchemaElement element)
		{
			string str = null;
			if (this.namespaceAliasMappings == null || !this.namespaceAliasMappings.TryGetValue(element.Namespace, out str))
			{
				return element.FullName();
			}
			else
			{
				return string.Concat(str, ".", element.Name);
			}
		}

		private static string SridAsXml(int? i)
		{
			if (i.HasValue)
			{
				return Convert.ToString(i.Value, CultureInfo.InvariantCulture);
			}
			else
			{
				return "Variable";
			}
		}

		private string TermAsXml(IEdmTerm term)
		{
			if (term != null)
			{
				return this.SerializationName(term);
			}
			else
			{
				return string.Empty;
			}
		}

		private string TypeDefinitionAsXml(IEdmSchemaType type)
		{
			return this.SerializationName(type);
		}

		private string TypeReferenceAsXml(IEdmTypeReference type)
		{
			if (!type.IsCollection())
			{
				if (!type.IsEntityReference())
				{
					return this.SerializationName((IEdmSchemaElement)type.Definition);
				}
				else
				{
					return string.Concat("Ref(", this.SerializationName(type.AsEntityReference().EntityReferenceDefinition().EntityType), ")");
				}
			}
			else
			{
				IEdmCollectionTypeReference edmCollectionTypeReference = type.AsCollection();
				return string.Concat("Collection(", this.SerializationName((IEdmSchemaElement)edmCollectionTypeReference.ElementType().Definition), ")");
			}
		}

		internal void WriteAnnotationsElementHeader(string annotationsTarget)
		{
			this.xmlWriter.WriteStartElement("Annotations");
			this.WriteRequiredAttribute<string>("Target", annotationsTarget, new Func<string, string>(EdmValueWriter.StringAsXml));
		}

		internal void WriteAnnotationStringAttribute(IEdmDirectValueAnnotation annotation)
		{
			IEdmPrimitiveValue value = (IEdmPrimitiveValue)annotation.Value;
			if (value != null)
			{
				this.xmlWriter.WriteAttributeString(annotation.Name, annotation.NamespaceUri, EdmValueWriter.PrimitiveValueAsXml(value));
			}
		}

		internal void WriteAnnotationStringElement(IEdmDirectValueAnnotation annotation)
		{
			IEdmPrimitiveValue value = (IEdmPrimitiveValue)annotation.Value;
			if (value != null)
			{
				this.xmlWriter.WriteRaw(((IEdmStringValue)value).Value);
			}
		}

		internal void WriteAssertTypeExpressionElementHeader(IEdmAssertTypeExpression expression, bool inlineType)
		{
			this.xmlWriter.WriteStartElement("AssertType");
			if (inlineType)
			{
				this.WriteRequiredAttribute<IEdmTypeReference>("Type", expression.Type, new Func<IEdmTypeReference, string>(this.TypeReferenceAsXml));
			}
		}

		internal void WriteAssociationElementHeader(IEdmNavigationProperty navigationProperty)
		{
			this.xmlWriter.WriteStartElement("Association");
			this.WriteRequiredAttribute<string>("Name", this.model.GetAssociationName(navigationProperty), new Func<string, string>(EdmValueWriter.StringAsXml));
		}

		internal void WriteAssociationEndElementHeader(IEdmNavigationProperty associationEnd)
		{
			this.xmlWriter.WriteStartElement("End");
				this.WriteRequiredAttribute<string>("Type", associationEnd.DeclaringEntityType ().FullName(), new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteRequiredAttribute<string>("Role", this.model.GetAssociationEndName(associationEnd), new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteRequiredAttribute<EdmMultiplicity>("Multiplicity", associationEnd.Multiplicity(), new Func<EdmMultiplicity, string>(EdmModelCsdlSchemaWriter.MultiplicityAsXml));
		}

		internal void WriteAssociationSetElementHeader(IEdmEntitySet entitySet, IEdmNavigationProperty navigationProperty)
		{
			this.xmlWriter.WriteStartElement("AssociationSet");
			this.WriteRequiredAttribute<string>("Name", this.model.GetAssociationSetName(entitySet, navigationProperty), new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteRequiredAttribute<string>("Association", this.model.GetAssociationFullName(navigationProperty), new Func<string, string>(EdmValueWriter.StringAsXml));
		}

		internal void WriteAssociationSetEndElementHeader(IEdmEntitySet entitySet, IEdmNavigationProperty property)
		{
			this.xmlWriter.WriteStartElement("End");
			this.WriteRequiredAttribute<string>("Role", this.model.GetAssociationEndName(property), new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteRequiredAttribute<string>("EntitySet", entitySet.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
		}

		internal void WriteBinaryConstantExpressionElement(IEdmBinaryConstantExpression expression)
		{
			this.xmlWriter.WriteStartElement("String");
			this.xmlWriter.WriteString(EdmValueWriter.BinaryAsXml(expression.Value));
			this.WriteEndElement();
		}

		internal void WriteBinaryTypeAttributes(IEdmBinaryTypeReference reference)
		{
			if (!reference.IsUnbounded)
			{
				this.WriteOptionalAttribute<int?>("MaxLength", reference.MaxLength, new Func<int?, string>(EdmValueWriter.IntAsXml));
			}
			else
			{
				this.WriteRequiredAttribute<string>("MaxLength", "Max", new Func<string, string>(EdmValueWriter.StringAsXml));
			}
			this.WriteOptionalAttribute<bool?>("FixedLength", reference.IsFixedLength, new Func<bool?, string>(EdmValueWriter.BooleanAsXml));
		}

		internal void WriteBooleanConstantExpressionElement(IEdmBooleanConstantExpression expression)
		{
			this.xmlWriter.WriteStartElement("Bool");
			this.xmlWriter.WriteString(EdmValueWriter.BooleanAsXml(expression.Value));
			this.WriteEndElement();
		}

		internal void WriteCollectionExpressionElementHeader(IEdmCollectionExpression expression)
		{
			this.xmlWriter.WriteStartElement("Collection");
		}

		internal void WriteCollectionTypeElementHeader(IEdmCollectionType collectionType, bool inlineType)
		{
			this.xmlWriter.WriteStartElement("CollectionType");
			if (inlineType)
			{
				this.WriteRequiredAttribute<IEdmTypeReference>("ElementType", collectionType.ElementType, new Func<IEdmTypeReference, string>(this.TypeReferenceAsXml));
			}
		}

		internal void WriteComplexTypeElementHeader(IEdmComplexType complexType)
		{
			this.xmlWriter.WriteStartElement("ComplexType");
			this.WriteRequiredAttribute<string>("Name", complexType.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteOptionalAttribute<IEdmComplexType>("BaseType", complexType.BaseComplexType(), new Func<IEdmComplexType, string>(this.TypeDefinitionAsXml));
			this.WriteOptionalAttribute<bool>("Abstract", complexType.IsAbstract, false, new Func<bool, string>(EdmValueWriter.BooleanAsXml));
		}

		internal void WriteDateTimeConstantExpressionElement(IEdmDateTimeConstantExpression expression)
		{
			this.xmlWriter.WriteStartElement("DateTime");
			this.xmlWriter.WriteString(EdmValueWriter.DateTimeAsXml(expression.Value));
			this.WriteEndElement();
		}

		internal void WriteDateTimeOffsetConstantExpressionElement(IEdmDateTimeOffsetConstantExpression expression)
		{
			this.xmlWriter.WriteStartElement("DateTimeOffset");
			this.xmlWriter.WriteString(EdmValueWriter.DateTimeOffsetAsXml(expression.Value));
			this.WriteEndElement();
		}

		internal void WriteDecimalConstantExpressionElement(IEdmDecimalConstantExpression expression)
		{
			this.xmlWriter.WriteStartElement("Decimal");
			this.xmlWriter.WriteString(EdmValueWriter.DecimalAsXml(expression.Value));
			this.WriteEndElement();
		}

		internal void WriteDecimalTypeAttributes(IEdmDecimalTypeReference reference)
		{
			this.WriteOptionalAttribute<int?>("Precision", reference.Precision, new Func<int?, string>(EdmValueWriter.IntAsXml));
			this.WriteOptionalAttribute<int?>("Scale", reference.Scale, new Func<int?, string>(EdmValueWriter.IntAsXml));
		}

		internal void WriteDefiningExpressionElement(string expression)
		{
			this.xmlWriter.WriteStartElement("DefiningExpression");
			this.xmlWriter.WriteString(expression);
			this.xmlWriter.WriteEndElement();
		}

		internal void WriteDelaredKeyPropertiesElementHeader()
		{
			this.xmlWriter.WriteStartElement("Key");
		}

		internal void WriteDocumentationElement(IEdmDocumentation documentation)
		{
			this.xmlWriter.WriteStartElement("Documentation");
			if (documentation.Summary != null)
			{
				this.xmlWriter.WriteStartElement("Summary");
				this.xmlWriter.WriteString(documentation.Summary);
				this.WriteEndElement();
			}
			if (documentation.Description != null)
			{
				this.xmlWriter.WriteStartElement("LongDescription");
				this.xmlWriter.WriteString(documentation.Description);
				this.WriteEndElement();
			}
			this.WriteEndElement();
		}

		internal void WriteEndElement()
		{
			this.xmlWriter.WriteEndElement();
		}

		internal void WriteEntityContainerElementHeader(IEdmEntityContainer container)
		{
			this.xmlWriter.WriteStartElement("EntityContainer");
			this.WriteRequiredAttribute<string>("Name", container.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
		}

		internal void WriteEntitySetElementHeader(IEdmEntitySet entitySet)
		{
			this.xmlWriter.WriteStartElement("EntitySet");
			this.WriteRequiredAttribute<string>("Name", entitySet.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteRequiredAttribute<string>("EntityType", entitySet.ElementType.FullName(), new Func<string, string>(EdmValueWriter.StringAsXml));
		}

		internal void WriteEntitySetReferenceExpressionElement(IEdmEntitySetReferenceExpression expression)
		{
			this.xmlWriter.WriteStartElement("EntitySetReference");
			this.WriteRequiredAttribute<IEdmEntitySet>("Name", expression.ReferencedEntitySet, new Func<IEdmEntitySet, string>(EdmModelCsdlSchemaWriter.EntitySetAsXml));
			this.WriteEndElement();
		}

		internal void WriteEntityTypeElementHeader(IEdmEntityType entityType)
		{
			this.xmlWriter.WriteStartElement("EntityType");
			this.WriteRequiredAttribute<string>("Name", entityType.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteOptionalAttribute<IEdmEntityType>("BaseType", entityType.BaseEntityType(), new Func<IEdmEntityType, string>(this.TypeDefinitionAsXml));
			this.WriteOptionalAttribute<bool>("Abstract", entityType.IsAbstract, false, new Func<bool, string>(EdmValueWriter.BooleanAsXml));
			this.WriteOptionalAttribute<bool>("OpenType", entityType.IsOpen, false, new Func<bool, string>(EdmValueWriter.BooleanAsXml));
		}

		internal void WriteEnumMemberElementHeader(IEdmEnumMember member)
		{
			this.xmlWriter.WriteStartElement("Member");
			this.WriteRequiredAttribute<string>("Name", member.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			bool? nullable = member.IsValueExplicit(this.model);
			if (!nullable.HasValue || nullable.Value)
			{
				this.WriteRequiredAttribute<IEdmPrimitiveValue>("Value", member.Value, new Func<IEdmPrimitiveValue, string>(EdmValueWriter.PrimitiveValueAsXml));
			}
		}

		internal void WriteEnumMemberReferenceExpressionElement(IEdmEnumMemberReferenceExpression expression)
		{
			this.xmlWriter.WriteStartElement("EnumMemberReference");
			this.WriteRequiredAttribute<IEdmEnumMember>("Name", expression.ReferencedEnumMember, new Func<IEdmEnumMember, string>(EdmModelCsdlSchemaWriter.EnumMemberAsXml));
			this.WriteEndElement();
		}

		internal void WriteEnumTypeElementHeader(IEdmEnumType enumType)
		{
			this.xmlWriter.WriteStartElement("EnumType");
			this.WriteRequiredAttribute<string>("Name", enumType.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			if (enumType.UnderlyingType.PrimitiveKind != EdmPrimitiveTypeKind.Int32)
			{
				this.WriteRequiredAttribute<IEdmPrimitiveType>("UnderlyingType", enumType.UnderlyingType, new Func<IEdmPrimitiveType, string>(this.TypeDefinitionAsXml));
			}
			this.WriteOptionalAttribute<bool>("IsFlags", enumType.IsFlags, false, new Func<bool, string>(EdmValueWriter.BooleanAsXml));
		}

		internal void WriteFloatingConstantExpressionElement(IEdmFloatingConstantExpression expression)
		{
			this.xmlWriter.WriteStartElement("Float");
			this.xmlWriter.WriteString(EdmValueWriter.FloatAsXml(expression.Value));
			this.WriteEndElement();
		}

		internal void WriteFunctionApplicationElementHeader(IEdmApplyExpression expression, bool isFunction)
		{
			this.xmlWriter.WriteStartElement("Apply");
			if (isFunction)
			{
				this.WriteRequiredAttribute<IEdmFunction>("Function", ((IEdmFunctionReferenceExpression)expression.AppliedFunction).ReferencedFunction, new Func<IEdmFunction, string>(this.FunctionAsXml));
			}
		}

		internal void WriteFunctionElementHeader(IEdmFunction function, bool inlineReturnType)
		{
			this.xmlWriter.WriteStartElement("Function");
			this.WriteRequiredAttribute<string>("Name", function.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			if (inlineReturnType)
			{
				this.WriteRequiredAttribute<IEdmTypeReference>("ReturnType", function.ReturnType, new Func<IEdmTypeReference, string>(this.TypeReferenceAsXml));
			}
		}

		internal void WriteFunctionImportElementHeader(IEdmFunctionImport functionImport)
		{
			this.xmlWriter.WriteStartElement("FunctionImport");
			this.WriteRequiredAttribute<string>("Name", functionImport.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteOptionalAttribute<IEdmTypeReference>("ReturnType", functionImport.ReturnType, new Func<IEdmTypeReference, string>(this.TypeReferenceAsXml));
			if (functionImport.IsComposable && functionImport.IsSideEffecting || !functionImport.IsComposable && !functionImport.IsSideEffecting)
			{
				this.WriteRequiredAttribute<bool>("IsSideEffecting", functionImport.IsSideEffecting, new Func<bool, string>(EdmValueWriter.BooleanAsXml));
			}
			this.WriteOptionalAttribute<bool>("IsComposable", functionImport.IsComposable, false, new Func<bool, string>(EdmValueWriter.BooleanAsXml));
			this.WriteOptionalAttribute<bool>("IsBindable", functionImport.IsBindable, false, new Func<bool, string>(EdmValueWriter.BooleanAsXml));
			if (functionImport.EntitySet == null)
			{
				return;
			}
			else
			{
				IEdmEntitySetReferenceExpression entitySet = functionImport.EntitySet as IEdmEntitySetReferenceExpression;
				if (entitySet == null)
				{
					IEdmPathExpression edmPathExpression = functionImport.EntitySet as IEdmPathExpression;
					if (edmPathExpression == null)
					{
						throw new InvalidOperationException(Strings.EdmModel_Validator_Semantic_FunctionImportEntitySetExpressionIsInvalid(functionImport.Name));
					}
					else
					{
						this.WriteOptionalAttribute<IEnumerable<string>>("EntitySetPath", edmPathExpression.Path, new Func<IEnumerable<string>, string>(EdmModelCsdlSchemaWriter.PathAsXml));
						return;
					}
				}
				else
				{
					this.WriteOptionalAttribute<string>("EntitySet", entitySet.ReferencedEntitySet.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
					return;
				}
			}
		}

		internal void WriteFunctionParameterElementHeader(IEdmFunctionParameter parameter, bool inlineType)
		{
			this.xmlWriter.WriteStartElement("Parameter");
			this.WriteRequiredAttribute<string>("Name", parameter.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			if (inlineType)
			{
				this.WriteRequiredAttribute<IEdmTypeReference>("Type", parameter.Type, new Func<IEdmTypeReference, string>(this.TypeReferenceAsXml));
			}
			this.WriteOptionalAttribute<EdmFunctionParameterMode>("Mode", parameter.Mode, EdmFunctionParameterMode.In, new Func<EdmFunctionParameterMode, string>(EdmModelCsdlSchemaWriter.FunctionParameterModeAsXml));
		}

		internal void WriteFunctionReferenceExpressionElement(IEdmFunctionReferenceExpression expression)
		{
			this.xmlWriter.WriteStartElement("FunctionReference");
			this.WriteRequiredAttribute<IEdmFunction>("Name", expression.ReferencedFunction, new Func<IEdmFunction, string>(this.FunctionAsXml));
			this.WriteEndElement();
		}

		internal void WriteGuidConstantExpressionElement(IEdmGuidConstantExpression expression)
		{
			this.xmlWriter.WriteStartElement("Guid");
			this.xmlWriter.WriteString(EdmValueWriter.GuidAsXml(expression.Value));
			this.WriteEndElement();
		}

		internal void WriteIfExpressionElementHeader(IEdmIfExpression expression)
		{
			this.xmlWriter.WriteStartElement("If");
		}

		internal void WriteInlineExpression(IEdmExpression expression)
		{
			EdmExpressionKind expressionKind = expression.ExpressionKind;
			switch (expressionKind)
			{
				case EdmExpressionKind.BinaryConstant:
				{
					this.WriteRequiredAttribute<byte[]>("Binary", ((IEdmBinaryConstantExpression)expression).Value, new Func<byte[], string>(EdmValueWriter.BinaryAsXml));
					return;
				}
				case EdmExpressionKind.BooleanConstant:
				{
					this.WriteRequiredAttribute<bool>("Bool", ((IEdmBooleanConstantExpression)expression).Value, new Func<bool, string>(EdmValueWriter.BooleanAsXml));
					return;
				}
				case EdmExpressionKind.DateTimeConstant:
				{
					this.WriteRequiredAttribute<DateTime>("DateTime", ((IEdmDateTimeConstantExpression)expression).Value, new Func<DateTime, string>(EdmValueWriter.DateTimeAsXml));
					return;
				}
				case EdmExpressionKind.DateTimeOffsetConstant:
				{
					this.WriteRequiredAttribute<DateTimeOffset>("DateTimeOffset", ((IEdmDateTimeOffsetConstantExpression)expression).Value, new Func<DateTimeOffset, string>(EdmValueWriter.DateTimeOffsetAsXml));
					return;
				}
				case EdmExpressionKind.DecimalConstant:
				{
					this.WriteRequiredAttribute<decimal>("Decimal", ((IEdmDecimalConstantExpression)expression).Value, new Func<decimal, string>(EdmValueWriter.DecimalAsXml));
					return;
				}
				case EdmExpressionKind.FloatingConstant:
				{
					this.WriteRequiredAttribute<double>("Float", ((IEdmFloatingConstantExpression)expression).Value, new Func<double, string>(EdmValueWriter.FloatAsXml));
					return;
				}
				case EdmExpressionKind.GuidConstant:
				{
					this.WriteRequiredAttribute<Guid>("Guid", ((IEdmGuidConstantExpression)expression).Value, new Func<Guid, string>(EdmValueWriter.GuidAsXml));
					return;
				}
				case EdmExpressionKind.IntegerConstant:
				{
					this.WriteRequiredAttribute<long>("Int", ((IEdmIntegerConstantExpression)expression).Value, new Func<long, string>(EdmValueWriter.LongAsXml));
					return;
				}
				case EdmExpressionKind.StringConstant:
				{
					this.WriteRequiredAttribute<string>("String", ((IEdmStringConstantExpression)expression).Value, new Func<string, string>(EdmValueWriter.StringAsXml));
					return;
				}
				case EdmExpressionKind.TimeConstant:
				{
					this.WriteRequiredAttribute<TimeSpan>("Time", ((IEdmTimeConstantExpression)expression).Value, new Func<TimeSpan, string>(EdmValueWriter.TimeAsXml));
					return;
				}
				case EdmExpressionKind.Null:
				case EdmExpressionKind.Record:
				case EdmExpressionKind.Collection:
				{
					return;
				}
				case EdmExpressionKind.Path:
				{
					this.WriteRequiredAttribute<IEnumerable<string>>("Path", ((IEdmPathExpression)expression).Path, new Func<IEnumerable<string>, string>(EdmModelCsdlSchemaWriter.PathAsXml));
					return;
				}
				default:
				{
					return;
				}
			}
		}

		internal void WriteIntegerConstantExpressionElement(IEdmIntegerConstantExpression expression)
		{
			this.xmlWriter.WriteStartElement("Int");
			this.xmlWriter.WriteString(EdmValueWriter.LongAsXml(expression.Value));
			this.WriteEndElement();
		}

		internal void WriteIsTypeExpressionElementHeader(IEdmIsTypeExpression expression, bool inlineType)
		{
			this.xmlWriter.WriteStartElement("IsType");
			if (inlineType)
			{
				this.WriteRequiredAttribute<IEdmTypeReference>("Type", expression.Type, new Func<IEdmTypeReference, string>(this.TypeReferenceAsXml));
			}
		}

		internal void WriteLabeledElementHeader(IEdmLabeledExpression labeledElement)
		{
			this.xmlWriter.WriteStartElement("LabeledElement");
			this.WriteRequiredAttribute<string>("Name", labeledElement.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
		}

		internal void WriteNamespaceUsingElement(string usingNamespace, string alias)
		{
			this.xmlWriter.WriteStartElement("Using");
			this.WriteRequiredAttribute<string>("Namespace", usingNamespace, new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteRequiredAttribute<string>("Alias", alias, new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteEndElement();
		}

		internal void WriteNavigationPropertyElementHeader(IEdmNavigationProperty member)
		{
			this.xmlWriter.WriteStartElement("NavigationProperty");
			this.WriteRequiredAttribute<string>("Name", member.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteRequiredAttribute<string>("Relationship", this.model.GetAssociationFullName(member), new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteRequiredAttribute<string>("ToRole", this.model.GetAssociationEndName(member.Partner), new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteRequiredAttribute<string>("FromRole", this.model.GetAssociationEndName(member), new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteOptionalAttribute<bool>("ContainsTarget", member.ContainsTarget, false, new Func<bool, string>(EdmValueWriter.BooleanAsXml));
		}

		internal void WriteNullableAttribute(IEdmTypeReference reference)
		{
			this.WriteOptionalAttribute<bool>("Nullable", reference.IsNullable, true, new Func<bool, string>(EdmValueWriter.BooleanAsXml));
		}

		internal void WriteNullConstantExpressionElement(IEdmNullExpression expression)
		{
			this.xmlWriter.WriteStartElement("Null");
			this.WriteEndElement();
		}

		internal void WriteOperationActionElement(string elementName, EdmOnDeleteAction operationAction)
		{
			this.xmlWriter.WriteStartElement(elementName);
			this.WriteRequiredAttribute<string>("Action", operationAction.ToString(), new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteEndElement();
		}

		internal void WriteOptionalAttribute<T>(string attribute, T value, T defaultValue, Func<T, string> toXml)
		{
			if (!value.Equals(defaultValue))
			{
				this.xmlWriter.WriteAttributeString(attribute, toXml(value));
			}
		}

		internal void WriteOptionalAttribute<T>(string attribute, T value, Func<T, string> toXml)
		{
			if (value != null)
			{
				this.xmlWriter.WriteAttributeString(attribute, toXml(value));
			}
		}

		internal void WriteParameterReferenceExpressionElement(IEdmParameterReferenceExpression expression)
		{
			this.xmlWriter.WriteStartElement("ParameterReference");
			this.WriteRequiredAttribute<IEdmFunctionParameter>("Name", expression.ReferencedParameter, new Func<IEdmFunctionParameter, string>(EdmModelCsdlSchemaWriter.ParameterAsXml));
			this.WriteEndElement();
		}

		internal void WritePathExpressionElement(IEdmPathExpression expression)
		{
			this.xmlWriter.WriteStartElement("Path");
			this.xmlWriter.WriteString(EdmModelCsdlSchemaWriter.PathAsXml(expression.Path));
			this.WriteEndElement();
		}

		internal void WritePropertyConstructorElementHeader(IEdmPropertyConstructor constructor, bool isInline)
		{
			this.xmlWriter.WriteStartElement("PropertyValue");
			this.WriteRequiredAttribute<string>("Property", constructor.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			if (isInline)
			{
				this.WriteInlineExpression(constructor.Value);
			}
		}

		internal void WritePropertyRefElement(IEdmStructuralProperty property)
		{
			this.xmlWriter.WriteStartElement("PropertyRef");
			this.WriteRequiredAttribute<string>("Name", property.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteEndElement();
		}

		internal void WritePropertyReferenceExpressionElementHeader(IEdmPropertyReferenceExpression expression)
		{
			this.xmlWriter.WriteStartElement("PropertyReference");
			this.WriteRequiredAttribute<IEdmProperty>("Name", expression.ReferencedProperty, new Func<IEdmProperty, string>(EdmModelCsdlSchemaWriter.PropertyAsXml));
		}

		internal void WritePropertyValueElementHeader(IEdmPropertyValueBinding value, bool isInline)
		{
			this.xmlWriter.WriteStartElement("PropertyValue");
			this.WriteRequiredAttribute<string>("Property", value.BoundProperty.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			if (isInline)
			{
				this.WriteInlineExpression(value.Value);
			}
		}

		internal void WriteRecordExpressionElementHeader(IEdmRecordExpression expression)
		{
			this.xmlWriter.WriteStartElement("Record");
			this.WriteOptionalAttribute<IEdmStructuredTypeReference>("Type", expression.DeclaredType, new Func<IEdmStructuredTypeReference, string>(this.TypeReferenceAsXml));
		}

		internal void WriteReferentialConstraintDependentEndElementHeader(IEdmNavigationProperty end)
		{
			this.xmlWriter.WriteStartElement("Dependent");
			this.WriteRequiredAttribute<string>("Role", this.model.GetAssociationEndName(end), new Func<string, string>(EdmValueWriter.StringAsXml));
		}

		internal void WriteReferentialConstraintElementHeader(IEdmNavigationProperty constraint)
		{
			this.xmlWriter.WriteStartElement("ReferentialConstraint");
		}

		internal void WriteReferentialConstraintPrincipalEndElementHeader(IEdmNavigationProperty end)
		{
			this.xmlWriter.WriteStartElement("Principal");
			this.WriteRequiredAttribute<string>("Role", this.model.GetAssociationEndName(end), new Func<string, string>(EdmValueWriter.StringAsXml));
		}

		internal void WriteRequiredAttribute<T>(string attribute, T value, Func<T, string> toXml)
		{
			this.xmlWriter.WriteAttributeString(attribute, toXml(value));
		}

		internal void WriteReturnTypeElementHeader()
		{
			this.xmlWriter.WriteStartElement("ReturnType");
		}

		internal void WriteRowTypeElementHeader()
		{
			this.xmlWriter.WriteStartElement("RowType");
		}

		internal void WriteSchemaElementHeader(EdmSchema schema, string alias, IEnumerable<KeyValuePair<string, string>> mappings)
		{
			string csdlNamespace = EdmModelCsdlSchemaWriter.GetCsdlNamespace(this.version);
			this.xmlWriter.WriteStartElement("Schema", csdlNamespace);
			this.WriteOptionalAttribute<string>("Namespace", schema.Namespace, string.Empty, new Func<string, string>(EdmValueWriter.StringAsXml));
			this.WriteOptionalAttribute<string>("Alias", alias, new Func<string, string>(EdmValueWriter.StringAsXml));
			if (mappings != null)
			{
				foreach (KeyValuePair<string, string> mapping in mappings)
				{
					this.xmlWriter.WriteAttributeString("xmlns", mapping.Key, null, mapping.Value);
				}
			}
		}

		internal void WriteSpatialTypeAttributes(IEdmSpatialTypeReference reference)
		{
			this.WriteRequiredAttribute<int?>("SRID", reference.SpatialReferenceIdentifier, new Func<int?, string>(EdmModelCsdlSchemaWriter.SridAsXml));
		}

		internal void WriteStringConstantExpressionElement(IEdmStringConstantExpression expression)
		{
			this.xmlWriter.WriteStartElement("String");
			this.xmlWriter.WriteString(EdmValueWriter.StringAsXml(expression.Value));
			this.WriteEndElement();
		}

		internal void WriteStringTypeAttributes(IEdmStringTypeReference reference)
		{
			this.WriteOptionalAttribute<string>("Collation", reference.Collation, new Func<string, string>(EdmValueWriter.StringAsXml));
			if (!reference.IsUnbounded)
			{
				this.WriteOptionalAttribute<int?>("MaxLength", reference.MaxLength, new Func<int?, string>(EdmValueWriter.IntAsXml));
			}
			else
			{
				this.WriteRequiredAttribute<string>("MaxLength", "Max", new Func<string, string>(EdmValueWriter.StringAsXml));
			}
			this.WriteOptionalAttribute<bool?>("FixedLength", reference.IsFixedLength, new Func<bool?, string>(EdmValueWriter.BooleanAsXml));
			this.WriteOptionalAttribute<bool?>("Unicode", reference.IsUnicode, new Func<bool?, string>(EdmValueWriter.BooleanAsXml));
		}

		internal void WriteStructuralPropertyElementHeader(IEdmStructuralProperty property, bool inlineType)
		{
			this.xmlWriter.WriteStartElement("Property");
			this.WriteRequiredAttribute<string>("Name", property.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			if (inlineType)
			{
				this.WriteRequiredAttribute<IEdmTypeReference>("Type", property.Type, new Func<IEdmTypeReference, string>(this.TypeReferenceAsXml));
			}
			this.WriteOptionalAttribute<EdmConcurrencyMode>("ConcurrencyMode", property.ConcurrencyMode, EdmConcurrencyMode.None, new Func<EdmConcurrencyMode, string>(EdmModelCsdlSchemaWriter.ConcurrencyModeAsXml));
			this.WriteOptionalAttribute<string>("DefaultValue", property.DefaultValueString, new Func<string, string>(EdmValueWriter.StringAsXml));
		}

		internal void WriteTemporalTypeAttributes(IEdmTemporalTypeReference reference)
		{
			this.WriteOptionalAttribute<int?>("Precision", reference.Precision, new Func<int?, string>(EdmValueWriter.IntAsXml));
		}

		internal void WriteTypeAnnotationElementHeader(IEdmTypeAnnotation annotation)
		{
			this.xmlWriter.WriteStartElement("TypeAnnotation");
			this.WriteRequiredAttribute<IEdmTerm>("Term", annotation.Term, new Func<IEdmTerm, string>(this.TermAsXml));
			this.WriteOptionalAttribute<string>("Qualifier", annotation.Qualifier, new Func<string, string>(EdmValueWriter.StringAsXml));
		}

		internal void WriteValueAnnotationElementHeader(IEdmValueAnnotation annotation, bool isInline)
		{
			this.xmlWriter.WriteStartElement("ValueAnnotation");
			this.WriteRequiredAttribute<IEdmTerm>("Term", annotation.Term, new Func<IEdmTerm, string>(this.TermAsXml));
			this.WriteOptionalAttribute<string>("Qualifier", annotation.Qualifier, new Func<string, string>(EdmValueWriter.StringAsXml));
			if (isInline)
			{
				this.WriteInlineExpression(annotation.Value);
			}
		}

		internal void WriteValueTermElementHeader(IEdmValueTerm term, bool inlineType)
		{
			this.xmlWriter.WriteStartElement("ValueTerm");
			this.WriteRequiredAttribute<string>("Name", term.Name, new Func<string, string>(EdmValueWriter.StringAsXml));
			if (inlineType && term.Type != null)
			{
				this.WriteRequiredAttribute<IEdmTypeReference>("Type", term.Type, new Func<IEdmTypeReference, string>(this.TypeReferenceAsXml));
			}
		}
	}
}