namespace Microsoft.Management.Infrastructure
{
    using Microsoft.Management.Infrastructure.Internal;
    using Microsoft.Management.Infrastructure.Internal.Data;
    using System;

    public abstract class CimMethodParameter
    {
        internal CimMethodParameter()
        {
        }

        public static CimMethodParameter Create(string name, object value, CimFlags flags)
        {
            Microsoft.Management.Infrastructure.CimType cimTypeFromDotNetValueOrThrowAnException = CimConverter.GetCimTypeFromDotNetValueOrThrowAnException(value);
            return Create(name, value, cimTypeFromDotNetValueOrThrowAnException, flags);
        }

        public static CimMethodParameter Create(string name, object value, Microsoft.Management.Infrastructure.CimType type, CimFlags flags)
        {
            return new CimMethodParameterBackedByCimProperty(new CimPropertyStandalone(name, value, type, flags));
        }

        public override string ToString()
        {
            return Helpers.ToStringFromNameAndValue(this.Name, this.Value);
        }

        public abstract Microsoft.Management.Infrastructure.CimType CimType { get; }

        public abstract CimFlags Flags { get; }

        public abstract string Name { get; }

        public abstract object Value { get; set; }
    }
}

