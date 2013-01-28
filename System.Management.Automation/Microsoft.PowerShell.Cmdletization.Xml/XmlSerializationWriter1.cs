namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.Globalization;
    using System.Xml;
    using System.Xml.Serialization;

    [GeneratedCode("sgen", "4.0")]
    internal class XmlSerializationWriter1 : XmlSerializationWriter
    {
        protected override void InitCallbacks()
        {
        }

        private void Write1_Object(string n, string ns, object o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType)
                {
                    Type type = o.GetType();
                    if (type != typeof(object))
                    {
                        if (type == typeof(EnumMetadataEnumValue))
                        {
                            this.Write48_EnumMetadataEnumValue(n, ns, (EnumMetadataEnumValue) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(EnumMetadataEnum))
                        {
                            this.Write47_EnumMetadataEnum(n, ns, (EnumMetadataEnum) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(ClassMetadataData))
                        {
                            this.Write46_ClassMetadataData(n, ns, (ClassMetadataData) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CommonMethodMetadataReturnValue))
                        {
                            this.Write45_Item(n, ns, (CommonMethodMetadataReturnValue) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CmdletParameterMetadataValidateRange))
                        {
                            this.Write43_Item(n, ns, (CmdletParameterMetadataValidateRange) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CmdletParameterMetadataValidateLength))
                        {
                            this.Write42_Item(n, ns, (CmdletParameterMetadataValidateLength) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CmdletParameterMetadataValidateCount))
                        {
                            this.Write41_Item(n, ns, (CmdletParameterMetadataValidateCount) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(AssociationAssociatedInstance))
                        {
                            this.Write40_AssociationAssociatedInstance(n, ns, (AssociationAssociatedInstance) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(ClassMetadataInstanceCmdlets))
                        {
                            this.Write39_ClassMetadataInstanceCmdlets(n, ns, (ClassMetadataInstanceCmdlets) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(ClassMetadata))
                        {
                            this.Write35_ClassMetadata(n, ns, (ClassMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(StaticCmdletMetadata))
                        {
                            this.Write33_StaticCmdletMetadata(n, ns, (StaticCmdletMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(InstanceCmdletMetadata))
                        {
                            this.Write30_InstanceCmdletMetadata(n, ns, (InstanceCmdletMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CommonMethodParameterMetadata))
                        {
                            this.Write25_CommonMethodParameterMetadata(n, ns, (CommonMethodParameterMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(StaticMethodParameterMetadata))
                        {
                            this.Write26_StaticMethodParameterMetadata(n, ns, (StaticMethodParameterMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(InstanceMethodParameterMetadata))
                        {
                            this.Write24_Item(n, ns, (InstanceMethodParameterMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CommonMethodMetadata))
                        {
                            this.Write28_CommonMethodMetadata(n, ns, (CommonMethodMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(InstanceMethodMetadata))
                        {
                            this.Write29_InstanceMethodMetadata(n, ns, (InstanceMethodMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(StaticMethodMetadata))
                        {
                            this.Write27_StaticMethodMetadata(n, ns, (StaticMethodMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CmdletOutputMetadata))
                        {
                            this.Write22_CmdletOutputMetadata(n, ns, (CmdletOutputMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(GetCmdletMetadata))
                        {
                            this.Write21_GetCmdletMetadata(n, ns, (GetCmdletMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CommonCmdletMetadata))
                        {
                            this.Write20_CommonCmdletMetadata(n, ns, (CommonCmdletMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(StaticCmdletMetadataCmdletMetadata))
                        {
                            this.Write44_Item(n, ns, (StaticCmdletMetadataCmdletMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(GetCmdletParameters))
                        {
                            this.Write18_GetCmdletParameters(n, ns, (GetCmdletParameters) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(QueryOption))
                        {
                            this.Write17_QueryOption(n, ns, (QueryOption) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(Association))
                        {
                            this.Write16_Association(n, ns, (Association) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(PropertyMetadata))
                        {
                            this.Write14_PropertyMetadata(n, ns, (PropertyMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(PropertyQuery))
                        {
                            this.Write13_PropertyQuery(n, ns, (PropertyQuery) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(WildcardablePropertyQuery))
                        {
                            this.Write12_WildcardablePropertyQuery(n, ns, (WildcardablePropertyQuery) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CmdletParameterMetadata))
                        {
                            this.Write9_CmdletParameterMetadata(n, ns, (CmdletParameterMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CmdletParameterMetadataForGetCmdletParameter))
                        {
                            this.Write10_Item(n, ns, (CmdletParameterMetadataForGetCmdletParameter) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CmdletParameterMetadataForGetCmdletFilteringParameter))
                        {
                            this.Write11_Item(n, ns, (CmdletParameterMetadataForGetCmdletFilteringParameter) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CmdletParameterMetadataForStaticMethodParameter))
                        {
                            this.Write8_Item(n, ns, (CmdletParameterMetadataForStaticMethodParameter) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CmdletParameterMetadataForInstanceMethodParameter))
                        {
                            this.Write7_Item(n, ns, (CmdletParameterMetadataForInstanceMethodParameter) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(TypeMetadata))
                        {
                            this.Write2_TypeMetadata(n, ns, (TypeMetadata) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(ItemsChoiceType))
                        {
                            base.Writer.WriteStartElement(n, ns);
                            base.WriteXsiType("ItemsChoiceType", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                            base.Writer.WriteString(this.Write3_ItemsChoiceType((ItemsChoiceType) o));
                            base.Writer.WriteEndElement();
                            return;
                        }
                        if (type == typeof(string[]))
                        {
                            base.Writer.WriteStartElement(n, ns);
                            base.WriteXsiType("ArrayOfString", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                            string[] strArray = (string[]) o;
                            if (strArray != null)
                            {
                                for (int i = 0; i < strArray.Length; i++)
                                {
                                    base.WriteElementString("AllowedValue", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", strArray[i]);
                                }
                            }
                            base.Writer.WriteEndElement();
                            return;
                        }
                        if (type == typeof(PropertyMetadata[]))
                        {
                            base.Writer.WriteStartElement(n, ns);
                            base.WriteXsiType("ArrayOfPropertyMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                            PropertyMetadata[] metadataArray = (PropertyMetadata[]) o;
                            if (metadataArray != null)
                            {
                                for (int j = 0; j < metadataArray.Length; j++)
                                {
                                    this.Write14_PropertyMetadata("Property", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", metadataArray[j], false, false);
                                }
                            }
                            base.Writer.WriteEndElement();
                            return;
                        }
                        if (type == typeof(Association[]))
                        {
                            base.Writer.WriteStartElement(n, ns);
                            base.WriteXsiType("ArrayOfAssociation", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                            Association[] associationArray = (Association[]) o;
                            if (associationArray != null)
                            {
                                for (int k = 0; k < associationArray.Length; k++)
                                {
                                    this.Write16_Association("Association", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", associationArray[k], false, false);
                                }
                            }
                            base.Writer.WriteEndElement();
                            return;
                        }
                        if (type == typeof(QueryOption[]))
                        {
                            base.Writer.WriteStartElement(n, ns);
                            base.WriteXsiType("ArrayOfQueryOption", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                            QueryOption[] optionArray = (QueryOption[]) o;
                            if (optionArray != null)
                            {
                                for (int m = 0; m < optionArray.Length; m++)
                                {
                                    this.Write17_QueryOption("Option", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", optionArray[m], false, false);
                                }
                            }
                            base.Writer.WriteEndElement();
                            return;
                        }
                        if (type == typeof(ConfirmImpact))
                        {
                            base.Writer.WriteStartElement(n, ns);
                            base.WriteXsiType("ConfirmImpact", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                            base.Writer.WriteString(this.Write19_ConfirmImpact((ConfirmImpact) o));
                            base.Writer.WriteEndElement();
                            return;
                        }
                        if (type == typeof(StaticMethodParameterMetadata[]))
                        {
                            base.Writer.WriteStartElement(n, ns);
                            base.WriteXsiType("ArrayOfStaticMethodParameterMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                            StaticMethodParameterMetadata[] metadataArray2 = (StaticMethodParameterMetadata[]) o;
                            if (metadataArray2 != null)
                            {
                                for (int num5 = 0; num5 < metadataArray2.Length; num5++)
                                {
                                    this.Write26_StaticMethodParameterMetadata("Parameter", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", metadataArray2[num5], false, false);
                                }
                            }
                            base.Writer.WriteEndElement();
                            return;
                        }
                        if (type == typeof(InstanceMethodParameterMetadata[]))
                        {
                            base.Writer.WriteStartElement(n, ns);
                            base.WriteXsiType("ArrayOfInstanceMethodParameterMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                            InstanceMethodParameterMetadata[] metadataArray3 = (InstanceMethodParameterMetadata[]) o;
                            if (metadataArray3 != null)
                            {
                                for (int num6 = 0; num6 < metadataArray3.Length; num6++)
                                {
                                    this.Write24_Item("Parameter", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", metadataArray3[num6], false, false);
                                }
                            }
                            base.Writer.WriteEndElement();
                            return;
                        }
                        if (type == typeof(StaticCmdletMetadata[]))
                        {
                            base.Writer.WriteStartElement(n, ns);
                            base.WriteXsiType("ArrayOfStaticCmdletMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                            StaticCmdletMetadata[] metadataArray4 = (StaticCmdletMetadata[]) o;
                            if (metadataArray4 != null)
                            {
                                for (int num7 = 0; num7 < metadataArray4.Length; num7++)
                                {
                                    this.Write33_StaticCmdletMetadata("Cmdlet", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", metadataArray4[num7], false, false);
                                }
                            }
                            base.Writer.WriteEndElement();
                            return;
                        }
                        if (type == typeof(ClassMetadataData[]))
                        {
                            base.Writer.WriteStartElement(n, ns);
                            base.WriteXsiType("ArrayOfClassMetadataData", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                            ClassMetadataData[] dataArray = (ClassMetadataData[]) o;
                            if (dataArray != null)
                            {
                                for (int num8 = 0; num8 < dataArray.Length; num8++)
                                {
                                    this.Write34_ClassMetadataData("Data", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", dataArray[num8], false, false);
                                }
                            }
                            base.Writer.WriteEndElement();
                            return;
                        }
                        if (type == typeof(EnumMetadataEnum[]))
                        {
                            base.Writer.WriteStartElement(n, ns);
                            base.WriteXsiType("ArrayOfEnumMetadataEnum", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                            EnumMetadataEnum[] enumArray = (EnumMetadataEnum[]) o;
                            if (enumArray != null)
                            {
                                for (int num9 = 0; num9 < enumArray.Length; num9++)
                                {
                                    this.Write37_EnumMetadataEnum("Enum", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", enumArray[num9], false, false);
                                }
                            }
                            base.Writer.WriteEndElement();
                            return;
                        }
                        base.WriteTypedPrimitive(n, ns, o, true);
                        return;
                    }
                }
                base.WriteStartElement(n, ns, o, false, null);
                base.WriteEndElement(o);
            }
        }

        private void Write10_Item(string n, string ns, CmdletParameterMetadataForGetCmdletParameter o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType)
                {
                    Type type = o.GetType();
                    if (type != typeof(CmdletParameterMetadataForGetCmdletParameter))
                    {
                        if (type != typeof(CmdletParameterMetadataForGetCmdletFilteringParameter))
                        {
                            throw base.CreateUnknownTypeException(o);
                        }
                        this.Write11_Item(n, ns, (CmdletParameterMetadataForGetCmdletFilteringParameter) o, isNullable, true);
                        return;
                    }
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("CmdletParameterMetadataForGetCmdletParameter", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                if (o.IsMandatorySpecified)
                {
                    base.WriteAttribute("IsMandatory", "", XmlConvert.ToString(o.IsMandatory));
                }
                string[] aliases = o.Aliases;
                if (aliases != null)
                {
                    base.Writer.WriteStartAttribute(null, "Aliases", "");
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        string str = aliases[i];
                        if (i != 0)
                        {
                            base.Writer.WriteString(" ");
                        }
                        base.WriteValue(str);
                    }
                    base.Writer.WriteEndAttribute();
                }
                base.WriteAttribute("PSName", "", o.PSName);
                base.WriteAttribute("Position", "", o.Position);
                if (o.ValueFromPipelineSpecified)
                {
                    base.WriteAttribute("ValueFromPipeline", "", XmlConvert.ToString(o.ValueFromPipeline));
                }
                if (o.ValueFromPipelineByPropertyNameSpecified)
                {
                    base.WriteAttribute("ValueFromPipelineByPropertyName", "", XmlConvert.ToString(o.ValueFromPipelineByPropertyName));
                }
                string[] cmdletParameterSets = o.CmdletParameterSets;
                if (cmdletParameterSets != null)
                {
                    base.Writer.WriteStartAttribute(null, "CmdletParameterSets", "");
                    for (int j = 0; j < cmdletParameterSets.Length; j++)
                    {
                        string str2 = cmdletParameterSets[j];
                        if (j != 0)
                        {
                            base.Writer.WriteString(" ");
                        }
                        base.WriteValue(str2);
                    }
                    base.Writer.WriteEndAttribute();
                }
                this.Write1_Object("AllowEmptyCollection", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowEmptyCollection, false, false);
                this.Write1_Object("AllowEmptyString", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowEmptyString, false, false);
                this.Write1_Object("AllowNull", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowNull, false, false);
                this.Write1_Object("ValidateNotNull", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateNotNull, false, false);
                this.Write1_Object("ValidateNotNullOrEmpty", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateNotNullOrEmpty, false, false);
                this.Write4_Item("ValidateCount", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateCount, false, false);
                this.Write5_Item("ValidateLength", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateLength, false, false);
                this.Write6_Item("ValidateRange", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateRange, false, false);
                string[] validateSet = o.ValidateSet;
                if (validateSet != null)
                {
                    base.WriteStartElement("ValidateSet", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                    for (int k = 0; k < validateSet.Length; k++)
                    {
                        base.WriteElementString("AllowedValue", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", validateSet[k]);
                    }
                    base.WriteEndElement();
                }
                bool isMandatorySpecified = o.IsMandatorySpecified;
                bool valueFromPipelineSpecified = o.ValueFromPipelineSpecified;
                bool valueFromPipelineByPropertyNameSpecified = o.ValueFromPipelineByPropertyNameSpecified;
                base.WriteEndElement(o);
            }
        }

        private void Write11_Item(string n, string ns, CmdletParameterMetadataForGetCmdletFilteringParameter o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(CmdletParameterMetadataForGetCmdletFilteringParameter)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("CmdletParameterMetadataForGetCmdletFilteringParameter", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                if (o.IsMandatorySpecified)
                {
                    base.WriteAttribute("IsMandatory", "", XmlConvert.ToString(o.IsMandatory));
                }
                string[] aliases = o.Aliases;
                if (aliases != null)
                {
                    base.Writer.WriteStartAttribute(null, "Aliases", "");
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        string str = aliases[i];
                        if (i != 0)
                        {
                            base.Writer.WriteString(" ");
                        }
                        base.WriteValue(str);
                    }
                    base.Writer.WriteEndAttribute();
                }
                base.WriteAttribute("PSName", "", o.PSName);
                base.WriteAttribute("Position", "", o.Position);
                if (o.ValueFromPipelineSpecified)
                {
                    base.WriteAttribute("ValueFromPipeline", "", XmlConvert.ToString(o.ValueFromPipeline));
                }
                if (o.ValueFromPipelineByPropertyNameSpecified)
                {
                    base.WriteAttribute("ValueFromPipelineByPropertyName", "", XmlConvert.ToString(o.ValueFromPipelineByPropertyName));
                }
                string[] cmdletParameterSets = o.CmdletParameterSets;
                if (cmdletParameterSets != null)
                {
                    base.Writer.WriteStartAttribute(null, "CmdletParameterSets", "");
                    for (int j = 0; j < cmdletParameterSets.Length; j++)
                    {
                        string str2 = cmdletParameterSets[j];
                        if (j != 0)
                        {
                            base.Writer.WriteString(" ");
                        }
                        base.WriteValue(str2);
                    }
                    base.Writer.WriteEndAttribute();
                }
                if (o.ErrorOnNoMatchSpecified)
                {
                    base.WriteAttribute("ErrorOnNoMatch", "", XmlConvert.ToString(o.ErrorOnNoMatch));
                }
                this.Write1_Object("AllowEmptyCollection", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowEmptyCollection, false, false);
                this.Write1_Object("AllowEmptyString", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowEmptyString, false, false);
                this.Write1_Object("AllowNull", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowNull, false, false);
                this.Write1_Object("ValidateNotNull", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateNotNull, false, false);
                this.Write1_Object("ValidateNotNullOrEmpty", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateNotNullOrEmpty, false, false);
                this.Write4_Item("ValidateCount", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateCount, false, false);
                this.Write5_Item("ValidateLength", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateLength, false, false);
                this.Write6_Item("ValidateRange", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateRange, false, false);
                string[] validateSet = o.ValidateSet;
                if (validateSet != null)
                {
                    base.WriteStartElement("ValidateSet", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                    for (int k = 0; k < validateSet.Length; k++)
                    {
                        base.WriteElementString("AllowedValue", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", validateSet[k]);
                    }
                    base.WriteEndElement();
                }
                bool isMandatorySpecified = o.IsMandatorySpecified;
                bool valueFromPipelineSpecified = o.ValueFromPipelineSpecified;
                bool valueFromPipelineByPropertyNameSpecified = o.ValueFromPipelineByPropertyNameSpecified;
                bool errorOnNoMatchSpecified = o.ErrorOnNoMatchSpecified;
                base.WriteEndElement(o);
            }
        }

        private void Write12_WildcardablePropertyQuery(string n, string ns, WildcardablePropertyQuery o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(WildcardablePropertyQuery)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("WildcardablePropertyQuery", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                if (o.AllowGlobbingSpecified)
                {
                    base.WriteAttribute("AllowGlobbing", "", XmlConvert.ToString(o.AllowGlobbing));
                }
                this.Write11_Item("CmdletParameterMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletParameterMetadata, false, false);
                bool allowGlobbingSpecified = o.AllowGlobbingSpecified;
                base.WriteEndElement(o);
            }
        }

        private void Write13_PropertyQuery(string n, string ns, PropertyQuery o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType)
                {
                    Type type = o.GetType();
                    if (type != typeof(PropertyQuery))
                    {
                        if (type != typeof(WildcardablePropertyQuery))
                        {
                            throw base.CreateUnknownTypeException(o);
                        }
                        this.Write12_WildcardablePropertyQuery(n, ns, (WildcardablePropertyQuery) o, isNullable, true);
                        return;
                    }
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("PropertyQuery", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                this.Write11_Item("CmdletParameterMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletParameterMetadata, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write14_PropertyMetadata(string n, string ns, PropertyMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(PropertyMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("PropertyMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("PropertyName", "", o.PropertyName);
                this.Write2_TypeMetadata("Type", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.Type, false, false);
                PropertyQuery[] items = o.Items;
                if (items != null)
                {
                    ItemsChoiceType[] itemsElementName = o.ItemsElementName;
                    if ((itemsElementName == null) || (itemsElementName.Length < items.Length))
                    {
                        throw base.CreateInvalidChoiceIdentifierValueException("Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType", "ItemsElementName");
                    }
                    for (int i = 0; i < items.Length; i++)
                    {
                        PropertyQuery query = items[i];
                        ItemsChoiceType type2 = itemsElementName[i];
                        if ((type2 == ItemsChoiceType.ExcludeQuery) && (query != null))
                        {
                            if ((query != null) && !(query is WildcardablePropertyQuery))
                            {
                                throw base.CreateMismatchChoiceException("Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery", "ItemsElementName", "Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@ExcludeQuery");
                            }
                            this.Write12_WildcardablePropertyQuery("ExcludeQuery", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", (WildcardablePropertyQuery) query, false, false);
                        }
                        else if ((type2 == ItemsChoiceType.RegularQuery) && (query != null))
                        {
                            if ((query != null) && !(query is WildcardablePropertyQuery))
                            {
                                throw base.CreateMismatchChoiceException("Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery", "ItemsElementName", "Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@RegularQuery");
                            }
                            this.Write12_WildcardablePropertyQuery("RegularQuery", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", (WildcardablePropertyQuery) query, false, false);
                        }
                        else if ((type2 == ItemsChoiceType.MinValueQuery) && (query != null))
                        {
                            if ((query != null) && (query == null))
                            {
                                throw base.CreateMismatchChoiceException("Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery", "ItemsElementName", "Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@MinValueQuery");
                            }
                            this.Write13_PropertyQuery("MinValueQuery", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", query, false, false);
                        }
                        else if ((type2 == ItemsChoiceType.MaxValueQuery) && (query != null))
                        {
                            if ((query != null) && (query == null))
                            {
                                throw base.CreateMismatchChoiceException("Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery", "ItemsElementName", "Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType.@MaxValueQuery");
                            }
                            this.Write13_PropertyQuery("MaxValueQuery", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", query, false, false);
                        }
                        else if (query != null)
                        {
                            throw base.CreateUnknownTypeException(query);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write15_AssociationAssociatedInstance(string n, string ns, AssociationAssociatedInstance o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(AssociationAssociatedInstance)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                this.Write2_TypeMetadata("Type", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.Type, false, false);
                this.Write11_Item("CmdletParameterMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletParameterMetadata, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write16_Association(string n, string ns, Association o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(Association)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("Association", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Association", "", o.Association1);
                base.WriteAttribute("SourceRole", "", o.SourceRole);
                base.WriteAttribute("ResultRole", "", o.ResultRole);
                this.Write15_AssociationAssociatedInstance("AssociatedInstance", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AssociatedInstance, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write17_QueryOption(string n, string ns, QueryOption o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(QueryOption)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("QueryOption", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("OptionName", "", o.OptionName);
                this.Write2_TypeMetadata("Type", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.Type, false, false);
                this.Write10_Item("CmdletParameterMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletParameterMetadata, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write18_GetCmdletParameters(string n, string ns, GetCmdletParameters o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(GetCmdletParameters)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("GetCmdletParameters", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("DefaultCmdletParameterSet", "", o.DefaultCmdletParameterSet);
                PropertyMetadata[] queryableProperties = o.QueryableProperties;
                if (queryableProperties != null)
                {
                    base.WriteStartElement("QueryableProperties", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                    for (int i = 0; i < queryableProperties.Length; i++)
                    {
                        this.Write14_PropertyMetadata("Property", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", queryableProperties[i], false, false);
                    }
                    base.WriteEndElement();
                }
                Association[] queryableAssociations = o.QueryableAssociations;
                if (queryableAssociations != null)
                {
                    base.WriteStartElement("QueryableAssociations", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                    for (int j = 0; j < queryableAssociations.Length; j++)
                    {
                        this.Write16_Association("Association", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", queryableAssociations[j], false, false);
                    }
                    base.WriteEndElement();
                }
                QueryOption[] queryOptions = o.QueryOptions;
                if (queryOptions != null)
                {
                    base.WriteStartElement("QueryOptions", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                    for (int k = 0; k < queryOptions.Length; k++)
                    {
                        this.Write17_QueryOption("Option", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", queryOptions[k], false, false);
                    }
                    base.WriteEndElement();
                }
                base.WriteEndElement(o);
            }
        }

        private string Write19_ConfirmImpact(ConfirmImpact v)
        {
            switch (v)
            {
                case ConfirmImpact.None:
                    return "None";

                case ConfirmImpact.Low:
                    return "Low";

                case ConfirmImpact.Medium:
                    return "Medium";

                case ConfirmImpact.High:
                    return "High";
            }
            long num = (long) v;
            throw base.CreateInvalidEnumValueException(num.ToString(CultureInfo.InvariantCulture), "Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact");
        }

        private void Write2_TypeMetadata(string n, string ns, TypeMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(TypeMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("TypeMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("PSType", "", o.PSType);
                base.WriteAttribute("ETSType", "", o.ETSType);
                base.WriteEndElement(o);
            }
        }

        private void Write20_CommonCmdletMetadata(string n, string ns, CommonCmdletMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType)
                {
                    Type type = o.GetType();
                    if (type != typeof(CommonCmdletMetadata))
                    {
                        if (type != typeof(StaticCmdletMetadataCmdletMetadata))
                        {
                            throw base.CreateUnknownTypeException(o);
                        }
                        this.Write44_Item(n, ns, (StaticCmdletMetadataCmdletMetadata) o, isNullable, true);
                        return;
                    }
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("CommonCmdletMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Verb", "", o.Verb);
                base.WriteAttribute("Noun", "", o.Noun);
                string[] aliases = o.Aliases;
                if (aliases != null)
                {
                    base.Writer.WriteStartAttribute(null, "Aliases", "");
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        string str = aliases[i];
                        if (i != 0)
                        {
                            base.Writer.WriteString(" ");
                        }
                        base.WriteValue(str);
                    }
                    base.Writer.WriteEndAttribute();
                }
                if (o.ConfirmImpactSpecified)
                {
                    base.WriteAttribute("ConfirmImpact", "", this.Write19_ConfirmImpact(o.ConfirmImpact));
                }
                base.WriteAttribute("HelpUri", "", o.HelpUri);
                bool confirmImpactSpecified = o.ConfirmImpactSpecified;
                base.WriteEndElement(o);
            }
        }

        private void Write21_GetCmdletMetadata(string n, string ns, GetCmdletMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(GetCmdletMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("GetCmdletMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                this.Write20_CommonCmdletMetadata("CmdletMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletMetadata, false, false);
                this.Write18_GetCmdletParameters("GetCmdletParameters", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.GetCmdletParameters, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write22_CmdletOutputMetadata(string n, string ns, CmdletOutputMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(CmdletOutputMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("CmdletOutputMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("PSName", "", o.PSName);
                this.Write1_Object("ErrorCode", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ErrorCode, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write23_Item(string n, string ns, CommonMethodMetadataReturnValue o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(CommonMethodMetadataReturnValue)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                this.Write2_TypeMetadata("Type", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.Type, false, false);
                this.Write22_CmdletOutputMetadata("CmdletOutputMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletOutputMetadata, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write24_Item(string n, string ns, InstanceMethodParameterMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(InstanceMethodParameterMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("InstanceMethodParameterMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("ParameterName", "", o.ParameterName);
                base.WriteAttribute("DefaultValue", "", o.DefaultValue);
                this.Write2_TypeMetadata("Type", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.Type, false, false);
                this.Write7_Item("CmdletParameterMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletParameterMetadata, false, false);
                this.Write22_CmdletOutputMetadata("CmdletOutputMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletOutputMetadata, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write25_CommonMethodParameterMetadata(string n, string ns, CommonMethodParameterMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType)
                {
                    Type type = o.GetType();
                    if (type != typeof(CommonMethodParameterMetadata))
                    {
                        if (type == typeof(StaticMethodParameterMetadata))
                        {
                            this.Write26_StaticMethodParameterMetadata(n, ns, (StaticMethodParameterMetadata) o, isNullable, true);
                            return;
                        }
                        if (type != typeof(InstanceMethodParameterMetadata))
                        {
                            throw base.CreateUnknownTypeException(o);
                        }
                        this.Write24_Item(n, ns, (InstanceMethodParameterMetadata) o, isNullable, true);
                        return;
                    }
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("CommonMethodParameterMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("ParameterName", "", o.ParameterName);
                base.WriteAttribute("DefaultValue", "", o.DefaultValue);
                this.Write2_TypeMetadata("Type", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.Type, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write26_StaticMethodParameterMetadata(string n, string ns, StaticMethodParameterMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(StaticMethodParameterMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("StaticMethodParameterMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("ParameterName", "", o.ParameterName);
                base.WriteAttribute("DefaultValue", "", o.DefaultValue);
                this.Write2_TypeMetadata("Type", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.Type, false, false);
                this.Write8_Item("CmdletParameterMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletParameterMetadata, false, false);
                this.Write22_CmdletOutputMetadata("CmdletOutputMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletOutputMetadata, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write27_StaticMethodMetadata(string n, string ns, StaticMethodMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(StaticMethodMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("StaticMethodMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("MethodName", "", o.MethodName);
                base.WriteAttribute("CmdletParameterSet", "", o.CmdletParameterSet);
                this.Write23_Item("ReturnValue", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ReturnValue, false, false);
                StaticMethodParameterMetadata[] parameters = o.Parameters;
                if (parameters != null)
                {
                    base.WriteStartElement("Parameters", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        this.Write26_StaticMethodParameterMetadata("Parameter", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", parameters[i], false, false);
                    }
                    base.WriteEndElement();
                }
                base.WriteEndElement(o);
            }
        }

        private void Write28_CommonMethodMetadata(string n, string ns, CommonMethodMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType)
                {
                    Type type = o.GetType();
                    if (type != typeof(CommonMethodMetadata))
                    {
                        if (type == typeof(InstanceMethodMetadata))
                        {
                            this.Write29_InstanceMethodMetadata(n, ns, (InstanceMethodMetadata) o, isNullable, true);
                            return;
                        }
                        if (type != typeof(StaticMethodMetadata))
                        {
                            throw base.CreateUnknownTypeException(o);
                        }
                        this.Write27_StaticMethodMetadata(n, ns, (StaticMethodMetadata) o, isNullable, true);
                        return;
                    }
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("CommonMethodMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("MethodName", "", o.MethodName);
                this.Write23_Item("ReturnValue", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ReturnValue, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write29_InstanceMethodMetadata(string n, string ns, InstanceMethodMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(InstanceMethodMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("InstanceMethodMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("MethodName", "", o.MethodName);
                this.Write23_Item("ReturnValue", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ReturnValue, false, false);
                InstanceMethodParameterMetadata[] parameters = o.Parameters;
                if (parameters != null)
                {
                    base.WriteStartElement("Parameters", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        this.Write24_Item("Parameter", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", parameters[i], false, false);
                    }
                    base.WriteEndElement();
                }
                base.WriteEndElement(o);
            }
        }

        private string Write3_ItemsChoiceType(ItemsChoiceType v)
        {
            switch (v)
            {
                case ItemsChoiceType.ExcludeQuery:
                    return "ExcludeQuery";

                case ItemsChoiceType.MaxValueQuery:
                    return "MaxValueQuery";

                case ItemsChoiceType.MinValueQuery:
                    return "MinValueQuery";

                case ItemsChoiceType.RegularQuery:
                    return "RegularQuery";
            }
            long num = (long) v;
            throw base.CreateInvalidEnumValueException(num.ToString(CultureInfo.InvariantCulture), "Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType");
        }

        private void Write30_InstanceCmdletMetadata(string n, string ns, InstanceCmdletMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(InstanceCmdletMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("InstanceCmdletMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                this.Write20_CommonCmdletMetadata("CmdletMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletMetadata, false, false);
                this.Write29_InstanceMethodMetadata("Method", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.Method, false, false);
                this.Write18_GetCmdletParameters("GetCmdletParameters", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.GetCmdletParameters, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write31_ClassMetadataInstanceCmdlets(string n, string ns, ClassMetadataInstanceCmdlets o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(ClassMetadataInstanceCmdlets)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                this.Write18_GetCmdletParameters("GetCmdletParameters", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.GetCmdletParameters, false, false);
                this.Write21_GetCmdletMetadata("GetCmdlet", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.GetCmdlet, false, false);
                InstanceCmdletMetadata[] cmdlet = o.Cmdlet;
                if (cmdlet != null)
                {
                    for (int i = 0; i < cmdlet.Length; i++)
                    {
                        this.Write30_InstanceCmdletMetadata("Cmdlet", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", cmdlet[i], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write32_Item(string n, string ns, StaticCmdletMetadataCmdletMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(StaticCmdletMetadataCmdletMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Verb", "", o.Verb);
                base.WriteAttribute("Noun", "", o.Noun);
                string[] aliases = o.Aliases;
                if (aliases != null)
                {
                    base.Writer.WriteStartAttribute(null, "Aliases", "");
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        string str = aliases[i];
                        if (i != 0)
                        {
                            base.Writer.WriteString(" ");
                        }
                        base.WriteValue(str);
                    }
                    base.Writer.WriteEndAttribute();
                }
                if (o.ConfirmImpactSpecified)
                {
                    base.WriteAttribute("ConfirmImpact", "", this.Write19_ConfirmImpact(o.ConfirmImpact));
                }
                base.WriteAttribute("HelpUri", "", o.HelpUri);
                base.WriteAttribute("DefaultCmdletParameterSet", "", o.DefaultCmdletParameterSet);
                bool confirmImpactSpecified = o.ConfirmImpactSpecified;
                base.WriteEndElement(o);
            }
        }

        private void Write33_StaticCmdletMetadata(string n, string ns, StaticCmdletMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(StaticCmdletMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("StaticCmdletMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                this.Write32_Item("CmdletMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletMetadata, false, false);
                StaticMethodMetadata[] method = o.Method;
                if (method != null)
                {
                    for (int i = 0; i < method.Length; i++)
                    {
                        this.Write27_StaticMethodMetadata("Method", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", method[i], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write34_ClassMetadataData(string n, string ns, ClassMetadataData o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(ClassMetadataData)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Name", "", o.Name);
                if (o.Value != null)
                {
                    base.WriteValue(o.Value);
                }
                base.WriteEndElement(o);
            }
        }

        private void Write35_ClassMetadata(string n, string ns, ClassMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(ClassMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("ClassMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("CmdletAdapter", "", o.CmdletAdapter);
                base.WriteAttribute("ClassName", "", o.ClassName);
                base.WriteAttribute("ClassVersion", "", o.ClassVersion);
                base.WriteElementString("Version", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.Version);
                base.WriteElementString("DefaultNoun", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.DefaultNoun);
                this.Write31_ClassMetadataInstanceCmdlets("InstanceCmdlets", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.InstanceCmdlets, false, false);
                StaticCmdletMetadata[] staticCmdlets = o.StaticCmdlets;
                if (staticCmdlets != null)
                {
                    base.WriteStartElement("StaticCmdlets", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                    for (int i = 0; i < staticCmdlets.Length; i++)
                    {
                        this.Write33_StaticCmdletMetadata("Cmdlet", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", staticCmdlets[i], false, false);
                    }
                    base.WriteEndElement();
                }
                ClassMetadataData[] cmdletAdapterPrivateData = o.CmdletAdapterPrivateData;
                if (cmdletAdapterPrivateData != null)
                {
                    base.WriteStartElement("CmdletAdapterPrivateData", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                    for (int j = 0; j < cmdletAdapterPrivateData.Length; j++)
                    {
                        this.Write34_ClassMetadataData("Data", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", cmdletAdapterPrivateData[j], false, false);
                    }
                    base.WriteEndElement();
                }
                base.WriteEndElement(o);
            }
        }

        private void Write36_EnumMetadataEnumValue(string n, string ns, EnumMetadataEnumValue o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(EnumMetadataEnumValue)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Name", "", o.Name);
                base.WriteAttribute("Value", "", o.Value);
                base.WriteEndElement(o);
            }
        }

        private void Write37_EnumMetadataEnum(string n, string ns, EnumMetadataEnum o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(EnumMetadataEnum)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("EnumName", "", o.EnumName);
                base.WriteAttribute("UnderlyingType", "", o.UnderlyingType);
                if (o.BitwiseFlagsSpecified)
                {
                    base.WriteAttribute("BitwiseFlags", "", XmlConvert.ToString(o.BitwiseFlags));
                }
                EnumMetadataEnumValue[] valueArray = o.Value;
                if (valueArray != null)
                {
                    for (int i = 0; i < valueArray.Length; i++)
                    {
                        this.Write36_EnumMetadataEnumValue("Value", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", valueArray[i], false, false);
                    }
                }
                bool bitwiseFlagsSpecified = o.BitwiseFlagsSpecified;
                base.WriteEndElement(o);
            }
        }

        private void Write38_PowerShellMetadata(string n, string ns, PowerShellMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(PowerShellMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                this.Write35_ClassMetadata("Class", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.Class, false, false);
                EnumMetadataEnum[] enums = o.Enums;
                if (enums != null)
                {
                    base.WriteStartElement("Enums", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                    for (int i = 0; i < enums.Length; i++)
                    {
                        this.Write37_EnumMetadataEnum("Enum", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", enums[i], false, false);
                    }
                    base.WriteEndElement();
                }
                base.WriteEndElement(o);
            }
        }

        private void Write39_ClassMetadataInstanceCmdlets(string n, string ns, ClassMetadataInstanceCmdlets o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(ClassMetadataInstanceCmdlets)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("ClassMetadataInstanceCmdlets", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                this.Write18_GetCmdletParameters("GetCmdletParameters", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.GetCmdletParameters, false, false);
                this.Write21_GetCmdletMetadata("GetCmdlet", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.GetCmdlet, false, false);
                InstanceCmdletMetadata[] cmdlet = o.Cmdlet;
                if (cmdlet != null)
                {
                    for (int i = 0; i < cmdlet.Length; i++)
                    {
                        this.Write30_InstanceCmdletMetadata("Cmdlet", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", cmdlet[i], false, false);
                    }
                }
                base.WriteEndElement(o);
            }
        }

        private void Write4_Item(string n, string ns, CmdletParameterMetadataValidateCount o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(CmdletParameterMetadataValidateCount)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Min", "", o.Min);
                base.WriteAttribute("Max", "", o.Max);
                base.WriteEndElement(o);
            }
        }

        private void Write40_AssociationAssociatedInstance(string n, string ns, AssociationAssociatedInstance o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(AssociationAssociatedInstance)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("AssociationAssociatedInstance", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                this.Write2_TypeMetadata("Type", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.Type, false, false);
                this.Write11_Item("CmdletParameterMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletParameterMetadata, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write41_Item(string n, string ns, CmdletParameterMetadataValidateCount o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(CmdletParameterMetadataValidateCount)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("CmdletParameterMetadataValidateCount", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Min", "", o.Min);
                base.WriteAttribute("Max", "", o.Max);
                base.WriteEndElement(o);
            }
        }

        private void Write42_Item(string n, string ns, CmdletParameterMetadataValidateLength o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(CmdletParameterMetadataValidateLength)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("CmdletParameterMetadataValidateLength", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Min", "", o.Min);
                base.WriteAttribute("Max", "", o.Max);
                base.WriteEndElement(o);
            }
        }

        private void Write43_Item(string n, string ns, CmdletParameterMetadataValidateRange o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(CmdletParameterMetadataValidateRange)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("CmdletParameterMetadataValidateRange", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Min", "", o.Min);
                base.WriteAttribute("Max", "", o.Max);
                base.WriteEndElement(o);
            }
        }

        private void Write44_Item(string n, string ns, StaticCmdletMetadataCmdletMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(StaticCmdletMetadataCmdletMetadata)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("StaticCmdletMetadataCmdletMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Verb", "", o.Verb);
                base.WriteAttribute("Noun", "", o.Noun);
                string[] aliases = o.Aliases;
                if (aliases != null)
                {
                    base.Writer.WriteStartAttribute(null, "Aliases", "");
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        string str = aliases[i];
                        if (i != 0)
                        {
                            base.Writer.WriteString(" ");
                        }
                        base.WriteValue(str);
                    }
                    base.Writer.WriteEndAttribute();
                }
                if (o.ConfirmImpactSpecified)
                {
                    base.WriteAttribute("ConfirmImpact", "", this.Write19_ConfirmImpact(o.ConfirmImpact));
                }
                base.WriteAttribute("HelpUri", "", o.HelpUri);
                base.WriteAttribute("DefaultCmdletParameterSet", "", o.DefaultCmdletParameterSet);
                bool confirmImpactSpecified = o.ConfirmImpactSpecified;
                base.WriteEndElement(o);
            }
        }

        private void Write45_Item(string n, string ns, CommonMethodMetadataReturnValue o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(CommonMethodMetadataReturnValue)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("CommonMethodMetadataReturnValue", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                this.Write2_TypeMetadata("Type", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.Type, false, false);
                this.Write22_CmdletOutputMetadata("CmdletOutputMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.CmdletOutputMetadata, false, false);
                base.WriteEndElement(o);
            }
        }

        private void Write46_ClassMetadataData(string n, string ns, ClassMetadataData o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(ClassMetadataData)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("ClassMetadataData", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Name", "", o.Name);
                if (o.Value != null)
                {
                    base.WriteValue(o.Value);
                }
                base.WriteEndElement(o);
            }
        }

        private void Write47_EnumMetadataEnum(string n, string ns, EnumMetadataEnum o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(EnumMetadataEnum)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("EnumMetadataEnum", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("EnumName", "", o.EnumName);
                base.WriteAttribute("UnderlyingType", "", o.UnderlyingType);
                if (o.BitwiseFlagsSpecified)
                {
                    base.WriteAttribute("BitwiseFlags", "", XmlConvert.ToString(o.BitwiseFlags));
                }
                EnumMetadataEnumValue[] valueArray = o.Value;
                if (valueArray != null)
                {
                    for (int i = 0; i < valueArray.Length; i++)
                    {
                        this.Write36_EnumMetadataEnumValue("Value", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", valueArray[i], false, false);
                    }
                }
                bool bitwiseFlagsSpecified = o.BitwiseFlagsSpecified;
                base.WriteEndElement(o);
            }
        }

        private void Write48_EnumMetadataEnumValue(string n, string ns, EnumMetadataEnumValue o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(EnumMetadataEnumValue)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("EnumMetadataEnumValue", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Name", "", o.Name);
                base.WriteAttribute("Value", "", o.Value);
                base.WriteEndElement(o);
            }
        }

        public void Write49_PowerShellMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("PowerShellMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
            }
            else
            {
                base.TopLevelElement();
                this.Write38_PowerShellMetadata("PowerShellMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", (PowerShellMetadata) o, false, false);
            }
        }

        private void Write5_Item(string n, string ns, CmdletParameterMetadataValidateLength o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(CmdletParameterMetadataValidateLength)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Min", "", o.Min);
                base.WriteAttribute("Max", "", o.Max);
                base.WriteEndElement(o);
            }
        }

        public void Write50_ClassMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("ClassMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write35_ClassMetadata("ClassMetadata", "", (ClassMetadata) o, true, false);
            }
        }

        public void Write51_ClassMetadataInstanceCmdlets(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("ClassMetadataInstanceCmdlets", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write39_ClassMetadataInstanceCmdlets("ClassMetadataInstanceCmdlets", "", (ClassMetadataInstanceCmdlets) o, true, false);
            }
        }

        public void Write52_GetCmdletParameters(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("GetCmdletParameters", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write18_GetCmdletParameters("GetCmdletParameters", "", (GetCmdletParameters) o, true, false);
            }
        }

        public void Write53_PropertyMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("PropertyMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write14_PropertyMetadata("PropertyMetadata", "", (PropertyMetadata) o, true, false);
            }
        }

        public void Write54_TypeMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("TypeMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write2_TypeMetadata("TypeMetadata", "", (TypeMetadata) o, true, false);
            }
        }

        public void Write55_Association(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("Association", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write16_Association("Association", "", (Association) o, true, false);
            }
        }

        public void Write56_AssociationAssociatedInstance(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("AssociationAssociatedInstance", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write40_AssociationAssociatedInstance("AssociationAssociatedInstance", "", (AssociationAssociatedInstance) o, true, false);
            }
        }

        public void Write57_CmdletParameterMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("CmdletParameterMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write9_CmdletParameterMetadata("CmdletParameterMetadata", "", (CmdletParameterMetadata) o, true, false);
            }
        }

        public void Write58_Item(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("CmdletParameterMetadataForGetCmdletParameter", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write10_Item("CmdletParameterMetadataForGetCmdletParameter", "", (CmdletParameterMetadataForGetCmdletParameter) o, true, false);
            }
        }

        public void Write59_Item(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("CmdletParameterMetadataForGetCmdletFilteringParameter", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write11_Item("CmdletParameterMetadataForGetCmdletFilteringParameter", "", (CmdletParameterMetadataForGetCmdletFilteringParameter) o, true, false);
            }
        }

        private void Write6_Item(string n, string ns, CmdletParameterMetadataValidateRange o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(CmdletParameterMetadataValidateRange)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType(null, "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                base.WriteAttribute("Min", "", o.Min);
                base.WriteAttribute("Max", "", o.Max);
                base.WriteEndElement(o);
            }
        }

        public void Write60_Item(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("CmdletParameterMetadataValidateCount", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write41_Item("CmdletParameterMetadataValidateCount", "", (CmdletParameterMetadataValidateCount) o, true, false);
            }
        }

        public void Write61_Item(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("CmdletParameterMetadataValidateLength", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write42_Item("CmdletParameterMetadataValidateLength", "", (CmdletParameterMetadataValidateLength) o, true, false);
            }
        }

        public void Write62_Item(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("CmdletParameterMetadataValidateRange", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write43_Item("CmdletParameterMetadataValidateRange", "", (CmdletParameterMetadataValidateRange) o, true, false);
            }
        }

        public void Write63_Item(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("CmdletParameterMetadataForInstanceMethodParameter", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write7_Item("CmdletParameterMetadataForInstanceMethodParameter", "", (CmdletParameterMetadataForInstanceMethodParameter) o, true, false);
            }
        }

        public void Write64_Item(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("CmdletParameterMetadataForStaticMethodParameter", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write8_Item("CmdletParameterMetadataForStaticMethodParameter", "", (CmdletParameterMetadataForStaticMethodParameter) o, true, false);
            }
        }

        public void Write65_QueryOption(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("QueryOption", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write17_QueryOption("QueryOption", "", (QueryOption) o, true, false);
            }
        }

        public void Write66_GetCmdletMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("GetCmdletMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write21_GetCmdletMetadata("GetCmdletMetadata", "", (GetCmdletMetadata) o, true, false);
            }
        }

        public void Write67_CommonCmdletMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("CommonCmdletMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write20_CommonCmdletMetadata("CommonCmdletMetadata", "", (CommonCmdletMetadata) o, true, false);
            }
        }

        public void Write68_ConfirmImpact(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("ConfirmImpact", "");
            }
            else
            {
                base.WriteElementString("ConfirmImpact", "", this.Write19_ConfirmImpact((ConfirmImpact) o));
            }
        }

        public void Write69_StaticCmdletMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("StaticCmdletMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write33_StaticCmdletMetadata("StaticCmdletMetadata", "", (StaticCmdletMetadata) o, true, false);
            }
        }

        private void Write7_Item(string n, string ns, CmdletParameterMetadataForInstanceMethodParameter o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(CmdletParameterMetadataForInstanceMethodParameter)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("CmdletParameterMetadataForInstanceMethodParameter", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                if (o.IsMandatorySpecified)
                {
                    base.WriteAttribute("IsMandatory", "", XmlConvert.ToString(o.IsMandatory));
                }
                string[] aliases = o.Aliases;
                if (aliases != null)
                {
                    base.Writer.WriteStartAttribute(null, "Aliases", "");
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        string str = aliases[i];
                        if (i != 0)
                        {
                            base.Writer.WriteString(" ");
                        }
                        base.WriteValue(str);
                    }
                    base.Writer.WriteEndAttribute();
                }
                base.WriteAttribute("PSName", "", o.PSName);
                base.WriteAttribute("Position", "", o.Position);
                if (o.ValueFromPipelineByPropertyNameSpecified)
                {
                    base.WriteAttribute("ValueFromPipelineByPropertyName", "", XmlConvert.ToString(o.ValueFromPipelineByPropertyName));
                }
                this.Write1_Object("AllowEmptyCollection", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowEmptyCollection, false, false);
                this.Write1_Object("AllowEmptyString", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowEmptyString, false, false);
                this.Write1_Object("AllowNull", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowNull, false, false);
                this.Write1_Object("ValidateNotNull", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateNotNull, false, false);
                this.Write1_Object("ValidateNotNullOrEmpty", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateNotNullOrEmpty, false, false);
                this.Write4_Item("ValidateCount", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateCount, false, false);
                this.Write5_Item("ValidateLength", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateLength, false, false);
                this.Write6_Item("ValidateRange", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateRange, false, false);
                string[] validateSet = o.ValidateSet;
                if (validateSet != null)
                {
                    base.WriteStartElement("ValidateSet", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                    for (int j = 0; j < validateSet.Length; j++)
                    {
                        base.WriteElementString("AllowedValue", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", validateSet[j]);
                    }
                    base.WriteEndElement();
                }
                bool isMandatorySpecified = o.IsMandatorySpecified;
                bool valueFromPipelineByPropertyNameSpecified = o.ValueFromPipelineByPropertyNameSpecified;
                base.WriteEndElement(o);
            }
        }

        public void Write70_Item(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("StaticCmdletMetadataCmdletMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write44_Item("StaticCmdletMetadataCmdletMetadata", "", (StaticCmdletMetadataCmdletMetadata) o, true, false);
            }
        }

        public void Write71_CommonMethodMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("CommonMethodMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write28_CommonMethodMetadata("CommonMethodMetadata", "", (CommonMethodMetadata) o, true, false);
            }
        }

        public void Write72_StaticMethodMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("StaticMethodMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write27_StaticMethodMetadata("StaticMethodMetadata", "", (StaticMethodMetadata) o, true, false);
            }
        }

        public void Write73_CommonMethodParameterMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("CommonMethodParameterMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write25_CommonMethodParameterMetadata("CommonMethodParameterMetadata", "", (CommonMethodParameterMetadata) o, true, false);
            }
        }

        public void Write74_StaticMethodParameterMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("StaticMethodParameterMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write26_StaticMethodParameterMetadata("StaticMethodParameterMetadata", "", (StaticMethodParameterMetadata) o, true, false);
            }
        }

        public void Write75_CmdletOutputMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("CmdletOutputMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write22_CmdletOutputMetadata("CmdletOutputMetadata", "", (CmdletOutputMetadata) o, true, false);
            }
        }

        public void Write76_Item(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("InstanceMethodParameterMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write24_Item("InstanceMethodParameterMetadata", "", (InstanceMethodParameterMetadata) o, true, false);
            }
        }

        public void Write77_Item(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("CommonMethodMetadataReturnValue", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write45_Item("CommonMethodMetadataReturnValue", "", (CommonMethodMetadataReturnValue) o, true, false);
            }
        }

        public void Write78_InstanceMethodMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("InstanceMethodMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write29_InstanceMethodMetadata("InstanceMethodMetadata", "", (InstanceMethodMetadata) o, true, false);
            }
        }

        public void Write79_InstanceCmdletMetadata(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("InstanceCmdletMetadata", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write30_InstanceCmdletMetadata("InstanceCmdletMetadata", "", (InstanceCmdletMetadata) o, true, false);
            }
        }

        private void Write8_Item(string n, string ns, CmdletParameterMetadataForStaticMethodParameter o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(CmdletParameterMetadataForStaticMethodParameter)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("CmdletParameterMetadataForStaticMethodParameter", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                if (o.IsMandatorySpecified)
                {
                    base.WriteAttribute("IsMandatory", "", XmlConvert.ToString(o.IsMandatory));
                }
                string[] aliases = o.Aliases;
                if (aliases != null)
                {
                    base.Writer.WriteStartAttribute(null, "Aliases", "");
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        string str = aliases[i];
                        if (i != 0)
                        {
                            base.Writer.WriteString(" ");
                        }
                        base.WriteValue(str);
                    }
                    base.Writer.WriteEndAttribute();
                }
                base.WriteAttribute("PSName", "", o.PSName);
                base.WriteAttribute("Position", "", o.Position);
                if (o.ValueFromPipelineSpecified)
                {
                    base.WriteAttribute("ValueFromPipeline", "", XmlConvert.ToString(o.ValueFromPipeline));
                }
                if (o.ValueFromPipelineByPropertyNameSpecified)
                {
                    base.WriteAttribute("ValueFromPipelineByPropertyName", "", XmlConvert.ToString(o.ValueFromPipelineByPropertyName));
                }
                this.Write1_Object("AllowEmptyCollection", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowEmptyCollection, false, false);
                this.Write1_Object("AllowEmptyString", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowEmptyString, false, false);
                this.Write1_Object("AllowNull", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowNull, false, false);
                this.Write1_Object("ValidateNotNull", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateNotNull, false, false);
                this.Write1_Object("ValidateNotNullOrEmpty", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateNotNullOrEmpty, false, false);
                this.Write4_Item("ValidateCount", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateCount, false, false);
                this.Write5_Item("ValidateLength", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateLength, false, false);
                this.Write6_Item("ValidateRange", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateRange, false, false);
                string[] validateSet = o.ValidateSet;
                if (validateSet != null)
                {
                    base.WriteStartElement("ValidateSet", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                    for (int j = 0; j < validateSet.Length; j++)
                    {
                        base.WriteElementString("AllowedValue", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", validateSet[j]);
                    }
                    base.WriteEndElement();
                }
                bool isMandatorySpecified = o.IsMandatorySpecified;
                bool valueFromPipelineSpecified = o.ValueFromPipelineSpecified;
                bool valueFromPipelineByPropertyNameSpecified = o.ValueFromPipelineByPropertyNameSpecified;
                base.WriteEndElement(o);
            }
        }

        public void Write80_PropertyQuery(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("PropertyQuery", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write13_PropertyQuery("PropertyQuery", "", (PropertyQuery) o, true, false);
            }
        }

        public void Write81_WildcardablePropertyQuery(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("WildcardablePropertyQuery", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write12_WildcardablePropertyQuery("WildcardablePropertyQuery", "", (WildcardablePropertyQuery) o, true, false);
            }
        }

        public void Write82_ItemsChoiceType(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteEmptyTag("ItemsChoiceType", "");
            }
            else
            {
                base.WriteElementString("ItemsChoiceType", "", this.Write3_ItemsChoiceType((ItemsChoiceType) o));
            }
        }

        public void Write83_ClassMetadataData(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("ClassMetadataData", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write46_ClassMetadataData("ClassMetadataData", "", (ClassMetadataData) o, true, false);
            }
        }

        public void Write84_EnumMetadataEnum(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("EnumMetadataEnum", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write47_EnumMetadataEnum("EnumMetadataEnum", "", (EnumMetadataEnum) o, true, false);
            }
        }

        public void Write85_EnumMetadataEnumValue(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("EnumMetadataEnumValue", "");
            }
            else
            {
                base.TopLevelElement();
                this.Write48_EnumMetadataEnumValue("EnumMetadataEnumValue", "", (EnumMetadataEnumValue) o, true, false);
            }
        }

        private void Write9_CmdletParameterMetadata(string n, string ns, CmdletParameterMetadata o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType)
                {
                    Type type = o.GetType();
                    if (type != typeof(CmdletParameterMetadata))
                    {
                        if (type == typeof(CmdletParameterMetadataForGetCmdletParameter))
                        {
                            this.Write10_Item(n, ns, (CmdletParameterMetadataForGetCmdletParameter) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CmdletParameterMetadataForGetCmdletFilteringParameter))
                        {
                            this.Write11_Item(n, ns, (CmdletParameterMetadataForGetCmdletFilteringParameter) o, isNullable, true);
                            return;
                        }
                        if (type == typeof(CmdletParameterMetadataForStaticMethodParameter))
                        {
                            this.Write8_Item(n, ns, (CmdletParameterMetadataForStaticMethodParameter) o, isNullable, true);
                            return;
                        }
                        if (type != typeof(CmdletParameterMetadataForInstanceMethodParameter))
                        {
                            throw base.CreateUnknownTypeException(o);
                        }
                        this.Write7_Item(n, ns, (CmdletParameterMetadataForInstanceMethodParameter) o, isNullable, true);
                        return;
                    }
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("CmdletParameterMetadata", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11");
                }
                if (o.IsMandatorySpecified)
                {
                    base.WriteAttribute("IsMandatory", "", XmlConvert.ToString(o.IsMandatory));
                }
                string[] aliases = o.Aliases;
                if (aliases != null)
                {
                    base.Writer.WriteStartAttribute(null, "Aliases", "");
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        string str = aliases[i];
                        if (i != 0)
                        {
                            base.Writer.WriteString(" ");
                        }
                        base.WriteValue(str);
                    }
                    base.Writer.WriteEndAttribute();
                }
                base.WriteAttribute("PSName", "", o.PSName);
                base.WriteAttribute("Position", "", o.Position);
                this.Write1_Object("AllowEmptyCollection", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowEmptyCollection, false, false);
                this.Write1_Object("AllowEmptyString", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowEmptyString, false, false);
                this.Write1_Object("AllowNull", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.AllowNull, false, false);
                this.Write1_Object("ValidateNotNull", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateNotNull, false, false);
                this.Write1_Object("ValidateNotNullOrEmpty", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateNotNullOrEmpty, false, false);
                this.Write4_Item("ValidateCount", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateCount, false, false);
                this.Write5_Item("ValidateLength", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateLength, false, false);
                this.Write6_Item("ValidateRange", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", o.ValidateRange, false, false);
                string[] validateSet = o.ValidateSet;
                if (validateSet != null)
                {
                    base.WriteStartElement("ValidateSet", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", null, false);
                    for (int j = 0; j < validateSet.Length; j++)
                    {
                        base.WriteElementString("AllowedValue", "http://schemas.microsoft.com/cmdlets-over-objects/2009/11", validateSet[j]);
                    }
                    base.WriteEndElement();
                }
                bool isMandatorySpecified = o.IsMandatorySpecified;
                base.WriteEndElement(o);
            }
        }
    }
}

