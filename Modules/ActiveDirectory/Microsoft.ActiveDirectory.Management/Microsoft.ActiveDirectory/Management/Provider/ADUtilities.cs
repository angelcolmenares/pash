using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;
using System.Security.Authentication;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	internal class ADUtilities
	{
		public ADUtilities()
		{
		}

		internal static ErrorCategory GetErrorCategory(Exception exception)
		{
			ErrorCategory errorCategory = ErrorCategory.NotSpecified;
			if (exception.GetType() == typeof(ArgumentException) || exception.GetType() == typeof(ArgumentNullException) || exception.GetType() == typeof(ArgumentOutOfRangeException))
			{
				errorCategory = ErrorCategory.InvalidArgument;
			}
			else
			{
				if (exception.GetType() == typeof(ADMultipleMatchingIdentitiesException) || exception.GetType() == typeof(ADPasswordException) || exception.GetType() == typeof(ADInvalidPasswordException) || exception.GetType() == typeof(ADPasswordComplexityException) || exception.GetType() == typeof(ADIdentityResolutionException))
				{
					errorCategory = ErrorCategory.InvalidData;
				}
				else
				{
					if (exception.GetType() != typeof(ADIdentityNotFoundException))
					{
						if (exception.GetType() != typeof(ADIdentityAlreadyExistsException))
						{
							if (exception.GetType() != typeof(AuthenticationException))
							{
								if (exception.GetType() == typeof(ADServerDownException) || exception.GetType() == typeof(ADReferralException))
								{
									errorCategory = ErrorCategory.ResourceUnavailable;
								}
								else
								{
									if (exception.GetType() != typeof(ADFilterParsingException))
									{
										if (exception.GetType() != typeof(UnauthorizedAccessException))
										{
											if (exception.GetType() == typeof(ADInvalidOperationException) || exception.GetType() == typeof(NotSupportedException) || exception.GetType() == typeof(ADIllegalModifyOperationException))
											{
												errorCategory = ErrorCategory.InvalidOperation;
											}
											else
											{
												if (exception.GetType() != typeof(NotImplementedException))
												{
													if (exception.GetType() != typeof(TimeoutException))
													{
														if (exception.GetType() == typeof(FormatException) || exception.GetType() == typeof(UriFormatException))
														{
															errorCategory = ErrorCategory.InvalidType;
														}
													}
													else
													{
														errorCategory = ErrorCategory.OperationTimeout;
													}
												}
												else
												{
													errorCategory = ErrorCategory.NotImplemented;
												}
											}
										}
										else
										{
											errorCategory = ErrorCategory.PermissionDenied;
										}
									}
									else
									{
										errorCategory = ErrorCategory.ParserError;
									}
								}
							}
							else
							{
								errorCategory = ErrorCategory.SecurityError;
							}
						}
						else
						{
							errorCategory = ErrorCategory.ResourceExists;
						}
					}
					else
					{
						errorCategory = ErrorCategory.ObjectNotFound;
					}
				}
			}
			return errorCategory;
		}

		internal static ErrorRecord GetErrorRecord(Exception exception, string errorId, object errorTarget)
		{
			return new ErrorRecord(exception, errorId, ADUtilities.GetErrorCategory(exception), errorTarget);
		}
	}
}