using System;
using System.Collections;
using System.DirectoryServices;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ActiveDirectorySchemaClassCollection : CollectionBase
	{
		private DirectoryEntry classEntry;

		private string propertyName;

		private ActiveDirectorySchemaClass schemaClass;

		private bool isBound;

		private DirectoryContext context;

		public ActiveDirectorySchemaClass this[int index]
		{
			get
			{
				return (ActiveDirectorySchemaClass)base.List[index];
			}
			set
			{
				if (value != null)
				{
					if (value.isBound)
					{
						if (this.Contains(value))
						{
							object[] objArray = new object[1];
							objArray[0] = value;
							throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "value");
						}
						else
						{
							base.List[index] = value;
							return;
						}
					}
					else
					{
						object[] name = new object[1];
						name[0] = value.Name;
						throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", name));
					}
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		internal ActiveDirectorySchemaClassCollection(DirectoryContext context, ActiveDirectorySchemaClass schemaClass, bool isBound, string propertyName, ICollection classNames, bool onlyNames)
		{
			this.schemaClass = schemaClass;
			this.propertyName = propertyName;
			this.isBound = isBound;
			this.context = context;
			foreach (string className in classNames)
			{
				base.InnerList.Add(new ActiveDirectorySchemaClass(context, className, (DirectoryEntry)null, (DirectoryEntry)null));
			}
		}

		internal ActiveDirectorySchemaClassCollection(DirectoryContext context, ActiveDirectorySchemaClass schemaClass, bool isBound, string propertyName, ICollection classes)
		{
			this.schemaClass = schemaClass;
			this.propertyName = propertyName;
			this.isBound = isBound;
			this.context = context;
			foreach (ActiveDirectorySchemaClass @class in classes)
			{
				base.InnerList.Add(@class);
			}
		}

		public int Add(ActiveDirectorySchemaClass schemaClass)
		{
			if (schemaClass != null)
			{
				if (schemaClass.isBound)
				{
					if (this.Contains(schemaClass))
					{
						object[] objArray = new object[1];
						objArray[0] = schemaClass;
						throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "schemaClass");
					}
					else
					{
						return base.List.Add(schemaClass);
					}
				}
				else
				{
					object[] name = new object[1];
					name[0] = schemaClass.Name;
					throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("schemaClass");
			}
		}

		public void AddRange(ActiveDirectorySchemaClass[] schemaClasses)
		{
			if (schemaClasses != null)
			{
				ActiveDirectorySchemaClass[] activeDirectorySchemaClassArray = schemaClasses;
				int num = 0;
				while (num < (int)activeDirectorySchemaClassArray.Length)
				{
					ActiveDirectorySchemaClass activeDirectorySchemaClass = activeDirectorySchemaClassArray[num];
					if (activeDirectorySchemaClass != null)
					{
						num++;
					}
					else
					{
						throw new ArgumentException("schemaClasses");
					}
				}
				for (int i = 0; i < (int)schemaClasses.Length; i++)
				{
					this.Add(schemaClasses[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("schemaClasses");
			}
		}

		public void AddRange(ActiveDirectorySchemaClassCollection schemaClasses)
		{
			if (schemaClasses != null)
			{
				foreach (ActiveDirectorySchemaClass schemaClass in schemaClasses)
				{
					if (schemaClass != null)
					{
						continue;
					}
					throw new ArgumentException("schemaClasses");
				}
				int count = schemaClasses.Count;
				for (int i = 0; i < count; i++)
				{
					this.Add(schemaClasses[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("schemaClasses");
			}
		}

		public void AddRange(ReadOnlyActiveDirectorySchemaClassCollection schemaClasses)
		{
			if (schemaClasses != null)
			{
				foreach (ActiveDirectorySchemaClass schemaClass in schemaClasses)
				{
					if (schemaClass != null)
					{
						continue;
					}
					throw new ArgumentException("schemaClasses");
				}
				int count = schemaClasses.Count;
				for (int i = 0; i < count; i++)
				{
					this.Add(schemaClasses[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("schemaClasses");
			}
		}

		public bool Contains(ActiveDirectorySchemaClass schemaClass)
		{
			if (schemaClass != null)
			{
				if (schemaClass.isBound)
				{
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySchemaClass item = (ActiveDirectorySchemaClass)base.InnerList[num];
						if (Utils.Compare(item.Name, schemaClass.Name) != 0)
						{
							num++;
						}
						else
						{
							return true;
						}
					}
					return false;
				}
				else
				{
					object[] name = new object[1];
					name[0] = schemaClass.Name;
					throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("schemaClass");
			}
		}

		public void CopyTo(ActiveDirectorySchemaClass[] schemaClasses, int index)
		{
			base.List.CopyTo(schemaClasses, index);
		}

		internal string[] GetMultiValuedProperty()
		{
			string[] name = new string[base.InnerList.Count];
			for (int i = 0; i < base.InnerList.Count; i++)
			{
				name[i] = ((ActiveDirectorySchemaClass)base.InnerList[i]).Name;
			}
			return name;
		}

		public int IndexOf(ActiveDirectorySchemaClass schemaClass)
		{
			if (schemaClass != null)
			{
				if (schemaClass.isBound)
				{
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySchemaClass item = (ActiveDirectorySchemaClass)base.InnerList[num];
						if (Utils.Compare(item.Name, schemaClass.Name) != 0)
						{
							num++;
						}
						else
						{
							return num;
						}
					}
					return -1;
				}
				else
				{
					object[] name = new object[1];
					name[0] = schemaClass.Name;
					throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("schemaClass");
			}
		}

		public void Insert(int index, ActiveDirectorySchemaClass schemaClass)
		{
			if (schemaClass != null)
			{
				if (schemaClass.isBound)
				{
					if (this.Contains(schemaClass))
					{
						object[] objArray = new object[1];
						objArray[0] = schemaClass;
						throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "schemaClass");
					}
					else
					{
						base.List.Insert(index, schemaClass);
						return;
					}
				}
				else
				{
					object[] name = new object[1];
					name[0] = schemaClass.Name;
					throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("schemaClass");
			}
		}

		protected override void OnClearComplete()
		{
			if (this.isBound)
			{
				if (this.classEntry == null)
				{
					this.classEntry = this.schemaClass.GetSchemaClassDirectoryEntry();
				}
				try
				{
					if (this.classEntry.Properties.Contains(this.propertyName))
					{
						this.classEntry.Properties[this.propertyName].Clear();
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			if (this.isBound)
			{
				if (this.classEntry == null)
				{
					this.classEntry = this.schemaClass.GetSchemaClassDirectoryEntry();
				}
				try
				{
					this.classEntry.Properties[this.propertyName].Add(((ActiveDirectorySchemaClass)value).Name);
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			if (this.isBound)
			{
				if (this.classEntry == null)
				{
					this.classEntry = this.schemaClass.GetSchemaClassDirectoryEntry();
				}
				string name = ((ActiveDirectorySchemaClass)value).Name;
				try
				{
					if (!this.classEntry.Properties[this.propertyName].Contains(name))
					{
						throw new ActiveDirectoryOperationException(Res.GetString("ValueCannotBeModified"));
					}
					else
					{
						this.classEntry.Properties[this.propertyName].Remove(name);
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
		}

		protected override void OnSetComplete(int index, object oldValue, object newValue)
		{
			if (this.isBound)
			{
				this.OnRemoveComplete(index, oldValue);
				this.OnInsertComplete(index, newValue);
			}
		}

		protected override void OnValidate(object value)
		{
			if (value != null)
			{
				if (value as ActiveDirectorySchemaClass != null)
				{
					if (((ActiveDirectorySchemaClass)value).isBound)
					{
						return;
					}
					else
					{
						object[] name = new object[1];
						name[0] = ((ActiveDirectorySchemaClass)value).Name;
						throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", name));
					}
				}
				else
				{
					throw new ArgumentException("value");
				}
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		public void Remove(ActiveDirectorySchemaClass schemaClass)
		{
			if (schemaClass != null)
			{
				if (schemaClass.isBound)
				{
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySchemaClass item = (ActiveDirectorySchemaClass)base.InnerList[num];
						if (Utils.Compare(item.Name, schemaClass.Name) != 0)
						{
							num++;
						}
						else
						{
							base.List.Remove(item);
							return;
						}
					}
					object[] objArray = new object[1];
					objArray[0] = schemaClass;
					throw new ArgumentException(Res.GetString("NotFoundInCollection", objArray), "schemaClass");
				}
				else
				{
					object[] name = new object[1];
					name[0] = schemaClass.Name;
					throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("schemaClass");
			}
		}
	}
}