namespace Microsoft.PowerShell.Cmdletization
{
    using Microsoft.PowerShell.Cmdletization.Xml;
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Reflection;
    using System.Reflection.Emit;

    internal static class EnumWriter
    {
        private static Lazy<ModuleBuilder> _moduleBuilder = new Lazy<ModuleBuilder>(new Func<ModuleBuilder>(EnumWriter.CreateModuleBuilder), true);
        private static object _moduleBuilderUsageLock = new object();
        private const string namespacePrefix = "Microsoft.PowerShell.Cmdletization.GeneratedTypes";

        internal static void Compile(EnumMetadataEnum enumMetadata)
        {
            Type type;
            EnumBuilder builder2;
            string enumFullName = GetEnumFullName(enumMetadata);
            if (enumMetadata.UnderlyingType != null)
            {
                type = (Type) LanguagePrimitives.ConvertTo(enumMetadata.UnderlyingType, typeof(Type), CultureInfo.InvariantCulture);
            }
            else
            {
                type = typeof(int);
            }
            ModuleBuilder builder = _moduleBuilder.Value;
            lock (_moduleBuilderUsageLock)
            {
                builder2 = builder.DefineEnum(enumFullName, TypeAttributes.Public, type);
            }
            if (enumMetadata.BitwiseFlagsSpecified && enumMetadata.BitwiseFlags)
            {
                CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(typeof(FlagsAttribute).GetConstructor(Type.EmptyTypes), new object[0]);
                builder2.SetCustomAttribute(customBuilder);
            }
            foreach (EnumMetadataEnumValue value2 in enumMetadata.Value)
            {
                string name = value2.Name;
                object literalValue = LanguagePrimitives.ConvertTo(value2.Value, type, CultureInfo.InvariantCulture);
                builder2.DefineLiteral(name, literalValue);
            }
            builder2.CreateType();
        }

        private static ModuleBuilder CreateModuleBuilder()
        {
            AssemblyName name = new AssemblyName("Microsoft.PowerShell.Cmdletization.GeneratedTypes");
            return AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run).DefineDynamicModule(name.Name);
        }

        internal static string GetEnumFullName(EnumMetadataEnum enumMetadata)
        {
            return ("Microsoft.PowerShell.Cmdletization.GeneratedTypes." + enumMetadata.EnumName);
        }
    }
}

