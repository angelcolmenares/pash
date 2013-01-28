using Microsoft.Management.Infrastructure.Options;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class CimPromptUser : CimSyncAction
	{
		private string message;

		private CimPromptType prompt;

		public CimPromptUser(string message, CimPromptType prompt)
		{
			this.message = message;
			this.prompt = prompt;
		}

		public override void Execute(CmdletOperationBase cmdlet)
		{
			bool flag;
			ValidationHelper.ValidateNoNullArgument(cmdlet, "cmdlet");
			bool flag1 = false;
			bool flag2 = false;
			CimPromptType cimPromptType = this.prompt;
			switch (cimPromptType)
			{
				case CimPromptType.None:
				{
					flag = cmdlet.ShouldProcess(this.message);
					if (!flag)
					{
						if (flag)
						{
							break;
						}
						this.responseType = CimResponseType.None;
						break;
					}
					else
					{
						this.responseType = CimResponseType.Yes;
						break;
					}
				}
				case CimPromptType.Critical:
				{
					flag = cmdlet.ShouldContinue(this.message, "caption", ref flag1, ref flag2);
					if (!flag1)
					{
						if (!flag2)
						{
							if (!flag)
							{
								if (flag)
								{
									break;
								}
								this.responseType = CimResponseType.None;
								break;
							}
							else
							{
								this.responseType = CimResponseType.Yes;
								break;
							}
						}
						else
						{
							this.responseType = CimResponseType.NoToAll;
							break;
						}
					}
					else
					{
						this.responseType = CimResponseType.YesToAll;
						break;
					}
				}
			}
			this.OnComplete();
		}
	}
}