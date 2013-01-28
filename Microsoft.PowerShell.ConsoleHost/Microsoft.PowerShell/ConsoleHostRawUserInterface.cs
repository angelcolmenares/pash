using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Microsoft.PowerShell
{
	internal sealed class ConsoleHostRawUserInterface : PSHostRawUserInterface
	{
		private const string StringsBaseName = "ConsoleHostRawUserInterfaceStrings";

		private const string InvalidConsoleColorErrorResource = "InvalidConsoleColorError";

		private const string InvalidCursorSizeErrorResource = "InvalidCursorSizeError";

		private const string InvalidXWindowPositionErrorResource = "InvalidXWindowPositionError";

		private const string InvalidYWindowPositionErrorResource = "InvalidYWindowPositionError";

		private const string InvalidBufferSizeErrorResource = "InvalidBufferSizeError";

		private const string WindowWidthTooSmallErrorResource = "WindowWidthTooSmallError";

		private const string WindowHeightTooSmallErrorResource = "WindowHeightTooSmallError";

		private const string WindowWidthLargerThanBufferErrorResource = "WindowWidthLargerThanBufferError";

		private const string WindowHeightLargerThanBufferErrorResource = "WindowHeightLargerThanBufferError";

		private const string WindowWidthTooLargeErrorTemplateResource = "WindowWidthTooLargeErrorTemplate";

		private const string WindowHeightTooLargeErrorTemplateResource = "WindowHeightTooLargeErrorTemplate";

		private const string WindowTooNarrowErrorResource = "WindowTooNarrowError";

		private const string WindowTooShortErrorResource = "WindowTooShortError";

		private const string InvalidReadKeyOptionsErrorResource = "InvalidReadKeyOptionsError";

		private const string WindowTitleTooShortErrorResource = "WindowTitleTooShortError";

		private const string WindowTitleTooLongErrorTemplateResource = "WindowTitleTooLongErrorTemplate";

		private const string InvalidRegionErrorTemplateResource = "InvalidRegionErrorTemplate";

		private const string CoordinateOutOfBufferErrorTemplateResource = "CoordinateOutOfBufferErrorTemplate";

		private ConsoleColor defaultForeground;

		private ConsoleColor defaultBackground;

		private ConsoleHostUserInterface parent;

		private ConsoleControl.KEY_EVENT_RECORD cachedKeyEvent;

		[TraceSource("ConsoleHostRawUserInterface", "Console host's subclass of S.M.A.Host.RawConsole")]
		private static PSTraceSource tracer;

		public override ConsoleColor BackgroundColor
		{
			get
			{
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
				ConsoleColor consoleColor = ConsoleColor.Black;
				ConsoleColor consoleColor1 = ConsoleColor.Black;
				ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
				ConsoleControl.WORDToColor(cONSOLESCREENBUFFERINFO.Attributes, out consoleColor1, out consoleColor);
				return consoleColor;
			}
			set
			{
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
				if (!ConsoleControl.IsConsoleColor(value))
				{
					throw PSTraceSource.NewArgumentException("value", "ConsoleHostRawUserInterfaceStrings", "InvalidConsoleColorError", new object[0]);
				}
				else
				{
					SafeFileHandle bufferInfo = ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
					short attributes = (short)cONSOLESCREENBUFFERINFO.Attributes;
					attributes = (short)(attributes & -241);
					attributes = (short)((ushort)attributes | (ushort)(ConsoleColor.DarkRed));
					ConsoleControl.SetConsoleTextAttribute(bufferInfo, (ushort)attributes);
					return;
				}
			}
		}

		public override Size BufferSize
		{
			get
			{
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
				ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
				return new Size(cONSOLESCREENBUFFERINFO.BufferSize.X, cONSOLESCREENBUFFERINFO.BufferSize.Y);
			}
			set
			{
				try
				{
					SafeFileHandle activeScreenBufferHandle = ConsoleControl.GetActiveScreenBufferHandle();
					ConsoleControl.SetConsoleScreenBufferSize(activeScreenBufferHandle, value);
				}
				catch (HostException hostException1)
				{
					HostException hostException = hostException1;
					Win32Exception innerException = hostException.InnerException as Win32Exception;
					if (innerException == null || innerException.NativeErrorCode != 87)
					{
						throw;
					}
					else
					{
						throw PSTraceSource.NewArgumentOutOfRangeException("value", value, "ConsoleHostRawUserInterfaceStrings", "InvalidBufferSizeError", new object[0]);
					}
				}
			}
		}

		public override Coordinates CursorPosition
		{
			get
			{
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
				ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
				Coordinates coordinate = new Coordinates(cONSOLESCREENBUFFERINFO.CursorPosition.X, cONSOLESCREENBUFFERINFO.CursorPosition.Y);
				return coordinate;
			}
			set
			{
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
				SafeFileHandle bufferInfo = ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
				ConsoleHostRawUserInterface.CheckCoordinateWithinBuffer(ref value, ref cONSOLESCREENBUFFERINFO, "value");
				ConsoleControl.SetConsoleCursorPosition(bufferInfo, value);
			}
		}

		public override int CursorSize
		{
			get
			{
				SafeFileHandle activeScreenBufferHandle = ConsoleControl.GetActiveScreenBufferHandle();
				int size = ConsoleControl.GetConsoleCursorInfo(activeScreenBufferHandle).Size;
				return size;
			}
			set
			{
				if (value < 0 || value > 100)
				{
					throw PSTraceSource.NewArgumentOutOfRangeException("value", value, "ConsoleHostRawUserInterfaceStrings", "InvalidCursorSizeError", new object[0]);
				}
				else
				{
					SafeFileHandle activeScreenBufferHandle = ConsoleControl.GetActiveScreenBufferHandle();
					ConsoleControl.CONSOLE_CURSOR_INFO consoleCursorInfo = ConsoleControl.GetConsoleCursorInfo(activeScreenBufferHandle);
					if (value != 0)
					{
						consoleCursorInfo.Size = value;
						consoleCursorInfo.Visible = true;
					}
					else
					{
						consoleCursorInfo.Visible = false;
					}
					ConsoleControl.SetConsoleCursorInfo(activeScreenBufferHandle, consoleCursorInfo);
					return;
				}
			}
		}

		public override ConsoleColor ForegroundColor
		{
			get
			{
				/*
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
				ConsoleColor consoleColor = ConsoleColor.Black;
				ConsoleColor consoleColor1 = ConsoleColor.Black;
				ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
				ConsoleControl.WORDToColor(cONSOLESCREENBUFFERINFO.Attributes, out consoleColor, out consoleColor1);
				return consoleColor;
				*/
				return Console.ForegroundColor;
			}
			set
			{
				if (!ConsoleControl.IsConsoleColor(value))
				{
					throw PSTraceSource.NewArgumentException("value", "ConsoleHostRawUserInterfaceStrings", "InvalidConsoleColorError", new object[0]);
				}
				else
				{
					//TODO: HACK: Color is not white :(
					Console.ForegroundColor = value;
					if (value == ConsoleColor.White) Console.ResetColor ();
				}
				/*
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
				if (!ConsoleControl.IsConsoleColor(value))
				{
					throw PSTraceSource.NewArgumentException("value", "ConsoleHostRawUserInterfaceStrings", "InvalidConsoleColorError", new object[0]);
				}
				else
				{
					SafeFileHandle bufferInfo = ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
					short attributes = (short)cONSOLESCREENBUFFERINFO.Attributes;
					attributes = (short)(attributes & -16);
					attributes = (short)((ushort)attributes | (ushort)value);
					ConsoleControl.SetConsoleTextAttribute(bufferInfo, (ushort)attributes);
					return;
				}
				*/
			}
		}

		public override bool KeyAvailable
		{
			get
			{
				if (this.cachedKeyEvent.RepeatCount <= 0)
				{
					SafeFileHandle inputHandle = ConsoleControl.GetInputHandle();
					ConsoleControl.INPUT_RECORD[] nPUTRECORDArray = new ConsoleControl.INPUT_RECORD[ConsoleControl.GetNumberOfConsoleInputEvents(inputHandle)];
					int num = ConsoleControl.PeekConsoleInput(inputHandle, ref nPUTRECORDArray);
					int num1 = 0;
					while (num1 < num)
					{
						if (nPUTRECORDArray[num1].EventType != 1 || nPUTRECORDArray[num1].KeyEvent.KeyDown && nPUTRECORDArray[num1].KeyEvent.RepeatCount == 0)
						{
							num1++;
						}
						else
						{
							return true;
						}
					}
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public override Size MaxPhysicalWindowSize
		{
			get
			{
				SafeFileHandle activeScreenBufferHandle = ConsoleControl.GetActiveScreenBufferHandle();
				return ConsoleControl.GetLargestConsoleWindowSize(activeScreenBufferHandle);
			}
		}

		public override Size MaxWindowSize
		{
			get
			{
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
				ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
				Size size = new Size(cONSOLESCREENBUFFERINFO.MaxWindowSize.X, cONSOLESCREENBUFFERINFO.MaxWindowSize.Y);
				return size;
			}
		}

		public override Coordinates WindowPosition
		{
			get
			{
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
				ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
				Coordinates coordinate = new Coordinates(cONSOLESCREENBUFFERINFO.WindowRect.Left, cONSOLESCREENBUFFERINFO.WindowRect.Top);
				return coordinate;
			}
			set
			{
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
				SafeFileHandle bufferInfo = ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
				ConsoleControl.SMALL_RECT windowRect = cONSOLESCREENBUFFERINFO.WindowRect;
				int right = windowRect.Right - windowRect.Left + 1;
				int bottom = windowRect.Bottom - windowRect.Top + 1;
				if (value.X < 0 || value.X > cONSOLESCREENBUFFERINFO.BufferSize.X - right)
				{
					throw PSTraceSource.NewArgumentOutOfRangeException("value.X", value.X, "ConsoleHostRawUserInterfaceStrings", "InvalidXWindowPositionError", new object[0]);
				}
				else
				{
					if (value.Y < 0 || value.Y > cONSOLESCREENBUFFERINFO.BufferSize.Y - bottom)
					{
						throw PSTraceSource.NewArgumentOutOfRangeException("value.Y", value.Y, "ConsoleHostRawUserInterfaceStrings", "InvalidYWindowPositionError", new object[0]);
					}
					else
					{
						windowRect.Left = (short)value.X;
						windowRect.Top = (short)value.Y;
						windowRect.Right = (short)(windowRect.Left + right - 1);
						windowRect.Bottom = (short)(windowRect.Top + bottom - 1);
						ConsoleControl.SetConsoleWindowInfo(bufferInfo, true, windowRect);
						return;
					}
				}
			}
		}

		public override Size WindowSize
		{
			get
			{
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
				ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
				Size size = new Size(cONSOLESCREENBUFFERINFO.WindowRect.Right - cONSOLESCREENBUFFERINFO.WindowRect.Left + 1, cONSOLESCREENBUFFERINFO.WindowRect.Bottom - cONSOLESCREENBUFFERINFO.WindowRect.Top + 1);
				return size;
			}
			set
			{
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
				SafeFileHandle bufferInfo = ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
				if (value.Width >= 1)
				{
					if (value.Height >= 1)
					{
						if (value.Width <= cONSOLESCREENBUFFERINFO.BufferSize.X)
						{
							if (value.Height <= cONSOLESCREENBUFFERINFO.BufferSize.Y)
							{
								if (value.Width <= cONSOLESCREENBUFFERINFO.MaxWindowSize.X)
								{
									if (value.Height <= cONSOLESCREENBUFFERINFO.MaxWindowSize.Y)
									{
										ConsoleControl.SMALL_RECT windowRect = cONSOLESCREENBUFFERINFO.WindowRect;
										windowRect.Right = (short)(windowRect.Left + value.Width - 1);
										windowRect.Bottom = (short)(windowRect.Top + value.Height - 1);
										short right = (short)(windowRect.Right - cONSOLESCREENBUFFERINFO.BufferSize.X - 1);
										short bottom = (short)(windowRect.Bottom - cONSOLESCREENBUFFERINFO.BufferSize.Y - 1);
										if (right > 0)
										{
											windowRect.Left = (short)(windowRect.Left - right);
											windowRect.Right = (short)(windowRect.Right - right);
										}
										if (bottom > 0)
										{
											windowRect.Top = (short)(windowRect.Top - bottom);
											windowRect.Bottom = (short)(windowRect.Bottom - bottom);
										}
										if (windowRect.Right >= windowRect.Left)
										{
											if (windowRect.Bottom >= windowRect.Top)
											{
												ConsoleControl.SetConsoleWindowInfo(bufferInfo, true, windowRect);
												return;
											}
											else
											{
												throw PSTraceSource.NewArgumentOutOfRangeException("value", value, "ConsoleHostRawUserInterfaceStrings", "WindowTooShortError", new object[0]);
											}
										}
										else
										{
											throw PSTraceSource.NewArgumentOutOfRangeException("value", value, "ConsoleHostRawUserInterfaceStrings", "WindowTooNarrowError", new object[0]);
										}
									}
									else
									{
										object[] y = new object[1];
										y[0] = cONSOLESCREENBUFFERINFO.MaxWindowSize.Y;
										throw PSTraceSource.NewArgumentOutOfRangeException("value.Height", value.Height, "ConsoleHostRawUserInterfaceStrings", "WindowHeightTooLargeErrorTemplate", y);
									}
								}
								else
								{
									object[] x = new object[1];
									x[0] = cONSOLESCREENBUFFERINFO.MaxWindowSize.X;
									throw PSTraceSource.NewArgumentOutOfRangeException("value.Width", value.Width, "ConsoleHostRawUserInterfaceStrings", "WindowWidthTooLargeErrorTemplate", x);
								}
							}
							else
							{
								throw PSTraceSource.NewArgumentOutOfRangeException("value.Height", value.Height, "ConsoleHostRawUserInterfaceStrings", "WindowHeightLargerThanBufferError", new object[0]);
							}
						}
						else
						{
							throw PSTraceSource.NewArgumentOutOfRangeException("value.Width", value.Width, "ConsoleHostRawUserInterfaceStrings", "WindowWidthLargerThanBufferError", new object[0]);
						}
					}
					else
					{
						throw PSTraceSource.NewArgumentOutOfRangeException("value.Height", value.Height, "ConsoleHostRawUserInterfaceStrings", "WindowHeightTooSmallError", new object[0]);
					}
				}
				else
				{
					throw PSTraceSource.NewArgumentOutOfRangeException("value.Width", value.Width, "ConsoleHostRawUserInterfaceStrings", "WindowWidthTooSmallError", new object[0]);
				}
			}
		}

		public override string WindowTitle
		{
			get
			{
				return ConsoleControl.GetConsoleWindowTitle();
			}
			set
			{
				if (value == null)
				{
					throw PSTraceSource.NewArgumentNullException("value");
				}
				else
				{
					if (value.Length < 0 || value.Length > 0x3ff)
					{
						if (value.Length >= 0)
						{
							object[] objArray = new object[1];
							objArray[0] = 0x3ff;
							throw PSTraceSource.NewArgumentException("value", "ConsoleHostRawUserInterfaceStrings", "WindowTitleTooLongErrorTemplate", objArray);
						}
						else
						{
							throw PSTraceSource.NewArgumentException("value", "ConsoleHostRawUserInterfaceStrings", "WindowTitleTooShortError", new object[0]);
						}
					}
					else
					{
						ConsoleControl.SetConsoleWindowTitle(value);
						return;
					}
				}
			}
		}

		static ConsoleHostRawUserInterface()
		{
			ConsoleHostRawUserInterface.tracer = PSTraceSource.GetTracer("ConsoleHostRawUserInterface", "Console host's subclass of S.M.A.Host.RawConsole");
		}

		internal ConsoleHostRawUserInterface(ConsoleHostUserInterface mshConsole)
		{
			this.defaultForeground = ConsoleColor.Gray;
			this.defaultForeground = this.ForegroundColor;
			this.defaultBackground = this.BackgroundColor;
			this.parent = mshConsole;
			WindowsIdentity current = WindowsIdentity.GetCurrent();
			WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
			if (windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
			{
				string windowTitleElevatedPrefix = ConsoleHostRawUserInterfaceStrings.WindowTitleElevatedPrefix;
				string windowTitleTemplate = ConsoleHostRawUserInterfaceStrings.WindowTitleTemplate;
				windowTitleTemplate = Regex.Escape(windowTitleTemplate).Replace("\\{1}", ".*").Replace("\\{0}", Regex.Escape(windowTitleElevatedPrefix));
				if (!Regex.IsMatch(this.WindowTitle, windowTitleTemplate))
				{
					this.WindowTitle = StringUtil.Format(ConsoleHostRawUserInterfaceStrings.WindowTitleTemplate, windowTitleElevatedPrefix, this.WindowTitle);
				}
			}
		}

		private static void CacheKeyEvent(ConsoleControl.KEY_EVENT_RECORD input, ref ConsoleControl.KEY_EVENT_RECORD cache)
		{
			if (input.RepeatCount > 1)
			{
				cache = input;
				cache.RepeatCount = (ushort)(cache.RepeatCount - 1);
			}
		}

		private static void CheckCoordinateWithinBuffer(ref Coordinates c, ref ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO bufferInfo, string paramName)
		{
			if (c.X < 0 || c.X > bufferInfo.BufferSize.X)
			{
				object[] bufferSize = new object[1];
				bufferSize[0] = bufferInfo.BufferSize;
				throw PSTraceSource.NewArgumentOutOfRangeException(string.Concat(paramName, ".X"), c.X, "ConsoleHostRawUserInterfaceStrings", "CoordinateOutOfBufferErrorTemplate", bufferSize);
			}
			else
			{
				if (c.Y < 0 || c.Y > bufferInfo.BufferSize.Y)
				{
					object[] objArray = new object[1];
					objArray[0] = bufferInfo.BufferSize;
					throw PSTraceSource.NewArgumentOutOfRangeException(string.Concat(paramName, ".Y"), c.Y, "ConsoleHostRawUserInterfaceStrings", "CoordinateOutOfBufferErrorTemplate", objArray);
				}
				else
				{
					return;
				}
			}
		}

		public override void Clear(int code) {
			ConsoleControl.Clear();
		}

		internal void ClearKeyCache()
		{
			this.cachedKeyEvent.RepeatCount = 0;
		}

		public override void FlushInputBuffer()
		{
			SafeFileHandle inputHandle = ConsoleControl.GetInputHandle();
			ConsoleControl.FlushConsoleInputBuffer(inputHandle);
			this.cachedKeyEvent.RepeatCount = 0;
		}

		public override BufferCell[,] GetBufferContents(Rectangle region)
		{
			ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
			if (region.Right >= region.Left)
			{
				if (region.Bottom >= region.Top)
				{
					SafeFileHandle bufferInfo = ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
					int x = cONSOLESCREENBUFFERINFO.BufferSize.X;
					int y = cONSOLESCREENBUFFERINFO.BufferSize.Y;
					if (region.Left >= x || region.Top >= y || region.Right < 0 || region.Bottom < 0)
					{
						ConsoleHostRawUserInterface.tracer.WriteLine("region outside boundaries", new object[0]);
						return new BufferCell[0, 0];
					}
					else
					{
						int num = Math.Max(0, region.Left);
						int num1 = Math.Min(x - 1, region.Right);
						int num2 = Math.Max(0, region.Top);
						int num3 = Math.Min(y - 1, region.Bottom);
						Coordinates coordinate = new Coordinates(num, num2);
						Rectangle left = new Rectangle();
						left.Left = Math.Max(0, -region.Left);
						left.Top = Math.Max(0, -region.Top);
						left.Right = left.Left + num1 - num;
						left.Bottom = left.Top + num3 - num2;
						BufferCell[,] bufferCellArray = new BufferCell[region.Bottom - region.Top + 1, region.Right - region.Left + 1];
						ConsoleControl.ReadConsoleOutput(bufferInfo, coordinate, left, ref bufferCellArray);
						return bufferCellArray;
					}
				}
				else
				{
					object[] objArray = new object[2];
					objArray[0] = "region.Bottom";
					objArray[1] = "region.Top";
					throw PSTraceSource.NewArgumentException("region", "ConsoleHostRawUserInterfaceStrings", "InvalidRegionErrorTemplate", objArray);
				}
			}
			else
			{
				object[] objArray1 = new object[2];
				objArray1[0] = "region.Right";
				objArray1[1] = "region.Left";
				throw PSTraceSource.NewArgumentException("region", "ConsoleHostRawUserInterfaceStrings", "InvalidRegionErrorTemplate", objArray1);
			}
		}

		internal static SafeFileHandle GetBufferInfo(out ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO bufferInfo)
		{
			SafeFileHandle activeScreenBufferHandle = ConsoleControl.GetActiveScreenBufferHandle();
			bufferInfo = ConsoleControl.GetConsoleScreenBufferInfo(activeScreenBufferHandle);
			return activeScreenBufferHandle;
		}

		private static void KEY_EVENT_RECORDToKeyInfo(ConsoleControl.KEY_EVENT_RECORD keyEventRecord, out KeyInfo keyInfo)
		{
			keyInfo = new KeyInfo(keyEventRecord.VirtualKeyCode, keyEventRecord.UnicodeChar, (ControlKeyStates)keyEventRecord.ControlKeyState, keyEventRecord.KeyDown);
		}

		public override int LengthInBufferCells(string s)
		{
			return this.LengthInBufferCells(s, 0);
		}

		public override int LengthInBufferCells(string s, int offset)
		{
			if (s != null)
			{
				return ConsoleControl.LengthInBufferCells(s, offset);
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("str");
			}
		}

		public override int LengthInBufferCells(char c)
		{
			return ConsoleControl.LengthInBufferCells(c);
		}

		private PipelineStoppedException NewPipelineStoppedException()
		{
			PipelineStoppedException pipelineStoppedException = new PipelineStoppedException();
			return pipelineStoppedException;
		}

		public override KeyInfo ReadKey(ReadKeyOptions options)
		{
			KeyInfo keyInfo;
			if ((options & (ReadKeyOptions.IncludeKeyDown | ReadKeyOptions.IncludeKeyUp)) != 0)
			{
				if (this.cachedKeyEvent.RepeatCount > 0)
				{
					if ((options & ReadKeyOptions.AllowCtrlC) != 0 || this.cachedKeyEvent.UnicodeChar != '\u0003')
					{
						if ((options & ReadKeyOptions.IncludeKeyUp) == 0 && !this.cachedKeyEvent.KeyDown || (options & ReadKeyOptions.IncludeKeyDown) == 0 && this.cachedKeyEvent.KeyDown)
						{
							this.cachedKeyEvent.RepeatCount = 0;
						}
					}
					else
					{
						ConsoleControl.KEY_EVENT_RECORD kEYEVENTRECORDPointer = this.cachedKeyEvent;
						this.cachedKeyEvent.RepeatCount = (ushort)(this.cachedKeyEvent.RepeatCount - 1);
						throw this.NewPipelineStoppedException();
					}
				}
				if (this.cachedKeyEvent.RepeatCount <= 0)
				{
					SafeFileHandle inputHandle = ConsoleControl.GetInputHandle();
					ConsoleControl.INPUT_RECORD[] nPUTRECORDArray = new ConsoleControl.INPUT_RECORD[1];
					ConsoleControl.ConsoleModes mode = ConsoleControl.GetMode(inputHandle);
					ConsoleControl.ConsoleModes consoleMode = mode & (ConsoleControl.ConsoleModes.ProcessedInput | ConsoleControl.ConsoleModes.LineInput | ConsoleControl.ConsoleModes.EchoInput | ConsoleControl.ConsoleModes.MouseInput | ConsoleControl.ConsoleModes.Insert | ConsoleControl.ConsoleModes.QuickEdit | ConsoleControl.ConsoleModes.Extended | ConsoleControl.ConsoleModes.AutoPosition | ConsoleControl.ConsoleModes.ProcessedOutput | ConsoleControl.ConsoleModes.WrapEndOfLine) & (ConsoleControl.ConsoleModes.ProcessedInput | ConsoleControl.ConsoleModes.LineInput | ConsoleControl.ConsoleModes.EchoInput | ConsoleControl.ConsoleModes.WindowInput | ConsoleControl.ConsoleModes.Insert | ConsoleControl.ConsoleModes.QuickEdit | ConsoleControl.ConsoleModes.Extended | ConsoleControl.ConsoleModes.AutoPosition | ConsoleControl.ConsoleModes.ProcessedOutput | ConsoleControl.ConsoleModes.WrapEndOfLine) & (ConsoleControl.ConsoleModes.LineInput | ConsoleControl.ConsoleModes.EchoInput | ConsoleControl.ConsoleModes.WindowInput | ConsoleControl.ConsoleModes.MouseInput | ConsoleControl.ConsoleModes.Insert | ConsoleControl.ConsoleModes.QuickEdit | ConsoleControl.ConsoleModes.Extended | ConsoleControl.ConsoleModes.AutoPosition | ConsoleControl.ConsoleModes.WrapEndOfLine);
					try
					{
						ConsoleControl.SetMode(inputHandle, consoleMode);
						do
						{
						Label0:
							int num = ConsoleControl.ReadConsoleInput(inputHandle, ref nPUTRECORDArray);
							if (num == 1 && nPUTRECORDArray[0].EventType == 1 && nPUTRECORDArray[0].KeyEvent.RepeatCount != 0)
							{
								if ((options & ReadKeyOptions.AllowCtrlC) != 0 || nPUTRECORDArray[0].KeyEvent.UnicodeChar != '\u0003')
								{
									continue;
								}
								ConsoleHostRawUserInterface.CacheKeyEvent(nPUTRECORDArray[0].KeyEvent, ref this.cachedKeyEvent);
								throw this.NewPipelineStoppedException();
							}
							else
							{
								goto Label0;
							}
						}
						while (((options & ReadKeyOptions.IncludeKeyDown) == 0 || !nPUTRECORDArray[0].KeyEvent.KeyDown) && ((options & ReadKeyOptions.IncludeKeyUp) == 0 || nPUTRECORDArray[0].KeyEvent.KeyDown));
						ConsoleHostRawUserInterface.CacheKeyEvent(nPUTRECORDArray[0].KeyEvent, ref this.cachedKeyEvent);
						ConsoleHostRawUserInterface.KEY_EVENT_RECORDToKeyInfo(nPUTRECORDArray[0].KeyEvent, out keyInfo);
					}
					finally
					{
						ConsoleControl.SetMode(inputHandle, mode);
					}
				}
				else
				{
					ConsoleHostRawUserInterface.KEY_EVENT_RECORDToKeyInfo(this.cachedKeyEvent, out keyInfo);
					this.cachedKeyEvent.RepeatCount = (ushort)(this.cachedKeyEvent.RepeatCount - 1);
				}
				if ((options & ReadKeyOptions.NoEcho) == 0)
				{
					char character = keyInfo.Character;
					this.parent.WriteToConsole(character.ToString(CultureInfo.CurrentCulture), true);
				}
				return keyInfo;
			}
			else
			{
				throw PSTraceSource.NewArgumentException("options", "ConsoleHostRawUserInterfaceStrings", "InvalidReadKeyOptionsError", new object[0]);
			}
		}

		public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
		{
			ConsoleControl.SMALL_RECT left;
            left.Left = (short)source.Left;
			left.Right = (short)source.Right;
			left.Top = (short)source.Top;
			left.Bottom = (short)source.Bottom;
			ConsoleControl.SMALL_RECT right;
            right.Left = (short)clip.Left;
			right.Right = (short)clip.Right;
			right.Top = (short)clip.Top;
			right.Bottom = (short)clip.Bottom;
			ConsoleControl.COORD x;
            x.X = (short)destination.X;
			x.Y = (short)destination.Y;
			ConsoleControl.CHAR_INFO character;
            character.UnicodeChar = fill.Character;
			character.Attributes = ConsoleControl.ColorToWORD(fill.ForegroundColor, fill.BackgroundColor);
			SafeFileHandle activeScreenBufferHandle = ConsoleControl.GetActiveScreenBufferHandle();
			ConsoleControl.ScrollConsoleScreenBuffer(activeScreenBufferHandle, left, right, x, character);
		}

		public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
		{
			ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
			if (contents == null)
			{
				PSTraceSource.NewArgumentNullException("contents");
			}
			SafeFileHandle bufferInfo = ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
			ConsoleHostRawUserInterface.CheckCoordinateWithinBuffer(ref origin, ref cONSOLESCREENBUFFERINFO, "origin");
			ConsoleControl.WriteConsoleOutput(bufferInfo, origin, contents);
		}

		public override void SetBufferContents(Rectangle region, BufferCell fill)
		{
			ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
			uint num = 0;
			if (region.Right >= region.Left)
			{
				if (region.Bottom >= region.Top)
				{
					SafeFileHandle bufferInfo = ConsoleHostRawUserInterface.GetBufferInfo(out cONSOLESCREENBUFFERINFO);
					int x = cONSOLESCREENBUFFERINFO.BufferSize.X;
					int y = cONSOLESCREENBUFFERINFO.BufferSize.Y;
					ushort wORD = ConsoleControl.ColorToWORD(fill.ForegroundColor, fill.BackgroundColor);
					Coordinates coordinate = new Coordinates(0, 0);
					if (region.Left != -1 || region.Right != -1 || region.Top != -1 || region.Bottom != -1)
					{
						if (region.Left >= x || region.Top >= y || region.Right < 0 || region.Bottom < 0)
						{
							ConsoleHostRawUserInterface.tracer.WriteLine("region outside boundaries", new object[0]);
							return;
						}
						else
						{
							int num1 = Math.Max(0, region.Left);
							int num2 = Math.Min(x - 1, region.Right);
							int num3 = num2 - num1 + 1;
							coordinate.X = num1;
							int num4 = Math.Max(0, region.Top);
							int num5 = Math.Min(y - 1, region.Bottom);
							coordinate.Y = num4;
							if (ConsoleControl.IsCJKOutputCodePage(out num))
							{
								Rectangle rectangle = new Rectangle(0, 0, 1, num5 - num4);
								int num6 = this.LengthInBufferCells(fill.Character);
								if (coordinate.X > 0)
								{
									BufferCell[,] bufferCellArray = new BufferCell[rectangle.Bottom + 1, 2];
									ConsoleControl.ReadConsoleOutputCJK(bufferInfo, num, new Coordinates(coordinate.X - 1, coordinate.Y), rectangle, ref bufferCellArray);
									int num7 = 0;
									while (num7 <= rectangle.Bottom)
									{
										if (bufferCellArray[num7, 0].BufferCellType != BufferCellType.Leading)
										{
											num7++;
										}
										else
										{
											throw PSTraceSource.NewArgumentException("fill");
										}
									}
								}
								if (num2 != x - 1)
								{
									BufferCell[,] bufferCellArray1 = new BufferCell[rectangle.Bottom + 1, 2];
									ConsoleControl.ReadConsoleOutputCJK(bufferInfo, num, new Coordinates(num2, coordinate.Y), rectangle, ref bufferCellArray1);
									if (num3 % 2 != 0)
									{
										int num8 = 0;
										while (num8 <= rectangle.Bottom)
										{
											if (!(bufferCellArray1[num8, 0].BufferCellType == BufferCellType.Leading ^ num6 == 2))
											{
												num8++;
											}
											else
											{
												throw PSTraceSource.NewArgumentException("fill");
											}
										}
									}
									else
									{
										int num9 = 0;
										while (num9 <= rectangle.Bottom)
										{
											if (bufferCellArray1[num9, 0].BufferCellType != BufferCellType.Leading)
											{
												num9++;
											}
											else
											{
												throw PSTraceSource.NewArgumentException("fill");
											}
										}
									}
								}
								else
								{
									if (num6 == 2)
									{
										throw PSTraceSource.NewArgumentException("fill");
									}
								}
								if (num3 % 2 == 1)
								{
									num3++;
								}
							}
							for (int i = num4; i <= num5; i++)
							{
								coordinate.Y = i;
								ConsoleControl.FillConsoleOutputCharacter(bufferInfo, fill.Character, num3, coordinate);
								ConsoleControl.FillConsoleOutputAttribute(bufferInfo, wORD, num3, coordinate);
							}
							return;
						}
					}
					else
					{
						if (x % 2 != 1 || !ConsoleControl.IsCJKOutputCodePage(out num) || this.LengthInBufferCells(fill.Character) != 2)
						{
							int num10 = x * y;
							ConsoleControl.FillConsoleOutputCharacter(bufferInfo, fill.Character, num10, coordinate);
							ConsoleControl.FillConsoleOutputAttribute(bufferInfo, wORD, num10, coordinate);
							return;
						}
						else
						{
							throw PSTraceSource.NewArgumentException("fill");
						}
					}
				}
				else
				{
					object[] objArray = new object[2];
					objArray[0] = "region.Bottom";
					objArray[1] = "region.Top";
					throw PSTraceSource.NewArgumentException("region", "ConsoleHostRawUserInterfaceStrings", "InvalidRegionErrorTemplate", objArray);
				}
			}
			else
			{
				object[] objArray1 = new object[2];
				objArray1[0] = "region.Right";
				objArray1[1] = "region.Left";
				throw PSTraceSource.NewArgumentException("region", "ConsoleHostRawUserInterfaceStrings", "InvalidRegionErrorTemplate", objArray1);
			}
		}
	}
}