using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal class CmdletSubroutinePipeline : DelegatePipeline
	{
		private const string _debugCategory = "CmdletSubroutinePipeline";

		protected override string DelegateMethodSuffix
		{
			get
			{
				return "CSRoutine";
			}
		}

		public CmdletSubroutinePipeline() : base(typeof(CmdletSubroutine))
		{
		}

		public void InsertAfter(CmdletSubroutine referenceDelegate, CmdletSubroutine newDelegate)
		{
			base.InsertAfter(referenceDelegate, newDelegate);
		}

		public void InsertAtEnd(CmdletSubroutine newDelegate)
		{
			base.InsertAtEnd(newDelegate);
		}

		public void InsertAtStart(CmdletSubroutine newDelegate)
		{
			base.InsertAtStart(newDelegate);
		}

		public void InsertBefore(CmdletSubroutine referenceDelegate, CmdletSubroutine newDelegate)
		{
			base.InsertBefore(referenceDelegate, newDelegate);
		}

		public void Invoke()
		{
			if (base.GetDelegate() == null || (int)base.GetDelegate().GetInvocationList().Length == 0)
			{
				DebugLogger.LogInfo("CmdletSubroutinePipeline", "No CmdletSubroutine delegates found");
				return;
			}
			else
			{
				Delegate[] invocationList = base.GetDelegate().GetInvocationList();
				int num = 0;
				while (num < (int)invocationList.Length)
				{
					Delegate @delegate = invocationList[num];
					object[] method = new object[4];
					method[0] = "Invoking Method: ";
					method[1] = @delegate.Method;
					method[2] = " on Target: ";
					method[3] = @delegate.Target;
					DebugLogger.LogInfo("CmdletSubroutinePipeline", string.Concat(method));
					bool flag = ((CmdletSubroutine)@delegate)();
					object[] target = new object[4];
					target[0] = "Exiting Method: ";
					target[1] = @delegate.Method;
					target[2] = " on Target: ";
					target[3] = @delegate.Target;
					DebugLogger.LogInfo("CmdletSubroutinePipeline", string.Concat(target));
					if (flag)
					{
						num++;
					}
					else
					{
						object[] objArray = new object[5];
						objArray[0] = "STOPPING PIPELINE. Method: ";
						objArray[1] = @delegate.Method;
						objArray[2] = " on Target: ";
						objArray[3] = @delegate.Target;
						objArray[4] = " returned ShouldContinue=FALSE";
						DebugLogger.LogInfo("CmdletSubroutinePipeline", string.Concat(objArray));
						return;
					}
				}
				return;
			}
		}

		public void Remove(CmdletSubroutine existingDelegate)
		{
			base.Remove(existingDelegate);
		}

		public void Replace(CmdletSubroutine existingDelegate, CmdletSubroutine newDelegate)
		{
			base.Replace(existingDelegate, newDelegate);
		}
	}
}