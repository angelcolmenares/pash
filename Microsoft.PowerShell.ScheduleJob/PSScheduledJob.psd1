@{

# Script module or binary module file associated with this manifest
ModuleToProcess = 'Microsoft.PowerShell.ScheduledJob.dll'

# Version number of this module.
ModuleVersion = '1.0.0.0'

# ID used to uniquely identify this module
GUID = '50cdb55f-5ab7-489f-9e94-4ec21ff51e59'

# Author of this module
Author = 'Microsoft Corporation'

# Company or vendor of this module
CompanyName = 'Microsoft Corporation'

# Copyright statement for this module
Copyright = '© Microsoft Corporation. All rights reserved.'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '3.0'

# Minimum version of the common language runtime (CLR) required by this module
CLRVersion = '4.0'

# Type files (.ps1xml) to be loaded when importing this module
#TypesToProcess = 'PSScheduledJob.types.ps1xml'

FormatsToProcess="PSScheduledJob.Format.ps1xml"

# Cmdlets to export from this module
CmdletsToExport = 'New-JobTrigger', 'Add-JobTrigger', 'Remove-JobTrigger', 
               'Get-JobTrigger', 'Set-JobTrigger', 'Enable-JobTrigger', 
               'Disable-JobTrigger', 'New-ScheduledJobOption', 'Get-ScheduledJobOption',
               'Set-ScheduledJobOption', 'Register-ScheduledJob', 'Get-ScheduledJob',
               'Set-ScheduledJob', 'Unregister-ScheduledJob', 'Enable-ScheduledJob',
               'Disable-ScheduledJob'
HelpInfoURI = 'http://go.microsoft.com/fwlink/?LinkID=223911'
}
