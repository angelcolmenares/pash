using System;
using System.Diagnostics.Eventing;
using System.Management.Automation.Tracing;

namespace Microsoft.Management.Odata.Tracing
{
	internal sealed class Tracer : EtwActivity
	{
		public const byte LevelCritical = 1;

		public const byte LevelError = 2;

		public const byte LevelWarning = 3;

		public const byte LevelInformational = 4;

		public const byte LevelVerbose = 5;

		public const long KeywordAll = 0xffffffffL;

		private static Guid providerId;

		internal static EventDescriptor AuthorizeUserRequestFailedEvent;

		internal static EventDescriptor AuthorizeUserRequestSucceededEvent;

		internal static EventDescriptor BeginOperation0Event;

		internal static EventDescriptor CmdletExecutionEndEvent;

		internal static EventDescriptor CmdletExecutionStartEvent;

		internal static EventDescriptor CommandExecutionResultCountExceededEvent;

		internal static EventDescriptor CommandExecutionTimeExceededEvent;

		internal static EventDescriptor CommandInvocationErrorEvent;

		internal static EventDescriptor CommandInvocationFailedExceptionEvent;

		internal static EventDescriptor CommandInvocationStartEvent;

		internal static EventDescriptor CommandInvocationSucceededEvent;

		internal static EventDescriptor ConcurrentRequestQuotaViolationCountEvent;

		internal static EventDescriptor CustomAuthorizationLoadedSuccessfullyEvent;

		internal static EventDescriptor CustomAuthorizationLoadingFailedEvent;

		internal static EventDescriptor CustomAuthzCallEndEvent;

		internal static EventDescriptor CustomAuthzCallStartEvent;

		internal static EventDescriptor CustomAuthzExceedTimeLimitEvent;

		internal static EventDescriptor CustomAuthzMethodExceededTimeLimitEvent;

		internal static EventDescriptor CustomModuleInvocationFailedExceptionEvent;

		internal static EventDescriptor DataServiceControllerCreationFailedOperationalEvent;

		internal static EventDescriptor DataServiceControllerCreationSucceededEvent;

		internal static EventDescriptor DataServiceProviderHandleExceptionEvent;

		internal static EventDescriptor DebugMessageEvent;

		internal static EventDescriptor EndOperationEvent;

		internal static EventDescriptor ErrorRecordEvent;

		internal static EventDescriptor ExceptionMessageEvent;

		internal static EventDescriptor ExclusiveStoreCreatedNewEvent;

		internal static EventDescriptor ExclusiveStoreTookFromCacheEvent;

		internal static EventDescriptor FailedToCreateRunspaceEvent;

		internal static EventDescriptor GetInitialSessionStateRequestFailedEvent;

		internal static EventDescriptor GetInitialSessionStateRequestSucceededEvent;

		internal static EventDescriptor GetMembershipIdEvent;

		internal static EventDescriptor HandleExceptionEventEvent;

		internal static EventDescriptor IdentityDescriptionEvent;

		internal static EventDescriptor IncomingMessageEvent;

		internal static EventDescriptor InformationRecordEvent;

		internal static EventDescriptor InvalidDataServiceConfigurationEvent;

		internal static EventDescriptor InvalidSchemaExceptionEvent;

		internal static EventDescriptor InvalidUpdateQueryEvent;

		internal static EventDescriptor InvalidUriForPublicRootHeaderEvent;

		internal static EventDescriptor InvocationInstanceEvent;

		internal static EventDescriptor MaxCmdletQuotaViolationEvent;

		internal static EventDescriptor MethodCall0Event;

		internal static EventDescriptor MethodCall1Event;

		internal static EventDescriptor MethodCall2Event;

		internal static EventDescriptor MethodCall3Event;

		internal static EventDescriptor OperationalPipelineErrorEvent;

		internal static EventDescriptor OutgoingMessageEvent;

		internal static EventDescriptor PipelineCompleteEvent;

		internal static EventDescriptor PipelineErrorEvent;

		internal static EventDescriptor PipelineStartEvent;

		internal static EventDescriptor PowerShellInstanceEvent;

		internal static EventDescriptor PowerShellPipelineCreationFailedEvent;

		internal static EventDescriptor PowerShellRunspaceCreationEndEvent;

		internal static EventDescriptor PowerShellRunspaceCreationStartEvent;

		internal static EventDescriptor PSObjectSerializationFailedEvent;

		internal static EventDescriptor PSObjectSerializationFailedExceptionEvent;

		internal static EventDescriptor PSSessionCallEndEvent;

		internal static EventDescriptor PSSessionCallStartEvent;

		internal static EventDescriptor PSSessionConfigurationLoadedSuccessfullyEvent;

		internal static EventDescriptor PSSessionConfigurationLoadingFailedEvent;

		internal static EventDescriptor PSSessionMethodExceededTimeLimitEvent;

		internal static EventDescriptor QueryGetQueryRootForResourceFailedEvent;

		internal static EventDescriptor RequestEndEvent;

		internal static EventDescriptor RequestPerSecondQuotaViolationCountEvent;

		internal static EventDescriptor RequestProcessingEndEvent;

		internal static EventDescriptor RequestProcessingStartEvent;

		internal static EventDescriptor RequestStartEvent;

		internal static EventDescriptor ResourcePropertyNotFoundExceptionEvent;

		internal static EventDescriptor ResourceSetNotFoundEvent;

		internal static EventDescriptor ResourceTypeNotFoundEvent;

		internal static EventDescriptor SchemaFileInvalidCmdletFieldParameterEvent;

		internal static EventDescriptor SchemaFileInvalidCmdletNameEvent;

		internal static EventDescriptor SchemaFileInvalidCmdletParametersEvent;

		internal static EventDescriptor SchemaFileLoadedSuccessfullyEvent;

		internal static EventDescriptor SchemaFileNotFoundEvent;

		internal static EventDescriptor SchemaFileNotValidCsdlEvent;

		internal static EventDescriptor SchemaLoadingEndEvent;

		internal static EventDescriptor SchemaLoadingStartEvent;

		internal static EventDescriptor SerializationMaximumObjectDepthReachedEvent;

		internal static EventDescriptor SerializationPropertyNotFoundEvent;

		internal static EventDescriptor SharedStoreCreatedNewEvent;

