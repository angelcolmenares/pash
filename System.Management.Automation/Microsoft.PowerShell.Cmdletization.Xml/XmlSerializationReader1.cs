namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.Xml;
    using System.Xml.Serialization;

    [GeneratedCode("sgen", "4.0")]
    internal class XmlSerializationReader1 : XmlSerializationReader
    {
        private string id1_PowerShellMetadata;
        private string id10_AssociationAssociatedInstance;
        private string id100_AssociatedInstance;
        private string id101_PropertyName;
        private string id102_MinValueQuery;
        private string id103_RegularQuery;
        private string id104_MaxValueQuery;
        private string id105_ExcludeQuery;
        private string id106_QueryableProperties;
        private string id107_QueryableAssociations;
        private string id108_QueryOptions;
        private string id109_CmdletAdapter;
        private string id11_CmdletParameterMetadata;
        private string id110_ClassName;
        private string id111_ClassVersion;
        private string id112_Version;
        private string id113_DefaultNoun;
        private string id114_InstanceCmdlets;
        private string id115_StaticCmdlets;
        private string id116_CmdletAdapterPrivateData;
        private string id117_GetCmdlet;
        private string id118_Class;
        private string id119_Enums;
        private string id12_Item;
        private string id13_Item;
        private string id14_Item;
        private string id15_Item;
        private string id16_Item;
        private string id17_Item;
        private string id18_Item;
        private string id19_QueryOption;
        private string id2_Item;
        private string id20_GetCmdletMetadata;
        private string id21_CommonCmdletMetadata;
        private string id22_ConfirmImpact;
        private string id23_StaticCmdletMetadata;
        private string id24_Item;
        private string id25_CommonMethodMetadata;
        private string id26_StaticMethodMetadata;
        private string id27_CommonMethodParameterMetadata;
        private string id28_StaticMethodParameterMetadata;
        private string id29_CmdletOutputMetadata;
        private string id3_ClassMetadata;
        private string id30_Item;
        private string id31_Item;
        private string id32_InstanceMethodMetadata;
        private string id33_InstanceCmdletMetadata;
        private string id34_PropertyQuery;
        private string id35_WildcardablePropertyQuery;
        private string id36_ItemsChoiceType;
        private string id37_ClassMetadataData;
        private string id38_EnumMetadataEnum;
        private string id39_EnumMetadataEnumValue;
        private string id4_Item;
        private string id40_Name;
        private string id41_Value;
        private string id42_EnumName;
        private string id43_UnderlyingType;
        private string id44_BitwiseFlags;
        private string id45_AllowGlobbing;
        private string id46_IsMandatory;
        private string id47_Aliases;
        private string id48_PSName;
        private string id49_Position;
        private string id5_ClassMetadataInstanceCmdlets;
        private string id50_ValueFromPipeline;
        private string id51_Item;
        private string id52_CmdletParameterSets;
        private string id53_ErrorOnNoMatch;
        private string id54_AllowEmptyCollection;
        private string id55_AllowEmptyString;
        private string id56_AllowNull;
        private string id57_ValidateNotNull;
        private string id58_ValidateNotNullOrEmpty;
        private string id59_ValidateCount;
        private string id6_GetCmdletParameters;
        private string id60_ValidateLength;
        private string id61_ValidateRange;
        private string id62_ValidateSet;
        private string id63_AllowedValue;
        private string id64_Min;
        private string id65_Max;
        private string id66_ArrayOfString;
        private string id67_ArrayOfPropertyMetadata;
        private string id68_Property;
        private string id69_ArrayOfAssociation;
        private string id7_PropertyMetadata;
        private string id70_ArrayOfQueryOption;
        private string id71_Option;
        private string id72_Item;
        private string id73_Parameter;
        private string id74_Item;
        private string id75_ArrayOfStaticCmdletMetadata;
        private string id76_Cmdlet;
        private string id77_ArrayOfClassMetadataData;
        private string id78_Data;
        private string id79_ArrayOfEnumMetadataEnum;
        private string id8_TypeMetadata;
        private string id80_Enum;
        private string id81_CmdletMetadata;
        private string id82_Method;
        private string id83_MethodName;
        private string id84_CmdletParameterSet;
        private string id85_ReturnValue;
        private string id86_Parameters;
        private string id87_ParameterName;
        private string id88_DefaultValue;
        private string id89_Type;
        private string id9_Association;
        private string id90_ErrorCode;
        private string id91_PSType;
        private string id92_ETSType;
        private string id93_Verb;
        private string id94_Noun;
        private string id95_HelpUri;
        private string id96_DefaultCmdletParameterSet;
        private string id97_OptionName;
        private string id98_SourceRole;
        private string id99_ResultRole;

        protected override void InitCallbacks()
        {
        }

        protected override void InitIDs()
        {
            this.id69_ArrayOfAssociation = base.Reader.NameTable.Add("ArrayOfAssociation");
            this.id45_AllowGlobbing = base.Reader.NameTable.Add("AllowGlobbing");
            this.id6_GetCmdletParameters = base.Reader.NameTable.Add("GetCmdletParameters");
            this.id24_Item = base.Reader.NameTable.Add("StaticCmdletMetadataCmdletMetadata");
            this.id61_ValidateRange = base.Reader.NameTable.Add("ValidateRange");
            this.id115_StaticCmdlets = base.Reader.NameTable.Add("StaticCmdlets");
            this.id57_ValidateNotNull = base.Reader.NameTable.Add("ValidateNotNull");
            this.id48_PSName = base.Reader.NameTable.Add("PSName");
            this.id113_DefaultNoun = base.Reader.NameTable.Add("DefaultNoun");
            this.id37_ClassMetadataData = base.Reader.NameTable.Add("ClassMetadataData");
            this.id111_ClassVersion = base.Reader.NameTable.Add("ClassVersion");
            this.id99_ResultRole = base.Reader.NameTable.Add("ResultRole");
            this.id50_ValueFromPipeline = base.Reader.NameTable.Add("ValueFromPipeline");
            this.id102_MinValueQuery = base.Reader.NameTable.Add("MinValueQuery");
            this.id116_CmdletAdapterPrivateData = base.Reader.NameTable.Add("CmdletAdapterPrivateData");
            this.id20_GetCmdletMetadata = base.Reader.NameTable.Add("GetCmdletMetadata");
            this.id117_GetCmdlet = base.Reader.NameTable.Add("GetCmdlet");
            this.id64_Min = base.Reader.NameTable.Add("Min");
            this.id55_AllowEmptyString = base.Reader.NameTable.Add("AllowEmptyString");
            this.id29_CmdletOutputMetadata = base.Reader.NameTable.Add("CmdletOutputMetadata");
            this.id103_RegularQuery = base.Reader.NameTable.Add("RegularQuery");
            this.id71_Option = base.Reader.NameTable.Add("Option");
            this.id72_Item = base.Reader.NameTable.Add("ArrayOfStaticMethodParameterMetadata");
            this.id22_ConfirmImpact = base.Reader.NameTable.Add("ConfirmImpact");
            this.id114_InstanceCmdlets = base.Reader.NameTable.Add("InstanceCmdlets");
            this.id80_Enum = base.Reader.NameTable.Add("Enum");
            this.id39_EnumMetadataEnumValue = base.Reader.NameTable.Add("EnumMetadataEnumValue");
            this.id108_QueryOptions = base.Reader.NameTable.Add("QueryOptions");
            this.id33_InstanceCmdletMetadata = base.Reader.NameTable.Add("InstanceCmdletMetadata");
            this.id59_ValidateCount = base.Reader.NameTable.Add("ValidateCount");
            this.id15_Item = base.Reader.NameTable.Add("CmdletParameterMetadataValidateLength");
            this.id78_Data = base.Reader.NameTable.Add("Data");
            this.id30_Item = base.Reader.NameTable.Add("InstanceMethodParameterMetadata");
            this.id1_PowerShellMetadata = base.Reader.NameTable.Add("PowerShellMetadata");
            this.id95_HelpUri = base.Reader.NameTable.Add("HelpUri");
            this.id88_DefaultValue = base.Reader.NameTable.Add("DefaultValue");
            this.id4_Item = base.Reader.NameTable.Add("");
            this.id31_Item = base.Reader.NameTable.Add("CommonMethodMetadataReturnValue");
            this.id42_EnumName = base.Reader.NameTable.Add("EnumName");
            this.id119_Enums = base.Reader.NameTable.Add("Enums");
            this.id79_ArrayOfEnumMetadataEnum = base.Reader.NameTable.Add("ArrayOfEnumMetadataEnum");
            this.id14_Item = base.Reader.NameTable.Add("CmdletParameterMetadataValidateCount");
            this.id47_Aliases = base.Reader.NameTable.Add("Aliases");
            this.id112_Version = base.Reader.NameTable.Add("Version");
            this.id11_CmdletParameterMetadata = base.Reader.NameTable.Add("CmdletParameterMetadata");
            this.id67_ArrayOfPropertyMetadata = base.Reader.NameTable.Add("ArrayOfPropertyMetadata");
            this.id9_Association = base.Reader.NameTable.Add("Association");
            this.id28_StaticMethodParameterMetadata = base.Reader.NameTable.Add("StaticMethodParameterMetadata");
            this.id94_Noun = base.Reader.NameTable.Add("Noun");
            this.id46_IsMandatory = base.Reader.NameTable.Add("IsMandatory");
            this.id34_PropertyQuery = base.Reader.NameTable.Add("PropertyQuery");
            this.id53_ErrorOnNoMatch = base.Reader.NameTable.Add("ErrorOnNoMatch");
            this.id3_ClassMetadata = base.Reader.NameTable.Add("ClassMetadata");
            this.id74_Item = base.Reader.NameTable.Add("ArrayOfInstanceMethodParameterMetadata");
            this.id2_Item = base.Reader.NameTable.Add("http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            this.id21_CommonCmdletMetadata = base.Reader.NameTable.Add("CommonCmdletMetadata");
            this.id36_ItemsChoiceType = base.Reader.NameTable.Add("ItemsChoiceType");
            this.id35_WildcardablePropertyQuery = base.Reader.NameTable.Add("WildcardablePropertyQuery");
            this.id110_ClassName = base.Reader.NameTable.Add("ClassName");
            this.id63_AllowedValue = base.Reader.NameTable.Add("AllowedValue");
            this.id51_Item = base.Reader.NameTable.Add("ValueFromPipelineByPropertyName");
            this.id54_AllowEmptyCollection = base.Reader.NameTable.Add("AllowEmptyCollection");
            this.id13_Item = base.Reader.NameTable.Add("CmdletParameterMetadataForGetCmdletFilteringParameter");
            this.id73_Parameter = base.Reader.NameTable.Add("Parameter");
            this.id18_Item = base.Reader.NameTable.Add("CmdletParameterMetadataForStaticMethodParameter");
            this.id104_MaxValueQuery = base.Reader.NameTable.Add("MaxValueQuery");
            this.id98_SourceRole = base.Reader.NameTable.Add("SourceRole");
            this.id5_ClassMetadataInstanceCmdlets = base.Reader.NameTable.Add("ClassMetadataInstanceCmdlets");
            this.id109_CmdletAdapter = base.Reader.NameTable.Add("CmdletAdapter");
            this.id10_AssociationAssociatedInstance = base.Reader.NameTable.Add("AssociationAssociatedInstance");
            this.id90_ErrorCode = base.Reader.NameTable.Add("ErrorCode");
            this.id40_Name = base.Reader.NameTable.Add("Name");
            this.id65_Max = base.Reader.NameTable.Add("Max");
            this.id49_Position = base.Reader.NameTable.Add("Position");
            this.id97_OptionName = base.Reader.NameTable.Add("OptionName");
            this.id81_CmdletMetadata = base.Reader.NameTable.Add("CmdletMetadata");
            this.id84_CmdletParameterSet = base.Reader.NameTable.Add("CmdletParameterSet");
            this.id101_PropertyName = base.Reader.NameTable.Add("PropertyName");
            this.id27_CommonMethodParameterMetadata = base.Reader.NameTable.Add("CommonMethodParameterMetadata");
            this.id105_ExcludeQuery = base.Reader.NameTable.Add("ExcludeQuery");
            this.id89_Type = base.Reader.NameTable.Add("Type");
            this.id32_InstanceMethodMetadata = base.Reader.NameTable.Add("InstanceMethodMetadata");
            this.id62_ValidateSet = base.Reader.NameTable.Add("ValidateSet");
            this.id52_CmdletParameterSets = base.Reader.NameTable.Add("CmdletParameterSets");
            this.id106_QueryableProperties = base.Reader.NameTable.Add("QueryableProperties");
            this.id56_AllowNull = base.Reader.NameTable.Add("AllowNull");
            this.id77_ArrayOfClassMetadataData = base.Reader.NameTable.Add("ArrayOfClassMetadataData");
            this.id96_DefaultCmdletParameterSet = base.Reader.NameTable.Add("DefaultCmdletParameterSet");
            this.id19_QueryOption = base.Reader.NameTable.Add("QueryOption");
            this.id86_Parameters = base.Reader.NameTable.Add("Parameters");
            this.id87_ParameterName = base.Reader.NameTable.Add("ParameterName");
            this.id44_BitwiseFlags = base.Reader.NameTable.Add("BitwiseFlags");
            this.id60_ValidateLength = base.Reader.NameTable.Add("ValidateLength");
            this.id75_ArrayOfStaticCmdletMetadata = base.Reader.NameTable.Add("ArrayOfStaticCmdletMetadata");
            this.id16_Item = base.Reader.NameTable.Add("CmdletParameterMetadataValidateRange");
            this.id38_EnumMetadataEnum = base.Reader.NameTable.Add("EnumMetadataEnum");
            this.id7_PropertyMetadata = base.Reader.NameTable.Add("PropertyMetadata");
            this.id107_QueryableAssociations = base.Reader.NameTable.Add("QueryableAssociations");
            this.id83_MethodName = base.Reader.NameTable.Add("MethodName");
            this.id8_TypeMetadata = base.Reader.NameTable.Add("TypeMetadata");
            this.id68_Property = base.Reader.NameTable.Add("Property");
            this.id26_StaticMethodMetadata = base.Reader.NameTable.Add("StaticMethodMetadata");
            this.id91_PSType = base.Reader.NameTable.Add("PSType");
            this.id43_UnderlyingType = base.Reader.NameTable.Add("UnderlyingType");
            this.id100_AssociatedInstance = base.Reader.NameTable.Add("AssociatedInstance");
            this.id76_Cmdlet = base.Reader.NameTable.Add("Cmdlet");
            this.id17_Item = base.Reader.NameTable.Add("CmdletParameterMetadataForInstanceMethodParameter");
            this.id82_Method = base.Reader.NameTable.Add("Method");
            this.id92_ETSType = base.Reader.NameTable.Add("ETSType");
            this.id25_CommonMethodMetadata = base.Reader.NameTable.Add("CommonMethodMetadata");
            this.id85_ReturnValue = base.Reader.NameTable.Add("ReturnValue");
            this.id66_ArrayOfString = base.Reader.NameTable.Add("ArrayOfString");
            this.id23_StaticCmdletMetadata = base.Reader.NameTable.Add("StaticCmdletMetadata");
            this.id58_ValidateNotNullOrEmpty = base.Reader.NameTable.Add("ValidateNotNullOrEmpty");
            this.id93_Verb = base.Reader.NameTable.Add("Verb");
            this.id118_Class = base.Reader.NameTable.Add("Class");
            this.id70_ArrayOfQueryOption = base.Reader.NameTable.Add("ArrayOfQueryOption");
            this.id12_Item = base.Reader.NameTable.Add("CmdletParameterMetadataForGetCmdletParameter");
            this.id41_Value = base.Reader.NameTable.Add("Value");
        }

        private object Read1_Object(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType)
            {
                if (flag)
                {
                    if (type != null)
                    {
                        return base.ReadTypedNull(type);
                    }
                    return null;
                }
                if (type == null)
                {
                    return base.ReadTypedPrimitive(new XmlQualifiedName("anyType", "http://www.w3.org/2001/XMLSchema"));
                }
                if ((type.Name == this.id39_EnumMetadataEnumValue) && (type.Namespace == this.id2_Item))
                {
                    return this.Read48_EnumMetadataEnumValue(isNullable, false);
                }
                if ((type.Name == this.id38_EnumMetadataEnum) && (type.Namespace == this.id2_Item))
                {
                    return this.Read47_EnumMetadataEnum(isNullable, false);
                }
                if ((type.Name == this.id37_ClassMetadataData) && (type.Namespace == this.id2_Item))
                {
                    return this.Read46_ClassMetadataData(isNullable, false);
                }
                if ((type.Name == this.id31_Item) && (type.Namespace == this.id2_Item))
                {
                    return this.Read45_Item(isNullable, false);
                }
                if ((type.Name == this.id16_Item) && (type.Namespace == this.id2_Item))
                {
                    return this.Read43_Item(isNullable, false);
                }
                if ((type.Name == this.id15_Item) && (type.Namespace == this.id2_Item))
                {
                    return this.Read42_Item(isNullable, false);
                }
                if ((type.Name == this.id14_Item) && (type.Namespace == this.id2_Item))
                {
                    return this.Read41_Item(isNullable, false);
                }
                if ((type.Name == this.id10_AssociationAssociatedInstance) && (type.Namespace == this.id2_Item))
                {
                    return this.Read40_AssociationAssociatedInstance(isNullable, false);
                }
                if ((type.Name == this.id5_ClassMetadataInstanceCmdlets) && (type.Namespace == this.id2_Item))
                {
                    return this.Read39_ClassMetadataInstanceCmdlets(isNullable, false);
                }
                if ((type.Name == this.id3_ClassMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read35_ClassMetadata(isNullable, false);
                }
                if ((type.Name == this.id23_StaticCmdletMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read33_StaticCmdletMetadata(isNullable, false);
                }
                if ((type.Name == this.id33_InstanceCmdletMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read30_InstanceCmdletMetadata(isNullable, false);
                }
                if ((type.Name == this.id27_CommonMethodParameterMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read25_CommonMethodParameterMetadata(isNullable, false);
                }
                if ((type.Name == this.id28_StaticMethodParameterMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read26_StaticMethodParameterMetadata(isNullable, false);
                }
                if ((type.Name == this.id30_Item) && (type.Namespace == this.id2_Item))
                {
                    return this.Read24_Item(isNullable, false);
                }
                if ((type.Name == this.id25_CommonMethodMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read28_CommonMethodMetadata(isNullable, false);
                }
                if ((type.Name == this.id32_InstanceMethodMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read29_InstanceMethodMetadata(isNullable, false);
                }
                if ((type.Name == this.id26_StaticMethodMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read27_StaticMethodMetadata(isNullable, false);
                }
                if ((type.Name == this.id29_CmdletOutputMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read22_CmdletOutputMetadata(isNullable, false);
                }
                if ((type.Name == this.id20_GetCmdletMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read21_GetCmdletMetadata(isNullable, false);
                }
                if ((type.Name == this.id21_CommonCmdletMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read20_CommonCmdletMetadata(isNullable, false);
                }
                if ((type.Name == this.id24_Item) && (type.Namespace == this.id2_Item))
                {
                    return this.Read44_Item(isNullable, false);
                }
                if ((type.Name == this.id6_GetCmdletParameters) && (type.Namespace == this.id2_Item))
                {
                    return this.Read18_GetCmdletParameters(isNullable, false);
                }
                if ((type.Name == this.id19_QueryOption) && (type.Namespace == this.id2_Item))
                {
                    return this.Read17_QueryOption(isNullable, false);
                }
                if ((type.Name == this.id9_Association) && (type.Namespace == this.id2_Item))
                {
                    return this.Read16_Association(isNullable, false);
                }
                if ((type.Name == this.id7_PropertyMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read14_PropertyMetadata(isNullable, false);
                }
                if ((type.Name == this.id34_PropertyQuery) && (type.Namespace == this.id2_Item))
                {
                    return this.Read13_PropertyQuery(isNullable, false);
                }
                if ((type.Name == this.id35_WildcardablePropertyQuery) && (type.Namespace == this.id2_Item))
                {
                    return this.Read12_WildcardablePropertyQuery(isNullable, false);
                }
                if ((type.Name == this.id11_CmdletParameterMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read9_CmdletParameterMetadata(isNullable, false);
                }
                if ((type.Name == this.id12_Item) && (type.Namespace == this.id2_Item))
                {
                    return this.Read10_Item(isNullable, false);
                }
                if ((type.Name == this.id13_Item) && (type.Namespace == this.id2_Item))
                {
                    return this.Read11_Item(isNullable, false);
                }
                if ((type.Name == this.id18_Item) && (type.Namespace == this.id2_Item))
                {
                    return this.Read8_Item(isNullable, false);
                }
                if ((type.Name == this.id17_Item) && (type.Namespace == this.id2_Item))
                {
                    return this.Read7_Item(isNullable, false);
                }
                if ((type.Name == this.id8_TypeMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read2_TypeMetadata(isNullable, false);
                }
                if ((type.Name == this.id36_ItemsChoiceType) && (type.Namespace == this.id2_Item))
                {
                    base.Reader.ReadStartElement();
                    object obj2 = this.Read3_ItemsChoiceType(base.CollapseWhitespace(base.Reader.ReadString()));
                    base.ReadEndElement();
                    return obj2;
                }
                if ((type.Name == this.id66_ArrayOfString) && (type.Namespace == this.id2_Item))
                {
                    string[] strArray = null;
                    if (base.ReadNull())
                    {
                        return strArray;
                    }
                    string[] strArray2 = null;
                    int num = 0;
                    if (base.Reader.IsEmptyElement)
                    {
                        base.Reader.Skip();
                    }
                    else
                    {
                        base.Reader.ReadStartElement();
                        base.Reader.MoveToContent();
                        int num2 = 0;
                        int num3 = base.ReaderCount;
                        while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                        {
                            if (base.Reader.NodeType == XmlNodeType.Element)
                            {
                                if ((base.Reader.LocalName == this.id63_AllowedValue) && (base.Reader.NamespaceURI == this.id2_Item))
                                {
                                    strArray2 = (string[]) base.EnsureArrayIndex(strArray2, num, typeof(string));
                                    strArray2[num++] = base.Reader.ReadElementString();
                                }
                                else
                                {
                                    base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowedValue");
                                }
                            }
                            else
                            {
                                base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowedValue");
                            }
                            base.Reader.MoveToContent();
                            base.CheckReaderCount(ref num2, ref num3);
                        }
                        base.ReadEndElement();
                    }
                    return (string[]) base.ShrinkArray(strArray2, num, typeof(string), false);
                }
                if ((type.Name == this.id67_ArrayOfPropertyMetadata) && (type.Namespace == this.id2_Item))
                {
                    PropertyMetadata[] metadataArray = null;
                    if (base.ReadNull())
                    {
                        return metadataArray;
                    }
                    PropertyMetadata[] metadataArray2 = null;
                    int num4 = 0;
                    if (base.Reader.IsEmptyElement)
                    {
                        base.Reader.Skip();
                    }
                    else
                    {
                        base.Reader.ReadStartElement();
                        base.Reader.MoveToContent();
                        int num5 = 0;
                        int num6 = base.ReaderCount;
                        while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                        {
                            if (base.Reader.NodeType == XmlNodeType.Element)
                            {
                                if ((base.Reader.LocalName == this.id68_Property) && (base.Reader.NamespaceURI == this.id2_Item))
                                {
                                    metadataArray2 = (PropertyMetadata[]) base.EnsureArrayIndex(metadataArray2, num4, typeof(PropertyMetadata));
                                    metadataArray2[num4++] = this.Read14_PropertyMetadata(false, true);
                                }
                                else
                                {
                                    base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Property");
                                }
                            }
                            else
                            {
                                base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Property");
                            }
                            base.Reader.MoveToContent();
                            base.CheckReaderCount(ref num5, ref num6);
                        }
                        base.ReadEndElement();
                    }
                    return (PropertyMetadata[]) base.ShrinkArray(metadataArray2, num4, typeof(PropertyMetadata), false);
                }
                if ((type.Name == this.id69_ArrayOfAssociation) && (type.Namespace == this.id2_Item))
                {
                    Association[] associationArray = null;
                    if (base.ReadNull())
                    {
                        return associationArray;
                    }
                    Association[] associationArray2 = null;
                    int num7 = 0;
                    if (base.Reader.IsEmptyElement)
                    {
                        base.Reader.Skip();
                    }
                    else
                    {
                        base.Reader.ReadStartElement();
                        base.Reader.MoveToContent();
                        int num8 = 0;
                        int num9 = base.ReaderCount;
                        while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                        {
                            if (base.Reader.NodeType == XmlNodeType.Element)
                            {
                                if ((base.Reader.LocalName == this.id9_Association) && (base.Reader.NamespaceURI == this.id2_Item))
                                {
                                    associationArray2 = (Association[]) base.EnsureArrayIndex(associationArray2, num7, typeof(Association));
                                    associationArray2[num7++] = this.Read16_Association(false, true);
                                }
                                else
                                {
                                    base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Association");
                                }
                            }
                            else
                            {
                                base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Association");
                            }
                            base.Reader.MoveToContent();
                            base.CheckReaderCount(ref num8, ref num9);
                        }
                        base.ReadEndElement();
                    }
                    return (Association[]) base.ShrinkArray(associationArray2, num7, typeof(Association), false);
                }
                if ((type.Name == this.id70_ArrayOfQueryOption) && (type.Namespace == this.id2_Item))
                {
                    QueryOption[] optionArray = null;
                    if (base.ReadNull())
                    {
                        return optionArray;
                    }
                    QueryOption[] optionArray2 = null;
                    int num10 = 0;
                    if (base.Reader.IsEmptyElement)
                    {
                        base.Reader.Skip();
                    }
                    else
                    {
                        base.Reader.ReadStartElement();
                        base.Reader.MoveToContent();
                        int num11 = 0;
                        int num12 = base.ReaderCount;
                        while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                        {
                            if (base.Reader.NodeType == XmlNodeType.Element)
                            {
                                if ((base.Reader.LocalName == this.id71_Option) && (base.Reader.NamespaceURI == this.id2_Item))
                                {
                                    optionArray2 = (QueryOption[]) base.EnsureArrayIndex(optionArray2, num10, typeof(QueryOption));
                                    optionArray2[num10++] = this.Read17_QueryOption(false, true);
                                }
                                else
                                {
                                    base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Option");
                                }
                            }
                            else
                            {
                                base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Option");
                            }
                            base.Reader.MoveToContent();
                            base.CheckReaderCount(ref num11, ref num12);
                        }
                        base.ReadEndElement();
                    }
                    return (QueryOption[]) base.ShrinkArray(optionArray2, num10, typeof(QueryOption), false);
                }
                if ((type.Name == this.id22_ConfirmImpact) && (type.Namespace == this.id2_Item))
                {
                    base.Reader.ReadStartElement();
                    object obj3 = this.Read19_ConfirmImpact(base.CollapseWhitespace(base.Reader.ReadString()));
                    base.ReadEndElement();
                    return obj3;
                }
                if ((type.Name == this.id72_Item) && (type.Namespace == this.id2_Item))
                {
                    StaticMethodParameterMetadata[] metadataArray3 = null;
                    if (base.ReadNull())
                    {
                        return metadataArray3;
                    }
                    StaticMethodParameterMetadata[] metadataArray4 = null;
                    int num13 = 0;
                    if (base.Reader.IsEmptyElement)
                    {
                        base.Reader.Skip();
                    }
                    else
                    {
                        base.Reader.ReadStartElement();
                        base.Reader.MoveToContent();
                        int num14 = 0;
                        int num15 = base.ReaderCount;
                        while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                        {
                            if (base.Reader.NodeType == XmlNodeType.Element)
                            {
                                if ((base.Reader.LocalName == this.id73_Parameter) && (base.Reader.NamespaceURI == this.id2_Item))
                                {
                                    metadataArray4 = (StaticMethodParameterMetadata[]) base.EnsureArrayIndex(metadataArray4, num13, typeof(StaticMethodParameterMetadata));
                                    metadataArray4[num13++] = this.Read26_StaticMethodParameterMetadata(false, true);
                                }
                                else
                                {
                                    base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameter");
                                }
                            }
                            else
                            {
                                base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameter");
                            }
                            base.Reader.MoveToContent();
                            base.CheckReaderCount(ref num14, ref num15);
                        }
                        base.ReadEndElement();
                    }
                    return (StaticMethodParameterMetadata[]) base.ShrinkArray(metadataArray4, num13, typeof(StaticMethodParameterMetadata), false);
                }
                if ((type.Name == this.id74_Item) && (type.Namespace == this.id2_Item))
                {
                    InstanceMethodParameterMetadata[] metadataArray5 = null;
                    if (base.ReadNull())
                    {
                        return metadataArray5;
                    }
                    InstanceMethodParameterMetadata[] metadataArray6 = null;
                    int num16 = 0;
                    if (base.Reader.IsEmptyElement)
                    {
                        base.Reader.Skip();
                    }
                    else
                    {
                        base.Reader.ReadStartElement();
                        base.Reader.MoveToContent();
                        int num17 = 0;
                        int num18 = base.ReaderCount;
                        while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                        {
                            if (base.Reader.NodeType == XmlNodeType.Element)
                            {
                                if ((base.Reader.LocalName == this.id73_Parameter) && (base.Reader.NamespaceURI == this.id2_Item))
                                {
                                    metadataArray6 = (InstanceMethodParameterMetadata[]) base.EnsureArrayIndex(metadataArray6, num16, typeof(InstanceMethodParameterMetadata));
                                    metadataArray6[num16++] = this.Read24_Item(false, true);
                                }
                                else
                                {
                                    base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameter");
                                }
                            }
                            else
                            {
                                base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameter");
                            }
                            base.Reader.MoveToContent();
                            base.CheckReaderCount(ref num17, ref num18);
                        }
                        base.ReadEndElement();
                    }
                    return (InstanceMethodParameterMetadata[]) base.ShrinkArray(metadataArray6, num16, typeof(InstanceMethodParameterMetadata), false);
                }
                if ((type.Name == this.id75_ArrayOfStaticCmdletMetadata) && (type.Namespace == this.id2_Item))
                {
                    StaticCmdletMetadata[] metadataArray7 = null;
                    if (base.ReadNull())
                    {
                        return metadataArray7;
                    }
                    StaticCmdletMetadata[] metadataArray8 = null;
                    int num19 = 0;
                    if (base.Reader.IsEmptyElement)
                    {
                        base.Reader.Skip();
                    }
                    else
                    {
                        base.Reader.ReadStartElement();
                        base.Reader.MoveToContent();
                        int num20 = 0;
                        int num21 = base.ReaderCount;
                        while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                        {
                            if (base.Reader.NodeType == XmlNodeType.Element)
                            {
                                if ((base.Reader.LocalName == this.id76_Cmdlet) && (base.Reader.NamespaceURI == this.id2_Item))
                                {
                                    metadataArray8 = (StaticCmdletMetadata[]) base.EnsureArrayIndex(metadataArray8, num19, typeof(StaticCmdletMetadata));
                                    metadataArray8[num19++] = this.Read33_StaticCmdletMetadata(false, true);
                                }
                                else
                                {
                                    base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Cmdlet");
                                }
                            }
                            else
                            {
                                base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Cmdlet");
                            }
                            base.Reader.MoveToContent();
                            base.CheckReaderCount(ref num20, ref num21);
                        }
                        base.ReadEndElement();
                    }
                    return (StaticCmdletMetadata[]) base.ShrinkArray(metadataArray8, num19, typeof(StaticCmdletMetadata), false);
                }
                if ((type.Name == this.id77_ArrayOfClassMetadataData) && (type.Namespace == this.id2_Item))
                {
                    ClassMetadataData[] dataArray = null;
                    if (base.ReadNull())
                    {
                        return dataArray;
                    }
                    ClassMetadataData[] dataArray2 = null;
                    int num22 = 0;
                    if (base.Reader.IsEmptyElement)
                    {
                        base.Reader.Skip();
                    }
                    else
                    {
                        base.Reader.ReadStartElement();
                        base.Reader.MoveToContent();
                        int num23 = 0;
                        int num24 = base.ReaderCount;
                        while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                        {
                            if (base.Reader.NodeType == XmlNodeType.Element)
                            {
                                if ((base.Reader.LocalName == this.id78_Data) && (base.Reader.NamespaceURI == this.id2_Item))
                                {
                                    dataArray2 = (ClassMetadataData[]) base.EnsureArrayIndex(dataArray2, num22, typeof(ClassMetadataData));
                                    dataArray2[num22++] = this.Read34_ClassMetadataData(false, true);
                                }
                                else
                                {
                                    base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Data");
                                }
                            }
                            else
                            {
                                base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Data");
                            }
                            base.Reader.MoveToContent();
                            base.CheckReaderCount(ref num23, ref num24);
                        }
                        base.ReadEndElement();
                    }
                    return (ClassMetadataData[]) base.ShrinkArray(dataArray2, num22, typeof(ClassMetadataData), false);
                }
                if ((type.Name != this.id79_ArrayOfEnumMetadataEnum) || (type.Namespace != this.id2_Item))
                {
                    return base.ReadTypedPrimitive(type);
                }
                EnumMetadataEnum[] enumArray = null;
                if (base.ReadNull())
                {
                    return enumArray;
                }
                EnumMetadataEnum[] a = null;
                int index = 0;
                if (base.Reader.IsEmptyElement)
                {
                    base.Reader.Skip();
                }
                else
                {
                    base.Reader.ReadStartElement();
                    base.Reader.MoveToContent();
                    int num26 = 0;
                    int num27 = base.ReaderCount;
                    while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                    {
                        if (base.Reader.NodeType == XmlNodeType.Element)
                        {
                            if ((base.Reader.LocalName == this.id80_Enum) && (base.Reader.NamespaceURI == this.id2_Item))
                            {
                                a = (EnumMetadataEnum[]) base.EnsureArrayIndex(a, index, typeof(EnumMetadataEnum));
                                a[index++] = this.Read37_EnumMetadataEnum(false, true);
                            }
                            else
                            {
                                base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Enum");
                            }
                        }
                        else
                        {
                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Enum");
                        }
                        base.Reader.MoveToContent();
                        base.CheckReaderCount(ref num26, ref num27);
                    }
                    base.ReadEndElement();
                }
                return (EnumMetadataEnum[]) base.ShrinkArray(a, index, typeof(EnumMetadataEnum), false);
            }
            if (flag)
            {
                return null;
            }
            object o = new object();
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private CmdletParameterMetadataForGetCmdletParameter Read10_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id12_Item) || (type.Namespace != this.id2_Item)))
            {
                if ((type.Name != this.id13_Item) || (type.Namespace != this.id2_Item))
                {
                    throw base.CreateUnknownTypeException(type);
                }
                return this.Read11_Item(isNullable, false);
            }
            if (flag)
            {
                return null;
            }
            CmdletParameterMetadataForGetCmdletParameter o = new CmdletParameterMetadataForGetCmdletParameter();
            string[] a = null;
            int index = 0;
            string[] strArray2 = null;
            int num2 = 0;
            bool[] flagArray = new bool[0x10];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[9] && (base.Reader.LocalName == this.id46_IsMandatory)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.IsMandatory = XmlConvert.ToBoolean(base.Reader.Value);
                    o.IsMandatorySpecified = true;
                    flagArray[9] = true;
                }
                else
                {
                    if ((base.Reader.LocalName == this.id47_Aliases) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        string[] strArray3 = base.Reader.Value.Split(null);
                        for (int i = 0; i < strArray3.Length; i++)
                        {
                            a = (string[]) base.EnsureArrayIndex(a, index, typeof(string));
                            a[index++] = strArray3[i];
                        }
                        continue;
                    }
                    if ((!flagArray[11] && (base.Reader.LocalName == this.id48_PSName)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.PSName = base.Reader.Value;
                        flagArray[11] = true;
                        continue;
                    }
                    if ((!flagArray[12] && (base.Reader.LocalName == this.id49_Position)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.Position = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[12] = true;
                        continue;
                    }
                    if ((!flagArray[13] && (base.Reader.LocalName == this.id50_ValueFromPipeline)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.ValueFromPipeline = XmlConvert.ToBoolean(base.Reader.Value);
                        o.ValueFromPipelineSpecified = true;
                        flagArray[13] = true;
                        continue;
                    }
                    if ((!flagArray[14] && (base.Reader.LocalName == this.id51_Item)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.ValueFromPipelineByPropertyName = XmlConvert.ToBoolean(base.Reader.Value);
                        o.ValueFromPipelineByPropertyNameSpecified = true;
                        flagArray[14] = true;
                        continue;
                    }
                    if ((base.Reader.LocalName == this.id52_CmdletParameterSets) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        string[] strArray4 = base.Reader.Value.Split(null);
                        for (int j = 0; j < strArray4.Length; j++)
                        {
                            strArray2 = (string[]) base.EnsureArrayIndex(strArray2, num2, typeof(string));
                            strArray2[num2++] = strArray4[j];
                        }
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":IsMandatory, :Aliases, :PSName, :Position, :ValueFromPipeline, :ValueFromPipelineByPropertyName, :CmdletParameterSets");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
                o.CmdletParameterSets = (string[]) base.ShrinkArray(strArray2, num2, typeof(string), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id54_AllowEmptyCollection)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowEmptyCollection = this.Read1_Object(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id55_AllowEmptyString)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowEmptyString = this.Read1_Object(false, true);
                        flagArray[1] = true;
                    }
                    else if ((!flagArray[2] && (base.Reader.LocalName == this.id56_AllowNull)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowNull = this.Read1_Object(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[3] && (base.Reader.LocalName == this.id57_ValidateNotNull)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateNotNull = this.Read1_Object(false, true);
                        flagArray[3] = true;
                    }
                    else if ((!flagArray[4] && (base.Reader.LocalName == this.id58_ValidateNotNullOrEmpty)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateNotNullOrEmpty = this.Read1_Object(false, true);
                        flagArray[4] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id59_ValidateCount)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateCount = this.Read4_Item(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[6] && (base.Reader.LocalName == this.id60_ValidateLength)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateLength = this.Read5_Item(false, true);
                        flagArray[6] = true;
                    }
                    else if ((!flagArray[7] && (base.Reader.LocalName == this.id61_ValidateRange)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateRange = this.Read6_Item(false, true);
                        flagArray[7] = true;
                    }
                    else if ((base.Reader.LocalName == this.id62_ValidateSet) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            string[] strArray5 = null;
                            int num7 = 0;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num8 = 0;
                                int num9 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id63_AllowedValue) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            strArray5 = (string[]) base.EnsureArrayIndex(strArray5, num7, typeof(string));
                                            strArray5[num7++] = base.Reader.ReadElementString();
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowedValue");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowedValue");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num8, ref num9);
                                }
                                base.ReadEndElement();
                            }
                            o.ValidateSet = (string[]) base.ShrinkArray(strArray5, num7, typeof(string), false);
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyCollection, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyString, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNullOrEmpty, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateCount, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateLength, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateRange, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateSet");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyCollection, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyString, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNullOrEmpty, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateCount, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateLength, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateRange, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateSet");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
            o.CmdletParameterSets = (string[]) base.ShrinkArray(strArray2, num2, typeof(string), true);
            base.ReadEndElement();
            return o;
        }

        private CmdletParameterMetadataForGetCmdletFilteringParameter Read11_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id13_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            CmdletParameterMetadataForGetCmdletFilteringParameter o = new CmdletParameterMetadataForGetCmdletFilteringParameter();
            string[] a = null;
            int index = 0;
            string[] strArray2 = null;
            int num2 = 0;
            bool[] flagArray = new bool[0x11];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[9] && (base.Reader.LocalName == this.id46_IsMandatory)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.IsMandatory = XmlConvert.ToBoolean(base.Reader.Value);
                    o.IsMandatorySpecified = true;
                    flagArray[9] = true;
                }
                else
                {
                    if ((base.Reader.LocalName == this.id47_Aliases) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        string[] strArray3 = base.Reader.Value.Split(null);
                        for (int i = 0; i < strArray3.Length; i++)
                        {
                            a = (string[]) base.EnsureArrayIndex(a, index, typeof(string));
                            a[index++] = strArray3[i];
                        }
                        continue;
                    }
                    if ((!flagArray[11] && (base.Reader.LocalName == this.id48_PSName)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.PSName = base.Reader.Value;
                        flagArray[11] = true;
                        continue;
                    }
                    if ((!flagArray[12] && (base.Reader.LocalName == this.id49_Position)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.Position = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[12] = true;
                        continue;
                    }
                    if ((!flagArray[13] && (base.Reader.LocalName == this.id50_ValueFromPipeline)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.ValueFromPipeline = XmlConvert.ToBoolean(base.Reader.Value);
                        o.ValueFromPipelineSpecified = true;
                        flagArray[13] = true;
                        continue;
                    }
                    if ((!flagArray[14] && (base.Reader.LocalName == this.id51_Item)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.ValueFromPipelineByPropertyName = XmlConvert.ToBoolean(base.Reader.Value);
                        o.ValueFromPipelineByPropertyNameSpecified = true;
                        flagArray[14] = true;
                        continue;
                    }
                    if ((base.Reader.LocalName == this.id52_CmdletParameterSets) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        string[] strArray4 = base.Reader.Value.Split(null);
                        for (int j = 0; j < strArray4.Length; j++)
                        {
                            strArray2 = (string[]) base.EnsureArrayIndex(strArray2, num2, typeof(string));
                            strArray2[num2++] = strArray4[j];
                        }
                        continue;
                    }
                    if ((!flagArray[0x10] && (base.Reader.LocalName == this.id53_ErrorOnNoMatch)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.ErrorOnNoMatch = XmlConvert.ToBoolean(base.Reader.Value);
                        o.ErrorOnNoMatchSpecified = true;
                        flagArray[0x10] = true;
                    }
                    else if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":IsMandatory, :Aliases, :PSName, :Position, :ValueFromPipeline, :ValueFromPipelineByPropertyName, :CmdletParameterSets, :ErrorOnNoMatch");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
                o.CmdletParameterSets = (string[]) base.ShrinkArray(strArray2, num2, typeof(string), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id54_AllowEmptyCollection)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowEmptyCollection = this.Read1_Object(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id55_AllowEmptyString)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowEmptyString = this.Read1_Object(false, true);
                        flagArray[1] = true;
                    }
                    else if ((!flagArray[2] && (base.Reader.LocalName == this.id56_AllowNull)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowNull = this.Read1_Object(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[3] && (base.Reader.LocalName == this.id57_ValidateNotNull)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateNotNull = this.Read1_Object(false, true);
                        flagArray[3] = true;
                    }
                    else if ((!flagArray[4] && (base.Reader.LocalName == this.id58_ValidateNotNullOrEmpty)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateNotNullOrEmpty = this.Read1_Object(false, true);
                        flagArray[4] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id59_ValidateCount)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateCount = this.Read4_Item(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[6] && (base.Reader.LocalName == this.id60_ValidateLength)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateLength = this.Read5_Item(false, true);
                        flagArray[6] = true;
                    }
                    else if ((!flagArray[7] && (base.Reader.LocalName == this.id61_ValidateRange)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateRange = this.Read6_Item(false, true);
                        flagArray[7] = true;
                    }
                    else if ((base.Reader.LocalName == this.id62_ValidateSet) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            string[] strArray5 = null;
                            int num7 = 0;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num8 = 0;
                                int num9 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id63_AllowedValue) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            strArray5 = (string[]) base.EnsureArrayIndex(strArray5, num7, typeof(string));
                                            strArray5[num7++] = base.Reader.ReadElementString();
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowedValue");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowedValue");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num8, ref num9);
                                }
                                base.ReadEndElement();
                            }
                            o.ValidateSet = (string[]) base.ShrinkArray(strArray5, num7, typeof(string), false);
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyCollection, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyString, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNullOrEmpty, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateCount, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateLength, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateRange, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateSet");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyCollection, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyString, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNullOrEmpty, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateCount, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateLength, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateRange, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateSet");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
            o.CmdletParameterSets = (string[]) base.ShrinkArray(strArray2, num2, typeof(string), true);
            base.ReadEndElement();
            return o;
        }

        private WildcardablePropertyQuery Read12_WildcardablePropertyQuery(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id35_WildcardablePropertyQuery) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            WildcardablePropertyQuery o = new WildcardablePropertyQuery();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id45_AllowGlobbing)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.AllowGlobbing = XmlConvert.ToBoolean(base.Reader.Value);
                    o.AllowGlobbingSpecified = true;
                    flagArray[1] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, ":AllowGlobbing");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id11_CmdletParameterMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletParameterMetadata = this.Read11_Item(false, true);
                        flagArray[0] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private PropertyQuery Read13_PropertyQuery(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id34_PropertyQuery) || (type.Namespace != this.id2_Item)))
            {
                if ((type.Name != this.id35_WildcardablePropertyQuery) || (type.Namespace != this.id2_Item))
                {
                    throw base.CreateUnknownTypeException(type);
                }
                return this.Read12_WildcardablePropertyQuery(isNullable, false);
            }
            if (flag)
            {
                return null;
            }
            PropertyQuery o = new PropertyQuery();
            bool[] flagArray = new bool[1];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id11_CmdletParameterMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletParameterMetadata = this.Read11_Item(false, true);
                        flagArray[0] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private PropertyMetadata Read14_PropertyMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id7_PropertyMetadata) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            PropertyMetadata o = new PropertyMetadata();
            PropertyQuery[] a = null;
            int length = 0;
            ItemsChoiceType[] typeArray = null;
            int num2 = 0;
            bool[] flagArray = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[2] && (base.Reader.LocalName == this.id101_PropertyName)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.PropertyName = base.Reader.Value;
                    flagArray[2] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, ":PropertyName");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Items = (PropertyQuery[]) base.ShrinkArray(a, length, typeof(PropertyQuery), true);
                o.ItemsElementName = (ItemsChoiceType[]) base.ShrinkArray(typeArray, num2, typeof(ItemsChoiceType), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id89_Type)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Type = this.Read2_TypeMetadata(false, true);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id102_MinValueQuery) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        a = (PropertyQuery[]) base.EnsureArrayIndex(a, length, typeof(PropertyQuery));
                        a[length++] = this.Read13_PropertyQuery(false, true);
                        typeArray = (ItemsChoiceType[]) base.EnsureArrayIndex(typeArray, num2, typeof(ItemsChoiceType));
                        typeArray[num2++] = ItemsChoiceType.MinValueQuery;
                    }
                    else if ((base.Reader.LocalName == this.id103_RegularQuery) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        a = (PropertyQuery[]) base.EnsureArrayIndex(a, length, typeof(PropertyQuery));
                        a[length++] = this.Read12_WildcardablePropertyQuery(false, true);
                        typeArray = (ItemsChoiceType[]) base.EnsureArrayIndex(typeArray, num2, typeof(ItemsChoiceType));
                        typeArray[num2++] = ItemsChoiceType.RegularQuery;
                    }
                    else if ((base.Reader.LocalName == this.id104_MaxValueQuery) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        a = (PropertyQuery[]) base.EnsureArrayIndex(a, length, typeof(PropertyQuery));
                        a[length++] = this.Read13_PropertyQuery(false, true);
                        typeArray = (ItemsChoiceType[]) base.EnsureArrayIndex(typeArray, num2, typeof(ItemsChoiceType));
                        typeArray[num2++] = ItemsChoiceType.MaxValueQuery;
                    }
                    else if ((base.Reader.LocalName == this.id105_ExcludeQuery) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        a = (PropertyQuery[]) base.EnsureArrayIndex(a, length, typeof(PropertyQuery));
                        a[length++] = this.Read12_WildcardablePropertyQuery(false, true);
                        typeArray = (ItemsChoiceType[]) base.EnsureArrayIndex(typeArray, num2, typeof(ItemsChoiceType));
                        typeArray[num2++] = ItemsChoiceType.ExcludeQuery;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:MinValueQuery, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:RegularQuery, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:MaxValueQuery, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ExcludeQuery");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:MinValueQuery, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:RegularQuery, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:MaxValueQuery, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ExcludeQuery");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Items = (PropertyQuery[]) base.ShrinkArray(a, length, typeof(PropertyQuery), true);
            o.ItemsElementName = (ItemsChoiceType[]) base.ShrinkArray(typeArray, num2, typeof(ItemsChoiceType), true);
            base.ReadEndElement();
            return o;
        }

        private AssociationAssociatedInstance Read15_AssociationAssociatedInstance(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id4_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            AssociationAssociatedInstance o = new AssociationAssociatedInstance();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id89_Type)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Type = this.Read2_TypeMetadata(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id11_CmdletParameterMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletParameterMetadata = this.Read11_Item(false, true);
                        flagArray[1] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private Association Read16_Association(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id9_Association) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            Association o = new Association();
            bool[] flagArray = new bool[4];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id9_Association)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Association1 = base.Reader.Value;
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id98_SourceRole)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.SourceRole = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id99_ResultRole)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.ResultRole = base.Reader.Value;
                        flagArray[3] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":Association, :SourceRole, :ResultRole");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id100_AssociatedInstance)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AssociatedInstance = this.Read15_AssociationAssociatedInstance(false, true);
                        flagArray[0] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AssociatedInstance");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AssociatedInstance");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private QueryOption Read17_QueryOption(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id19_QueryOption) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            QueryOption o = new QueryOption();
            bool[] flagArray = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[2] && (base.Reader.LocalName == this.id97_OptionName)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.OptionName = base.Reader.Value;
                    flagArray[2] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, ":OptionName");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id89_Type)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Type = this.Read2_TypeMetadata(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id11_CmdletParameterMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletParameterMetadata = this.Read10_Item(false, true);
                        flagArray[1] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private GetCmdletParameters Read18_GetCmdletParameters(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id6_GetCmdletParameters) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            GetCmdletParameters o = new GetCmdletParameters();
            bool[] flagArray = new bool[4];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[3] && (base.Reader.LocalName == this.id96_DefaultCmdletParameterSet)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.DefaultCmdletParameterSet = base.Reader.Value;
                    flagArray[3] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, ":DefaultCmdletParameterSet");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((base.Reader.LocalName == this.id106_QueryableProperties) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            PropertyMetadata[] a = null;
                            int index = 0;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num4 = 0;
                                int num5 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id68_Property) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            a = (PropertyMetadata[]) base.EnsureArrayIndex(a, index, typeof(PropertyMetadata));
                                            a[index++] = this.Read14_PropertyMetadata(false, true);
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Property");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Property");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num4, ref num5);
                                }
                                base.ReadEndElement();
                            }
                            o.QueryableProperties = (PropertyMetadata[]) base.ShrinkArray(a, index, typeof(PropertyMetadata), false);
                        }
                    }
                    else if ((base.Reader.LocalName == this.id107_QueryableAssociations) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            Association[] associationArray = null;
                            int num6 = 0;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num7 = 0;
                                int num8 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id9_Association) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            associationArray = (Association[]) base.EnsureArrayIndex(associationArray, num6, typeof(Association));
                                            associationArray[num6++] = this.Read16_Association(false, true);
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Association");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Association");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num7, ref num8);
                                }
                                base.ReadEndElement();
                            }
                            o.QueryableAssociations = (Association[]) base.ShrinkArray(associationArray, num6, typeof(Association), false);
                        }
                    }
                    else if ((base.Reader.LocalName == this.id108_QueryOptions) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            QueryOption[] optionArray = null;
                            int num9 = 0;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num10 = 0;
                                int num11 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id71_Option) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            optionArray = (QueryOption[]) base.EnsureArrayIndex(optionArray, num9, typeof(QueryOption));
                                            optionArray[num9++] = this.Read17_QueryOption(false, true);
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Option");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Option");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num10, ref num11);
                                }
                                base.ReadEndElement();
                            }
                            o.QueryOptions = (QueryOption[]) base.ShrinkArray(optionArray, num9, typeof(QueryOption), false);
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:QueryableProperties, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:QueryableAssociations, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:QueryOptions");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:QueryableProperties, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:QueryableAssociations, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:QueryOptions");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private ConfirmImpact Read19_ConfirmImpact(string s)
        {
            switch (s)
            {
                case "None":
                    return ConfirmImpact.None;

                case "Low":
                    return ConfirmImpact.Low;

                case "Medium":
                    return ConfirmImpact.Medium;

                case "High":
                    return ConfirmImpact.High;
            }
            throw base.CreateUnknownConstantException(s, typeof(ConfirmImpact));
        }

        private TypeMetadata Read2_TypeMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id8_TypeMetadata) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            TypeMetadata o = new TypeMetadata();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id91_PSType)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.PSType = base.Reader.Value;
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id92_ETSType)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.ETSType = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":PSType, :ETSType");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private CommonCmdletMetadata Read20_CommonCmdletMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id21_CommonCmdletMetadata) || (type.Namespace != this.id2_Item)))
            {
                if ((type.Name != this.id24_Item) || (type.Namespace != this.id2_Item))
                {
                    throw base.CreateUnknownTypeException(type);
                }
                return this.Read44_Item(isNullable, false);
            }
            if (flag)
            {
                return null;
            }
            CommonCmdletMetadata o = new CommonCmdletMetadata();
            string[] a = null;
            int index = 0;
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id93_Verb)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Verb = base.Reader.Value;
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id94_Noun)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.Noun = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if ((base.Reader.LocalName == this.id47_Aliases) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        string[] strArray2 = base.Reader.Value.Split(null);
                        for (int i = 0; i < strArray2.Length; i++)
                        {
                            a = (string[]) base.EnsureArrayIndex(a, index, typeof(string));
                            a[index++] = strArray2[i];
                        }
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id22_ConfirmImpact)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.ConfirmImpact = this.Read19_ConfirmImpact(base.Reader.Value);
                        o.ConfirmImpactSpecified = true;
                        flagArray[3] = true;
                    }
                    else
                    {
                        if ((!flagArray[4] && (base.Reader.LocalName == this.id95_HelpUri)) && (base.Reader.NamespaceURI == this.id4_Item))
                        {
                            o.HelpUri = base.CollapseWhitespace(base.Reader.Value);
                            flagArray[4] = true;
                            continue;
                        }
                        if (!base.IsXmlnsAttribute(base.Reader.Name))
                        {
                            base.UnknownNode(o, ":Verb, :Noun, :Aliases, :ConfirmImpact, :HelpUri");
                        }
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
            base.ReadEndElement();
            return o;
        }

        private GetCmdletMetadata Read21_GetCmdletMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id20_GetCmdletMetadata) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            GetCmdletMetadata o = new GetCmdletMetadata();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id81_CmdletMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletMetadata = this.Read20_CommonCmdletMetadata(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id6_GetCmdletParameters)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.GetCmdletParameters = this.Read18_GetCmdletParameters(false, true);
                        flagArray[1] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdletParameters");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdletParameters");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private CmdletOutputMetadata Read22_CmdletOutputMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id29_CmdletOutputMetadata) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            CmdletOutputMetadata o = new CmdletOutputMetadata();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id48_PSName)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.PSName = base.Reader.Value;
                    flagArray[1] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, ":PSName");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id90_ErrorCode)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ErrorCode = this.Read1_Object(false, true);
                        flagArray[0] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ErrorCode");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ErrorCode");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private CommonMethodMetadataReturnValue Read23_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id4_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            CommonMethodMetadataReturnValue o = new CommonMethodMetadataReturnValue();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id89_Type)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Type = this.Read2_TypeMetadata(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id29_CmdletOutputMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletOutputMetadata = this.Read22_CmdletOutputMetadata(false, true);
                        flagArray[1] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletOutputMetadata");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletOutputMetadata");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private InstanceMethodParameterMetadata Read24_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id30_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            InstanceMethodParameterMetadata o = new InstanceMethodParameterMetadata();
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id87_ParameterName)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.ParameterName = base.Reader.Value;
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id88_DefaultValue)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.DefaultValue = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":ParameterName, :DefaultValue");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id89_Type)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Type = this.Read2_TypeMetadata(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[3] && (base.Reader.LocalName == this.id11_CmdletParameterMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletParameterMetadata = this.Read7_Item(false, true);
                        flagArray[3] = true;
                    }
                    else if ((!flagArray[4] && (base.Reader.LocalName == this.id29_CmdletOutputMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletOutputMetadata = this.Read22_CmdletOutputMetadata(false, true);
                        flagArray[4] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletOutputMetadata");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletOutputMetadata");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private CommonMethodParameterMetadata Read25_CommonMethodParameterMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id27_CommonMethodParameterMetadata) || (type.Namespace != this.id2_Item)))
            {
                if ((type.Name == this.id28_StaticMethodParameterMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read26_StaticMethodParameterMetadata(isNullable, false);
                }
                if ((type.Name != this.id30_Item) || (type.Namespace != this.id2_Item))
                {
                    throw base.CreateUnknownTypeException(type);
                }
                return this.Read24_Item(isNullable, false);
            }
            if (flag)
            {
                return null;
            }
            CommonMethodParameterMetadata o = new CommonMethodParameterMetadata();
            bool[] flagArray = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id87_ParameterName)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.ParameterName = base.Reader.Value;
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id88_DefaultValue)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.DefaultValue = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":ParameterName, :DefaultValue");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id89_Type)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Type = this.Read2_TypeMetadata(false, true);
                        flagArray[0] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private StaticMethodParameterMetadata Read26_StaticMethodParameterMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id28_StaticMethodParameterMetadata) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            StaticMethodParameterMetadata o = new StaticMethodParameterMetadata();
            bool[] flagArray = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id87_ParameterName)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.ParameterName = base.Reader.Value;
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id88_DefaultValue)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.DefaultValue = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":ParameterName, :DefaultValue");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id89_Type)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Type = this.Read2_TypeMetadata(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[3] && (base.Reader.LocalName == this.id11_CmdletParameterMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletParameterMetadata = this.Read8_Item(false, true);
                        flagArray[3] = true;
                    }
                    else if ((!flagArray[4] && (base.Reader.LocalName == this.id29_CmdletOutputMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletOutputMetadata = this.Read22_CmdletOutputMetadata(false, true);
                        flagArray[4] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletOutputMetadata");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletOutputMetadata");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private StaticMethodMetadata Read27_StaticMethodMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id26_StaticMethodMetadata) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            StaticMethodMetadata o = new StaticMethodMetadata();
            bool[] flagArray = new bool[4];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id83_MethodName)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.MethodName = base.Reader.Value;
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id84_CmdletParameterSet)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.CmdletParameterSet = base.Reader.Value;
                        flagArray[3] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":MethodName, :CmdletParameterSet");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id85_ReturnValue)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ReturnValue = this.Read23_Item(false, true);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id86_Parameters) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            StaticMethodParameterMetadata[] a = null;
                            int index = 0;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num4 = 0;
                                int num5 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id73_Parameter) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            a = (StaticMethodParameterMetadata[]) base.EnsureArrayIndex(a, index, typeof(StaticMethodParameterMetadata));
                                            a[index++] = this.Read26_StaticMethodParameterMetadata(false, true);
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameter");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameter");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num4, ref num5);
                                }
                                base.ReadEndElement();
                            }
                            o.Parameters = (StaticMethodParameterMetadata[]) base.ShrinkArray(a, index, typeof(StaticMethodParameterMetadata), false);
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ReturnValue, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameters");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ReturnValue, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameters");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private CommonMethodMetadata Read28_CommonMethodMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id25_CommonMethodMetadata) || (type.Namespace != this.id2_Item)))
            {
                if ((type.Name == this.id32_InstanceMethodMetadata) && (type.Namespace == this.id2_Item))
                {
                    return this.Read29_InstanceMethodMetadata(isNullable, false);
                }
                if ((type.Name != this.id26_StaticMethodMetadata) || (type.Namespace != this.id2_Item))
                {
                    throw base.CreateUnknownTypeException(type);
                }
                return this.Read27_StaticMethodMetadata(isNullable, false);
            }
            if (flag)
            {
                return null;
            }
            CommonMethodMetadata o = new CommonMethodMetadata();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id83_MethodName)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.MethodName = base.Reader.Value;
                    flagArray[1] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, ":MethodName");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id85_ReturnValue)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ReturnValue = this.Read23_Item(false, true);
                        flagArray[0] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ReturnValue");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ReturnValue");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private InstanceMethodMetadata Read29_InstanceMethodMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id32_InstanceMethodMetadata) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            InstanceMethodMetadata o = new InstanceMethodMetadata();
            bool[] flagArray = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id83_MethodName)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.MethodName = base.Reader.Value;
                    flagArray[1] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, ":MethodName");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id85_ReturnValue)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ReturnValue = this.Read23_Item(false, true);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id86_Parameters) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            InstanceMethodParameterMetadata[] a = null;
                            int index = 0;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num4 = 0;
                                int num5 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id73_Parameter) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            a = (InstanceMethodParameterMetadata[]) base.EnsureArrayIndex(a, index, typeof(InstanceMethodParameterMetadata));
                                            a[index++] = this.Read24_Item(false, true);
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameter");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameter");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num4, ref num5);
                                }
                                base.ReadEndElement();
                            }
                            o.Parameters = (InstanceMethodParameterMetadata[]) base.ShrinkArray(a, index, typeof(InstanceMethodParameterMetadata), false);
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ReturnValue, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameters");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ReturnValue, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Parameters");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private ItemsChoiceType Read3_ItemsChoiceType(string s)
        {
            switch (s)
            {
                case "ExcludeQuery":
                    return ItemsChoiceType.ExcludeQuery;

                case "MaxValueQuery":
                    return ItemsChoiceType.MaxValueQuery;

                case "MinValueQuery":
                    return ItemsChoiceType.MinValueQuery;

                case "RegularQuery":
                    return ItemsChoiceType.RegularQuery;
            }
            throw base.CreateUnknownConstantException(s, typeof(ItemsChoiceType));
        }

        private InstanceCmdletMetadata Read30_InstanceCmdletMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id33_InstanceCmdletMetadata) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            InstanceCmdletMetadata o = new InstanceCmdletMetadata();
            bool[] flagArray = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id81_CmdletMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletMetadata = this.Read20_CommonCmdletMetadata(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id82_Method)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Method = this.Read29_InstanceMethodMetadata(false, true);
                        flagArray[1] = true;
                    }
                    else if ((!flagArray[2] && (base.Reader.LocalName == this.id6_GetCmdletParameters)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.GetCmdletParameters = this.Read18_GetCmdletParameters(false, true);
                        flagArray[2] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Method, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdletParameters");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Method, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdletParameters");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private ClassMetadataInstanceCmdlets Read31_ClassMetadataInstanceCmdlets(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id4_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            ClassMetadataInstanceCmdlets o = new ClassMetadataInstanceCmdlets();
            InstanceCmdletMetadata[] a = null;
            int length = 0;
            bool[] flagArray = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Cmdlet = (InstanceCmdletMetadata[]) base.ShrinkArray(a, length, typeof(InstanceCmdletMetadata), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id6_GetCmdletParameters)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.GetCmdletParameters = this.Read18_GetCmdletParameters(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id117_GetCmdlet)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.GetCmdlet = this.Read21_GetCmdletMetadata(false, true);
                        flagArray[1] = true;
                    }
                    else if ((base.Reader.LocalName == this.id76_Cmdlet) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        a = (InstanceCmdletMetadata[]) base.EnsureArrayIndex(a, length, typeof(InstanceCmdletMetadata));
                        a[length++] = this.Read30_InstanceCmdletMetadata(false, true);
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdletParameters, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdlet, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Cmdlet");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdletParameters, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdlet, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Cmdlet");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Cmdlet = (InstanceCmdletMetadata[]) base.ShrinkArray(a, length, typeof(InstanceCmdletMetadata), true);
            base.ReadEndElement();
            return o;
        }

        private StaticCmdletMetadataCmdletMetadata Read32_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id4_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            StaticCmdletMetadataCmdletMetadata o = new StaticCmdletMetadataCmdletMetadata();
            string[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id93_Verb)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Verb = base.Reader.Value;
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id94_Noun)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.Noun = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if ((base.Reader.LocalName == this.id47_Aliases) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        string[] strArray2 = base.Reader.Value.Split(null);
                        for (int i = 0; i < strArray2.Length; i++)
                        {
                            a = (string[]) base.EnsureArrayIndex(a, index, typeof(string));
                            a[index++] = strArray2[i];
                        }
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id22_ConfirmImpact)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.ConfirmImpact = this.Read19_ConfirmImpact(base.Reader.Value);
                        o.ConfirmImpactSpecified = true;
                        flagArray[3] = true;
                    }
                    else
                    {
                        if ((!flagArray[4] && (base.Reader.LocalName == this.id95_HelpUri)) && (base.Reader.NamespaceURI == this.id4_Item))
                        {
                            o.HelpUri = base.CollapseWhitespace(base.Reader.Value);
                            flagArray[4] = true;
                            continue;
                        }
                        if ((!flagArray[5] && (base.Reader.LocalName == this.id96_DefaultCmdletParameterSet)) && (base.Reader.NamespaceURI == this.id4_Item))
                        {
                            o.DefaultCmdletParameterSet = base.Reader.Value;
                            flagArray[5] = true;
                            continue;
                        }
                        if (!base.IsXmlnsAttribute(base.Reader.Name))
                        {
                            base.UnknownNode(o, ":Verb, :Noun, :Aliases, :ConfirmImpact, :HelpUri, :DefaultCmdletParameterSet");
                        }
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
            base.ReadEndElement();
            return o;
        }

        private StaticCmdletMetadata Read33_StaticCmdletMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id23_StaticCmdletMetadata) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            StaticCmdletMetadata o = new StaticCmdletMetadata();
            StaticMethodMetadata[] a = null;
            int length = 0;
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Method = (StaticMethodMetadata[]) base.ShrinkArray(a, length, typeof(StaticMethodMetadata), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id81_CmdletMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletMetadata = this.Read32_Item(false, true);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id82_Method) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        a = (StaticMethodMetadata[]) base.EnsureArrayIndex(a, length, typeof(StaticMethodMetadata));
                        a[length++] = this.Read27_StaticMethodMetadata(false, true);
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Method");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletMetadata, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Method");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Method = (StaticMethodMetadata[]) base.ShrinkArray(a, length, typeof(StaticMethodMetadata), true);
            base.ReadEndElement();
            return o;
        }

        private ClassMetadataData Read34_ClassMetadataData(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id4_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            ClassMetadataData o = new ClassMetadataData();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id40_Name)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[0] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, ":Name");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                string str = null;
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else if (((base.Reader.NodeType == XmlNodeType.Text) || (base.Reader.NodeType == XmlNodeType.CDATA)) || ((base.Reader.NodeType == XmlNodeType.Whitespace) || (base.Reader.NodeType == XmlNodeType.SignificantWhitespace)))
                {
                    str = base.ReadString(str, false);
                    o.Value = str;
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private ClassMetadata Read35_ClassMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id3_ClassMetadata) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            ClassMetadata o = new ClassMetadata();
            bool[] flagArray = new bool[8];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[5] && (base.Reader.LocalName == this.id109_CmdletAdapter)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.CmdletAdapter = base.Reader.Value;
                    flagArray[5] = true;
                }
                else
                {
                    if ((!flagArray[6] && (base.Reader.LocalName == this.id110_ClassName)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.ClassName = base.Reader.Value;
                        flagArray[6] = true;
                        continue;
                    }
                    if ((!flagArray[7] && (base.Reader.LocalName == this.id111_ClassVersion)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.ClassVersion = base.Reader.Value;
                        flagArray[7] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":CmdletAdapter, :ClassName, :ClassVersion");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id112_Version)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Version = base.Reader.ReadElementString();
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id113_DefaultNoun)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.DefaultNoun = base.Reader.ReadElementString();
                        flagArray[1] = true;
                    }
                    else if ((!flagArray[2] && (base.Reader.LocalName == this.id114_InstanceCmdlets)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.InstanceCmdlets = this.Read31_ClassMetadataInstanceCmdlets(false, true);
                        flagArray[2] = true;
                    }
                    else if ((base.Reader.LocalName == this.id115_StaticCmdlets) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            StaticCmdletMetadata[] a = null;
                            int index = 0;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num4 = 0;
                                int num5 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id76_Cmdlet) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            a = (StaticCmdletMetadata[]) base.EnsureArrayIndex(a, index, typeof(StaticCmdletMetadata));
                                            a[index++] = this.Read33_StaticCmdletMetadata(false, true);
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Cmdlet");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Cmdlet");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num4, ref num5);
                                }
                                base.ReadEndElement();
                            }
                            o.StaticCmdlets = (StaticCmdletMetadata[]) base.ShrinkArray(a, index, typeof(StaticCmdletMetadata), false);
                        }
                    }
                    else if ((base.Reader.LocalName == this.id116_CmdletAdapterPrivateData) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            ClassMetadataData[] dataArray = null;
                            int num6 = 0;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num7 = 0;
                                int num8 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id78_Data) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            dataArray = (ClassMetadataData[]) base.EnsureArrayIndex(dataArray, num6, typeof(ClassMetadataData));
                                            dataArray[num6++] = this.Read34_ClassMetadataData(false, true);
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Data");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Data");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num7, ref num8);
                                }
                                base.ReadEndElement();
                            }
                            o.CmdletAdapterPrivateData = (ClassMetadataData[]) base.ShrinkArray(dataArray, num6, typeof(ClassMetadataData), false);
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Version, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:DefaultNoun, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:InstanceCmdlets, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:StaticCmdlets, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletAdapterPrivateData");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Version, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:DefaultNoun, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:InstanceCmdlets, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:StaticCmdlets, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletAdapterPrivateData");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private EnumMetadataEnumValue Read36_EnumMetadataEnumValue(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id4_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            EnumMetadataEnumValue o = new EnumMetadataEnumValue();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id40_Name)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id41_Value)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.Value = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":Name, :Value");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private EnumMetadataEnum Read37_EnumMetadataEnum(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id4_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            EnumMetadataEnum o = new EnumMetadataEnum();
            EnumMetadataEnumValue[] a = null;
            int length = 0;
            bool[] flagArray = new bool[4];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id42_EnumName)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.EnumName = base.Reader.Value;
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id43_UnderlyingType)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.UnderlyingType = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id44_BitwiseFlags)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.BitwiseFlags = XmlConvert.ToBoolean(base.Reader.Value);
                        o.BitwiseFlagsSpecified = true;
                        flagArray[3] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":EnumName, :UnderlyingType, :BitwiseFlags");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Value = (EnumMetadataEnumValue[]) base.ShrinkArray(a, length, typeof(EnumMetadataEnumValue), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((base.Reader.LocalName == this.id41_Value) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        a = (EnumMetadataEnumValue[]) base.EnsureArrayIndex(a, length, typeof(EnumMetadataEnumValue));
                        a[length++] = this.Read36_EnumMetadataEnumValue(false, true);
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Value");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Value");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Value = (EnumMetadataEnumValue[]) base.ShrinkArray(a, length, typeof(EnumMetadataEnumValue), true);
            base.ReadEndElement();
            return o;
        }

        private PowerShellMetadata Read38_PowerShellMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id4_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            PowerShellMetadata o = new PowerShellMetadata();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id118_Class)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Class = this.Read35_ClassMetadata(false, true);
                        flagArray[0] = true;
                    }
                    else if ((base.Reader.LocalName == this.id119_Enums) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            EnumMetadataEnum[] a = null;
                            int index = 0;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num4 = 0;
                                int num5 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id80_Enum) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            a = (EnumMetadataEnum[]) base.EnsureArrayIndex(a, index, typeof(EnumMetadataEnum));
                                            a[index++] = this.Read37_EnumMetadataEnum(false, true);
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Enum");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Enum");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num4, ref num5);
                                }
                                base.ReadEndElement();
                            }
                            o.Enums = (EnumMetadataEnum[]) base.ShrinkArray(a, index, typeof(EnumMetadataEnum), false);
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Class, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Enums");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Class, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Enums");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private ClassMetadataInstanceCmdlets Read39_ClassMetadataInstanceCmdlets(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id5_ClassMetadataInstanceCmdlets) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            ClassMetadataInstanceCmdlets o = new ClassMetadataInstanceCmdlets();
            InstanceCmdletMetadata[] a = null;
            int length = 0;
            bool[] flagArray = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Cmdlet = (InstanceCmdletMetadata[]) base.ShrinkArray(a, length, typeof(InstanceCmdletMetadata), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id6_GetCmdletParameters)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.GetCmdletParameters = this.Read18_GetCmdletParameters(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id117_GetCmdlet)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.GetCmdlet = this.Read21_GetCmdletMetadata(false, true);
                        flagArray[1] = true;
                    }
                    else if ((base.Reader.LocalName == this.id76_Cmdlet) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        a = (InstanceCmdletMetadata[]) base.EnsureArrayIndex(a, length, typeof(InstanceCmdletMetadata));
                        a[length++] = this.Read30_InstanceCmdletMetadata(false, true);
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdletParameters, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdlet, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Cmdlet");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdletParameters, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:GetCmdlet, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Cmdlet");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Cmdlet = (InstanceCmdletMetadata[]) base.ShrinkArray(a, length, typeof(InstanceCmdletMetadata), true);
            base.ReadEndElement();
            return o;
        }

        private CmdletParameterMetadataValidateCount Read4_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id4_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            CmdletParameterMetadataValidateCount o = new CmdletParameterMetadataValidateCount();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id64_Min)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Min = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id65_Max)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.Max = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":Min, :Max");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private AssociationAssociatedInstance Read40_AssociationAssociatedInstance(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id10_AssociationAssociatedInstance) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            AssociationAssociatedInstance o = new AssociationAssociatedInstance();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id89_Type)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Type = this.Read2_TypeMetadata(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id11_CmdletParameterMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletParameterMetadata = this.Read11_Item(false, true);
                        flagArray[1] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletParameterMetadata");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private CmdletParameterMetadataValidateCount Read41_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id14_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            CmdletParameterMetadataValidateCount o = new CmdletParameterMetadataValidateCount();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id64_Min)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Min = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id65_Max)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.Max = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":Min, :Max");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private CmdletParameterMetadataValidateLength Read42_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id15_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            CmdletParameterMetadataValidateLength o = new CmdletParameterMetadataValidateLength();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id64_Min)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Min = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id65_Max)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.Max = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":Min, :Max");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private CmdletParameterMetadataValidateRange Read43_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id16_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            CmdletParameterMetadataValidateRange o = new CmdletParameterMetadataValidateRange();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id64_Min)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Min = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id65_Max)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.Max = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":Min, :Max");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private StaticCmdletMetadataCmdletMetadata Read44_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id24_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            StaticCmdletMetadataCmdletMetadata o = new StaticCmdletMetadataCmdletMetadata();
            string[] a = null;
            int index = 0;
            bool[] flagArray = new bool[6];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id93_Verb)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Verb = base.Reader.Value;
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id94_Noun)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.Noun = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if ((base.Reader.LocalName == this.id47_Aliases) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        string[] strArray2 = base.Reader.Value.Split(null);
                        for (int i = 0; i < strArray2.Length; i++)
                        {
                            a = (string[]) base.EnsureArrayIndex(a, index, typeof(string));
                            a[index++] = strArray2[i];
                        }
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id22_ConfirmImpact)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.ConfirmImpact = this.Read19_ConfirmImpact(base.Reader.Value);
                        o.ConfirmImpactSpecified = true;
                        flagArray[3] = true;
                    }
                    else
                    {
                        if ((!flagArray[4] && (base.Reader.LocalName == this.id95_HelpUri)) && (base.Reader.NamespaceURI == this.id4_Item))
                        {
                            o.HelpUri = base.CollapseWhitespace(base.Reader.Value);
                            flagArray[4] = true;
                            continue;
                        }
                        if ((!flagArray[5] && (base.Reader.LocalName == this.id96_DefaultCmdletParameterSet)) && (base.Reader.NamespaceURI == this.id4_Item))
                        {
                            o.DefaultCmdletParameterSet = base.Reader.Value;
                            flagArray[5] = true;
                            continue;
                        }
                        if (!base.IsXmlnsAttribute(base.Reader.Name))
                        {
                            base.UnknownNode(o, ":Verb, :Noun, :Aliases, :ConfirmImpact, :HelpUri, :DefaultCmdletParameterSet");
                        }
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
            base.ReadEndElement();
            return o;
        }

        private CommonMethodMetadataReturnValue Read45_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id31_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            CommonMethodMetadataReturnValue o = new CommonMethodMetadataReturnValue();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id89_Type)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.Type = this.Read2_TypeMetadata(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id29_CmdletOutputMetadata)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.CmdletOutputMetadata = this.Read22_CmdletOutputMetadata(false, true);
                        flagArray[1] = true;
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletOutputMetadata");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Type, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:CmdletOutputMetadata");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private ClassMetadataData Read46_ClassMetadataData(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id37_ClassMetadataData) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            ClassMetadataData o = new ClassMetadataData();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id40_Name)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[0] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, ":Name");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                string str = null;
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else if (((base.Reader.NodeType == XmlNodeType.Text) || (base.Reader.NodeType == XmlNodeType.CDATA)) || ((base.Reader.NodeType == XmlNodeType.Whitespace) || (base.Reader.NodeType == XmlNodeType.SignificantWhitespace)))
                {
                    str = base.ReadString(str, false);
                    o.Value = str;
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private EnumMetadataEnum Read47_EnumMetadataEnum(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id38_EnumMetadataEnum) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            EnumMetadataEnum o = new EnumMetadataEnum();
            EnumMetadataEnumValue[] a = null;
            int length = 0;
            bool[] flagArray = new bool[4];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[1] && (base.Reader.LocalName == this.id42_EnumName)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.EnumName = base.Reader.Value;
                    flagArray[1] = true;
                }
                else
                {
                    if ((!flagArray[2] && (base.Reader.LocalName == this.id43_UnderlyingType)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.UnderlyingType = base.Reader.Value;
                        flagArray[2] = true;
                        continue;
                    }
                    if ((!flagArray[3] && (base.Reader.LocalName == this.id44_BitwiseFlags)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.BitwiseFlags = XmlConvert.ToBoolean(base.Reader.Value);
                        o.BitwiseFlagsSpecified = true;
                        flagArray[3] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":EnumName, :UnderlyingType, :BitwiseFlags");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Value = (EnumMetadataEnumValue[]) base.ShrinkArray(a, length, typeof(EnumMetadataEnumValue), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((base.Reader.LocalName == this.id41_Value) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        a = (EnumMetadataEnumValue[]) base.EnsureArrayIndex(a, length, typeof(EnumMetadataEnumValue));
                        a[length++] = this.Read36_EnumMetadataEnumValue(false, true);
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Value");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:Value");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Value = (EnumMetadataEnumValue[]) base.ShrinkArray(a, length, typeof(EnumMetadataEnumValue), true);
            base.ReadEndElement();
            return o;
        }

        private EnumMetadataEnumValue Read48_EnumMetadataEnumValue(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id39_EnumMetadataEnumValue) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            EnumMetadataEnumValue o = new EnumMetadataEnumValue();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id40_Name)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Name = base.Reader.Value;
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id41_Value)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.Value = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":Name, :Value");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        public object Read49_PowerShellMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id1_PowerShellMetadata) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read38_PowerShellMetadata(false, true);
            }
            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:PowerShellMetadata");
            return null;
        }

        private CmdletParameterMetadataValidateLength Read5_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id4_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            CmdletParameterMetadataValidateLength o = new CmdletParameterMetadataValidateLength();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id64_Min)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Min = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id65_Max)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.Max = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":Min, :Max");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        public object Read50_ClassMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id3_ClassMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read35_ClassMetadata(true, true);
            }
            base.UnknownNode(null, ":ClassMetadata");
            return null;
        }

        public object Read51_ClassMetadataInstanceCmdlets()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id5_ClassMetadataInstanceCmdlets) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read39_ClassMetadataInstanceCmdlets(true, true);
            }
            base.UnknownNode(null, ":ClassMetadataInstanceCmdlets");
            return null;
        }

        public object Read52_GetCmdletParameters()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id6_GetCmdletParameters) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read18_GetCmdletParameters(true, true);
            }
            base.UnknownNode(null, ":GetCmdletParameters");
            return null;
        }

        public object Read53_PropertyMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id7_PropertyMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read14_PropertyMetadata(true, true);
            }
            base.UnknownNode(null, ":PropertyMetadata");
            return null;
        }

        public object Read54_TypeMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id8_TypeMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read2_TypeMetadata(true, true);
            }
            base.UnknownNode(null, ":TypeMetadata");
            return null;
        }

        public object Read55_Association()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id9_Association) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read16_Association(true, true);
            }
            base.UnknownNode(null, ":Association");
            return null;
        }

        public object Read56_AssociationAssociatedInstance()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id10_AssociationAssociatedInstance) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read40_AssociationAssociatedInstance(true, true);
            }
            base.UnknownNode(null, ":AssociationAssociatedInstance");
            return null;
        }

        public object Read57_CmdletParameterMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id11_CmdletParameterMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read9_CmdletParameterMetadata(true, true);
            }
            base.UnknownNode(null, ":CmdletParameterMetadata");
            return null;
        }

        public object Read58_Item()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id12_Item) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read10_Item(true, true);
            }
            base.UnknownNode(null, ":CmdletParameterMetadataForGetCmdletParameter");
            return null;
        }

        public object Read59_Item()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id13_Item) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read11_Item(true, true);
            }
            base.UnknownNode(null, ":CmdletParameterMetadataForGetCmdletFilteringParameter");
            return null;
        }

        private CmdletParameterMetadataValidateRange Read6_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id4_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            CmdletParameterMetadataValidateRange o = new CmdletParameterMetadataValidateRange();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id64_Min)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.Min = base.CollapseWhitespace(base.Reader.Value);
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id65_Max)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.Max = base.CollapseWhitespace(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":Min, :Max");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        public object Read60_Item()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id14_Item) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read41_Item(true, true);
            }
            base.UnknownNode(null, ":CmdletParameterMetadataValidateCount");
            return null;
        }

        public object Read61_Item()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id15_Item) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read42_Item(true, true);
            }
            base.UnknownNode(null, ":CmdletParameterMetadataValidateLength");
            return null;
        }

        public object Read62_Item()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id16_Item) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read43_Item(true, true);
            }
            base.UnknownNode(null, ":CmdletParameterMetadataValidateRange");
            return null;
        }

        public object Read63_Item()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id17_Item) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read7_Item(true, true);
            }
            base.UnknownNode(null, ":CmdletParameterMetadataForInstanceMethodParameter");
            return null;
        }

        public object Read64_Item()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id18_Item) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read8_Item(true, true);
            }
            base.UnknownNode(null, ":CmdletParameterMetadataForStaticMethodParameter");
            return null;
        }

        public object Read65_QueryOption()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id19_QueryOption) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read17_QueryOption(true, true);
            }
            base.UnknownNode(null, ":QueryOption");
            return null;
        }

        public object Read66_GetCmdletMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id20_GetCmdletMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read21_GetCmdletMetadata(true, true);
            }
            base.UnknownNode(null, ":GetCmdletMetadata");
            return null;
        }

        public object Read67_CommonCmdletMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id21_CommonCmdletMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read20_CommonCmdletMetadata(true, true);
            }
            base.UnknownNode(null, ":CommonCmdletMetadata");
            return null;
        }

        public object Read68_ConfirmImpact()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id22_ConfirmImpact) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read19_ConfirmImpact(base.Reader.ReadElementString());
            }
            base.UnknownNode(null, ":ConfirmImpact");
            return null;
        }

        public object Read69_StaticCmdletMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id23_StaticCmdletMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read33_StaticCmdletMetadata(true, true);
            }
            base.UnknownNode(null, ":StaticCmdletMetadata");
            return null;
        }

        private CmdletParameterMetadataForInstanceMethodParameter Read7_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id17_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            CmdletParameterMetadataForInstanceMethodParameter o = new CmdletParameterMetadataForInstanceMethodParameter();
            string[] a = null;
            int index = 0;
            bool[] flagArray = new bool[14];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[9] && (base.Reader.LocalName == this.id46_IsMandatory)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.IsMandatory = XmlConvert.ToBoolean(base.Reader.Value);
                    o.IsMandatorySpecified = true;
                    flagArray[9] = true;
                }
                else
                {
                    if ((base.Reader.LocalName == this.id47_Aliases) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        string[] strArray2 = base.Reader.Value.Split(null);
                        for (int i = 0; i < strArray2.Length; i++)
                        {
                            a = (string[]) base.EnsureArrayIndex(a, index, typeof(string));
                            a[index++] = strArray2[i];
                        }
                        continue;
                    }
                    if ((!flagArray[11] && (base.Reader.LocalName == this.id48_PSName)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.PSName = base.Reader.Value;
                        flagArray[11] = true;
                    }
                    else
                    {
                        if ((!flagArray[12] && (base.Reader.LocalName == this.id49_Position)) && (base.Reader.NamespaceURI == this.id4_Item))
                        {
                            o.Position = base.CollapseWhitespace(base.Reader.Value);
                            flagArray[12] = true;
                            continue;
                        }
                        if ((!flagArray[13] && (base.Reader.LocalName == this.id51_Item)) && (base.Reader.NamespaceURI == this.id4_Item))
                        {
                            o.ValueFromPipelineByPropertyName = XmlConvert.ToBoolean(base.Reader.Value);
                            o.ValueFromPipelineByPropertyNameSpecified = true;
                            flagArray[13] = true;
                            continue;
                        }
                        if (!base.IsXmlnsAttribute(base.Reader.Name))
                        {
                            base.UnknownNode(o, ":IsMandatory, :Aliases, :PSName, :Position, :ValueFromPipelineByPropertyName");
                        }
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id54_AllowEmptyCollection)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowEmptyCollection = this.Read1_Object(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id55_AllowEmptyString)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowEmptyString = this.Read1_Object(false, true);
                        flagArray[1] = true;
                    }
                    else if ((!flagArray[2] && (base.Reader.LocalName == this.id56_AllowNull)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowNull = this.Read1_Object(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[3] && (base.Reader.LocalName == this.id57_ValidateNotNull)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateNotNull = this.Read1_Object(false, true);
                        flagArray[3] = true;
                    }
                    else if ((!flagArray[4] && (base.Reader.LocalName == this.id58_ValidateNotNullOrEmpty)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateNotNullOrEmpty = this.Read1_Object(false, true);
                        flagArray[4] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id59_ValidateCount)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateCount = this.Read4_Item(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[6] && (base.Reader.LocalName == this.id60_ValidateLength)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateLength = this.Read5_Item(false, true);
                        flagArray[6] = true;
                    }
                    else if ((!flagArray[7] && (base.Reader.LocalName == this.id61_ValidateRange)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateRange = this.Read6_Item(false, true);
                        flagArray[7] = true;
                    }
                    else if ((base.Reader.LocalName == this.id62_ValidateSet) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            string[] strArray3 = null;
                            int num5 = 0;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num6 = 0;
                                int num7 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id63_AllowedValue) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            strArray3 = (string[]) base.EnsureArrayIndex(strArray3, num5, typeof(string));
                                            strArray3[num5++] = base.Reader.ReadElementString();
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowedValue");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowedValue");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num6, ref num7);
                                }
                                base.ReadEndElement();
                            }
                            o.ValidateSet = (string[]) base.ShrinkArray(strArray3, num5, typeof(string), false);
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyCollection, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyString, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNullOrEmpty, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateCount, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateLength, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateRange, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateSet");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyCollection, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyString, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNullOrEmpty, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateCount, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateLength, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateRange, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateSet");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
            base.ReadEndElement();
            return o;
        }

        public object Read70_Item()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id24_Item) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read44_Item(true, true);
            }
            base.UnknownNode(null, ":StaticCmdletMetadataCmdletMetadata");
            return null;
        }

        public object Read71_CommonMethodMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id25_CommonMethodMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read28_CommonMethodMetadata(true, true);
            }
            base.UnknownNode(null, ":CommonMethodMetadata");
            return null;
        }

        public object Read72_StaticMethodMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id26_StaticMethodMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read27_StaticMethodMetadata(true, true);
            }
            base.UnknownNode(null, ":StaticMethodMetadata");
            return null;
        }

        public object Read73_CommonMethodParameterMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id27_CommonMethodParameterMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read25_CommonMethodParameterMetadata(true, true);
            }
            base.UnknownNode(null, ":CommonMethodParameterMetadata");
            return null;
        }

        public object Read74_StaticMethodParameterMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id28_StaticMethodParameterMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read26_StaticMethodParameterMetadata(true, true);
            }
            base.UnknownNode(null, ":StaticMethodParameterMetadata");
            return null;
        }

        public object Read75_CmdletOutputMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id29_CmdletOutputMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read22_CmdletOutputMetadata(true, true);
            }
            base.UnknownNode(null, ":CmdletOutputMetadata");
            return null;
        }

        public object Read76_Item()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id30_Item) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read24_Item(true, true);
            }
            base.UnknownNode(null, ":InstanceMethodParameterMetadata");
            return null;
        }

        public object Read77_Item()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id31_Item) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read45_Item(true, true);
            }
            base.UnknownNode(null, ":CommonMethodMetadataReturnValue");
            return null;
        }

        public object Read78_InstanceMethodMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id32_InstanceMethodMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read29_InstanceMethodMetadata(true, true);
            }
            base.UnknownNode(null, ":InstanceMethodMetadata");
            return null;
        }

        public object Read79_InstanceCmdletMetadata()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id33_InstanceCmdletMetadata) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read30_InstanceCmdletMetadata(true, true);
            }
            base.UnknownNode(null, ":InstanceCmdletMetadata");
            return null;
        }

        private CmdletParameterMetadataForStaticMethodParameter Read8_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id18_Item) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            CmdletParameterMetadataForStaticMethodParameter o = new CmdletParameterMetadataForStaticMethodParameter();
            string[] a = null;
            int index = 0;
            bool[] flagArray = new bool[15];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[9] && (base.Reader.LocalName == this.id46_IsMandatory)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.IsMandatory = XmlConvert.ToBoolean(base.Reader.Value);
                    o.IsMandatorySpecified = true;
                    flagArray[9] = true;
                }
                else
                {
                    if ((base.Reader.LocalName == this.id47_Aliases) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        string[] strArray2 = base.Reader.Value.Split(null);
                        for (int i = 0; i < strArray2.Length; i++)
                        {
                            a = (string[]) base.EnsureArrayIndex(a, index, typeof(string));
                            a[index++] = strArray2[i];
                        }
                        continue;
                    }
                    if ((!flagArray[11] && (base.Reader.LocalName == this.id48_PSName)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.PSName = base.Reader.Value;
                        flagArray[11] = true;
                    }
                    else
                    {
                        if ((!flagArray[12] && (base.Reader.LocalName == this.id49_Position)) && (base.Reader.NamespaceURI == this.id4_Item))
                        {
                            o.Position = base.CollapseWhitespace(base.Reader.Value);
                            flagArray[12] = true;
                            continue;
                        }
                        if ((!flagArray[13] && (base.Reader.LocalName == this.id50_ValueFromPipeline)) && (base.Reader.NamespaceURI == this.id4_Item))
                        {
                            o.ValueFromPipeline = XmlConvert.ToBoolean(base.Reader.Value);
                            o.ValueFromPipelineSpecified = true;
                            flagArray[13] = true;
                            continue;
                        }
                        if ((!flagArray[14] && (base.Reader.LocalName == this.id51_Item)) && (base.Reader.NamespaceURI == this.id4_Item))
                        {
                            o.ValueFromPipelineByPropertyName = XmlConvert.ToBoolean(base.Reader.Value);
                            o.ValueFromPipelineByPropertyNameSpecified = true;
                            flagArray[14] = true;
                            continue;
                        }
                        if (!base.IsXmlnsAttribute(base.Reader.Name))
                        {
                            base.UnknownNode(o, ":IsMandatory, :Aliases, :PSName, :Position, :ValueFromPipeline, :ValueFromPipelineByPropertyName");
                        }
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id54_AllowEmptyCollection)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowEmptyCollection = this.Read1_Object(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id55_AllowEmptyString)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowEmptyString = this.Read1_Object(false, true);
                        flagArray[1] = true;
                    }
                    else if ((!flagArray[2] && (base.Reader.LocalName == this.id56_AllowNull)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowNull = this.Read1_Object(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[3] && (base.Reader.LocalName == this.id57_ValidateNotNull)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateNotNull = this.Read1_Object(false, true);
                        flagArray[3] = true;
                    }
                    else if ((!flagArray[4] && (base.Reader.LocalName == this.id58_ValidateNotNullOrEmpty)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateNotNullOrEmpty = this.Read1_Object(false, true);
                        flagArray[4] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id59_ValidateCount)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateCount = this.Read4_Item(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[6] && (base.Reader.LocalName == this.id60_ValidateLength)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateLength = this.Read5_Item(false, true);
                        flagArray[6] = true;
                    }
                    else if ((!flagArray[7] && (base.Reader.LocalName == this.id61_ValidateRange)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateRange = this.Read6_Item(false, true);
                        flagArray[7] = true;
                    }
                    else if ((base.Reader.LocalName == this.id62_ValidateSet) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            string[] strArray3 = null;
                            int num5 = 0;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num6 = 0;
                                int num7 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id63_AllowedValue) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            strArray3 = (string[]) base.EnsureArrayIndex(strArray3, num5, typeof(string));
                                            strArray3[num5++] = base.Reader.ReadElementString();
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowedValue");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowedValue");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num6, ref num7);
                                }
                                base.ReadEndElement();
                            }
                            o.ValidateSet = (string[]) base.ShrinkArray(strArray3, num5, typeof(string), false);
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyCollection, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyString, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNullOrEmpty, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateCount, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateLength, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateRange, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateSet");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyCollection, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyString, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNullOrEmpty, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateCount, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateLength, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateRange, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateSet");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
            base.ReadEndElement();
            return o;
        }

        public object Read80_PropertyQuery()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id34_PropertyQuery) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read13_PropertyQuery(true, true);
            }
            base.UnknownNode(null, ":PropertyQuery");
            return null;
        }

        public object Read81_WildcardablePropertyQuery()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id35_WildcardablePropertyQuery) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read12_WildcardablePropertyQuery(true, true);
            }
            base.UnknownNode(null, ":WildcardablePropertyQuery");
            return null;
        }

        public object Read82_ItemsChoiceType()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id36_ItemsChoiceType) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read3_ItemsChoiceType(base.Reader.ReadElementString());
            }
            base.UnknownNode(null, ":ItemsChoiceType");
            return null;
        }

        public object Read83_ClassMetadataData()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id37_ClassMetadataData) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read46_ClassMetadataData(true, true);
            }
            base.UnknownNode(null, ":ClassMetadataData");
            return null;
        }

        public object Read84_EnumMetadataEnum()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id38_EnumMetadataEnum) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read47_EnumMetadataEnum(true, true);
            }
            base.UnknownNode(null, ":EnumMetadataEnum");
            return null;
        }

        public object Read85_EnumMetadataEnumValue()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id39_EnumMetadataEnumValue) || (base.Reader.NamespaceURI != this.id4_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read48_EnumMetadataEnumValue(true, true);
            }
            base.UnknownNode(null, ":EnumMetadataEnumValue");
            return null;
        }

        private CmdletParameterMetadata Read9_CmdletParameterMetadata(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id11_CmdletParameterMetadata) || (type.Namespace != this.id2_Item)))
            {
                if ((type.Name == this.id12_Item) && (type.Namespace == this.id2_Item))
                {
                    return this.Read10_Item(isNullable, false);
                }
                if ((type.Name == this.id13_Item) && (type.Namespace == this.id2_Item))
                {
                    return this.Read11_Item(isNullable, false);
                }
                if ((type.Name == this.id18_Item) && (type.Namespace == this.id2_Item))
                {
                    return this.Read8_Item(isNullable, false);
                }
                if ((type.Name != this.id17_Item) || (type.Namespace != this.id2_Item))
                {
                    throw base.CreateUnknownTypeException(type);
                }
                return this.Read7_Item(isNullable, false);
            }
            if (flag)
            {
                return null;
            }
            CmdletParameterMetadata o = new CmdletParameterMetadata();
            string[] a = null;
            int index = 0;
            bool[] flagArray = new bool[13];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[9] && (base.Reader.LocalName == this.id46_IsMandatory)) && (base.Reader.NamespaceURI == this.id4_Item))
                {
                    o.IsMandatory = XmlConvert.ToBoolean(base.Reader.Value);
                    o.IsMandatorySpecified = true;
                    flagArray[9] = true;
                }
                else
                {
                    if ((base.Reader.LocalName == this.id47_Aliases) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        string[] strArray2 = base.Reader.Value.Split(null);
                        for (int i = 0; i < strArray2.Length; i++)
                        {
                            a = (string[]) base.EnsureArrayIndex(a, index, typeof(string));
                            a[index++] = strArray2[i];
                        }
                        continue;
                    }
                    if ((!flagArray[11] && (base.Reader.LocalName == this.id48_PSName)) && (base.Reader.NamespaceURI == this.id4_Item))
                    {
                        o.PSName = base.Reader.Value;
                        flagArray[11] = true;
                    }
                    else
                    {
                        if ((!flagArray[12] && (base.Reader.LocalName == this.id49_Position)) && (base.Reader.NamespaceURI == this.id4_Item))
                        {
                            o.Position = base.CollapseWhitespace(base.Reader.Value);
                            flagArray[12] = true;
                            continue;
                        }
                        if (!base.IsXmlnsAttribute(base.Reader.Name))
                        {
                            base.UnknownNode(o, ":IsMandatory, :Aliases, :PSName, :Position");
                        }
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((!flagArray[0] && (base.Reader.LocalName == this.id54_AllowEmptyCollection)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowEmptyCollection = this.Read1_Object(false, true);
                        flagArray[0] = true;
                    }
                    else if ((!flagArray[1] && (base.Reader.LocalName == this.id55_AllowEmptyString)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowEmptyString = this.Read1_Object(false, true);
                        flagArray[1] = true;
                    }
                    else if ((!flagArray[2] && (base.Reader.LocalName == this.id56_AllowNull)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.AllowNull = this.Read1_Object(false, true);
                        flagArray[2] = true;
                    }
                    else if ((!flagArray[3] && (base.Reader.LocalName == this.id57_ValidateNotNull)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateNotNull = this.Read1_Object(false, true);
                        flagArray[3] = true;
                    }
                    else if ((!flagArray[4] && (base.Reader.LocalName == this.id58_ValidateNotNullOrEmpty)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateNotNullOrEmpty = this.Read1_Object(false, true);
                        flagArray[4] = true;
                    }
                    else if ((!flagArray[5] && (base.Reader.LocalName == this.id59_ValidateCount)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateCount = this.Read4_Item(false, true);
                        flagArray[5] = true;
                    }
                    else if ((!flagArray[6] && (base.Reader.LocalName == this.id60_ValidateLength)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateLength = this.Read5_Item(false, true);
                        flagArray[6] = true;
                    }
                    else if ((!flagArray[7] && (base.Reader.LocalName == this.id61_ValidateRange)) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        o.ValidateRange = this.Read6_Item(false, true);
                        flagArray[7] = true;
                    }
                    else if ((base.Reader.LocalName == this.id62_ValidateSet) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (!base.ReadNull())
                        {
                            string[] strArray3 = null;
                            int num5 = 0;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num6 = 0;
                                int num7 = base.ReaderCount;
                                while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if ((base.Reader.LocalName == this.id63_AllowedValue) && (base.Reader.NamespaceURI == this.id2_Item))
                                        {
                                            strArray3 = (string[]) base.EnsureArrayIndex(strArray3, num5, typeof(string));
                                            strArray3[num5++] = base.Reader.ReadElementString();
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowedValue");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowedValue");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num6, ref num7);
                                }
                                base.ReadEndElement();
                            }
                            o.ValidateSet = (string[]) base.ShrinkArray(strArray3, num5, typeof(string), false);
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyCollection, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyString, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNullOrEmpty, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateCount, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateLength, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateRange, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateSet");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyCollection, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowEmptyString, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:AllowNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNull, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateNotNullOrEmpty, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateCount, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateLength, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateRange, http://schemas.microsoft.com/cmdlets-over-objects/2009/11:ValidateSet");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            o.Aliases = (string[]) base.ShrinkArray(a, index, typeof(string), true);
            base.ReadEndElement();
            return o;
        }
    }
}

