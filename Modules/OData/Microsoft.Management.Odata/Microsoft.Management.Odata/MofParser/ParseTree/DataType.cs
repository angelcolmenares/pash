using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal abstract class DataType
	{
		public abstract DataTypeType Type
		{
			get;
		}

		internal DataType()
		{
		}

		public override bool Equals(object obj)
		{
			return obj as DataType == this;
		}

		public override int GetHashCode()
		{
			return this.Type.GetHashCode();
		}

		public static bool operator ==(DataType o1, DataType o2)
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

		public static bool operator !=(DataType o1, DataType o2)
		{
			return !(o1 == o2);
		}
	}
}