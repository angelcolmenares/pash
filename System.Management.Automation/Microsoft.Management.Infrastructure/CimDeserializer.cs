using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Management.Infrastructure.Serialization
{
    //TODO:


	public sealed class CimDeserializer : IDisposable
	{

		private bool _disposed;

		private CimDeserializer(string format, int flags)
		{
            //TODO:
		}

		internal void AssertNotDisposed()
		{
			if (!this._disposed)
			{
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().FullName);
			}
		}

		public static CimDeserializer Create()
		{
			return new CimDeserializer("MI_XML", 0);
		}

		public static CimDeserializer Create(string format, int flags)
		{
			if (!string.IsNullOrEmpty(format))
			{
				return new CimDeserializer(format, flags);
			}
			else
			{
				throw new ArgumentNullException("format");
			}
		}

		public CimClass DeserializeClass(byte[] serializedData, ref int offset)
		{
			return this.DeserializeClass(serializedData, ref offset, null);
		}

		public CimClass DeserializeClass(byte[] serializedData, ref int offset, CimClass parentClass)
		{
			return this.DeserializeClass(serializedData, ref offset, parentClass, null, null);
		}

		public CimClass DeserializeClass(byte[] serializedData, ref int offset, CimClass parentClass, string computerName, string namespaceName)
		{
			ClassHandle classHandle = this.DeserializeClassHandle(serializedData, ref offset, parentClass, computerName, namespaceName);
			return new CimClass(classHandle);
		}

		internal ClassHandle DeserializeClassHandle (byte[] serializedData, ref int offset, CimClass parentClass, string computerName, string namespaceName)
		{
			return null;
		}
		
		public CimInstance DeserializeInstance(byte[] serializedData, ref int offset)
		{
			return this.DeserializeInstance(serializedData, ref offset, null);
		}

		public CimInstance DeserializeInstance(byte[] serializedData, ref int offset, IEnumerable<CimClass> cimClasses)
		{
            //TODO:
			InstanceHandle instanceHandle = this.DeserializeInstanceHandle(serializedData, ref offset, cimClasses);
			return new CimInstance(instanceHandle, null);
		}

		internal Microsoft.Management.Infrastructure.Native.InstanceHandle DeserializeInstanceHandle (byte[] serializedData, ref int offset,  IEnumerable<CimClass> classes)
		{
			return null; //TODO:
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this._disposed)
			{
				if (disposing)
				{
                    
				}
				this._disposed = true;
				return;
			}
			else
			{
				return;
			}
		}
	}
}