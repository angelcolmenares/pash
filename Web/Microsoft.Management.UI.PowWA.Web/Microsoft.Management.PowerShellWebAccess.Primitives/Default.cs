using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.UI;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class Default : Page
	{
		public Default()
		{
		}

		private bool DirectoryExists(string directory)
		{
			return Directory.Exists(base.Server.MapPath(directory));
		}

		public string GetCultureDirectory()
		{
			if (!this.DirectoryExists(CultureInfo.CurrentUICulture.Name))
			{
				if (!this.DirectoryExists(CultureInfo.InstalledUICulture.Name))
				{
					DirectoryInfo[] directories = (new DirectoryInfo(base.Server.MapPath("."))).GetDirectories("*-*");
					CultureInfo installedUICulture = CultureInfo.InstalledUICulture;
					while (installedUICulture != installedUICulture.Parent && installedUICulture.Parent != null)
					{
						installedUICulture = installedUICulture.Parent;
						DirectoryInfo[] directoryInfoArray = directories;
						for (int i = 0; i < (int)directoryInfoArray.Length; i++)
						{
							DirectoryInfo directoryInfo = directoryInfoArray[i];
							try
							{
								CultureInfo parent = CultureInfo.CreateSpecificCulture(directoryInfo.Name);
								while (parent != parent.Parent && parent.Parent != null)
								{
									parent = parent.Parent;
									if (string.Compare(parent.Name, installedUICulture.Name, StringComparison.OrdinalIgnoreCase) != 0 || !this.DirectoryExists(directoryInfo.Name))
									{
										continue;
									}
									string name = directoryInfo.Name;
									return name;
								}
							}
							catch (CultureNotFoundException cultureNotFoundException)
							{
							}
						}
					}
					if (!this.DirectoryExists("en-us"))
					{
						return null;
					}
					else
					{
						return "en-us";
					}
				}
				else
				{
					return CultureInfo.InstalledUICulture.Name;
				}
			}
			else
			{
				return CultureInfo.CurrentUICulture.Name;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (base.Request.PathInfo.Length == 0)
			{
				string cultureDirectory = this.GetCultureDirectory();
				if (cultureDirectory != null)
				{
					if (!PowwaSessionManager.Instance.SessionExists(this.Session.SessionID))
					{
						stringBuilder.Append(cultureDirectory).Append("/logon.aspx").Append(base.Request.Url.Query);
					}
					else
					{
						stringBuilder.Append(cultureDirectory).Append("/console.aspx");
					}
				}
			}
			if (stringBuilder.Length <= 0)
			{
				base.Response.StatusCode = 0x194;
				base.Response.End();
				return;
			}
			else
			{
				base.Response.Redirect(stringBuilder.ToString(), true);
				return;
			}
		}
	}
}