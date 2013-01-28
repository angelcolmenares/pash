using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal class DistinguishedName
	{
		private Component[] components;

		public Component[] Components
		{
			get
			{
				return this.components;
			}
		}

		public DistinguishedName(string dn)
		{
			this.components = Utils.GetDNComponents(dn);
		}

		public bool Equals(DistinguishedName dn)
		{
			bool flag = true;
			if (dn == null || this.components.GetLength(0) != dn.Components.GetLength(0))
			{
				flag = false;
			}
			else
			{
				int num = 0;
				while (num < this.components.GetLength(0))
				{
					if (Utils.Compare(this.components[num].Name, dn.Components[num].Name) != 0 || Utils.Compare(this.components[num].Value, dn.Components[num].Value) != 0)
					{
						flag = false;
						break;
					}
					else
					{
						num++;
					}
				}
			}
			return flag;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj as DistinguishedName == null)
			{
				return false;
			}
			else
			{
				return this.Equals((DistinguishedName)obj);
			}
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			for (int i = 0; i < this.components.GetLength(0); i++)
			{
				hashCode = hashCode + this.components[i].Name.ToUpperInvariant().GetHashCode() + this.components[i].Value.ToUpperInvariant().GetHashCode();
			}
			return hashCode;
		}

		public override string ToString()
		{
			string str = string.Concat(this.components[0].Name, "=", this.components[0].Value);
			for (int i = 1; i < this.components.GetLength(0); i++)
			{
				string[] name = new string[5];
				name[0] = str;
				name[1] = ",";
				name[2] = this.components[i].Name;
				name[3] = "=";
				name[4] = this.components[i].Value;
				str = string.Concat(name);
			}
			return str;
		}
	}
}