using System;

namespace System.DirectoryServices.AccountManagement
{
	internal abstract class ResultSet : IDisposable
	{
		internal abstract object CurrentAsPrincipal
		{
			get;
		}

		protected ResultSet()
		{
		}

		public virtual void Dispose()
		{
		}

		internal abstract bool MoveNext();

		internal abstract void Reset();
	}
}