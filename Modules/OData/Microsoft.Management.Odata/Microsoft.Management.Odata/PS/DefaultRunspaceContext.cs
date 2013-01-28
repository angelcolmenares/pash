using System;
using System.Management.Automation.Runspaces;

namespace Microsoft.Management.Odata.PS
{
	internal class DefaultRunspaceContext : IDisposable
	{
		private Runspace oldRunspace;

		protected DefaultRunspaceContext(Runspace rs)
		{
			this.oldRunspace = Runspace.DefaultRunspace;
			Runspace.DefaultRunspace = rs;
		}

		public static DefaultRunspaceContext Create(Runspace rs)
		{
			return new DefaultRunspaceContext(rs);
		}

		public void Dispose()
		{
			Runspace.DefaultRunspace = this.oldRunspace;
			GC.SuppressFinalize(this);
		}
	}
}