using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class ClassName : DataType
	{
		private readonly string m_schemaName;

		private readonly string m_identifier;

		public string FullName
		{
			get
			{
				if (this.m_schemaName == null)
				{
					return this.m_identifier;
				}
				else
				{
					return string.Concat(this.m_schemaName, "_", this.m_identifier);
				}
			}
		}

		public string Identifier
		{
			get
			{
				return this.m_identifier;
			}
		}

		public string Schema
		{
			get
			{
				return this.m_schemaName;
			}
		}

		public override DataTypeType Type
		{
			get
			{
				return DataTypeType.ClassName;
			}
		}

		internal ClassName(string schemaName, string identifier)
		{
			this.m_schemaName = schemaName;
			this.m_identifier = identifier;
		}

		public override bool Equals(object obj)
		{
			ClassName className = obj as ClassName;
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			else
			{
				if (object.ReferenceEquals(className, null) || !(className.m_identifier == this.m_identifier))
				{
					return false;
				}
				else
				{
					return className.m_schemaName == this.m_schemaName;
				}
			}
		}

		public override int GetHashCode()
		{
			int hashCode;
			int num = this.m_identifier.GetHashCode();
			int num1 = num;
			if (this.m_schemaName != null)
			{
				hashCode = this.m_schemaName.GetHashCode();
			}
			else
			{
				hashCode = 0;
			}
			num = num1 ^ hashCode;
			return num;
		}

		public static bool operator ==(ClassName cn1, ClassName cn2)
		{
			if (object.ReferenceEquals(cn1, cn2))
			{
				return true;
			}
			else
			{
				if (object.ReferenceEquals(cn1, null))
				{
					return false;
				}
				else
				{
					return cn1.Equals(cn2);
				}
			}
		}

		public static bool operator !=(ClassName cn1, ClassName cn2)
		{
			return !(cn1 == cn2);
		}

		public override string ToString()
		{
			return this.FullName;
		}
	}
}