using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReplicationCursorCollection : ReadOnlyCollectionBase
	{
		private DirectoryServer server;

		public ReplicationCursor this[int index]
		{
			get
			{
				return (ReplicationCursor)base.InnerList[index];
			}
		}

		internal ReplicationCursorCollection(DirectoryServer server)
		{
			this.server = server;
		}

		private int Add(ReplicationCursor cursor)
		{
			return base.InnerList.Add(cursor);
		}

		internal void AddHelper(string partition, object cursors, bool advanced, IntPtr info)
		{
			int num;
			IntPtr intPtr;
			if (!advanced)
			{
				num = ((DS_REPL_CURSORS)cursors).cNumCursors;
			}
			else
			{
				num = ((DS_REPL_CURSORS_3)cursors).cNumCursors;
			}
			for (int i = 0; i < num; i++)
			{
				if (!advanced)
				{
					intPtr = (IntPtr)((long)info + (long)(Marshal.SizeOf(typeof(int)) * 2) + (long)(i * Marshal.SizeOf(typeof(DS_REPL_CURSOR))));
					DS_REPL_CURSOR dSREPLCURSOR = new DS_REPL_CURSOR();
					Marshal.PtrToStructure(intPtr, dSREPLCURSOR);
					ReplicationCursor replicationCursor = new ReplicationCursor(this.server, partition, dSREPLCURSOR.uuidSourceDsaInvocationID, dSREPLCURSOR.usnAttributeFilter);
					this.Add(replicationCursor);
				}
				else
				{
					intPtr = (IntPtr)((long)info + (long)(Marshal.SizeOf(typeof(int)) * 2) + (long)(i * Marshal.SizeOf(typeof(DS_REPL_CURSOR_3))));
					DS_REPL_CURSOR_3 dSREPLCURSOR3 = new DS_REPL_CURSOR_3();
					Marshal.PtrToStructure(intPtr, dSREPLCURSOR3);
					ReplicationCursor replicationCursor1 = new ReplicationCursor(this.server, partition, dSREPLCURSOR3.uuidSourceDsaInvocationID, dSREPLCURSOR3.usnAttributeFilter, dSREPLCURSOR3.ftimeLastSyncSuccess, dSREPLCURSOR3.pszSourceDsaDN);
					this.Add(replicationCursor1);
				}
			}
		}

		public bool Contains(ReplicationCursor cursor)
		{
			if (cursor != null)
			{
				return base.InnerList.Contains(cursor);
			}
			else
			{
				throw new ArgumentNullException("cursor");
			}
		}

		public void CopyTo(ReplicationCursor[] values, int index)
		{
			base.InnerList.CopyTo(values, index);
		}

		public int IndexOf(ReplicationCursor cursor)
		{
			if (cursor != null)
			{
				return base.InnerList.IndexOf(cursor);
			}
			else
			{
				throw new ArgumentNullException("cursor");
			}
		}
	}
}