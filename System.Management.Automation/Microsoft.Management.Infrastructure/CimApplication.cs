using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal.Operations;
using Microsoft.Management.Infrastructure.Native;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;

namespace Microsoft.Management.Infrastructure.Internal
{
	internal static class CimApplication
	{
		private readonly static Lazy<ApplicationHandle> LazyHandle;

		private readonly static ReaderWriterLockSlim IsShutdownPendingLock;

		private static bool _isShutdownPending;

		private readonly static WeakReferenceHashSet<CimSession> TrackedCimSessions;

		private readonly static WeakReferenceHashSet<CimOperation> TrackedCimOperations;

		public static ApplicationHandle Handle
		{
			get
			{
				return CimApplication.LazyHandle.Value;
			}
		}

		static CimApplication()
		{
			CimApplication.LazyHandle = new Lazy<ApplicationHandle>(new Func<ApplicationHandle>(CimApplication.GetApplicationHandle));
			CimApplication.IsShutdownPendingLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
			CimApplication.TrackedCimSessions = new WeakReferenceHashSet<CimSession>();
			CimApplication.TrackedCimOperations = new WeakReferenceHashSet<CimOperation>();
		}

		internal static void AddTracking(CimSession cimSession)
		{
			CimApplication.TrackedCimSessions.Add(cimSession);
		}

		internal static void AddTracking(CimOperation cimOperation)
		{
			CimApplication.TrackedCimOperations.Add(cimOperation);
		}

		internal static IDisposable AssertNoPendingShutdown()
		{
			if (CimApplication.IsShutdownPendingLock.WaitingWriteCount == 0)
			{
				IDisposable readerWriterLockSlimReaderLock = new CimApplication.ReaderWriterLockSlim_ReaderLock(CimApplication.IsShutdownPendingLock);
				if (!CimApplication._isShutdownPending)
				{
					return readerWriterLockSlimReaderLock;
				}
				else
				{
					readerWriterLockSlimReaderLock.Dispose();
					throw new InvalidOperationException(Strings.AppDomainIsBeingUnloaded);
				}
			}
			else
			{
				throw new InvalidOperationException(Strings.AppDomainIsBeingUnloaded);
			}
		}

		private static ApplicationHandle GetApplicationHandle()
		{
			ApplicationHandle applicationHandle = null;
			InstanceHandle instanceHandle = null;
			MiResult miResult = ApplicationMethods.Initialize(0, AppDomain.CurrentDomain.FriendlyName, out instanceHandle, out applicationHandle);
			CimException.ThrowIfMiResultFailure(miResult, instanceHandle);
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.DomainUnload += (object sender, EventArgs eventArgs) => CimApplication.Shutdown();
			AppDomain appDomain = AppDomain.CurrentDomain;
			appDomain.ProcessExit += (object param0, EventArgs param1) => {
				ApplicationMethods.SupressFurtherCallbacks();
				CimApplication.Shutdown();
			};
			return applicationHandle;
		}

		internal static void RemoveTracking(CimSession cimSession)
		{
			CimApplication.TrackedCimSessions.Remove(cimSession);
		}

		internal static void RemoveTracking(CimOperation cimOperation)
		{
			CimApplication.TrackedCimOperations.Remove(cimOperation);
		}

		private static void Shutdown ()
		{
			CimApplication.IsShutdownPendingLock.EnterWriteLock ();
			try {
				if (!CimApplication._isShutdownPending) {
					CimApplication._isShutdownPending = true;
				} else {
					return;
				}
			} finally {
				CimApplication.IsShutdownPendingLock.ExitWriteLock ();
			}
			IEnumerable<CimSession> cimSessions = CimApplication.TrackedCimSessions.GetSnapshotOfLiveObjects ();

			IEnumerable<CimOperation> snapshotOfLiveObjects = CimApplication.TrackedCimOperations.GetSnapshotOfLiveObjects ();
			
			foreach (CimOperation snapshotOfLiveObject in snapshotOfLiveObjects) {
				snapshotOfLiveObject.Cancel (CancellationMode.ThrowOperationCancelledException);
			}
			GC.Collect();
			GC.WaitForPendingFinalizers();
			foreach (CimSession cimSession in cimSessions)
			{
				cimSession.Dispose();
			}
			CimApplication.Handle.Dispose();
		}

		private sealed class ReaderWriterLockSlim_ReaderLock : IDisposable
		{
			private readonly ReaderWriterLockSlim _readerWriterLockSlim;

			internal ReaderWriterLockSlim_ReaderLock(ReaderWriterLockSlim readerWriterLockSlim)
			{
				this._readerWriterLockSlim = readerWriterLockSlim;
				this._readerWriterLockSlim.EnterReadLock();
			}

			public void Dispose()
			{
				this._readerWriterLockSlim.ExitReadLock();
			}
		}
	}
}