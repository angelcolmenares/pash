using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace Microsoft.PowerShell
{
	internal class Executor
	{
		private static Executor currentExecutor;

		private static object staticStateLock;

		private ConsoleHost parent;

		private Pipeline pipeline;

		private bool cancelled;

		internal bool useNestedPipelines;

		private object instanceStateLock;

		private bool isPromptFunctionExecutor;

		internal static Executor CurrentExecutor
		{
			get
			{
				Executor executor = null;
				lock (Executor.staticStateLock)
				{
				}
				return executor;
			}
			set
			{
				lock (Executor.staticStateLock)
				{
					Executor.currentExecutor = value;
				}
			}
		}

		static Executor()
		{
			Executor.staticStateLock = new object();
		}

		internal Executor(ConsoleHost parent, bool useNestedPipelines, bool isPromptFunctionExecutor)
		{
			this.instanceStateLock = new object();
			this.parent = parent;
			this.useNestedPipelines = useNestedPipelines;
			this.isPromptFunctionExecutor = isPromptFunctionExecutor;
			this.Reset();
		}

		private void AsyncPipelineFailureHandler(Exception ex)
		{
			ErrorRecord errorRecord = null;
			IContainsErrorRecord containsErrorRecord = ex as IContainsErrorRecord;
			if (containsErrorRecord != null)
			{
				errorRecord = containsErrorRecord.ErrorRecord;
				errorRecord = new ErrorRecord(errorRecord, ex);
			}
			if (errorRecord == null)
			{
				errorRecord = new ErrorRecord(ex, "ConsoleHostAsyncPipelineFailure", ErrorCategory.NotSpecified, null);
			}
			this.parent.ErrorSerializer.Serialize(errorRecord);
		}

		private void Cancel()
		{
			lock (this.instanceStateLock)
			{
				if (this.pipeline != null && !this.cancelled)
				{
					this.cancelled = true;
					if (this.isPromptFunctionExecutor)
					{
						Thread.Sleep(100);
					}
					this.pipeline.Stop();
				}
			}
		}

		internal static void CancelCurrentExecutor()
		{
			Executor executor = null;
			lock (Executor.staticStateLock)
			{
			}
			if (executor != null)
			{
				executor.Cancel();
			}
		}

		internal Pipeline CreatePipeline()
		{
			if (!this.useNestedPipelines)
			{
				return this.parent.RunspaceRef.CreatePipeline();
			}
			else
			{
				return this.parent.RunspaceRef.CreateNestedPipeline();
			}
		}

		internal Pipeline CreatePipeline(string command, bool addToHistory)
		{
			return this.parent.RunspaceRef.CreatePipeline(command, addToHistory, this.useNestedPipelines);
		}

		private void ErrorObjectStreamHandler(object sender, EventArgs e)
		{
			PipelineReader<object> pipelineReader = (PipelineReader<object>)sender;
			Collection<object> objs = pipelineReader.NonBlockingRead();
			foreach (object obj in objs)
			{
				this.parent.ErrorSerializer.Serialize(obj);
			}
		}

		internal Collection<PSObject> ExecuteCommand(string command, out Exception exceptionThrown, Executor.ExecutionOptions options)
		{
			Pipeline pipeline = this.CreatePipeline(command, (options & Executor.ExecutionOptions.AddToHistory) > Executor.ExecutionOptions.None);
			return this.ExecuteCommandHelper(pipeline, out exceptionThrown, options);
		}

		internal Collection<PSObject> ExecuteCommand(string command)
		{
			Exception exception = null;
			Collection<PSObject> pSObjects = this.ExecuteCommand(command, out exception, Executor.ExecutionOptions.None);
			if (exception == null)
			{
			}
			return pSObjects;
		}

		internal bool? ExecuteCommandAndGetResultAsBool(string command)
		{
			Exception exception = null;
			bool? nullable = this.ExecuteCommandAndGetResultAsBool(command, out exception);
			return nullable;
		}

		internal bool? ExecuteCommandAndGetResultAsBool(string command, out Exception exceptionThrown)
		{
			bool flag;
			exceptionThrown = null;
			bool? nullable = null;
			Collection<PSObject> pSObjects = this.ExecuteCommand(command, out exceptionThrown, Executor.ExecutionOptions.None);
			if (exceptionThrown == null && pSObjects != null && pSObjects.Count != 0)
			{
				bool? nullablePointer = nullable;
				if (pSObjects.Count > 1)
				{
					flag = true;
				}
				else
				{
					flag = LanguagePrimitives.IsTrue(pSObjects[0]);
				}
				nullable = new bool?(flag);
			}
			return nullable;
		}

		internal string ExecuteCommandAndGetResultAsString(string command, out Exception exceptionThrown)
		{
			exceptionThrown = null;
			string str = null;
			Collection<PSObject> pSObjects = this.ExecuteCommand(command, out exceptionThrown, Executor.ExecutionOptions.None);
			if (exceptionThrown == null && pSObjects != null && pSObjects.Count != 0)
			{
				if (pSObjects[0] != null)
				{
					PSObject item = pSObjects[0];
					if (item == null)
					{
						str = pSObjects[0].ToString();
					}
					else
					{
						str = item.BaseObject.ToString();
					}
				}
				else
				{
					return string.Empty;
				}
			}
			return str;
		}

		internal void ExecuteCommandAsync(string command, out Exception exceptionThrown, Executor.ExecutionOptions options)
		{
			bool flag = (options & Executor.ExecutionOptions.AddToHistory) > Executor.ExecutionOptions.None;
			Pipeline pipeline = this.parent.RunspaceRef.CreatePipeline(command, flag, false);
			this.ExecuteCommandAsyncHelper(pipeline, out exceptionThrown, options);
		}

		internal void ExecuteCommandAsyncHelper(Pipeline tempPipeline, out Exception exceptionThrown, Executor.ExecutionOptions options)
		{
			exceptionThrown = null;
			Executor currentExecutor = Executor.CurrentExecutor;
			Executor.CurrentExecutor = this;
			lock (this.instanceStateLock)
			{
				this.pipeline = tempPipeline;
			}
			try
			{
				try
				{
					if ((options & Executor.ExecutionOptions.AddOutputter) > Executor.ExecutionOptions.None && this.parent.OutputFormat == Serialization.DataFormat.Text)
					{
						if (tempPipeline.Commands.Count == 1)
						{
							tempPipeline.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
						}
						Command command = new Command("Out-Default", false, true);
						tempPipeline.Commands.Add(command);
					}
					tempPipeline.Output.DataReady += new EventHandler(this.OutputObjectStreamHandler);
					tempPipeline.Error.DataReady += new EventHandler(this.ErrorObjectStreamHandler);
					Executor.PipelineFinishedWaitHandle pipelineFinishedWaitHandle = new Executor.PipelineFinishedWaitHandle(tempPipeline);
					tempPipeline.InvokeAsync();
					if ((options & Executor.ExecutionOptions.ReadInputObjects) > Executor.ExecutionOptions.None && this.parent.IsStandardInputRedirected)
					{
						WrappedDeserializer wrappedDeserializer = new WrappedDeserializer(this.parent.InputFormat, "Input", this.parent.StandardInReader);
						while (!wrappedDeserializer.AtEnd)
						{
							object obj = wrappedDeserializer.Deserialize();
							if (obj == null)
							{
								break;
							}
							try
							{
								tempPipeline.Input.Write(obj);
							}
							catch (PipelineClosedException pipelineClosedException)
							{
								break;
							}
						}
						wrappedDeserializer.End();
					}
					tempPipeline.Input.Close();
					pipelineFinishedWaitHandle.Wait();
					if (tempPipeline.PipelineStateInfo.State == PipelineState.Failed && tempPipeline.PipelineStateInfo.Reason != null)
					{
						if (this.parent.OutputFormat != Serialization.DataFormat.Text)
						{
							this.AsyncPipelineFailureHandler(tempPipeline.PipelineStateInfo.Reason);
						}
						else
						{
							exceptionThrown = tempPipeline.PipelineStateInfo.Reason;
						}
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ConsoleHost.CheckForSevereException(exception);
					exceptionThrown = exception;
				}
			}
			finally
			{
				this.parent.ui.ResetProgress();
				Executor.CurrentExecutor = currentExecutor;
				this.Reset();
			}
		}

		internal Collection<PSObject> ExecuteCommandHelper(Pipeline tempPipeline, out Exception exceptionThrown, Executor.ExecutionOptions options)
		{
			exceptionThrown = null;
			Collection<PSObject> pSObjects = null;
			if ((options & Executor.ExecutionOptions.AddOutputter) > Executor.ExecutionOptions.None)
			{
				if (tempPipeline.Commands.Count == 1)
				{
					tempPipeline.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
				}
				Command command = new Command("Out-Default", false, new bool?(true), true);
				tempPipeline.Commands.Add(command);
			}
			Executor currentExecutor = Executor.CurrentExecutor;
			Executor.CurrentExecutor = this;
			lock (this.instanceStateLock)
			{
				this.pipeline = tempPipeline;
			}
			try
			{
				try
				{
					pSObjects = tempPipeline.Invoke();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ConsoleHost.CheckForSevereException(exception);
					exceptionThrown = exception;
				}
			}
			finally
			{
				this.parent.ui.ResetProgress();
				Executor.CurrentExecutor = currentExecutor;
				this.Reset();
			}
			return pSObjects;
		}

		private void OutputObjectStreamHandler(object sender, EventArgs e)
		{
			PipelineReader<PSObject> pipelineReader = (PipelineReader<PSObject>)sender;
			Collection<PSObject> pSObjects = pipelineReader.NonBlockingRead();
			foreach (PSObject pSObject in pSObjects)
			{
				this.parent.OutputSerializer.Serialize(pSObject);
			}
		}

		private void Reset()
		{
			lock (this.instanceStateLock)
			{
				this.pipeline = null;
				this.cancelled = false;
			}
		}

		[Flags]
		internal enum ExecutionOptions
		{
			None = 0,
			AddOutputter = 1,
			AddToHistory = 2,
			ReadInputObjects = 4
		}

		private class PipelineFinishedWaitHandle
		{
			private ManualResetEvent eventHandle;

			internal PipelineFinishedWaitHandle(Pipeline p)
			{
				this.eventHandle = new ManualResetEvent(false);
				p.StateChanged += new EventHandler<PipelineStateEventArgs>(this.PipelineStateChangedHandler);
			}

			private void PipelineStateChangedHandler(object sender, PipelineStateEventArgs e)
			{
				if (e.PipelineStateInfo.State == PipelineState.Completed || e.PipelineStateInfo.State == PipelineState.Failed || e.PipelineStateInfo.State == PipelineState.Stopped)
				{
					this.eventHandle.Set();
				}
			}

			internal void Wait()
			{
				this.eventHandle.WaitOne();
			}
		}
	}
}