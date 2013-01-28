using System;
using System.Collections;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text;

namespace System.DirectoryServices.ActiveDirectory
{
	internal class ExceptionHelper
	{
		private static int ERROR_NOT_ENOUGH_MEMORY;

		private static int ERROR_OUTOFMEMORY;

		private static int ERROR_DS_DRA_OUT_OF_MEM;

		private static int ERROR_NO_SUCH_DOMAIN;

		private static int ERROR_ACCESS_DENIED;

		private static int ERROR_NO_LOGON_SERVERS;

		private static int ERROR_DS_DRA_ACCESS_DENIED;

		private static int RPC_S_OUT_OF_RESOURCES;

		internal static int RPC_S_SERVER_UNAVAILABLE;

		internal static int RPC_S_CALL_FAILED;

		private static int ERROR_CANCELLED;

		internal static int ERROR_DS_DRA_BAD_DN;

		internal static int ERROR_DS_NAME_UNPARSEABLE;

		internal static int ERROR_DS_UNKNOWN_ERROR;

		static ExceptionHelper()
		{
			ExceptionHelper.ERROR_NOT_ENOUGH_MEMORY = 8;
			ExceptionHelper.ERROR_OUTOFMEMORY = 14;
			ExceptionHelper.ERROR_DS_DRA_OUT_OF_MEM = 0x20fe;
			ExceptionHelper.ERROR_NO_SUCH_DOMAIN = 0x54b;
			ExceptionHelper.ERROR_ACCESS_DENIED = 5;
			ExceptionHelper.ERROR_NO_LOGON_SERVERS = 0x51f;
			ExceptionHelper.ERROR_DS_DRA_ACCESS_DENIED = 0x2105;
			ExceptionHelper.RPC_S_OUT_OF_RESOURCES = 0x6b9;
			ExceptionHelper.RPC_S_SERVER_UNAVAILABLE = 0x6ba;
			ExceptionHelper.RPC_S_CALL_FAILED = 0x6be;
			ExceptionHelper.ERROR_CANCELLED = 0x4c7;
			ExceptionHelper.ERROR_DS_DRA_BAD_DN = 0x20f7;
			ExceptionHelper.ERROR_DS_NAME_UNPARSEABLE = 0x209e;
			ExceptionHelper.ERROR_DS_UNKNOWN_ERROR = 0x20ef;
		}

		public ExceptionHelper()
		{
		}

		internal static Exception CreateForestTrustCollisionException(IntPtr collisionInfo)
		{
			ForestTrustRelationshipCollisionCollection forestTrustRelationshipCollisionCollection = new ForestTrustRelationshipCollisionCollection();
			LSA_FOREST_TRUST_COLLISION_INFORMATION lSAFORESTTRUSTCOLLISIONINFORMATION = new LSA_FOREST_TRUST_COLLISION_INFORMATION();
			Marshal.PtrToStructure(collisionInfo, lSAFORESTTRUSTCOLLISIONINFORMATION);
			int recordCount = lSAFORESTTRUSTCOLLISIONINFORMATION.RecordCount;
			for (int i = 0; i < recordCount; i++)
			{
				IntPtr intPtr = Marshal.ReadIntPtr(lSAFORESTTRUSTCOLLISIONINFORMATION.Entries, i * Marshal.SizeOf(typeof(IntPtr)));
				LSA_FOREST_TRUST_COLLISION_RECORD lSAFORESTTRUSTCOLLISIONRECORD = new LSA_FOREST_TRUST_COLLISION_RECORD();
				Marshal.PtrToStructure(intPtr, lSAFORESTTRUSTCOLLISIONRECORD);
				ForestTrustCollisionType type = lSAFORESTTRUSTCOLLISIONRECORD.Type;
				string stringUni = Marshal.PtrToStringUni(lSAFORESTTRUSTCOLLISIONRECORD.Name.Buffer, lSAFORESTTRUSTCOLLISIONRECORD.Name.Length / 2);
				TopLevelNameCollisionOptions flags = TopLevelNameCollisionOptions.None;
				DomainCollisionOptions domainCollisionOption = DomainCollisionOptions.None;
				if (type != ForestTrustCollisionType.TopLevelName)
				{
					if (type == ForestTrustCollisionType.Domain)
					{
						domainCollisionOption = (DomainCollisionOptions)lSAFORESTTRUSTCOLLISIONRECORD.Flags;
					}
				}
				else
				{
					flags = (TopLevelNameCollisionOptions)lSAFORESTTRUSTCOLLISIONRECORD.Flags;
				}
				ForestTrustRelationshipCollision forestTrustRelationshipCollision = new ForestTrustRelationshipCollision(type, flags, domainCollisionOption, stringUni);
				forestTrustRelationshipCollisionCollection.Add(forestTrustRelationshipCollision);
			}
			ForestTrustCollisionException forestTrustCollisionException = new ForestTrustCollisionException(Res.GetString("ForestTrustCollision"), null, forestTrustRelationshipCollisionCollection);
			return forestTrustCollisionException;
		}

