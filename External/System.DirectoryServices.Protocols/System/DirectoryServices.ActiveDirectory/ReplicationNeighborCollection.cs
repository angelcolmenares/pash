using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReplicationNeighborCollection : ReadOnlyCollectionBase
	{
		private DirectoryServer server;

		private Hashtable nameTable;

		public ReplicationNeighbor this[int index]
		{
			get
			{
				return (ReplicationNeighbor)base.InnerList[index];
			}
		}

		internal ReplicationNeighborCollection(DirectoryServer server)
		{
			this.server = server;
			Hashtable hashtables = new Hashtable();
			this.nameTable = Hashtable.Synchronized(hashtables);
		}

		private int Add(ReplicationNeighbor neighbor)
		{
			return base.InnerList.Add(neighbor);
		}

		internal void AddHelper(DS_REPL_NEIGHBORS neighbors, IntPtr info)
		{
			int num = neighbors.cNumNeighbors;
			for (int i = 0; i < num; i++)
			{
				IntPtr intPtr = (IntPtr)((long)info + (long)(Marshal.SizeOf(typeof(int)) * 2) + (long)(i * Marshal.SizeOf(typeof(DS_REPL_NEIGHBOR))));
				ReplicationNeighbor replicationNeighbor = new ReplicationNeighbor(intPtr, this.server, this.nameTable);
				this.Add(replicationNeighbor);
			}
		}

		public bool Contains(ReplicationNeighbor neighbor)
		{
			if (neighbor != null)
			{
				return base.InnerList.Contains(neighbor);
			}
			else
			{
				throw new ArgumentNullException("neighbor");
			}
		}

		public void CopyTo(ReplicationNeighbor[] neighbors, int index)
		{
			base.InnerList.CopyTo(neighbors, index);
		}

		public int IndexOf(ReplicationNeighbor neighbor)
		{
			if (neighbor != null)
			{
				return base.InnerList.IndexOf(neighbor);
			}
			else
			{
				throw new ArgumentNullException("neighbor");
			}
		}
	}
}