using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal abstract class ScanBase : AScanner<ValueType, LexLocation>
	{
		protected abstract int CurrentSc
		{
			get;
			set;
		}

		public virtual int EolState
		{
			get
			{
				return this.CurrentSc;
			}
			set
			{
				this.CurrentSc = value;
			}
		}

		protected ScanBase()
		{
		}
	}
}