		internal static SyncFromAllServersOperationException CreateSyncAllException(IntPtr errorInfo, bool singleError)
		{
			if (errorInfo != (IntPtr)0)
			{
				if (!singleError)
				{
					IntPtr intPtr = Marshal.ReadIntPtr(errorInfo);
					ArrayList arrayLists = new ArrayList();
					int num = 0;
					while (intPtr != (IntPtr)0)
					{
						DS_REPSYNCALL_ERRINFO dSREPSYNCALLERRINFO = new DS_REPSYNCALL_ERRINFO();
						Marshal.PtrToStructure(intPtr, dSREPSYNCALLERRINFO);
						if (dSREPSYNCALLERRINFO.dwWin32Err != ExceptionHelper.ERROR_CANCELLED)
						{
							string errorMessage = ExceptionHelper.GetErrorMessage(dSREPSYNCALLERRINFO.dwWin32Err, false);
							string stringUni = Marshal.PtrToStringUni(dSREPSYNCALLERRINFO.pszSrcId);
							string str = Marshal.PtrToStringUni(dSREPSYNCALLERRINFO.pszSvrId);
							SyncFromAllServersErrorInformation syncFromAllServersErrorInformation = new SyncFromAllServersErrorInformation(dSREPSYNCALLERRINFO.error, dSREPSYNCALLERRINFO.dwWin32Err, errorMessage, stringUni, str);
							arrayLists.Add(syncFromAllServersErrorInformation);
						}
						num++;
						intPtr = Marshal.ReadIntPtr(errorInfo, num * Marshal.SizeOf(typeof(IntPtr)));
					}
					if (arrayLists.Count != 0)
					{
						SyncFromAllServersErrorInformation[] syncFromAllServersErrorInformationArray = new SyncFromAllServersErrorInformation[arrayLists.Count];
						for (int i = 0; i < arrayLists.Count; i++)
						{
							SyncFromAllServersErrorInformation item = (SyncFromAllServersErrorInformation)arrayLists[i];
							syncFromAllServersErrorInformationArray[i] = new SyncFromAllServersErrorInformation(item.ErrorCategory, item.ErrorCode, item.ErrorMessage, item.SourceServer, item.TargetServer);
						}
						return new SyncFromAllServersOperationException(Res.GetString("DSSyncAllFailure"), null, syncFromAllServersErrorInformationArray);
					}
					else
					{
						return null;
					}
				}
				else
				{
					DS_REPSYNCALL_ERRINFO dSREPSYNCALLERRINFO1 = new DS_REPSYNCALL_ERRINFO();
					Marshal.PtrToStructure(errorInfo, dSREPSYNCALLERRINFO1);
					string errorMessage1 = ExceptionHelper.GetErrorMessage(dSREPSYNCALLERRINFO1.dwWin32Err, false);
					string stringUni1 = Marshal.PtrToStringUni(dSREPSYNCALLERRINFO1.pszSrcId);
					string str1 = Marshal.PtrToStringUni(dSREPSYNCALLERRINFO1.pszSvrId);
					if (dSREPSYNCALLERRINFO1.dwWin32Err != ExceptionHelper.ERROR_CANCELLED)
					{
						SyncFromAllServersErrorInformation syncFromAllServersErrorInformation1 = new SyncFromAllServersErrorInformation(dSREPSYNCALLERRINFO1.error, dSREPSYNCALLERRINFO1.dwWin32Err, errorMessage1, stringUni1, str1);
						SyncFromAllServersErrorInformation[] syncFromAllServersErrorInformationArray1 = new SyncFromAllServersErrorInformation[1];
						syncFromAllServersErrorInformationArray1[0] = syncFromAllServersErrorInformation1;
						return new SyncFromAllServersOperationException(Res.GetString("DSSyncAllFailure"), null, syncFromAllServersErrorInformationArray1);
					}
					else
					{
						return null;
					}
				}
			}
			else
			{
				return new SyncFromAllServersOperationException();
			}
		}

