using System;
using System.Collections;
using System.DirectoryServices;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ActiveDirectorySchemaPropertyCollection : CollectionBase
	{
		private DirectoryEntry classEntry;

		private string propertyName;

		private ActiveDirectorySchemaClass schemaClass;

		private bool isBound;

		private DirectoryContext context;

		public ActiveDirectorySchemaProperty this[int index]
		{
			get
			{
				return (ActiveDirectorySchemaProperty)base.List[index];
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

		internal ActiveDirectorySchemaPropertyCollection(DirectoryContext context, ActiveDirectorySchemaClass schemaClass, bool isBound, string propertyName, ICollection propertyNames, bool onlyNames)
		{
			this.schemaClass = schemaClass;
			this.propertyName = propertyName;
			this.isBound = isBound;
			this.context = context;
			foreach (string str in propertyNames)
			{
				base.InnerList.Add(new ActiveDirectorySchemaProperty(context, str, (DirectoryEntry)null, (DirectoryEntry)null));
			}
		}

		internal ActiveDirectorySchemaPropertyCollection(DirectoryContext context, ActiveDirectorySchemaClass schemaClass, bool isBound, string propertyName, ICollection properties)
		{
			this.schemaClass = schemaClass;
			this.propertyName = propertyName;
			this.isBound = isBound;
			this.context = context;
			foreach (ActiveDirectorySchemaProperty property in properties)
			{
				base.InnerList.Add(property);
			}
		}

		public int Add(ActiveDirectorySchemaProperty schemaProperty)
		{
			if (schemaProperty != null)
			{
				if (schemaProperty.isBound)
				{
					if (this.Contains(schemaProperty))
					{
						object[] objArray = new object[1];
						objArray[0] = schemaProperty;
						throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "schemaProperty");
					}
					else
					{
						return base.List.Add(schemaProperty);
					}
				}
				else
				{
					object[] name = new object[1];
					name[0] = schemaProperty.Name;
					throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("schemaProperty");
			}
		}

		public void AddRange(ActiveDirectorySchemaProperty[] properties)
		{
			if (properties != null)
			{
				ActiveDirectorySchemaProperty[] activeDirectorySchemaPropertyArray = properties;
				int num = 0;
				while (num < (int)activeDirectorySchemaPropertyArray.Length)
				{
					ActiveDirectorySchemaProperty activeDirectorySchemaProperty = activeDirectorySchemaPropertyArray[num];
					if (activeDirectorySchemaProperty != null)
					{
						num++;
					}
					else
					{
						throw new ArgumentException("properties");
					}
				}
				for (int i = 0; i < (int)properties.Length; i++)
				{
					this.Add(properties[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("properties");
			}
		}

		public void AddRange(ActiveDirectorySchemaPropertyCollection properties)
		{
			if (properties != null)
			{
				foreach (ActiveDirectorySchemaProperty property in properties)
				{
					if (property != null)
					{
						continue;
					}
					throw new ArgumentException("properties");
				}
				int count = properties.Count;
				for (int i = 0; i < count; i++)
				{
					this.Add(properties[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("properties");
			}
		}

		public void AddRange(ReadOnlyActiveDirectorySchemaPropertyCollection properties)
		{
			if (properties != null)
			{
				foreach (ActiveDirectorySchemaProperty property in properties)
				{
					if (property != null)
					{
						continue;
					}
					throw new ArgumentException("properties");
				}
				int count = properties.Count;
				for (int i = 0; i < count; i++)
				{
					this.Add(properties[i]);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("properties");
			}
		}

		public bool Contains(ActiveDirectorySchemaProperty schemaProperty)
		{
			if (schemaProperty != null)
			{
				if (schemaProperty.isBound)
				{
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySchemaProperty item = (ActiveDirectorySchemaProperty)base.InnerList[num];
						if (Utils.Compare(item.Name, schemaProperty.Name) != 0)
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
					name[0] = schemaProperty.Name;
					throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("schemaProperty");
			}
		}

		internal bool Contains(string propertyName)
		{
			int num = 0;
			while (num < base.InnerList.Count)
			{
				ActiveDirectorySchemaProperty item = (ActiveDirectorySchemaProperty)base.InnerList[num];
				if (Utils.Compare(item.Name, propertyName) != 0)
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

		public void CopyTo(ActiveDirectorySchemaProperty[] properties, int index)
		{
			base.List.CopyTo(properties, index);
		}

		internal string[] GetMultiValuedProperty()
		{
			string[] name = new string[base.InnerList.Count];
			for (int i = 0; i < base.InnerList.Count; i++)
			{
				name[i] = ((ActiveDirectorySchemaProperty)base.InnerList[i]).Name;
			}
			return name;
		}

		public int IndexOf(ActiveDirectorySchemaProperty schemaProperty)
		{
			if (schemaProperty != null)
			{
				if (schemaProperty.isBound)
				{
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySchemaProperty item = (ActiveDirectorySchemaProperty)base.InnerList[num];
						if (Utils.Compare(item.Name, schemaProperty.Name) != 0)
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
					name[0] = schemaProperty.Name;
					throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("schemaProperty");
			}
		}

		public void Insert(int index, ActiveDirectorySchemaProperty schemaProperty)
		{
			if (schemaProperty != null)
			{
				if (schemaProperty.isBound)
				{
					if (this.Contains(schemaProperty))
					{
						object[] objArray = new object[1];
						objArray[0] = schemaProperty;
						throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", objArray), "schemaProperty");
					}
					else
					{
						base.List.Insert(index, schemaProperty);
						return;
					}
				}
				else
				{
					object[] name = new object[1];
					name[0] = schemaProperty.Name;
					throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("schemaProperty");
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
					this.classEntry.Properties[this.propertyName].Add(((ActiveDirectorySchemaProperty)value).Name);
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
				string name = ((ActiveDirectorySchemaProperty)value).Name;
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
				if (value as ActiveDirectorySchemaProperty != null)
				{
					if (((ActiveDirectorySchemaProperty)value).isBound)
					{
						return;
					}
					else
					{
						object[] name = new object[1];
						name[0] = ((ActiveDirectorySchemaProperty)value).Name;
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

		public void Remove(ActiveDirectorySchemaProperty schemaProperty)
		{
			if (schemaProperty != null)
			{
				if (schemaProperty.isBound)
				{
					int num = 0;
					while (num < base.InnerList.Count)
					{
						ActiveDirectorySchemaProperty item = (ActiveDirectorySchemaProperty)base.InnerList[num];
						if (Utils.Compare(item.Name, schemaProperty.Name) != 0)
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
					objArray[0] = schemaProperty;
					throw new ArgumentException(Res.GetString("NotFoundInCollection", objArray), "schemaProperty");
				}
				else
				{
					object[] name = new object[1];
					name[0] = schemaProperty.Name;
					throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", name));
				}
			}
			else
			{
				throw new ArgumentNullException("schemaProperty");
			}
		}
	}
}