		internal static EventDescriptor SharedStoreTookFromCacheEvent;

		internal static EventDescriptor SystemQuotaViolationCountEvent;

		internal static EventDescriptor TypeLoadExceptionEvent;

		internal static EventDescriptor UnauthorizedAccessExceptionEvent;

		internal static EventDescriptor UriParsingFailedEvent;

		internal static EventDescriptor UserAuthorizedSuccessfullyEvent;

		internal static EventDescriptor UserNotAuthorizedEvent;

		internal static EventDescriptor UserQuotaInformationEvent;

		internal static EventDescriptor UserQuotaSucceededEvent;

		internal static EventDescriptor UserQuotaViolationEvent;

		internal static EventDescriptor UserQuotaViolationCountEvent;

		internal static EventDescriptor UserSchemaCreationEndEvent;

		internal static EventDescriptor UserSchemaCreationFailedEvent;

		internal static EventDescriptor UserSchemaCreationStartEvent;

		internal static EventDescriptor UserSchemaCreationSucceededEvent;

		internal static EventDescriptor ValidDataServiceConfigurationEvent;

		internal static EventDescriptor WriteTransferEventEvent;

		internal static EventDescriptor WrongResourceTypeUsedExceptionEvent;

		protected override Guid ProviderId
		{
			get
			{
				return Tracer.providerId;
			}
		}

		protected override EventDescriptor TransferEvent
		{
			get
			{
				return Tracer.WriteTransferEventEvent;
			}
		}

