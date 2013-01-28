namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation.Host;

    internal class ServerRemoteHostRawUserInterface : PSHostRawUserInterface
    {
        private ServerRemoteHostUserInterface _remoteHostUserInterface;
        private ServerMethodExecutor _serverMethodExecutor;

        internal ServerRemoteHostRawUserInterface(ServerRemoteHostUserInterface remoteHostUserInterface)
        {
            this._remoteHostUserInterface = remoteHostUserInterface;
            this._serverMethodExecutor = remoteHostUserInterface.ServerRemoteHost.ServerMethodExecutor;
        }

		public override void Clear (int code)
		{

		}

        public override void FlushInputBuffer()
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.FlushInputBuffer);
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetBufferContents);
        }

        public override int LengthInBufferCells(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return source.Length;
        }

        public override int LengthInBufferCells(string source, int offset)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return (source.Length - offset);
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            return this._serverMethodExecutor.ExecuteMethod<KeyInfo>(RemoteHostMethodId.ReadKey, new object[] { options });
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.ScrollBufferContents, new object[] { source, destination, clip, fill });
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetBufferContents2, new object[] { origin, contents });
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetBufferContents1, new object[] { rectangle, fill });
        }

        public override ConsoleColor BackgroundColor
        {
            get
            {
                if (!this.HostDefaultData.HasValue(HostDefaultDataId.BackgroundColor))
                {
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetBackgroundColor);
                }
                return (ConsoleColor) this.HostDefaultData.GetValue(HostDefaultDataId.BackgroundColor);
            }
            set
            {
                this.HostDefaultData.SetValue(HostDefaultDataId.BackgroundColor, value);
                this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetBackgroundColor, new object[] { value });
            }
        }

        public override Size BufferSize
        {
            get
            {
                if (!this.HostDefaultData.HasValue(HostDefaultDataId.BufferSize))
                {
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetBufferSize);
                }
                return (Size) this.HostDefaultData.GetValue(HostDefaultDataId.BufferSize);
            }
            set
            {
                this.HostDefaultData.SetValue(HostDefaultDataId.BufferSize, value);
                this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetBufferSize, new object[] { value });
            }
        }

        public override Coordinates CursorPosition
        {
            get
            {
                if (!this.HostDefaultData.HasValue(HostDefaultDataId.CursorPosition))
                {
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetCursorPosition);
                }
                return (Coordinates) this.HostDefaultData.GetValue(HostDefaultDataId.CursorPosition);
            }
            set
            {
                this.HostDefaultData.SetValue(HostDefaultDataId.CursorPosition, value);
                this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetCursorPosition, new object[] { value });
            }
        }

        public override int CursorSize
        {
            get
            {
                if (!this.HostDefaultData.HasValue(HostDefaultDataId.CursorSize))
                {
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetCursorSize);
                }
                return (int) this.HostDefaultData.GetValue(HostDefaultDataId.CursorSize);
            }
            set
            {
                this.HostDefaultData.SetValue(HostDefaultDataId.CursorSize, value);
                this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetCursorSize, new object[] { value });
            }
        }

        public override ConsoleColor ForegroundColor
        {
            get
            {
                if (!this.HostDefaultData.HasValue(HostDefaultDataId.ForegroundColor))
                {
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetForegroundColor);
                }
                return (ConsoleColor) this.HostDefaultData.GetValue(HostDefaultDataId.ForegroundColor);
            }
            set
            {
                this.HostDefaultData.SetValue(HostDefaultDataId.ForegroundColor, value);
                this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetForegroundColor, new object[] { value });
            }
        }

        private System.Management.Automation.Remoting.HostDefaultData HostDefaultData
        {
            get
            {
                return this._remoteHostUserInterface.ServerRemoteHost.HostInfo.HostDefaultData;
            }
        }

        public override bool KeyAvailable
        {
            get
            {
                throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetKeyAvailable);
            }
        }

        public override Size MaxPhysicalWindowSize
        {
            get
            {
                if (!this.HostDefaultData.HasValue(HostDefaultDataId.MaxPhysicalWindowSize))
                {
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetMaxPhysicalWindowSize);
                }
                return (Size) this.HostDefaultData.GetValue(HostDefaultDataId.MaxPhysicalWindowSize);
            }
        }

        public override Size MaxWindowSize
        {
            get
            {
                if (!this.HostDefaultData.HasValue(HostDefaultDataId.MaxWindowSize))
                {
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetMaxWindowSize);
                }
                return (Size) this.HostDefaultData.GetValue(HostDefaultDataId.MaxWindowSize);
            }
        }

        public override Coordinates WindowPosition
        {
            get
            {
                if (!this.HostDefaultData.HasValue(HostDefaultDataId.WindowPosition))
                {
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetWindowPosition);
                }
                return (Coordinates) this.HostDefaultData.GetValue(HostDefaultDataId.WindowPosition);
            }
            set
            {
                this.HostDefaultData.SetValue(HostDefaultDataId.WindowPosition, value);
                this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetWindowPosition, new object[] { value });
            }
        }

        public override Size WindowSize
        {
            get
            {
                if (!this.HostDefaultData.HasValue(HostDefaultDataId.WindowSize))
                {
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetWindowSize);
                }
                return (Size) this.HostDefaultData.GetValue(HostDefaultDataId.WindowSize);
            }
            set
            {
                this.HostDefaultData.SetValue(HostDefaultDataId.WindowSize, value);
                this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetWindowSize, new object[] { value });
            }
        }

        public override string WindowTitle
        {
            get
            {
                if (!this.HostDefaultData.HasValue(HostDefaultDataId.WindowTitle))
                {
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetWindowTitle);
                }
                return (string) this.HostDefaultData.GetValue(HostDefaultDataId.WindowTitle);
            }
            set
            {
                this.HostDefaultData.SetValue(HostDefaultDataId.WindowTitle, value);
                this._serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetWindowTitle, new object[] { value });
            }
        }
    }
}

