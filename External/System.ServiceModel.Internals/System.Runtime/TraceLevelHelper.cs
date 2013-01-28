using System;
using System.Diagnostics;

namespace System.Runtime
{
	internal class TraceLevelHelper
	{
		private static TraceEventType[] EtwLevelToTraceEventType;

		static TraceLevelHelper()
		{
			TraceEventType[] traceEventTypeArray = new TraceEventType[6];
			traceEventTypeArray[0] = TraceEventType.Critical;
			traceEventTypeArray[1] = TraceEventType.Critical;
			traceEventTypeArray[2] = TraceEventType.Error;
			traceEventTypeArray[3] = TraceEventType.Warning;
			traceEventTypeArray[4] = TraceEventType.Information;
			traceEventTypeArray[5] = TraceEventType.Verbose;
			TraceLevelHelper.EtwLevelToTraceEventType = traceEventTypeArray;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public TraceLevelHelper()
		{
		}

		internal static TraceEventType GetTraceEventType(byte level, byte opcode)
		{
			byte num = opcode;
			switch (num)
			{
				case 1:
				{
					return TraceEventType.Start;
				}
				case 2:
				{
					return TraceEventType.Stop;
				}
				default:
				{
					switch (num)
					{
						case 7:
						{
							return TraceEventType.Resume;
						}
						case 8:
						{
							return TraceEventType.Suspend;
						}
					}
				}
				break;
			}
			return TraceLevelHelper.EtwLevelToTraceEventType[level];
		}

		internal static TraceEventType GetTraceEventType(TraceEventLevel level)
		{
			return TraceLevelHelper.EtwLevelToTraceEventType[(int)level];
		}

		internal static TraceEventType GetTraceEventType(byte level)
		{
			return TraceLevelHelper.EtwLevelToTraceEventType[level];
		}

		internal static string LookupSeverity(TraceEventLevel level, TraceEventOpcode opcode)
		{
			string str;
			TraceEventOpcode traceEventOpcode = opcode;
			switch (traceEventOpcode)
			{
				case TraceEventOpcode.Start:
				{
					str = "Start";
					break;
				}
				case TraceEventOpcode.Stop:
				{
					str = "Stop";
					break;
				}
				default:
				{
					switch (traceEventOpcode)
					{
						case TraceEventOpcode.Resume:
						{
							str = "Resume";
							return str;
						}
						case TraceEventOpcode.Suspend:
						{
							str = "Suspend";
							return str;
						}
						default:
						{
							TraceEventLevel traceEventLevel = level;
							switch (traceEventLevel)
							{
								case TraceEventLevel.Critical:
								{
									str = "Critical";
									return str;
								}
								case TraceEventLevel.Error:
								{
									str = "Error";
									return str;
								}
								case TraceEventLevel.Warning:
								{
									str = "Warning";
									return str;
								}
								case TraceEventLevel.Informational:
								{
									str = "Information";
									return str;
								}
								case TraceEventLevel.Verbose:
								{
									str = "Verbose";
									return str;
								}
							}
							break;
						}
					}
					str = level.ToString();
					break;
				}
				break;
			}
			return str;
		}
	}
}