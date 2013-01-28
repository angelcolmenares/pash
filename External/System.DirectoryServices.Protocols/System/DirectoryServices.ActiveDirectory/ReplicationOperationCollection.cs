using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReplicationOperationCollection : ReadOnlyCollectionBase
	{
		private DirectoryServer server;

		private Hashtable nameTable;

		public ReplicationOperation this[int index]
		{
			get
			{
				return (ReplicationOperation)base.InnerList[index];
			}
		}

		internal ReplicationOperationCollection(DirectoryServer server)
		{
			this.server = server;
			Hashtable hashtables = new Hashtable();
			this.nameTable = Hashtable.Synchronized(hashtables);
		}

		private int Add(ReplicationOperation operation)
		{
			return base.InnerList.Add(operation);
		}

		internal void AddHelper(DS_REPL_PENDING_OPS operations, IntPtr info)
		{
			int num = operations.cNumPendingOps;
			for (int i = 0; i < num; i++)
			{
				IntPtr intPtr = (IntPtr)((long)info + (long)Marshal.SizeOf(typeof(DS_REPL_PENDING_OPS)) + (long)(i * Marshal.SizeOf(typeof(DS_REPL_OP))));
				ReplicationOperation replicationOperation = new ReplicationOperation(intPtr, this.server, this.nameTable);
				this.Add(replicationOperation);
			}
		}

		public bool Contains(ReplicationOperation operation)
		{
			if (operation != null)
			{
				return base.InnerList.Contains(operation);
			}
			else
			{
				throw new ArgumentNullException("operation");
			}
		}

		public void CopyTo(ReplicationOperation[] operations, int index)
		{
			base.InnerList.CopyTo(operations, index);
		}

		internal ReplicationOperation GetFirstOperation()
		{
			ReplicationOperation item = (ReplicationOperation)base.InnerList[0];
			base.InnerList.RemoveAt(0);
			return item;
		}

		public int IndexOf(ReplicationOperation operation)
		{
			if (operation != null)
			{
				return base.InnerList.IndexOf(operation);
			}
			else
			{
				throw new ArgumentNullException("operation");
			}
		}
	}
}