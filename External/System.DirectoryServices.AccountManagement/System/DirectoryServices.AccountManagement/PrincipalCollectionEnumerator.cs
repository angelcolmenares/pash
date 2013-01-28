using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Security;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	internal class PrincipalCollectionEnumerator : IEnumerator<Principal>, IDisposable, IEnumerator
	{
		private Principal current;

		private ResultSet resultSet;

		private List<Principal> insertedValuesPending;

		private List<Principal> insertedValuesCompleted;

		private List<Principal> removedValuesPending;

		private List<Principal> removedValuesCompleted;

		private bool endReached;

		private IEnumerator<Principal> enumerator;

		private PrincipalCollectionEnumerator.CurrentEnumeratorMode currentMode;

		private bool disposed;

		private DateTime creationTime;

		private PrincipalCollection memberCollection;

		public Principal Current
		{
			[SecuritySafeCritical]
			get
			{
				this.CheckDisposed();
				if (this.endReached || this.currentMode == PrincipalCollectionEnumerator.CurrentEnumeratorMode.None)
				{
					throw new InvalidOperationException(StringResources.PrincipalCollectionEnumInvalidPos);
				}
				else
				{
					return this.current;
				}
			}
		}

		object System.Collections.IEnumerator.Current
		{
			[SecurityCritical]
			get
			{
				return this.Current;
			}
		}

		internal PrincipalCollectionEnumerator(ResultSet resultSet, PrincipalCollection memberCollection, List<Principal> removedValuesCompleted, List<Principal> removedValuesPending, List<Principal> insertedValuesCompleted, List<Principal> insertedValuesPending)
		{
			this.creationTime = DateTime.UtcNow;
			this.resultSet = resultSet;
			this.memberCollection = memberCollection;
			this.removedValuesCompleted = removedValuesCompleted;
			this.removedValuesPending = removedValuesPending;
			this.insertedValuesCompleted = insertedValuesCompleted;
			this.insertedValuesPending = insertedValuesPending;
		}

		[SecurityCritical]
		private void CheckChanged()
		{
			if (this.memberCollection.LastChange <= this.creationTime)
			{
				return;
			}
			else
			{
				throw new InvalidOperationException(StringResources.PrincipalCollectionEnumHasChanged);
			}
		}

		private void CheckDisposed()
		{
			if (!this.disposed)
			{
				return;
			}
			else
			{
				throw new ObjectDisposedException("PrincipalCollectionEnumerator");
			}
		}

		public void Dispose()
		{
			this.disposed = true;
		}

		[SecuritySafeCritical]
		public bool MoveNext()
		{
			bool flag;
			this.CheckDisposed();
			this.CheckChanged();
			if (!this.endReached)
			{
				lock (this.resultSet)
				{
					if (this.currentMode == PrincipalCollectionEnumerator.CurrentEnumeratorMode.None)
					{
						this.resultSet.Reset();
						if (this.memberCollection.Cleared || this.memberCollection.ClearCompleted)
						{
							this.currentMode = PrincipalCollectionEnumerator.CurrentEnumeratorMode.InsertedValuesCompleted;
							this.enumerator = this.insertedValuesCompleted.GetEnumerator();
						}
						else
						{
							this.currentMode = PrincipalCollectionEnumerator.CurrentEnumeratorMode.ResultSet;
							this.enumerator = null;
						}
					}
					if (this.currentMode == PrincipalCollectionEnumerator.CurrentEnumeratorMode.ResultSet)
					{
						bool flag1 = false;
						do
						{
							bool flag2 = this.resultSet.MoveNext();
							if (!flag2)
							{
								this.currentMode = PrincipalCollectionEnumerator.CurrentEnumeratorMode.InsertedValuesCompleted;
								this.enumerator = this.insertedValuesCompleted.GetEnumerator();
								flag1 = false;
							}
							else
							{
								Principal currentAsPrincipal = (Principal)this.resultSet.CurrentAsPrincipal;
								if (this.removedValuesCompleted.Contains(currentAsPrincipal) || this.removedValuesPending.Contains(currentAsPrincipal))
								{
									flag1 = true;
								}
								else
								{
									if (this.insertedValuesCompleted.Contains(currentAsPrincipal) || this.insertedValuesPending.Contains(currentAsPrincipal))
									{
										flag1 = true;
									}
									else
									{
										flag1 = false;
										this.current = currentAsPrincipal;
										flag = true;
										return flag;
									}
								}
							}
						}
						while (flag1);
					}
					if (this.currentMode == PrincipalCollectionEnumerator.CurrentEnumeratorMode.InsertedValuesCompleted)
					{
						bool flag3 = this.enumerator.MoveNext();
						if (!flag3)
						{
							this.currentMode = PrincipalCollectionEnumerator.CurrentEnumeratorMode.InsertedValuesPending;
							this.enumerator = this.insertedValuesPending.GetEnumerator();
						}
						else
						{
							this.current = this.enumerator.Current;
							flag = true;
							return flag;
						}
					}
					if (this.currentMode != PrincipalCollectionEnumerator.CurrentEnumeratorMode.InsertedValuesPending)
					{
						return false;
					}
					else
					{
						bool flag4 = this.enumerator.MoveNext();
						if (!flag4)
						{
							this.endReached = true;
							flag = false;
						}
						else
						{
							this.current = this.enumerator.Current;
							flag = true;
						}
					}
				}
				return flag;
			}
			else
			{
				return false;
			}
		}

		[SecurityCritical]
		public void Reset()
		{
			this.CheckDisposed();
			this.CheckChanged();
			this.endReached = false;
			this.enumerator = null;
			this.currentMode = PrincipalCollectionEnumerator.CurrentEnumeratorMode.None;
		}

		[SecurityCritical]
		bool System.Collections.IEnumerator.MoveNext()
		{
			return this.MoveNext();
		}

		[SecurityCritical]
		void System.Collections.IEnumerator.Reset()
		{
			this.Reset();
		}

		private enum CurrentEnumeratorMode
		{
			None,
			ResultSet,
			InsertedValuesCompleted,
			InsertedValuesPending
		}
	}
}