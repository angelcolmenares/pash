using System;
using System.Collections.Generic;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class ObjectReference : DataType
	{
		private readonly ClassName m_name;

		private Dictionary<string, object> m_annotations;

		public object this[string annotationName]
		{
			get
			{
				object obj = null;
				if (this.m_annotations != null)
				{
					this.m_annotations.TryGetValue(annotationName, out obj);
				}
				return obj;
			}
			set
			{
				if (value != null)
				{
					if (this.m_annotations == null)
					{
						this.m_annotations = new Dictionary<string, object>();
					}
					this.m_annotations[annotationName] = value;
				}
				else
				{
					if (this.m_annotations != null)
					{
						this.m_annotations.Remove(annotationName);
						return;
					}
				}
			}
		}

		public ClassName Name
		{
			get
			{
				return this.m_name;
			}
		}

		public override DataTypeType Type
		{
			get
			{
				return DataTypeType.ObjectReference;
			}
		}

		public ObjectReference(ClassName name)
		{
			this.m_name = name;
		}

		public override bool Equals(object obj)
		{
			ObjectReference objectReference = obj as ObjectReference;
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			else
			{
				if (object.ReferenceEquals(objectReference, null))
				{
					return false;
				}
				else
				{
					return objectReference.Name.Equals(this.Name);
				}
			}
		}

		public override int GetHashCode()
		{
			return this.Name.GetHashCode();
		}

		public static bool operator ==(ObjectReference o1, ObjectReference o2)
		{
			if (object.ReferenceEquals(o1, o2))
			{
				return true;
			}
			else
			{
				if (object.ReferenceEquals(o1, null))
				{
					return false;
				}
				else
				{
					return o1.Equals(o2);
				}
			}
		}

		public static bool operator !=(ObjectReference o1, ObjectReference o2)
		{
			return !(o1 == o2);
		}

		public override string ToString()
		{
			return string.Concat(this.Name, " REF");
		}
	}
}