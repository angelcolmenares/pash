using System;
using System.Management.Automation.Host;

namespace Microsoft.PowerShell
{
	internal class ProgressPane
	{
		private Coordinates location;

		private Size bufSize;

		private BufferCell[,] savedRegion;

		private BufferCell[,] progressRegion;

		private PSHostRawUserInterface rawui;

		private ConsoleHostUserInterface ui;

		internal bool IsShowing
		{
			get
			{
				return this.savedRegion != null;
			}
		}

		internal ProgressPane(ConsoleHostUserInterface ui)
		{
			this.location = new Coordinates(0, 0);
			if (ui != null)
			{
				this.ui = ui;
				this.rawui = ui.RawUI;
				return;
			}
			else
			{
				throw new ArgumentNullException("ui");
			}
		}

		internal void Hide()
		{
			if (this.IsShowing)
			{
				this.rawui.SetBufferContents(this.location, this.savedRegion);
				this.savedRegion = null;
			}
		}

		internal void Show()
		{
			BufferCell[,] bufferCellArray = this.progressRegion;
			if (bufferCellArray != null)
			{
				int length = bufferCellArray.GetLength(0);
				int num = bufferCellArray.GetLength(1);
				this.location = this.rawui.WindowPosition;
				this.location.X = 0;
				this.location.Y = Math.Min(this.location.Y + 2, this.bufSize.Height);
				this.savedRegion = this.rawui.GetBufferContents(new Rectangle(this.location.X, this.location.Y, this.location.X + num - 1, this.location.Y + length - 1));
				this.rawui.SetBufferContents(this.location, bufferCellArray);
				return;
			}
			else
			{
				return;
			}
		}

		internal void Show(PendingProgress pendingProgress)
		{
			bool flag;
			this.bufSize = this.rawui.BufferSize;
			int width = this.bufSize.Width;
			Size windowSize = this.rawui.WindowSize;
			int num = Math.Max(5, windowSize.Height / 3);
			string[] strArrays = pendingProgress.Render(width, num, this.rawui);
			if (strArrays != null)
			{
				BufferCell[,] bufferCellArray = this.rawui.NewBufferCellArray(strArrays, this.ui.ProgressForegroundColor, this.ui.ProgressBackgroundColor);
				if (this.progressRegion != null)
				{
					if (bufferCellArray.GetLength(0) != this.progressRegion.GetLength(0) || bufferCellArray.GetLength(1) != this.progressRegion.GetLength(1))
					{
						flag = true;
					}
					else
					{
						flag = false;
					}
					bool flag1 = flag;
					this.progressRegion = bufferCellArray;
					if (!flag1)
					{
						this.rawui.SetBufferContents(this.location, this.progressRegion);
						return;
					}
					else
					{
						if (this.IsShowing)
						{
							this.Hide();
						}
						this.Show();
						return;
					}
				}
				else
				{
					this.progressRegion = bufferCellArray;
					this.Show();
					return;
				}
			}
			else
			{
				this.Hide();
				this.progressRegion = null;
				return;
			}
		}
	}
}