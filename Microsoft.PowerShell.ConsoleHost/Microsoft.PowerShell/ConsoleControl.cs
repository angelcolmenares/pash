 using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using Mono.Terminal;

namespace Microsoft.PowerShell
{
	internal static class ConsoleControl
	{

		internal const int SW_HIDE = 0;

		internal const int SW_SHOWNORMAL = 1;

		internal const int SW_NORMAL = 1;

		internal const int SW_SHOWMINIMIZED = 2;

		internal const int SW_SHOWMAXIMIZED = 3;

		internal const int SW_MAXIMIZE = 3;

		internal const int SW_SHOWNOACTIVATE = 4;

		internal const int SW_SHOW = 5;

		internal const int SW_MINIMIZE = 6;

		internal const int SW_SHOWMINNOACTIVE = 7;

		internal const int SW_SHOWNA = 8;

		internal const int SW_RESTORE = 9;

		internal const int SW_SHOWDEFAULT = 10;

		internal const int SW_FORCEMINIMIZE = 11;

		internal const int SW_MAX = 11;

		private const string StringsResourceBaseName = "ConsoleControlStrings";

		private readonly static Lazy<SafeFileHandle> _inputHandle;

		private readonly static Lazy<SafeFileHandle> _outputHandle;

		[TraceSource("ConsoleControl", "Console control methods")]
		private static PSTraceSource tracer;

