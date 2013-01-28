namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;

    [DebuggerDisplay("Command = {_commandInfo}")]
    public class InvocationInfo
    {
        private Dictionary<string, object> _boundParameters;
        private readonly CommandInfo _commandInfo;
        private System.Management.Automation.CommandOrigin _commandOrigin;
        private IScriptExtent _displayScriptPosition;
        private bool _expectingInput;
        private long _historyId;
        private string _invocationName;
        private int[] _pipelineIterationInfo;
        private int _pipelineLength;
        private int _pipelinePosition;
        private IScriptExtent _scriptPosition;
        private List<object> _unboundArguments;

        internal InvocationInfo(InternalCommand command) : this(command.CommandInfo, command.InvocationExtent ?? PositionUtilities.EmptyExtent)
        {
            this._commandOrigin = command.CommandOrigin;
        }

        internal InvocationInfo(PSObject psObject)
        {
            this._historyId = -1L;
            this._pipelineIterationInfo = new int[0];
            this._commandOrigin = (System.Management.Automation.CommandOrigin) SerializationUtilities.GetPsObjectPropertyBaseObject(psObject, "InvocationInfo_CommandOrigin");
            this._expectingInput = (bool) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_ExpectingInput");
            this._invocationName = (string) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_InvocationName");
            this._historyId = (long) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_HistoryId");
            this._pipelineLength = (int) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_PipelineLength");
            this._pipelinePosition = (int) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_PipelinePosition");
            string propertyValue = (string) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_ScriptName");
            int scriptLineNumber = (int) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_ScriptLineNumber");
            int offsetInLine = (int) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_OffsetInLine");
            string line = (string) SerializationUtilities.GetPropertyValue(psObject, "InvocationInfo_Line");
            System.Management.Automation.Language.ScriptPosition startPosition = new System.Management.Automation.Language.ScriptPosition(propertyValue, scriptLineNumber, offsetInLine, line);
            this._scriptPosition = new ScriptExtent(startPosition, startPosition);
            this._commandInfo = RemoteCommandInfo.FromPSObjectForRemoting(psObject);
            ArrayList psObjectPropertyBaseObject = (ArrayList) SerializationUtilities.GetPsObjectPropertyBaseObject(psObject, "InvocationInfo_PipelineIterationInfo");
            if (psObjectPropertyBaseObject != null)
            {
                this._pipelineIterationInfo = (int[]) psObjectPropertyBaseObject.ToArray(Type.GetType("System.Int32"));
            }
            else
            {
                this._pipelineIterationInfo = new int[0];
            }
            Hashtable hashtable = (Hashtable) SerializationUtilities.GetPsObjectPropertyBaseObject(psObject, "InvocationInfo_BoundParameters");
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            if (hashtable != null)
            {
                foreach (DictionaryEntry entry in hashtable)
                {
                    dictionary.Add((string) entry.Key, entry.Value);
                }
            }
            this._boundParameters = dictionary;
            ArrayList list2 = (ArrayList) SerializationUtilities.GetPsObjectPropertyBaseObject(psObject, "InvocationInfo_UnboundArguments");
            this._unboundArguments = new List<object>();
            if (list2 != null)
            {
                foreach (object obj2 in list2)
                {
                    this._unboundArguments.Add(obj2);
                }
            }
            object obj3 = SerializationUtilities.GetPropertyValue(psObject, "SerializeExtent");
            bool flag = false;
            if (obj3 != null)
            {
                flag = (bool) obj3;
            }
            if (flag)
            {
                this._displayScriptPosition = ScriptExtent.FromPSObjectForRemoting(psObject);
            }
        }

        internal InvocationInfo(CommandInfo commandInfo, IScriptExtent scriptPosition) : this(commandInfo, scriptPosition, null)
        {
        }

        internal InvocationInfo(CommandInfo commandInfo, IScriptExtent scriptPosition, ExecutionContext context)
        {
            this._historyId = -1L;
            this._pipelineIterationInfo = new int[0];
            this._commandInfo = commandInfo;
            this._commandOrigin = System.Management.Automation.CommandOrigin.Internal;
            this._scriptPosition = scriptPosition;
            ExecutionContext context2 = null;
            if ((commandInfo != null) && (commandInfo.Context != null))
            {
                context2 = commandInfo.Context;
            }
            else if (context != null)
            {
                context2 = context;
            }
            if (context2 != null)
            {
                LocalRunspace currentRunspace = context2.CurrentRunspace as LocalRunspace;
                if ((currentRunspace != null) && (currentRunspace.History != null))
                {
                    this._historyId = currentRunspace.History.GetNextHistoryId();
                }
            }
        }

        internal string GetFullScript()
        {
            if ((this.ScriptPosition != null) && (this.ScriptPosition.StartScriptPosition != null))
            {
                return this.ScriptPosition.StartScriptPosition.GetFullScript();
            }
            return null;
        }

        internal void ToPSObjectForRemoting(PSObject psObject)
        {
            RemotingEncoder.AddNoteProperty<object>(psObject, "InvocationInfo_BoundParameters", () => this.BoundParameters);
            RemotingEncoder.AddNoteProperty<System.Management.Automation.CommandOrigin>(psObject, "InvocationInfo_CommandOrigin", () => this.CommandOrigin);
            RemotingEncoder.AddNoteProperty<bool>(psObject, "InvocationInfo_ExpectingInput", () => this.ExpectingInput);
            RemotingEncoder.AddNoteProperty<string>(psObject, "InvocationInfo_InvocationName", () => this.InvocationName);
            RemotingEncoder.AddNoteProperty<string>(psObject, "InvocationInfo_Line", () => this.Line);
            RemotingEncoder.AddNoteProperty<int>(psObject, "InvocationInfo_OffsetInLine", () => this.OffsetInLine);
            RemotingEncoder.AddNoteProperty<long>(psObject, "InvocationInfo_HistoryId", () => this.HistoryId);
            RemotingEncoder.AddNoteProperty<int[]>(psObject, "InvocationInfo_PipelineIterationInfo", () => this.PipelineIterationInfo);
            RemotingEncoder.AddNoteProperty<int>(psObject, "InvocationInfo_PipelineLength", () => this.PipelineLength);
            RemotingEncoder.AddNoteProperty<int>(psObject, "InvocationInfo_PipelinePosition", () => this.PipelinePosition);
            RemotingEncoder.AddNoteProperty<string>(psObject, "InvocationInfo_PSScriptRoot", () => this.PSScriptRoot);
            RemotingEncoder.AddNoteProperty<string>(psObject, "InvocationInfo_PSCommandPath", () => this.PSCommandPath);
            RemotingEncoder.AddNoteProperty<string>(psObject, "InvocationInfo_PositionMessage", () => this.PositionMessage);
            RemotingEncoder.AddNoteProperty<int>(psObject, "InvocationInfo_ScriptLineNumber", () => this.ScriptLineNumber);
            RemotingEncoder.AddNoteProperty<string>(psObject, "InvocationInfo_ScriptName", () => this.ScriptName);
            RemotingEncoder.AddNoteProperty<object>(psObject, "InvocationInfo_UnboundArguments", () => this.UnboundArguments);
            ScriptExtent displayScriptPosition = this.DisplayScriptPosition as ScriptExtent;
            if (displayScriptPosition != null)
            {
                displayScriptPosition.ToPSObjectForRemoting(psObject);
                RemotingEncoder.AddNoteProperty<bool>(psObject, "SerializeExtent", () => true);
            }
            else
            {
                RemotingEncoder.AddNoteProperty<bool>(psObject, "SerializeExtent", () => false);
            }
            RemoteCommandInfo.ToPSObjectForRemoting(this.MyCommand, psObject);
        }

        public Dictionary<string, object> BoundParameters
        {
            get
            {
                if (this._boundParameters == null)
                {
                    this._boundParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                }
                return this._boundParameters;
            }
            internal set
            {
                this._boundParameters = value;
            }
        }

        public System.Management.Automation.CommandOrigin CommandOrigin
        {
            get
            {
                return this._commandOrigin;
            }
            internal set
            {
                this._commandOrigin = value;
            }
        }

        public IScriptExtent DisplayScriptPosition
        {
            get
            {
                return this._displayScriptPosition;
            }
            set
            {
                this._displayScriptPosition = value;
            }
        }

        public bool ExpectingInput
        {
            get
            {
                return this._expectingInput;
            }
            internal set
            {
                this._expectingInput = value;
            }
        }

        public long HistoryId
        {
            get
            {
                return this._historyId;
            }
            internal set
            {
                this._historyId = value;
            }
        }

        public string InvocationName
        {
            get
            {
                return (this._invocationName ?? "");
            }
            internal set
            {
                this._invocationName = value;
            }
        }

        public string Line
        {
            get
            {
                if (this.ScriptPosition.StartScriptPosition != null)
                {
                    return this.ScriptPosition.StartScriptPosition.Line;
                }
                return string.Empty;
            }
        }

        public CommandInfo MyCommand
        {
            get
            {
                return this._commandInfo;
            }
        }

        public int OffsetInLine
        {
            get
            {
                return this.ScriptPosition.StartColumnNumber;
            }
        }

        internal int[] PipelineIterationInfo
        {
            get
            {
                return this._pipelineIterationInfo;
            }
            set
            {
                this._pipelineIterationInfo = value;
            }
        }

        public int PipelineLength
        {
            get
            {
                return this._pipelineLength;
            }
            internal set
            {
                this._pipelineLength = value;
            }
        }

        public int PipelinePosition
        {
            get
            {
                return this._pipelinePosition;
            }
            internal set
            {
                this._pipelinePosition = value;
            }
        }

        public string PositionMessage
        {
            get
            {
                return PositionUtilities.VerboseMessage(this.ScriptPosition);
            }
        }

        public string PSCommandPath
        {
            get
            {
                return this.ScriptPosition.File;
            }
        }

        public string PSScriptRoot
        {
            get
            {
                if (!string.IsNullOrEmpty(this.ScriptPosition.File))
                {
                    return Path.GetDirectoryName(this.ScriptPosition.File);
                }
                return string.Empty;
            }
        }

        public int ScriptLineNumber
        {
            get
            {
                return this.ScriptPosition.StartLineNumber;
            }
        }

        public string ScriptName
        {
            get
            {
                return (this.ScriptPosition.File ?? "");
            }
        }

        internal IScriptExtent ScriptPosition
        {
            get
            {
                if (this._displayScriptPosition != null)
                {
                    return this._displayScriptPosition;
                }
                return this._scriptPosition;
            }
            set
            {
                this._scriptPosition = value;
            }
        }

        public List<object> UnboundArguments
        {
            get
            {
                if (this._unboundArguments == null)
                {
                    this._unboundArguments = new List<object>();
                }
                return this._unboundArguments;
            }
            internal set
            {
                this._unboundArguments = value;
            }
        }
    }
}

