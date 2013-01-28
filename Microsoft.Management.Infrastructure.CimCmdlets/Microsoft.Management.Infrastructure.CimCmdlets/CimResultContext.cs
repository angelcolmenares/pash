using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimResultContext
	{
		private object errorSource;

		internal object ErrorSource
		{
			get
			{
				return this.errorSource;
			}
		}

		internal CimResultContext(object ErrorSource)
		{
			this.errorSource = ErrorSource;
		}
	}
}