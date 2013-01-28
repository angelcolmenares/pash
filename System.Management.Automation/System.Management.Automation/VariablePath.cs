namespace System.Management.Automation
{
    using System;

    public class VariablePath
    {
        private VariablePathFlags _flags;
        private string _unqualifiedPath;
        private string _userPath;

        private VariablePath()
        {
        }

        public VariablePath(string path) : this(path, VariablePathFlags.None)
        {
        }

        internal VariablePath(string path, VariablePathFlags knownFlags)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            this._userPath = path;
            this._flags = knownFlags;
            string str = null;
            string str2 = null;
            VariablePathFlags unqualified = VariablePathFlags.Unqualified;
            int startIndex = 0;
            int index = -1;
        Label_0036:
            switch (path[0])
            {
                case 'P':
                case 'p':
                    str = "rivate";
                    str2 = "RIVATE";
                    unqualified = VariablePathFlags.Private;
                    break;

                case 'S':
                case 's':
                    str = "cript";
                    str2 = "CRIPT";
                    unqualified = VariablePathFlags.Script;
                    break;

                case 'V':
                case 'v':
                    if (knownFlags == VariablePathFlags.None)
                    {
                        str = "ariable";
                        str2 = "ARIABLE";
                        unqualified = VariablePathFlags.Variable;
                    }
                    break;

                case 'G':
                case 'g':
                    str = "lobal";
                    str2 = "LOBAL";
                    unqualified = VariablePathFlags.Global;
                    break;

                case 'L':
                case 'l':
                    str = "ocal";
                    str2 = "OCAL";
                    unqualified = VariablePathFlags.Local;
                    break;
            }
            if (str != null)
            {
                startIndex++;
                int num3 = 0;
                while ((startIndex < path.Length) && (num3 < str.Length))
                {
                    if ((path[startIndex] != str[num3]) && (path[startIndex] != str2[num3]))
                    {
                        break;
                    }
                    num3++;
                    startIndex++;
                }
                if (((num3 == str.Length) && (startIndex < path.Length)) && (path[startIndex] == ':'))
                {
                    if (this._flags == VariablePathFlags.None)
                    {
                        this._flags = VariablePathFlags.Variable;
                    }
                    this._flags |= unqualified;
                    index = startIndex;
                    startIndex++;
                    if (unqualified == VariablePathFlags.Variable)
                    {
                        knownFlags = VariablePathFlags.Variable;
                        str = (string) (str2 = null);
                        unqualified = VariablePathFlags.None;
                        goto Label_0036;
                    }
                }
            }
            if (this._flags == VariablePathFlags.None)
            {
                index = path.IndexOf(':', startIndex);
                if (index > 0)
                {
                    this._flags = VariablePathFlags.DriveQualified;
                }
            }
            if (index == -1)
            {
                this._unqualifiedPath = this._userPath;
            }
            else
            {
                this._unqualifiedPath = this._userPath.Substring(index + 1);
            }
            if (this._flags == VariablePathFlags.None)
            {
                this._flags = VariablePathFlags.Unqualified | VariablePathFlags.Variable;
            }
        }

        internal VariablePath CloneAndSetLocal()
        {
            return new VariablePath { _userPath = this._userPath, _unqualifiedPath = this._unqualifiedPath, _flags = VariablePathFlags.Variable | VariablePathFlags.Local };
        }

        public override string ToString()
        {
            return this._userPath;
        }

        public string DriveName
        {
            get
            {
                if (!this.IsDriveQualified)
                {
                    return null;
                }
                return this._userPath.Substring(0, this._userPath.IndexOf(':'));
            }
        }

        public bool IsDriveQualified
        {
            get
            {
                return (VariablePathFlags.None != (this._flags & VariablePathFlags.DriveQualified));
            }
        }

        internal bool IsFunction
        {
            get
            {
                return (VariablePathFlags.None != (this._flags & VariablePathFlags.Function));
            }
        }

        public bool IsGlobal
        {
            get
            {
                return (VariablePathFlags.None != (this._flags & VariablePathFlags.Global));
            }
        }

        public bool IsLocal
        {
            get
            {
                return (VariablePathFlags.None != (this._flags & VariablePathFlags.Local));
            }
        }

        public bool IsPrivate
        {
            get
            {
                return (VariablePathFlags.None != (this._flags & VariablePathFlags.Private));
            }
        }

        public bool IsScript
        {
            get
            {
                return (VariablePathFlags.None != (this._flags & VariablePathFlags.Script));
            }
        }

        public bool IsUnqualified
        {
            get
            {
                return (VariablePathFlags.None != (this._flags & VariablePathFlags.Unqualified));
            }
        }

        public bool IsUnscopedVariable
        {
            get
            {
                return (VariablePathFlags.None == (this._flags & VariablePathFlags.UnscopedVariableMask));
            }
        }

        public bool IsVariable
        {
            get
            {
                return (VariablePathFlags.None != (this._flags & VariablePathFlags.Variable));
            }
        }

        internal string QualifiedName
        {
            get
            {
                if (!this.IsDriveQualified)
                {
                    return this._unqualifiedPath;
                }
                return this._userPath;
            }
        }

        internal string UnqualifiedPath
        {
            get
            {
                return this._unqualifiedPath;
            }
        }

        public string UserPath
        {
            get
            {
                return this._userPath;
            }
        }
    }
}

