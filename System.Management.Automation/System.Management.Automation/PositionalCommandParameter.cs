namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;

    internal class PositionalCommandParameter
    {
        private MergedCompiledCommandParameter parameter;
        private Collection<ParameterSetSpecificMetadata> parameterSetData = new Collection<ParameterSetSpecificMetadata>();

        internal PositionalCommandParameter(MergedCompiledCommandParameter parameter)
        {
            this.parameter = parameter;
        }

        internal MergedCompiledCommandParameter Parameter
        {
            get
            {
                return this.parameter;
            }
        }

        internal Collection<ParameterSetSpecificMetadata> ParameterSetData
        {
            get
            {
                return this.parameterSetData;
            }
        }
    }
}

