namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Xml.Serialization;

    [GeneratedCode("sgen", "4.0")]
    internal class XmlSerializerContract : XmlSerializerImplementation
    {
        private Hashtable readMethods;
        private Hashtable typedSerializers;
        private Hashtable writeMethods;

        public override bool CanSerialize(Type type)
        {
            return ((type == typeof(PowerShellMetadata)) || ((type == typeof(ClassMetadata)) || ((type == typeof(ClassMetadataInstanceCmdlets)) || ((type == typeof(GetCmdletParameters)) || ((type == typeof(PropertyMetadata)) || ((type == typeof(TypeMetadata)) || ((type == typeof(Association)) || ((type == typeof(AssociationAssociatedInstance)) || ((type == typeof(CmdletParameterMetadata)) || ((type == typeof(CmdletParameterMetadataForGetCmdletParameter)) || ((type == typeof(CmdletParameterMetadataForGetCmdletFilteringParameter)) || ((type == typeof(CmdletParameterMetadataValidateCount)) || ((type == typeof(CmdletParameterMetadataValidateLength)) || ((type == typeof(CmdletParameterMetadataValidateRange)) || ((type == typeof(CmdletParameterMetadataForInstanceMethodParameter)) || ((type == typeof(CmdletParameterMetadataForStaticMethodParameter)) || ((type == typeof(QueryOption)) || ((type == typeof(GetCmdletMetadata)) || ((type == typeof(CommonCmdletMetadata)) || ((type == typeof(ConfirmImpact)) || ((type == typeof(StaticCmdletMetadata)) || ((type == typeof(StaticCmdletMetadataCmdletMetadata)) || ((type == typeof(CommonMethodMetadata)) || ((type == typeof(StaticMethodMetadata)) || ((type == typeof(CommonMethodParameterMetadata)) || ((type == typeof(StaticMethodParameterMetadata)) || ((type == typeof(CmdletOutputMetadata)) || ((type == typeof(InstanceMethodParameterMetadata)) || ((type == typeof(CommonMethodMetadataReturnValue)) || ((type == typeof(InstanceMethodMetadata)) || ((type == typeof(InstanceCmdletMetadata)) || ((type == typeof(PropertyQuery)) || ((type == typeof(WildcardablePropertyQuery)) || ((type == typeof(ItemsChoiceType)) || ((type == typeof(ClassMetadataData)) || ((type == typeof(EnumMetadataEnum)) || (type == typeof(EnumMetadataEnumValue))))))))))))))))))))))))))))))))))))));
        }

        public override XmlSerializer GetSerializer(Type type)
        {
            if (type == typeof(PowerShellMetadata))
            {
                return new PowerShellMetadataSerializer();
            }
            if (type == typeof(ClassMetadata))
            {
                return new ClassMetadataSerializer();
            }
            if (type == typeof(ClassMetadataInstanceCmdlets))
            {
                return new ClassMetadataInstanceCmdletsSerializer();
            }
            if (type == typeof(GetCmdletParameters))
            {
                return new GetCmdletParametersSerializer();
            }
            if (type == typeof(PropertyMetadata))
            {
                return new PropertyMetadataSerializer();
            }
            if (type == typeof(TypeMetadata))
            {
                return new TypeMetadataSerializer();
            }
            if (type == typeof(Association))
            {
                return new AssociationSerializer();
            }
            if (type == typeof(AssociationAssociatedInstance))
            {
                return new AssociationAssociatedInstanceSerializer();
            }
            if (type == typeof(CmdletParameterMetadata))
            {
                return new CmdletParameterMetadataSerializer();
            }
            if (type == typeof(CmdletParameterMetadataForGetCmdletParameter))
            {
                return new CmdletParameterMetadataForGetCmdletParameterSerializer();
            }
            if (type == typeof(CmdletParameterMetadataForGetCmdletFilteringParameter))
            {
                return new CmdletParameterMetadataForGetCmdletFilteringParameterSerializer();
            }
            if (type == typeof(CmdletParameterMetadataValidateCount))
            {
                return new CmdletParameterMetadataValidateCountSerializer();
            }
            if (type == typeof(CmdletParameterMetadataValidateLength))
            {
                return new CmdletParameterMetadataValidateLengthSerializer();
            }
            if (type == typeof(CmdletParameterMetadataValidateRange))
            {
                return new CmdletParameterMetadataValidateRangeSerializer();
            }
            if (type == typeof(CmdletParameterMetadataForInstanceMethodParameter))
            {
                return new CmdletParameterMetadataForInstanceMethodParameterSerializer();
            }
            if (type == typeof(CmdletParameterMetadataForStaticMethodParameter))
            {
                return new CmdletParameterMetadataForStaticMethodParameterSerializer();
            }
            if (type == typeof(QueryOption))
            {
                return new QueryOptionSerializer();
            }
            if (type == typeof(GetCmdletMetadata))
            {
                return new GetCmdletMetadataSerializer();
            }
            if (type == typeof(CommonCmdletMetadata))
            {
                return new CommonCmdletMetadataSerializer();
            }
            if (type == typeof(ConfirmImpact))
            {
                return new ConfirmImpactSerializer();
            }
            if (type == typeof(StaticCmdletMetadata))
            {
                return new StaticCmdletMetadataSerializer();
            }
            if (type == typeof(StaticCmdletMetadataCmdletMetadata))
            {
                return new StaticCmdletMetadataCmdletMetadataSerializer();
            }
            if (type == typeof(CommonMethodMetadata))
            {
                return new CommonMethodMetadataSerializer();
            }
            if (type == typeof(StaticMethodMetadata))
            {
                return new StaticMethodMetadataSerializer();
            }
            if (type == typeof(CommonMethodParameterMetadata))
            {
                return new CommonMethodParameterMetadataSerializer();
            }
            if (type == typeof(StaticMethodParameterMetadata))
            {
                return new StaticMethodParameterMetadataSerializer();
            }
            if (type == typeof(CmdletOutputMetadata))
            {
                return new CmdletOutputMetadataSerializer();
            }
            if (type == typeof(InstanceMethodParameterMetadata))
            {
                return new InstanceMethodParameterMetadataSerializer();
            }
            if (type == typeof(CommonMethodMetadataReturnValue))
            {
                return new CommonMethodMetadataReturnValueSerializer();
            }
            if (type == typeof(InstanceMethodMetadata))
            {
                return new InstanceMethodMetadataSerializer();
            }
            if (type == typeof(InstanceCmdletMetadata))
            {
                return new InstanceCmdletMetadataSerializer();
            }
            if (type == typeof(PropertyQuery))
            {
                return new PropertyQuerySerializer();
            }
            if (type == typeof(WildcardablePropertyQuery))
            {
                return new WildcardablePropertyQuerySerializer();
            }
            if (type == typeof(ItemsChoiceType))
            {
                return new ItemsChoiceTypeSerializer();
            }
            if (type == typeof(ClassMetadataData))
            {
                return new ClassMetadataDataSerializer();
            }
            if (type == typeof(EnumMetadataEnum))
            {
                return new EnumMetadataEnumSerializer();
            }
            if (type == typeof(EnumMetadataEnumValue))
            {
                return new EnumMetadataEnumValueSerializer();
            }
            return null;
        }

        public override XmlSerializationReader Reader
        {
            get
            {
                return new XmlSerializationReader1();
            }
        }

        public override Hashtable ReadMethods
        {
            get
            {
                if (this.readMethods == null)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata:http://schemas.microsoft.com/cmdlets-over-objects/2009/11::False:"] = "Read49_PowerShellMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata::"] = "Read50_ClassMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets::"] = "Read51_ClassMetadataInstanceCmdlets";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters::"] = "Read52_GetCmdletParameters";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata::"] = "Read53_PropertyMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata::"] = "Read54_TypeMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.Association::"] = "Read55_Association";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance::"] = "Read56_AssociationAssociatedInstance";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata::"] = "Read57_CmdletParameterMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter::"] = "Read58_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter::"] = "Read59_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount::"] = "Read60_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength::"] = "Read61_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange::"] = "Read62_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter::"] = "Read63_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter::"] = "Read64_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.QueryOption::"] = "Read65_QueryOption";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata::"] = "Read66_GetCmdletMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata::"] = "Read67_CommonCmdletMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact::"] = "Read68_ConfirmImpact";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata::"] = "Read69_StaticCmdletMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata::"] = "Read70_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata::"] = "Read71_CommonMethodMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata::"] = "Read72_StaticMethodMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata::"] = "Read73_CommonMethodParameterMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata::"] = "Read74_StaticMethodParameterMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata::"] = "Read75_CmdletOutputMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata::"] = "Read76_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue::"] = "Read77_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata::"] = "Read78_InstanceMethodMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata::"] = "Read79_InstanceCmdletMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery::"] = "Read80_PropertyQuery";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery::"] = "Read81_WildcardablePropertyQuery";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType::"] = "Read82_ItemsChoiceType";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData::"] = "Read83_ClassMetadataData";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum::"] = "Read84_EnumMetadataEnum";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue::"] = "Read85_EnumMetadataEnumValue";
                    if (this.readMethods == null)
                    {
                        this.readMethods = hashtable;
                    }
                }
                return this.readMethods;
            }
        }

        public override Hashtable TypedSerializers
        {
            get
            {
                if (this.typedSerializers == null)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance::", new AssociationAssociatedInstanceSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.Association::", new AssociationSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets::", new ClassMetadataInstanceCmdletsSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata:http://schemas.microsoft.com/cmdlets-over-objects/2009/11::False:", new PowerShellMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue::", new EnumMetadataEnumValueSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata::", new StaticCmdletMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType::", new ItemsChoiceTypeSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery::", new PropertyQuerySerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata::", new CommonMethodParameterMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata::", new StaticMethodMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata::", new CmdletParameterMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata::", new InstanceCmdletMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue::", new CommonMethodMetadataReturnValueSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata::", new PropertyMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter::", new CmdletParameterMetadataForGetCmdletParameterSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata::", new CmdletOutputMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum::", new EnumMetadataEnumSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.QueryOption::", new QueryOptionSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata::", new InstanceMethodParameterMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange::", new CmdletParameterMetadataValidateRangeSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData::", new ClassMetadataDataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact::", new ConfirmImpactSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata::", new StaticCmdletMetadataCmdletMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata::", new GetCmdletMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength::", new CmdletParameterMetadataValidateLengthSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata::", new InstanceMethodMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata::", new CommonMethodMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount::", new CmdletParameterMetadataValidateCountSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters::", new GetCmdletParametersSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter::", new CmdletParameterMetadataForInstanceMethodParameterSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata::", new CommonCmdletMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata::", new TypeMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter::", new CmdletParameterMetadataForGetCmdletFilteringParameterSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata::", new StaticMethodParameterMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter::", new CmdletParameterMetadataForStaticMethodParameterSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata::", new ClassMetadataSerializer());
                    hashtable.Add("Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery::", new WildcardablePropertyQuerySerializer());
                    if (this.typedSerializers == null)
                    {
                        this.typedSerializers = hashtable;
                    }
                }
                return this.typedSerializers;
            }
        }

        public override Hashtable WriteMethods
        {
            get
            {
                if (this.writeMethods == null)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.PowerShellMetadata:http://schemas.microsoft.com/cmdlets-over-objects/2009/11::False:"] = "Write49_PowerShellMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.ClassMetadata::"] = "Write50_ClassMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataInstanceCmdlets::"] = "Write51_ClassMetadataInstanceCmdlets";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.GetCmdletParameters::"] = "Write52_GetCmdletParameters";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.PropertyMetadata::"] = "Write53_PropertyMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.TypeMetadata::"] = "Write54_TypeMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.Association::"] = "Write55_Association";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.AssociationAssociatedInstance::"] = "Write56_AssociationAssociatedInstance";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadata::"] = "Write57_CmdletParameterMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletParameter::"] = "Write58_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForGetCmdletFilteringParameter::"] = "Write59_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateCount::"] = "Write60_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateLength::"] = "Write61_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataValidateRange::"] = "Write62_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForInstanceMethodParameter::"] = "Write63_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletParameterMetadataForStaticMethodParameter::"] = "Write64_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.QueryOption::"] = "Write65_QueryOption";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.GetCmdletMetadata::"] = "Write66_GetCmdletMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CommonCmdletMetadata::"] = "Write67_CommonCmdletMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.ConfirmImpact::"] = "Write68_ConfirmImpact";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadata::"] = "Write69_StaticCmdletMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.StaticCmdletMetadataCmdletMetadata::"] = "Write70_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadata::"] = "Write71_CommonMethodMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.StaticMethodMetadata::"] = "Write72_StaticMethodMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CommonMethodParameterMetadata::"] = "Write73_CommonMethodParameterMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.StaticMethodParameterMetadata::"] = "Write74_StaticMethodParameterMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CmdletOutputMetadata::"] = "Write75_CmdletOutputMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodParameterMetadata::"] = "Write76_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.CommonMethodMetadataReturnValue::"] = "Write77_Item";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.InstanceMethodMetadata::"] = "Write78_InstanceMethodMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.InstanceCmdletMetadata::"] = "Write79_InstanceCmdletMetadata";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.PropertyQuery::"] = "Write80_PropertyQuery";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.WildcardablePropertyQuery::"] = "Write81_WildcardablePropertyQuery";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.ItemsChoiceType::"] = "Write82_ItemsChoiceType";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.ClassMetadataData::"] = "Write83_ClassMetadataData";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnum::"] = "Write84_EnumMetadataEnum";
                    hashtable["Microsoft.PowerShell.Cmdletization.Xml.EnumMetadataEnumValue::"] = "Write85_EnumMetadataEnumValue";
                    if (this.writeMethods == null)
                    {
                        this.writeMethods = hashtable;
                    }
                }
                return this.writeMethods;
            }
        }

        public override XmlSerializationWriter Writer
        {
            get
            {
                return new XmlSerializationWriter1();
            }
        }
    }
}

