using System;
using System.IO;

namespace System.Runtime
{
	internal class BufferedOutputStream : Stream
	{
		private InternalBufferManager bufferManager;

		private byte[][] chunks;

		private int chunkCount;

		private byte[] currentChunk;

		private int currentChunkSize;

		private int maxSize;

		private int maxSizeQuota;

		private int totalSize;

		private bool callerReturnsBuffer;

		private bool bufferReturned;

		private bool initialized;

		public override bool CanRead
		{
			get
			{
				return false;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}

		public override long Length
		{
			get
			{
				return (long)this.totalSize;
			}
		}

		public override long Position
		{
			get
			{
				throw Fx.Exception.AsError(new NotSupportedException(InternalSR.SeekNotSupported));
			}
			set
			{
				throw Fx.Exception.AsError(new NotSupportedException(InternalSR.SeekNotSupported));
			}
		}

		public BufferedOutputStream()
		{
			this.chunks = new byte[4][];
		}

		public BufferedOutputStream(int initialSize, int maxSize, InternalBufferManager bufferManager) : this()
		{
			this.Reinitialize(initialSize, maxSize, bufferManager);
		}

		public BufferedOutputStream(int maxSize) : this(0, maxSize, InternalBufferManager.Create((long)0, 0x7fffffff))
		{
		}

		private void AllocNextChunk(int minimumChunkSize)
		{
			int length;
			if ((int)this.currentChunk.Length <= 0x3fffffff)
			{
				length = (int)this.currentChunk.Length * 2;
			}
			else
			{
				length = 0x7fffffff;
			}
			if (minimumChunkSize > length)
			{
				length = minimumChunkSize;
			}
			byte[] numArray = this.bufferManager.TakeBuffer(length);
			if (this.chunkCount == (int)this.chunks.Length)
			{
				byte[][] numArray1 = new byte[(int)this.chunks.Length * 2][];
				Array.Copy(this.chunks, numArray1, (int)this.chunks.Length);
				this.chunks = numArray1;
			}
			BufferedOutputStream bufferedOutputStream = this;
			int num = bufferedOutputStream.chunkCount;
			int num1 = num;
			bufferedOutputStream.chunkCount = num + 1;
			this.chunks[num1] = numArray;
			this.currentChunk = numArray;
			this.currentChunkSize = 0;
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ReadNotSupported));
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			this.Write(buffer, offset, size);
			return new CompletedAsyncResult(callback, state);
		}

		public void Clear()
		{
			if (!this.callerReturnsBuffer)
			{
				for (int i = 0; i < this.chunkCount; i++)
				{
					this.bufferManager.ReturnBuffer(this.chunks[i]);
					this.chunks[i] = null;
				}
			}
			this.callerReturnsBuffer = false;
			this.initialized = false;
			this.bufferReturned = false;
			this.chunkCount = 0;
			this.currentChunk = null;
		}

		public override void Close()
		{
		}

		protected virtual Exception CreateQuotaExceededException(int maxSizeQuota)
		{
			return new InvalidOperationException(InternalSR.BufferedOutputStreamQuotaExceeded(maxSizeQuota));
		}

		public override int EndRead(IAsyncResult result)
		{
			throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ReadNotSupported));
		}

		public override void EndWrite(IAsyncResult result)
		{
			CompletedAsyncResult.End(result);
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int size)
		{
			throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ReadNotSupported));
		}

		public override int ReadByte()
		{
			throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ReadNotSupported));
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Reinitialize(int initialSize, int maxSizeQuota, InternalBufferManager bufferManager)
		{
			this.Reinitialize(initialSize, maxSizeQuota, maxSizeQuota, bufferManager);
		}

		public void Reinitialize(int initialSize, int maxSizeQuota, int effectiveMaxSize, InternalBufferManager bufferManager)
		{
			this.maxSizeQuota = maxSizeQuota;
			this.maxSize = effectiveMaxSize;
			this.bufferManager = bufferManager;
			this.currentChunk = bufferManager.TakeBuffer(initialSize);
			this.currentChunkSize = 0;
			this.totalSize = 0;
			this.chunkCount = 1;
			this.chunks[0] = this.currentChunk;
			this.initialized = true;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw Fx.Exception.AsError(new NotSupportedException(InternalSR.SeekNotSupported));
		}

		public override void SetLength(long value)
		{
			throw Fx.Exception.AsError(new NotSupportedException(InternalSR.SeekNotSupported));
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Skip(int size)
		{
			this.WriteCore(null, 0, size);
		}

		public byte[] ToArray(out int bufferSize)
		{
			byte[] numArray;
			if (this.chunkCount != 1)
			{
				numArray = this.bufferManager.TakeBuffer(this.totalSize);
				int length = 0;
				int num = this.chunkCount - 1;
				for (int i = 0; i < num; i++)
				{
					byte[] numArray1 = this.chunks[i];
					Buffer.BlockCopy(numArray1, 0, numArray, length, (int)numArray1.Length);
					length = length + (int)numArray1.Length;
				}
				Buffer.BlockCopy(this.currentChunk, 0, numArray, length, this.currentChunkSize);
				bufferSize = this.totalSize;
			}
			else
			{
				numArray = this.currentChunk;
				bufferSize = this.currentChunkSize;
				this.callerReturnsBuffer = true;
			}
			this.bufferReturned = true;
			return numArray;
		}

		public MemoryStream ToMemoryStream()
		{
			int num = 0;
			byte[] array = this.ToArray(out num);
			return new MemoryStream(array, 0, num);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public override void Write(byte[] buffer, int offset, int size)
		{
			this.WriteCore(buffer, offset, size);
		}

		public override void WriteByte(byte value)
		{
			if (this.totalSize != this.maxSize)
			{
				if (this.currentChunkSize == (int)this.currentChunk.Length)
				{
					this.AllocNextChunk(1);
				}
				BufferedOutputStream bufferedOutputStream = this;
				int num = bufferedOutputStream.currentChunkSize;
				int num1 = num;
				bufferedOutputStream.currentChunkSize = num + 1;
				this.currentChunk[num1] = value;
				return;
			}
			else
			{
				throw Fx.Exception.AsError(this.CreateQuotaExceededException(this.maxSize));
			}
		}

		private void WriteCore(byte[] buffer, int offset, int size)
		{
			if (size >= 0)
			{
				if (0x7fffffff - size >= this.totalSize)
				{
					int num = this.totalSize + size;
					if (num <= this.maxSize)
					{
						int length = (int)this.currentChunk.Length - this.currentChunkSize;
						if (size > length)
						{
							if (length > 0)
							{
								if (buffer != null)
								{
									Buffer.BlockCopy(buffer, offset, this.currentChunk, this.currentChunkSize, length);
								}
								this.currentChunkSize = (int)this.currentChunk.Length;
								offset = offset + length;
								size = size - length;
							}
							this.AllocNextChunk(size);
						}
						if (buffer != null)
						{
							Buffer.BlockCopy(buffer, offset, this.currentChunk, this.currentChunkSize, size);
						}
						this.totalSize = num;
						BufferedOutputStream bufferedOutputStream = this;
						bufferedOutputStream.currentChunkSize = bufferedOutputStream.currentChunkSize + size;
						return;
					}
					else
					{
						throw Fx.Exception.AsError(this.CreateQuotaExceededException(this.maxSizeQuota));
					}
				}
				else
				{
					throw Fx.Exception.AsError(this.CreateQuotaExceededException(this.maxSizeQuota));
				}
			}
			else
			{
				throw Fx.Exception.ArgumentOutOfRange("size", size, InternalSR.ValueMustBeNonNegative);
			}
		}
	}
}