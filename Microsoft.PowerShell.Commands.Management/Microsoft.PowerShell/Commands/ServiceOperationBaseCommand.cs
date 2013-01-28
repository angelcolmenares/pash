using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
	public abstract class ServiceOperationBaseCommand : MultipleServiceCommandBase
	{
		private SwitchParameter passThru;

		[Parameter(Position=0, Mandatory=true, ParameterSetName="InputObject", ValueFromPipeline=true)]
		[ValidateNotNullOrEmpty]
		public ServiceController[] InputObject
		{
			get
			{
				return base.InputObject;
			}
			set
			{
				base.InputObject = value;
			}
		}

		[Alias(new string[] { "ServiceName" })]
		[Parameter(Position=0, ParameterSetName="Default", Mandatory=true, ValueFromPipelineByPropertyName=true, ValueFromPipeline=true)]
		public string[] Name
		{
			get
			{
				return this.serviceNames;
			}
			set
			{
				this.serviceNames = value;
				this.selectionMode = MultipleServiceCommandBase.SelectionMode.ServiceName;
			}
		}

		[Parameter]
		public SwitchParameter PassThru
		{
			get
			{
				return this.passThru;
			}
			set
			{
				this.passThru = value;
			}
		}

		protected ServiceOperationBaseCommand()
		{
		}

		internal bool DoPauseService(ServiceController serviceController)
		{
			Exception exception = null;
			bool flag = false;
			try
			{
				serviceController.Pause();
			}
			catch (Win32Exception win32Exception1)
			{
				Win32Exception win32Exception = win32Exception1;
				if (0x426 == win32Exception.NativeErrorCode)
				{
					flag = true;
				}
				exception = win32Exception;
			}
			catch (InvalidOperationException invalidOperationException1)
			{
				InvalidOperationException invalidOperationException = invalidOperationException1;
				Win32Exception innerException = invalidOperationException.InnerException as Win32Exception;
				if (innerException != null && 0x426 == innerException.NativeErrorCode)
				{
					flag = true;
				}
				exception = invalidOperationException;
			}
			if (exception == null)
			{
				if (this.DoWaitForStatus(serviceController, ServiceControllerStatus.Paused, ServiceControllerStatus.PausePending, ServiceResources.SuspendingService, "SuspendServiceFailed", ServiceResources.SuspendServiceFailed))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				if (!flag)
				{
					if (!serviceController.CanPauseAndContinue)
					{
						base.WriteNonTerminatingError(serviceController, exception, "CouldNotSuspendServiceNotSupported", ServiceResources.CouldNotSuspendServiceNotSupported, ErrorCategory.CloseError);
					}
				}
				else
				{
					base.WriteNonTerminatingError(serviceController, exception, "CouldNotSuspendServiceNotRunning", ServiceResources.CouldNotSuspendServiceNotRunning, ErrorCategory.CloseError);
				}
				base.WriteNonTerminatingError(serviceController, exception, "CouldNotSuspendService", ServiceResources.CouldNotSuspendService, ErrorCategory.CloseError);
				return false;
			}
		}

		internal bool DoResumeService(ServiceController serviceController)
		{
			Exception exception = null;
			bool flag = false;
			try
			{
				serviceController.Continue();
			}
			catch (Win32Exception win32Exception1)
			{
				Win32Exception win32Exception = win32Exception1;
				if (0x426 == win32Exception.NativeErrorCode)
				{
					flag = true;
				}
				exception = win32Exception;
			}
			catch (InvalidOperationException invalidOperationException1)
			{
				InvalidOperationException invalidOperationException = invalidOperationException1;
				Win32Exception innerException = invalidOperationException.InnerException as Win32Exception;
				if (innerException != null && 0x426 == innerException.NativeErrorCode)
				{
					flag = true;
				}
				exception = invalidOperationException;
			}
			if (exception == null)
			{
				if (this.DoWaitForStatus(serviceController, ServiceControllerStatus.Running, ServiceControllerStatus.ContinuePending, ServiceResources.ResumingService, "ResumeServiceFailed", ServiceResources.ResumeServiceFailed))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				if (!flag)
				{
					if (!serviceController.CanPauseAndContinue)
					{
						base.WriteNonTerminatingError(serviceController, exception, "CouldNotResumeServiceNotSupported", ServiceResources.CouldNotResumeServiceNotSupported, ErrorCategory.CloseError);
					}
				}
				else
				{
					base.WriteNonTerminatingError(serviceController, exception, "CouldNotResumeServiceNotRunning", ServiceResources.CouldNotResumeServiceNotRunning, ErrorCategory.CloseError);
				}
				base.WriteNonTerminatingError(serviceController, exception, "CouldNotResumeService", ServiceResources.CouldNotResumeService, ErrorCategory.CloseError);
				return false;
			}
		}

		internal bool DoStartService(ServiceController serviceController)
		{
			Exception exception = null;
			try
			{
				serviceController.Start();
			}
			catch (Win32Exception win32Exception1)
			{
				Win32Exception win32Exception = win32Exception1;
				if (0x420 != win32Exception.NativeErrorCode)
				{
					exception = win32Exception;
				}
			}
			catch (InvalidOperationException invalidOperationException1)
			{
				InvalidOperationException invalidOperationException = invalidOperationException1;
				Win32Exception innerException = invalidOperationException.InnerException as Win32Exception;
				if (innerException == null || 0x420 != innerException.NativeErrorCode)
				{
					exception = invalidOperationException;
				}
			}
			if (exception == null)
			{
				if (this.DoWaitForStatus(serviceController, ServiceControllerStatus.Running, ServiceControllerStatus.StartPending, ServiceResources.StartingService, "StartServiceFailed", ServiceResources.StartServiceFailed))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				base.WriteNonTerminatingError(serviceController, exception, "CouldNotStartService", ServiceResources.CouldNotStartService, ErrorCategory.OpenError);
				return false;
			}
		}

		internal List<ServiceController> DoStopService(ServiceController serviceController, bool force)
		{
			List<ServiceController> serviceControllers = new List<ServiceController>();
			ServiceController[] dependentServices = null;
			try
			{
				dependentServices = serviceController.DependentServices;
			}
			catch (Win32Exception win32Exception1)
			{
				Win32Exception win32Exception = win32Exception1;
				base.WriteNonTerminatingError(serviceController, win32Exception, "CouldNotAccessDependentServices", ServiceResources.CouldNotAccessDependentServices, ErrorCategory.InvalidOperation);
			}
			catch (InvalidOperationException invalidOperationException1)
			{
				InvalidOperationException invalidOperationException = invalidOperationException1;
				base.WriteNonTerminatingError(serviceController, invalidOperationException, "CouldNotAccessDependentServices", ServiceResources.CouldNotAccessDependentServices, ErrorCategory.InvalidOperation);
			}
			if (force || dependentServices == null || (int)dependentServices.Length <= 0 || this.HaveAllDependentServicesStopped(dependentServices))
			{
				if (dependentServices != null)
				{
					ServiceController[] serviceControllerArray = dependentServices;
					for (int i = 0; i < (int)serviceControllerArray.Length; i++)
					{
						ServiceController serviceController1 = serviceControllerArray[i];
						if ((serviceController1.Status == ServiceControllerStatus.Running || serviceController1.Status == ServiceControllerStatus.StartPending) && serviceController1.CanStop)
						{
							serviceControllers.Add(serviceController1);
						}
					}
				}
				Exception exception = null;
				try
				{
					serviceController.Stop();
				}
				catch (Win32Exception win32Exception3)
				{
					Win32Exception win32Exception2 = win32Exception3;
					if (0x426 != win32Exception2.NativeErrorCode)
					{
						exception = win32Exception2;
					}
				}
				catch (InvalidOperationException invalidOperationException3)
				{
					InvalidOperationException invalidOperationException2 = invalidOperationException3;
					Win32Exception innerException = invalidOperationException2.InnerException as Win32Exception;
					if (innerException == null || 0x426 != innerException.NativeErrorCode)
					{
						exception = invalidOperationException2;
					}
				}
				if (exception == null)
				{
					if (this.DoWaitForStatus(serviceController, ServiceControllerStatus.Stopped, ServiceControllerStatus.StopPending, ServiceResources.StoppingService, "StopServiceFailed", ServiceResources.StopServiceFailed))
					{
						this.RemoveNotStoppedServices(serviceControllers);
						serviceControllers.Add(serviceController);
						return serviceControllers;
					}
					else
					{
						this.RemoveNotStoppedServices(serviceControllers);
						return serviceControllers;
					}
				}
				else
				{
					base.WriteNonTerminatingError(serviceController, exception, "CouldNotStopService", ServiceResources.CouldNotStopService, ErrorCategory.CloseError);
					this.RemoveNotStoppedServices(serviceControllers);
					return serviceControllers;
				}
			}
			else
			{
				base.WriteNonTerminatingError(serviceController, null, "ServiceHasDependentServices", ServiceResources.ServiceHasDependentServices, ErrorCategory.InvalidOperation);
				return serviceControllers;
			}
		}

		internal bool DoWaitForStatus(ServiceController serviceController, ServiceControllerStatus targetStatus, ServiceControllerStatus pendingStatus, string resourceIdPending, string errorId, string errorMessage)
		{
			bool flag;
			while (true)
			{
				try
				{
					serviceController.WaitForStatus(targetStatus, new TimeSpan((long)0x1312d00));
					flag = true;
					break;
				}
				catch (System.ServiceProcess.TimeoutException timeoutException)
				{
					if (serviceController.Status == pendingStatus || serviceController.Status == targetStatus)
					{
						string str = StringUtil.Format(resourceIdPending, serviceController.ServiceName, serviceController.DisplayName);
						base.WriteWarning(str);
					}
					else
					{
						base.WriteNonTerminatingError(serviceController, null, errorId, errorMessage, ErrorCategory.OpenError);
						flag = false;
						break;
					}
				}
			}
			return flag;
		}

		private bool HaveAllDependentServicesStopped(ICollection<ServiceController> dependentServices)
		{
			bool flag;
			IEnumerator<ServiceController> enumerator = dependentServices.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					ServiceController current = enumerator.Current;
					if (current.Status == ServiceControllerStatus.Stopped)
					{
						continue;
					}
					flag = false;
					return flag;
				}
				return true;
			}
			return flag;
		}

		internal void RemoveNotStoppedServices(List<ServiceController> services)
		{
			foreach (ServiceController service in services)
			{
				if (service.Status == ServiceControllerStatus.Stopped || service.Status == ServiceControllerStatus.StopPending)
				{
					continue;
				}
				services.Remove(service);
			}
		}
	}
}