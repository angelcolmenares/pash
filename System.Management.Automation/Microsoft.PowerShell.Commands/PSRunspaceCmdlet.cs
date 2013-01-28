namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;

    public abstract class PSRunspaceCmdlet : PSRemotingCmdlet
    {
        private string[] computerNames;
        protected const string IdParameterSet = "Id";
        protected const string InstanceIdParameterSet = "InstanceId";
        protected const string NameParameterSet = "Name";
        private string[] names;
        private Guid[] remoteRunspaceIds;
        private int[] sessionIds;

        protected PSRunspaceCmdlet()
        {
        }

        internal Dictionary<Guid, PSSession> GetAllRunspaces(bool writeobject, bool writeErrorOnNoMatch)
        {
            Dictionary<Guid, PSSession> dictionary = new Dictionary<Guid, PSSession>();
            foreach (PSSession session in base.RunspaceRepository.Runspaces)
            {
                if (writeobject)
                {
                    base.WriteObject(session);
                }
                else
                {
                    dictionary.Add(session.InstanceId, session);
                }
            }
            return dictionary;
        }

        protected Dictionary<Guid, PSSession> GetMatchingRunspaces(bool writeobject, bool writeErrorOnNoMatch)
        {
            switch (base.ParameterSetName)
            {
                case "ComputerName":
                    return this.GetMatchingRunspacesByComputerName(writeobject, writeErrorOnNoMatch);

                case "InstanceId":
                    return this.GetMatchingRunspacesByRunspaceId(writeobject, writeErrorOnNoMatch);

                case "Name":
                    return this.GetMatchingRunspacesByName(writeobject, writeErrorOnNoMatch);

                case "Id":
                    return this.GetMatchingRunspacesBySessionId(writeobject, writeErrorOnNoMatch);
            }
            return null;
        }

        private Dictionary<Guid, PSSession> GetMatchingRunspacesByComputerName(bool writeobject, bool writeErrorOnNoMatch)
        {
            if ((this.computerNames == null) || (this.computerNames.Length == 0))
            {
                return this.GetAllRunspaces(writeobject, writeErrorOnNoMatch);
            }
            Dictionary<Guid, PSSession> dictionary = new Dictionary<Guid, PSSession>();
            List<PSSession> runspaces = base.RunspaceRepository.Runspaces;
            foreach (string str in this.computerNames)
            {
                WildcardPattern pattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
                bool flag = false;
                foreach (PSSession session in runspaces)
                {
                    if (pattern.IsMatch(session.ComputerName))
                    {
                        flag = true;
                        if (writeobject)
                        {
                            base.WriteObject(session);
                        }
                        else
                        {
                            try
                            {
                                dictionary.Add(session.InstanceId, session);
                            }
                            catch (ArgumentException)
                            {
                            }
                        }
                    }
                }
                if (!flag && writeErrorOnNoMatch)
                {
                    this.WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedComputer, RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedComputer, str);
                }
            }
            return dictionary;
        }

        protected Dictionary<Guid, PSSession> GetMatchingRunspacesByName(bool writeobject, bool writeErrorOnNoMatch)
        {
            Dictionary<Guid, PSSession> dictionary = new Dictionary<Guid, PSSession>();
            List<PSSession> runspaces = base.RunspaceRepository.Runspaces;
            foreach (string str in this.names)
            {
                WildcardPattern pattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
                bool flag = false;
                foreach (PSSession session in runspaces)
                {
                    if (pattern.IsMatch(session.Name))
                    {
                        flag = true;
                        if (writeobject)
                        {
                            base.WriteObject(session);
                        }
                        else
                        {
                            try
                            {
                                dictionary.Add(session.InstanceId, session);
                            }
                            catch (ArgumentException)
                            {
                            }
                        }
                    }
                }
                if ((!flag && writeErrorOnNoMatch) && !WildcardPattern.ContainsWildcardCharacters(str))
                {
                    this.WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedName, RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedName, str);
                }
            }
            return dictionary;
        }

        protected Dictionary<Guid, PSSession> GetMatchingRunspacesByRunspaceId(bool writeobject, bool writeErrorOnNoMatch)
        {
            Dictionary<Guid, PSSession> dictionary = new Dictionary<Guid, PSSession>();
            List<PSSession> runspaces = base.RunspaceRepository.Runspaces;
            foreach (Guid guid in this.remoteRunspaceIds)
            {
                bool flag = false;
                foreach (PSSession session in runspaces)
                {
                    if (guid.Equals(session.InstanceId))
                    {
                        flag = true;
                        if (writeobject)
                        {
                            base.WriteObject(session);
                        }
                        else
                        {
                            try
                            {
                                dictionary.Add(session.InstanceId, session);
                            }
                            catch (ArgumentException)
                            {
                            }
                        }
                    }
                }
                if (!flag && writeErrorOnNoMatch)
                {
                    this.WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedRunspaceId, RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedRunspaceId, guid);
                }
            }
            return dictionary;
        }

        private Dictionary<Guid, PSSession> GetMatchingRunspacesBySessionId(bool writeobject, bool writeErrorOnNoMatch)
        {
            Dictionary<Guid, PSSession> dictionary = new Dictionary<Guid, PSSession>();
            List<PSSession> runspaces = base.RunspaceRepository.Runspaces;
            foreach (int num in this.sessionIds)
            {
                bool flag = false;
                foreach (PSSession session in runspaces)
                {
                    if (num == session.Id)
                    {
                        flag = true;
                        if (writeobject)
                        {
                            base.WriteObject(session);
                        }
                        else
                        {
                            try
                            {
                                dictionary.Add(session.InstanceId, session);
                            }
                            catch (ArgumentException)
                            {
                            }
                        }
                    }
                }
                if (!flag && writeErrorOnNoMatch)
                {
                    this.WriteInvalidArgumentError(PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedSessionId, RemotingErrorIdStrings.RemoteRunspaceNotAvailableForSpecifiedSessionId, num);
                }
            }
            return dictionary;
        }

        private void WriteInvalidArgumentError(PSRemotingErrorId errorId, string resourceString, object errorArgument)
        {
            string message = base.GetMessage(resourceString, new object[] { errorArgument });
            base.WriteError(new ErrorRecord(new ArgumentException(message), errorId.ToString(), ErrorCategory.InvalidArgument, errorArgument));
        }

        [ValidateNotNullOrEmpty, Alias(new string[] { "Cn" }), Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName")]
        public virtual string[] ComputerName
        {
            get
            {
                return this.computerNames;
            }
            set
            {
                this.computerNames = value;
            }
        }

        [ValidateNotNull, Parameter(Position=0, ValueFromPipelineByPropertyName=true, Mandatory=true, ParameterSetName="Id")]
        public int[] Id
        {
            get
            {
                return this.sessionIds;
            }
            set
            {
                this.sessionIds = value;
            }
        }

        [Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="InstanceId"), ValidateNotNull]
        public virtual Guid[] InstanceId
        {
            get
            {
                return this.remoteRunspaceIds;
            }
            set
            {
                this.remoteRunspaceIds = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="Name")]
        public virtual string[] Name
        {
            get
            {
                return this.names;
            }
            set
            {
                this.names = value;
            }
        }
    }
}

