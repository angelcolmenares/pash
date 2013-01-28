namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    internal sealed class ArgumentToTypeNameTransformationAttribute : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            object obj2 = PSObject.Base(inputData);
            if (obj2 is Type)
            {
                return ((Type) obj2).FullName;
            }
            if (obj2 is string)
            {
                Type type;
                string valueToConvert = (string) obj2;
                if (LanguagePrimitives.TryConvertTo<Type>(valueToConvert, out type))
                {
                    valueToConvert = type.FullName;
                }
                return valueToConvert;
            }
            if (obj2 is TypeData)
            {
                return ((TypeData) obj2).TypeName;
            }
            return obj2.GetType().FullName;
        }
    }
}

