namespace System.Management.Automation.Runspaces
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Net;
    using System.Text;

    public sealed class PowerShellProcessInstance : IDisposable
    {
        private bool _isDisposed;
        private System.Diagnostics.Process _process;
        private bool _processExited;
        private System.Management.Automation.Runspaces.RunspacePool _runspacePool;
        private bool _started;
        private readonly ProcessStartInfo _startInfo;
        private readonly object _syncObject;
        private OutOfProcessTextWriter _textWriter;
        private static readonly string PSExePath = Path.Combine(Utils.GetApplicationBase(Utils.DefaultPowerShellShellID), "powershell.exe");

        public PowerShellProcessInstance() : this(null, null, null, false)
        {
        }

        public PowerShellProcessInstance(Version powerShellVersion, PSCredential credential, ScriptBlock initializationScript, bool useWow64)
        {
            this._syncObject = new object();
            string pSExePath = PSExePath;
            if (useWow64)
            {
                string environmentVariable = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                if (!string.IsNullOrEmpty(environmentVariable) && (environmentVariable.Equals("amd64", StringComparison.OrdinalIgnoreCase) || environmentVariable.Equals("ia64", StringComparison.OrdinalIgnoreCase)))
                {
                    pSExePath = PSExePath.ToLowerInvariant().Replace(@"\system32\", @"\syswow64\");
                    if (!System.IO.File.Exists(pSExePath))
                    {
                        throw new PSInvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.IPCWowComponentNotPresent, new object[] { pSExePath }));
                    }
                }
            }
            string str4 = string.Empty;
            Version version = powerShellVersion ?? PSVersionInfo.PSVersion;
            if (null == version)
            {
                version = new Version(3, 0);
            }
            str4 = string.Format(CultureInfo.InvariantCulture, "-Version {0}", new object[] { new Version(version.Major, version.Minor) });
            str4 = string.Format(CultureInfo.InvariantCulture, "{0} -s -NoLogo -NoProfile", new object[] { str4 });
            if (initializationScript != null)
            {
                string str5 = initializationScript.ToString();
                if (!string.IsNullOrEmpty(str5))
                {
                    string str6 = Convert.ToBase64String(Encoding.Unicode.GetBytes(str5));
                    str4 = string.Format(CultureInfo.InvariantCulture, "{0} -EncodedCommand {1}", new object[] { str4, str6 });
                }
            }
            ProcessStartInfo info = new ProcessStartInfo {
                FileName = useWow64 ? pSExePath : PSExePath,
                Arguments = str4,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                LoadUserProfile = true
            };
            this._startInfo = info;
            if (credential != null)
            {
                NetworkCredential networkCredential = credential.GetNetworkCredential();
                this._startInfo.UserName = networkCredential.UserName;
                this._startInfo.Domain = string.IsNullOrEmpty(networkCredential.Domain) ? "." : networkCredential.Domain;
                this._startInfo.Password = credential.Password;
            }
            System.Diagnostics.Process process = new System.Diagnostics.Process {
                StartInfo = this._startInfo,
                EnableRaisingEvents = true
            };
            this._process = process;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this._isDisposed)
            {
                lock (this._syncObject)
                {
                    if (this._isDisposed)
                    {
                        return;
                    }
                    this._isDisposed = true;
                }
                if (disposing)
                {
                    try
                    {
                        if ((this._process != null) && !this._process.HasExited)
                        {
                            this._process.Kill();
                        }
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    catch (Win32Exception)
                    {
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
            }
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            lock (this._syncObject)
            {
                this._processExited = true;
            }
        }

        internal void Start()
        {
            if (this.HasExited)
            {
                throw new InvalidOperationException();
            }
            lock (this._syncObject)
            {
                if (this._started)
                {
                    return;
                }
                this._started = true;
                this._process.Exited += new EventHandler(this.ProcessExited);
            }
            this._process.Start();
        }

        public bool HasExited
        {
            get
            {
                return (this._processExited || ((this._started && (this._process != null)) && this._process.HasExited));
            }
        }

        public System.Diagnostics.Process Process
        {
            get
            {
                return this._process;
            }
        }

        internal System.Management.Automation.Runspaces.RunspacePool RunspacePool
        {
            get
            {
                lock (this._syncObject)
                {
                    return this._runspacePool;
                }
            }
            set
            {
                lock (this._syncObject)
                {
                    this._runspacePool = value;
                }
            }
        }

        internal OutOfProcessTextWriter StdInWriter
        {
            get
            {
                return this._textWriter;
            }
            set
            {
                this._textWriter = value;
            }
        }
    }
}

