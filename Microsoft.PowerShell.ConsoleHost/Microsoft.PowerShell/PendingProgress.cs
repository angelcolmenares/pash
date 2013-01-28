using System;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell
{
	internal class PendingProgress
	{
		private const int maxNodeCount = 128;

		private ArrayList topLevelNodes;

		private int nodeCount;

		[TraceSource("PendingProgress", "Console Host Progress")]
		private static PSTraceSource tracer;

		static PendingProgress()
		{
			PendingProgress.tracer = PSTraceSource.GetTracer("PendingProgress", "Console Host Progress");
		}

		public PendingProgress()
		{
			this.topLevelNodes = new ArrayList();
		}

		private void AddNode(ArrayList nodes, ProgressNode nodeToAdd)
		{
			nodes.Add(nodeToAdd);
			PendingProgress pendingProgress = this;
			pendingProgress.nodeCount = pendingProgress.nodeCount + 1;
		}

		private void AgeNodesAndResetStyle()
		{
			PendingProgress.AgeAndResetStyleVisitor ageAndResetStyleVisitor = new PendingProgress.AgeAndResetStyleVisitor();
			PendingProgress.NodeVisitor.VisitNodes(this.topLevelNodes, ageAndResetStyleVisitor);
		}

		private int CompressToFit(PSHostRawUserInterface rawUi, int maxHeight, int maxWidth)
		{
			int num = 0;
			if (!this.CompressToFitHelper(rawUi, maxHeight, maxWidth, out num, ProgressNode.RenderStyle.FullPlus, ProgressNode.RenderStyle.Full))
			{
				if (!this.CompressToFitHelper(rawUi, maxHeight, maxWidth, out num, ProgressNode.RenderStyle.Full, ProgressNode.RenderStyle.Compact))
				{
					if (!this.CompressToFitHelper(rawUi, maxHeight, maxWidth, out num, ProgressNode.RenderStyle.Compact, ProgressNode.RenderStyle.Minimal))
					{
						if (!this.CompressToFitHelper(rawUi, maxHeight, maxWidth, out num, ProgressNode.RenderStyle.Minimal, ProgressNode.RenderStyle.Invisible))
						{
							return 0;
						}
						else
						{
							return num;
						}
					}
					else
					{
						return 0;
					}
				}
				else
				{
					return 0;
				}
			}
			else
			{
				return 0;
			}
		}

		private bool CompressToFitHelper(PSHostRawUserInterface rawUi, int maxHeight, int maxWidth, out int nodesCompressed, ProgressNode.RenderStyle priorStyle, ProgressNode.RenderStyle newStyle)
		{
			nodesCompressed = 0;
			int num = 0;
			do
			{
				ProgressNode progressNode = this.FindOldestNodeOfGivenStyle(this.topLevelNodes, num, priorStyle);
				if (progressNode == null)
				{
					return false;
				}
				else
				{
					progressNode.Style = newStyle;
					nodesCompressed = nodesCompressed + 1;
				}
			}
			while (this.TallyHeight(rawUi, maxHeight, maxWidth) > maxHeight);
			return true;
		}

		private void EvictNode()
		{
			ArrayList arrayLists = null;
			int num = -1;
			ProgressNode progressNode = this.FindOldestLeafmostNode(out arrayLists, out num);
			if (progressNode != null)
			{
				this.RemoveNode(arrayLists, num);
				return;
			}
			else
			{
				this.RemoveNode(this.topLevelNodes, 0);
				return;
			}
		}

		private ProgressNode FindNodeById(long sourceId, int activityId)
		{
			ArrayList arrayLists = null;
			int num = -1;
			return this.FindNodeById(sourceId, activityId, out arrayLists, out num);
		}

		private ProgressNode FindNodeById(long sourceId, int activityId, out ArrayList listWhereFound, out int indexWhereFound)
		{
			listWhereFound = null;
			indexWhereFound = -1;
			PendingProgress.FindByIdNodeVisitor findByIdNodeVisitor = new PendingProgress.FindByIdNodeVisitor(sourceId, activityId);
			PendingProgress.NodeVisitor.VisitNodes(this.topLevelNodes, findByIdNodeVisitor);
			listWhereFound = findByIdNodeVisitor.ListWhereFound;
			indexWhereFound = findByIdNodeVisitor.IndexWhereFound;
			return findByIdNodeVisitor.FoundNode;
		}

		private ProgressNode FindOldestLeafmostNode(out ArrayList listWhereFound, out int indexWhereFound)
		{
			ProgressNode progressNode;
			listWhereFound = null;
			indexWhereFound = -1;
			ArrayList children = this.topLevelNodes;
			while (true)
			{
				progressNode = this.FindOldestLeafmostNodeHelper(children, out listWhereFound, out indexWhereFound);
				if (progressNode == null || progressNode.Children == null || progressNode.Children.Count == 0)
				{
					break;
				}
				children = progressNode.Children;
			}
			return progressNode;
		}

		private ProgressNode FindOldestLeafmostNodeHelper(ArrayList treeToSearch, out ArrayList listWhereFound, out int indexWhereFound)
		{
			listWhereFound = null;
			indexWhereFound = -1;
			PendingProgress.FindOldestNodeVisitor findOldestNodeVisitor = new PendingProgress.FindOldestNodeVisitor();
			PendingProgress.NodeVisitor.VisitNodes(treeToSearch, findOldestNodeVisitor);
			listWhereFound = findOldestNodeVisitor.ListWhereFound;
			indexWhereFound = findOldestNodeVisitor.IndexWhereFound;
			return findOldestNodeVisitor.FoundNode;
		}

		private ProgressNode FindOldestNodeOfGivenStyle(ArrayList nodes, int oldestSoFar, ProgressNode.RenderStyle style)
		{
			if (nodes != null)
			{
				ProgressNode progressNode = null;
				for (int i = 0; i < nodes.Count; i++)
				{
					ProgressNode item = (ProgressNode)nodes[i];
					if (item.Age >= oldestSoFar && item.Style == style)
					{
						progressNode = item;
						oldestSoFar = progressNode.Age;
					}
					if (item.Children != null)
					{
						ProgressNode progressNode1 = this.FindOldestNodeOfGivenStyle(item.Children, oldestSoFar, style);
						if (progressNode1 != null)
						{
							progressNode = progressNode1;
							oldestSoFar = progressNode.Age;
						}
					}
				}
				return progressNode;
			}
			else
			{
				return null;
			}
		}

		private void RemoveNode(ArrayList nodes, int indexToRemove)
		{
			nodes.RemoveAt(indexToRemove);
			PendingProgress pendingProgress = this;
			pendingProgress.nodeCount = pendingProgress.nodeCount - 1;
		}

		private void RemoveNodeAndPromoteChildren(ArrayList nodes, int indexToRemove)
		{
			ProgressNode item = (ProgressNode)nodes[indexToRemove];
			if (item != null)
			{
				if (item.Children == null)
				{
					this.RemoveNode(nodes, indexToRemove);
					return;
				}
				else
				{
					for (int i = 0; i < item.Children.Count; i++)
					{
						((ProgressNode)item.Children[i]).ParentActivityId = -1;
					}
					nodes.RemoveAt(indexToRemove);
					PendingProgress pendingProgress = this;
					pendingProgress.nodeCount = pendingProgress.nodeCount - 1;
					nodes.InsertRange(indexToRemove, item.Children);
					return;
				}
			}
			else
			{
				return;
			}
		}

		internal string[] Render(int maxWidth, int maxHeight, PSHostRawUserInterface rawUI)
		{
			if (this.topLevelNodes == null || this.topLevelNodes.Count <= 0)
			{
				return null;
			}
			else
			{
				int fit = 0;
				if (this.TallyHeight(rawUI, maxHeight, maxWidth) > maxHeight)
				{
					fit = this.CompressToFit(rawUI, maxHeight, maxWidth);
				}
				ArrayList arrayLists = new ArrayList();
				string str = new string(' ', maxWidth);
				arrayLists.Add(str);
				this.RenderHelper(arrayLists, this.topLevelNodes, 0, maxWidth, rawUI);
				if (fit != 1)
				{
					if (fit > 1)
					{
						arrayLists.Add(string.Concat(" ", StringUtil.Format(ProgressNodeStrings.InvisibleNodesMessagePlural, fit)));
					}
				}
				else
				{
					arrayLists.Add(string.Concat(" ", StringUtil.Format(ProgressNodeStrings.InvisibleNodesMessageSingular, fit)));
				}
				arrayLists.Add(str);
				return (string[])arrayLists.ToArray(typeof(string));
			}
		}

		private void RenderHelper(ArrayList strings, ArrayList nodes, int indentation, int maxWidth, PSHostRawUserInterface rawUI)
		{
			int num;
			if (nodes != null)
			{
				foreach (ProgressNode node in nodes)
				{
					int count = strings.Count;
					node.Render(strings, indentation, maxWidth, rawUI);
					if (node.Children == null)
					{
						continue;
					}
					if (strings.Count > count)
					{
						num = 2;
					}
					else
					{
						num = 0;
					}
					int num1 = num;
					this.RenderHelper(strings, node.Children, indentation + num1, maxWidth, rawUI);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private int TallyHeight(PSHostRawUserInterface rawUi, int maxHeight, int maxWidth)
		{
			PendingProgress.HeightTallyer heightTallyer = new PendingProgress.HeightTallyer(rawUi, maxHeight, maxWidth);
			PendingProgress.NodeVisitor.VisitNodes(this.topLevelNodes, heightTallyer);
			return heightTallyer.Tally;
		}

		internal void Update(long sourceId, ProgressRecord record)
		{
			if (record.ParentActivityId != record.ActivityId)
			{
				ArrayList arrayLists = null;
				int num = -1;
				ProgressNode activity = this.FindNodeById(sourceId, record.ActivityId, out arrayLists, out num);
				if (activity != null)
				{
					if (record.RecordType != ProgressRecordType.Completed)
					{
						if (record.ParentActivityId == activity.ParentActivityId)
						{
							activity.Activity = record.Activity;
							activity.StatusDescription = record.StatusDescription;
							activity.CurrentOperation = record.CurrentOperation;
							activity.PercentComplete = Math.Min(record.PercentComplete, 100);
							activity.SecondsRemaining = record.SecondsRemaining;
							activity.Age = 0;
							this.AgeNodesAndResetStyle();
							return;
						}
						this.RemoveNodeAndPromoteChildren(arrayLists, num);
					}
					else
					{
						this.RemoveNodeAndPromoteChildren(arrayLists, num);
						this.AgeNodesAndResetStyle();
						return;
					}
				}
				if (record.RecordType != ProgressRecordType.Completed)
				{
					ProgressNode progressNode = new ProgressNode(sourceId, record);
					while (this.nodeCount >= 128)
					{
						this.EvictNode();
					}
					if (progressNode.ParentActivityId >= 0)
					{
						ProgressNode arrayLists1 = this.FindNodeById(progressNode.SourceId, progressNode.ParentActivityId);
						if (arrayLists1 != null)
						{
							if (arrayLists1.Children == null)
							{
								arrayLists1.Children = new ArrayList();
							}
							this.AddNode(arrayLists1.Children, progressNode);
							this.AgeNodesAndResetStyle();
							return;
						}
						progressNode.ParentActivityId = -1;
					}
					this.AddNode(this.topLevelNodes, progressNode);
				}
			}
			else
			{
				PendingProgress.tracer.WriteLine("Ignoring malformed record:", new object[0]);
				PendingProgress.tracer.WriteLine(record);
			}
			this.AgeNodesAndResetStyle();
		}

		private class AgeAndResetStyleVisitor : PendingProgress.NodeVisitor
		{
			public AgeAndResetStyleVisitor()
			{
			}

			internal override bool Visit(ProgressNode node, ArrayList unused, int unusedToo)
			{
				node.Age = Math.Min(node.Age + 1, 0x7ffffffe);
				node.Style = ProgressNode.RenderStyle.FullPlus;
				return true;
			}
		}

		private class FindByIdNodeVisitor : PendingProgress.NodeVisitor
		{
			internal ProgressNode FoundNode;

			internal ArrayList ListWhereFound;

			internal int IndexWhereFound;

			private int idToFind;

			private long sourceIdToFind;

			internal FindByIdNodeVisitor(long sourceIdToFind, int activityIdToFind)
			{
				this.IndexWhereFound = -1;
				this.idToFind = -1;
				this.sourceIdToFind = sourceIdToFind;
				this.idToFind = activityIdToFind;
			}

			internal override bool Visit(ProgressNode node, ArrayList listWhereFound, int indexWhereFound)
			{
				if (node.ActivityId != this.idToFind || node.SourceId != this.sourceIdToFind)
				{
					return true;
				}
				else
				{
					this.FoundNode = node;
					this.ListWhereFound = listWhereFound;
					this.IndexWhereFound = indexWhereFound;
					return false;
				}
			}
		}

		private class FindOldestNodeVisitor : PendingProgress.NodeVisitor
		{
			internal ProgressNode FoundNode;

			internal ArrayList ListWhereFound;

			internal int IndexWhereFound;

			private int oldestSoFar;

			public FindOldestNodeVisitor()
			{
				this.IndexWhereFound = -1;
			}

			internal override bool Visit(ProgressNode node, ArrayList listWhereFound, int indexWhereFound)
			{
				if (node.Age >= this.oldestSoFar)
				{
					this.oldestSoFar = node.Age;
					this.FoundNode = node;
					this.ListWhereFound = listWhereFound;
					this.IndexWhereFound = indexWhereFound;
				}
				return true;
			}
		}

		private class HeightTallyer : PendingProgress.NodeVisitor
		{
			private PSHostRawUserInterface rawUi;

			private int maxHeight;

			private int maxWidth;

			internal int Tally;

			internal HeightTallyer(PSHostRawUserInterface rawUi, int maxHeight, int maxWidth)
			{
				this.rawUi = rawUi;
				this.maxHeight = maxHeight;
				this.maxWidth = maxWidth;
			}

			internal override bool Visit(ProgressNode node, ArrayList unused, int unusedToo)
			{
				PendingProgress.HeightTallyer tally = this;
				tally.Tally = tally.Tally + node.LinesRequiredMethod(this.rawUi, this.maxWidth);
				if (this.Tally <= this.maxHeight)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		private abstract class NodeVisitor
		{
			protected NodeVisitor()
			{
			}

			internal abstract bool Visit(ProgressNode node, ArrayList listWhereFound, int indexWhereFound);

			internal static void VisitNodes(ArrayList nodes, PendingProgress.NodeVisitor v)
			{
				if (nodes != null)
				{
					int num = 0;
					while (num < nodes.Count)
					{
						ProgressNode item = (ProgressNode)nodes[num];
						if (v.Visit(item, nodes, num))
						{
							if (item.Children != null)
							{
								PendingProgress.NodeVisitor.VisitNodes(item.Children, v);
							}
							num++;
						}
						else
						{
							return;
						}
					}
					return;
				}
				else
				{
					return;
				}
			}
		}
	}
}