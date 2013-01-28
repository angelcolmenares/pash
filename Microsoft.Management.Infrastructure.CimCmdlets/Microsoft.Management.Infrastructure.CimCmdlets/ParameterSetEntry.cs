using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class ParameterSetEntry
	{
		private readonly bool isDefaultParameterSet;

		private readonly uint mandatoryParameterCount;

		private bool isValueSet;

		private bool isValueSetAtBeginProcess;

		private uint setMandatoryParameterCount;

		private uint setMandatoryParameterCountAtBeginProcess;

		internal bool IsDefaultParameterSet
		{
			get
			{
				return this.isDefaultParameterSet;
			}
		}

		internal bool IsValueSet
		{
			get
			{
				return this.isValueSet;
			}
			set
			{
				this.isValueSet = value;
			}
		}

		internal bool IsValueSetAtBeginProcess
		{
			get
			{
				return this.isValueSetAtBeginProcess;
			}
			set
			{
				this.isValueSetAtBeginProcess = value;
			}
		}

		internal uint MandatoryParameterCount
		{
			get
			{
				return this.mandatoryParameterCount;
			}
		}

		internal uint SetMandatoryParameterCount
		{
			get
			{
				return this.setMandatoryParameterCount;
			}
			set
			{
				this.setMandatoryParameterCount = value;
			}
		}

		internal uint SetMandatoryParameterCountAtBeginProcess
		{
			get
			{
				return this.setMandatoryParameterCountAtBeginProcess;
			}
			set
			{
				this.setMandatoryParameterCountAtBeginProcess = value;
			}
		}

		internal ParameterSetEntry(uint mandatoryParameterCount)
		{
			this.mandatoryParameterCount = mandatoryParameterCount;
			this.isDefaultParameterSet = false;
			this.reset();
		}

		internal ParameterSetEntry(ParameterSetEntry toClone)
		{
			this.mandatoryParameterCount = toClone.MandatoryParameterCount;
			this.isDefaultParameterSet = toClone.IsDefaultParameterSet;
			this.reset();
		}

		internal ParameterSetEntry(uint mandatoryParameterCount, bool isDefault)
		{
			this.mandatoryParameterCount = mandatoryParameterCount;
			this.isDefaultParameterSet = isDefault;
			this.reset();
		}

		internal void reset()
		{
			this.setMandatoryParameterCount = this.setMandatoryParameterCountAtBeginProcess;
			this.isValueSet = this.isValueSetAtBeginProcess;
		}
	}
}