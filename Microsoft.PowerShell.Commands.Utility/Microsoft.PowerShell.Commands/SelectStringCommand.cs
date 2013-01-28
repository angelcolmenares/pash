namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;

    [Cmdlet("Select", "String", DefaultParameterSetName="File", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113388"), OutputType(new Type[] { typeof(MatchInfo), typeof(bool) })]
    public sealed class SelectStringCommand : PSCmdlet
    {
        private bool allMatches;
        private bool caseSensitive;
        private int[] context;
        private bool doneProcessing;
        private string encoding;
        internal WildcardPattern[] exclude;
        internal string[] excludeStrings;
        private string[] fullName;
        private ContextTracker globalContextTracker;
        internal WildcardPattern[] include;
        internal string[] includeStrings;
        private PSObject inputObject = AutomationNull.Value;
        private int inputRecordNumber;
        private bool isLiteralPath;
        private bool list;
        private bool notMatch;
        private string[] pattern;
        private int postContext;
        private int preContext;
        private bool quiet;
        private Regex[] regexPattern;
        private bool simpleMatch;
        private System.Text.Encoding textEncoding;

        protected override void BeginProcessing()
        {
            if (this.encoding != null)
            {
                this.textEncoding = EncodingConversion.Convert(this, this.encoding);
            }
            else
            {
                this.textEncoding = new UTF8Encoding();
            }
            if (!this.simpleMatch)
            {
                RegexOptions options = this.caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                this.regexPattern = new Regex[this.pattern.Length];
                for (int i = 0; i < this.pattern.Length; i++)
                {
                    try
                    {
                        this.regexPattern[i] = new Regex(this.pattern[i], options);
                    }
                    catch (Exception exception)
                    {
                        base.ThrowTerminatingError(BuildErrorRecord(MatchStringStrings.InvalidRegex, this.pattern[i], exception.Message, "InvalidRegex", exception));
                        throw;
                    }
                }
            }
            this.globalContextTracker = new ContextTracker(this.preContext, this.postContext);
        }

        private static ErrorRecord BuildErrorRecord(string messageId, object[] arguments, string errorId, Exception innerException)
        {
            return new ErrorRecord(new ArgumentException(StringUtil.Format(messageId, arguments), innerException), errorId, ErrorCategory.InvalidArgument, null);
        }

        private static ErrorRecord BuildErrorRecord(string messageId, string argument, string errorId, Exception innerException)
        {
            return BuildErrorRecord(messageId, new object[] { argument }, errorId, innerException);
        }

        private static ErrorRecord BuildErrorRecord(string messageId, string arg0, string arg1, string errorId, Exception innerException)
        {
            return BuildErrorRecord(messageId, new object[] { arg0, arg1 }, errorId, innerException);
        }

        private bool doMatch(object operand, out MatchInfo matchResult, out string operandString)
        {
            bool success = false;
            Match[] array = null;
            int index = 0;
            matchResult = null;
            MatchInfo info = operand as MatchInfo;
            if (info != null)
            {
                operandString = info.Line;
                if ((this.preContext > 0) || (this.postContext > 0))
                {
                    this.preContext = 0;
                    this.postContext = 0;
                    this.globalContextTracker = new ContextTracker(this.preContext, this.postContext);
                    this.WarnFilterContext();
                }
            }
            else
            {
                operandString = (string) LanguagePrimitives.ConvertTo(operand, typeof(string), CultureInfo.InvariantCulture);
            }
            if (!this.simpleMatch)
            {
                while (index < this.pattern.Length)
                {
                    Regex regex = this.regexPattern[index];
                    if (this.allMatches && !this.notMatch)
                    {
                        MatchCollection matchs = regex.Matches(operandString);
                        if ((matchs != null) && (matchs.Count > 0))
                        {
                            array = new Match[matchs.Count];
                            matchs.CopyTo(array, 0);
                            success = true;
                        }
                    }
                    else
                    {
                        Match match = regex.Match(operandString);
                        success = match.Success;
                        if (match.Success)
                        {
                            array = new Match[] { match };
                        }
                    }
                    if (success)
                    {
                        break;
                    }
                    index++;
                }
            }
            else
            {
                StringComparison comparisonType = this.caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
                while (index < this.pattern.Length)
                {
                    string str = this.pattern[index];
                    if (operandString.IndexOf(str, comparisonType) >= 0)
                    {
                        success = true;
                        break;
                    }
                    index++;
                }
            }
            if (this.notMatch)
            {
                success = !success;
                index = 0;
            }
            if (!success)
            {
                return false;
            }
            if (info != null)
            {
                if (info.Context != null)
                {
                    matchResult = info.Clone();
                    matchResult.Context.DisplayPreContext = new string[0];
                    matchResult.Context.DisplayPostContext = new string[0];
                }
                else
                {
                    matchResult = info;
                }
                return true;
            }
            matchResult = new MatchInfo();
            matchResult.IgnoreCase = !this.caseSensitive;
            matchResult.Line = operandString;
            matchResult.Pattern = this.pattern[index];
            if ((this.preContext > 0) || (this.postContext > 0))
            {
                matchResult.Context = new MatchInfoContext();
            }
            matchResult.Matches = (array != null) ? array : new Match[0];
            return true;
        }

        protected override void EndProcessing()
        {
            this.globalContextTracker.TrackEOF();
            if (!this.doneProcessing)
            {
                this.FlushTrackerQueue(this.globalContextTracker);
            }
        }

        private bool FlushTrackerQueue(ContextTracker contextTracker)
        {
            if (contextTracker.EmitQueue.Count < 1)
            {
                return false;
            }
            if (this.quiet && !this.list)
            {
                base.WriteObject(true);
            }
            else if (this.list)
            {
                base.WriteObject(contextTracker.EmitQueue[0]);
            }
            else
            {
                foreach (MatchInfo info in contextTracker.EmitQueue)
                {
                    base.WriteObject(info);
                }
            }
            contextTracker.EmitQueue.Clear();
            return true;
        }

        private bool meetsIncludeExcludeCriteria(string filename)
        {
            bool flag = false;
            if (this.include != null)
            {
                foreach (WildcardPattern pattern in this.include)
                {
                    if (pattern.IsMatch(filename))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            else
            {
                flag = true;
            }
            if (!flag)
            {
                return false;
            }
            if (this.exclude != null)
            {
                foreach (WildcardPattern pattern2 in this.exclude)
                {
                    if (pattern2.IsMatch(filename))
                    {
                        return false;
                    }
                }
            }
            return flag;
        }

        private bool ProcessFile(string filename)
        {
            ContextTracker contextTracker = new ContextTracker(this.preContext, this.postContext);
            bool flag = false;
            try
            {
                if (!this.meetsIncludeExcludeCriteria(filename))
                {
                    return false;
                }
                using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(stream, this.textEncoding))
                    {
                        string str;
                        int num = 0;
                        while ((str = reader.ReadLine()) != null)
                        {
                            num++;
                            MatchInfo matchResult = null;
                            string operandString = null;
                            if (this.doMatch(str, out matchResult, out operandString))
                            {
                                matchResult.Path = filename;
                                matchResult.LineNumber = num;
                                contextTracker.TrackMatch(matchResult);
                            }
                            else
                            {
                                contextTracker.TrackLine(str);
                            }
                            if (contextTracker.EmitQueue.Count > 0)
                            {
                                flag = true;
                                if (this.quiet || this.list)
                                {
                                    goto Label_00C9;
                                }
                                this.FlushTrackerQueue(contextTracker);
                            }
                        }
                    }
                }
            Label_00C9:
                contextTracker.TrackEOF();
                if (this.FlushTrackerQueue(contextTracker))
                {
                    flag = true;
                }
            }
            catch (NotSupportedException exception)
            {
                base.WriteError(BuildErrorRecord(MatchStringStrings.FileReadError, filename, exception.Message, "ProcessingFile", exception));
            }
            catch (IOException exception2)
            {
                base.WriteError(BuildErrorRecord(MatchStringStrings.FileReadError, filename, exception2.Message, "ProcessingFile", exception2));
            }
            catch (SecurityException exception3)
            {
                base.WriteError(BuildErrorRecord(MatchStringStrings.FileReadError, filename, exception3.Message, "ProcessingFile", exception3));
            }
            catch (UnauthorizedAccessException exception4)
            {
                base.WriteError(BuildErrorRecord(MatchStringStrings.FileReadError, filename, exception4.Message, "ProcessingFile", exception4));
            }
            return flag;
        }

        protected override void ProcessRecord()
        {
            if (!this.doneProcessing)
            {
                List<string> list = null;
                if (this.fullName != null)
                {
                    list = this.ResolveFilePaths(this.fullName, this.isLiteralPath);
                    if (list == null)
                    {
                        return;
                    }
                }
                else
                {
                    FileInfo baseObject = this.inputObject.BaseObject as FileInfo;
                    if (baseObject != null)
                    {
                        list = new List<string> {
                            baseObject.FullName
                        };
                    }
                }
                if (list != null)
                {
                    foreach (string str in list)
                    {
                        bool flag = this.ProcessFile(str);
                        if (this.quiet && flag)
                        {
                            return;
                        }
                    }
                    if (this.quiet)
                    {
                        if (this.list)
                        {
                            base.WriteObject(null);
                        }
                        else
                        {
                            base.WriteObject(false);
                        }
                    }
                }
                else
                {
                    this.inputRecordNumber++;
                    object operand = this.inputObject.BaseObject as MatchInfo;
                    if (operand == null)
                    {
                        operand = this.inputObject;
                    }
                    MatchInfo matchResult = null;
                    string operandString = null;
                    if (this.doMatch(operand, out matchResult, out operandString))
                    {
                        if (!(operand is MatchInfo))
                        {
                            matchResult.LineNumber = this.inputRecordNumber;
                        }
                        this.globalContextTracker.TrackMatch(matchResult);
                    }
                    else
                    {
                        this.globalContextTracker.TrackLine(operandString);
                    }
                    if (this.FlushTrackerQueue(this.globalContextTracker) && this.quiet)
                    {
                        this.doneProcessing = true;
                    }
                }
            }
        }

        private List<string> ResolveFilePaths(string[] filePaths, bool isLiteralPath)
        {
            List<string> list = new List<string>();
            foreach (string str in filePaths)
            {
                ProviderInfo info;
                Collection<string> resolvedProviderPathFromPSPath;
                if (isLiteralPath)
                {
                    PSDriveInfo info2;
                    resolvedProviderPathFromPSPath = new Collection<string>();
                    string item = base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(str, out info, out info2);
                    resolvedProviderPathFromPSPath.Add(item);
                }
                else
                {
                    resolvedProviderPathFromPSPath = base.GetResolvedProviderPathFromPSPath(str, out info);
                }
                if (!info.NameEquals(base.Context.ProviderNames.FileSystem))
                {
                    base.WriteError(BuildErrorRecord(MatchStringStrings.FileOpenError, info.FullName, "ProcessingFile", null));
                }
                else
                {
                    list.AddRange(resolvedProviderPathFromPSPath);
                }
            }
            return list;
        }

        private void WarnFilterContext()
        {
            string filterContextWarning = MatchStringStrings.FilterContextWarning;
            base.WriteWarning(filterContextWarning);
        }

        [Parameter]
        public SwitchParameter AllMatches
        {
            get
            {
                return this.allMatches;
            }
            set
            {
                this.allMatches = (bool) value;
            }
        }

        [Parameter]
        public SwitchParameter CaseSensitive
        {
            get
            {
                return this.caseSensitive;
            }
            set
            {
                this.caseSensitive = (bool) value;
            }
        }

        [Parameter, ValidateNotNullOrEmpty, ValidateCount(1, 2), ValidateRange(0, 0x7fffffff)]
        public int[] Context
        {
            get
            {
                return this.context;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                this.context = value;
                if (this.context.Length == 1)
                {
                    this.preContext = this.context[0];
                    this.postContext = this.context[0];
                }
                else if (this.context.Length >= 2)
                {
                    this.preContext = this.context[0];
                    this.postContext = this.context[1];
                }
            }
        }

        [ValidateSet(new string[] { "unicode", "utf7", "utf8", "utf32", "ascii", "bigendianunicode", "default", "oem" }), Parameter, ValidateNotNullOrEmpty]
        public string Encoding
        {
            get
            {
                return this.encoding;
            }
            set
            {
                this.encoding = value;
            }
        }

        [Parameter, ValidateNotNullOrEmpty]
        public string[] Exclude
        {
            get
            {
                return this.excludeStrings;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                this.excludeStrings = value;
                this.exclude = new WildcardPattern[this.excludeStrings.Length];
                for (int i = 0; i < this.excludeStrings.Length; i++)
                {
                    this.exclude[i] = new WildcardPattern(this.excludeStrings[i], WildcardOptions.IgnoreCase);
                }
            }
        }

        [ValidateNotNullOrEmpty, Parameter]
        public string[] Include
        {
            get
            {
                return this.includeStrings;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                this.includeStrings = value;
                this.include = new WildcardPattern[this.includeStrings.Length];
                for (int i = 0; i < this.includeStrings.Length; i++)
                {
                    this.include[i] = new WildcardPattern(this.includeStrings[i], WildcardOptions.IgnoreCase);
                }
            }
        }

        [AllowNull, AllowEmptyString, Parameter(ValueFromPipeline=true, Mandatory=true, ParameterSetName="Object")]
        public PSObject InputObject
        {
            get
            {
                return this.inputObject;
            }
            set
            {
                this.inputObject = LanguagePrimitives.IsNull(value) ? PSObject.AsPSObject("") : value;
            }
        }

        [Parameter]
        public SwitchParameter List
        {
            get
            {
                return this.list;
            }
            set
            {
                this.list = (bool) value;
            }
        }

        [Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="LiteralFile"), FileinfoToString, Alias(new string[] { "PSPath" })]
        public string[] LiteralPath
        {
            get
            {
                return this.fullName;
            }
            set
            {
                this.fullName = value;
                this.isLiteralPath = true;
            }
        }

        [Parameter]
        public SwitchParameter NotMatch
        {
            get
            {
                return this.notMatch;
            }
            set
            {
                this.notMatch = (bool) value;
            }
        }

        [Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="File"), FileinfoToString]
        public string[] Path
        {
            get
            {
                return this.fullName;
            }
            set
            {
                this.fullName = value;
            }
        }

        [Parameter(Mandatory=true, Position=0)]
        public string[] Pattern
        {
            get
            {
                return this.pattern;
            }
            set
            {
                this.pattern = value;
            }
        }

        [Parameter]
        public SwitchParameter Quiet
        {
            get
            {
                return this.quiet;
            }
            set
            {
                this.quiet = (bool) value;
            }
        }

        [Parameter]
        public SwitchParameter SimpleMatch
        {
            get
            {
                return this.simpleMatch;
            }
            set
            {
                this.simpleMatch = (bool) value;
            }
        }

        private class CircularBuffer<T> : ICollection<T>, IEnumerable<T>, IEnumerable
        {
            private int firstIndex;
            private T[] items;
            private int length;

            public CircularBuffer(int capacity)
            {
                if (capacity < 0)
                {
                    throw new ArgumentOutOfRangeException("capacity");
                }
                this.items = new T[capacity];
                this.Clear();
            }

            public void Add(T item)
            {
                if (this.Capacity != 0)
                {
                    int firstIndex;
                    if (this.IsFull)
                    {
                        firstIndex = this.firstIndex;
                        this.firstIndex = (this.firstIndex + 1) % this.Capacity;
                    }
                    else
                    {
                        firstIndex = this.firstIndex + this.length;
                        this.length++;
                    }
                    this.items[firstIndex] = item;
                }
            }

            public void Clear()
            {
                this.firstIndex = 0;
                this.length = 0;
            }

            public bool Contains(T item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (arrayIndex < 0)
                {
                    throw new ArgumentOutOfRangeException("arrayIndex");
                }
                if (this.length > (array.Length - arrayIndex))
                {
                    throw new ArgumentException("arrayIndex");
                }
                foreach (T local in this)
                {
                    array[arrayIndex++] = local;
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                int zeroBasedIndex = 0;
                while (true)
                {
                    if (zeroBasedIndex >= this.length)
                    {
                        yield break;
                    }
                    yield return this.items[this.WrapIndex(zeroBasedIndex)];
                    zeroBasedIndex++;
                }
            }

            public bool Remove(T item)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public T[] ToArray()
            {
                T[] array = new T[this.Count];
                this.CopyTo(array, 0);
                return array;
            }

            private int WrapIndex(int zeroBasedIndex)
            {
                if ((this.Capacity == 0) || (zeroBasedIndex < 0))
                {
                    throw new ArgumentOutOfRangeException("zeroBasedIndex");
                }
                return ((zeroBasedIndex + this.firstIndex) % this.Capacity);
            }

            public int Capacity
            {
                get
                {
                    return this.items.Length;
                }
            }

            public int Count
            {
                get
                {
                    return this.length;
                }
            }

            public bool IsFull
            {
                get
                {
                    return (this.length == this.Capacity);
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public T this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.Count))
                    {
                        throw new ArgumentOutOfRangeException("index");
                    }
                    return this.items[this.WrapIndex(index)];
                }
            }
        }
            
        private class ContextTracker : SelectStringCommand.IContextTracker
        {
            private SelectStringCommand.IContextTracker displayTracker;
            private IList<MatchInfo> emitQueue;
            private SelectStringCommand.IContextTracker logicalTracker;

            public ContextTracker(int preContext, int postContext)
            {
                this.displayTracker = new SelectStringCommand.DisplayContextTracker(preContext, postContext);
                this.logicalTracker = new SelectStringCommand.LogicalContextTracker(preContext, postContext);
                this.emitQueue = new List<MatchInfo>();
            }

            public void TrackEOF()
            {
                this.displayTracker.TrackEOF();
                this.logicalTracker.TrackEOF();
                this.UpdateQueue();
            }

            public void TrackLine(string line)
            {
                this.displayTracker.TrackLine(line);
                this.logicalTracker.TrackLine(line);
                this.UpdateQueue();
            }

            public void TrackMatch(MatchInfo match)
            {
                this.displayTracker.TrackMatch(match);
                this.logicalTracker.TrackMatch(match);
                this.UpdateQueue();
            }

            private void UpdateQueue()
            {
                foreach (MatchInfo info in this.logicalTracker.EmitQueue)
                {
                    this.emitQueue.Add(info);
                }
                this.logicalTracker.EmitQueue.Clear();
                this.displayTracker.EmitQueue.Clear();
            }

            public IList<MatchInfo> EmitQueue
            {
                get
                {
                    return this.emitQueue;
                }
            }
        }

        private class DisplayContextTracker : SelectStringCommand.IContextTracker
        {
            private List<string> collectedPostContext;
            private SelectStringCommand.CircularBuffer<string> collectedPreContext;
            private ContextState contextState;
            private List<MatchInfo> emitQueue;
            private MatchInfo matchInfo;
            private int postContext;
            private int preContext;

            public DisplayContextTracker(int preContext, int postContext)
            {
                this.preContext = preContext;
                this.postContext = postContext;
                this.collectedPreContext = new SelectStringCommand.CircularBuffer<string>(preContext);
                this.collectedPostContext = new List<string>(postContext);
                this.emitQueue = new List<MatchInfo>();
                this.Reset();
            }

            private void Reset()
            {
                this.contextState = (this.preContext > 0) ? ContextState.CollectPre : ContextState.InitialState;
                this.collectedPreContext.Clear();
                this.collectedPostContext.Clear();
                this.matchInfo = null;
            }

            public void TrackEOF()
            {
                if (this.contextState == ContextState.CollectPost)
                {
                    this.UpdateQueue();
                }
            }

            public void TrackLine(string line)
            {
                switch (this.contextState)
                {
                    case ContextState.InitialState:
                        break;

                    case ContextState.CollectPre:
                        this.collectedPreContext.Add(line);
                        return;

                    case ContextState.CollectPost:
                        this.collectedPostContext.Add(line);
                        if (this.collectedPostContext.Count >= this.postContext)
                        {
                            this.UpdateQueue();
                        }
                        break;

                    default:
                        return;
                }
            }

            public void TrackMatch(MatchInfo match)
            {
                if (this.contextState == ContextState.CollectPost)
                {
                    this.UpdateQueue();
                }
                this.matchInfo = match;
                if (this.postContext > 0)
                {
                    this.contextState = ContextState.CollectPost;
                }
                else
                {
                    this.UpdateQueue();
                }
            }

            private void UpdateQueue()
            {
                if (this.matchInfo != null)
                {
                    this.emitQueue.Add(this.matchInfo);
                    if (this.matchInfo.Context != null)
                    {
                        this.matchInfo.Context.DisplayPreContext = this.collectedPreContext.ToArray();
                        this.matchInfo.Context.DisplayPostContext = this.collectedPostContext.ToArray();
                    }
                    this.Reset();
                }
            }

            public IList<MatchInfo> EmitQueue
            {
                get
                {
                    return this.emitQueue;
                }
            }

            private enum ContextState
            {
                InitialState,
                CollectPre,
                CollectPost
            }
        }

        private class FileinfoToStringAttribute : ArgumentTransformationAttribute
        {
            public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
            {
                FileInfo info;
                object baseObject = inputData;
                PSObject obj3 = baseObject as PSObject;
                if (obj3 != null)
                {
                    baseObject = obj3.BaseObject;
                }
                IList list = baseObject as IList;
                if (list != null)
                {
                    object[] objArray = new object[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        object obj4 = list[i];
                        obj3 = obj4 as PSObject;
                        if (obj3 != null)
                        {
                            obj4 = obj3.BaseObject;
                        }
                        info = obj4 as FileInfo;
                        if (info != null)
                        {
                            objArray[i] = info.FullName;
                        }
                        else
                        {
                            objArray[i] = obj4;
                        }
                    }
                    return objArray;
                }
                info = baseObject as FileInfo;
                if (info != null)
                {
                    return info.FullName;
                }
                return inputData;
            }
        }

        private interface IContextTracker
        {
            void TrackEOF();
            void TrackLine(string line);
            void TrackMatch(MatchInfo match);

            IList<MatchInfo> EmitQueue { get; }
        }

        private class LogicalContextTracker : SelectStringCommand.IContextTracker
        {
            private SelectStringCommand.CircularBuffer<ContextEntry> collectedContext;
            private List<MatchInfo> emitQueue;
            private bool hasProcessedPreEntries;
            private int postContext;
            private int preContext;

            public LogicalContextTracker(int preContext, int postContext)
            {
                this.preContext = preContext;
                this.postContext = postContext;
                this.collectedContext = new SelectStringCommand.CircularBuffer<ContextEntry>((preContext + postContext) + 1);
                this.emitQueue = new List<MatchInfo>();
            }

            private string[] CopyContext(int startIndex, int length)
            {
                string[] strArray = new string[length];
                for (int i = 0; i < length; i++)
                {
                    strArray[i] = this.collectedContext[startIndex + i].ToString();
                }
                return strArray;
            }

            private void Emit(MatchInfo match, int preStartIndex, int preLength, int postStartIndex, int postLength)
            {
                if (match.Context != null)
                {
                    match.Context.PreContext = this.CopyContext(preStartIndex, preLength);
                    match.Context.PostContext = this.CopyContext(postStartIndex, postLength);
                }
                this.emitQueue.Add(match);
            }

            private void EmitAllInRange(int startIndex, int endIndex)
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    MatchInfo match = this.collectedContext[i].Match;
                    if (match != null)
                    {
                        int preStartIndex = Math.Max(i - this.preContext, 0);
                        int postLength = Math.Min(this.postContext, (this.collectedContext.Count - i) - 1);
                        this.Emit(match, preStartIndex, i - preStartIndex, i + 1, postLength);
                    }
                }
            }

            public void TrackEOF()
            {
                int startIndex = this.collectedContext.IsFull ? (this.preContext + 1) : 0;
                this.EmitAllInRange(startIndex, this.collectedContext.Count - 1);
            }

            public void TrackLine(string line)
            {
                ContextEntry item = new ContextEntry(line);
                this.collectedContext.Add(item);
                this.UpdateQueue();
            }

            public void TrackMatch(MatchInfo match)
            {
                ContextEntry item = new ContextEntry(match);
                this.collectedContext.Add(item);
                this.UpdateQueue();
            }

            private void UpdateQueue()
            {
                if (this.collectedContext.IsFull)
                {
                    if (this.hasProcessedPreEntries)
                    {
                        this.EmitAllInRange(this.preContext, this.preContext);
                    }
                    else
                    {
                        this.EmitAllInRange(0, this.preContext);
                        this.hasProcessedPreEntries = true;
                    }
                }
            }

            public IList<MatchInfo> EmitQueue
            {
                get
                {
                    return this.emitQueue;
                }
            }

            private class ContextEntry
            {
                public string Line;
                public MatchInfo Match;

                public ContextEntry(MatchInfo match)
                {
                    this.Match = match;
                }

                public ContextEntry(string line)
                {
                    this.Line = line;
                }

                public override string ToString()
                {
                    if (this.Match == null)
                    {
                        return this.Line;
                    }
                    return this.Match.Line;
                }
            }
        }
    }
}

