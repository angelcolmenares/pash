using System;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell
{
	internal class ProgressNode : ProgressRecord
	{
		internal ArrayList Children;

		internal int Age;

		internal ProgressNode.RenderStyle Style;

		internal long SourceId;

		private int LinesRequiredInCompactStyle
		{
			get
			{
				int num = 2;
				if (!string.IsNullOrEmpty(base.CurrentOperation))
				{
					num++;
				}
				return num;
			}
		}

		internal ProgressNode(long sourceId, ProgressRecord record) : base(record.ActivityId, record.Activity, record.StatusDescription)
		{
			this.Style = ProgressNode.RenderStyle.FullPlus;
			base.ParentActivityId = record.ParentActivityId;
			base.CurrentOperation = record.CurrentOperation;
			base.PercentComplete = Math.Min(record.PercentComplete, 100);
			base.SecondsRemaining = record.SecondsRemaining;
			base.RecordType = record.RecordType;
			this.Style = ProgressNode.RenderStyle.FullPlus;
			this.SourceId = sourceId;
		}

		private int LinesRequiredInFullStyleMethod(PSHostRawUserInterface rawUi, int maxWidth, bool isFullPlus)
		{
			int count = 1;
			string str = new string(' ', 5);
			ArrayList arrayLists = new ArrayList();
			if (!isFullPlus)
			{
				count++;
			}
			else
			{
				arrayLists.Clear();
				ProgressNode.RenderFullDescription(base.StatusDescription, str, maxWidth, rawUi, arrayLists, true);
				count = count + arrayLists.Count;
			}
			if (base.PercentComplete >= 0)
			{
				count++;
			}
			if (base.SecondsRemaining >= 0)
			{
				count++;
			}
			if (!string.IsNullOrEmpty(base.CurrentOperation))
			{
				if (!isFullPlus)
				{
					count = count + 2;
				}
				else
				{
					count++;
					arrayLists.Clear();
					ProgressNode.RenderFullDescription(base.CurrentOperation, str, maxWidth, rawUi, arrayLists, true);
					count = count + arrayLists.Count;
				}
			}
			return count;
		}

		internal int LinesRequiredMethod(PSHostRawUserInterface rawUi, int maxWidth)
		{
			ProgressNode.RenderStyle style = this.Style;
			switch (style)
			{
				case ProgressNode.RenderStyle.Invisible:
				{
					return 0;
				}
				case ProgressNode.RenderStyle.Minimal:
				{
					return 1;
				}
				case ProgressNode.RenderStyle.Compact:
				{
					return this.LinesRequiredInCompactStyle;
				}
				case ProgressNode.RenderStyle.Full:
				{
					return this.LinesRequiredInFullStyleMethod(rawUi, maxWidth, false);
				}
				case ProgressNode.RenderStyle.FullPlus:
				{
					return this.LinesRequiredInFullStyleMethod(rawUi, maxWidth, true);
				}
			}
			return 0;
		}

		internal void Render(ArrayList strCollection, int indentation, int maxWidth, PSHostRawUserInterface rawUI)
		{
			ProgressNode.RenderStyle style = this.Style;
			switch (style)
			{
				case ProgressNode.RenderStyle.Invisible:
				{
					return;
				}
				case ProgressNode.RenderStyle.Minimal:
				{
					this.RenderMinimal(strCollection, indentation, maxWidth, rawUI);
					return;
				}
				case ProgressNode.RenderStyle.Compact:
				{
					this.RenderCompact(strCollection, indentation, maxWidth, rawUI);
					return;
				}
				case ProgressNode.RenderStyle.Full:
				{
					this.RenderFull(strCollection, indentation, maxWidth, rawUI, false);
					return;
				}
				case ProgressNode.RenderStyle.FullPlus:
				{
					this.RenderFull(strCollection, indentation, maxWidth, rawUI, true);
					return;
				}
				default:
				{
					return;
				}
			}
		}

		private void RenderCompact(ArrayList strCollection, int indentation, int maxWidth, PSHostRawUserInterface rawUI)
		{
			string str = new string(' ', indentation);
			strCollection.Add(StringUtil.TruncateToBufferCellWidth(rawUI, StringUtil.Format(" {0}{1} ", str, base.Activity), maxWidth));
			indentation = indentation + 3;
			str = new string(' ', indentation);
			string str1 = "";
			if (base.PercentComplete >= 0)
			{
				str1 = StringUtil.Format("{0}% ", base.PercentComplete);
			}
			string str2 = "";
			if (base.SecondsRemaining >= 0)
			{
				TimeSpan timeSpan = new TimeSpan(0, 0, base.SecondsRemaining);
				str2 = string.Concat(timeSpan.ToString(), " ");
			}
			object[] statusDescription = new object[4];
			statusDescription[0] = str;
			statusDescription[1] = str1;
			statusDescription[2] = str2;
			statusDescription[3] = base.StatusDescription;
			strCollection.Add(StringUtil.TruncateToBufferCellWidth(rawUI, StringUtil.Format(" {0}{1}{2}{3} ", statusDescription), maxWidth));
			if (!string.IsNullOrEmpty(base.CurrentOperation))
			{
				strCollection.Add(StringUtil.TruncateToBufferCellWidth(rawUI, StringUtil.Format(" {0}{1} ", str, base.CurrentOperation), maxWidth));
			}
		}

		private void RenderFull(ArrayList strCollection, int indentation, int maxWidth, PSHostRawUserInterface rawUI, bool isFullPlus)
		{
			string str = new string(' ', indentation);
			strCollection.Add(StringUtil.TruncateToBufferCellWidth(rawUI, StringUtil.Format(" {0}{1} ", str, base.Activity), maxWidth));
			indentation = indentation + 3;
			str = new string(' ', indentation);
			ProgressNode.RenderFullDescription(base.StatusDescription, str, maxWidth, rawUI, strCollection, isFullPlus);
			if (base.PercentComplete >= 0)
			{
				int num = Math.Max(3, maxWidth - indentation - 2 - 2 - 5);
				int percentComplete = base.PercentComplete * num / 100;
				if (base.PercentComplete < 100 && percentComplete == num)
				{
					percentComplete--;
				}
				object[] objArray = new object[3];
				objArray[0] = str;
				objArray[1] = new string('o', percentComplete);
				objArray[2] = new string(' ', num - percentComplete);
				strCollection.Add(StringUtil.TruncateToBufferCellWidth(rawUI, StringUtil.Format(" {0}[{1}{2}] ", objArray), maxWidth));
			}
			if (base.SecondsRemaining >= 0)
			{
				TimeSpan timeSpan = new TimeSpan(0, 0, base.SecondsRemaining);
				strCollection.Add(StringUtil.TruncateToBufferCellWidth(rawUI, string.Concat(" ", StringUtil.Format(ProgressNodeStrings.SecondsRemaining, str, timeSpan), " "), maxWidth));
			}
			if (!string.IsNullOrEmpty(base.CurrentOperation))
			{
				strCollection.Add(" ");
				ProgressNode.RenderFullDescription(base.CurrentOperation, str, maxWidth, rawUI, strCollection, isFullPlus);
			}
		}

		private static void RenderFullDescription(string description, string indent, int maxWidth, PSHostRawUserInterface rawUi, ArrayList strCollection, bool isFullPlus)
		{
			string str = StringUtil.Format(" {0}{1} ", indent, description);
			do
			{
				string bufferCellWidth = StringUtil.TruncateToBufferCellWidth(rawUi, str, maxWidth);
				strCollection.Add(bufferCellWidth);
				if (str.Length != bufferCellWidth.Length)
				{
					str = StringUtil.Format(" {0}{1}", indent, str.Substring(bufferCellWidth.Length));
				}
				else
				{
					return;
				}
			}
			while (isFullPlus);
		}

		private void RenderMinimal(ArrayList strCollection, int indentation, int maxWidth, PSHostRawUserInterface rawUI)
		{
			string str = new string(' ', indentation);
			string str1 = "";
			if (base.PercentComplete >= 0)
			{
				str1 = StringUtil.Format("{0}% ", base.PercentComplete);
			}
			string str2 = "";
			if (base.SecondsRemaining >= 0)
			{
				TimeSpan timeSpan = new TimeSpan(0, 0, base.SecondsRemaining);
				str2 = string.Concat(timeSpan.ToString(), " ");
			}
			object[] activity = new object[5];
			activity[0] = str;
			activity[1] = base.Activity;
			activity[2] = str1;
			activity[3] = str2;
			activity[4] = base.StatusDescription;
			strCollection.Add(StringUtil.TruncateToBufferCellWidth(rawUI, StringUtil.Format(" {0}{1} {2}{3}{4} ", activity), maxWidth));
		}

		internal enum RenderStyle
		{
			Invisible,
			Minimal,
			Compact,
			Full,
			FullPlus
		}
	}
}