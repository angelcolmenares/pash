using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
	public abstract class MultipleServiceCommandBase : ServiceBaseCommand
	{
		internal MultipleServiceCommandBase.SelectionMode selectionMode;

		internal string[] serviceNames;

		internal string[] displayNames;

		internal string[] include;

		internal string[] exclude;

		private ServiceController[] inputObject;

		private ServiceController[] allServices;

		private string[] computerName;

		internal ServiceController[] AllServices
		{
			get
			{
				if (this.allServices == null)
				{
					List<ServiceController> serviceControllers = new List<ServiceController>();
					if ((int)this.SuppliedComputerName.Length <= 0)
					{
						try {
							serviceControllers.AddRange(ServiceController.GetServices());
						}
						catch(Exception ex)
						{
							var stack = ex.StackTrace;
						}
					}
					else
					{
						string[] suppliedComputerName = this.SuppliedComputerName;
						for (int i = 0; i < (int)suppliedComputerName.Length; i++)
						{
							string str = suppliedComputerName[i];
							serviceControllers.AddRange(ServiceController.GetServices(str));
						}
					}
					this.allServices = serviceControllers.ToArray();
				}
				return this.allServices;
			}
		}

		[Parameter(ParameterSetName="DisplayName", Mandatory=true)]
		public string[] DisplayName
		{
			get
			{
				return this.displayNames;
			}
			set
			{
				this.displayNames = value;
				this.selectionMode = MultipleServiceCommandBase.SelectionMode.DisplayName;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string[] Exclude
		{
			get
			{
				return this.exclude;
			}
			set
			{
				this.exclude = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string[] Include
		{
			get
			{
				return this.include;
			}
			set
			{
				this.include = value;
			}
		}

		[Parameter(ParameterSetName="InputObject", ValueFromPipeline=true)]
		[ValidateNotNullOrEmpty]
		public ServiceController[] InputObject
		{
			get
			{
				return this.inputObject;
			}
			set
			{
				this.inputObject = value;
				this.selectionMode = MultipleServiceCommandBase.SelectionMode.InputObject;
			}
		}

		protected string[] SuppliedComputerName
		{
			get
			{
				return this.computerName;
			}
			set
			{
				this.computerName = value;
			}
		}

		protected MultipleServiceCommandBase()
		{
			this.computerName = new string[0];
		}

		private void AddIfValidService(IList<ServiceController> listOfValidServices, string nameOfService, string computerName)
		{
			try
			{
				ServiceController serviceController = new ServiceController(nameOfService, computerName);
				listOfValidServices.Add(serviceController);
			}
			catch (InvalidOperationException invalidOperationException)
			{
			}
			catch (ArgumentException argumentException)
			{
			}
		}

		private void IncludeExcludeAdd(List<ServiceController> list, ServiceController service, bool checkDuplicates)
		{
			if (this.include == null || this.Matches(service, this.include))
			{
				if (this.exclude == null || !this.Matches(service, this.exclude))
				{
					if (checkDuplicates)
					{
						foreach (ServiceController serviceController in list)
						{
							if (!(serviceController.ServiceName == service.ServiceName) || !(serviceController.MachineName == service.MachineName))
							{
								continue;
							}
							return;
						}
					}
					list.Add(service);
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private bool Matches(ServiceController service, string[] matchList)
		{
			string displayName;
			if (matchList != null)
			{
				if (this.selectionMode == MultipleServiceCommandBase.SelectionMode.DisplayName)
				{
					displayName = service.DisplayName;
				}
				else
				{
					displayName = service.ServiceName;
				}
				string str = displayName;
				string[] strArrays = matchList;
				int num = 0;
				while (num < (int)strArrays.Length)
				{
					string str1 = strArrays[num];
					WildcardPattern wildcardPattern = new WildcardPattern(str1, WildcardOptions.IgnoreCase);
					if (!wildcardPattern.IsMatch(str))
					{
						num++;
					}
					else
					{
						bool flag = true;
						return flag;
					}
				}
				return false;
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("matchList");
			}
		}

		internal List<ServiceController> MatchingServices()
		{
			List<ServiceController> serviceControllers;
			MultipleServiceCommandBase.SelectionMode selectionMode = this.selectionMode;
			switch (selectionMode)
			{
				case MultipleServiceCommandBase.SelectionMode.DisplayName:
				{
					serviceControllers = this.MatchingServicesByDisplayName();
					break;
				}
				case MultipleServiceCommandBase.SelectionMode.InputObject:
				{
					serviceControllers = this.MatchingServicesByInput();
					break;
				}
				default:
				{
					serviceControllers = this.MatchingServicesByServiceName();
					break;
				}
			}
			serviceControllers.Sort(new Comparison<ServiceController>(MultipleServiceCommandBase.ServiceComparison));
			return serviceControllers;
		}

		private List<ServiceController> MatchingServicesByDisplayName()
		{
			List<ServiceController> serviceControllers = new List<ServiceController>();
			if (this.DisplayName != null)
			{
				string[] displayName = this.DisplayName;
				for (int i = 0; i < (int)displayName.Length; i++)
				{
					string str = displayName[i];
					WildcardPattern wildcardPattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
					bool flag = false;
					ServiceController[] allServices = this.AllServices;
					for (int j = 0; j < (int)allServices.Length; j++)
					{
						ServiceController serviceController = allServices[j];
						if (wildcardPattern.IsMatch(serviceController.DisplayName))
						{
							flag = true;
							this.IncludeExcludeAdd(serviceControllers, serviceController, true);
						}
					}
					if (!flag && !WildcardPattern.ContainsWildcardCharacters(str))
					{
						base.WriteNonTerminatingError("", str, str, null, "NoServiceFoundForGivenDisplayName", ServiceResources.NoServiceFoundForGivenDisplayName, ErrorCategory.ObjectNotFound);
					}
				}
				return serviceControllers;
			}
			else
			{
				throw PSTraceSource.NewInvalidOperationException();
			}
		}

		private List<ServiceController> MatchingServicesByInput()
		{
			List<ServiceController> serviceControllers = new List<ServiceController>();
			if (this.InputObject != null)
			{
				ServiceController[] inputObject = this.InputObject;
				for (int i = 0; i < (int)inputObject.Length; i++)
				{
					ServiceController serviceController = inputObject[i];
					serviceController.Refresh();
					this.IncludeExcludeAdd(serviceControllers, serviceController, false);
				}
				return serviceControllers;
			}
			else
			{
				throw PSTraceSource.NewInvalidOperationException();
			}
		}

		private List<ServiceController> MatchingServicesByServiceName()
		{
			List<ServiceController> serviceControllers = new List<ServiceController>();
			if (this.serviceNames != null)
			{
				string[] strArrays = this.serviceNames;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					bool flag = false;
					if (!WildcardPattern.ContainsWildcardCharacters(str))
					{
						foreach (ServiceController serviceController in this.OneService(str))
						{
							flag = true;
							this.IncludeExcludeAdd(serviceControllers, serviceController, true);
						}
					}
					else
					{
						WildcardPattern wildcardPattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
						ServiceController[] allServices = this.AllServices;
						for (int j = 0; j < (int)allServices.Length; j++)
						{
							ServiceController serviceController1 = allServices[j];
							if (wildcardPattern.IsMatch(serviceController1.ServiceName))
							{
								flag = true;
								this.IncludeExcludeAdd(serviceControllers, serviceController1, true);
							}
						}
					}
					if (!flag && !WildcardPattern.ContainsWildcardCharacters(str))
					{
						base.WriteNonTerminatingError(str, "", str, null, "NoServiceFoundForGivenName", ServiceResources.NoServiceFoundForGivenName, ErrorCategory.ObjectNotFound);
					}
				}
				return serviceControllers;
			}
			else
			{
				ServiceController[] serviceControllerArray = this.AllServices;
				for (int k = 0; k < (int)serviceControllerArray.Length; k++)
				{
					ServiceController serviceController2 = serviceControllerArray[k];
					this.IncludeExcludeAdd(serviceControllers, serviceController2, false);
				}
				return serviceControllers;
			}
		}

		internal List<ServiceController> OneService(string nameOfService)
		{
			List<ServiceController> serviceControllers = new List<ServiceController>();
			if ((int)this.SuppliedComputerName.Length <= 0)
			{
				this.AddIfValidService(serviceControllers, nameOfService, ".");
			}
			else
			{
				string[] suppliedComputerName = this.SuppliedComputerName;
				for (int i = 0; i < (int)suppliedComputerName.Length; i++)
				{
					string str = suppliedComputerName[i];
					this.AddIfValidService(serviceControllers, nameOfService, str);
				}
			}
			return serviceControllers;
		}

		private static int ServiceComparison(ServiceController x, ServiceController y)
		{
			return string.Compare(x.ServiceName, y.ServiceName, StringComparison.CurrentCultureIgnoreCase);
		}

		internal enum SelectionMode
		{
			Default,
			DisplayName,
			InputObject,
			ServiceName
		}
	}
}