using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class ParameterDefinitionEntry
	{
		private readonly string parameterSetName;

		private readonly bool mandatory;

		internal bool IsMandatory
		{
			get
			{
				return this.mandatory;
			}
		}

		internal string ParameterSetName
		{
			get
			{
				return this.parameterSetName;
			}
		}

		internal ParameterDefinitionEntry(string parameterSetName, bool mandatory)
		{
			this.mandatory = mandatory;
			this.parameterSetName = parameterSetName;
		}
	}
}