Complete the installation of Microsoft PowerShell Web Access by doing the following.

1) Open a Microsoft PowerShell console with elevated user rights 

To do this, on the Windows taskbar, right-click the Microsoft PowerShell shortcut, and then 
click "Run as administrator." You can also right-click the Microsoft PowerShell program file, 
Powershell.exe, to start Microsoft PowerShell as an administrator

To open Microsoft PowerShell as an administrator from the new Windows Start page, search for 
the Microsoft PowerShell shortcut by typing any part of the name Microsoft PowerShell until the 
shortcut is displayed on the Start page. Right-click the shortcut, and then click Advanced. 
On the Advanced menu, click Run as administrator.  


2) Install the Microsoft PowerShell Web Access web application and SSL certificate

You can run the cmdlet Install-PswaWebApplication to install the web application or
install manually. For manual installation, please refer to the help link at the bottom of 
this file.   

Microsoft PowerShell Web Access uses the HTTPS protocol, and a Secure Sockets Layer (SSL) 
certificate is necessary. If you do not have a certificate, but want to set up a test 
environment, the cmdlet can install the web application with a test SSL certificate. 
To configure your environment with a test certificate, add the -UseTestCertificate 
parameter. Do not use the test certificate option for production environments. 

Install-PswaWebApplication allows you to specify the website and web application name. 
The default values for -WebSiteName and -WebApplicationName are "Default Web Site" and 
"pswa" respectively.  
       
Additional Web Server (IIS) security can be applied to the web application, such as client 
certificate authorization, and denial-of-service attack prevention. See the help link at 
the bottom of this file for more advanced setup options.


3)  Authorize users to open Microsoft PowerShell Web Access connections

By default, no users are authorized to open Microsoft PowerShell Web Access connections. 
Authorization is granted, and authorization rules created, by running the 
Add-PswaAuthorizationRule cmdlet.

We recommend that you consider which users in your organization need access to computers 
from outside your corporate network, and configure authorization rules accordingly.  

You can use "*" to grant access to all users, computers, or session configurations.

Example:  Add-PswaAuthorizationRule * * *

In the preceding example, any user would be authorized to connect to any session 
configuration on any computer.  Local access rules that are set on destination computers 
would still apply, and might prevent users from establishing connections.  For more 
information about how to configure authorization rules and additional authorization 
rule options, type Get-Help Add-PswaAuthorizationRule.   

Other cmdlets for managing authorization rules are Get-PswaAuthorizationRule, 
Remove-PswaAuthorizationRule, and Test-PswaAuthorizationRule.


4)  Start using Microsoft PowerShell Web Access

The default web address is https://<yourservername>/pswa.


For more help and information about Microsoft PowerShell Web Access, see the following 
website: http://go.microsoft.com/fwlink/?LinkID=221050
