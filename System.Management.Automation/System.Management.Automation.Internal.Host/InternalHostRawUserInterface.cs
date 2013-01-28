namespace System.Management.Automation.Internal.Host
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Runspaces;

    internal class InternalHostRawUserInterface : PSHostRawUserInterface
    {
        private PSHostRawUserInterface externalRawUI;
        private InternalHost parentHost;

        internal InternalHostRawUserInterface(PSHostRawUserInterface externalRawUI, InternalHost parentHost)
        {
            this.externalRawUI = externalRawUI;
            this.parentHost = parentHost;
        }

		public override void Clear (int code)
		{
			if (this.externalRawUI == null)
			{
				this.ThrowNotInteractive();
			}
			this.externalRawUI.Clear(code);
		}

        public override void FlushInputBuffer()
        {
            if (this.externalRawUI == null)
            {
                this.ThrowNotInteractive();
            }
            this.externalRawUI.FlushInputBuffer();
        }

        public override BufferCell[,] GetBufferContents(Rectangle r)
        {
            if (this.externalRawUI == null)
            {
                this.ThrowNotInteractive();
            }
            return this.externalRawUI.GetBufferContents(r);
        }

        public override int LengthInBufferCells(char character)
        {
            if (this.externalRawUI == null)
            {
                this.ThrowNotInteractive();
            }
            return this.externalRawUI.LengthInBufferCells(character);
        }

        public override int LengthInBufferCells(string str)
        {
            if (this.externalRawUI == null)
            {
                this.ThrowNotInteractive();
            }
            return this.externalRawUI.LengthInBufferCells(str);
        }

        public override int LengthInBufferCells(string str, int offset)
        {
            if (this.externalRawUI == null)
            {
                this.ThrowNotInteractive();
            }
            return this.externalRawUI.LengthInBufferCells(str, offset);
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            if (this.externalRawUI == null)
            {
                this.ThrowNotInteractive();
            }
            KeyInfo info = new KeyInfo();
            try
            {
                info = this.externalRawUI.ReadKey(options);
            }
            catch (PipelineStoppedException)
            {
                LocalPipeline currentlyRunningPipeline = (LocalPipeline) ((RunspaceBase) this.parentHost.Context.CurrentRunspace).GetCurrentlyRunningPipeline();
                if (currentlyRunningPipeline == null)
                {
                    throw;
                }
                currentlyRunningPipeline.Stopper.Stop();
            }
            return info;
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            if (this.externalRawUI == null)
            {
                this.ThrowNotInteractive();
            }
            this.externalRawUI.ScrollBufferContents(source, destination, clip, fill);
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            if (this.externalRawUI == null)
            {
                this.ThrowNotInteractive();
            }
            this.externalRawUI.SetBufferContents(origin, contents);
        }

        public override void SetBufferContents(Rectangle r, BufferCell fill)
        {
            if (this.externalRawUI == null)
            {
                this.ThrowNotInteractive();
            }
            this.externalRawUI.SetBufferContents(r, fill);
        }

        internal void ThrowNotInteractive()
        {
            HostException exception = new HostException(HostInterfaceExceptionsStrings.HostFunctionNotImplemented, null, "HostFunctionNotImplemented", ErrorCategory.NotImplemented);
            throw exception;
        }

        public override ConsoleColor BackgroundColor
        {
            get
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                return this.externalRawUI.BackgroundColor;
            }
            set
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                this.externalRawUI.BackgroundColor = value;
            }
        }

        public override Size BufferSize
        {
            get
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                return this.externalRawUI.BufferSize;
            }
            set
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                this.externalRawUI.BufferSize = value;
            }
        }

        public override Coordinates CursorPosition
        {
            get
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                return this.externalRawUI.CursorPosition;
            }
            set
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                this.externalRawUI.CursorPosition = value;
            }
        }

        public override int CursorSize
        {
            get
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                return this.externalRawUI.CursorSize;
            }
            set
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                this.externalRawUI.CursorSize = value;
            }
        }

        public override ConsoleColor ForegroundColor
        {
            get
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                return this.externalRawUI.ForegroundColor;
            }
            set
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                this.externalRawUI.ForegroundColor = value;
            }
        }

        public override bool KeyAvailable
        {
            get
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                return this.externalRawUI.KeyAvailable;
            }
        }

        public override Size MaxPhysicalWindowSize
        {
            get
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                return this.externalRawUI.MaxPhysicalWindowSize;
            }
        }

        public override Size MaxWindowSize
        {
            get
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                return this.externalRawUI.MaxWindowSize;
            }
        }

        public override Coordinates WindowPosition
        {
            get
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                return this.externalRawUI.WindowPosition;
            }
            set
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                this.externalRawUI.WindowPosition = value;
            }
        }

        public override Size WindowSize
        {
            get
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                return this.externalRawUI.WindowSize;
            }
            set
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                this.externalRawUI.WindowSize = value;
            }
        }

        public override string WindowTitle
        {
            get
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                return this.externalRawUI.WindowTitle;
            }
            set
            {
                if (this.externalRawUI == null)
                {
                    this.ThrowNotInteractive();
                }
                this.externalRawUI.WindowTitle = value;
            }
        }
    }
}

