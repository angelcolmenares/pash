using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Management.Instrumentation
{
	internal class MetaDataInfo : IDisposable
	{
		private IMetaDataImportInternalOnly importInterface;

		private string name;

		private Guid mvid;

		public Guid Mvid
		{
			get
			{
				this.InitNameAndMvid();
				return this.mvid;
			}
		}

		public string Name
		{
			get
			{
				this.InitNameAndMvid();
				return this.name;
			}
		}

		public MetaDataInfo(Assembly assembly) : this(assembly.Location)
		{
		}

		public MetaDataInfo(string assemblyName)
		{
			Guid guid = new Guid(((GuidAttribute)Attribute.GetCustomAttribute(typeof(IMetaDataImportInternalOnly), typeof(GuidAttribute), false)).Value);
			IMetaDataDispenser corMetaDataDispenser = (IMetaDataDispenser)(new CorMetaDataDispenser());
			this.importInterface = (IMetaDataImportInternalOnly)corMetaDataDispenser.OpenScope(assemblyName, 0, ref guid);
			Marshal.ReleaseComObject(corMetaDataDispenser);
		}

		public void Dispose()
		{
			if (this.importInterface == null)
			{
				Marshal.ReleaseComObject(this.importInterface);
			}
			this.importInterface = null;
			GC.SuppressFinalize(this);
		}

		~MetaDataInfo()
		{
			try
			{
				this.Dispose();
			}
			finally
			{
				//this.Finalize();
			}
		}

		public static Guid GetMvid(Assembly assembly)
		{
			Guid mvid;
			using (MetaDataInfo metaDataInfo = new MetaDataInfo(assembly))
			{
				mvid = metaDataInfo.Mvid;
			}
			return mvid;
		}

		private void InitNameAndMvid()
		{
			int num = 0;
			if (this.name == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Capacity = 0;
				this.importInterface.GetScopeProps(stringBuilder, stringBuilder.Capacity, out num, out this.mvid);
				stringBuilder.Capacity = num;
				this.importInterface.GetScopeProps(stringBuilder, stringBuilder.Capacity, out num, out this.mvid);
				this.name = stringBuilder.ToString();
			}
		}
	}
}