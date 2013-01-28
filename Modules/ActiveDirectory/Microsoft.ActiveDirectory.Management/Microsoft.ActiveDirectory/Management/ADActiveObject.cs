using Microsoft.ActiveDirectory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADActiveObject : IDisposable
	{
		private const string _debugCategory = "ADActiveObject";

		private ADObject _adObject;

		private ADSession _adSession;

		private IADSyncOperations _syncOps;

		private ADSessionHandle _sessionHandle;

		private ADTypeConverter _typeConverter;

		private SecurityMasks _sdFlags;

		private bool _disposed;

		public SecurityMasks SecurityDescriptorFlags
		{
			get
			{
				return this._sdFlags;
			}
			set
			{
				this._sdFlags = value;
			}
		}

		public ADActiveObject(ADObject obj)
		{
			this._sdFlags = SecurityMasks.Dacl;
			if (obj != null)
			{
				this._adSession = ADSession.ConstructSession(obj.SessionInfo);
				this._adObject = obj;
				return;
			}
			else
			{
				DebugLogger.LogWarning("ADActiveObject", "Constructor(ADObject) called with null obj");
				throw new ArgumentNullException("obj");
			}
		}

		public ADActiveObject(ADSessionInfo sessionInfo, ADObject obj)
		{
			this._sdFlags = SecurityMasks.Dacl;
			if (obj != null)
			{
				this._adSession = ADSession.ConstructSession(sessionInfo);
				this._adObject = obj;
				return;
			}
			else
			{
				DebugLogger.LogWarning("ADActiveObject", "Constructor(ADSessionInfo,ADObject) called with null obj");
				throw new ArgumentNullException("obj");
			}
		}

		private ADActiveObject(ADSession session, ADObject obj)
		{
			this._sdFlags = SecurityMasks.Dacl;
			if (obj != null)
			{
				if (session != null)
				{
					this._adObject = obj;
					this._adSession = session;
					return;
				}
				else
				{
					DebugLogger.LogWarning("ADActiveObject", "Constructor(ADSession,ADObject) called with null session");
					throw new ArgumentNullException("session");
				}
			}
			else
			{
				DebugLogger.LogWarning("ADActiveObject", "Constructor(ADSession,ADObject) called with null obj");
				throw new ArgumentNullException("obj");
			}
		}

		public void Create()
		{
			this.Init();
			DebugLogger.WriteLine("ADActiveObject", string.Concat("Create called for ", this._adObject.DistinguishedName));
			if (this._adObject.ObjectClass != null)
			{
				DirectoryAttribute[] directoryAttributeArray = new DirectoryAttribute[this._adObject.NonNullCount - 1];
				int num = 0;
				bool flag = false;
				foreach (KeyValuePair<string, ADPropertyValueCollection> keyValuePair in this._adObject)
				{
					string key = keyValuePair.Key;
					if (flag || string.Compare(key, "distinguishedName", StringComparison.OrdinalIgnoreCase) != 0)
					{
						DirectoryAttribute directoryAttribute = this.CreateDirectoryAttribute(key, keyValuePair.Value, null);
						if (directoryAttribute == null)
						{
							continue;
						}
						directoryAttributeArray[num] = directoryAttribute;
						num++;
					}
					else
					{
						flag = true;
					}
				}
				ADAddRequest aDAddRequest = new ADAddRequest(this._adObject.DistinguishedName, directoryAttributeArray);
				if (this._sdFlags != SecurityMasks.None)
				{
					aDAddRequest.Controls.Add(new SecurityDescriptorFlagControl(this._sdFlags));
				}
				this._syncOps.Add(this._sessionHandle, aDAddRequest);
				DebugLogger.WriteLine("ADActiveObject", string.Concat("Create succeeded for ", this._adObject.DistinguishedName));
				return;
			}
			else
			{
				DebugLogger.LogWarning("ADActiveObject", "Create called with null objectClass");
				throw new ArgumentNullException("objectClass");
			}
		}

		private DirectoryAttributeModification CreateDirAttrModification(string attrName, IList valueList, DirectoryAttributeModification attrMod)
		{
			if (attrMod == null)
			{
				attrMod = new DirectoryAttributeModification();
			}
			attrMod.Name = attrName;
			if (valueList == null || valueList.Count == 0 || valueList[0] == null)
			{
				attrMod.Operation = DirectoryAttributeOperation.Delete;
			}
			else
			{
				this.CreateDirectoryAttribute(attrName, valueList, attrMod);
			}
			return attrMod;
		}

		private DirectoryAttribute CreateDirectoryAttribute(string attrName, IList valueList, DirectoryAttribute dirAttr)
		{
			if (valueList == null || valueList.Count == 0)
			{
				return null;
			}
			else
			{
				if (dirAttr == null)
				{
					dirAttr = new DirectoryAttribute();
				}
				dirAttr.Name = attrName;
				if (valueList.Count != 1)
				{
					dirAttr.AddRange(this._typeConverter.ConvertToRaw(attrName, valueList));
				}
				else
				{
					object raw = this._typeConverter.ConvertToRaw(attrName, valueList[0]);
					if (raw.GetType() != typeof(byte[]))
					{
						dirAttr.Add((string)raw);
					}
					else
					{
						dirAttr.Add((byte[])raw);
					}
				}
				return dirAttr;
			}
		}

		public void CrossDomainMove(string newParentDN, string newName, string targetDCName)
		{
			this.Init();
			DebugLogger.WriteLine("ADActiveObject", string.Concat("CrossDomainMove called for ", this._adObject.DistinguishedName));
			string[] strArrays = new string[6];
			strArrays[0] = "CrossDomainMove: newParentDN=";
			strArrays[1] = newParentDN;
			strArrays[2] = " newName=";
			strArrays[3] = newName;
			strArrays[4] = " targetDCName";
			strArrays[5] = targetDCName;
			DebugLogger.WriteLine("ADActiveObject", string.Concat(strArrays));
			ADModifyDNRequest aDModifyDNRequest = new ADModifyDNRequest(this._adObject.DistinguishedName, newParentDN, newName);
			CrossDomainMoveControl crossDomainMoveControl = new CrossDomainMoveControl(targetDCName);
			aDModifyDNRequest.Controls.Add(crossDomainMoveControl);
			this._syncOps.ModifyDN(this._sessionHandle, aDModifyDNRequest);
			DebugLogger.WriteLine("ADActiveObject", string.Concat("CrossDomainMove succeeded for ", this._adObject.DistinguishedName));
		}

		public void Delete()
		{
			this.Delete(false);
		}

		public void Delete(bool isDeleted)
		{
			bool value;
			this.Init();
			DebugLogger.WriteLine("ADActiveObject", string.Concat("Delete called for ", this._adObject.DistinguishedName));
			if (isDeleted)
			{
				value = true;
			}
			else
			{
				if (!this._adObject.Contains("Deleted"))
				{
					value = false;
				}
				else
				{
					value = (bool)this._adObject.GetValue("Deleted");
				}
			}
			isDeleted = value;
			ADDeleteRequest aDDeleteRequest = new ADDeleteRequest(this._adObject.DistinguishedName, isDeleted);
			this._syncOps.Delete(this._sessionHandle, aDDeleteRequest);
			DebugLogger.WriteLine("ADActiveObject", string.Concat("Delete succeeded for ", this._adObject.DistinguishedName));
		}

		public void DeleteTree()
		{
			this.DeleteTree(false);
		}

		public void DeleteTree(bool isDeleted)
		{
			bool value;
			this.Init();
			DebugLogger.WriteLine("ADActiveObject", string.Concat("DeleteTree called for ", this._adObject.DistinguishedName));
			if (isDeleted)
			{
				value = true;
			}
			else
			{
				if (!this._adObject.Contains("Deleted"))
				{
					value = false;
				}
				else
				{
					value = (bool)this._adObject.GetValue("Deleted");
				}
			}
			isDeleted = value;
			ADDeleteRequest aDDeleteRequest = new ADDeleteRequest(this._adObject.DistinguishedName, isDeleted);
			TreeDeleteControl treeDeleteControl = new TreeDeleteControl();
			aDDeleteRequest.Controls.Add(treeDeleteControl);
			this._syncOps.Delete(this._sessionHandle, aDDeleteRequest);
			DebugLogger.WriteLine("ADActiveObject", string.Concat("DeleteTree succeeded for ", this._adObject.DistinguishedName));
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Uninit();
			}
			this._disposed = true;
		}

		~ADActiveObject()
		{
			try
			{
				DebugLogger.WriteLine("ADActiveObject", "Destructor ADActiveObject");
				this.Dispose(false);
			}
			finally
			{
				//this.Finalize();
			}
		}

		private void Init()
		{
			object obj;
			if (!this._disposed)
			{
				if (this._syncOps == null)
				{
					if (string.IsNullOrEmpty(this._adObject.DistinguishedName))
					{
						string str = "ADActiveObject";
						string str1 = "Init: DistinguishedName is {0}";
						object[] objArray = new object[1];
						object[] objArray1 = objArray;
						int num = 0;
						if (this._adObject.DistinguishedName == null)
						{
							obj = "null";
						}
						else
						{
							obj = "empty";
						}
						objArray1[num] = obj;
						DebugLogger.LogInfo(str, str1, objArray);
					}
					this._sessionHandle = this._adSession.GetSessionHandle();
					this._syncOps = this._adSession.GetSyncOperationsInterface();
					this._typeConverter = new ADTypeConverter(this._adSession.SessionInfo);
				}
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public void Move(string newParentDN, string newName)
		{
			this.Init();
			string[] distinguishedName = new string[6];
			distinguishedName[0] = "Move called for ";
			distinguishedName[1] = this._adObject.DistinguishedName;
			distinguishedName[2] = ". newParentDN=";
			distinguishedName[3] = newParentDN;
			distinguishedName[4] = " newName=";
			distinguishedName[5] = newName;
			DebugLogger.WriteLine("ADActiveObject", string.Concat(distinguishedName));
			ADModifyDNRequest aDModifyDNRequest = new ADModifyDNRequest(this._adObject.DistinguishedName, newParentDN, newName);
			this._syncOps.ModifyDN(this._sessionHandle, aDModifyDNRequest);
			DebugLogger.WriteLine("ADActiveObject", string.Concat("Move succeeded for ", this._adObject.DistinguishedName));
		}

		public void Rename(string newName)
		{
			this.Init();
			DebugLogger.WriteLine("ADActiveObject", string.Concat("Rename called for ", this._adObject.DistinguishedName, ". newName is ", newName));
			ADModifyDNRequest aDModifyDNRequest = new ADModifyDNRequest(this._adObject.DistinguishedName, null, newName);
			this._syncOps.ModifyDN(this._sessionHandle, aDModifyDNRequest);
			DebugLogger.WriteLine("ADActiveObject", string.Concat("Rename succeeded for ", this._adObject.DistinguishedName));
		}

		public void Undelete(string newDN)
		{
			this.Init();
			if (newDN != null)
			{
				if (newDN.Length != 0)
				{
					DebugLogger.WriteLine("ADActiveObject", string.Concat("Undelete ", this._adObject.DistinguishedName, " to ", newDN));
					DirectoryAttributeModification[] directoryAttributeModification = new DirectoryAttributeModification[2];
					directoryAttributeModification[0] = new DirectoryAttributeModification();
					directoryAttributeModification[0].Name = "distinguishedName";
					directoryAttributeModification[0].Add(newDN);
					directoryAttributeModification[1] = new DirectoryAttributeModification();
					directoryAttributeModification[1].Name = "isDeleted";
					directoryAttributeModification[1].Operation = DirectoryAttributeOperation.Delete;
					ADModifyRequest aDModifyRequest = new ADModifyRequest(this._adObject.DistinguishedName, directoryAttributeModification);
					aDModifyRequest.Controls.Add(new PermissiveModifyControl());
					aDModifyRequest.Controls.Add(new ShowDeletedControl());
					this._syncOps.Modify(this._sessionHandle, aDModifyRequest);
					string[] distinguishedName = new string[5];
					distinguishedName[0] = "Undelete ";
					distinguishedName[1] = this._adObject.DistinguishedName;
					distinguishedName[2] = " to ";
					distinguishedName[3] = newDN;
					distinguishedName[4] = " succeeded";
					DebugLogger.WriteLine("ADActiveObject", string.Concat(distinguishedName));
					return;
				}
				else
				{
					throw new ArgumentException(StringResources.EmptyStringParameter, "New DistinguishedName");
				}
			}
			else
			{
				throw new ArgumentNullException("New DistinguishedName");
			}
		}

		private void Uninit()
		{
			if (this._adSession != null)
			{
				this._adSession.Delete();
				this._adSession = null;
			}
		}

		public void Update()
		{
			ADPropertyValueCollection value;
			this.Init();
			DebugLogger.WriteLine("ADActiveObject", string.Concat("Update called for ", this._adObject.DistinguishedName));
			DirectoryAttributeModificationCollection directoryAttributeModificationCollection = new DirectoryAttributeModificationCollection();
			if (!this._adObject.TrackChanges)
			{
				bool flag = false;
				foreach (KeyValuePair<string, ADPropertyValueCollection> keyValuePair in this._adObject)
				{
					string key = keyValuePair.Key;
					if (flag || string.Compare(key, "distinguishedName", StringComparison.OrdinalIgnoreCase) != 0)
					{
						value = keyValuePair.Value;
						this.UpdateValueCollectionChanges(key, value, directoryAttributeModificationCollection);
					}
					else
					{
						flag = true;
					}
				}
			}
			else
			{
				foreach (string addedProperty in this._adObject.AddedProperties)
				{
					value = this._adObject[addedProperty];
					this.UpdateValueCollectionChanges(addedProperty, value, directoryAttributeModificationCollection);
				}
				foreach (string removedProperty in this._adObject.RemovedProperties)
				{
					DirectoryAttributeModification directoryAttributeModification = new DirectoryAttributeModification();
					directoryAttributeModification.Name = removedProperty;
					directoryAttributeModification.Operation = DirectoryAttributeOperation.Delete;
					directoryAttributeModificationCollection.Add(directoryAttributeModification);
				}
				foreach (string modifiedProperty in this._adObject.ModifiedProperties)
				{
					value = this._adObject[modifiedProperty];
					this.UpdateValueCollectionChanges(modifiedProperty, value, directoryAttributeModificationCollection);
				}
			}
			if (directoryAttributeModificationCollection.Count != 0)
			{
				DirectoryAttributeModification[] directoryAttributeModificationArray = new DirectoryAttributeModification[directoryAttributeModificationCollection.Count];
				directoryAttributeModificationCollection.CopyTo(directoryAttributeModificationArray, 0);
				ADModifyRequest aDModifyRequest = new ADModifyRequest(this._adObject.DistinguishedName, directoryAttributeModificationArray);
				PermissiveModifyControl permissiveModifyControl = new PermissiveModifyControl();
				aDModifyRequest.Controls.Add(permissiveModifyControl);
				if (this._sdFlags != SecurityMasks.None)
				{
					aDModifyRequest.Controls.Add(new SecurityDescriptorFlagControl(this._sdFlags));
				}
				this._syncOps.Modify(this._sessionHandle, aDModifyRequest);
				DebugLogger.WriteLine("ADActiveObject", string.Concat("Update succeeded for ", this._adObject.DistinguishedName));
				return;
			}
			else
			{
				return;
			}
		}

		private void UpdateValueCollectionChanges(string attrName, ADPropertyValueCollection valueCollection, DirectoryAttributeModificationCollection mods)
		{
			DirectoryAttributeModification directoryAttributeModification;
			if (valueCollection != null)
			{
				if (!valueCollection.TrackChanges)
				{
					directoryAttributeModification = this.CreateDirAttrModification(attrName, valueCollection, null);
					mods.Add(directoryAttributeModification);
				}
				else
				{
					if (!valueCollection.IsValuesCleared)
					{
						if (valueCollection.ReplacedValues.Count <= 0)
						{
							if (valueCollection.DeletedValues.Count > 0)
							{
								directoryAttributeModification = this.CreateDirAttrModification(attrName, valueCollection.DeletedValues, null);
								directoryAttributeModification.Operation = DirectoryAttributeOperation.Delete;
								mods.Add(directoryAttributeModification);
							}
							if (valueCollection.AddedValues.Count > 0)
							{
								directoryAttributeModification = new DirectoryAttributeModification();
								directoryAttributeModification.Operation = DirectoryAttributeOperation.Add;
								this.CreateDirAttrModification(attrName, valueCollection.AddedValues, directoryAttributeModification);
								mods.Add(directoryAttributeModification);
								return;
							}
						}
						else
						{
							directoryAttributeModification = this.CreateDirAttrModification(attrName, valueCollection.ReplacedValues, null);
							mods.Add(directoryAttributeModification);
							return;
						}
					}
					else
					{
						directoryAttributeModification = new DirectoryAttributeModification();
						directoryAttributeModification.Name = attrName;
						directoryAttributeModification.Operation = DirectoryAttributeOperation.Delete;
						mods.Add(directoryAttributeModification);
						return;
					}
				}
				return;
			}
			else
			{
				directoryAttributeModification = new DirectoryAttributeModification();
				directoryAttributeModification.Name = attrName;
				directoryAttributeModification.Operation = DirectoryAttributeOperation.Delete;
				mods.Add(directoryAttributeModification);
				return;
			}
		}
	}
}