		static Tracer()
		{
			Tracer.providerId = Guid.Parse("60b4c807-9e58-40d0-a608-9a60dffdd6b9");
			Tracer.AuthorizeUserRequestFailedEvent = new EventDescriptor(0xfa4, 1, 16, 2, 11, 2, -9223372036854775800L);
			Tracer.AuthorizeUserRequestSucceededEvent = new EventDescriptor(0xfa3, 1, 17, 4, 11, 2, 0x4000000000000008L);
			Tracer.BeginOperation0Event = new EventDescriptor(0x1771, 1, 18, 5, 1, 3, 0x2000000000000002L);
			Tracer.CmdletExecutionEndEvent = new EventDescriptor(0x1b62, 1, 17, 4, 2, 6, 0x4001000000000010L);
			Tracer.CmdletExecutionStartEvent = new EventDescriptor(0x1b61, 1, 17, 4, 1, 6, 0x4001000000000010L);
			Tracer.CommandExecutionResultCountExceededEvent = new EventDescriptor(0x5e1, 1, 16, 2, 0, 11, -9223372036854775807L);
			Tracer.CommandExecutionTimeExceededEvent = new EventDescriptor(0x5e0, 1, 16, 2, 0, 11, -9223372036854775807L);
			Tracer.CommandInvocationErrorEvent = new EventDescriptor(0xa10d, 1, 17, 2, 2, 0, 0x4000000000000000L);
			Tracer.CommandInvocationFailedExceptionEvent = new EventDescriptor(0xa104, 1, 16, 2, 12, 0, -9223372036854775808L);
			Tracer.CommandInvocationStartEvent = new EventDescriptor(0xa10b, 1, 17, 5, 1, 0, 0x4000000000000000L);
			Tracer.CommandInvocationSucceededEvent = new EventDescriptor(0xa10c, 1, 17, 4, 2, 0, 0x4000000000000000L);
			Tracer.ConcurrentRequestQuotaViolationCountEvent = new EventDescriptor(0x5dd, 1, 16, 2, 0, 11, -9223372036854775807L);
			Tracer.CustomAuthorizationLoadedSuccessfullyEvent = new EventDescriptor(0xfa2, 1, 16, 4, 11, 1, -9223372036854775800L);
			Tracer.CustomAuthorizationLoadingFailedEvent = new EventDescriptor(0xfa1, 1, 16, 2, 10, 1, -9223372036854775800L);
			Tracer.CustomAuthzCallEndEvent = new EventDescriptor(0x1b5e, 1, 17, 4, 2, 3, 0x4001000000000008L);
			Tracer.CustomAuthzCallStartEvent = new EventDescriptor(0x1b5d, 1, 17, 4, 1, 3, 0x4001000000000008L);
			Tracer.CustomAuthzExceedTimeLimitEvent = new EventDescriptor(0xfa7, 1, 17, 3, 11, 3, 0x4000000000000008L);
			Tracer.CustomAuthzMethodExceededTimeLimitEvent = new EventDescriptor(0x5df, 1, 16, 3, 0, 11, -9223372036854775807L);
			Tracer.CustomModuleInvocationFailedExceptionEvent = new EventDescriptor(0xa10a, 1, 16, 2, 12, 0, -9223372036854775808L);
			Tracer.DataServiceControllerCreationFailedOperationalEvent = new EventDescriptor(0x44d, 1, 16, 1, 10, 1, -9223372036854775807L);
			Tracer.DataServiceControllerCreationSucceededEvent = new EventDescriptor(0x44e, 1, 16, 4, 10, 1, -9223372036854775807L);
			Tracer.DataServiceProviderHandleExceptionEvent = new EventDescriptor(0x7d6, 1, 16, 2, 13, 3, -9223372036854775806L);
			Tracer.DebugMessageEvent = new EventDescriptor(0xc001, 1, 18, 5, 0, 0, 0x2000000000000000L);
			Tracer.EndOperationEvent = new EventDescriptor(0x177a, 1, 18, 5, 2, 3, 0x2000000000000002L);
			Tracer.ErrorRecordEvent = new EventDescriptor(0x138f, 1, 17, 4, 0, 6, 0x4000000000000010L);
			Tracer.ExceptionMessageEvent = new EventDescriptor(0xa001, 1, 18, 5, 0, 0, 0x2000000000000000L);
			Tracer.ExclusiveStoreCreatedNewEvent = new EventDescriptor(0x4b1, 1, 18, 5, 15, 7, 0x2000000000000001L);
			Tracer.ExclusiveStoreTookFromCacheEvent = new EventDescriptor(0x4b2, 1, 18, 5, 15, 7, 0x2000000000000001L);
			Tracer.FailedToCreateRunspaceEvent = new EventDescriptor(0x14b7, 1, 16, 2, 13, 6, -9223372036854775792L);
			Tracer.GetInitialSessionStateRequestFailedEvent = new EventDescriptor(0x1008, 1, 16, 2, 11, 3, -9223372036854775800L);
			Tracer.GetInitialSessionStateRequestSucceededEvent = new EventDescriptor(0x1007, 1, 17, 4, 11, 3, 0x4000000000000008L);
			Tracer.GetMembershipIdEvent = new EventDescriptor(0xfa6, 1, 16, 2, 11, 3, -9223372036854775800L);
			Tracer.HandleExceptionEventEvent = new EventDescriptor(0x835, 1, 16, 2, 12, 3, -9223372036854775806L);
			Tracer.IdentityDescriptionEvent = new EventDescriptor(0x451, 1, 17, 4, 0, 1, 0x4000000000000001L);
			Tracer.IncomingMessageEvent = new EventDescriptor(0x7d4, 1, 17, 4, 13, 3, 0x4000000000000002L);
			Tracer.InformationRecordEvent = new EventDescriptor(0x1390, 1, 17, 4, 0, 6, 0x4000000000000010L);
			Tracer.InvalidDataServiceConfigurationEvent = new EventDescriptor(0x44f, 1, 16, 2, 10, 1, -9223372036854775807L);
			Tracer.InvalidSchemaExceptionEvent = new EventDescriptor(0xa107, 1, 16, 2, 12, 0, -9223372036854775808L);
			Tracer.InvalidUpdateQueryEvent = new EventDescriptor(0x7d3, 1, 17, 3, 13, 3, 0x4000000000000002L);
			Tracer.InvalidUriForPublicRootHeaderEvent = new EventDescriptor(0x515, 1, 16, 3, 0, 0, -9223372036854775807L);
			Tracer.InvocationInstanceEvent = new EventDescriptor(0xa10f, 1, 17, 4, 0, 0, 0x4000000000000000L);
			Tracer.MaxCmdletQuotaViolationEvent = new EventDescriptor(0x5e6, 1, 17, 4, 0, 11, 0x4000000000000001L);
			Tracer.MethodCall0Event = new EventDescriptor(0xb55, 1, 18, 5, 11, 3, 0x2000000000000002L);
			Tracer.MethodCall1Event = new EventDescriptor(0xb56, 1, 18, 5, 11, 3, 0x2000000000000002L);
			Tracer.MethodCall2Event = new EventDescriptor(0xb57, 1, 18, 5, 11, 3, 0x2000000000000002L);
			Tracer.MethodCall3Event = new EventDescriptor(0xb58, 1, 18, 5, 11, 3, 0x2000000000000002L);
			Tracer.OperationalPipelineErrorEvent = new EventDescriptor(0xa113, 1, 16, 2, 0, 0, -9223372036854775808L);
			Tracer.OutgoingMessageEvent = new EventDescriptor(0x7d5, 1, 17, 4, 13, 3, 0x4000000000000002L);
			Tracer.PipelineCompleteEvent = new EventDescriptor(0xa111, 1, 17, 4, 2, 0, 0x4000000000000000L);
			Tracer.PipelineErrorEvent = new EventDescriptor(0xa112, 1, 17, 2, 2, 0, 0x4000000000000000L);
			Tracer.PipelineStartEvent = new EventDescriptor(0xa110, 1, 17, 5, 1, 0, 0x4000000000000000L);
			Tracer.PowerShellInstanceEvent = new EventDescriptor(0x138e, 1, 17, 4, 0, 6, 0x4000000000000010L);
			Tracer.PowerShellPipelineCreationFailedEvent = new EventDescriptor(0x1389, 1, 16, 2, 11, 6, -9223372036854775792L);
			Tracer.PowerShellRunspaceCreationEndEvent = new EventDescriptor(0x1b68, 1, 17, 4, 2, 1, 0x4001000000000010L);
			Tracer.PowerShellRunspaceCreationStartEvent = new EventDescriptor(0x1b67, 1, 17, 4, 1, 1, 0x4001000000000010L);
			Tracer.PSObjectSerializationFailedEvent = new EventDescriptor(0x138b, 1, 16, 2, 14, 6, -9223372036854775792L);
			Tracer.PSObjectSerializationFailedExceptionEvent = new EventDescriptor(0xa105, 1, 16, 2, 12, 0, -9223372036854775808L);
			Tracer.PSSessionCallEndEvent = new EventDescriptor(0x1b60, 1, 17, 4, 2, 3, 0x4001000000000008L);
			Tracer.PSSessionCallStartEvent = new EventDescriptor(0x1b5f, 1, 17, 4, 1, 3, 0x4001000000000008L);
			Tracer.PSSessionConfigurationLoadedSuccessfullyEvent = new EventDescriptor(0x1005, 1, 16, 4, 11, 1, -9223372036854775800L);
			Tracer.PSSessionConfigurationLoadingFailedEvent = new EventDescriptor(0x1006, 1, 16, 2, 11, 1, -9223372036854775800L);
			Tracer.PSSessionMethodExceededTimeLimitEvent = new EventDescriptor(0x1009, 1, 17, 3, 11, 3, 0x4000000000000008L);
			Tracer.QueryGetQueryRootForResourceFailedEvent = new EventDescriptor(0x8fd, 1, 16, 2, 11, 5, -9223372036854775806L);
			Tracer.RequestEndEvent = new EventDescriptor(0x1b5a, 1, 17, 4, 2, 3, 0x4001000000000000L);
			Tracer.RequestPerSecondQuotaViolationCountEvent = new EventDescriptor(0x5de, 1, 16, 2, 0, 11, -9223372036854775807L);
			Tracer.RequestProcessingEndEvent = new EventDescriptor(0x1b5c, 1, 17, 4, 2, 3, 0x4001000000000000L);
			Tracer.RequestProcessingStartEvent = new EventDescriptor(0x1b5b, 1, 17, 4, 1, 3, 0x4001000000000000L);
			Tracer.RequestStartEvent = new EventDescriptor(0x1b59, 1, 17, 4, 1, 3, 0x4001000000000000L);
			Tracer.ResourcePropertyNotFoundExceptionEvent = new EventDescriptor(0xa109, 1, 16, 2, 12, 0, -9223372036854775808L);
			Tracer.ResourceSetNotFoundEvent = new EventDescriptor(0xbc1, 1, 16, 2, 11, 3, -9223372036854775804L);
			Tracer.ResourceTypeNotFoundEvent = new EventDescriptor(0xa10e, 1, 16, 2, 0, 0, -9223372036854775808L);
			Tracer.SchemaFileInvalidCmdletFieldParameterEvent = new EventDescriptor(0xbbd, 1, 16, 2, 11, 1, -9223372036854775804L);
			Tracer.SchemaFileInvalidCmdletNameEvent = new EventDescriptor(0xbbc, 1, 16, 2, 11, 1, -9223372036854775804L);
			Tracer.SchemaFileInvalidCmdletParametersEvent = new EventDescriptor(0xbbe, 1, 16, 2, 11, 1, -9223372036854775804L);
			Tracer.SchemaFileLoadedSuccessfullyEvent = new EventDescriptor(0xbb9, 1, 16, 4, 11, 1, -9223372036854775804L);
			Tracer.SchemaFileNotFoundEvent = new EventDescriptor(0xbba, 1, 16, 2, 11, 1, -9223372036854775804L);
			Tracer.SchemaFileNotValidCsdlEvent = new EventDescriptor(0xbbb, 1, 16, 2, 11, 1, -9223372036854775804L);
			Tracer.SchemaLoadingEndEvent = new EventDescriptor(0x1b64, 1, 17, 4, 2, 1, 0x4001000000000004L);
			Tracer.SchemaLoadingStartEvent = new EventDescriptor(0x1b63, 1, 17, 4, 1, 1, 0x4001000000000004L);
			Tracer.SerializationMaximumObjectDepthReachedEvent = new EventDescriptor(0x138d, 1, 18, 3, 14, 6, 0x2000000000000010L);
			Tracer.SerializationPropertyNotFoundEvent = new EventDescriptor(0x138c, 1, 18, 3, 14, 6, 0x2000000000000010L);
			Tracer.SharedStoreCreatedNewEvent = new EventDescriptor(0x4b4, 1, 18, 5, 15, 7, 0x2000000000000001L);
			Tracer.SharedStoreTookFromCacheEvent = new EventDescriptor(0x4b3, 1, 18, 5, 15, 7, 0x2000000000000001L);
			Tracer.SystemQuotaViolationCountEvent = new EventDescriptor(0x5e4, 1, 16, 4, 0, 11, -9223372036854775807L);
			Tracer.TypeLoadExceptionEvent = new EventDescriptor(0xa102, 1, 16, 2, 12, 0, -9223372036854775808L);
			Tracer.UnauthorizedAccessExceptionEvent = new EventDescriptor(0xa103, 1, 16, 2, 12, 0, -9223372036854775808L);
			Tracer.UriParsingFailedEvent = new EventDescriptor(0x8fe, 1, 16, 2, 11, 5, -9223372036854775807L);
			Tracer.UserAuthorizedSuccessfullyEvent = new EventDescriptor(0x7d1, 1, 17, 4, 11, 2, 0x4000000000000002L);
			Tracer.UserNotAuthorizedEvent = new EventDescriptor(0x7d2, 1, 17, 5, 11, 2, 0x4000000000000002L);
			Tracer.UserQuotaInformationEvent = new EventDescriptor(0x5e7, 1, 17, 4, 0, 11, 0x4000000000000001L);
			Tracer.UserQuotaSucceededEvent = new EventDescriptor(0x5e3, 1, 17, 4, 0, 11, 0x4000000000000001L);
			Tracer.UserQuotaViolationEvent = new EventDescriptor(0x5e2, 1, 16, 2, 0, 11, -9223372036854775807L);
			Tracer.UserQuotaViolationCountEvent = new EventDescriptor(0x5e5, 1, 16, 4, 0, 11, -9223372036854775807L);
			Tracer.UserSchemaCreationEndEvent = new EventDescriptor(0x1b66, 1, 17, 4, 2, 1, 0x4001000000000004L);
			Tracer.UserSchemaCreationFailedEvent = new EventDescriptor(0xbc0, 1, 16, 2, 11, 3, -9223372036854775804L);
			Tracer.UserSchemaCreationStartEvent = new EventDescriptor(0x1b65, 1, 17, 4, 1, 1, 0x4001000000000004L);
			Tracer.UserSchemaCreationSucceededEvent = new EventDescriptor(0xbbf, 1, 17, 4, 11, 3, 0x4000000000000004L);
			Tracer.ValidDataServiceConfigurationEvent = new EventDescriptor(0x450, 1, 16, 4, 10, 1, -9223372036854775807L);
			Tracer.WriteTransferEventEvent = new EventDescriptor(0xd001, 1, 17, 4, 11, 0, 0x4000000000000000L);
			Tracer.WrongResourceTypeUsedExceptionEvent = new EventDescriptor(0xa108, 1, 16, 2, 12, 0, -9223372036854775808L);
		}

