namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Threading;

    internal class History
    {
        private HistoryInfo[] _buffer;
        private int _capacity;
        private long _countEntriesAdded;
        private int _countEntriesInBuffer;
        private object _syncRoot = new object();
        internal const int DefaultHistorySize = 0x1000;

        internal History(System.Management.Automation.ExecutionContext context)
        {
            Collection<Attribute> attributes = new Collection<Attribute> {
                new ValidateRangeAttribute(1, 0x7fff)
            };
            PSVariable variable = new PSVariable("MaximumHistoryCount", 0x1000, ScopedItemOptions.None, attributes) {
                Description = SessionStateStrings.MaxHistoryCountDescription
            };
            context.EngineSessionState.SetVariable(variable, false, CommandOrigin.Internal);
            this._capacity = 0x1000;
            this._buffer = new HistoryInfo[this._capacity];
        }

        private long Add(HistoryInfo entry)
        {
            if (entry == null)
            {
                throw PSTraceSource.NewArgumentNullException("entry");
            }
            this._buffer[this.GetIndexForNewEntry()] = entry;
            this._countEntriesAdded += 1L;
            entry.SetId(this._countEntriesAdded);
            this.IncrementCountOfEntriesInBuffer();
            return this._countEntriesAdded;
        }

        internal long AddEntry(long pipelineId, string cmdline, PipelineState status, DateTime startTime, DateTime endTime, bool skipIfLocked)
        {
            long num;
            if (!Monitor.TryEnter(this._syncRoot, skipIfLocked ? 0 : -1))
            {
                return -1L;
            }
            try
            {
                this.ReallocateBufferIfNeeded();
                HistoryInfo entry = new HistoryInfo(pipelineId, cmdline, status, startTime, endTime);
                num = this.Add(entry);
            }
            finally
            {
                Monitor.Exit(this._syncRoot);
            }
            return num;
        }

        internal int Buffercapacity()
        {
            return this._capacity;
        }

        internal void ClearEntry(long id)
        {
            lock (this._syncRoot)
            {
                if (id < 0L)
                {
                    throw PSTraceSource.NewArgumentOutOfRangeException("id", id);
                }
                if ((this._countEntriesInBuffer != 0) && (id <= this._countEntriesAdded))
                {
                    HistoryInfo info = this.CoreGetEntry(id);
                    if (info != null)
                    {
                        info.Cleared = true;
                        this._countEntriesInBuffer--;
                    }
                }
            }
        }

        private HistoryInfo CoreGetEntry(long id)
        {
            if (id <= 0L)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("id", id);
            }
            if (this._countEntriesInBuffer == 0)
            {
                return null;
            }
            if (id > this._countEntriesAdded)
            {
                return null;
            }
            return this._buffer[this.GetIndexFromId(id)];
        }

        internal HistoryInfo[] GetEntries(long id, long count, SwitchParameter newest)
        {
            this.ReallocateBufferIfNeeded();
            if (count < -1L)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("count", count);
            }
            if (newest.ToString() == null)
            {
                throw PSTraceSource.NewArgumentNullException("newest");
            }
            if (((count == -1L) || (count > this._countEntriesAdded)) || (count > this._countEntriesInBuffer))
            {
                count = this._countEntriesInBuffer;
            }
            if ((count == 0L) || (this._countEntriesInBuffer == 0))
            {
                return new HistoryInfo[0];
            }
            lock (this._syncRoot)
            {
                ArrayList list = new ArrayList();
                if (id > 0L)
                {
                    long num;
                    long num2 = id;
                    if (!newest.IsPresent)
                    {
                        num = (num2 - count) + 1L;
                        if (num < 1L)
                        {
                            num = 1L;
                        }
                        for (long i = num2; i >= num; i -= 1L)
                        {
                            if (num <= 1L)
                            {
                                break;
                            }
                            if ((this._buffer[this.GetIndexFromId(i)] != null) && this._buffer[this.GetIndexFromId(i)].Cleared)
                            {
                                num -= 1L;
                            }
                        }
                        for (long j = num; j <= num2; j += 1L)
                        {
                            if ((this._buffer[this.GetIndexFromId(j)] != null) && !this._buffer[this.GetIndexFromId(j)].Cleared)
                            {
                                list.Add(this._buffer[this.GetIndexFromId(j)].Clone());
                            }
                        }
                    }
                    else
                    {
                        num = (num2 + count) - 1L;
                        if (num >= this._countEntriesAdded)
                        {
                            num = this._countEntriesAdded;
                        }
                        for (long k = num2; k <= num; k += 1L)
                        {
                            if (num >= this._countEntriesAdded)
                            {
                                break;
                            }
                            if ((this._buffer[this.GetIndexFromId(k)] != null) && this._buffer[this.GetIndexFromId(k)].Cleared)
                            {
                                num += 1L;
                            }
                        }
                        for (long m = num; m >= num2; m -= 1L)
                        {
                            if ((this._buffer[this.GetIndexFromId(m)] != null) && !this._buffer[this.GetIndexFromId(m)].Cleared)
                            {
                                list.Add(this._buffer[this.GetIndexFromId(m)].Clone());
                            }
                        }
                    }
                }
                else
                {
                    long num7;
                    long num8 = 0L;
                    if (this._capacity != 0x1000)
                    {
                        num8 = this.SmallestIDinBuffer();
                    }
                    if (!newest.IsPresent)
                    {
                        num7 = 1L;
                        if ((this._capacity != 0x1000) && (this._countEntriesAdded > this._capacity))
                        {
                            num7 = num8;
                        }
                        long num9 = count - 1L;
                        while (num9 >= 0L)
                        {
                            if (num7 > this._countEntriesAdded)
                            {
                                break;
                            }
                            if (((num7 <= 0L) || (this.GetIndexFromId(num7) >= this._buffer.Length)) || this._buffer[this.GetIndexFromId(num7)].Cleared)
                            {
                                num7 += 1L;
                            }
                            else
                            {
                                list.Add(this._buffer[this.GetIndexFromId(num7)].Clone());
                                num9 -= 1L;
                                num7 += 1L;
                            }
                        }
                    }
                    else
                    {
                        num7 = this._countEntriesAdded;
                        long num10 = count - 1L;
                        while (num10 >= 0L)
                        {
                            if ((((this._capacity != 0x1000) && (this._countEntriesAdded > this._capacity)) && (num7 < num8)) || (num7 < 1L))
                            {
                                break;
                            }
                            if (((num7 <= 0L) || (this.GetIndexFromId(num7) >= this._buffer.Length)) || this._buffer[this.GetIndexFromId(num7)].Cleared)
                            {
                                num7 -= 1L;
                            }
                            else
                            {
                                list.Add(this._buffer[this.GetIndexFromId(num7)].Clone());
                                num10 -= 1L;
                                num7 -= 1L;
                            }
                        }
                    }
                }
                HistoryInfo[] array = new HistoryInfo[list.Count];
                list.CopyTo(array);
                return array;
            }
        }

        internal HistoryInfo[] GetEntries(WildcardPattern wildcardpattern, long count, SwitchParameter newest)
        {
            lock (this._syncRoot)
            {
                if (count < -1L)
                {
                    throw PSTraceSource.NewArgumentOutOfRangeException("count", count);
                }
                if (newest.ToString() == null)
                {
                    throw PSTraceSource.NewArgumentNullException("newest");
                }
                if ((count > this._countEntriesAdded) || (count == -1L))
                {
                    count = this._countEntriesInBuffer;
                }
                ArrayList list = new ArrayList();
                long num = 1L;
                if (this._capacity != 0x1000)
                {
                    num = this.SmallestIDinBuffer();
                }
                if (count != 0L)
                {
                    if (!newest.IsPresent)
                    {
                        long id = 1L;
                        if ((this._capacity != 0x1000) && (this._countEntriesAdded > this._capacity))
                        {
                            id = num;
                        }
                        long num3 = 0L;
                        while (num3 <= (count - 1L))
                        {
                            if (id > this._countEntriesAdded)
                            {
                                break;
                            }
                            if (!this._buffer[this.GetIndexFromId(id)].Cleared && wildcardpattern.IsMatch(this._buffer[this.GetIndexFromId(id)].CommandLine.Trim()))
                            {
                                list.Add(this._buffer[this.GetIndexFromId(id)].Clone());
                                num3 += 1L;
                            }
                            id += 1L;
                        }
                    }
                    else
                    {
                        long num4 = this._countEntriesAdded;
                        long num5 = 0L;
                        while (num5 <= (count - 1L))
                        {
                            if ((((this._capacity != 0x1000) && (this._countEntriesAdded > this._capacity)) && (num4 < num)) || (num4 < 1L))
                            {
                                break;
                            }
                            if (!this._buffer[this.GetIndexFromId(num4)].Cleared && wildcardpattern.IsMatch(this._buffer[this.GetIndexFromId(num4)].CommandLine.Trim()))
                            {
                                list.Add(this._buffer[this.GetIndexFromId(num4)].Clone());
                                num5 += 1L;
                            }
                            num4 -= 1L;
                        }
                    }
                }
                else
                {
                    for (long i = 1L; i <= this._countEntriesAdded; i += 1L)
                    {
                        if (!this._buffer[this.GetIndexFromId(i)].Cleared && wildcardpattern.IsMatch(this._buffer[this.GetIndexFromId(i)].CommandLine.Trim()))
                        {
                            list.Add(this._buffer[this.GetIndexFromId(i)].Clone());
                        }
                    }
                }
                HistoryInfo[] array = new HistoryInfo[list.Count];
                list.CopyTo(array);
                return array;
            }
        }

        internal HistoryInfo GetEntry(long id)
        {
            lock (this._syncRoot)
            {
                this.ReallocateBufferIfNeeded();
                HistoryInfo info = this.CoreGetEntry(id);
                if ((info != null) && !info.Cleared)
                {
                    return info.Clone();
                }
                return null;
            }
        }

        private int GetHistorySize()
        {
            int num = 0;
            System.Management.Automation.ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            object valueToConvert = (executionContextFromTLS != null) ? executionContextFromTLS.GetVariableValue(SpecialVariables.HistorySizeVarPath) : null;
            if (valueToConvert != null)
            {
                try
                {
                    num = (int) LanguagePrimitives.ConvertTo(valueToConvert, typeof(int), CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException)
                {
                }
            }
            if (num <= 0)
            {
                num = 0x1000;
            }
            return num;
        }

        private int GetIndexForNewEntry()
        {
            return (int) (this._countEntriesAdded % ((long) this._capacity));
        }

        private int GetIndexFromId(long id)
        {
            return (int) ((id - 1L) % ((long) this._capacity));
        }

        private static int GetIndexFromId(long id, int capacity)
        {
            return (int) ((id - 1L) % ((long) capacity));
        }

        internal long GetNextHistoryId()
        {
            return (this._countEntriesAdded + 1L);
        }

        private void IncrementCountOfEntriesInBuffer()
        {
            if (this._countEntriesInBuffer < this._capacity)
            {
                this._countEntriesInBuffer++;
            }
        }

        private void ReallocateBufferIfNeeded()
        {
            int historySize = this.GetHistorySize();
            if (historySize != this._capacity)
            {
                HistoryInfo[] infoArray = new HistoryInfo[historySize];
                int num2 = this._countEntriesInBuffer;
                if (num2 < this._countEntriesAdded)
                {
                    num2 = (int) this._countEntriesAdded;
                }
                if (this._countEntriesInBuffer > historySize)
                {
                    num2 = historySize;
                }
                for (int i = num2; i > 0; i--)
                {
                    long id = (this._countEntriesAdded - i) + 1L;
                    infoArray[GetIndexFromId(id, historySize)] = this._buffer[this.GetIndexFromId(id)];
                }
                this._countEntriesInBuffer = num2;
                this._capacity = historySize;
                this._buffer = infoArray;
            }
        }

        private long SmallestIDinBuffer()
        {
            long id = 0L;
            if (this._buffer != null)
            {
                for (int i = 0; i < this._buffer.Length; i++)
                {
                    if ((this._buffer[i] != null) && !this._buffer[i].Cleared)
                    {
                        id = this._buffer[i].Id;
                        break;
                    }
                }
                for (int j = 0; j < this._buffer.Length; j++)
                {
                    if (((this._buffer[j] != null) && !this._buffer[j].Cleared) && (id > this._buffer[j].Id))
                    {
                        id = this._buffer[j].Id;
                    }
                }
            }
            return id;
        }

        internal void UpdateEntry(long id, PipelineState status, DateTime endTime, bool skipIfLocked)
        {
            if (Monitor.TryEnter(this._syncRoot, skipIfLocked ? 0 : -1))
            {
                try
                {
                    HistoryInfo info = this.CoreGetEntry(id);
                    if (info != null)
                    {
                        info.SetStatus(status);
                        info.SetEndTime(endTime);
                    }
                }
                finally
                {
                    Monitor.Exit(this._syncRoot);
                }
            }
        }
    }
}

