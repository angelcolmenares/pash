namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    internal class ParameterSetPromptingData
    {
        private bool isDefaultSet;
        private Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> nonpipelineableMandatoryParameters = new Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata>();
        private int parameterSet;
        private Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> pipelineableMandatoryByPropertyNameParameters = new Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata>();
        private Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> pipelineableMandatoryByValueParameters = new Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata>();
        private Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> pipelineableMandatoryParameters = new Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata>();

        internal ParameterSetPromptingData(int parameterSet, bool isDefaultSet)
        {
            this.parameterSet = parameterSet;
            this.isDefaultSet = isDefaultSet;
        }

        internal bool IsAllSet
        {
            get
            {
                return (this.parameterSet == int.MaxValue);
            }
        }

        internal bool IsDefaultSet
        {
            get
            {
                return this.isDefaultSet;
            }
        }

        internal Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> NonpipelineableMandatoryParameters
        {
            get
            {
                return this.nonpipelineableMandatoryParameters;
            }
        }

        internal int ParameterSet
        {
            get
            {
                return this.parameterSet;
            }
        }

        internal Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> PipelineableMandatoryByPropertyNameParameters
        {
            get
            {
                return this.pipelineableMandatoryByPropertyNameParameters;
            }
        }

        internal Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> PipelineableMandatoryByValueParameters
        {
            get
            {
                return this.pipelineableMandatoryByValueParameters;
            }
        }

        internal Dictionary<MergedCompiledCommandParameter, ParameterSetSpecificMetadata> PipelineableMandatoryParameters
        {
            get
            {
                return this.pipelineableMandatoryParameters;
            }
        }
    }
}

