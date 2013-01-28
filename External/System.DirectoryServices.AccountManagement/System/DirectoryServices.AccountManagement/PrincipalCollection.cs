using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Runtime;
using System.Security;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class PrincipalCollection : ICollection<Principal>, ICollection, IEnumerable<Principal>, IEnumerable
	{
		[SecuritySafeCritical]
		private GroupPrincipal owningGroup;

		private BookmarkableResultSet resultSet;

		private List<Principal> insertedValuesCompleted;

		private List<Principal> insertedValuesPending;

		private List<Principal> removedValuesCompleted;

		private List<Principal> removedValuesPending;

		private bool clearPending;

		private bool clearCompleted;

		private DateTime lastChange;

		private bool disposed;

		internal bool Changed
		{
			get
			{
				if (this.insertedValuesPending.Count > 0 || this.removedValuesPending.Count > 0)
				{
					return true;
				}
				else
				{
					return this.clearPending;
				}
			}
		}

		internal bool ClearCompleted
		{
			get
			{
				return this.clearCompleted;
			}
		}

		internal bool Cleared
		{
			get
			{
				return this.clearPending;
			}
		}

		public int Count
		{
			[SecurityCritical]
			get
			{
				int num;
				this.CheckDisposed();
				lock (this.resultSet)
				{
					ResultSetBookmark resultSetBookmark = null;
					try
					{
						resultSetBookmark = this.resultSet.BookmarkAndReset();
						PrincipalCollectionEnumerator principalCollectionEnumerator = new PrincipalCollectionEnumerator(this.resultSet, this, this.removedValuesCompleted, this.removedValuesPending, this.insertedValuesCompleted, this.insertedValuesPending);
						int num1 = 0;
						while (principalCollectionEnumerator.MoveNext())
						{
							num1++;
						}
						num = num1;
					}
					finally
					{
						if (resultSetBookmark != null)
						{
							this.resultSet.Reset();
							this.resultSet.RestoreBookmark(resultSetBookmark);
						}
					}
				}
				return num;
			}
		}

		internal List<Principal> Inserted
		{
			get
			{
				return this.insertedValuesPending;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		internal DateTime LastChange
		{
			get
			{
				return this.lastChange;
			}
		}

		internal List<Principal> Removed
		{
			get
			{
				return this.removedValuesPending;
			}
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		int System.Collections.ICollection.Count
		{
			[SecurityCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.Count;
			}
		}

		bool System.Collections.ICollection.IsSynchronized
		{
			[SecurityCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.IsSynchronized;
			}
		}

		object System.Collections.ICollection.SyncRoot
		{
			[SecurityCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.SyncRoot;
			}
		}

		[SecurityCritical]
		internal PrincipalCollection(BookmarkableResultSet results, GroupPrincipal owningGroup)
		{
			this.insertedValuesCompleted = new List<Principal>();
			this.insertedValuesPending = new List<Principal>();
			this.removedValuesCompleted = new List<Principal>();
			this.removedValuesPending = new List<Principal>();
			this.lastChange = DateTime.UtcNow;
			this.resultSet = results;
			this.owningGroup = owningGroup;
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Add(UserPrincipal user)
		{
			this.Add(user);
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Add(GroupPrincipal group)
		{
			this.Add(group);
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Add(ComputerPrincipal computer)
		{
			this.Add(computer);
		}

		[SecurityCritical]
		public void Add(Principal principal)
		{
			this.CheckDisposed();
			if (principal != null)
			{
				if (!this.Contains(principal))
				{
					this.MarkChange();
					if (!this.removedValuesPending.Contains(principal))
					{
						this.insertedValuesPending.Add(principal);
						this.removedValuesCompleted.Remove(principal);
					}
					else
					{
						this.removedValuesPending.Remove(principal);
						if (!this.insertedValuesCompleted.Contains(principal))
						{
							this.insertedValuesCompleted.Add(principal);
							return;
						}
					}
					return;
				}
				else
				{
					throw new PrincipalExistsException(StringResources.PrincipalExistsExceptionText);
				}
			}
			else
			{
				throw new ArgumentNullException("principal");
			}
		}

		[SecurityCritical]
		public void Add(PrincipalContext context, IdentityType identityType, string identityValue)
		{
			this.CheckDisposed();
			if (context != null)
			{
				if (identityValue != null)
				{
					Principal principal = Principal.FindByIdentity(context, identityType, identityValue);
					if (principal == null)
					{
						throw new NoMatchingPrincipalException(StringResources.NoMatchingPrincipalExceptionText);
					}
					else
					{
						this.Add(principal);
						return;
					}
				}
				else
				{
					throw new ArgumentNullException("identityValue");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
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
				throw new ObjectDisposedException("PrincipalCollection");
			}
		}

		[SecurityCritical]
		public void Clear()
		{
			string str = null;
			this.CheckDisposed();
			StoreCtx storeCtxToUse = this.owningGroup.GetStoreCtxToUse();
			if (storeCtxToUse == null || storeCtxToUse.CanGroupBeCleared(this.owningGroup, out str))
			{
				this.MarkChange();
				this.insertedValuesPending.Clear();
				this.removedValuesPending.Clear();
				this.insertedValuesCompleted.Clear();
				this.removedValuesCompleted.Clear();
				this.clearPending = true;
				return;
			}
			else
			{
				throw new InvalidOperationException(str);
			}
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public bool Contains(UserPrincipal user)
		{
			return this.Contains(user);
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public bool Contains(GroupPrincipal group)
		{
			return this.Contains(group);
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public bool Contains(ComputerPrincipal computer)
		{
			return this.Contains(computer);
		}

		[SecurityCritical]
		public bool Contains(Principal principal)
		{
			StoreCtx storeCtxToUse = this.owningGroup.GetStoreCtxToUse();
			if (storeCtxToUse == null || !storeCtxToUse.SupportsNativeMembershipTest)
			{
				return this.ContainsEnumTest(principal);
			}
			else
			{
				return this.ContainsNativeTest(principal);
			}
		}

		[SecurityCritical]
		public bool Contains(PrincipalContext context, IdentityType identityType, string identityValue)
		{
			this.CheckDisposed();
			if (context != null)
			{
				if (identityValue != null)
				{
					bool flag = false;
					Principal principal = Principal.FindByIdentity(context, identityType, identityValue);
					if (principal != null)
					{
						flag = this.Contains(principal);
					}
					return flag;
				}
				else
				{
					throw new ArgumentNullException("identityValue");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		[SecuritySafeCritical]
		private bool ContainsEnumTest(Principal principal)
		{
			bool flag;
			this.CheckDisposed();
			if (principal != null)
			{
				lock (this.resultSet)
				{
					ResultSetBookmark resultSetBookmark = null;
					try
					{
						resultSetBookmark = this.resultSet.BookmarkAndReset();
						PrincipalCollectionEnumerator principalCollectionEnumerator = new PrincipalCollectionEnumerator(this.resultSet, this, this.removedValuesCompleted, this.removedValuesPending, this.insertedValuesCompleted, this.insertedValuesPending);
						while (principalCollectionEnumerator.MoveNext())
						{
							Principal current = principalCollectionEnumerator.Current;
							if (!current.Equals(principal))
							{
								continue;
							}
							flag = true;
							return flag;
						}
					}
					finally
					{
						if (resultSetBookmark != null)
						{
							this.resultSet.RestoreBookmark(resultSetBookmark);
						}
					}
					return false;
				}
				return flag;
			}
			else
			{
				throw new ArgumentNullException("principal");
			}
		}

		[SecuritySafeCritical]
		private bool ContainsNativeTest(Principal principal)
		{
			this.CheckDisposed();
			if (principal != null)
			{
				if (this.insertedValuesCompleted.Contains(principal) || this.insertedValuesPending.Contains(principal))
				{
					return true;
				}
				else
				{
					if (this.removedValuesCompleted.Contains(principal) || this.removedValuesPending.Contains(principal))
					{
						return false;
					}
					else
					{
						if (this.clearPending || this.clearCompleted)
						{
							return false;
						}
						else
						{
							if (this.owningGroup.unpersisted || principal.unpersisted)
							{
								return false;
							}
							else
							{
								return this.owningGroup.GetStoreCtxToUse().IsMemberOfInStore(this.owningGroup, principal);
							}
						}
					}
				}
			}
			else
			{
				throw new ArgumentNullException("principal");
			}
		}

		public void CopyTo(Principal[] array, int index)
		{
			this.CopyTo(array, index);
		}

		internal void Dispose()
		{
			if (!this.disposed)
			{
				lock (this.resultSet)
				{
					if (this.resultSet != null)
					{
						this.resultSet.Dispose();
					}
				}
				this.disposed = true;
			}
		}

		[SecurityCritical]
		public IEnumerator<Principal> GetEnumerator()
		{
			this.CheckDisposed();
			return new PrincipalCollectionEnumerator(this.resultSet, this, this.removedValuesCompleted, this.removedValuesPending, this.insertedValuesCompleted, this.insertedValuesPending);
		}

		internal void MarkChange()
		{
			this.lastChange = DateTime.UtcNow;
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public bool Remove(UserPrincipal user)
		{
			return this.Remove(user);
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public bool Remove(GroupPrincipal group)
		{
			return this.Remove(group);
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public bool Remove(ComputerPrincipal computer)
		{
			return this.Remove(computer);
		}

		[SecurityCritical]
		public bool Remove(Principal principal)
		{
			string str = null;
			bool flag;
			this.CheckDisposed();
			if (principal != null)
			{
				StoreCtx storeCtxToUse = this.owningGroup.GetStoreCtxToUse();
				if (storeCtxToUse == null || storeCtxToUse.CanGroupMemberBeRemoved(this.owningGroup, principal, out str))
				{
					if (!this.insertedValuesPending.Contains(principal))
					{
						flag = this.Contains(principal);
						if (flag)
						{
							this.MarkChange();
							this.removedValuesPending.Add(principal);
							this.insertedValuesCompleted.Remove(principal);
						}
					}
					else
					{
						this.MarkChange();
						this.insertedValuesPending.Remove(principal);
						flag = true;
						if (!this.removedValuesCompleted.Contains(principal))
						{
							this.removedValuesCompleted.Add(principal);
						}
					}
					return flag;
				}
				else
				{
					throw new InvalidOperationException(str);
				}
			}
			else
			{
				throw new ArgumentNullException("principal");
			}
		}

		[SecurityCritical]
		public bool Remove(PrincipalContext context, IdentityType identityType, string identityValue)
		{
			this.CheckDisposed();
			if (context != null)
			{
				if (identityValue != null)
				{
					Principal principal = Principal.FindByIdentity(context, identityType, identityValue);
					if (principal != null)
					{
						return this.Remove(principal);
					}
					else
					{
						throw new NoMatchingPrincipalException(StringResources.NoMatchingPrincipalExceptionText);
					}
				}
				else
				{
					throw new ArgumentNullException("identityValue");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		internal void ResetTracking()
		{
			foreach (Principal principal in this.removedValuesPending)
			{
				this.removedValuesCompleted.Add(principal);
			}
			this.removedValuesPending.Clear();
			foreach (Principal principal1 in this.insertedValuesPending)
			{
				this.insertedValuesCompleted.Add(principal1);
			}
			this.insertedValuesPending.Clear();
			if (this.clearPending)
			{
				this.clearCompleted = true;
				this.clearPending = false;
			}
		}

		[SecurityCritical]
		void System.Collections.ICollection.CopyTo(Array array, int index)
		{
			this.CheckDisposed();
			if (index >= 0)
			{
				if (array != null)
				{
					if (array.Rank == 1)
					{
						if (index < array.GetLength(0))
						{
							ArrayList arrayLists = new ArrayList();
							lock (this.resultSet)
							{
								ResultSetBookmark resultSetBookmark = null;
								try
								{
									resultSetBookmark = this.resultSet.BookmarkAndReset();
									PrincipalCollectionEnumerator principalCollectionEnumerator = new PrincipalCollectionEnumerator(this.resultSet, this, this.removedValuesCompleted, this.removedValuesPending, this.insertedValuesCompleted, this.insertedValuesPending);
									int length = array.GetLength(0) - index;
									int num = 0;
									while (principalCollectionEnumerator.MoveNext())
									{
										arrayLists.Add(principalCollectionEnumerator.Current);
										num++;
										if (length >= num)
										{
											continue;
										}
										throw new ArgumentException(StringResources.PrincipalCollectionArrayTooSmall);
									}
								}
								finally
								{
									if (resultSetBookmark != null)
									{
										this.resultSet.RestoreBookmark(resultSetBookmark);
									}
								}
							}
							foreach (object arrayList in arrayLists)
							{
								array.SetValue(arrayList, index);
								index++;
							}
							return;
						}
						else
						{
							throw new ArgumentException(StringResources.PrincipalCollectionIndexNotInArray);
						}
					}
					else
					{
						throw new ArgumentException(StringResources.PrincipalCollectionNotOneDimensional);
					}
				}
				else
				{
					throw new ArgumentNullException("array");
				}
			}
			else
			{
				throw new ArgumentOutOfRangeException("index");
			}
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}