		public Tracer()
		{
		}

		[EtwEvent(0xfa4L)]
		public void AuthorizeUserRequestFailed(string userName, string authenticationType)
		{
			object[] objArray = new object[2];
			objArray[0] = userName;
			objArray[1] = authenticationType;
			base.WriteEvent(Tracer.AuthorizeUserRequestFailedEvent, objArray);
		}

		[EtwEvent(0xfa3L)]
		public void AuthorizeUserRequestSucceeded(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.AuthorizeUserRequestSucceededEvent, objArray);
		}

		[EtwEvent(0x1771L)]
		public void BeginOperation0(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.BeginOperation0Event, objArray);
		}

		[EtwEvent(0x1b62L)]
		public void CmdletExecutionEnd(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.CmdletExecutionEndEvent, objArray);
		}

		[EtwEvent(0x1b61L)]
		public void CmdletExecutionStart(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.CmdletExecutionStartEvent, objArray);
		}

		[EtwEvent(0x5e1L)]
		public void CommandExecutionResultCountExceeded(string commandName, string userName, int maximumExecutionLimit, int maximumObjectsAllowed)
		{
			object[] objArray = new object[4];
			objArray[0] = commandName;
			objArray[1] = userName;
			objArray[2] = maximumExecutionLimit;
			objArray[3] = maximumObjectsAllowed;
			base.WriteEvent(Tracer.CommandExecutionResultCountExceededEvent, objArray);
		}

		[EtwEvent(0x5e0L)]
		public void CommandExecutionTimeExceeded(string commandName, string userName, int maximumExecutionLimit, int maximumObjectsAllowed)
		{
			object[] objArray = new object[4];
			objArray[0] = commandName;
			objArray[1] = userName;
			objArray[2] = maximumExecutionLimit;
			objArray[3] = maximumObjectsAllowed;
			base.WriteEvent(Tracer.CommandExecutionTimeExceededEvent, objArray);
		}

		[EtwEvent(0xa10dL)]
		public void CommandInvocationError(string cmdlet, uint errorCount, string record1, string record2, string record3)
		{
			object[] objArray = new object[5];
			objArray[0] = cmdlet;
			objArray[1] = errorCount;
			objArray[2] = record1;
			objArray[3] = record2;
			objArray[4] = record3;
			base.WriteEvent(Tracer.CommandInvocationErrorEvent, objArray);
		}

		[EtwEvent(0xa104L)]
		public void CommandInvocationFailedException(string cmdlet, string exceptionType, string message)
		{
			object[] objArray = new object[3];
			objArray[0] = cmdlet;
			objArray[1] = exceptionType;
			objArray[2] = message;
			base.WriteEvent(Tracer.CommandInvocationFailedExceptionEvent, objArray);
		}

		[EtwEvent(0xa10bL)]
		public void CommandInvocationStart(string cmdlet)
		{
			object[] objArray = new object[1];
			objArray[0] = cmdlet;
			base.WriteEvent(Tracer.CommandInvocationStartEvent, objArray);
		}

		[EtwEvent(0xa10cL)]
		public void CommandInvocationSucceeded(string cmdlet, uint objectCount)
		{
			object[] objArray = new object[2];
			objArray[0] = cmdlet;
			objArray[1] = objectCount;
			base.WriteEvent(Tracer.CommandInvocationSucceededEvent, objArray);
		}

		[EtwEvent(0x5ddL)]
		public void ConcurrentRequestQuotaViolationCount(int quotaViolations, string startTime)
		{
			object[] objArray = new object[2];
			objArray[0] = quotaViolations;
			objArray[1] = startTime;
			base.WriteEvent(Tracer.ConcurrentRequestQuotaViolationCountEvent, objArray);
		}

		[EtwEvent(0xfa2L)]
		public void CustomAuthorizationLoadedSuccessfully(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.CustomAuthorizationLoadedSuccessfullyEvent, objArray);
		}

		[EtwEvent(0xfa1L)]
		public void CustomAuthorizationLoadingFailed(string param1, string param2)
		{
			object[] objArray = new object[2];
			objArray[0] = param1;
			objArray[1] = param2;
			base.WriteEvent(Tracer.CustomAuthorizationLoadingFailedEvent, objArray);
		}

		[EtwEvent(0x1b5eL)]
		public void CustomAuthzCallEnd(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.CustomAuthzCallEndEvent, objArray);
		}

		[EtwEvent(0x1b5dL)]
		public void CustomAuthzCallStart(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.CustomAuthzCallStartEvent, objArray);
		}

		[EtwEvent(0xfa7L)]
		public void CustomAuthzExceedTimeLimit(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.CustomAuthzExceedTimeLimitEvent, objArray);
		}

		[EtwEvent(0x5dfL)]
		public void CustomAuthzMethodExceededTimeLimit(string interfaceName, string methoName, string timeTaken)
		{
			object[] objArray = new object[3];
			objArray[0] = interfaceName;
			objArray[1] = methoName;
			objArray[2] = timeTaken;
			base.WriteEvent(Tracer.CustomAuthzMethodExceededTimeLimitEvent, objArray);
		}

		[EtwEvent(0xa10aL)]
		public void CustomModuleInvocationFailedException(string param1, string param2, string param3)
		{
			object[] objArray = new object[3];
			objArray[0] = param1;
			objArray[1] = param2;
			objArray[2] = param3;
			base.WriteEvent(Tracer.CustomModuleInvocationFailedExceptionEvent, objArray);
		}

		[EtwEvent(0x44dL)]
		public void DataServiceControllerCreationFailedOperational(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.DataServiceControllerCreationFailedOperationalEvent, objArray);
		}

		[EtwEvent(0x44eL)]
		public void DataServiceControllerCreationSucceeded()
		{
			base.WriteEvent(Tracer.DataServiceControllerCreationSucceededEvent, new object[0]);
		}

		[EtwEvent(0x7d6L)]
		public void DataServiceProviderHandleException(string exception, int responseStatusCode, string responseContentType, string responseWritten, string useVerboseError)
		{
			object[] objArray = new object[5];
			objArray[0] = exception;
			objArray[1] = responseStatusCode;
			objArray[2] = responseContentType;
			objArray[3] = responseWritten;
			objArray[4] = useVerboseError;
			base.WriteEvent(Tracer.DataServiceProviderHandleExceptionEvent, objArray);
		}

		[EtwEvent(0xc001L)]
		public void DebugMessage(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.DebugMessageEvent, objArray);
		}

		[EtwEvent(0x177aL)]
		public void EndOperation(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.EndOperationEvent, objArray);
		}

		[EtwEvent(0x138fL)]
		public void ErrorRecord(string exception, string errorMessage, string categoryInfo)
		{
			object[] objArray = new object[3];
			objArray[0] = exception;
			objArray[1] = errorMessage;
			objArray[2] = categoryInfo;
			base.WriteEvent(Tracer.ErrorRecordEvent, objArray);
		}

		[EtwEvent(0xa001L)]
		public void ExceptionMessage(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.ExceptionMessageEvent, objArray);
		}

		[EtwEvent(0x4b1L)]
		public void ExclusiveStoreCreatedNew(string userName, string itemTypeName)
		{
			object[] objArray = new object[2];
			objArray[0] = userName;
			objArray[1] = itemTypeName;
			base.WriteEvent(Tracer.ExclusiveStoreCreatedNewEvent, objArray);
		}

		[EtwEvent(0x4b2L)]
		public void ExclusiveStoreTookFromCache(string userName, string itemTypeName)
		{
			object[] objArray = new object[2];
			objArray[0] = userName;
			objArray[1] = itemTypeName;
			base.WriteEvent(Tracer.ExclusiveStoreTookFromCacheEvent, objArray);
		}

		[EtwEvent(0x14b7L)]
		public void FailedToCreateRunspace(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.FailedToCreateRunspaceEvent, objArray);
		}

		[EtwEvent(0x1008L)]
		public void GetInitialSessionStateRequestFailed(string param1, string param2)
		{
			object[] objArray = new object[2];
			objArray[0] = param1;
			objArray[1] = param2;
			base.WriteEvent(Tracer.GetInitialSessionStateRequestFailedEvent, objArray);
		}

		[EtwEvent(0x1007L)]
		public void GetInitialSessionStateRequestSucceeded(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.GetInitialSessionStateRequestSucceededEvent, objArray);
		}

		[EtwEvent(0xfa6L)]
		public void GetMembershipId(string param1, string param2)
		{
			object[] objArray = new object[2];
			objArray[0] = param1;
			objArray[1] = param2;
			base.WriteEvent(Tracer.GetMembershipIdEvent, objArray);
		}

		[EtwEvent(0x835L)]
		public void HandleExceptionEvent(string userName, string exceptionCode, string exceptionMessage)
		{
			object[] objArray = new object[3];
			objArray[0] = userName;
			objArray[1] = exceptionCode;
			objArray[2] = exceptionMessage;
			base.WriteEvent(Tracer.HandleExceptionEventEvent, objArray);
		}

		[EtwEvent(0x451L)]
		public void IdentityDescription(string userName, string authenticationType, string isAuthenticated, string isWindowsIdentity)
		{
			object[] objArray = new object[4];
			objArray[0] = userName;
			objArray[1] = authenticationType;
			objArray[2] = isAuthenticated;
			objArray[3] = isWindowsIdentity;
			base.WriteEvent(Tracer.IdentityDescriptionEvent, objArray);
		}

		[EtwEvent(0x7d4L)]
		public void IncomingMessage(string uri, string httpRequestType)
		{
			object[] objArray = new object[2];
			objArray[0] = uri;
			objArray[1] = httpRequestType;
			base.WriteEvent(Tracer.IncomingMessageEvent, objArray);
		}

		[EtwEvent(0x1390L)]
		public void InformationRecord(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.InformationRecordEvent, objArray);
		}

		[EtwEvent(0x44fL)]
		public void InvalidDataServiceConfiguration(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.InvalidDataServiceConfigurationEvent, objArray);
		}

		[EtwEvent(0xa107L)]
		public void InvalidSchemaException(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.InvalidSchemaExceptionEvent, objArray);
		}

		[EtwEvent(0x7d3L)]
		public void InvalidUpdateQuery(string resourceType, string query, string exceptionType, string exceptionMessage)
		{
			object[] objArray = new object[4];
			objArray[0] = resourceType;
			objArray[1] = query;
			objArray[2] = exceptionType;
			objArray[3] = exceptionMessage;
			base.WriteEvent(Tracer.InvalidUpdateQueryEvent, objArray);
		}

		[EtwEvent(0x515L)]
		public void InvalidUriForPublicRootHeader(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.InvalidUriForPublicRootHeaderEvent, objArray);
		}

		[EtwEvent(0xa10fL)]
		public void InvocationInstance(string context, int hashCode, string id, string command, string outputFormat, string expirationTime, string status, int errorCount, string errorMessage)
		{
			object[] objArray = new object[9];
			objArray[0] = context;
			objArray[1] = hashCode;
			objArray[2] = id;
			objArray[3] = command;
			objArray[4] = outputFormat;
			objArray[5] = expirationTime;
			objArray[6] = status;
			objArray[7] = errorCount;
			objArray[8] = errorMessage;
			base.WriteEvent(Tracer.InvocationInstanceEvent, objArray);
		}

		[EtwEvent(0x5e6L)]
		public void MaxCmdletQuotaViolation(uint value)
		{
			object[] objArray = new object[1];
			objArray[0] = value;
			base.WriteEvent(Tracer.MaxCmdletQuotaViolationEvent, objArray);
		}

		[EtwEvent(0xb55L)]
		public void MethodCall0(string className, string method)
		{
			object[] objArray = new object[2];
			objArray[0] = className;
			objArray[1] = method;
			base.WriteEvent(Tracer.MethodCall0Event, objArray);
		}

		[EtwEvent(0xb56L)]
		public void MethodCall1(string className, string method, string param)
		{
			object[] objArray = new object[3];
			objArray[0] = className;
			objArray[1] = method;
			objArray[2] = param;
			base.WriteEvent(Tracer.MethodCall1Event, objArray);
		}

		[EtwEvent(0xb57L)]
		public void MethodCall2(string className, string method, string param1, string param2)
		{
			object[] objArray = new object[4];
			objArray[0] = className;
			objArray[1] = method;
			objArray[2] = param1;
			objArray[3] = param2;
			base.WriteEvent(Tracer.MethodCall2Event, objArray);
		}

		[EtwEvent(0xb58L)]
		public void MethodCall3(string className, string method, string param1, string param2, string param3)
		{
			object[] objArray = new object[5];
			objArray[0] = className;
			objArray[1] = method;
			objArray[2] = param1;
			objArray[3] = param2;
			objArray[4] = param3;
			base.WriteEvent(Tracer.MethodCall3Event, objArray);
		}

		[EtwEvent(0xa113L)]
		public void OperationalPipelineError(Guid id, string command, int errorCount, string error)
		{
			object[] objArray = new object[4];
			objArray[0] = id;
			objArray[1] = command;
			objArray[2] = errorCount;
			objArray[3] = error;
			base.WriteEvent(Tracer.OperationalPipelineErrorEvent, objArray);
		}

		[EtwEvent(0x7d5L)]
		public void OutgoingMessage(string uri, int statusCode, string requestId, string dataServiceVersion, string contentType)
		{
			object[] objArray = new object[5];
			objArray[0] = uri;
			objArray[1] = statusCode;
			objArray[2] = requestId;
			objArray[3] = dataServiceVersion;
			objArray[4] = contentType;
			base.WriteEvent(Tracer.OutgoingMessageEvent, objArray);
		}

		[EtwEvent(0xa111L)]
		public void PipelineComplete(Guid id)
		{
			object[] objArray = new object[1];
			objArray[0] = id;
			base.WriteEvent(Tracer.PipelineCompleteEvent, objArray);
		}

		[EtwEvent(0xa112L)]
		public void PipelineError(Guid id, string command, int errorCount, string error)
		{
			object[] objArray = new object[4];
			objArray[0] = id;
			objArray[1] = command;
			objArray[2] = errorCount;
			objArray[3] = error;
			base.WriteEvent(Tracer.PipelineErrorEvent, objArray);
		}

		[EtwEvent(0xa110L)]
		public void PipelineStart(Guid id, string command, string outputFormat)
		{
			object[] objArray = new object[3];
			objArray[0] = id;
			objArray[1] = command;
			objArray[2] = outputFormat;
			base.WriteEvent(Tracer.PipelineStartEvent, objArray);
		}

		[EtwEvent(0x138eL)]
		public void PowerShellInstance(Guid instanceId, string command, string invocationState, string invocationReason)
		{
			object[] objArray = new object[4];
			objArray[0] = instanceId;
			objArray[1] = command;
			objArray[2] = invocationState;
			objArray[3] = invocationReason;
			base.WriteEvent(Tracer.PowerShellInstanceEvent, objArray);
		}

		[EtwEvent(0x1389L)]
		public void PowerShellPipelineCreationFailed(string param1, string param2)
		{
			object[] objArray = new object[2];
			objArray[0] = param1;
			objArray[1] = param2;
			base.WriteEvent(Tracer.PowerShellPipelineCreationFailedEvent, objArray);
		}

		[EtwEvent(0x1b68L)]
		public void PowerShellRunspaceCreationEnd(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.PowerShellRunspaceCreationEndEvent, objArray);
		}

		[EtwEvent(0x1b67L)]
		public void PowerShellRunspaceCreationStart(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.PowerShellRunspaceCreationStartEvent, objArray);
		}

		[EtwEvent(0x138bL)]
		public void PSObjectSerializationFailed(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.PSObjectSerializationFailedEvent, objArray);
		}

		[EtwEvent(0xa105L)]
		public void PSObjectSerializationFailedException(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.PSObjectSerializationFailedExceptionEvent, objArray);
		}

		[EtwEvent(0x1b60L)]
		public void PSSessionCallEnd(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.PSSessionCallEndEvent, objArray);
		}

		[EtwEvent(0x1b5fL)]
		public void PSSessionCallStart(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.PSSessionCallStartEvent, objArray);
		}

		[EtwEvent(0x1005L)]
		public void PSSessionConfigurationLoadedSuccessfully(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.PSSessionConfigurationLoadedSuccessfullyEvent, objArray);
		}

		[EtwEvent(0x1006L)]
		public void PSSessionConfigurationLoadingFailed(string param1, string param2)
		{
			object[] objArray = new object[2];
			objArray[0] = param1;
			objArray[1] = param2;
			base.WriteEvent(Tracer.PSSessionConfigurationLoadingFailedEvent, objArray);
		}

		[EtwEvent(0x1009L)]
		public void PSSessionMethodExceededTimeLimit(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.PSSessionMethodExceededTimeLimitEvent, objArray);
		}

		[EtwEvent(0x8fdL)]
		public void QueryGetQueryRootForResourceFailed(string param1, string param2)
		{
			object[] objArray = new object[2];
			objArray[0] = param1;
			objArray[1] = param2;
			base.WriteEvent(Tracer.QueryGetQueryRootForResourceFailedEvent, objArray);
		}

		[EtwEvent(0x1b5aL)]
		public void RequestEnd()
		{
			base.WriteEvent(Tracer.RequestEndEvent, new object[0]);
		}

		[EtwEvent(0x5deL)]
		public void RequestPerSecondQuotaViolationCount(int quotaViolations, string startTime)
		{
			object[] objArray = new object[2];
			objArray[0] = quotaViolations;
			objArray[1] = startTime;
			base.WriteEvent(Tracer.RequestPerSecondQuotaViolationCountEvent, objArray);
		}

		[EtwEvent(0x1b5cL)]
		public void RequestProcessingEnd()
		{
			base.WriteEvent(Tracer.RequestProcessingEndEvent, new object[0]);
		}

		[EtwEvent(0x1b5bL)]
		public void RequestProcessingStart()
		{
			base.WriteEvent(Tracer.RequestProcessingStartEvent, new object[0]);
		}

		[EtwEvent(0x1b59L)]
		public void RequestStart()
		{
			base.WriteEvent(Tracer.RequestStartEvent, new object[0]);
		}

		[EtwEvent(0xa109L)]
		public void ResourcePropertyNotFoundException(string param1, string param2)
		{
			object[] objArray = new object[2];
			objArray[0] = param1;
			objArray[1] = param2;
			base.WriteEvent(Tracer.ResourcePropertyNotFoundExceptionEvent, objArray);
		}

		[EtwEvent(0xbc1L)]
		public void ResourceSetNotFound(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.ResourceSetNotFoundEvent, objArray);
		}

		[EtwEvent(0xa10eL)]
		public void ResourceTypeNotFound(string type)
		{
			object[] objArray = new object[1];
			objArray[0] = type;
			base.WriteEvent(Tracer.ResourceTypeNotFoundEvent, objArray);
		}

		[EtwEvent(0xbbdL)]
		public void SchemaFileInvalidCmdletFieldParameter(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.SchemaFileInvalidCmdletFieldParameterEvent, objArray);
		}

		[EtwEvent(0xbbcL)]
		public void SchemaFileInvalidCmdletName(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.SchemaFileInvalidCmdletNameEvent, objArray);
		}

		[EtwEvent(0xbbeL)]
		public void SchemaFileInvalidCmdletParameters(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.SchemaFileInvalidCmdletParametersEvent, objArray);
		}

		[EtwEvent(0xbb9L)]
		public void SchemaFileLoadedSuccessfully(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.SchemaFileLoadedSuccessfullyEvent, objArray);
		}

		[EtwEvent(0xbbaL)]
		public void SchemaFileNotFound(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.SchemaFileNotFoundEvent, objArray);
		}

		[EtwEvent(0xbbbL)]
		public void SchemaFileNotValidCsdl(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.SchemaFileNotValidCsdlEvent, objArray);
		}

		[EtwEvent(0x1b64L)]
		public void SchemaLoadingEnd(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.SchemaLoadingEndEvent, objArray);
		}

		[EtwEvent(0x1b63L)]
		public void SchemaLoadingStart(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.SchemaLoadingStartEvent, objArray);
		}

		[EtwEvent(0x138dL)]
		public void SerializationMaximumObjectDepthReached()
		{
			base.WriteEvent(Tracer.SerializationMaximumObjectDepthReachedEvent, new object[0]);
		}

		[EtwEvent(0x138cL)]
		public void SerializationPropertyNotFound(string param1, string param2)
		{
			object[] objArray = new object[2];
			objArray[0] = param1;
			objArray[1] = param2;
			base.WriteEvent(Tracer.SerializationPropertyNotFoundEvent, objArray);
		}

		[EtwEvent(0x4b4L)]
		public void SharedStoreCreatedNew(string userName, string itemTypeName)
		{
			object[] objArray = new object[2];
			objArray[0] = userName;
			objArray[1] = itemTypeName;
			base.WriteEvent(Tracer.SharedStoreCreatedNewEvent, objArray);
		}

		[EtwEvent(0x4b3L)]
		public void SharedStoreTookFromCache(string userName, string itemTypeName)
		{
			object[] objArray = new object[2];
			objArray[0] = userName;
			objArray[1] = itemTypeName;
			base.WriteEvent(Tracer.SharedStoreTookFromCacheEvent, objArray);
		}

		[EtwEvent(0x5e4L)]
		public void SystemQuotaViolationCount(int quotaViolations, string startTime)
		{
			object[] objArray = new object[2];
			objArray[0] = quotaViolations;
			objArray[1] = startTime;
			base.WriteEvent(Tracer.SystemQuotaViolationCountEvent, objArray);
		}

		[EtwEvent(0xa102L)]
		public void TypeLoadException(string typeName, string assemblyName, string applicationBase)
		{
			object[] objArray = new object[3];
			objArray[0] = typeName;
			objArray[1] = assemblyName;
			objArray[2] = applicationBase;
			base.WriteEvent(Tracer.TypeLoadExceptionEvent, objArray);
		}

		[EtwEvent(0xa103L)]
		public void UnauthorizedAccessException(string userName, string authenticationType)
		{
			object[] objArray = new object[2];
			objArray[0] = userName;
			objArray[1] = authenticationType;
			base.WriteEvent(Tracer.UnauthorizedAccessExceptionEvent, objArray);
		}

		[EtwEvent(0x8feL)]
		public void UriParsingFailed(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.UriParsingFailedEvent, objArray);
		}

		[EtwEvent(0x7d1L)]
		public void UserAuthorizedSuccessfully(string userName, string authenticationType)
		{
			object[] objArray = new object[2];
			objArray[0] = userName;
			objArray[1] = authenticationType;
			base.WriteEvent(Tracer.UserAuthorizedSuccessfullyEvent, objArray);
		}

		[EtwEvent(0x7d2L)]
		public void UserNotAuthorized(string userName, string authenticationType)
		{
			object[] objArray = new object[2];
			objArray[0] = userName;
			objArray[1] = authenticationType;
			base.WriteEvent(Tracer.UserNotAuthorizedEvent, objArray);
		}

		[EtwEvent(0x5e7L)]
		public void UserQuotaInformation(string userName, int maxConcurrentRequest, int maxRequestPerTimeSlot, int timeSlotSize)
		{
			object[] objArray = new object[4];
			objArray[0] = userName;
			objArray[1] = maxConcurrentRequest;
			objArray[2] = maxRequestPerTimeSlot;
			objArray[3] = timeSlotSize;
			base.WriteEvent(Tracer.UserQuotaInformationEvent, objArray);
		}

		[EtwEvent(0x5e3L)]
		public void UserQuotaSucceeded(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.UserQuotaSucceededEvent, objArray);
		}

		[EtwEvent(0x5e2L)]
		public void UserQuotaViolation(string param1, string param2)
		{
			object[] objArray = new object[2];
			objArray[0] = param1;
			objArray[1] = param2;
			base.WriteEvent(Tracer.UserQuotaViolationEvent, objArray);
		}

		[EtwEvent(0x5e5L)]
		public void UserQuotaViolationCount(int quotaViolations, string startTime)
		{
			object[] objArray = new object[2];
			objArray[0] = quotaViolations;
			objArray[1] = startTime;
			base.WriteEvent(Tracer.UserQuotaViolationCountEvent, objArray);
		}

		[EtwEvent(0x1b66L)]
		public void UserSchemaCreationEnd(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.UserSchemaCreationEndEvent, objArray);
		}

		[EtwEvent(0xbc0L)]
		public void UserSchemaCreationFailed(string param1, string param2)
		{
			object[] objArray = new object[2];
			objArray[0] = param1;
			objArray[1] = param2;
			base.WriteEvent(Tracer.UserSchemaCreationFailedEvent, objArray);
		}

		[EtwEvent(0x1b65L)]
		public void UserSchemaCreationStart(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.UserSchemaCreationStartEvent, objArray);
		}

		[EtwEvent(0xbbfL)]
		public void UserSchemaCreationSucceeded(string param1)
		{
			object[] objArray = new object[1];
			objArray[0] = param1;
			base.WriteEvent(Tracer.UserSchemaCreationSucceededEvent, objArray);
		}

		[EtwEvent(0x450L)]
		public void ValidDataServiceConfiguration()
		{
			base.WriteEvent(Tracer.ValidDataServiceConfigurationEvent, new object[0]);
		}

		[EtwEvent(0xd001L)]
		public void WriteTransferEvent()
		{
			base.WriteEvent(Tracer.WriteTransferEventEvent, new object[0]);
		}

		[EtwEvent(0xa108L)]
		public void WrongResourceTypeUsedException(string param1, string param2, string param3)
		{
			object[] objArray = new object[3];
			objArray[0] = param1;
			objArray[1] = param2;
			objArray[2] = param3;
			base.WriteEvent(Tracer.WrongResourceTypeUsedExceptionEvent, objArray);
		}
	}
}