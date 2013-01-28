using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure.Serialization
{
	public sealed class CimSerializer : IDisposable
	{
		
		private bool _disposed;

		private CimSerializer(string format, int flags)
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

		public static CimSerializer Create()
		{
			return new CimSerializer("MI_XML", 0);
		}

		public static CimSerializer Create(string format, int flags)
		{
			if (!string.IsNullOrEmpty(format))
			{
				return new CimSerializer(format, flags);
			}
			else
			{
				throw new ArgumentNullException("format");
			}
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

		public bool Serialize(CimInstance cimInstance, InstanceSerializationOptions options, byte[] buffer, ref int offset)
		{
			int num = 0;
			if (cimInstance != null)
			{
				if (buffer != null)
				{
					if ((long)offset <= (long)((int)buffer.Length))
					{
						if ((long)offset == (long)((int)buffer.Length))
						{
							buffer = null;
						}
					}
					else
					{
						throw new ArgumentOutOfRangeException("offset");
					}
				}
				else
				{
					if (offset != 0)
					{
						throw new ArgumentNullException("buffer");
					}
				}
				this.AssertNotDisposed();
				bool flag = true;
                MiResult miResult = MiResult.OK; //TODO: // SerializerMethods.SerializeInstance(this._myHandle, options, cimInstance.InstanceHandle, buffer, offset, out num);
				MiResult miResult1 = miResult;
				switch (miResult1)
				{
					case MiResult.OK:
					{
						offset = offset + num;
						flag = true;
						return flag;
					}
					case MiResult.FAILED:
					{
						if (buffer == null || (long)(offset + num) > (long)((int)buffer.Length))
						{
							miResult = MiResult.OK;
							offset = offset + num;
						}
						flag = false;
						return flag;
					}
				}
				CimException.ThrowIfMiResultFailure(miResult);
				return flag;
			}
			else
			{
				throw new ArgumentNullException("cimInstance");
			}
		}

		public bool Serialize(CimClass cimClass, ClassSerializationOptions options, byte[] buffer, ref int offset)
		{
			int num = 0;
			if (cimClass != null)
			{
				if (buffer != null)
				{
					if ((long)offset <= (long)((int)buffer.Length))
					{
						if ((long)offset == (long)((int)buffer.Length))
						{
							buffer = null;
						}
					}
					else
					{
						throw new ArgumentOutOfRangeException("offset");
					}
				}
				else
				{
					if (offset != 0)
					{
						throw new ArgumentNullException("buffer");
					}
				}
				this.AssertNotDisposed();
				bool flag = true;
                MiResult miResult = MiResult.OK; //TODO: // SerializerMethods.SerializeClass(this._myHandle, options, cimClass.ClassHandle, buffer, offset, out num);
				MiResult miResult1 = miResult;
				switch (miResult1)
				{
					case MiResult.OK:
					{
						offset = offset + num;
						flag = true;
						return flag;
					}
					case MiResult.FAILED:
					{
						if (buffer == null || (long)(offset + num) > (long)((int)buffer.Length))
						{
							miResult = MiResult.OK;
							offset = offset + num;
						}
						flag = false;
						return flag;
					}
				}
				CimException.ThrowIfMiResultFailure(miResult);
				return flag;
			}
			else
			{
				throw new ArgumentNullException("cimClass");
			}
		}

		public byte[] Serialize(CimInstance cimInstance, InstanceSerializationOptions options)
		{
			unsafe
			{
				if (cimInstance != null)
				{
					this.AssertNotDisposed();
					int num = 0;
					this.Serialize(cimInstance, options, null, ref num);
					byte[] numArray = new byte[num];
					int num1 = 0;
					this.Serialize(cimInstance, options, numArray, ref num1);
					return numArray;
				}
				else
				{
					throw new ArgumentNullException("cimInstance");
				}
			}
		}

		public byte[] Serialize(CimClass cimClass, ClassSerializationOptions options)
		{
			unsafe
			{
				if (cimClass != null)
				{
					this.AssertNotDisposed();
					int num = 0;
					this.Serialize(cimClass, options, null, ref num);
					byte[] numArray = new byte[num];
					int num1 = 0;
					this.Serialize(cimClass, options, numArray, ref num1);
					return numArray;
				}
				else
				{
					throw new ArgumentNullException("cimClass");
				}
			}
		}
	}
}