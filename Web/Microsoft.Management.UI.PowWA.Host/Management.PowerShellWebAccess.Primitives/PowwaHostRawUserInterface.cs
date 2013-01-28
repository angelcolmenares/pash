using System;
using System.Globalization;
using System.Management.Automation.Host;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	internal class PowwaHostRawUserInterface : PSHostRawUserInterface, IMessageCreated
	{
		public readonly static Size DefaultBufferSizeDesktop;

		public readonly static Size MaxBufferSizeDesktop;

		public readonly static Size MinBufferSizeDesktop;

		public readonly static Size DefaultWindowSizeDesktop;

		public readonly static Size MaxWindowSizeDesktop;

		public readonly static Size MinWindowSizeDesktop;

		public readonly static Size DefaultBufferSizeMobile;

		public readonly static Size MaxBufferSizeMobile;

		public readonly static Size MinBufferSizeMobile;

		public readonly static Size DefaultWindowSizeMobile;

		public readonly static Size MaxWindowSizeMobile;

		public readonly static Size MinWindowSizeMobile;

		private readonly Size maxBufferSize;

		private readonly Size minBufferSize;

		private readonly Size maxWindowSize;

		private readonly Size minWindowSize;

		private Size bufferSize;

		private Size windowSize;

		private ConsoleColor backgroundColor;

		private ConsoleColor foregroundColor;

		private string windowTitle;

		public override ConsoleColor BackgroundColor
		{
			get
			{
				return this.backgroundColor;
			}
			set
			{
				this.backgroundColor = value;
				this.OnMessageCreated(new MessageCreatedEventArgs(new SetBackgroundColorMessage(this.backgroundColor), false));
			}
		}

		public override Size BufferSize
		{
			get
			{
				return this.bufferSize;
			}
			set
			{
				Size minBufferSize = this.MinBufferSize;
				if (value.Width >= minBufferSize.Width)
				{
					Size maxBufferSize = this.MaxBufferSize;
					if (value.Width <= maxBufferSize.Width)
					{
						Size size = this.MinBufferSize;
						if (value.Height >= size.Height)
						{
							Size maxBufferSize1 = this.MaxBufferSize;
							if (value.Height <= maxBufferSize1.Height)
							{
								Size windowSize = this.WindowSize;
								if (value.Width >= windowSize.Width)
								{
									Size windowSize1 = this.WindowSize;
									if (value.Height >= windowSize1.Height)
									{
										this.bufferSize = value;
										this.OnMessageCreated(new MessageCreatedEventArgs(new SetBufferSizeMessage(this.bufferSize), false));
										return;
									}
								}
								PowwaEvents.PowwaEVENT_DEBUG_LOG0(string.Concat("SetBufferSize(): ", Resources.BufferSmallerThanWindow));
								throw new ArgumentOutOfRangeException("value", Resources.BufferSmallerThanWindow);
							}
						}
						int height = value.Height;
						PowwaEvents.PowwaEVENT_DEBUG_LOG1("SetBufferSize(): Invalid Height.", "New Height", height.ToString(CultureInfo.InvariantCulture));
						object[] objArray = new object[2];
						Size minWindowSize = this.MinWindowSize;
						objArray[0] = minWindowSize.Height;
						Size maxWindowSize = this.MaxWindowSize;
						objArray[1] = maxWindowSize.Height;
						string str = string.Format(CultureInfo.CurrentCulture, Resources.BufferHeightOutOfRange_Format, objArray);
						throw new ArgumentOutOfRangeException("value", str);
					}
				}
				int width = value.Width;
				PowwaEvents.PowwaEVENT_DEBUG_LOG1("SetBufferSize(): Invalid Width.", "New Width", width.ToString(CultureInfo.InvariantCulture));
				object[] width1 = new object[2];
				Size minBufferSize1 = this.MinBufferSize;
				width1[0] = minBufferSize1.Width;
				Size size1 = this.MaxBufferSize;
				width1[1] = size1.Width;
				string str1 = string.Format(CultureInfo.CurrentCulture, Resources.BufferWidthOutOfRange_Format, width1);
				throw new ArgumentOutOfRangeException("value", str1);
			}
		}

		public override void Clear (int code)
		{

		}

		public override Coordinates CursorPosition
		{
			get;
			set;
		}

		public override int CursorSize
		{
			get
			{
				throw new NotSupportedException(Resources.PSHostRawUserInterfaceCursorSizeNotSupported);
			}
			set
			{
				throw new NotSupportedException(Resources.PSHostRawUserInterfaceCursorSizeNotSupported);
			}
		}

		public override ConsoleColor ForegroundColor
		{
			get
			{
				return this.foregroundColor;
			}
			set
			{
				this.foregroundColor = value;
				this.OnMessageCreated(new MessageCreatedEventArgs(new SetForegroundColorMessage(this.foregroundColor), false));
			}
		}

		public override bool KeyAvailable
		{
			get
			{
				throw new NotSupportedException(Resources.PSHostRawUserInterfaceKeyAvailableNotSupported);
			}
		}

		public Size MaxBufferSize
		{
			get
			{
				return this.maxBufferSize;
			}
		}

		public override Size MaxPhysicalWindowSize
		{
			get
			{
				throw new NotSupportedException(Resources.PSHostRawUserInterfaceMaxPhysicalWindowSizeNotSupported);
			}
		}

		public override Size MaxWindowSize
		{
			get
			{
				return this.maxWindowSize;
			}
		}

		public Size MinBufferSize
		{
			get
			{
				return this.minBufferSize;
			}
		}

		public Size MinWindowSize
		{
			get
			{
				return this.minWindowSize;
			}
		}

		public override Coordinates WindowPosition
		{
			get
			{
				throw new NotSupportedException(Resources.PSHostRawUserInterfaceWindowPositionNotSupported);
			}
			set
			{
				throw new NotSupportedException(Resources.PSHostRawUserInterfaceWindowPositionNotSupported);
			}
		}

		public override Size WindowSize
		{
			get
			{
				return this.windowSize;
			}
			set
			{
				Size minWindowSize = this.MinWindowSize;
				if (value.Width >= minWindowSize.Width)
				{
					Size maxWindowSize = this.MaxWindowSize;
					if (value.Width <= maxWindowSize.Width)
					{
						Size size = this.MinWindowSize;
						if (value.Height >= size.Height)
						{
							Size maxWindowSize1 = this.MaxWindowSize;
							if (value.Height <= maxWindowSize1.Height)
							{
								Size bufferSize = this.BufferSize;
								if (value.Width <= bufferSize.Width)
								{
									Size bufferSize1 = this.BufferSize;
									if (value.Height <= bufferSize1.Height)
									{
										this.windowSize = value;
										this.OnMessageCreated(new MessageCreatedEventArgs(new SetWindowSizeMessage(this.windowSize), false));
										return;
									}
								}
								PowwaEvents.PowwaEVENT_DEBUG_LOG0(string.Concat("SetWindowSize(): ", Resources.WindowLargerThanBuffer));
								throw new ArgumentOutOfRangeException("value", Resources.WindowLargerThanBuffer);
							}
						}
						int height = value.Height;
						PowwaEvents.PowwaEVENT_DEBUG_LOG1("SetWindowSize(): Invalid Height", "New Height", height.ToString(CultureInfo.InvariantCulture));
						object[] objArray = new object[2];
						Size minWindowSize1 = this.MinWindowSize;
						objArray[0] = minWindowSize1.Height;
						Size size1 = this.MaxWindowSize;
						objArray[1] = size1.Height;
						string str = string.Format(CultureInfo.CurrentCulture, Resources.WindowHeightOutOfRange_Format, objArray);
						throw new ArgumentOutOfRangeException("value", str);
					}
				}
				int width = value.Width;
				PowwaEvents.PowwaEVENT_DEBUG_LOG1("SetWindowSize(): Invalid Width", "New Width", width.ToString(CultureInfo.InvariantCulture));
				object[] width1 = new object[2];
				Size minWindowSize2 = this.MinWindowSize;
				width1[0] = minWindowSize2.Width;
				Size maxWindowSize2 = this.MaxWindowSize;
				width1[1] = maxWindowSize2.Width;
				string str1 = string.Format(CultureInfo.CurrentCulture, Resources.WindowWidthOutOfRange_Format, width1);
				throw new ArgumentOutOfRangeException("value", str1);
			}
		}

		public override string WindowTitle
		{
			get
			{
				return this.windowTitle;
			}
			set
			{
				this.windowTitle = value;
				this.OnMessageCreated(new MessageCreatedEventArgs(new SetWindowTitleMessage(this.windowTitle), false));
			}
		}

		static PowwaHostRawUserInterface()
		{
			PowwaHostRawUserInterface.DefaultBufferSizeDesktop = new Size(120, 0x12c);
			PowwaHostRawUserInterface.MaxBufferSizeDesktop = new Size(250, 0x3e7);
			PowwaHostRawUserInterface.MinBufferSizeDesktop = new Size(20, 14);
			PowwaHostRawUserInterface.DefaultWindowSizeDesktop = new Size(120, 35);
			PowwaHostRawUserInterface.MaxWindowSizeDesktop = new Size(250, 0x3e7);
			PowwaHostRawUserInterface.MinWindowSizeDesktop = new Size(20, 14);
			PowwaHostRawUserInterface.DefaultBufferSizeMobile = new Size(80, 100);
			PowwaHostRawUserInterface.MaxBufferSizeMobile = new Size(250, 0x3e7);
			PowwaHostRawUserInterface.MinBufferSizeMobile = new Size(20, 14);
			PowwaHostRawUserInterface.DefaultWindowSizeMobile = new Size(80, 25);
			PowwaHostRawUserInterface.MaxWindowSizeMobile = new Size(250, 0x3e7);
			PowwaHostRawUserInterface.MinWindowSizeMobile = new Size(20, 14);
		}

		public PowwaHostRawUserInterface(ClientInfo clientInfo)
		{
			this.backgroundColor = ConsoleColor.DarkBlue;
			this.foregroundColor = ConsoleColor.White;
			this.windowTitle = string.Empty;
			if (!HtmlHelper.IsMobileBrowser(clientInfo.Agent))
			{
				this.bufferSize = PowwaHostRawUserInterface.DefaultBufferSizeDesktop;
				this.maxBufferSize = PowwaHostRawUserInterface.MaxBufferSizeDesktop;
				this.minBufferSize = PowwaHostRawUserInterface.MinBufferSizeDesktop;
				this.windowSize = PowwaHostRawUserInterface.DefaultWindowSizeDesktop;
				this.maxWindowSize = PowwaHostRawUserInterface.MaxWindowSizeDesktop;
				this.minWindowSize = PowwaHostRawUserInterface.MinWindowSizeDesktop;
				return;
			}
			else
			{
				this.bufferSize = PowwaHostRawUserInterface.DefaultBufferSizeMobile;
				this.maxBufferSize = PowwaHostRawUserInterface.MaxBufferSizeMobile;
				this.minBufferSize = PowwaHostRawUserInterface.MinBufferSizeMobile;
				this.windowSize = PowwaHostRawUserInterface.DefaultWindowSizeMobile;
				this.maxWindowSize = PowwaHostRawUserInterface.MaxWindowSizeMobile;
				this.minWindowSize = PowwaHostRawUserInterface.MinWindowSizeMobile;
				return;
			}
		}

		public override void FlushInputBuffer()
		{
			throw new NotSupportedException(Resources.PSHostRawUserInterfaceFlushInputBufferNotSupported);
		}

		public override BufferCell[,] GetBufferContents(Rectangle rectangle)
		{
			throw new NotSupportedException(Resources.PSHostRawUserInterfaceGetBufferContentsNotSupported);
		}

		private void OnMessageCreated(MessageCreatedEventArgs e)
		{
			EventHandler<MessageCreatedEventArgs> eventHandler = this.MessageCreated;
			if (eventHandler != null)
			{
				eventHandler(this, e);
			}
		}

		public override KeyInfo ReadKey(ReadKeyOptions options)
		{
			throw new NotSupportedException(Resources.PSHostRawUserInterfaceReadKeyNotSupported);
		}

		public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
		{
			throw new NotSupportedException(Resources.PSHostRawUserInterfaceScrollBufferContentsNotSupported);
		}

		public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
		{
			this.OnMessageCreated(new MessageCreatedEventArgs(new ClearBufferMessage(), false));
		}

		public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
		{
			throw new NotSupportedException(Resources.PSHostRawUserInterfaceSetBufferContentsNotSupported);
		}

		public event EventHandler<MessageCreatedEventArgs> MessageCreated;
	}
}