namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    internal class Pipe
    {
        private ExecutionContext _context;
        private CommandProcessorBase _downstreamCmdlet;
        private IEnumerator _enumeratorToProcess;
        private bool _enumeratorToProcessIsEmpty;
        private List<IList> _errorVariableList;
        private PipelineWriter _externalWriter;
        private bool _isRedirected;
        private bool _nullPipe;
        private Queue _objectQueue;
        private PipelineReader<object> _objectReader;
        private int _outBufferCount;
        private System.Management.Automation.Internal.PipelineProcessor _outputPipeline;
        private List<IList> _outVariableList;
        private ArrayList _resultList;
        private List<IList> _warningVariableList;

        internal Pipe()
        {
            this._objectQueue = new Queue();
        }

        internal Pipe(ArrayList resultList)
        {
            this._isRedirected = true;
            this._resultList = resultList;
        }

        internal Pipe(IEnumerator enumeratorToProcess)
        {
            this._enumeratorToProcess = enumeratorToProcess;
            this._enumeratorToProcessIsEmpty = false;
        }

        internal Pipe(ExecutionContext context, System.Management.Automation.Internal.PipelineProcessor outputPipeline)
        {
            this._isRedirected = true;
            this._context = context;
            this._outputPipeline = outputPipeline;
        }

        internal void Add(object obj)
        {
            if (obj != AutomationNull.Value)
            {
                AddToVarList(this._outVariableList, obj);
                if (!this._nullPipe)
                {
                    this.AddToPipe(obj);
                }
            }
        }

        internal void AddItems(object objects)
        {
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(objects);
            try
            {
                object obj2;
                if (enumerator != null)
                {
                    goto Label_002A;
                }
                this.Add(objects);
                goto Label_0054;
            Label_0013:
                obj2 = ParserOps.Current(null, enumerator);
                if (obj2 != AutomationNull.Value)
                {
                    this.Add(obj2);
                }
            Label_002A:
                if (ParserOps.MoveNext(this._context, null, enumerator))
                {
                    goto Label_0013;
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if ((disposable != null) && !(objects is IEnumerator))
                {
                    disposable.Dispose();
                }
            }
        Label_0054:
            if (this._externalWriter != null)
            {
                return;
            }
            if (((this._downstreamCmdlet != null) && (this._objectQueue != null)) && (this._objectQueue.Count > this._outBufferCount))
            {
                this._downstreamCmdlet.DoExecute();
            }
        }

        private void AddToPipe(object obj)
        {
            if (this._outputPipeline != null)
            {
                this._context.PushPipelineProcessor(this._outputPipeline);
                this._outputPipeline.Step(obj);
                this._context.PopPipelineProcessor(false);
            }
            else if (this._resultList != null)
            {
                this._resultList.Add(obj);
            }
            else if (this._externalWriter != null)
            {
                this._externalWriter.Write(obj);
            }
            else if (this._objectQueue != null)
            {
                this._objectQueue.Enqueue(obj);
                if ((this._downstreamCmdlet != null) && (this._objectQueue.Count > this._outBufferCount))
                {
                    this._downstreamCmdlet.DoExecute();
                }
            }
        }

        private static void AddToVarList(List<IList> varList, object obj)
        {
            if (varList != null)
            {
                foreach (IList list in varList)
                {
                    list.Add(obj);
                }
            }
        }

        internal void AddVariableList(VariableStreamKind kind, IList list)
        {
            switch (kind)
            {
                case VariableStreamKind.Output:
                    if (this._outVariableList == null)
                    {
                        this._outVariableList = new List<IList>();
                    }
                    this._outVariableList.Add(list);
                    return;

                case VariableStreamKind.Error:
                    if (this._errorVariableList == null)
                    {
                        this._errorVariableList = new List<IList>();
                    }
                    this._errorVariableList.Add(list);
                    return;

                case VariableStreamKind.Warning:
                    if (this._warningVariableList == null)
                    {
                        this._warningVariableList = new List<IList>();
                    }
                    this._warningVariableList.Add(list);
                    return;
            }
        }

        internal void AddWithoutAppendingOutVarList(object obj)
        {
            if ((obj != AutomationNull.Value) && !this._nullPipe)
            {
                this.AddToPipe(obj);
            }
        }

        internal void AppendVariableList(VariableStreamKind kind, object obj)
        {
            switch (kind)
            {
                case VariableStreamKind.Output:
                    AddToVarList(this._outVariableList, obj);
                    return;

                case VariableStreamKind.Error:
                    AddToVarList(this._errorVariableList, obj);
                    return;

                case VariableStreamKind.Warning:
                    AddToVarList(this._warningVariableList, obj);
                    return;
            }
        }

        internal void Clear()
        {
            if (this._objectQueue != null)
            {
                this._objectQueue.Clear();
            }
        }

        internal void RemoveVariableList(VariableStreamKind kind, IList list)
        {
            switch (kind)
            {
                case VariableStreamKind.Output:
                    this._outVariableList.Remove(list);
                    return;

                case VariableStreamKind.Error:
                    this._errorVariableList.Remove(list);
                    return;

                case VariableStreamKind.Warning:
                    this._warningVariableList.Remove(list);
                    return;
            }
        }

        internal object Retrieve()
        {
            if ((this._objectQueue != null) && (this._objectQueue.Count != 0))
            {
                return this._objectQueue.Dequeue();
            }
            if (this._enumeratorToProcess != null)
            {
                if (this._enumeratorToProcessIsEmpty)
                {
                    return AutomationNull.Value;
                }
                if (!ParserOps.MoveNext(this._context, null, this._enumeratorToProcess))
                {
                    this._enumeratorToProcessIsEmpty = true;
                    return AutomationNull.Value;
                }
                return ParserOps.Current(null, this._enumeratorToProcess);
            }
            if (this.ExternalReader != null)
            {
                try
                {
                    object obj2 = this.ExternalReader.Read();
                    if (AutomationNull.Value == obj2)
                    {
                        this.ExternalReader = null;
                    }
                    return obj2;
                }
                catch (PipelineClosedException)
                {
                    return AutomationNull.Value;
                }
                catch (ObjectDisposedException)
                {
                    return AutomationNull.Value;
                }
            }
            return AutomationNull.Value;
        }

        internal object[] ToArray()
        {
            if ((this._objectQueue != null) && (this._objectQueue.Count != 0))
            {
                return this._objectQueue.ToArray();
            }
            return MshCommandRuntime.StaticEmptyArray;
        }

        public override string ToString()
        {
            if (this._downstreamCmdlet != null)
            {
                return this._downstreamCmdlet.ToString();
            }
            return base.ToString();
        }

        internal CommandProcessorBase DownstreamCmdlet
        {
            get
            {
                return this._downstreamCmdlet;
            }
            set
            {
                this._downstreamCmdlet = value;
            }
        }

        internal bool Empty
        {
            get
            {
                if (this._enumeratorToProcess != null)
                {
                    return this._enumeratorToProcessIsEmpty;
                }
                if (this._objectQueue != null)
                {
                    return (this._objectQueue.Count == 0);
                }
                return true;
            }
        }

        internal PipelineReader<object> ExternalReader
        {
            get
            {
                return this._objectReader;
            }
            set
            {
                this._objectReader = value;
            }
        }

        internal PipelineWriter ExternalWriter
        {
            get
            {
                return this._externalWriter;
            }
            set
            {
                this._externalWriter = value;
            }
        }

        internal bool IsRedirected
        {
            get
            {
                if (this._downstreamCmdlet == null)
                {
                    return this._isRedirected;
                }
                return true;
            }
        }

        internal bool NullPipe
        {
            get
            {
                return this._nullPipe;
            }
            set
            {
                this._isRedirected = true;
                this._nullPipe = value;
            }
        }

        internal Queue ObjectQueue
        {
            get
            {
                return this._objectQueue;
            }
        }

        internal int OutBufferCount
        {
            get
            {
                return this._outBufferCount;
            }
            set
            {
                this._outBufferCount = value;
            }
        }

        internal System.Management.Automation.Internal.PipelineProcessor PipelineProcessor
        {
            get
            {
                return this._outputPipeline;
            }
        }
    }
}

