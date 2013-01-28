using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class DangerousHandleAccessor : IDisposable
	{
		private bool needToCallDangerousRelease;

		private SafeHandle safeHandle;

		internal DangerousHandleAccessor(SafeHandle safeHandle)
		{
			this.safeHandle = safeHandle;
			this.needToCallDangerousRelease = false;
		}

		~DangerousHandleAccessor()
		{
			if (this.needToCallDangerousRelease)
			{
				this.safeHandle.DangerousRelease();
				this.needToCallDangerousRelease = false;
			}
		}


		private void Destroy ()
		{
			if (this.needToCallDangerousRelease)
			{
				this.safeHandle.DangerousRelease();
				this.needToCallDangerousRelease = false;
			}
		}

		[SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId="System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification="We are calling DangerousAddRef/Release as prescribed in the docs + have to do this to call inline methods")]
		internal IntPtr DangerousGetHandle()
		{
			SafeHandle safeHandle = this.safeHandle;
			if (safeHandle != null)
			{
				if (!this.needToCallDangerousRelease)
				{
					safeHandle.DangerousAddRef(ref this.needToCallDangerousRelease);
					if (!this.needToCallDangerousRelease)
					{
						throw new ObjectDisposedException(this.safeHandle.ToString());
					}
				}
				return this.safeHandle.DangerousGetHandle();
			}
			else
			{
				return IntPtr.Zero;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				//this.Finalize();
			}
			else
			{
				Destroy();
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}