using System;
using System.Diagnostics.PerformanceData;
using System.Management.Automation.PerformanceData;

namespace Microsoft.PowerShell.Workflow
{
	internal static class PSWorkflowPerformanceCounterSetInfo
	{
		internal static Guid ProviderId;

		internal static Guid CounterSetId;

		internal static CounterSetInstanceType CounterSetType;

		internal static CounterInfo[] CounterInfoArray;

		static PSWorkflowPerformanceCounterSetInfo()
		{
			PSWorkflowPerformanceCounterSetInfo.ProviderId = new Guid("{5db760bc-64b2-4da7-b4ef-7dab105fbb8c}");
			PSWorkflowPerformanceCounterSetInfo.CounterSetId = new Guid("{faa17411-9025-4b86-8b5e-ce2f32b06e13}");
			PSWorkflowPerformanceCounterSetInfo.CounterSetType = CounterSetInstanceType.Multiple;
			CounterInfo[] counterInfo = new CounterInfo[29];
			counterInfo[0] = new CounterInfo(1, CounterType.RawData64);
			counterInfo[1] = new CounterInfo(2, CounterType.RateOfCountPerSecond64);
			counterInfo[2] = new CounterInfo(3, CounterType.RawData64);
			counterInfo[3] = new CounterInfo(4, CounterType.RateOfCountPerSecond64);
			counterInfo[4] = new CounterInfo(5, CounterType.RawData64);
			counterInfo[5] = new CounterInfo(6, CounterType.RateOfCountPerSecond64);
			counterInfo[6] = new CounterInfo(7, CounterType.RawData64);
			counterInfo[7] = new CounterInfo(8, CounterType.RateOfCountPerSecond64);
			counterInfo[8] = new CounterInfo(9, CounterType.RawData64);
			counterInfo[9] = new CounterInfo(10, CounterType.RateOfCountPerSecond64);
			counterInfo[10] = new CounterInfo(11, CounterType.RawData64);
			counterInfo[11] = new CounterInfo(12, CounterType.RateOfCountPerSecond64);
			counterInfo[12] = new CounterInfo(13, CounterType.RawData64);
			counterInfo[13] = new CounterInfo(14, CounterType.RateOfCountPerSecond64);
			counterInfo[14] = new CounterInfo(15, CounterType.RawData64);
			counterInfo[15] = new CounterInfo(16, CounterType.RawData64);
			counterInfo[16] = new CounterInfo(17, CounterType.RateOfCountPerSecond64);
			counterInfo[17] = new CounterInfo(18, CounterType.RawData64);
			counterInfo[18] = new CounterInfo(19, CounterType.RateOfCountPerSecond64);
			counterInfo[19] = new CounterInfo(20, CounterType.RawData64);
			counterInfo[20] = new CounterInfo(21, CounterType.RawData64);
			counterInfo[21] = new CounterInfo(22, CounterType.RawData64);
			counterInfo[22] = new CounterInfo(23, CounterType.RawData64);
			counterInfo[23] = new CounterInfo(24, CounterType.RawData64);
			counterInfo[24] = new CounterInfo(25, CounterType.RawData64);
			counterInfo[25] = new CounterInfo(26, CounterType.RawData64);
			counterInfo[26] = new CounterInfo(27, CounterType.RawData64);
			counterInfo[27] = new CounterInfo(28, CounterType.RawData64);
			counterInfo[28] = new CounterInfo(29, CounterType.RawData64);
			PSWorkflowPerformanceCounterSetInfo.CounterInfoArray = counterInfo;
		}
	}
}