		static ConsoleControl()
		{
			ConsoleControl._inputHandle = new Lazy<SafeFileHandle>(() => {
				IntPtr intPtr = ConsoleControl.NativeMethods.CreateFile("CONIN$", -1073741824, 1, (IntPtr)0, 3, 0, (IntPtr)0);
				if (intPtr != ConsoleControl.NativeMethods.INVALID_HANDLE_VALUE)
				{
					return new SafeFileHandle(intPtr, true);
				}
				else
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "RetreiveInputConsoleHandle", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.GetInputModeExceptionTemplate);
					throw hostException;
				}
			}
			);
			ConsoleControl._outputHandle = new Lazy<SafeFileHandle>(() => {
				IntPtr intPtr = IntPtr.Zero; //ConsoleControl.NativeMethods.CreateFile("CONOUT$", -1073741824, 2, (IntPtr)0, 3, 0, (IntPtr)0);
				//Type t = typeof(System.ConsoleDriver);
				//var fields = t.GetFields (System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
				if (intPtr != ConsoleControl.NativeMethods.INVALID_HANDLE_VALUE)
				{
					return new SafeFileHandle(intPtr, true);
				}
				else
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "RetreiveActiveScreenBufferConsoleHandle", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.GetActiveScreenBufferHandleExceptionTemplate);
					throw hostException;
				}
			}
			);
			ConsoleControl.tracer = PSTraceSource.GetTracer("ConsoleControl", "Console control methods");
		}

		[ArchitectureSensitive]
		internal static void AddBreakHandler(ConsoleControl.BreakHandler handlerDelegate)
		{
			bool flag = ConsoleControl.NativeMethods.SetConsoleCtrlHandler(handlerDelegate, true);
			if (flag)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "AddBreakHandler", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.AddBreakHandlerExceptionMessage);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		private static void BuildEdgeTypeInfo(Rectangle contentsRegion, BufferCell[,] contents, List<ConsoleControl.BufferCellArrayRowTypeRange> sameEdgeAreas, out int firstLeftTrailingRow, out int firstRightLeadingRow)
		{
			firstLeftTrailingRow = -1;
			firstRightLeadingRow = -1;
			ConsoleControl.BufferCellArrayRowType edgeType = ConsoleControl.GetEdgeType(contents[contentsRegion.Top, contentsRegion.Left], contents[contentsRegion.Top, contentsRegion.Right]);
			int top = contentsRegion.Top;
			while (top <= contentsRegion.Bottom)
			{
				ConsoleControl.BufferCellArrayRowTypeRange bufferCellArrayRowTypeRange;
                bufferCellArrayRowTypeRange.Start = top;
				bufferCellArrayRowTypeRange.Type = edgeType;
				if (firstLeftTrailingRow == -1 && (bufferCellArrayRowTypeRange.Type & ConsoleControl.BufferCellArrayRowType.LeftTrailing) != 0)
				{
					firstLeftTrailingRow = top;
				}
				if (firstRightLeadingRow == -1 && (bufferCellArrayRowTypeRange.Type & ConsoleControl.BufferCellArrayRowType.RightLeading) != 0)
				{
					firstRightLeadingRow = top;
				}
				do
				{
					top++;
					if (top <= contentsRegion.Bottom)
					{
						edgeType = ConsoleControl.GetEdgeType(contents[top, contentsRegion.Left], contents[top, contentsRegion.Right]);
					}
					else
					{
						bufferCellArrayRowTypeRange.End = top - 1;
						sameEdgeAreas.Add(bufferCellArrayRowTypeRange);
						return;
					}
				}
				while (edgeType == bufferCellArrayRowTypeRange.Type);
				bufferCellArrayRowTypeRange.End = top - 1;
				sameEdgeAreas.Add(bufferCellArrayRowTypeRange);
			}
		}

		internal static void Clear ()
		{
			Console.Clear ();
		}

		[ArchitectureSensitive]
		private static void CheckWriteConsoleOutputContents(BufferCell[,] contents, Rectangle contentsRegion)
		{
			for (int i = contentsRegion.Top; i <= contentsRegion.Bottom; i++)
			{
				int left = contentsRegion.Left;
				while (left <= contentsRegion.Right)
				{
					if (contents[i, left].BufferCellType != BufferCellType.Trailing || contents[i, left].Character == 0)
					{
						int num = ConsoleControl.LengthInBufferCells(contents[i, left].Character);
						if (contents[i, left].BufferCellType == BufferCellType.Leading)
						{
							if (num != 1)
							{
								if (num == 2)
								{
									left++;
									if (left > contentsRegion.Right)
									{
										break;
									}
									if (contents[i, left].Character != 0 || contents[i, left].BufferCellType != BufferCellType.Trailing)
									{
										object[] objArray = new object[2];
										objArray[0] = i;
										objArray[1] = left;
										throw PSTraceSource.NewArgumentException(string.Format(CultureInfo.InvariantCulture, "contents[{0}, {1}]", objArray));
									}
								}
							}
							else
							{
								object[] objArray1 = new object[2];
								objArray1[0] = i;
								objArray1[1] = left;
								throw PSTraceSource.NewArgumentException(string.Format(CultureInfo.InvariantCulture, "contents[{0}, {1}]", objArray1));
							}
						}
						left++;
					}
					else
					{
						object[] objArray2 = new object[2];
						objArray2[0] = i;
						objArray2[1] = left;
						throw PSTraceSource.NewArgumentException(string.Format(CultureInfo.InvariantCulture, "contents[{0}, {1}]", objArray2));
					}
				}
			}
		}

		[ArchitectureSensitive]
		internal static void CheckWriteEdges(SafeFileHandle consoleHandle, uint codePage, Coordinates origin, BufferCell[,] contents, Rectangle contentsRegion, ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO bufferInfo, int firstLeftTrailingRow, int firstRightLeadingRow)
		{
			Rectangle rectangle = new Rectangle(0, 0, 1, contentsRegion.Bottom - contentsRegion.Top);
			if (origin.X != 0)
			{
				BufferCell[,] bufferCellArray = new BufferCell[rectangle.Bottom + 1, 2];
				ConsoleControl.ReadConsoleOutputCJK(consoleHandle, codePage, new Coordinates(origin.X - 1, origin.Y), rectangle, ref bufferCellArray);
				int top = contentsRegion.Top;
				int num = 0;
				while (top <= contentsRegion.Bottom)
				{
					if (!(bufferCellArray[top, 0].BufferCellType == BufferCellType.Leading ^ contents[top, contentsRegion.Left].BufferCellType == BufferCellType.Trailing))
					{
						top++;
						num++;
					}
					else
					{
						object[] left = new object[2];
						left[0] = top;
						left[1] = contentsRegion.Left;
						throw PSTraceSource.NewArgumentException(string.Format(CultureInfo.InvariantCulture, "contents[{0}, {1}]", left));
					}
				}
			}
			else
			{
				if (firstLeftTrailingRow >= 0)
				{
					object[] objArray = new object[2];
					objArray[0] = firstLeftTrailingRow;
					objArray[1] = contentsRegion.Left;
					throw PSTraceSource.NewArgumentException(string.Format(CultureInfo.InvariantCulture, "contents[{0}, {1}]", objArray));
				}
			}
			if (origin.X + contentsRegion.Right - contentsRegion.Left + 1 < bufferInfo.BufferSize.X)
			{
				BufferCell[,] bufferCellArray1 = new BufferCell[rectangle.Bottom + 1, 2];
				ConsoleControl.ReadConsoleOutputCJK(consoleHandle, codePage, new Coordinates(origin.X + contentsRegion.Right - contentsRegion.Left, origin.Y), rectangle, ref bufferCellArray1);
				int top1 = contentsRegion.Top;
				int num1 = 0;
				while (top1 <= contentsRegion.Bottom)
				{
					if (!(bufferCellArray1[top1, 0].BufferCellType == BufferCellType.Leading ^ contents[top1, contentsRegion.Right].BufferCellType == BufferCellType.Leading))
					{
						top1++;
						num1++;
					}
					else
					{
						object[] right = new object[2];
						right[0] = top1;
						right[1] = contentsRegion.Right;
						throw PSTraceSource.NewArgumentException(string.Format(CultureInfo.InvariantCulture, "contents[{0}, {1}]", right));
					}
				}
			}
			else
			{
				if (firstRightLeadingRow >= 0)
				{
					object[] right1 = new object[2];
					right1[0] = firstRightLeadingRow;
					right1[1] = contentsRegion.Right;
					throw PSTraceSource.NewArgumentException(string.Format(CultureInfo.InvariantCulture, "contents[{0}, {1}]", right1));
				}
			}
		}

		private static uint CodePageToCharSet(uint codePage)
		{
			ConsoleControl.CHARSETINFO cHARSETINFO;
			if (!ConsoleControl.NativeMethods.TranslateCharsetInfo((IntPtr)((ulong)codePage), out cHARSETINFO, 2))
			{
				cHARSETINFO.ciCharset = 0xff;
			}
			return cHARSETINFO.ciCharset;
		}

		[ArchitectureSensitive]
		internal static ushort ColorToWORD(ConsoleColor foreground, ConsoleColor background)
		{
			ushort num = (ushort)(background | ConsoleColor.DarkRed | foreground);
			return num;
		}

		private static HostException CreateHostException(int win32Error, string errorId, ErrorCategory category, string resourceStr)
		{
			Win32Exception win32Exception = new Win32Exception(win32Error);
			string str = StringUtil.Format(resourceStr, win32Exception.Message, win32Error);
			HostException hostException = new HostException(str, win32Exception, errorId, category);
			return hostException;
		}

		[ArchitectureSensitive]
		internal static void FillConsoleOutputAttribute(SafeFileHandle consoleHandle, ushort attribute, int numberToWrite, Coordinates origin)
		{
			ConsoleControl.COORD x;
            x.X = (short)origin.X;
			x.Y = (short)origin.Y;
			int num = 0;
			bool flag = ConsoleControl.NativeMethods.FillConsoleOutputAttribute(consoleHandle.DangerousGetHandle(), attribute, numberToWrite, x, out num);
			if (flag)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "FillConsoleOutputAttribute", ErrorCategory.WriteError, ConsoleControlStrings.FillConsoleOutputAttributeExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static void FillConsoleOutputCharacter(SafeFileHandle consoleHandle, char character, int numberToWrite, Coordinates origin)
		{
			ConsoleControl.COORD x;
            x.X = (short)origin.X;
			x.Y = (short)origin.Y;
			int num = 0;
			bool flag = ConsoleControl.NativeMethods.FillConsoleOutputCharacter(consoleHandle.DangerousGetHandle(), character, numberToWrite, x, out num);
			if (flag)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "FillConsoleOutputCharacter", ErrorCategory.WriteError, ConsoleControlStrings.FillConsoleOutputCharacterExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static void FlushConsoleInputBuffer(SafeFileHandle consoleHandle)
		{
			bool flag = ConsoleControl.NativeMethods.FlushConsoleInputBuffer(consoleHandle.DangerousGetHandle());
			if (flag)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "FlushConsoleInputBuffer", ErrorCategory.ReadError, ConsoleControlStrings.FlushConsoleInputBufferExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static SafeFileHandle GetActiveScreenBufferHandle()
		{
			return ConsoleControl._outputHandle.Value;
		}

		[ArchitectureSensitive]
		internal static ConsoleControl.CONSOLE_CURSOR_INFO GetConsoleCursorInfo(SafeFileHandle consoleHandle)
		{
			ConsoleControl.CONSOLE_CURSOR_INFO cONSOLECURSORINFO;
			bool consoleCursorInfo = ConsoleControl.NativeMethods.GetConsoleCursorInfo(consoleHandle.DangerousGetHandle(), out cONSOLECURSORINFO);
			if (consoleCursorInfo)
			{
				return cONSOLECURSORINFO;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "GetConsoleCursorInfo", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.GetConsoleCursorInfoExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static ConsoleControl.CONSOLE_FONT_INFO_EX GetConsoleFontInfo(SafeFileHandle consoleHandle)
		{
			ConsoleControl.CONSOLE_FONT_INFO_EX cONSOLEFONTINFOEX = new ConsoleControl.CONSOLE_FONT_INFO_EX();
			cONSOLEFONTINFOEX.cbSize = Marshal.SizeOf(cONSOLEFONTINFOEX);
			bool currentConsoleFontEx = ConsoleControl.NativeMethods.GetCurrentConsoleFontEx(consoleHandle.DangerousGetHandle(), false, ref cONSOLEFONTINFOEX);
			if (currentConsoleFontEx)
			{
				return cONSOLEFONTINFOEX;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "GetConsoleFontInfo", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.GetConsoleFontInfoExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO GetConsoleScreenBufferInfo(SafeFileHandle consoleHandle)
		{
			ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
			bool consoleScreenBufferInfo = ConsoleControl.NativeMethods.GetConsoleScreenBufferInfo(consoleHandle.DangerousGetHandle(), out cONSOLESCREENBUFFERINFO);
			if (consoleScreenBufferInfo)
			{
				return cONSOLESCREENBUFFERINFO;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "GetConsoleScreenBufferInfo", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.GetConsoleScreenBufferInfoExceptionTemplate);
				throw hostException;
			}
		}

		[DllImport("Kernel32.dll", CharSet=CharSet.None)]
		internal static extern IntPtr GetConsoleWindow();

		[ArchitectureSensitive]
		internal static string GetConsoleWindowTitle()
		{
			int num = 0x400;
			StringBuilder stringBuilder = new StringBuilder(num);
			uint consoleTitle = ConsoleControl.NativeMethods.GetConsoleTitle(stringBuilder, num);
			if (consoleTitle != 0)
			{
				return stringBuilder.ToString();
			}
			else
			{
				return string.Empty;
			}
		}

		[ArchitectureSensitive]
		private static ConsoleControl.BufferCellArrayRowType GetEdgeType(BufferCell left, BufferCell right)
		{
			ConsoleControl.BufferCellArrayRowType bufferCellArrayRowType = 0;
			if (left.BufferCellType == BufferCellType.Trailing)
			{
				bufferCellArrayRowType = bufferCellArrayRowType | ConsoleControl.BufferCellArrayRowType.LeftTrailing;
			}
			if (right.BufferCellType == BufferCellType.Leading)
			{
				bufferCellArrayRowType = bufferCellArrayRowType | ConsoleControl.BufferCellArrayRowType.RightLeading;
			}
			return bufferCellArrayRowType;
		}

		[ArchitectureSensitive]
		internal static SafeFileHandle GetInputHandle()
		{
			return ConsoleControl._inputHandle.Value;
		}

		[ArchitectureSensitive]
		internal static Size GetLargestConsoleWindowSize(SafeFileHandle consoleHandle)
		{
			ConsoleControl.COORD largestConsoleWindowSize = ConsoleControl.NativeMethods.GetLargestConsoleWindowSize(consoleHandle.DangerousGetHandle());
			if (largestConsoleWindowSize.X != 0 || largestConsoleWindowSize.Y != 0)
			{
				return new Size(largestConsoleWindowSize.X, largestConsoleWindowSize.Y);
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "GetLargestConsoleWindowSize", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.GetLargestConsoleWindowSizeExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static ConsoleControl.ConsoleModes GetMode(SafeFileHandle consoleHandle)
		{
			int num = 0;
			bool consoleMode = ConsoleControl.NativeMethods.GetConsoleMode(consoleHandle.DangerousGetHandle(), out num);
			if (consoleMode)
			{
				ConsoleControl.tracer.WriteLine(num);
				return (ConsoleControl.ConsoleModes)num;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "GetConsoleMode", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.GetModeExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static int GetNumberOfConsoleInputEvents(SafeFileHandle consoleHandle)
		{
			int num = 0;
			bool numberOfConsoleInputEvents = ConsoleControl.NativeMethods.GetNumberOfConsoleInputEvents(consoleHandle.DangerousGetHandle(), out num);
			if (numberOfConsoleInputEvents)
			{
				return num;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "GetNumberOfConsoleInputEvents", ErrorCategory.ReadError, ConsoleControlStrings.GetNumberOfConsoleInputEventsExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static IntPtr GetStdHandle(long handleId)
		{
			return ConsoleControl.NativeMethods.GetStdHandle(handleId);
		}

		private static bool IsAnyDBCSCharSet(uint charSet)
		{
			if (charSet == 128 || charSet == 129 || charSet == 136)
			{
				return true;
			}
			else
			{
				return charSet == 134;
			}
		}

		private static bool IsAvailableFarEastCodePage(uint codePage)
		{
			uint charSet = ConsoleControl.CodePageToCharSet(codePage);
			return ConsoleControl.IsAnyDBCSCharSet(charSet);
		}

		[ArchitectureSensitive]
		internal static bool IsCJKOutputCodePage(out uint codePage)
		{
			codePage = ConsoleControl.NativeMethods.GetConsoleOutputCP();
			if (codePage == 0x3a4 || codePage == 0x3a8 || codePage == 0x3b5)
			{
				return true;
			}
			else
			{
				return codePage == 0x3b6;
			}
		}

		[ArchitectureSensitive]
		internal static bool IsConsoleColor(ConsoleColor c)
		{
			ConsoleColor consoleColor = c;
			switch (consoleColor)
			{
				case ConsoleColor.Black:
				case ConsoleColor.DarkBlue:
				case ConsoleColor.DarkGreen:
				case ConsoleColor.DarkCyan:
				case ConsoleColor.DarkRed:
				case ConsoleColor.DarkMagenta:
				case ConsoleColor.DarkYellow:
				case ConsoleColor.Gray:
				case ConsoleColor.DarkGray:
				case ConsoleColor.Blue:
				case ConsoleColor.Green:
				case ConsoleColor.Cyan:
				case ConsoleColor.Red:
				case ConsoleColor.Magenta:
				case ConsoleColor.Yellow:
				case ConsoleColor.White:
				{
					return true;
				}
			}
			return false;
		}

		internal static int LengthInBufferCells(char c)
		{
			uint consoleOutputCP = ConsoleControl.NativeMethods.GetConsoleOutputCP();
			return ConsoleControl.LengthInBufferCells(c, consoleOutputCP);
		}

		[ArchitectureSensitive]
		private static int LengthInBufferCells(char c, uint codePage)
		{
			int num;
			if (ConsoleControl.IsAvailableFarEastCodePage(codePage))
			{
				IntPtr intPtr = (IntPtr)0;
				IntPtr intPtr1 = (IntPtr)0;
				bool flag = false;
				ConsoleControl.TEXTMETRIC tEXTMETRIC = new ConsoleControl.TEXTMETRIC();
				try
				{
					num = ConsoleControl.LengthInBufferCellsFE(c, ref intPtr, ref intPtr1, ref flag, ref tEXTMETRIC);
				}
				finally
				{
					if (intPtr != (IntPtr)0 && intPtr1 != (IntPtr)0)
					{
						ConsoleControl.NativeMethods.ReleaseDC(intPtr, intPtr1);
					}
				}
				return num;
			}
			else
			{
				return 1;
			}
		}

		[ArchitectureSensitive]
		internal static int LengthInBufferCells(string str, int offset)
		{
			int num;
			uint consoleOutputCP = ConsoleControl.NativeMethods.GetConsoleOutputCP();
			if (ConsoleControl.IsAvailableFarEastCodePage(consoleOutputCP))
			{
				IntPtr intPtr = (IntPtr)0;
				IntPtr intPtr1 = (IntPtr)0;
				bool flag = false;
				ConsoleControl.TEXTMETRIC tEXTMETRIC = new ConsoleControl.TEXTMETRIC();
				int num1 = 0;
				try
				{
					int length = str.Length;
					for (int i = offset; i < length; i++)
					{
						char chr = str[i];
						num1 = num1 + ConsoleControl.LengthInBufferCellsFE(chr, ref intPtr, ref intPtr1, ref flag, ref tEXTMETRIC);
					}
					num = num1;
				}
				finally
				{
					if (intPtr != (IntPtr)0 && intPtr1 != (IntPtr)0)
					{
						ConsoleControl.NativeMethods.ReleaseDC(intPtr, intPtr1);
					}
				}
				return num;
			}
			else
			{
				return str.Length - offset;
			}
		}

		private static int LengthInBufferCellsFE(char c, ref IntPtr hwnd, ref IntPtr hDC, ref bool istmInitialized, ref ConsoleControl.TEXTMETRIC tm)
		{
			bool textMetrics;
			int num = 0;
			if (32 > c || c > '~')
			{
				if (0x3041 > c || c > '\u3094')
				{
					if (0x30a1 > c || c > '\u30F6')
					{
						if (0x3105 > c || c > '\u312C')
						{
							if (0x3131 > c || c > '\u318E')
							{
								if (0xac00 > c || c > '\uD7A3')
								{
									if (0xff01 > c || c > '～')
									{
										if (0xff61 > c || c > '\uFF9F')
										{
											if ((0xffa0 > c || c > '\uFFBE') && (0xffc2 > c || c > '\uFFC7') && (0xffca > c || c > '\uFFCF') && (0xffd2 > c || c > '\uFFD7') && (0xffda > c || c > '\uFFDC'))
											{
												if (0xffe0 > c || c > '￦')
												{
													if (0x4e00 > c || c > '\u9FA5')
													{
														if (0xf900 > c || c > '\uFA2D')
														{
															if (hDC == (IntPtr)0)
															{
																hwnd = ConsoleControl.NativeMethods.GetConsoleWindow();
																if ((IntPtr)0 != hwnd)
																{
																	hDC = ConsoleControl.NativeMethods.GetDC(hwnd);
																	if ((IntPtr)0 == hDC)
																	{
																		int lastWin32Error = Marshal.GetLastWin32Error();
																		object[] objArray = new object[1];
																		objArray[0] = lastWin32Error;
																		ConsoleControl.tracer.TraceError("Win32 Error 0x{0:X} occurred when getting the Device Context of the console window.", objArray);
																		return 1;
																	}
																}
																else
																{
																	int lastWin32Error1 = Marshal.GetLastWin32Error();
																	object[] objArray1 = new object[1];
																	objArray1[0] = lastWin32Error1;
																	ConsoleControl.tracer.TraceError("Win32 Error 0x{0:X} occurred when getting the window handle to the console.", objArray1);
																	return 1;
																}
															}
															if (!istmInitialized)
															{
																textMetrics = ConsoleControl.NativeMethods.GetTextMetrics(hDC, out tm);
																if (textMetrics)
																{
																	istmInitialized = true;
																}
																else
																{
																	int num1 = Marshal.GetLastWin32Error();
																	object[] objArray2 = new object[1];
																	objArray2[0] = num1;
																	ConsoleControl.tracer.TraceError("Win32 Error 0x{0:X} occurred when getting the Text Metric of the console window's Device Context.", objArray2);
																	return 1;
																}
															}
															textMetrics = ConsoleControl.NativeMethods.GetCharWidth32(hDC, c, c, out num);
															if (textMetrics)
															{
																if (num < tm.tmMaxCharWidth)
																{
																	object[] objArray3 = new object[1];
																	objArray3[0] = (int)c;
																	ConsoleControl.tracer.WriteLine("failed to locate char {0}, return 1", objArray3);
																	return 1;
																}
																else
																{
																	return 2;
																}
															}
															else
															{
																int lastWin32Error2 = Marshal.GetLastWin32Error();
																object[] objArray4 = new object[1];
																objArray4[0] = lastWin32Error2;
																ConsoleControl.tracer.TraceError("Win32 Error 0x{0:X} occurred when getting the width of a char.", objArray4);
																return 1;
															}
														}
														else
														{
															return 2;
														}
													}
													else
													{
														return 2;
													}
												}
												else
												{
													return 2;
												}
											}
											else
											{
												return 1;
											}
										}
										else
										{
											return 1;
										}
									}
									else
									{
										return 2;
									}
								}
								else
								{
									return 2;
								}
							}
							else
							{
								return 2;
							}
						}
						else
						{
							return 2;
						}
					}
					else
					{
						return 2;
					}
				}
				else
				{
					return 2;
				}
			}
			else
			{
				return 1;
			}
		}

		internal static void MimicKeyPress(ConsoleControl.INPUT[] inputs)
		{
			int num = ConsoleControl.NativeMethods.SendInput(inputs.Length, inputs, Marshal.SizeOf(typeof(ConsoleControl.INPUT)));
			if (num != 0)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "SendKeyPressInput", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SendKeyPressInputExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static int PeekConsoleInput(SafeFileHandle consoleHandle, ref ConsoleControl.INPUT_RECORD[] buffer)
		{
			int num = 0;
			bool flag = ConsoleControl.NativeMethods.PeekConsoleInput(consoleHandle.DangerousGetHandle(), buffer, buffer.Length, out num);
			if (flag)
			{
				return num;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "PeekConsoleInput", ErrorCategory.ReadError, ConsoleControlStrings.PeekConsoleInputExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static string ReadConsole(SafeFileHandle consoleHandle, string initialContent, int charactersToRead, bool endOnTab, out int keyState)
		{
			keyState = 0;
			ConsoleControl.CONSOLE_READCONSOLE_CONTROL length = new ConsoleControl.CONSOLE_READCONSOLE_CONTROL();
			length.nLength = Marshal.SizeOf(length);
			length.nInitialChars = initialContent.Length;
			length.dwControlKeyState = 0;
			if (endOnTab)
			{
				length.dwCtrlWakeupMask = 0x200;
			}
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder(initialContent, charactersToRead);
			bool flag = ConsoleControl.NativeMethods.ReadConsole(consoleHandle.DangerousGetHandle(), stringBuilder, charactersToRead, out num, ref length);
			keyState = length.dwControlKeyState;
			if (flag)
			{
				if (num > stringBuilder.Length)
				{
					num = stringBuilder.Length;
				}
				return stringBuilder.ToString(0, num);
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "ReadConsole", ErrorCategory.ReadError, ConsoleControlStrings.ReadConsoleExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static int ReadConsoleInput(SafeFileHandle consoleHandle, ref ConsoleControl.INPUT_RECORD[] buffer)
		{
			int num = 0;
			bool flag = ConsoleControl.NativeMethods.ReadConsoleInput(consoleHandle.DangerousGetHandle(), buffer, buffer.Length, out num);
			if (flag)
			{
				return num;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "ReadConsoleInput", ErrorCategory.ReadError, ConsoleControlStrings.ReadConsoleInputExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static void ReadConsoleOutput(SafeFileHandle consoleHandle, Coordinates origin, Rectangle contentsRegion, ref BufferCell[,] contents)
		{
			uint num = 0;
			Coordinates coordinate;
			if (!ConsoleControl.IsCJKOutputCodePage(out num))
			{
				ConsoleControl.ReadConsoleOutputPlain(consoleHandle, origin, contentsRegion, ref contents);
			}
			else
			{
				ConsoleControl.ReadConsoleOutputCJK(consoleHandle, num, origin, contentsRegion, ref contents);
				BufferCell[,] bufferCellArray = null;
				Rectangle rectangle = new Rectangle(0, 0, 1, contentsRegion.Bottom - contentsRegion.Top);
				if (origin.X > 0 && ConsoleControl.ShouldCheck(contentsRegion.Left, contents, contentsRegion))
				{
					bufferCellArray = new BufferCell[rectangle.Bottom + 1, 2];
					coordinate = new Coordinates(origin.X - 1, origin.Y);
					ConsoleControl.ReadConsoleOutputCJK(consoleHandle, num, coordinate, rectangle, ref bufferCellArray);
					for (int i = 0; i <= rectangle.Bottom; i++)
					{
						if (bufferCellArray[i, 0].BufferCellType == BufferCellType.Leading)
						{
							contents[contentsRegion.Top + i, 0].Character = '\0';
							contents[contentsRegion.Top + i, 0].BufferCellType = BufferCellType.Trailing;
						}
					}
				}
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO consoleScreenBufferInfo = ConsoleControl.GetConsoleScreenBufferInfo(consoleHandle);
				if (origin.X + contentsRegion.Right - contentsRegion.Left + 1 < consoleScreenBufferInfo.BufferSize.X && ConsoleControl.ShouldCheck(contentsRegion.Right, contents, contentsRegion))
				{
					if (bufferCellArray == null)
					{
						bufferCellArray = new BufferCell[rectangle.Bottom + 1, 2];
					}
					coordinate = new Coordinates(origin.X + contentsRegion.Right - contentsRegion.Left, origin.Y);
					ConsoleControl.ReadConsoleOutputCJK(consoleHandle, num, coordinate, rectangle, ref bufferCellArray);
					for (int j = 0; j <= rectangle.Bottom; j++)
					{
						if (bufferCellArray[j, 0].BufferCellType == BufferCellType.Leading)
						{
							contents[contentsRegion.Top + j, contentsRegion.Right] = bufferCellArray[j, 0];
						}
					}
					return;
				}
			}
		}

		[ArchitectureSensitive]
		internal static void ReadConsoleOutputCJK(SafeFileHandle consoleHandle, uint codePage, Coordinates origin, Rectangle contentsRegion, ref BufferCell[,] contents)
		{
			int bottom = contentsRegion.Bottom - contentsRegion.Top + 1;
			int right = contentsRegion.Right - contentsRegion.Left + 1;
			if (bottom <= 0 || right <= 0)
			{
				ConsoleControl.tracer.WriteLine("invalid contents region", new object[0]);
				return;
			}
			else
			{
				int num = 0x800;
				ConsoleControl.SMALL_RECT y;
                y.Top = (short)origin.Y;
				int y1 = bottom;
				while (y1 > 0)
				{
					y.Left = (short)origin.X;
					ConsoleControl.COORD cOORD;
                    cOORD.X = (short)Math.Min(right, num);
					cOORD.Y = (short)Math.Min(y1, num / cOORD.X);
					y.Bottom = (short)(y.Top + cOORD.Y - 1);
					int top = bottom - y1 + contentsRegion.Top;
					int x = right;
					while (x > 0)
					{
						int left = right - x + contentsRegion.Left;
						y.Right = (short)(y.Left + cOORD.X - 1);
						Rectangle rectangle = new Rectangle(left, top, left + cOORD.X - 1, top + cOORD.Y - 1);
						bool flag = ConsoleControl.ReadConsoleOutputCJKSmall(consoleHandle, codePage, new Coordinates(y.Left, y.Top), rectangle, ref contents);
						if (flag)
						{
							x = x - cOORD.X;
							y.Left = (short)(y.Left + cOORD.X);
							if (x > 0 && cOORD.Y == 1 && contents[rectangle.Bottom, rectangle.Right].Character == ' ')
							{
								x++;
								y.Left = (short)(y.Left - 1);
							}
							cOORD.X = (short)Math.Min(x, num);
						}
						else
						{
							if (num >= 2)
							{
								num = num / 2;
								if (right != x)
								{
									cOORD.X = (short)Math.Min(x, num);
								}
								else
								{
									cOORD.Y = 0;
									break;
								}
							}
							else
							{
								int lastWin32Error = Marshal.GetLastWin32Error();
								HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "ReadConsoleOutput", ErrorCategory.ReadError, ConsoleControlStrings.ReadConsoleOutputExceptionTemplate);
								throw hostException;
							}
						}
					}
					y1 = y1 - cOORD.Y;
					y.Top = (short)(y.Top + cOORD.Y);
				}
				int lowerBound = contents.GetLowerBound(0);
				int upperBound = contents.GetUpperBound(0);
				int lowerBound1 = contents.GetLowerBound(1);
				int upperBound1 = contents.GetUpperBound(1);
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO consoleScreenBufferInfo = ConsoleControl.GetConsoleScreenBufferInfo(consoleHandle);
				ConsoleColor consoleColor = ConsoleColor.Black;
				ConsoleColor consoleColor1 = ConsoleColor.Black;
				ConsoleControl.WORDToColor(consoleScreenBufferInfo.Attributes, out consoleColor, out consoleColor1);
				while (lowerBound <= upperBound)
				{
					int right1 = lowerBound1;
					while (true)
					{
						if (contentsRegion.Top <= lowerBound && lowerBound <= contentsRegion.Bottom && contentsRegion.Left <= right1 && right1 <= contentsRegion.Right)
						{
							right1 = contentsRegion.Right + 1;
						}
						if (right1 > upperBound1)
						{
							break;
						}
						contents[lowerBound, right1] = new BufferCell(' ', consoleColor, consoleColor1, BufferCellType.Complete);
						right1++;
					}
					lowerBound++;
				}
				return;
			}
		}

		[ArchitectureSensitive]
		private static bool ReadConsoleOutputCJKSmall(SafeFileHandle consoleHandle, uint codePage, Coordinates origin, Rectangle contentsRegion, ref BufferCell[,] contents)
		{
			ConsoleColor consoleColor = ConsoleColor.Black;
			ConsoleColor consoleColor1 = ConsoleColor.Black;
            ConsoleControl.COORD right;
            right.X = (short)(contentsRegion.Right - contentsRegion.Left + 1);
			right.Y = (short)(contentsRegion.Bottom - contentsRegion.Top + 1);
			ConsoleControl.COORD cOORD;
            cOORD.X = 0;
			cOORD.Y = 0;
			ConsoleControl.CHAR_INFO[] cHARINFOArray = new ConsoleControl.CHAR_INFO[right.X * right.Y];
			ConsoleControl.SMALL_RECT x;
            x.Left = (short)origin.X;
			x.Top = (short)origin.Y;
			x.Right = (short)(origin.X + right.X - 1);
			x.Bottom = (short)(origin.Y + right.Y - 1);
			bool flag = ConsoleControl.NativeMethods.ReadConsoleOutput(consoleHandle.DangerousGetHandle(), cHARINFOArray, right, cOORD, ref x);
			if (flag)
			{
				int num = 0;
				for (int i = contentsRegion.Top; i <= contentsRegion.Bottom; i++)
				{
					int left = contentsRegion.Left;
					while (left <= contentsRegion.Right)
					{
						contents[i, left].Character = Convert.ToChar(cHARINFOArray[num].UnicodeChar);
						ConsoleControl.WORDToColor(cHARINFOArray[num].Attributes, out consoleColor, out consoleColor1);
						contents[i, left].ForegroundColor = consoleColor;
						contents[i, left].BackgroundColor = consoleColor1;
						if ((cHARINFOArray[num].Attributes & 0x100) != 0x100)
						{
							if ((cHARINFOArray[num].Attributes & 0x200) != 0x200)
							{
								int num1 = ConsoleControl.LengthInBufferCells(contents[i, left].Character);
								if (num1 != 2)
								{
									contents[i, left].BufferCellType = BufferCellType.Complete;
								}
								else
								{
									contents[i, left].BufferCellType = BufferCellType.Leading;
									left++;
									contents[i, left].Character = '\0';
									contents[i, left].ForegroundColor = consoleColor;
									contents[i, left].BackgroundColor = consoleColor1;
									contents[i, left].BufferCellType = BufferCellType.Trailing;
								}
							}
							else
							{
								contents[i, left].Character = '\0';
								contents[i, left].BufferCellType = BufferCellType.Trailing;
							}
						}
						else
						{
							contents[i, left].BufferCellType = BufferCellType.Leading;
						}
						left++;
						num++;
					}
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		[ArchitectureSensitive]
		private static void ReadConsoleOutputPlain(SafeFileHandle consoleHandle, Coordinates origin, Rectangle contentsRegion, ref BufferCell[,] contents)
		{
			ConsoleColor consoleColor = ConsoleColor.Black;
			ConsoleColor consoleColor1 = ConsoleColor.Black;
			int bottom = contentsRegion.Bottom - contentsRegion.Top + 1;
			int right = contentsRegion.Right - contentsRegion.Left + 1;
			if (bottom <= 0 || right <= 0)
			{
				ConsoleControl.tracer.WriteLine("invalid contents region", new object[0]);
				return;
			}
			else
			{
				int num = 0x800;
				ConsoleControl.COORD cOORD;
                cOORD.X = 0;
				cOORD.Y = 0;
				ConsoleControl.SMALL_RECT y;
                y.Top = (short)origin.Y;
				int y1 = bottom;
				while (y1 > 0)
				{
					y.Left = (short)origin.X;
					ConsoleControl.COORD cOORD1;
                    cOORD1.X = (short)Math.Min(right, num);
					cOORD1.Y = (short)Math.Min(y1, num / cOORD1.X);
					y.Bottom = (short)(y.Top + cOORD1.Y - 1);
					int top = bottom - y1 + contentsRegion.Top;
					int x = right;
					while (x > 0)
					{
						y.Right = (short)(y.Left + cOORD1.X - 1);
						ConsoleControl.CHAR_INFO[] cHARINFOArray = new ConsoleControl.CHAR_INFO[cOORD1.Y * cOORD1.X];
						bool flag = ConsoleControl.NativeMethods.ReadConsoleOutput(consoleHandle.DangerousGetHandle(), cHARINFOArray, cOORD1, cOORD, ref y);
						if (flag)
						{
							int left = right - x + contentsRegion.Left;
							int num1 = 0;
							for (int i = top; i < cOORD1.Y + top; i++)
							{
								int num2 = left;
								while (num2 < cOORD1.X + left)
								{
									contents[i, num2].Character = Convert.ToChar(cHARINFOArray[num1].UnicodeChar);
									ConsoleControl.WORDToColor(cHARINFOArray[num1].Attributes, out consoleColor, out consoleColor1);
									contents[i, num2].ForegroundColor = consoleColor;
									contents[i, num2].BackgroundColor = consoleColor1;
									num2++;
									num1++;
								}
							}
							x = x - cOORD1.X;
							y.Left = (short)(y.Left + cOORD1.X);
							cOORD1.X = (short)Math.Min(x, num);
						}
						else
						{
							if (num >= 2)
							{
								num = num / 2;
								if (right != x)
								{
									cOORD1.X = (short)Math.Min(x, num);
								}
								else
								{
									cOORD1.Y = 0;
									break;
								}
							}
							else
							{
								int lastWin32Error = Marshal.GetLastWin32Error();
								HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "ReadConsoleOutput", ErrorCategory.ReadError, ConsoleControlStrings.ReadConsoleOutputExceptionTemplate);
								throw hostException;
							}
						}
					}
					y1 = y1 - cOORD1.Y;
					y.Top = (short)(y.Top + cOORD1.Y);
				}
				int lowerBound = contents.GetLowerBound(0);
				int upperBound = contents.GetUpperBound(0);
				int lowerBound1 = contents.GetLowerBound(1);
				int upperBound1 = contents.GetUpperBound(1);
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO consoleScreenBufferInfo = ConsoleControl.GetConsoleScreenBufferInfo(consoleHandle);
				ConsoleColor consoleColor2 = ConsoleColor.Black;
				ConsoleColor consoleColor3 = ConsoleColor.Black;
				ConsoleControl.WORDToColor(consoleScreenBufferInfo.Attributes, out consoleColor2, out consoleColor3);
				while (lowerBound <= upperBound)
				{
					int right1 = lowerBound1;
					while (true)
					{
						if (contentsRegion.Top <= lowerBound && lowerBound <= contentsRegion.Bottom && contentsRegion.Left <= right1 && right1 <= contentsRegion.Right)
						{
							right1 = contentsRegion.Right + 1;
						}
						if (right1 > upperBound1)
						{
							break;
						}
						contents[lowerBound, right1].Character = ' ';
						contents[lowerBound, right1].ForegroundColor = consoleColor2;
						contents[lowerBound, right1].BackgroundColor = consoleColor3;
						right1++;
					}
					lowerBound++;
				}
				return;
			}
		}

		internal static void RemoveBreakHandler()
		{
			bool flag = ConsoleControl.NativeMethods.SetConsoleCtrlHandler(null, false);
			if (flag)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "RemoveBreakHandler", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.RemoveBreakHandlerExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static void ScrollConsoleScreenBuffer(SafeFileHandle consoleHandle, ConsoleControl.SMALL_RECT scrollRectangle, ConsoleControl.SMALL_RECT clipRectangle, ConsoleControl.COORD destOrigin, ConsoleControl.CHAR_INFO fill)
		{
			bool flag = ConsoleControl.NativeMethods.ScrollConsoleScreenBuffer(consoleHandle.DangerousGetHandle(), ref scrollRectangle, ref clipRectangle, destOrigin, ref fill);
			if (flag)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "ScrollConsoleScreenBuffer", ErrorCategory.WriteError, ConsoleControlStrings.ScrollConsoleScreenBufferExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static void SetConsoleCursorInfo(SafeFileHandle consoleHandle, ConsoleControl.CONSOLE_CURSOR_INFO cursorInfo)
		{
			bool flag = ConsoleControl.NativeMethods.SetConsoleCursorInfo(consoleHandle.DangerousGetHandle(), ref cursorInfo);
			if (flag)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "SetConsoleCursorInfo", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetConsoleCursorInfoExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static void SetConsoleCursorPosition(SafeFileHandle consoleHandle, Coordinates cursorPosition)
		{
			ConsoleControl.COORD x;
            x.X = (short)cursorPosition.X;
			x.Y = (short)cursorPosition.Y;
			bool flag = ConsoleControl.NativeMethods.SetConsoleCursorPosition(consoleHandle.DangerousGetHandle(), x);
			if (flag)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "SetConsoleCursorPosition", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetConsoleCursorPositionExceptionTemplate);
				throw hostException;
			}
		}

		internal static void SetConsoleMode(ProcessWindowStyle style)
		{
			IntPtr consoleWindow = ConsoleControl.GetConsoleWindow();
			ProcessWindowStyle processWindowStyle = style;
			switch (processWindowStyle)
			{
				case ProcessWindowStyle.Normal:
				{
					ConsoleControl.ShowWindow(consoleWindow, 1);
					return;
				}
				case ProcessWindowStyle.Hidden:
				{
					ConsoleControl.ShowWindow(consoleWindow, 0);
					return;
				}
				case ProcessWindowStyle.Minimized:
				{
					ConsoleControl.ShowWindow(consoleWindow, 6);
					return;
				}
				case ProcessWindowStyle.Maximized:
				{
					ConsoleControl.ShowWindow(consoleWindow, 3);
					return;
				}
				default:
				{
					return;
				}
			}
		}

		[ArchitectureSensitive]
		internal static void SetConsoleScreenBufferSize(SafeFileHandle consoleHandle, Size newSize)
		{
			ConsoleControl.COORD width;
            width.X = (short)newSize.Width;
			width.Y = (short)newSize.Height;
			bool flag = ConsoleControl.NativeMethods.SetConsoleScreenBufferSize(consoleHandle.DangerousGetHandle(), width);
			if (flag)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "SetConsoleScreenBufferSize", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetConsoleScreenBufferSizeExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static void SetConsoleTextAttribute(SafeFileHandle consoleHandle, ushort attribute)
		{
			bool flag = ConsoleControl.NativeMethods.SetConsoleTextAttribute(consoleHandle.DangerousGetHandle(), attribute);
			if (flag)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "SetConsoleTextAttribute", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetConsoleTextAttributeExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static void SetConsoleWindowInfo(SafeFileHandle consoleHandle, bool absolute, ConsoleControl.SMALL_RECT windowInfo)
		{
			bool flag = ConsoleControl.NativeMethods.SetConsoleWindowInfo(consoleHandle.DangerousGetHandle(), absolute, ref windowInfo);
			if (flag)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "SetConsoleWindowInfo", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetConsoleWindowInfoExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static void SetConsoleWindowTitle(string consoleTitle)
		{
			bool flag = ConsoleControl.NativeMethods.SetConsoleTitle(consoleTitle);
			if (flag)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "SetConsoleWindowTitle", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetConsoleWindowTitleExceptionTemplate);
				throw hostException;
			}
		}

		[ArchitectureSensitive]
		internal static void SetMode(SafeFileHandle consoleHandle, ConsoleControl.ConsoleModes mode)
		{
			bool flag = ConsoleControl.NativeMethods.SetConsoleMode(consoleHandle.DangerousGetHandle(), (int)mode);
			if (flag)
			{
				return;
			}
			else
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "SetConsoleMode", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetModeExceptionTemplate);
				throw hostException;
			}
		}

		private static bool ShouldCheck(int edge, BufferCell[,] contents, Rectangle contentsRegion)
		{
			int top = contentsRegion.Top;
			while (top <= contentsRegion.Bottom)
			{
				if (contents[top, edge].Character != ' ')
				{
					top++;
				}
				else
				{
					return true;
				}
			}
			return false;
		}

		[DllImport("user32.dll", CharSet=CharSet.None)]
		internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		internal static void UpdateLocaleSpecificFont()
		{
			bool flag;
			int num = 0x1b5;
			int num1 = 0x3a8;
			int num2 = 0x3b6;
			int consoleCP = ConsoleControl.NativeMethods.GetConsoleCP();
			SafeFileHandle activeScreenBufferHandle = ConsoleControl.GetActiveScreenBufferHandle();
			ConsoleControl.CONSOLE_FONT_INFO_EX consoleFontInfo = ConsoleControl.GetConsoleFontInfo(activeScreenBufferHandle);
			if (consoleCP == num || consoleCP == num1 || consoleCP == num2)
			{
				flag = true;
			}
			else
			{
				flag = false;
			}
			bool flag1 = flag;
			if (!flag1 && consoleFontInfo.cbSize == 84 && consoleFontInfo.FontFace.Equals("Lucida Console", StringComparison.OrdinalIgnoreCase) && consoleFontInfo.FontFamily == 54 && consoleFontInfo.FontHeight == 12 && consoleFontInfo.FontWidth == 7 && consoleFontInfo.FontWeight == 0x190)
			{
				ConsoleControl.CONSOLE_FONT_INFO_EX cONSOLEFONTINFOEX = new ConsoleControl.CONSOLE_FONT_INFO_EX();
				cONSOLEFONTINFOEX.cbSize = Marshal.SizeOf(cONSOLEFONTINFOEX);
				cONSOLEFONTINFOEX.FontFace = "Terminal";
				cONSOLEFONTINFOEX.FontFamily = 48;
				cONSOLEFONTINFOEX.FontHeight = 12;
				cONSOLEFONTINFOEX.FontWidth = 8;
				cONSOLEFONTINFOEX.FontWeight = 0x190;
				cONSOLEFONTINFOEX.nFont = 6;
				bool flag2 = ConsoleControl.NativeMethods.SetCurrentConsoleFontEx(activeScreenBufferHandle.DangerousGetHandle(), false, ref cONSOLEFONTINFOEX);
				if (!flag2)
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "SetConsoleFontInfo", ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetConsoleFontInfoExceptionTemplate);
					throw hostException;
				}
			}
		}

		[ArchitectureSensitive]
		internal static void WORDToColor(ushort attribute, out ConsoleColor foreground, out ConsoleColor background)
		{
			foreground = (ConsoleColor)(attribute & 15);
			background = (ConsoleColor)((attribute & 240) >> 4);
		}

		[ArchitectureSensitive]
		internal static void WriteConsole(SafeFileHandle consoleHandle, string output)
		{
			string str;
			int num = 0;
			if (!string.IsNullOrEmpty(output))
			{
				int length = 0;
				while (length < output.Length)
				{
					if (length + 0x3fff >= output.Length)
					{
						str = output.Substring(length);
						length = output.Length;
					}
					else
					{
						str = output.Substring(length, 0x3fff);
						length = length + 0x3fff;
					}
					bool flag = ConsoleControl.NativeMethods.WriteConsole(consoleHandle.DangerousGetHandle(), str, str.Length, out num, IntPtr.Zero);
					if (flag)
					{
						continue;
					}
					int lastWin32Error = Marshal.GetLastWin32Error();
					HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "WriteConsole", ErrorCategory.WriteError, ConsoleControlStrings.WriteConsoleExceptionTemplate);
					throw hostException;
				}
				return;
			}
			else
			{
				return;
			}
		}

		[ArchitectureSensitive]
		internal static void WriteConsoleOutput(SafeFileHandle consoleHandle, Coordinates origin, BufferCell[,] contents)
		{
			uint num = 0;
			if (contents != null)
			{
				if (!ConsoleControl.IsCJKOutputCodePage(out num))
				{
					ConsoleControl.WriteConsoleOutputPlain(consoleHandle, origin, contents);
				}
				else
				{
					Rectangle lowerBound = new Rectangle();
					ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO consoleScreenBufferInfo = ConsoleControl.GetConsoleScreenBufferInfo(consoleHandle);
					int x = consoleScreenBufferInfo.BufferSize.X;
					int y = consoleScreenBufferInfo.BufferSize.Y;
					Rectangle rectangle = new Rectangle(origin.X, origin.Y, Math.Min(origin.X + contents.GetLength(1) - 1, x - 1), Math.Min(origin.Y + contents.GetLength(0) - 1, y - 1));
					lowerBound.Left = contents.GetLowerBound(1);
					lowerBound.Top = contents.GetLowerBound(0);
					lowerBound.Right = lowerBound.Left + rectangle.Right - rectangle.Left;
					lowerBound.Bottom = lowerBound.Top + rectangle.Bottom - rectangle.Top;
					ConsoleControl.CheckWriteConsoleOutputContents(contents, lowerBound);
					List<ConsoleControl.BufferCellArrayRowTypeRange> bufferCellArrayRowTypeRanges = new List<ConsoleControl.BufferCellArrayRowTypeRange>();
					int num1 = -1;
					int num2 = -1;
					ConsoleControl.BuildEdgeTypeInfo(lowerBound, contents, bufferCellArrayRowTypeRanges, out num1, out num2);
					ConsoleControl.CheckWriteEdges(consoleHandle, num, origin, contents, lowerBound, consoleScreenBufferInfo, num1, num2);
					foreach (ConsoleControl.BufferCellArrayRowTypeRange bufferCellArrayRowTypeRange in bufferCellArrayRowTypeRanges)
					{
						Coordinates coordinate = new Coordinates(origin.X, origin.Y + bufferCellArrayRowTypeRange.Start - lowerBound.Top);
						Rectangle left = new Rectangle(lowerBound.Left, bufferCellArrayRowTypeRange.Start, lowerBound.Right, bufferCellArrayRowTypeRange.End);
						if ((bufferCellArrayRowTypeRange.Type & ConsoleControl.BufferCellArrayRowType.LeftTrailing) != 0)
						{
							left.Left = left.Left + 1;
							coordinate.X = coordinate.X + 1;
							if (coordinate.X >= x || left.Right < left.Left)
							{
								return;
							}
						}
						ConsoleControl.WriteConsoleOutputCJK(consoleHandle, coordinate, left, contents, bufferCellArrayRowTypeRange.Type);
					}
				}
				return;
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("contents");
			}
		}

		[ArchitectureSensitive]
		private static void WriteConsoleOutputCJK(SafeFileHandle consoleHandle, Coordinates origin, Rectangle contentsRegion, BufferCell[,] contents, ConsoleControl.BufferCellArrayRowType rowType)
		{
			bool flag;
			int bottom = contentsRegion.Bottom - contentsRegion.Top + 1;
			int right = contentsRegion.Right - contentsRegion.Left + 1;
			ConsoleControl.CONSOLE_FONT_INFO_EX consoleFontInfo = ConsoleControl.GetConsoleFontInfo(consoleHandle);
			int fontFamily = consoleFontInfo.FontFamily & 6;
			bool flag1 = (fontFamily & 4) == 4;
			int num = 0x800;
			ConsoleControl.COORD cOORD;
            cOORD.X = 0;
			cOORD.Y = 0;
			ConsoleControl.SMALL_RECT y;
            y.Top = (short)origin.Y;
			int y1 = bottom;
			while (y1 > 0)
			{
				y.Left = (short)origin.X;
				ConsoleControl.COORD x;
                x.X = (short)Math.Min(right, num);
				x.Y = (short)Math.Min(y1, num / x.X);
				y.Bottom = (short)(y.Top + x.Y - 1);
				int top = bottom - y1 + contentsRegion.Top;
				int x1 = right;
				while (x1 > 0)
				{
					y.Right = (short)(y.Left + x.X - 1);
					int left = right - x1 + contentsRegion.Left;
					if (x1 > x.X && contents[top, left + x.X - 1].BufferCellType == BufferCellType.Leading)
					{
						x.X = (short)(x.X - 1);
						y.Right = (short)(y.Right - 1);
					}
					ConsoleControl.CHAR_INFO[] character = new ConsoleControl.CHAR_INFO[x.Y * x.X];
					int num1 = 0;
					bool flag2 = false;
					BufferCell bufferCell = new BufferCell();
					for (int i = top; i < x.Y + top; i++)
					{
						int num2 = left;
						while (num2 < x.X + left)
						{
							if (contents[i, num2].BufferCellType != BufferCellType.Complete)
							{
								if (contents[i, num2].BufferCellType != BufferCellType.Leading)
								{
									if (contents[i, num2].BufferCellType == BufferCellType.Trailing)
									{
										if (!flag2 || !flag1)
										{
											num1--;
										}
										else
										{
											character[num1].UnicodeChar = bufferCell.Character;
											character[num1].Attributes = (ushort)(ConsoleControl.ColorToWORD(contents[i, num2].ForegroundColor, contents[i, num2].BackgroundColor) | 0x200);
										}
										flag2 = false;
									}
								}
								else
								{
									character[num1].UnicodeChar = contents[i, num2].Character;
									character[num1].Attributes = (ushort)(ConsoleControl.ColorToWORD(contents[i, num2].ForegroundColor, contents[i, num2].BackgroundColor) | 0x100);
									flag2 = true;
									bufferCell = contents[i, num2];
								}
							}
							else
							{
								character[num1].UnicodeChar = contents[i, num2].Character;
								character[num1].Attributes = ConsoleControl.ColorToWORD(contents[i, num2].ForegroundColor, contents[i, num2].BackgroundColor);
								flag2 = false;
							}
							num2++;
							num1++;
						}
					}
					if ((rowType & ConsoleControl.BufferCellArrayRowType.RightLeading) == 0 || x1 != x.X)
					{
						flag = ConsoleControl.NativeMethods.WriteConsoleOutput(consoleHandle.DangerousGetHandle(), character, x, cOORD, ref y);
					}
					else
					{
						ConsoleControl.COORD cOORD1 = x;
						cOORD1.X = (short)(cOORD1.X + 1);
						ConsoleControl.SMALL_RECT sMALLRECT = y;
						sMALLRECT.Right = (short)(sMALLRECT.Right + 1);
						flag = ConsoleControl.NativeMethods.WriteConsoleOutput(consoleHandle.DangerousGetHandle(), character, cOORD1, cOORD, ref sMALLRECT);
					}
					if (flag)
					{
						x1 = x1 - x.X;
						y.Left = (short)(y.Left + x.X);
						x.X = (short)Math.Min(x1, num);
					}
					else
					{
						if (num >= 2)
						{
							num = num / 2;
							if (right != x1)
							{
								x.X = (short)Math.Min(x1, num);
							}
							else
							{
								x.Y = 0;
								break;
							}
						}
						else
						{
							int lastWin32Error = Marshal.GetLastWin32Error();
							HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "WriteConsoleOutput", ErrorCategory.WriteError, ConsoleControlStrings.WriteConsoleOutputExceptionTemplate);
							throw hostException;
						}
					}
				}
				y1 = y1 - x.Y;
				y.Top = (short)(y.Top + x.Y);
			}
		}

		[ArchitectureSensitive]
		private static void WriteConsoleOutputPlain(SafeFileHandle consoleHandle, Coordinates origin, BufferCell[,] contents)
		{
			int length = contents.GetLength(0);
			int num = contents.GetLength(1);
			if (length <= 0 || num <= 0)
			{
				ConsoleControl.tracer.WriteLine("contents passed in has 0 rows and columns", new object[0]);
				return;
			}
			else
			{
				int num1 = 0x800;
				ConsoleControl.COORD cOORD;
                cOORD.X = 0;
				cOORD.Y = 0;
				ConsoleControl.SMALL_RECT y;
                y.Top = (short)origin.Y;
				int y1 = length;
				while (y1 > 0)
				{
					y.Left = (short)origin.X;
					ConsoleControl.COORD cOORD1;
                    cOORD1.X = (short)Math.Min(num, num1);
					cOORD1.Y = (short)Math.Min(y1, num1 / cOORD1.X);
					y.Bottom = (short)(y.Top + cOORD1.Y - 1);
					int lowerBound = length - y1 + contents.GetLowerBound(0);
					int x = num;
					while (x > 0)
					{
						y.Right = (short)(y.Left + cOORD1.X - 1);
						int lowerBound1 = num - x + contents.GetLowerBound(1);
						ConsoleControl.CHAR_INFO[] character = new ConsoleControl.CHAR_INFO[cOORD1.Y * cOORD1.X];
						int num2 = lowerBound;
						int num3 = 0;
						while (num2 < cOORD1.Y + lowerBound)
						{
							int num4 = lowerBound1;
							while (num4 < cOORD1.X + lowerBound1)
							{
								character[num3].UnicodeChar = contents[num2, num4].Character;
								character[num3].Attributes = ConsoleControl.ColorToWORD(contents[num2, num4].ForegroundColor, contents[num2, num4].BackgroundColor);
								num4++;
								num3++;
							}
							num2++;
						}
						bool flag = ConsoleControl.NativeMethods.WriteConsoleOutput(consoleHandle.DangerousGetHandle(), character, cOORD1, cOORD, ref y);
						if (flag)
						{
							x = x - cOORD1.X;
							y.Left = (short)(y.Left + cOORD1.X);
							cOORD1.X = (short)Math.Min(x, num1);
						}
						else
						{
							if (num1 >= 2)
							{
								num1 = num1 / 2;
								if (num != x)
								{
									cOORD1.X = (short)Math.Min(x, num1);
								}
								else
								{
									cOORD1.Y = 0;
									break;
								}
							}
							else
							{
								int lastWin32Error = Marshal.GetLastWin32Error();
								HostException hostException = ConsoleControl.CreateHostException(lastWin32Error, "WriteConsoleOutput", ErrorCategory.WriteError, ConsoleControlStrings.WriteConsoleOutputExceptionTemplate);
								throw hostException;
							}
						}
					}
					y1 = y1 - cOORD1.Y;
					y.Top = (short)(y.Top + cOORD1.Y);
				}
				return;
			}
		}

		internal delegate bool BreakHandler(ConsoleControl.ConsoleBreakSignal ConsoleBreakSignal);

		[Flags]
		private enum BufferCellArrayRowType : uint
		{
			LeftTrailing = 1,
			RightLeading = 2
		}

		private struct BufferCellArrayRowTypeRange
		{
			internal int Start;

			internal int End;

			internal ConsoleControl.BufferCellArrayRowType Type;

		}

		internal struct CHAR_INFO
		{
			internal ushort UnicodeChar;

			internal ushort Attributes;

		}

		internal struct CHARSETINFO
		{
			internal uint ciCharset;

			internal uint ciACP;

			internal ConsoleControl.FONTSIGNATURE fs;

		}

		internal struct CONSOLE_CURSOR_INFO
		{
			internal int Size;

			internal bool Visible;

			public override string ToString()
			{
				object[] size = new object[2];
				size[0] = this.Size;
				size[1] = this.Visible;
				return string.Format(CultureInfo.InvariantCulture, "Size: {0}, Visible: {1}", size);
			}
		}

		internal struct CONSOLE_FONT_INFO_EX
		{
			internal int cbSize;

			internal int nFont;

			internal short FontWidth;

			internal short FontHeight;

			internal int FontFamily;

			internal int FontWeight;

			internal string FontFace;

		}

		internal struct CONSOLE_READCONSOLE_CONTROL
		{
			internal int nLength;

			internal int nInitialChars;

			internal int dwCtrlWakeupMask;

			internal int dwControlKeyState;

		}

		internal struct CONSOLE_SCREEN_BUFFER_INFO
		{
			internal ConsoleControl.COORD BufferSize;

			internal ConsoleControl.COORD CursorPosition;

			internal ushort Attributes;

			internal ConsoleControl.SMALL_RECT WindowRect;

			internal ConsoleControl.COORD MaxWindowSize;

			internal uint Padding;

		}

		internal enum ConsoleBreakSignal : uint
		{
			CtrlC = 0,
			CtrlBreak = 1,
			Close = 2,
			Logoff = 5,
			Shutdown = 6,
			None = 255
		}

		[Flags]
		internal enum ConsoleModes : int
		{
			ProcessedInput = 1,
			ProcessedOutput = 1,
			LineInput = 2,
			WrapEndOfLine = 2,
			EchoInput = 4,
			WindowInput = 8,
			MouseInput = 16,
			Insert = 32,
			QuickEdit = 64,
			Extended = 128,
			AutoPosition = 256
		}

		[Flags]
		internal enum ControlKeyStates : int
		{
			RIGHT_ALT_PRESSED = 1,
			LEFT_ALT_PRESSED = 2,
			RIGHT_CTRL_PRESSED = 4,
			LEFT_CTRL_PRESSED = 8,
			SHIFT_PRESSED = 16,
			NUMLOCK_ON = 32,
			SCROLLLOCK_ON = 64,
			CAPSLOCK_ON = 128,
			ENHANCED_KEY = 256
		}

		internal struct COORD
		{
			internal short X;

			internal short Y;

			public override string ToString()
			{
				object[] x = new object[2];
				x[0] = this.X;
				x[1] = this.Y;
				return string.Format(CultureInfo.InvariantCulture, "{0},{1}", x);
			}
		}

		internal struct FONTSIGNATURE
		{
			internal uint fsUsb0;

			internal uint fsUsb1;

			internal uint fsUsb2;

			internal uint fsUsb3;

			internal uint fsCsb0;

			internal uint fsCsb1;

		}

		internal struct HardwareInput
		{
			internal uint Msg;

			internal ushort ParamL;

			internal ushort ParamH;

		}

		internal struct INPUT
		{
			internal uint Type;

			internal ConsoleControl.MouseKeyboardHardwareInput Data;

		}

		internal struct INPUT_RECORD
		{
			internal ushort EventType;

			internal ConsoleControl.KEY_EVENT_RECORD KeyEvent;

		}

		internal enum InputRecordEventTypes : ushort
		{
			KEY_EVENT = 1,
			MOUSE_EVENT = 2,
			WINDOW_BUFFER_SIZE_EVENT = 4,
			MENU_EVENT = 8,
			FOCUS_EVENT = 16
		}

		internal enum InputType : int
		{
			Mouse,
			Keyboard,
			Hardware
		}

		internal struct KEY_EVENT_RECORD
		{
			internal bool KeyDown;

			internal ushort RepeatCount;

			internal ushort VirtualKeyCode;

			internal ushort VirtualScanCode;

			internal char UnicodeChar;

			internal uint ControlKeyState;

		}

		internal enum KeyboardFlag : uint
		{
			ExtendedKey = 1,
			KeyUp = 2,
			Unicode = 4,
			ScanCode = 8
		}

		internal struct KeyboardInput
		{
			internal ushort Vk;

			internal ushort Scan;

			internal uint Flags;

			internal uint Time;

			internal IntPtr ExtraInfo;

		}

		internal struct MouseInput
		{
			internal int X;

			internal int Y;

			internal uint MouseData;

			internal uint Flags;

			internal uint Time;

			internal IntPtr ExtraInfo;

		}

		[StructLayout(LayoutKind.Explicit)]
		internal struct MouseKeyboardHardwareInput
		{
			[FieldOffset(0)]
			internal ConsoleControl.MouseInput Mouse;

			[FieldOffset(0)]
			internal ConsoleControl.KeyboardInput Keyboard;

			[FieldOffset(0)]
			internal ConsoleControl.HardwareInput Hardware;

		}

		internal static class NativeMethods
		{
			internal const int FontTypeMask = 6;

			internal const int TrueTypeFont = 4;

			internal readonly static IntPtr INVALID_HANDLE_VALUE;

			static NativeMethods()
			{
				ConsoleControl.NativeMethods.INVALID_HANDLE_VALUE = new IntPtr(-1);
			}

			/*
			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern IntPtr CreateFile(string fileName, int desiredAccess, int ShareModes, IntPtr securityAttributes, uint creationDisposition, int flagsAndAttributes, IntPtr templateFileWin32Handle);

			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool FillConsoleOutputAttribute(IntPtr consoleOutput, ushort attribute, int length, ConsoleControl.COORD writeCoord, out int numberOfAttrsWritten);

			[DllImport("KERNEL32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool FillConsoleOutputCharacter(IntPtr consoleOutput, char character, int length, ConsoleControl.COORD writeCoord, out int numberOfCharsWritten);

			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool FlushConsoleInputBuffer(IntPtr consoleInput);

			[DllImport("GDI32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool GetCharWidth32(IntPtr hdc, uint first, uint last, out int width);

			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern int GetConsoleCP();

			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool GetConsoleMode(IntPtr consoleHandle, out int mode);
            */

			internal static IntPtr CreateFile(string fileName, int desiredAccess, int ShareModes, IntPtr securityAttributes, uint creationDisposition, int flagsAndAttributes, IntPtr templateFileWin32Handle)
			{
				return IntPtr.Zero;
			}

			internal static bool FillConsoleOutputAttribute(IntPtr consoleOutput, ushort attribute, int length, ConsoleControl.COORD writeCoord, out int numberOfAttrsWritten)
			{
				numberOfAttrsWritten = length;
				return true;
			}

			internal static bool FillConsoleOutputCharacter(IntPtr consoleOutput, char character, int length, ConsoleControl.COORD writeCoord, out int numberOfCharsWritten)
			{
				numberOfCharsWritten = length;
				return true;
			}

			internal static bool FlushConsoleInputBuffer(IntPtr consoleInput)
			{
				return true;
			}

			internal static bool GetCharWidth32 (IntPtr hdc, uint first, uint last, out int width)
			{
				width = 10;
				return true;
			}

			internal static int GetConsoleCP()
            {
                return 437;
            }

			internal static bool GetConsoleCursorInfo (IntPtr consoleOutput, out ConsoleControl.CONSOLE_CURSOR_INFO consoleCursorInfo)
			{
				ConsoleControl.CONSOLE_CURSOR_INFO c = new ConsoleControl.CONSOLE_CURSOR_INFO();
				c.Size = Console.CursorSize;
				c.Visible = Console.CursorVisible;
				consoleCursorInfo = c;
				return true;
			}

            
            internal static bool GetConsoleMode(IntPtr consoleHandle, out int mode)
            {
                mode = __mode;
                return true;
            }


			/*
			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern uint GetConsoleOutputCP();

			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool GetConsoleScreenBufferInfo(IntPtr consoleHandle, out ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO consoleScreenBufferInfo);
			*/

			internal static uint GetConsoleOutputCP()
			{
				return 0;
			}

			internal static bool GetConsoleScreenBufferInfo(IntPtr consoleHandle, out ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO consoleScreenBufferInfo)
			{
				ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO c = new CONSOLE_SCREEN_BUFFER_INFO();
				c.BufferSize.X = (short)Console.BufferWidth;
				c.BufferSize.Y = (short)Console.BufferHeight;
				c.CursorPosition.X = (short)Console.CursorLeft;
				c.CursorPosition.Y = (short)Console.CursorTop;
				c.MaxWindowSize.X = (short)Console.LargestWindowWidth;
				c.MaxWindowSize.Y = (short)Console.LargestWindowHeight;
				consoleScreenBufferInfo = c;
				return true;
			}

			/*
			[DllImport("KERNEL32.dll", CharSet=CharSet.Unicode)]
			internal static extern uint GetConsoleTitle(StringBuilder consoleTitle, int size);
			*/

			internal static uint GetConsoleTitle(StringBuilder consoleTitle, int size)
			{
				Console.Title = consoleTitle.ToString ();
				return 0;
			}

			/*
			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern IntPtr GetConsoleWindow();
			*/

			internal static IntPtr GetConsoleWindow()
			{
				return IntPtr.Zero;
			}

            /*
			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool GetCurrentConsoleFontEx(IntPtr consoleOutput, bool bMaximumWindow, ref ConsoleControl.CONSOLE_FONT_INFO_EX consoleFontInfo);
            */

            internal static bool GetCurrentConsoleFontEx(IntPtr consoleOutput, bool bMaximumWindow, ref ConsoleControl.CONSOLE_FONT_INFO_EX consoleFontInfo)
            {
                return true;
            }

			/*

			[DllImport("User32.dll", CharSet=CharSet.Unicode)]
			internal static extern IntPtr GetDC(IntPtr hwnd);
			*/

			internal static IntPtr GetDC (IntPtr hwnd)
			{
				return hwnd;
			}

			/*
			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern ConsoleControl.COORD GetLargestConsoleWindowSize(IntPtr consoleOutput);
			*/

			internal static ConsoleControl.COORD GetLargestConsoleWindowSize(IntPtr consoleOutput)
			{
				ConsoleControl.COORD c;
				c.X = (short)Console.LargestWindowWidth;
				c.Y = (short)Console.LargestWindowHeight;
				return c;
			}

			/*
			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool GetNumberOfConsoleInputEvents(IntPtr consoleInput, out int numberOfEvents);
			*/
			internal static bool GetNumberOfConsoleInputEvents(IntPtr consoleInput, out int numberOfEvents)
			{
				numberOfEvents = 0;
				return true;
			}


			/* 
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern IntPtr GetStdHandle(long handleId);
            */

            internal static IntPtr GetStdHandle(long handleId)
            {
                return IntPtr.Zero;
            }

            /*
			[DllImport("GDI32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool GetTextMetrics(IntPtr hdc, out ConsoleControl.TEXTMETRIC tm);
            */

            internal static bool GetTextMetrics(IntPtr hdc, out ConsoleControl.TEXTMETRIC tm)
            {
                ConsoleControl.TEXTMETRIC rtm = new ConsoleControl.TEXTMETRIC();
                rtm.tmDefaultChar = Char.MinValue;
                tm = rtm;
                return true;
            }

			/*
			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool PeekConsoleInput(IntPtr consoleInput, ConsoleControl.INPUT_RECORD[] buffer, int length, out int numberOfEventsRead);

			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool ReadConsole(IntPtr consoleInput, StringBuilder buffer, int numberOfCharsToRead, out int numberOfCharsRead, ref ConsoleControl.CONSOLE_READCONSOLE_CONTROL controlData);

			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool ReadConsoleInput(IntPtr consoleInput, ConsoleControl.INPUT_RECORD[] buffer, int length, out int numberOfEventsRead);

			[DllImport("KERNEL32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool ReadConsoleOutput(IntPtr consoleOutput, ConsoleControl.CHAR_INFO[] buffer, ConsoleControl.COORD bufferSize, ConsoleControl.COORD bufferCoord, ref ConsoleControl.SMALL_RECT readRegion);

			[DllImport("User32.dll", CharSet=CharSet.Unicode)]
			internal static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool ScrollConsoleScreenBuffer(IntPtr consoleOutput, ref ConsoleControl.SMALL_RECT scrollRectangle, ref ConsoleControl.SMALL_RECT clipRectangle, ConsoleControl.COORD destinationOrigin, ref ConsoleControl.CHAR_INFO fill);

			[DllImport("user32.dll", CharSet=CharSet.Unicode)]
			internal static extern int SendInput(int inputNumbers, ConsoleControl.INPUT[] inputs, int sizeOfInput);

			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool SetConsoleCtrlHandler(ConsoleControl.BreakHandler handlerRoutine, bool add);
			*/

			internal static bool PeekConsoleInput(IntPtr consoleInput, ConsoleControl.INPUT_RECORD[] buffer, int length, out int numberOfEventsRead)
			{
				numberOfEventsRead = 0;
				return true;
			}
			private static LineEditor editor = new LineEditor ("PowerShell") { Initial = "", ExternalComplete = true, TabAtStartCompletes = true };

			public static LineEditor Editor {
				get { return editor; }
			}

			public class ConsoleHistory : LineEditor.IExternalHistory
			{
				private int _index =  -1;
				private Microsoft.PowerShell.Commands.HistoryInfo[] _entries;

				#region IExternalHistory implementation

				public string Next (string currentText)
				{
					if (!currentText.Equals (lastCommand, StringComparison.OrdinalIgnoreCase)) lastCommand = "";
					if (!string.IsNullOrEmpty (lastInput) && currentText.StartsWith (lastInput, StringComparison.OrdinalIgnoreCase)) currentText = lastInput;
					if (_entries == null) _entries = ConsoleHost.SingletonInstance.LocalRunspace.History.GetEntries (0, 4096, true).Distinct (historyInfoCommandEqualityComparer).ToArray ();
					if (_entries.Length == 0) return null;
					if (_index == -1) _index = _entries.Length;
					_index--;
					if (_index == -1) _index = _entries.Length -1;
					string newCommand = null;
					if (_entries.Length == 0) return null;
					if (!string.IsNullOrEmpty (currentText)) {
						for (var jj = Math.Max (_index - 1, 0); jj >= 0; jj--) {
							if (_entries [jj].CommandLine.StartsWith (currentText, StringComparison.OrdinalIgnoreCase)  && !lastCommand.Equals (_entries [jj].CommandLine,StringComparison.OrdinalIgnoreCase)) {
								newCommand = _entries [jj].CommandLine;
								_index = jj;
								break;
							}
						}
					}
					if (string.IsNullOrEmpty (newCommand)) {
						if (!string.IsNullOrEmpty (currentText)) {
							for (var jj = _entries.Length - 1;jj >= Math.Max (_index - 1, 0); jj--) {
								if (_entries [jj].CommandLine.StartsWith (currentText, StringComparison.OrdinalIgnoreCase) && !lastCommand.Equals (_entries [jj].CommandLine,StringComparison.OrdinalIgnoreCase)) {
									newCommand = _entries [jj].CommandLine;
									_index = jj;
									break;
								}
							}
						}
						if (string.IsNullOrEmpty (newCommand)) {
							newCommand = _entries.ElementAt (_index).CommandLine;
						}
					}
					lastCommand = newCommand;
					return newCommand;
				}

				private string lastCommand = "";
				private string lastInput = "";

				public string Previous (string currentText)
				{
					if (!currentText.Equals (lastCommand, StringComparison.OrdinalIgnoreCase)) lastCommand = "";
					if (!string.IsNullOrEmpty (lastInput) && currentText.StartsWith (lastInput, StringComparison.OrdinalIgnoreCase)) currentText = lastInput;
					if (_entries == null) _entries = ConsoleHost.SingletonInstance.LocalRunspace.History.GetEntries (0, 4096, true).Distinct(historyInfoCommandEqualityComparer).ToArray();
					if (_entries.Length == 0) return null;
					if (_index == _entries.Length - 1) _index = -1;
					_index++;
					string newCommand = null;
					if (!string.IsNullOrEmpty (currentText)) {
						for (var jj = _index; jj < _entries.Length; jj++) {
							if (_entries [jj].CommandLine.StartsWith (currentText, StringComparison.OrdinalIgnoreCase) &&  !lastCommand.Equals (_entries [jj].CommandLine,StringComparison.OrdinalIgnoreCase)) {
								newCommand = _entries [jj].CommandLine;
								_index = jj;
								break;
							}
						}
					}

					if (string.IsNullOrEmpty (newCommand)) {
						if (!string.IsNullOrEmpty (currentText)) {
							for (var jj = 0;jj < _index; jj++) {
								if (_entries [jj].CommandLine.StartsWith (currentText, StringComparison.OrdinalIgnoreCase) &&  !lastCommand.Equals (_entries [jj].CommandLine,StringComparison.OrdinalIgnoreCase)) {
									newCommand = _entries [jj].CommandLine;
									_index = jj;
									break;
								}
							}
						}
						if (string.IsNullOrEmpty (newCommand)) {
							newCommand = _entries.ElementAt (_index).CommandLine;
						}
					}
					lastCommand = newCommand;
					lastInput = currentText;
					return newCommand;
				}

				public void ResetFilter ()
				{
					lastInput = "";
				}

				#endregion
			}

			internal static void SetPrompt (string str)
			{
				editor.Prompt = str;
			}

			internal static bool ReadConsole (IntPtr consoleInput, StringBuilder buffer, int numberOfCharsToRead, out int numberOfCharsRead, ref ConsoleControl.CONSOLE_READCONSOLE_CONTROL controlData)
			{
				getCursor.Invoke (driverObj, null);
				rl_startx.SetValue (driverObj, cursorLeft.GetValue (driverObj));
				rl_starty.SetValue (driverObj, cursorTop.GetValue (driverObj));

				string line = editor.Edit (editor.Prompt, editor.Initial, new ConsoleHistory());
				editor.Initial = "";
				buffer.Append (line);
				if (!line.EndsWith ("\t", StringComparison.OrdinalIgnoreCase))
					buffer.Append ("\r\n");

				numberOfCharsRead = buffer.Length;

				return true;
			}

			private static FieldInfo driverField = Type.GetType("System.ConsoleDriver").GetField ("driver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			private static object driverObj = driverField.GetValue (null);
			private static Type driver = driverObj.GetType ();
			private static FieldInfo consoleInited = driver.GetField ("inited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			private static FieldInfo rl_startx = driver.GetField ("rl_startx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			private static FieldInfo rl_starty = driver.GetField ("rl_starty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			private static FieldInfo cursorLeft = driver.GetField ("cursorLeft", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			private static FieldInfo cursorTop = driver.GetField ("cursorTop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			private static FieldInfo control_characters = driver.GetField ("control_characters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			private static MethodInfo initConsole = driver.GetMethod ("Init", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			private static MethodInfo getCursor = driver.GetMethod ("GetCursorPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			private static MethodInfo readKeyInternal = driver.GetMethod ("ReadKeyInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			private static MethodInfo echoMethod = driver.GetMethod ("Echo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			private static MethodInfo echoFlush = driver.GetMethod ("EchoFlush", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			private static MethodInfo setCursorPosition = driver.GetMethod ("SetCursorPosition", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			private static MethodInfo incrementX = driver.GetMethod ("IncrementX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			private static MethodInfo writeToConsole = driver.GetMethod ("WriteConsole", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			private static Microsoft.PowerShell.Commands.HistoryInfoCommandEqualityComparer historyInfoCommandEqualityComparer = new Microsoft.PowerShell.Commands.HistoryInfoCommandEqualityComparer();


			internal static bool ReadConsoleInput (IntPtr consoleInput, ConsoleControl.INPUT_RECORD[] buffer, int length, out int numberOfEventsRead)
			{
				numberOfEventsRead = 0;
				return true;
			}

			internal static bool ReadConsoleOutput(IntPtr consoleOutput, ConsoleControl.CHAR_INFO[] buffer, ConsoleControl.COORD bufferSize, ConsoleControl.COORD bufferCoord, ref ConsoleControl.SMALL_RECT readRegion)
			{
				return true;
			}

			internal static int ReleaseDC(IntPtr hwnd, IntPtr hdc)
			{
				return 0;
			}

			internal static bool ScrollConsoleScreenBuffer(IntPtr consoleOutput, ref ConsoleControl.SMALL_RECT scrollRectangle, ref ConsoleControl.SMALL_RECT clipRectangle, ConsoleControl.COORD destinationOrigin, ref ConsoleControl.CHAR_INFO fill)
			{
				return true;
			}

			internal static int SendInput (int inputNumbers, ConsoleControl.INPUT[] inputs, int sizeOfInput)
			{
				return 0;
			}

			internal static bool SetConsoleCtrlHandler (ConsoleControl.BreakHandler handlerRoutine, bool add)
			{
				return true;
			}

			/*
			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool SetConsoleCursorInfo(IntPtr consoleOutput, ref ConsoleControl.CONSOLE_CURSOR_INFO consoleCursorInfo);

			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool SetConsoleCursorPosition(IntPtr consoleOutput, ConsoleControl.COORD cursorPosition);
			*/

			internal static bool SetConsoleCursorInfo (IntPtr consoleOutput, ref ConsoleControl.CONSOLE_CURSOR_INFO consoleCursorInfo)
			{
				consoleCursorInfo.Size = Console.CursorSize;
				consoleCursorInfo.Visible = Console.CursorVisible;
				return true;
			}

			internal static bool SetConsoleCursorPosition(IntPtr consoleOutput, ConsoleControl.COORD cursorPosition)
			{
				Console.SetCursorPosition(cursorPosition.X, cursorPosition.Y);
				return true;
			}


            /*
			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool SetConsoleMode(IntPtr consoleHandle, int mode);
            */

            private static int __mode = (int)ConsoleModes.WindowInput;

            internal static bool SetConsoleMode(IntPtr consoleHandle, int mode)
            {
                __mode = mode;
                return true;
            }

			/*
			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool SetConsoleScreenBufferSize(IntPtr consoleOutput, ConsoleControl.COORD size);

			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool SetConsoleTextAttribute(IntPtr consoleOutput, ushort attributes);

			[DllImport("KERNEL32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool SetConsoleTitle(string consoleTitle);

			[DllImport("KERNEL32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool SetConsoleWindowInfo(IntPtr consoleHandle, bool absolute, ref ConsoleControl.SMALL_RECT windowInfo);

			[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool SetCurrentConsoleFontEx(IntPtr consoleOutput, bool bMaximumWindow, ref ConsoleControl.CONSOLE_FONT_INFO_EX consoleFontInfo);
			*/


			internal static bool SetConsoleScreenBufferSize (IntPtr consoleOutput, ConsoleControl.COORD size)
			{
				Console.SetBufferSize (size.X,  size.Y);
				return true;
			}

			internal static bool SetConsoleTextAttribute (IntPtr consoleOutput, ushort attributes)
			{					
				return true;
			}

			internal static bool SetConsoleTitle(string consoleTitle)
			{
				Console.Title = consoleTitle;
				return true;
			}

			internal static bool SetConsoleWindowInfo(IntPtr consoleHandle, bool absolute, ref ConsoleControl.SMALL_RECT windowInfo)
			{
				return true;
			}

			internal static bool SetCurrentConsoleFontEx (IntPtr consoleOutput, bool bMaximumWindow, ref ConsoleControl.CONSOLE_FONT_INFO_EX consoleFontInfo)
			{
				return true;
			}

			/*
			[DllImport("GDI32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool TranslateCharsetInfo(IntPtr src, out ConsoleControl.CHARSETINFO Cs, uint options);
			*/

			internal static bool TranslateCharsetInfo(IntPtr src, out ConsoleControl.CHARSETINFO Cs, uint options)
			{
				ConsoleControl.CHARSETINFO C = new ConsoleControl.CHARSETINFO();
				C.ciCharset = 255;
				Cs = C;
				return true;
			}

            /*
			[DllImport("KERNEL32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool WriteConsole(IntPtr consoleOutput, string buffer, int numberOfCharsToWrite, out int numberOfCharsWritten, IntPtr reserved);

			[DllImport("KERNEL32.dll", CharSet=CharSet.Unicode)]
			internal static extern bool WriteConsoleOutput(IntPtr consoleOutput, ConsoleControl.CHAR_INFO[] buffer, ConsoleControl.COORD bufferSize, ConsoleControl.COORD bufferCoord, ref ConsoleControl.SMALL_RECT writeRegion);
            */

            internal static bool WriteConsole(IntPtr consoleOutput, string buffer, int numberOfCharsToWrite, out int numberOfCharsWritten, IntPtr reserved)
            {
                if (buffer != null)
                {
                    numberOfCharsWritten = buffer.Length;
                    Console.Write(buffer);
                }
                else { numberOfCharsWritten = 0; }
                return true;
            }

            internal static bool WriteConsoleOutput(IntPtr consoleOutput, ConsoleControl.CHAR_INFO[] buffer, ConsoleControl.COORD bufferSize, ConsoleControl.COORD bufferCoord, ref ConsoleControl.SMALL_RECT writeRegion)
            {
                if (buffer == null) return true;
                var sb = new StringBuilder();
                foreach (var e in buffer)
                {
                    sb.Append(Convert.ToChar(e.UnicodeChar));
                }
                Console.Write(sb.ToString());
                return true;
            }

			[Flags]
			internal enum AccessQualifiers : uint
			{
				GenericWrite = 1073741824,
				GenericRead = 2147483648
			}

			internal enum CHAR_INFO_Attributes : uint
			{
				COMMON_LVB_LEADING_BYTE = 256,
				COMMON_LVB_TRAILING_BYTE = 512
			}

			internal enum CreationDisposition : uint
			{
				CreateNew = 1,
				CreateAlways = 2,
				OpenExisting = 3,
				OpenAlways = 4,
				TruncateExisting = 5
			}

			[Flags]
			internal enum ShareModes : uint
			{
				ShareRead = 1,
				ShareWrite = 2
			}
		}

		internal struct SMALL_RECT
		{
			internal short Left;

			internal short Top;

			internal short Right;

			internal short Bottom;

			public override string ToString()
			{
				object[] left = new object[4];
				left[0] = this.Left;
				left[1] = this.Top;
				left[2] = this.Right;
				left[3] = this.Bottom;
				return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", left);
			}
		}

		internal enum StandardHandleId : long
		{
			Error = 4294967284,
			Output = 4294967285,
			Input = 4294967286
		}

		internal struct TEXTMETRIC
		{
			public int tmHeight;

			public int tmAscent;

			public int tmDescent;

			public int tmInternalLeading;

			public int tmExternalLeading;

			public int tmAveCharWidth;

			public int tmMaxCharWidth;

			public int tmWeight;

			public int tmOverhang;

			public int tmDigitizedAspectX;

			public int tmDigitizedAspectY;

			public char tmFirstChar;

			public char tmLastChar;

			public char tmDefaultChar;

			public char tmBreakChar;

			public byte tmItalic;

			public byte tmUnderlined;

			public byte tmStruckOut;

			public byte tmPitchAndFamily;

			public byte tmCharSet;

		}

		internal enum VirtualKeyCode : ushort
		{
			Return = 13,
			Left = 37
		}
	}
}