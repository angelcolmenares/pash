using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReplicationFailureCollection : ReadOnlyCollectionBase
	{
		private DirectoryServer server;

		private Hashtable nameTable;

		public ReplicationFailure this[int index]
		{
			get
			{
				return (ReplicationFailure)base.InnerList[index];
			}
		}

		internal ReplicationFailureCollection(DirectoryServer server)
		{
			this.server = server;
			Hashtable hashtables = new Hashtable();
			this.nameTable = Hashtable.Synchronized(hashtables);
		}

		private int Add(ReplicationFailure failure)
		{
			return base.InnerList.Add(failure);
		}

		internal void AddHelper(DS_REPL_KCC_DSA_FAILURES failures, IntPtr info)
		{
			int num = failures.cNumEntries;
			for (int i = 0; i < num; i++)
			{
				IntPtr intPtr = (IntPtr)((long)info + (long)(Marshal.SizeOf(typeof(int)) * 2) + (long)(i * Marshal.SizeOf(typeof(DS_REPL_KCC_DSA_FAILURE))));
				ReplicationFailure replicationFailure = new ReplicationFailure(intPtr, this.server, this.nameTable);
				if (replicationFailure.LastErrorCode == 0)
				{
					replicationFailure.lastResult = ExceptionHelper.ERROR_DS_UNKNOWN_ERROR;
				}
				this.Add(replicationFailure);
			}
		}

		public bool Contains(ReplicationFailure failure)
		{
			if (failure != null)
			{
				return base.InnerList.Contains(failure);
			}
			else
			{
				throw new ArgumentNullException("failure");
			}
		}

		public void CopyTo(ReplicationFailure[] failures, int index)
		{
			base.InnerList.CopyTo(failures, index);
		}

		public int IndexOf(ReplicationFailure failure)
		{
			if (failure != null)
			{
				return base.InnerList.IndexOf(failure);
			}
			else
			{
				throw new ArgumentNullException("failure");
			}
		}
	}
}