		internal static string GetErrorMessage(int errorCode, bool hresult)
		{
			string str;
			int num = errorCode;
			if (!hresult)
			{
				num = num & 0xffff | 0x70000 | -2147483648;
			}
			StringBuilder stringBuilder = new StringBuilder(0x100);
			int num1 = UnsafeNativeMethods.FormatMessageW(0x3200, 0, num, 0, stringBuilder, stringBuilder.Capacity + 1, 0);
			if (num1 == 0)
			{
				object[] objArray = new object[1];
				objArray[0] = Convert.ToString((long)num, 16);
				str = Res.GetString("DSUnknown", objArray);
			}
			else
			{
				str = stringBuilder.ToString(0, num1);
			}
			return str;
		}

		internal static Exception GetExceptionFromCOMException(COMException e)
		{
			return ExceptionHelper.GetExceptionFromCOMException(null, e);
		}

		internal static Exception GetExceptionFromCOMException(DirectoryContext context, COMException e)
		{
			Exception activeDirectoryServerDownException;
			int errorCode = e.ErrorCode;
			string message = e.Message;
			if (errorCode != -2147024891)
			{
				if (errorCode != -2147023570)
				{
					if (errorCode != -2147016657)
					{
						if (errorCode != -2147016651)
						{
							if (errorCode != -2147019886)
							{
								if (errorCode != -2147024888)
								{
									if (errorCode == -2147016646 || errorCode == -2147016690 || errorCode == -2147016689)
									{
										if (context == null)
										{
											activeDirectoryServerDownException = new ActiveDirectoryServerDownException(message, e, errorCode, null);
										}
										else
										{
											activeDirectoryServerDownException = new ActiveDirectoryServerDownException(message, e, errorCode, context.GetServerName());
										}
									}
									else
									{
										activeDirectoryServerDownException = new ActiveDirectoryOperationException(message, e, errorCode);
									}
								}
								else
								{
									activeDirectoryServerDownException = new OutOfMemoryException();
								}
							}
							else
							{
								activeDirectoryServerDownException = new ActiveDirectoryObjectExistsException(message, e);
							}
						}
						else
						{
							activeDirectoryServerDownException = new InvalidOperationException(message, e);
						}
					}
					else
					{
						activeDirectoryServerDownException = new InvalidOperationException(message, e);
					}
				}
				else
				{
					activeDirectoryServerDownException = new AuthenticationException(message, e);
				}
			}
			else
			{
				activeDirectoryServerDownException = new UnauthorizedAccessException(message, e);
			}
			return activeDirectoryServerDownException;
		}

		internal static Exception GetExceptionFromErrorCode(int errorCode)
		{
			return ExceptionHelper.GetExceptionFromErrorCode(errorCode, null);
		}

		internal static Exception GetExceptionFromErrorCode(int errorCode, string targetName)
		{
			string errorMessage = ExceptionHelper.GetErrorMessage(errorCode, false);
			if (errorCode == ExceptionHelper.ERROR_ACCESS_DENIED || errorCode == ExceptionHelper.ERROR_DS_DRA_ACCESS_DENIED)
			{
				return new UnauthorizedAccessException(errorMessage);
			}
			else
			{
				if (errorCode == ExceptionHelper.ERROR_NOT_ENOUGH_MEMORY || errorCode == ExceptionHelper.ERROR_OUTOFMEMORY || errorCode == ExceptionHelper.ERROR_DS_DRA_OUT_OF_MEM || errorCode == ExceptionHelper.RPC_S_OUT_OF_RESOURCES)
				{
					return new OutOfMemoryException();
				}
				else
				{
					if (errorCode == ExceptionHelper.ERROR_NO_LOGON_SERVERS || errorCode == ExceptionHelper.ERROR_NO_SUCH_DOMAIN || errorCode == ExceptionHelper.RPC_S_SERVER_UNAVAILABLE || errorCode == ExceptionHelper.RPC_S_CALL_FAILED)
					{
						return new ActiveDirectoryServerDownException(errorMessage, errorCode, targetName);
					}
					else
					{
						return new ActiveDirectoryOperationException(errorMessage, errorCode);
					}
				}
			}
		}
	}
}