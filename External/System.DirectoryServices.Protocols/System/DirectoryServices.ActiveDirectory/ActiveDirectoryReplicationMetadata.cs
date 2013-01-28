using System;
using System.Collections;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ActiveDirectoryReplicationMetadata : DictionaryBase
	{
		private DirectoryServer server;

		private Hashtable nameTable;

		private AttributeMetadataCollection dataValueCollection;

		private ReadOnlyStringCollection dataNameCollection;

		public ReadOnlyStringCollection AttributeNames
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.dataNameCollection;
			}
		}

		public AttributeMetadata this[string name]
		{
			get
			{
				string lower = name.ToLower(CultureInfo.InvariantCulture);
				if (!this.Contains(lower))
				{
					return null;
				}
				else
				{
					return (AttributeMetadata)base.InnerHashtable[lower];
				}
			}
		}

		public AttributeMetadataCollection Values
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.dataValueCollection;
			}
		}

		internal ActiveDirectoryReplicationMetadata(DirectoryServer server)
		{
			this.dataValueCollection = new AttributeMetadataCollection();
			this.dataNameCollection = new ReadOnlyStringCollection();
			this.server = server;
			Hashtable hashtables = new Hashtable();
			this.nameTable = Hashtable.Synchronized(hashtables);
		}

		private void Add(string name, AttributeMetadata value)
		{
			base.Dictionary.Add(name.ToLower(CultureInfo.InvariantCulture), value);
			this.dataNameCollection.Add(name);
			this.dataValueCollection.Add(value);
		}

		internal void AddHelper(int count, IntPtr info, bool advanced)
		{
			IntPtr intPtr;
			for (int i = 0; i < count; i++)
			{
				if (!advanced)
				{
					intPtr = (IntPtr)((long)info + (long)(Marshal.SizeOf(typeof(int)) * 2) + (long)(i * Marshal.SizeOf(typeof(DS_REPL_ATTR_META_DATA))));
					AttributeMetadata attributeMetadatum = new AttributeMetadata(intPtr, false, this.server, this.nameTable);
					this.Add(attributeMetadatum.Name, attributeMetadatum);
				}
				else
				{
					intPtr = (IntPtr)((long)info + (long)(Marshal.SizeOf(typeof(int)) * 2) + (long)(i * Marshal.SizeOf(typeof(DS_REPL_ATTR_META_DATA_2))));
					AttributeMetadata attributeMetadatum1 = new AttributeMetadata(intPtr, true, this.server, this.nameTable);
					this.Add(attributeMetadatum1.Name, attributeMetadatum1);
				}
			}
		}

		public bool Contains(string attributeName)
		{
			string lower = attributeName.ToLower(CultureInfo.InvariantCulture);
			return base.Dictionary.Contains(lower);
		}

		public void CopyTo(AttributeMetadata[] array, int index)
		{
			base.Dictionary.Values.CopyTo(array, index);
		}
	}
}