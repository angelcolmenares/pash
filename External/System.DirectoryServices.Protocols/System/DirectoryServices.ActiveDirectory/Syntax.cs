using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal class Syntax
	{
		public string attributeSyntax;

		public int oMSyntax;

		public OMObjectClass oMObjectClass;

		public Syntax(string attributeSyntax, int oMSyntax, OMObjectClass oMObjectClass)
		{
			this.attributeSyntax = attributeSyntax;
			this.oMSyntax = oMSyntax;
			this.oMObjectClass = oMObjectClass;
		}

		public bool Equals(Syntax syntax)
		{
			bool flag = true;
			if (!syntax.attributeSyntax.Equals(this.attributeSyntax) || syntax.oMSyntax != this.oMSyntax)
			{
				flag = false;
			}
			else
			{
				if (this.oMObjectClass != null && syntax.oMObjectClass == null || this.oMObjectClass == null && syntax.oMObjectClass != null || this.oMObjectClass != null && syntax.oMObjectClass != null && !this.oMObjectClass.Equals(syntax.oMObjectClass))
				{
					flag = false;
				}
			}
			return flag;
		}
	}
}