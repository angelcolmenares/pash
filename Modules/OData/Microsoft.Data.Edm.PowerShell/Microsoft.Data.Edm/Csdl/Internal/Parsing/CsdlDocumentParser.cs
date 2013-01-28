using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Common;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing
{
	internal class CsdlDocumentParser : EdmXmlDocumentParser<CsdlSchema>
	{
		private Version artifactVersion;

		internal override IEnumerable<KeyValuePair<Version, string>> SupportedVersions
		{
			get
			{
				return (from kvp in CsdlConstants.SupportedVersions select from ns in kvp.Value select new KeyValuePair<Version, string>(kvp.Key, ns)).SelectMany(x => x);
			}
		}

		internal CsdlDocumentParser(string documentPath, XmlReader reader) : base(documentPath, reader)
		{
		}

		private void AddChildParsers(XmlElementParser parent, IEnumerable<XmlElementParser> children)
		{
			foreach (XmlElementParser child in children)
			{
				parent.AddChildParser(child);
			}
		}

		protected override void AnnotateItem(object result, XmlElementValueCollection childValues)
		{
			CsdlElement csdlElement = result as CsdlElement;
			if (csdlElement != null)
			{
				foreach (XmlAnnotationInfo annotation in this.currentElement.Annotations)
				{
					csdlElement.AddAnnotation(new CsdlDirectValueAnnotation(annotation.NamespaceName, annotation.Name, annotation.Value, annotation.IsAttribute, annotation.Location));
				}
				foreach (CsdlVocabularyAnnotationBase csdlVocabularyAnnotationBase in childValues.ValuesOfType<CsdlVocabularyAnnotationBase>())
				{
					csdlElement.AddAnnotation(csdlVocabularyAnnotationBase);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private static CsdlConstantExpression ConstantExpression(EdmValueKind kind, XmlElementValueCollection childValues, CsdlLocation location)
		{
			string textValue;
			XmlTextValue firstText = childValues.FirstText;
			EdmValueKind edmValueKind = kind;
			if (firstText != null)
			{
				textValue = firstText.TextValue;
			}
			else
			{
				textValue = string.Empty;
			}
			return new CsdlConstantExpression(edmValueKind, textValue, location);
		}

		private XmlElementParser<CsdlSchema> CreateRootElementParser()
		{
			CsdlDocumentParser csdlDocumentParser = this;
			string str = "Documentation";
			Func<XmlElementInfo, XmlElementValueCollection, CsdlDocumentation> func = new Func<XmlElementInfo, XmlElementValueCollection, CsdlDocumentation>(this.OnDocumentationElement);
			XmlElementParser[] xmlElementParserArray = new XmlElementParser[2];
			XmlElementParser[] xmlElementParserArray1 = xmlElementParserArray;
			int num = 0;
			CsdlDocumentParser csdlDocumentParser1 = this;
			string str1 = "Summary";
			xmlElementParserArray1[num] = csdlDocumentParser1.Element<string>(str1, (XmlElementInfo element, XmlElementValueCollection children) => children.FirstText.Value, new XmlElementParser[0]);
			XmlElementParser[] xmlElementParserArray2 = xmlElementParserArray;
			int num1 = 1;
			CsdlDocumentParser csdlDocumentParser2 = this;
			string str2 = "LongDescription";
			xmlElementParserArray2[num1] = csdlDocumentParser2.Element<string>(str2, (XmlElementInfo element, XmlElementValueCollection children) => children.FirstText.TextValue, new XmlElementParser[0]);
			XmlElementParser<CsdlDocumentation> xmlElementParser = csdlDocumentParser.CsdlElement<CsdlDocumentation>(str, func, xmlElementParserArray);
			XmlElementParser[] xmlElementParserArray3 = new XmlElementParser[1];
			xmlElementParserArray3[0] = xmlElementParser;
			XmlElementParser<CsdlTypeReference> xmlElementParser1 = base.CsdlElement<CsdlTypeReference>("ReferenceType", new Func<XmlElementInfo, XmlElementValueCollection, CsdlTypeReference>(this.OnEntityReferenceTypeElement), xmlElementParserArray3);
			XmlElementParser<CsdlTypeReference> xmlElementParser2 = base.CsdlElement<CsdlTypeReference>("RowType", new Func<XmlElementInfo, XmlElementValueCollection, CsdlTypeReference>(this.OnRowTypeElement), new XmlElementParser[0]);
			XmlElementParser[] xmlElementParserArray4 = new XmlElementParser[4];
			xmlElementParserArray4[0] = xmlElementParser;
			XmlElementParser[] xmlElementParserArray5 = new XmlElementParser[1];
			xmlElementParserArray5[0] = xmlElementParser;
			xmlElementParserArray4[1] = base.CsdlElement<CsdlTypeReference>("TypeRef", new Func<XmlElementInfo, XmlElementValueCollection, CsdlTypeReference>(this.OnTypeRefElement), xmlElementParserArray5);
			xmlElementParserArray4[2] = xmlElementParser2;
			xmlElementParserArray4[3] = xmlElementParser1;
			XmlElementParser<CsdlTypeReference> xmlElementParser3 = base.CsdlElement<CsdlTypeReference>("CollectionType", new Func<XmlElementInfo, XmlElementValueCollection, CsdlTypeReference>(this.OnCollectionTypeElement), xmlElementParserArray4);
			XmlElementParser[] xmlElementParserArray6 = new XmlElementParser[1];
			xmlElementParserArray6[0] = xmlElementParser;
			XmlElementParser<CsdlProperty> xmlElementParser4 = base.CsdlElement<CsdlProperty>("Property", new Func<XmlElementInfo, XmlElementValueCollection, CsdlProperty>(this.OnPropertyElement), xmlElementParserArray6);
			XmlElementParser[] xmlElementParserArray7 = new XmlElementParser[5];
			xmlElementParserArray7[0] = xmlElementParser;
			XmlElementParser[] xmlElementParserArray8 = new XmlElementParser[1];
			xmlElementParserArray8[0] = xmlElementParser;
			xmlElementParserArray7[1] = base.CsdlElement<CsdlTypeReference>("TypeRef", new Func<XmlElementInfo, XmlElementValueCollection, CsdlTypeReference>(this.OnTypeRefElement), xmlElementParserArray8);
			xmlElementParserArray7[2] = xmlElementParser2;
			xmlElementParserArray7[3] = xmlElementParser3;
			xmlElementParserArray7[4] = xmlElementParser1;
			XmlElementParser<CsdlProperty> xmlElementParser5 = base.CsdlElement<CsdlProperty>("Property", new Func<XmlElementInfo, XmlElementValueCollection, CsdlProperty>(this.OnPropertyElement), xmlElementParserArray7);
			XmlElementParser<CsdlExpressionBase> xmlElementParser6 = base.CsdlElement<CsdlExpressionBase>("String", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(CsdlDocumentParser.OnStringConstantExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser7 = base.CsdlElement<CsdlExpressionBase>("Binary", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(CsdlDocumentParser.OnBinaryConstantExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser8 = base.CsdlElement<CsdlExpressionBase>("Int", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(CsdlDocumentParser.OnIntConstantExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser9 = base.CsdlElement<CsdlExpressionBase>("Float", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(CsdlDocumentParser.OnFloatConstantExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser10 = base.CsdlElement<CsdlExpressionBase>("Guid", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(CsdlDocumentParser.OnGuidConstantExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser11 = base.CsdlElement<CsdlExpressionBase>("Decimal", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(CsdlDocumentParser.OnDecimalConstantExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser12 = base.CsdlElement<CsdlExpressionBase>("Bool", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(CsdlDocumentParser.OnBoolConstantExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser13 = base.CsdlElement<CsdlExpressionBase>("DateTime", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(CsdlDocumentParser.OnDateTimeConstantExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser14 = base.CsdlElement<CsdlExpressionBase>("Time", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(CsdlDocumentParser.OnTimeConstantExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser15 = base.CsdlElement<CsdlExpressionBase>("DateTimeOffset", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(CsdlDocumentParser.OnDateTimeOffsetConstantExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser16 = base.CsdlElement<CsdlExpressionBase>("Null", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(CsdlDocumentParser.OnNullConstantExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser17 = base.CsdlElement<CsdlExpressionBase>("Path", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(CsdlDocumentParser.OnPathExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser18 = base.CsdlElement<CsdlExpressionBase>("FunctionReference", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(this.OnFunctionReferenceExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser19 = base.CsdlElement<CsdlExpressionBase>("ParameterReference", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(this.OnParameterReferenceExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser20 = base.CsdlElement<CsdlExpressionBase>("EntitySetReference", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(this.OnEntitySetReferenceExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser21 = base.CsdlElement<CsdlExpressionBase>("EnumMemberReference", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(this.OnEnumMemberReferenceExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser22 = base.CsdlElement<CsdlExpressionBase>("PropertyReference", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(this.OnPropertyReferenceExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser23 = base.CsdlElement<CsdlExpressionBase>("If", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(this.OnIfExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser24 = base.CsdlElement<CsdlExpressionBase>("AssertType", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(this.OnAssertTypeExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser25 = base.CsdlElement<CsdlExpressionBase>("IsType", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(this.OnIsTypeExpression), new XmlElementParser[0]);
			XmlElementParser<CsdlPropertyValue> xmlElementParser26 = base.CsdlElement<CsdlPropertyValue>("PropertyValue", new Func<XmlElementInfo, XmlElementValueCollection, CsdlPropertyValue>(this.OnPropertyValueElement), new XmlElementParser[0]);
			XmlElementParser[] xmlElementParserArray9 = new XmlElementParser[1];
			xmlElementParserArray9[0] = xmlElementParser26;
			XmlElementParser<CsdlRecordExpression> xmlElementParser27 = base.CsdlElement<CsdlRecordExpression>("Record", new Func<XmlElementInfo, XmlElementValueCollection, CsdlRecordExpression>(this.OnRecordElement), xmlElementParserArray9);
			XmlElementParser<CsdlLabeledExpression> xmlElementParser28 = base.CsdlElement<CsdlLabeledExpression>("LabeledElement", new Func<XmlElementInfo, XmlElementValueCollection, CsdlLabeledExpression>(this.OnLabeledElement), new XmlElementParser[0]);
			XmlElementParser<CsdlCollectionExpression> xmlElementParser29 = base.CsdlElement<CsdlCollectionExpression>("Collection", new Func<XmlElementInfo, XmlElementValueCollection, CsdlCollectionExpression>(this.OnCollectionElement), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser30 = base.CsdlElement<CsdlExpressionBase>("Apply", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(this.OnApplyElement), new XmlElementParser[0]);
			XmlElementParser<CsdlExpressionBase> xmlElementParser31 = base.CsdlElement<CsdlExpressionBase>("LabeledElementReference", new Func<XmlElementInfo, XmlElementValueCollection, CsdlExpressionBase>(this.OnLabeledElementReferenceExpression), new XmlElementParser[0]);
			XmlElementParser[] xmlElementParserArray10 = new XmlElementParser[26];
			xmlElementParserArray10[0] = xmlElementParser6;
			xmlElementParserArray10[1] = xmlElementParser7;
			xmlElementParserArray10[2] = xmlElementParser8;
			xmlElementParserArray10[3] = xmlElementParser9;
			xmlElementParserArray10[4] = xmlElementParser10;
			xmlElementParserArray10[5] = xmlElementParser11;
			xmlElementParserArray10[6] = xmlElementParser12;
			xmlElementParserArray10[7] = xmlElementParser13;
			xmlElementParserArray10[8] = xmlElementParser15;
			xmlElementParserArray10[9] = xmlElementParser14;
			xmlElementParserArray10[10] = xmlElementParser16;
			xmlElementParserArray10[11] = xmlElementParser17;
			xmlElementParserArray10[12] = xmlElementParser23;
			xmlElementParserArray10[13] = xmlElementParser25;
			xmlElementParserArray10[14] = xmlElementParser24;
			xmlElementParserArray10[15] = xmlElementParser27;
			xmlElementParserArray10[16] = xmlElementParser29;
			xmlElementParserArray10[17] = xmlElementParser31;
			xmlElementParserArray10[18] = xmlElementParser22;
			xmlElementParserArray10[19] = xmlElementParser26;
			xmlElementParserArray10[20] = xmlElementParser28;
			xmlElementParserArray10[21] = xmlElementParser18;
			xmlElementParserArray10[22] = xmlElementParser20;
			xmlElementParserArray10[23] = xmlElementParser21;
			xmlElementParserArray10[24] = xmlElementParser19;
			xmlElementParserArray10[25] = xmlElementParser30;
			XmlElementParser[] xmlElementParserArray11 = xmlElementParserArray10;
			this.AddChildParsers(xmlElementParser22, xmlElementParserArray11);
			this.AddChildParsers(xmlElementParser23, xmlElementParserArray11);
			this.AddChildParsers(xmlElementParser24, xmlElementParserArray11);
			this.AddChildParsers(xmlElementParser25, xmlElementParserArray11);
			this.AddChildParsers(xmlElementParser26, xmlElementParserArray11);
			this.AddChildParsers(xmlElementParser29, xmlElementParserArray11);
			this.AddChildParsers(xmlElementParser28, xmlElementParserArray11);
			this.AddChildParsers(xmlElementParser30, xmlElementParserArray11);
			XmlElementParser<CsdlValueAnnotation> xmlElementParser32 = base.CsdlElement<CsdlValueAnnotation>("ValueAnnotation", new Func<XmlElementInfo, XmlElementValueCollection, CsdlValueAnnotation>(this.OnValueAnnotationElement), new XmlElementParser[0]);
			this.AddChildParsers(xmlElementParser32, xmlElementParserArray11);
			XmlElementParser[] xmlElementParserArray12 = new XmlElementParser[1];
			xmlElementParserArray12[0] = xmlElementParser26;
			XmlElementParser<CsdlTypeAnnotation> xmlElementParser33 = base.CsdlElement<CsdlTypeAnnotation>("TypeAnnotation", new Func<XmlElementInfo, XmlElementValueCollection, CsdlTypeAnnotation>(this.OnTypeAnnotationElement), xmlElementParserArray12);
			xmlElementParser4.AddChildParser(xmlElementParser32);
			xmlElementParser4.AddChildParser(xmlElementParser33);
			xmlElementParser5.AddChildParser(xmlElementParser32);
			xmlElementParser5.AddChildParser(xmlElementParser33);
			xmlElementParser2.AddChildParser(xmlElementParser5);
			xmlElementParser3.AddChildParser(xmlElementParser3);
			CsdlDocumentParser csdlDocumentParser3 = this;
			string str3 = "Schema";
			Func<XmlElementInfo, XmlElementValueCollection, CsdlSchema> func1 = new Func<XmlElementInfo, XmlElementValueCollection, CsdlSchema>(this.OnSchemaElement);
			XmlElementParser[] xmlElementParserArray13 = new XmlElementParser[10];
			xmlElementParserArray13[0] = xmlElementParser;
			xmlElementParserArray13[1] = base.CsdlElement<CsdlUsing>("Using", new Func<XmlElementInfo, XmlElementValueCollection, CsdlUsing>(this.OnUsingElement), new XmlElementParser[0]);
			XmlElementParser[] xmlElementParserArray14 = new XmlElementParser[4];
			xmlElementParserArray14[0] = xmlElementParser;
			xmlElementParserArray14[1] = xmlElementParser4;
			xmlElementParserArray14[2] = xmlElementParser32;
			xmlElementParserArray14[3] = xmlElementParser33;
			xmlElementParserArray13[2] = base.CsdlElement<CsdlComplexType>("ComplexType", new Func<XmlElementInfo, XmlElementValueCollection, CsdlComplexType>(this.OnComplexTypeElement), xmlElementParserArray14);
			XmlElementParser[] xmlElementParserArray15 = new XmlElementParser[6];
			xmlElementParserArray15[0] = xmlElementParser;
			XmlElementParser[] xmlElementParserArray16 = new XmlElementParser[1];
			xmlElementParserArray16[0] = base.CsdlElement<CsdlPropertyReference>("PropertyRef", new Func<XmlElementInfo, XmlElementValueCollection, CsdlPropertyReference>(this.OnPropertyRefElement), new XmlElementParser[0]);
			xmlElementParserArray15[1] = base.CsdlElement<CsdlKey>("Key", new Func<XmlElementInfo, XmlElementValueCollection, CsdlKey>(CsdlDocumentParser.OnEntityKeyElement), xmlElementParserArray16);
			xmlElementParserArray15[2] = xmlElementParser4;
			XmlElementParser[] xmlElementParserArray17 = new XmlElementParser[3];
			xmlElementParserArray17[0] = xmlElementParser;
			xmlElementParserArray17[1] = xmlElementParser32;
			xmlElementParserArray17[2] = xmlElementParser33;
			xmlElementParserArray15[3] = base.CsdlElement<CsdlNavigationProperty>("NavigationProperty", new Func<XmlElementInfo, XmlElementValueCollection, CsdlNavigationProperty>(this.OnNavigationPropertyElement), xmlElementParserArray17);
			xmlElementParserArray15[4] = xmlElementParser32;
			xmlElementParserArray15[5] = xmlElementParser33;
			xmlElementParserArray13[3] = base.CsdlElement<CsdlEntityType>("EntityType", new Func<XmlElementInfo, XmlElementValueCollection, CsdlEntityType>(this.OnEntityTypeElement), xmlElementParserArray15);
			XmlElementParser[] xmlElementParserArray18 = new XmlElementParser[3];
			xmlElementParserArray18[0] = xmlElementParser;
			XmlElementParser[] xmlElementParserArray19 = new XmlElementParser[2];
			xmlElementParserArray19[0] = xmlElementParser;
			XmlElementParser[] xmlElementParserArray20 = new XmlElementParser[1];
			xmlElementParserArray20[0] = xmlElementParser;
			xmlElementParserArray19[1] = base.CsdlElement<CsdlOnDelete>("OnDelete", new Func<XmlElementInfo, XmlElementValueCollection, CsdlOnDelete>(this.OnDeleteActionElement), xmlElementParserArray20);
			xmlElementParserArray18[1] = base.CsdlElement<CsdlAssociationEnd>("End", new Func<XmlElementInfo, XmlElementValueCollection, CsdlAssociationEnd>(this.OnAssociationEndElement), xmlElementParserArray19);
			XmlElementParser[] xmlElementParserArray21 = new XmlElementParser[3];
			xmlElementParserArray21[0] = xmlElementParser;
			XmlElementParser[] xmlElementParserArray22 = new XmlElementParser[2];
			xmlElementParserArray22[0] = xmlElementParser;
			xmlElementParserArray22[1] = base.CsdlElement<CsdlPropertyReference>("PropertyRef", new Func<XmlElementInfo, XmlElementValueCollection, CsdlPropertyReference>(this.OnPropertyRefElement), new XmlElementParser[0]);
			xmlElementParserArray21[1] = base.CsdlElement<CsdlReferentialConstraintRole>("Principal", new Func<XmlElementInfo, XmlElementValueCollection, CsdlReferentialConstraintRole>(this.OnReferentialConstraintRoleElement), xmlElementParserArray22);
			XmlElementParser[] xmlElementParserArray23 = new XmlElementParser[2];
			xmlElementParserArray23[0] = xmlElementParser;
			xmlElementParserArray23[1] = base.CsdlElement<CsdlPropertyReference>("PropertyRef", new Func<XmlElementInfo, XmlElementValueCollection, CsdlPropertyReference>(this.OnPropertyRefElement), new XmlElementParser[0]);
			xmlElementParserArray21[2] = base.CsdlElement<CsdlReferentialConstraintRole>("Dependent", new Func<XmlElementInfo, XmlElementValueCollection, CsdlReferentialConstraintRole>(this.OnReferentialConstraintRoleElement), xmlElementParserArray23);
			xmlElementParserArray18[2] = base.CsdlElement<CsdlReferentialConstraint>("ReferentialConstraint", new Func<XmlElementInfo, XmlElementValueCollection, CsdlReferentialConstraint>(this.OnReferentialConstraintElement), xmlElementParserArray21);
			xmlElementParserArray13[4] = base.CsdlElement<CsdlAssociation>("Association", new Func<XmlElementInfo, XmlElementValueCollection, CsdlAssociation>(this.OnAssociationElement), xmlElementParserArray18);
			XmlElementParser[] xmlElementParserArray24 = new XmlElementParser[4];
			xmlElementParserArray24[0] = xmlElementParser;
			XmlElementParser[] xmlElementParserArray25 = new XmlElementParser[1];
			xmlElementParserArray25[0] = xmlElementParser;
			xmlElementParserArray24[1] = base.CsdlElement<CsdlEnumMember>("Member", new Func<XmlElementInfo, XmlElementValueCollection, CsdlEnumMember>(this.OnEnumMemberElement), xmlElementParserArray25);
			xmlElementParserArray24[2] = xmlElementParser32;
			xmlElementParserArray24[3] = xmlElementParser33;
			xmlElementParserArray13[5] = base.CsdlElement<CsdlEnumType>("EnumType", new Func<XmlElementInfo, XmlElementValueCollection, CsdlEnumType>(this.OnEnumTypeElement), xmlElementParserArray24);
			XmlElementParser[] xmlElementParserArray26 = xmlElementParserArray13;
			int num2 = 6;
			CsdlDocumentParser csdlDocumentParser4 = this;
			string str4 = "Function";
			Func<XmlElementInfo, XmlElementValueCollection, CsdlFunction> func2 = new Func<XmlElementInfo, XmlElementValueCollection, CsdlFunction>(this.OnFunctionElement);
			XmlElementParser[] xmlElementParserArray27 = new XmlElementParser[6];
			xmlElementParserArray27[0] = xmlElementParser;
			XmlElementParser[] xmlElementParserArray28 = new XmlElementParser[7];
			xmlElementParserArray28[0] = xmlElementParser;
			xmlElementParserArray = new XmlElementParser[1];
			xmlElementParserArray[0] = xmlElementParser;
			xmlElementParserArray28[1] = base.CsdlElement<CsdlTypeReference>("TypeRef", new Func<XmlElementInfo, XmlElementValueCollection, CsdlTypeReference>(this.OnTypeRefElement), xmlElementParserArray);
			xmlElementParserArray28[2] = xmlElementParser2;
			xmlElementParserArray28[3] = xmlElementParser3;
			xmlElementParserArray28[4] = xmlElementParser1;
			xmlElementParserArray28[5] = xmlElementParser32;
			xmlElementParserArray28[6] = xmlElementParser33;
			xmlElementParserArray27[1] = base.CsdlElement<CsdlFunctionParameter>("Parameter", new Func<XmlElementInfo, XmlElementValueCollection, CsdlFunctionParameter>(this.OnParameterElement), xmlElementParserArray28);
			XmlElementParser[] xmlElementParserArray29 = xmlElementParserArray27;
			int num3 = 2;
			CsdlDocumentParser csdlDocumentParser5 = this;
			string str5 = "DefiningExpression";
			xmlElementParserArray29[num3] = csdlDocumentParser5.Element<string>(str5, (XmlElementInfo element, XmlElementValueCollection children) => children.FirstText.Value, new XmlElementParser[0]);
			xmlElementParserArray = new XmlElementParser[5];
			xmlElementParserArray[0] = xmlElementParser;
			xmlElementParserArray3 = new XmlElementParser[1];
			xmlElementParserArray3[0] = xmlElementParser;
			xmlElementParserArray[1] = base.CsdlElement<CsdlTypeReference>("TypeRef", new Func<XmlElementInfo, XmlElementValueCollection, CsdlTypeReference>(this.OnTypeRefElement), xmlElementParserArray3);
			xmlElementParserArray[2] = xmlElementParser2;
			xmlElementParserArray[3] = xmlElementParser3;
			xmlElementParserArray[4] = xmlElementParser1;
			xmlElementParserArray27[3] = base.CsdlElement<CsdlFunctionReturnType>("ReturnType", new Func<XmlElementInfo, XmlElementValueCollection, CsdlFunctionReturnType>(this.OnReturnTypeElement), xmlElementParserArray);
			xmlElementParserArray27[4] = xmlElementParser32;
			xmlElementParserArray27[5] = xmlElementParser33;
			xmlElementParserArray26[num2] = csdlDocumentParser4.CsdlElement<CsdlFunction>(str4, func2, xmlElementParserArray27);
			xmlElementParserArray = new XmlElementParser[6];
			xmlElementParserArray3 = new XmlElementParser[1];
			xmlElementParserArray3[0] = xmlElementParser;
			xmlElementParserArray[0] = base.CsdlElement<CsdlTypeReference>("TypeRef", new Func<XmlElementInfo, XmlElementValueCollection, CsdlTypeReference>(this.OnTypeRefElement), xmlElementParserArray3);
			xmlElementParserArray[1] = xmlElementParser2;
			xmlElementParserArray[2] = xmlElementParser3;
			xmlElementParserArray[3] = xmlElementParser1;
			xmlElementParserArray[4] = xmlElementParser32;
			xmlElementParserArray[5] = xmlElementParser33;
			xmlElementParserArray13[7] = base.CsdlElement<CsdlValueTerm>("ValueTerm", new Func<XmlElementInfo, XmlElementValueCollection, CsdlValueTerm>(this.OnValueTermElement), xmlElementParserArray);
			xmlElementParserArray = new XmlElementParser[2];
			xmlElementParserArray[0] = xmlElementParser32;
			xmlElementParserArray[1] = xmlElementParser33;
			xmlElementParserArray13[8] = base.CsdlElement<CsdlAnnotations>("Annotations", new Func<XmlElementInfo, XmlElementValueCollection, CsdlAnnotations>(this.OnAnnotationsElement), xmlElementParserArray);
			xmlElementParserArray = new XmlElementParser[6];
			xmlElementParserArray[0] = xmlElementParser;
			xmlElementParserArray3 = new XmlElementParser[3];
			xmlElementParserArray3[0] = xmlElementParser;
			xmlElementParserArray3[1] = xmlElementParser32;
			xmlElementParserArray3[2] = xmlElementParser33;
			xmlElementParserArray[1] = base.CsdlElement<CsdlEntitySet>("EntitySet", new Func<XmlElementInfo, XmlElementValueCollection, CsdlEntitySet>(this.OnEntitySetElement), xmlElementParserArray3);
			xmlElementParserArray3 = new XmlElementParser[2];
			xmlElementParserArray3[0] = xmlElementParser;
			xmlElementParserArray4 = new XmlElementParser[1];
			xmlElementParserArray4[0] = xmlElementParser;
			xmlElementParserArray3[1] = base.CsdlElement<CsdlAssociationSetEnd>("End", new Func<XmlElementInfo, XmlElementValueCollection, CsdlAssociationSetEnd>(this.OnAssociationSetEndElement), xmlElementParserArray4);
			xmlElementParserArray[2] = base.CsdlElement<CsdlAssociationSet>("AssociationSet", new Func<XmlElementInfo, XmlElementValueCollection, CsdlAssociationSet>(this.OnAssociationSetElement), xmlElementParserArray3);
			xmlElementParserArray3 = new XmlElementParser[4];
			xmlElementParserArray3[0] = xmlElementParser;
			xmlElementParserArray4 = new XmlElementParser[3];
			xmlElementParserArray4[0] = xmlElementParser;
			xmlElementParserArray4[1] = xmlElementParser32;
			xmlElementParserArray4[2] = xmlElementParser33;
			xmlElementParserArray3[1] = base.CsdlElement<CsdlFunctionParameter>("Parameter", new Func<XmlElementInfo, XmlElementValueCollection, CsdlFunctionParameter>(this.OnFunctionImportParameterElement), xmlElementParserArray4);
			xmlElementParserArray3[2] = xmlElementParser32;
			xmlElementParserArray3[3] = xmlElementParser33;
			xmlElementParserArray[3] = base.CsdlElement<CsdlFunctionImport>("FunctionImport", new Func<XmlElementInfo, XmlElementValueCollection, CsdlFunctionImport>(this.OnFunctionImportElement), xmlElementParserArray3);
			xmlElementParserArray[4] = xmlElementParser32;
			xmlElementParserArray[5] = xmlElementParser33;
			xmlElementParserArray13[9] = base.CsdlElement<CsdlEntityContainer>("EntityContainer", new Func<XmlElementInfo, XmlElementValueCollection, CsdlEntityContainer>(this.OnEntityContainerElement), xmlElementParserArray);
			XmlElementParser<CsdlSchema> xmlElementParser34 = csdlDocumentParser3.CsdlElement<CsdlSchema>(str3, func1, xmlElementParserArray13);
			return xmlElementParser34;
		}

		private static CsdlDocumentation Documentation(XmlElementValueCollection childValues)
		{
			return childValues.ValuesOfType<CsdlDocumentation>().FirstOrDefault<CsdlDocumentation>();
		}

		private CsdlReferentialConstraintRole GetConstraintRole(XmlElementValueCollection childValues, string roleElementName, Func<string> improperNumberMessage)
		{
			IEnumerable<XmlElementValue<CsdlReferentialConstraintRole>> array = childValues.FindByName<CsdlReferentialConstraintRole>(roleElementName).ToArray<XmlElementValue<CsdlReferentialConstraintRole>>();
			if (array.Count<XmlElementValue<CsdlReferentialConstraintRole>>() > 1)
			{
				base.ReportError(array.ElementAt<XmlElementValue<CsdlReferentialConstraintRole>>(1).Location, EdmErrorCode.InvalidAssociation, improperNumberMessage());
			}
			XmlElementValue<CsdlReferentialConstraintRole> xmlElementValue = array.FirstOrDefault<XmlElementValue<CsdlReferentialConstraintRole>>();
			if (xmlElementValue != null)
			{
				return xmlElementValue.Value;
			}
			else
			{
				return null;
			}
		}

		private CsdlAnnotations OnAnnotationsElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Target");
			string str1 = base.Optional("Qualifier");
			IEnumerable<CsdlVocabularyAnnotationBase> csdlVocabularyAnnotationBases = childValues.ValuesOfType<CsdlVocabularyAnnotationBase>();
			return new CsdlAnnotations(csdlVocabularyAnnotationBases, str, str1);
		}

		private CsdlApplyExpression OnApplyElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Optional("Function");
			IEnumerable<CsdlExpressionBase> csdlExpressionBases = childValues.ValuesOfType<CsdlExpressionBase>();
			return new CsdlApplyExpression(str, csdlExpressionBases, element.Location);
		}

		private CsdlExpressionBase OnAssertTypeExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.OptionalType("Type");
			CsdlTypeReference csdlTypeReference = this.ParseTypeReference(str, childValues, element.Location, CsdlDocumentParser.Optionality.Required);
			IEnumerable<CsdlExpressionBase> csdlExpressionBases = childValues.ValuesOfType<CsdlExpressionBase>();
			if (csdlExpressionBases.Count<CsdlExpressionBase>() != 1)
			{
				base.ReportError(element.Location, EdmErrorCode.InvalidAssertTypeExpressionIncorrectNumberOfOperands, Strings.CsdlParser_InvalidAssertTypeExpressionIncorrectNumberOfOperands);
			}
			return new CsdlAssertTypeExpression(csdlTypeReference, csdlExpressionBases.ElementAtOrDefault<CsdlExpressionBase>(0), element.Location);
		}

		private CsdlAssociation OnAssociationElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Name");
			IEnumerable<CsdlAssociationEnd> csdlAssociationEnds = childValues.ValuesOfType<CsdlAssociationEnd>();
			if (csdlAssociationEnds.Count<CsdlAssociationEnd>() != 2)
			{
				base.ReportError(element.Location, EdmErrorCode.InvalidAssociation, Strings.CsdlParser_InvalidAssociationIncorrectNumberOfEnds(str));
			}
			IEnumerable<CsdlReferentialConstraint> csdlReferentialConstraints = childValues.ValuesOfType<CsdlReferentialConstraint>();
			if (csdlReferentialConstraints.Count<CsdlReferentialConstraint>() > 1)
			{
				base.ReportError(childValues.OfResultType<CsdlReferentialConstraint>().ElementAt<XmlElementValue<CsdlReferentialConstraint>>(1).Location, EdmErrorCode.InvalidAssociation, Strings.CsdlParser_AssociationHasAtMostOneConstraint);
			}
			return new CsdlAssociation(str, csdlAssociationEnds.ElementAtOrDefault<CsdlAssociationEnd>(0), csdlAssociationEnds.ElementAtOrDefault<CsdlAssociationEnd>(1), csdlReferentialConstraints.FirstOrDefault<CsdlReferentialConstraint>(), CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlAssociationEnd OnAssociationEndElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			bool flag;
			string str = base.RequiredType("Type");
			string str1 = base.Optional("Role");
			EdmMultiplicity edmMultiplicity = base.RequiredMultiplicity("Multiplicity");
			CsdlOnDelete csdlOnDelete = childValues.ValuesOfType<CsdlOnDelete>().FirstOrDefault<CsdlOnDelete>();
			EdmMultiplicity edmMultiplicity1 = edmMultiplicity;
			switch (edmMultiplicity1)
			{
				case EdmMultiplicity.One:
				case EdmMultiplicity.Many:
				{
					flag = false;
					break;
				}
				default:
				{
					flag = true;
					break;
				}
			}
			CsdlNamedTypeReference csdlNamedTypeReference = new CsdlNamedTypeReference(str, flag, element.Location);
			return new CsdlAssociationEnd(str1, csdlNamedTypeReference, edmMultiplicity, csdlOnDelete, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlAssociationSet OnAssociationSetElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Name");
			string str1 = base.RequiredQualifiedName("Association");
			IEnumerable<CsdlAssociationSetEnd> csdlAssociationSetEnds = childValues.ValuesOfType<CsdlAssociationSetEnd>();
			if (csdlAssociationSetEnds.Count<CsdlAssociationSetEnd>() > 2)
			{
				base.ReportError(element.Location, EdmErrorCode.InvalidAssociationSet, Strings.CsdlParser_InvalidAssociationSetIncorrectNumberOfEnds(str));
			}
			return new CsdlAssociationSet(str, str1, csdlAssociationSetEnds.ElementAtOrDefault<CsdlAssociationSetEnd>(0), csdlAssociationSetEnds.ElementAtOrDefault<CsdlAssociationSetEnd>(1), CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlAssociationSetEnd OnAssociationSetEndElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Role");
			string str1 = base.Required("EntitySet");
			return new CsdlAssociationSetEnd(str, str1, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private static CsdlConstantExpression OnBinaryConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return CsdlDocumentParser.ConstantExpression(EdmValueKind.Binary, childValues, element.Location);
		}

		private static CsdlConstantExpression OnBoolConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return CsdlDocumentParser.ConstantExpression(EdmValueKind.Boolean, childValues, element.Location);
		}

		private CsdlCollectionExpression OnCollectionElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.OptionalType("Type");
			CsdlTypeReference csdlTypeReference = this.ParseTypeReference(str, childValues, element.Location, CsdlDocumentParser.Optionality.Optional);
			IEnumerable<CsdlExpressionBase> csdlExpressionBases = childValues.ValuesOfType<CsdlExpressionBase>();
			return new CsdlCollectionExpression(csdlTypeReference, csdlExpressionBases, element.Location);
		}

		private CsdlTypeReference OnCollectionTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.OptionalType("ElementType");
			CsdlTypeReference csdlTypeReference = this.ParseTypeReference(str, childValues, element.Location, CsdlDocumentParser.Optionality.Required);
			return new CsdlExpressionTypeReference(new CsdlCollectionType(csdlTypeReference, element.Location), false, element.Location);
		}

		private CsdlComplexType OnComplexTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			bool valueOrDefault;
			string str = base.Required("Name");
			string str1 = base.OptionalQualifiedName("BaseType");
			bool? nullable = base.OptionalBoolean("Abstract");
			if (nullable.HasValue)
			{
				valueOrDefault = nullable.GetValueOrDefault();
			}
			else
			{
				valueOrDefault = false;
			}
			bool flag = valueOrDefault;
			return new CsdlComplexType(str, str1, flag, childValues.ValuesOfType<CsdlProperty>(), CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private static CsdlConstantExpression OnDateTimeConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return CsdlDocumentParser.ConstantExpression(EdmValueKind.DateTime, childValues, element.Location);
		}

		private static CsdlConstantExpression OnDateTimeOffsetConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return CsdlDocumentParser.ConstantExpression(EdmValueKind.DateTimeOffset, childValues, element.Location);
		}

		private static CsdlConstantExpression OnDecimalConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return CsdlDocumentParser.ConstantExpression(EdmValueKind.Decimal, childValues, element.Location);
		}

		private CsdlOnDelete OnDeleteActionElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			EdmOnDeleteAction edmOnDeleteAction = base.RequiredOnDeleteAction("Action");
			return new CsdlOnDelete(edmOnDeleteAction, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlDocumentation OnDocumentationElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return new CsdlDocumentation(childValues["Summary"].TextValue, childValues["LongDescription"].TextValue, element.Location);
		}

		private CsdlEntityContainer OnEntityContainerElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Name");
			string str1 = base.Optional("Extends");
			return new CsdlEntityContainer(str, str1, childValues.ValuesOfType<CsdlEntitySet>(), childValues.ValuesOfType<CsdlAssociationSet>(), childValues.ValuesOfType<CsdlFunctionImport>(), CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private static CsdlKey OnEntityKeyElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return new CsdlKey(new List<CsdlPropertyReference>(childValues.ValuesOfType<CsdlPropertyReference>()), element.Location);
		}

		private CsdlTypeReference OnEntityReferenceTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.RequiredType("Type");
			return new CsdlExpressionTypeReference(new CsdlEntityReferenceType(this.ParseTypeReference(str, null, element.Location, CsdlDocumentParser.Optionality.Required), element.Location), true, element.Location);
		}

		private CsdlEntitySet OnEntitySetElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Name");
			string str1 = base.RequiredQualifiedName("EntityType");
			return new CsdlEntitySet(str, str1, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlEntitySetReferenceExpression OnEntitySetReferenceExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.RequiredEntitySetPath("Name");
			return new CsdlEntitySetReferenceExpression(str, element.Location);
		}

		private CsdlEntityType OnEntityTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			bool valueOrDefault;
			bool flag;
			string str = base.Required("Name");
			string str1 = base.OptionalQualifiedName("BaseType");
			bool? nullable = base.OptionalBoolean("OpenType");
			if (nullable.HasValue)
			{
				valueOrDefault = nullable.GetValueOrDefault();
			}
			else
			{
				valueOrDefault = false;
			}
			bool flag1 = valueOrDefault;
			bool? nullable1 = base.OptionalBoolean("Abstract");
			if (nullable1.HasValue)
			{
				flag = nullable1.GetValueOrDefault();
			}
			else
			{
				flag = false;
			}
			bool flag2 = flag;
			CsdlKey csdlKey = childValues.ValuesOfType<CsdlKey>().FirstOrDefault<CsdlKey>();
			return new CsdlEntityType(str, str1, flag2, flag1, csdlKey, childValues.ValuesOfType<CsdlProperty>(), childValues.ValuesOfType<CsdlNavigationProperty>(), CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlEnumMember OnEnumMemberElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Name");
			long? nullable = base.OptionalLong("Value");
			return new CsdlEnumMember(str, nullable, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlEnumMemberReferenceExpression OnEnumMemberReferenceExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.RequiredEnumMemberPath("Name");
			return new CsdlEnumMemberReferenceExpression(str, element.Location);
		}

		private CsdlEnumType OnEnumTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			bool valueOrDefault;
			string str = base.Required("Name");
			string str1 = base.OptionalType("UnderlyingType");
			bool? nullable = base.OptionalBoolean("IsFlags");
			string str2 = str;
			string str3 = str1;
			bool? nullable1 = nullable;
			if (nullable1.HasValue)
			{
				valueOrDefault = nullable1.GetValueOrDefault();
			}
			else
			{
				valueOrDefault = false;
			}
			return new CsdlEnumType(str2, str3, valueOrDefault, childValues.ValuesOfType<CsdlEnumMember>(), CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private static CsdlConstantExpression OnFloatConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return CsdlDocumentParser.ConstantExpression(EdmValueKind.Floating, childValues, element.Location);
		}

		private CsdlFunction OnFunctionElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Name");
			string str1 = base.OptionalType("ReturnType");
			IEnumerable<CsdlFunctionParameter> csdlFunctionParameters = childValues.ValuesOfType<CsdlFunctionParameter>();
			XmlElementValue item = childValues["DefiningExpression"];
			string str2 = null;
			if (item as XmlElementValueCollection.MissingXmlElementValue == null)
			{
				string textValue = item.TextValue;
				string empty = textValue;
				if (textValue == null)
				{
					empty = string.Empty;
				}
				str2 = empty;
			}
			CsdlTypeReference returnType = null;
			if (str1 != null)
			{
				returnType = this.ParseTypeReference(str1, null, element.Location, CsdlDocumentParser.Optionality.Required);
			}
			else
			{
				CsdlFunctionReturnType csdlFunctionReturnType = childValues.ValuesOfType<CsdlFunctionReturnType>().FirstOrDefault<CsdlFunctionReturnType>();
				if (csdlFunctionReturnType != null)
				{
					returnType = csdlFunctionReturnType.ReturnType;
				}
			}
			return new CsdlFunction(str, csdlFunctionParameters, str2, returnType, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlFunctionImport OnFunctionImportElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			bool valueOrDefault;
			bool flag;
			bool valueOrDefault1;
			string str = base.Required("Name");
			string str1 = base.OptionalType("ReturnType");
			CsdlTypeReference csdlTypeReference = this.ParseTypeReference(str1, childValues, element.Location, CsdlDocumentParser.Optionality.Optional);
			bool? nullable = base.OptionalBoolean("IsComposable");
			if (nullable.HasValue)
			{
				valueOrDefault = nullable.GetValueOrDefault();
			}
			else
			{
				valueOrDefault = false;
			}
			bool flag1 = valueOrDefault;
			bool? nullable1 = base.OptionalBoolean("IsSideEffecting");
			if (nullable1.HasValue)
			{
				flag = nullable1.GetValueOrDefault();
			}
			else
			{
				if (flag1)
				{
					flag = false;
				}
				else
				{
					flag = true;
				}
			}
			bool flag2 = flag;
			bool? nullable2 = base.OptionalBoolean("IsBindable");
			if (nullable2.HasValue)
			{
				valueOrDefault1 = nullable2.GetValueOrDefault();
			}
			else
			{
				valueOrDefault1 = false;
			}
			bool flag3 = valueOrDefault1;
			string str2 = base.Optional("EntitySet");
			string str3 = base.Optional("EntitySetPath");
			IEnumerable<CsdlFunctionParameter> csdlFunctionParameters = childValues.ValuesOfType<CsdlFunctionParameter>();
			return new CsdlFunctionImport(str, flag2, flag1, flag3, str2, str3, csdlFunctionParameters, csdlTypeReference, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlFunctionParameter OnFunctionImportParameterElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			EdmFunctionParameterMode valueOrDefault;
			string str = base.Required("Name");
			string str1 = base.RequiredType("Type");
			EdmFunctionParameterMode? nullable = base.OptionalFunctionParameterMode("Mode");
			CsdlTypeReference csdlTypeReference = this.ParseTypeReference(str1, null, element.Location, CsdlDocumentParser.Optionality.Required);
			string str2 = str;
			CsdlTypeReference csdlTypeReference1 = csdlTypeReference;
			EdmFunctionParameterMode? nullable1 = nullable;
			if (nullable1.HasValue)
			{
				valueOrDefault = nullable1.GetValueOrDefault();
			}
			else
			{
				valueOrDefault = EdmFunctionParameterMode.In;
			}
			return new CsdlFunctionParameter(str2, csdlTypeReference1, valueOrDefault, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlFunctionReferenceExpression OnFunctionReferenceExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.RequiredQualifiedName("Name");
			return new CsdlFunctionReferenceExpression(str, element.Location);
		}

		private static CsdlConstantExpression OnGuidConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return CsdlDocumentParser.ConstantExpression(EdmValueKind.Guid, childValues, element.Location);
		}

		private CsdlExpressionBase OnIfExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			IEnumerable<CsdlExpressionBase> csdlExpressionBases = childValues.ValuesOfType<CsdlExpressionBase>();
			if (csdlExpressionBases.Count<CsdlExpressionBase>() != 3)
			{
				base.ReportError(element.Location, EdmErrorCode.InvalidIfExpressionIncorrectNumberOfOperands, Strings.CsdlParser_InvalidIfExpressionIncorrectNumberOfOperands);
			}
			return new CsdlIfExpression(csdlExpressionBases.ElementAtOrDefault<CsdlExpressionBase>(0), csdlExpressionBases.ElementAtOrDefault<CsdlExpressionBase>(1), csdlExpressionBases.ElementAtOrDefault<CsdlExpressionBase>(2), element.Location);
		}

		private static CsdlConstantExpression OnIntConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return CsdlDocumentParser.ConstantExpression(EdmValueKind.Integer, childValues, element.Location);
		}

		private CsdlExpressionBase OnIsTypeExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.OptionalType("Type");
			CsdlTypeReference csdlTypeReference = this.ParseTypeReference(str, childValues, element.Location, CsdlDocumentParser.Optionality.Required);
			IEnumerable<CsdlExpressionBase> csdlExpressionBases = childValues.ValuesOfType<CsdlExpressionBase>();
			if (csdlExpressionBases.Count<CsdlExpressionBase>() != 1)
			{
				base.ReportError(element.Location, EdmErrorCode.InvalidIsTypeExpressionIncorrectNumberOfOperands, Strings.CsdlParser_InvalidIsTypeExpressionIncorrectNumberOfOperands);
			}
			return new CsdlIsTypeExpression(csdlTypeReference, csdlExpressionBases.ElementAtOrDefault<CsdlExpressionBase>(0), element.Location);
		}

		private CsdlLabeledExpression OnLabeledElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Name");
			IEnumerable<CsdlExpressionBase> csdlExpressionBases = childValues.ValuesOfType<CsdlExpressionBase>();
			if (csdlExpressionBases.Count<CsdlExpressionBase>() != 1)
			{
				base.ReportError(element.Location, EdmErrorCode.InvalidLabeledElementExpressionIncorrectNumberOfOperands, Strings.CsdlParser_InvalidLabeledElementExpressionIncorrectNumberOfOperands);
			}
			return new CsdlLabeledExpression(str, csdlExpressionBases.ElementAtOrDefault<CsdlExpressionBase>(0), element.Location);
		}

		private CsdlLabeledExpressionReferenceExpression OnLabeledElementReferenceExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Name");
			return new CsdlLabeledExpressionReferenceExpression(str, element.Location);
		}

		private CsdlNavigationProperty OnNavigationPropertyElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			bool valueOrDefault;
			string str = base.Required("Name");
			string str1 = base.Required("Relationship");
			string str2 = base.Required("ToRole");
			string str3 = base.Required("FromRole");
			bool? nullable = base.OptionalBoolean("ContainsTarget");
			string str4 = str;
			string str5 = str1;
			string str6 = str2;
			string str7 = str3;
			bool? nullable1 = nullable;
			if (nullable1.HasValue)
			{
				valueOrDefault = nullable1.GetValueOrDefault();
			}
			else
			{
				valueOrDefault = false;
			}
			return new CsdlNavigationProperty(str4, str5, str6, str7, valueOrDefault, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private static CsdlConstantExpression OnNullConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return CsdlDocumentParser.ConstantExpression(EdmValueKind.Null, childValues, element.Location);
		}

		private CsdlFunctionParameter OnParameterElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Name");
			string str1 = base.OptionalType("Type");
			CsdlTypeReference csdlTypeReference = this.ParseTypeReference(str1, childValues, element.Location, CsdlDocumentParser.Optionality.Required);
			return new CsdlFunctionParameter(str, csdlTypeReference, EdmFunctionParameterMode.In, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlParameterReferenceExpression OnParameterReferenceExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Name");
			return new CsdlParameterReferenceExpression(str, element.Location);
		}

		private static CsdlPathExpression OnPathExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string textValue;
			XmlTextValue firstText = childValues.FirstText;
			if (firstText != null)
			{
				textValue = firstText.TextValue;
			}
			else
			{
				textValue = string.Empty;
			}
			return new CsdlPathExpression(textValue, element.Location);
		}

		private CsdlProperty OnPropertyElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			int valueOrDefault;
			string str = base.OptionalType("Type");
			CsdlTypeReference csdlTypeReference = this.ParseTypeReference(str, childValues, element.Location, CsdlDocumentParser.Optionality.Required);
			string str1 = base.Required("Name");
			string str2 = base.Optional("DefaultValue");
			EdmConcurrencyMode? nullable = base.OptionalConcurrencyMode("ConcurrencyMode");
			EdmConcurrencyMode? nullable1 = nullable;
			if (nullable1.HasValue)
			{
				valueOrDefault = (int)nullable1.GetValueOrDefault();
			}
			else
			{
				valueOrDefault = 0;
			}
			bool flag = valueOrDefault == 1;
			return new CsdlProperty(str1, csdlTypeReference, flag, str2, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlPropertyReference OnPropertyRefElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return new CsdlPropertyReference(base.Required("Name"), element.Location);
		}

		private CsdlPropertyReferenceExpression OnPropertyReferenceExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Name");
			return new CsdlPropertyReferenceExpression(str, childValues.ValuesOfType<CsdlExpressionBase>().FirstOrDefault<CsdlExpressionBase>(), element.Location);
		}

		private CsdlPropertyValue OnPropertyValueElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Property");
			CsdlExpressionBase csdlExpressionBase = this.ParseAnnotationExpression(element, childValues);
			return new CsdlPropertyValue(str, csdlExpressionBase, element.Location);
		}

		private CsdlRecordExpression OnRecordElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			CsdlTypeReference csdlNamedTypeReference;
			string str = base.OptionalQualifiedName("Type");
			IEnumerable<CsdlPropertyValue> csdlPropertyValues = childValues.ValuesOfType<CsdlPropertyValue>();
			if (str != null)
			{
				csdlNamedTypeReference = new CsdlNamedTypeReference(str, false, element.Location);
			}
			else
			{
				csdlNamedTypeReference = null;
			}
			return new CsdlRecordExpression(csdlNamedTypeReference, csdlPropertyValues, element.Location);
		}

		private CsdlReferentialConstraint OnReferentialConstraintElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			CsdlDocumentParser csdlDocumentParser = this;
			XmlElementValueCollection xmlElementValueCollections = childValues;
			string str = "Principal";
			CsdlReferentialConstraintRole constraintRole = csdlDocumentParser.GetConstraintRole(xmlElementValueCollections, str, () => Strings.CsdlParser_ReferentialConstraintRequiresOnePrincipal);
			CsdlDocumentParser csdlDocumentParser1 = this;
			XmlElementValueCollection xmlElementValueCollections1 = childValues;
			string str1 = "Dependent";
			CsdlReferentialConstraintRole csdlReferentialConstraintRole = csdlDocumentParser1.GetConstraintRole(xmlElementValueCollections1, str1, () => Strings.CsdlParser_ReferentialConstraintRequiresOneDependent);
			return new CsdlReferentialConstraint(constraintRole, csdlReferentialConstraintRole, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlReferentialConstraintRole OnReferentialConstraintRoleElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Role");
			IEnumerable<CsdlPropertyReference> csdlPropertyReferences = childValues.ValuesOfType<CsdlPropertyReference>();
			return new CsdlReferentialConstraintRole(str, csdlPropertyReferences, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlFunctionReturnType OnReturnTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.OptionalType("Type");
			CsdlTypeReference csdlTypeReference = this.ParseTypeReference(str, childValues, element.Location, CsdlDocumentParser.Optionality.Required);
			return new CsdlFunctionReturnType(csdlTypeReference, element.Location);
		}

		private CsdlTypeReference OnRowTypeElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return new CsdlExpressionTypeReference(new CsdlRowType(childValues.ValuesOfType<CsdlProperty>(), element.Location), true, element.Location);
		}

		private CsdlSchema OnSchemaElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Optional("Namespace");
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			string str1 = empty;
			string str2 = base.OptionalAlias("Alias");
			CsdlSchema csdlSchema = new CsdlSchema(str1, str2, this.artifactVersion, childValues.ValuesOfType<CsdlUsing>(), childValues.ValuesOfType<CsdlAssociation>(), childValues.ValuesOfType<CsdlStructuredType>(), childValues.ValuesOfType<CsdlEnumType>(), childValues.ValuesOfType<CsdlFunction>(), childValues.ValuesOfType<CsdlValueTerm>(), childValues.ValuesOfType<CsdlEntityContainer>(), childValues.ValuesOfType<CsdlAnnotations>(), CsdlDocumentParser.Documentation(childValues), element.Location);
			return csdlSchema;
		}

		private static CsdlConstantExpression OnStringConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return CsdlDocumentParser.ConstantExpression(EdmValueKind.String, childValues, element.Location);
		}

		private static CsdlConstantExpression OnTimeConstantExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			return CsdlDocumentParser.ConstantExpression(EdmValueKind.Time, childValues, element.Location);
		}

		private CsdlTypeAnnotation OnTypeAnnotationElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.RequiredQualifiedName("Term");
			string str1 = base.Optional("Qualifier");
			IEnumerable<CsdlPropertyValue> csdlPropertyValues = childValues.ValuesOfType<CsdlPropertyValue>();
			return new CsdlTypeAnnotation(str, str1, csdlPropertyValues, element.Location);
		}

		private CsdlTypeReference OnTypeRefElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.RequiredType("Type");
			return this.ParseTypeReference(str, null, element.Location, CsdlDocumentParser.Optionality.Required);
		}

		private CsdlUsing OnUsingElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.Required("Namespace");
			string str1 = base.RequiredAlias("Alias");
			return new CsdlUsing(str, str1, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlValueAnnotation OnValueAnnotationElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.RequiredQualifiedName("Term");
			string str1 = base.Optional("Qualifier");
			CsdlExpressionBase csdlExpressionBase = this.ParseAnnotationExpression(element, childValues);
			return new CsdlValueAnnotation(str, str1, csdlExpressionBase, element.Location);
		}

		private CsdlValueTerm OnValueTermElement(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			string str = base.OptionalType("Type");
			CsdlTypeReference csdlTypeReference = this.ParseTypeReference(str, childValues, element.Location, CsdlDocumentParser.Optionality.Required);
			string str1 = base.Required("Name");
			return new CsdlValueTerm(str1, csdlTypeReference, CsdlDocumentParser.Documentation(childValues), element.Location);
		}

		private CsdlExpressionBase ParseAnnotationExpression(XmlElementInfo element, XmlElementValueCollection childValues)
		{
			EdmValueKind edmValueKind;
			CsdlExpressionBase csdlExpressionBase = childValues.ValuesOfType<CsdlExpressionBase>().FirstOrDefault<CsdlExpressionBase>();
			if (csdlExpressionBase == null)
			{
				string str = base.Optional("Path");
				if (str == null)
				{
					string str1 = base.Optional("String");
					if (str1 == null)
					{
						str1 = base.Optional("Bool");
						if (str1 == null)
						{
							str1 = base.Optional("Int");
							if (str1 == null)
							{
								str1 = base.Optional("Float");
								if (str1 == null)
								{
									str1 = base.Optional("DateTime");
									if (str1 == null)
									{
										str1 = base.Optional("DateTimeOffset");
										if (str1 == null)
										{
											str1 = base.Optional("Time");
											if (str1 == null)
											{
												str1 = base.Optional("Decimal");
												if (str1 == null)
												{
													str1 = base.Optional("Binary");
													if (str1 == null)
													{
														str1 = base.Optional("Guid");
														if (str1 == null)
														{
															return null;
														}
														else
														{
															edmValueKind = EdmValueKind.Guid;
														}
													}
													else
													{
														edmValueKind = EdmValueKind.Binary;
													}
												}
												else
												{
													edmValueKind = EdmValueKind.Decimal;
												}
											}
											else
											{
												edmValueKind = EdmValueKind.Time;
											}
										}
										else
										{
											edmValueKind = EdmValueKind.DateTimeOffset;
										}
									}
									else
									{
										edmValueKind = EdmValueKind.DateTime;
									}
								}
								else
								{
									edmValueKind = EdmValueKind.Floating;
								}
							}
							else
							{
								edmValueKind = EdmValueKind.Integer;
							}
						}
						else
						{
							edmValueKind = EdmValueKind.Boolean;
						}
					}
					else
					{
						edmValueKind = EdmValueKind.String;
					}
					return new CsdlConstantExpression(edmValueKind, str1, element.Location);
				}
				else
				{
					return new CsdlPathExpression(str, element.Location);
				}
			}
			else
			{
				return csdlExpressionBase;
			}
		}

		private void ParseBinaryFacets(out bool Unbounded, out int? maxLength, out bool? fixedLength)
		{
			this.ParseMaxLength(out Unbounded, out maxLength);
			fixedLength = base.OptionalBoolean("FixedLength");
		}

		private void ParseDecimalFacets(out int? precision, out int? scale)
		{
			precision = base.OptionalInteger("Precision");
			scale = base.OptionalInteger("Scale");
		}

		private void ParseMaxLength(out bool Unbounded, out int? maxLength)
		{
			string str = base.Optional("MaxLength");
			if (str != null)
			{
				if (!str.EqualsOrdinalIgnoreCase("Max"))
				{
					Unbounded = false;
					maxLength = base.OptionalMaxLength("MaxLength");
					return;
				}
				else
				{
					Unbounded = true;
					maxLength = null;
					return;
				}
			}
			else
			{
				Unbounded = false;
				maxLength = null;
				return;
			}
		}

		private CsdlNamedTypeReference ParseNamedTypeReference(string typeName, bool isNullable, CsdlLocation parentLocation)
		{
			int? nullable;
			bool flag = false;
			bool? nullable1;
			int? nullable2;
			int? nullable3;
			int? nullable4;
			int? nullable5;
			bool flag1 = false;
			bool? nullable6;
			bool? nullable7;
			string str = null;
			int? nullable8;
			int? nullable9;
			EdmCoreModel instance = EdmCoreModel.Instance;
			EdmPrimitiveTypeKind primitiveTypeKind = instance.GetPrimitiveTypeKind(typeName);
			EdmPrimitiveTypeKind edmPrimitiveTypeKind = primitiveTypeKind;
			switch (edmPrimitiveTypeKind)
			{
				case EdmPrimitiveTypeKind.Binary:
				{
					this.ParseBinaryFacets(out flag, out nullable, out nullable1);
					return new CsdlBinaryTypeReference(nullable1, flag, nullable, typeName, isNullable, parentLocation);
				}
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
					return new CsdlPrimitiveTypeReference(primitiveTypeKind, typeName, isNullable, parentLocation);
				}
				case EdmPrimitiveTypeKind.DateTime:
				case EdmPrimitiveTypeKind.DateTimeOffset:
				case EdmPrimitiveTypeKind.Time:
				{
					this.ParseTemporalFacets(out nullable2);
					return new CsdlTemporalTypeReference(primitiveTypeKind, nullable2, typeName, isNullable, parentLocation);
				}
				case EdmPrimitiveTypeKind.Decimal:
				{
					this.ParseDecimalFacets(out nullable3, out nullable4);
					return new CsdlDecimalTypeReference(nullable3, nullable4, typeName, isNullable, parentLocation);
				}
				case EdmPrimitiveTypeKind.String:
				{
					this.ParseStringFacets(out flag1, out nullable5, out nullable6, out nullable7, out str);
					return new CsdlStringTypeReference(nullable6, flag1, nullable5, nullable7, str, typeName, isNullable, parentLocation);
				}
				case EdmPrimitiveTypeKind.Geography:
				case EdmPrimitiveTypeKind.GeographyPoint:
				case EdmPrimitiveTypeKind.GeographyLineString:
				case EdmPrimitiveTypeKind.GeographyPolygon:
				case EdmPrimitiveTypeKind.GeographyCollection:
				case EdmPrimitiveTypeKind.GeographyMultiPolygon:
				case EdmPrimitiveTypeKind.GeographyMultiLineString:
				case EdmPrimitiveTypeKind.GeographyMultiPoint:
				{
					this.ParseSpatialFacets(out nullable8, 0x10e6);
					return new CsdlSpatialTypeReference(primitiveTypeKind, nullable8, typeName, isNullable, parentLocation);
				}
				case EdmPrimitiveTypeKind.Geometry:
				case EdmPrimitiveTypeKind.GeometryPoint:
				case EdmPrimitiveTypeKind.GeometryLineString:
				case EdmPrimitiveTypeKind.GeometryPolygon:
				case EdmPrimitiveTypeKind.GeometryCollection:
				case EdmPrimitiveTypeKind.GeometryMultiPolygon:
				case EdmPrimitiveTypeKind.GeometryMultiLineString:
				case EdmPrimitiveTypeKind.GeometryMultiPoint:
				{
					this.ParseSpatialFacets(out nullable9, 0);
					return new CsdlSpatialTypeReference(primitiveTypeKind, nullable9, typeName, isNullable, parentLocation);
				}
			}
			return new CsdlNamedTypeReference(typeName, isNullable, parentLocation);
		}

		private void ParseSpatialFacets(out int? srid, int defaultSrid)
		{
			srid = base.OptionalSrid("SRID", defaultSrid);
		}

		private void ParseStringFacets(out bool Unbounded, out int? maxLength, out bool? fixedLength, out bool? unicode, out string collation)
		{
			this.ParseMaxLength(out Unbounded, out maxLength);
			fixedLength = base.OptionalBoolean("FixedLength");
			unicode = base.OptionalBoolean("Unicode");
			collation = base.Optional("Collation");
		}

		private void ParseTemporalFacets(out int? precision)
		{
			precision = base.OptionalInteger("Precision");
		}

		private CsdlTypeReference ParseTypeReference(string typeString, XmlElementValueCollection childValues, CsdlLocation parentLocation, CsdlDocumentParser.Optionality typeInfoOptionality)
		{
			bool valueOrDefault;
			string str;
			string str1;
			bool? nullable = base.OptionalBoolean("Nullable");
			if (nullable.HasValue)
			{
				valueOrDefault = nullable.GetValueOrDefault();
			}
			else
			{
				valueOrDefault = true;
			}
			bool flag = valueOrDefault;
			CsdlTypeReference csdlExpressionTypeReference = null;
			if (typeString == null)
			{
				if (childValues != null)
				{
					csdlExpressionTypeReference = childValues.ValuesOfType<CsdlTypeReference>().FirstOrDefault<CsdlTypeReference>();
				}
			}
			else
			{
				char[] chrArray = new char[2];
				chrArray[0] = '(';
				chrArray[1] = ')';
				string[] strArrays = typeString.Split(chrArray);
				string str2 = strArrays[0];
				string str3 = str2;
				string str4 = str3;
				if (str3 != null)
				{
					if (str4 == "Collection")
					{
						if (strArrays.Count<string>() > 1)
						{
							str = strArrays[1];
						}
						else
						{
							str = typeString;
						}
						string str5 = str;
						csdlExpressionTypeReference = new CsdlExpressionTypeReference(new CsdlCollectionType(this.ParseNamedTypeReference(str5, flag, parentLocation), parentLocation), false, parentLocation);
						if (csdlExpressionTypeReference == null && typeInfoOptionality == CsdlDocumentParser.Optionality.Required)
						{
							if (childValues != null)
							{
								base.ReportError(parentLocation, EdmErrorCode.MissingType, Strings.CsdlParser_MissingTypeAttributeOrElement);
							}
							csdlExpressionTypeReference = new CsdlNamedTypeReference(string.Empty, flag, parentLocation);
						}
						return csdlExpressionTypeReference;
					}
					else
					{
						if (str4 != "Ref")
						{
							goto Label2;
						}
						if (strArrays.Count<string>() > 1)
						{
							str1 = strArrays[1];
						}
						else
						{
							str1 = typeString;
						}
						string str6 = str1;
						csdlExpressionTypeReference = new CsdlExpressionTypeReference(new CsdlEntityReferenceType(this.ParseNamedTypeReference(str6, flag, parentLocation), parentLocation), true, parentLocation);
						if (csdlExpressionTypeReference == null && typeInfoOptionality == CsdlDocumentParser.Optionality.Required)
						{
							if (childValues != null)
							{
								base.ReportError(parentLocation, EdmErrorCode.MissingType, Strings.CsdlParser_MissingTypeAttributeOrElement);
							}
							csdlExpressionTypeReference = new CsdlNamedTypeReference(string.Empty, flag, parentLocation);
						}
						return csdlExpressionTypeReference;
					}
				}
			Label2:
				csdlExpressionTypeReference = this.ParseNamedTypeReference(str2, flag, parentLocation);
			}
			if (csdlExpressionTypeReference == null && typeInfoOptionality == CsdlDocumentParser.Optionality.Required)
			{
				if (childValues != null)
				{
					base.ReportError(parentLocation, EdmErrorCode.MissingType, Strings.CsdlParser_MissingTypeAttributeOrElement);
				}
				csdlExpressionTypeReference = new CsdlNamedTypeReference(string.Empty, flag, parentLocation);
			}
			return csdlExpressionTypeReference;
		}

		protected override bool TryGetDocumentElementParser(Version csdlArtifactVersion, XmlElementInfo rootElement, out XmlElementParser<CsdlSchema> parser)
		{
			EdmUtil.CheckArgumentNull<XmlElementInfo>(rootElement, "rootElement");
			this.artifactVersion = csdlArtifactVersion;
			if (!string.Equals(rootElement.Name, "Schema", StringComparison.Ordinal))
			{
				parser = null;
				return false;
			}
			else
			{
				parser = this.CreateRootElementParser();
				return true;
			}
		}

		private enum Optionality
		{
			Optional,
			Required
		}
	}
}