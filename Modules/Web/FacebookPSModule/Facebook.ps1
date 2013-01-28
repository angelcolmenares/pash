<#
Facebook PowerShell Module
http://facebookpsmodule.codeplex.com
Jon Newman (c) 2011

Facebook and PowerShell -- two great things which go great together!

Facebook PowerShell Module is a PowerShell module for managing Facebook,
including user data, friends lists, events etc. It depends on the Facebook C# SDK
which is currently (2011.09.12) in version 5.2.1.

At this writing (2011.09.12), Facebook PowerShell Module is in Alpha state.
This is not yet a solid tool for managing your mission-critical IT processes!

The concept behind Facebook PowerShell Module is simple:
Facebook is not just a curiosity for individual use anymore!
For many organizations, Facebook is a key tool for reaching out to customers.
IT organizations need tools to manage Facebook alongside their other
customer-facing services such as email and websites, and to integrate
them all together.

The scenarios covered by this Technology Preview are centered around the needs
of my lead customer. Not all object types or operations
are supported at this time.


Here are some example commands:

# Sets up your initial connection to Facebook. Your access token will be
# cached in file "$env:LOCALAPPDATA\FacebookPowerShellModule_CachedToken.txt".
New-FBConnection

# Read the list of your friends
Get-FBFriend

# Read additional data about your friends
Get-FBFriend | Get-FBObjectData

# Read even more data about your friends
Get-FBFriend | Get-FBObjectData -fields id,name,religion

# Count how many of your friends are male vs. female
Get-FBFriend -fields id,name,gender | group gender

# Save your friends list to a file (schedule this into Task Scheduler!)
$filename = "c:\temp\friends.$(get-date -format yyyy-MM-dd-HH-mm-ss).csv"
$friends = Get-FBFriend
$userdata = $friends | Get-FBObjectData
$userdata | Export-Csv $filename

# Read your events
Get-FBEvents

# Create an event
New-FBEvent -name "Barbeque" -StartTime "8/1/2011 12:00" -EndTime "8/1/2011 16:00" -Location "Our Place"

# Add a new message to your feed (set your status)
New-FBFeed -Message "I'm eating pasta! Isn't that exciting!"

# Add a Facebook photo to an album
$wallphotos = Get-FBAlbum | where {$_.Name -eq "Wall Photos"}
Add-FBPhoto -AlbumId $wallphotos.Id -Name "test name" -Message "test message" -Path "C:\Users\MyUserName\Pictures\FolderName\DSCF0018.JPG"

# Add a new message to your feed including a picture link
# Use -PictureId to add pictures from your albums per https://developers.facebook.com/blog/post/526
# Use -PicturePath to add pictures which are already out on the web
# Note that there may be limits on adding photos from other people's albums
#  per http://forum.developers.facebook.net/viewtopic.php?pid=238529
New-FBFeed -Message "Smile!" -PictureId 125342585543647
New-FBFeed -Message "Smile!" -PicturePath "http://jonnewman.com/jonnthmb2.jpg"

See file FacebookExamples.ps1 for more advanced examples.


Here are some notes on using Facebook PowerShell Module:

Questions about the lists of available fields can be answered at FaceBook's
developer site http://developers.facebook.com/docs/reference/api.

In particular, some limitations are imposed by the Facebook Graph API.
I currently know of these specific limitations, but there are probably more:
(1) You cannot post status with a live link to a Fan Page.
(2) There are limits on posting links to photos stored on FBCDN per
    http://forum.developers.facebook.net/viewtopic.php?pid=238529.
(3) Some objects cannot be deleted unless they were created by
    your AppId.
These are things you can do as a regular Facebook user but not as a
Facebook application. It would theoretically be possible to bypass the Graph API
and retrieve such information with HTML "screen-scraping",
but this would be liable to break at any time with even small changes
to the way Facebook presents its data, and might possibly be a violation
of Facebook's Terms Of Service.

Using the patterns below, it should be easy to extend this system to cover
additional object types.

Show-FBConnectionDialog only works with PowerShell ISE, or on regular PowerShell
if you load PresentationFramework and run -STA.

You can't send Facebook messages using the API:
http://stackoverflow.com/questions/2943297/how-send-message-facebook-friend-through-graph-api-using-accessstoken
However, you can use Show-FBMessageDialog to send bulk messages, but this
is not fully automatable.

It would be possible to implement New-FBAlbum, but at this writing (6/3/2012)
there does not appear to be an API for Remove-FBAlbum.

Future work items may include:

Keep track of access token lifetime

Support for limit, since, until query modifiers

Delete support

Request Dialog support: example URI is
http://www.facebook.com/dialog/apprequests?app_id=179873125388138&redirect_uri=http://jonnewman.com/response/&display=page&show_error=true&message=MyMessage&to=100002395463043

Support adding to a page feed

Query support e.g. "Get-FBFriend Joe" using FQL e.g.
$(Get-FBConnection).Query("SELECT $fieldlist FROM friend WHERE aid=""$friendName""")

Coverage for additional Facebook object types

Specify output formatting

Improved error handling -- part of the problem is that the exceptions
issued by Facebook C# SDK are pretty undifferentiated. Random Q: should
OAuth Authentication Error derive from Access Denied?

Automated tests

Updates to Facebook C# SDK as they become available

Consistent translation from DateTime strings

Give Get-FBObjectData an optional field for object type

There are some opportunities to use "splatting" to simplify code a bit


Status notes:

There are a number of places where I use unnecessary intermediate variables.
These help me debug, they can eventually be removed.

I need to figure out how to deal with paging.
Right now long lists will be truncated.

It doesn't look possible to enumerate page likes:
http://stackoverflow.com/questions/7823911/pick-a-user-who-liked-my-page-facebook

Links:
http://blog.prabir.me/post.aspx?id=57cae4fa-d812-4f68-9f2f-09fc49c3ce96


Enjoy, tell your friends, and please send feedback using Codeplex!
I'm especially interested in collecting scenarios of how you intend
to use Facebook PowerShell Module.

-- Jon Newman jonn_msft@hotmail.com
#>

# Directory where this script is located
# $Script:ScriptDir = Split-Path $MyInvocation.MyCommand.Path

Write-Debug 'FacebookPSModule loading: Cached Connection $null'
# Need to be script scope so that caching works. Note that this is not exported from module.
[Facebook.FacebookClient]$script:FB_FacebookCachedConnection = $null

# We request by default a very generous set of permissions.
Write-Debug "FacebookPSModule loading: Setting FB_DefaultExtendedPermissions"
[string[]]$FB_DefaultExtendedPermissions = @(
            "user_about_me",
            "friends_about_me",
            "user_events",
            "friends_events",
            "user_groups",
            "friends_groups",
            "user_interests",
            "friends_interests",
            "user_relationships",
            "friends_relationships",
            "user_religion_politics",
            "friends_religion_politics",
            "read_friendlists",
            "manage_friendlists",
            "manage_pages",
            "email",
            "publish_stream",
            "offline_access",
            "create_event",
            "create_note",
            "photo_upload",
            "publish_stream",
            "read_stream",
            #"rsvp_update",
            "share_item",
            "status_update",
            "user_photos",
            "user_birthday",
            "user_education_history",
            "user_likes",
            "user_checkins"
            )

# This application ID is set up specifically for use by
# Facebook PowerShell Module.
Write-Debug 'FacebookPSModule loading: Setting $FB_DefaultAppId and $FB_DefaultRedirectUri'
[string]$FB_DefaultAppId = "179873125388138" # from developers.facebook.com
# This is the AppSecret for the default FacebookPSModule app,
# used by Get-FBExtendedAccessToken.
# I do not believe that there is significant danger in exposing it,
# since this is not a "real" app.
# Please be careful with the AppSecrets for any applications you define yourself.
[string]$FB_DefaultAppSecret = "ece3cd41aa518cfaa2f33eab89f69e2d"
[string]$FB_DefaultRedirectUri = "http://jonnewman.com/"

Write-Debug "FacebookPSModule loading: Setting FB_DefaultConnectionFile"
[string]$FB_DefaultConnectionFile = "$env:LOCALAPPDATA\FacebookPowerShellModule_CachedToken.txt"

# http://thepowershellguy.com/blogs/posh/archive/2007/02/21/scripting-games-2007-advanced-powershell-event-7.aspx
<# 
 .Synopsis
  Save a Facebook connection data as a file.

 .Description
  Save a Facebook connection data as a file. The file is encrypted to limit access
  to the current user via ConvertFrom-SecureString.
  Cached Facebook connections (access tokens) become invalid if the user account
  changes its password. If this happens, use New-FBConnection to load
  a fresh connection.
  This method does not require that there be an existing connection.

 .Parameter Connection
  Facebook connection data as defined by Facebook C# SDK (facebooksdk.codeplex.com).

 .Parameter FileName
  Path to file where the connection data should be saved, default is
  $FB_DefaultConnectionFile.

 .Parameter NoEncrypt
  If set, do not encrypt file. Recommended practice is to encrypt the
  connection files, because the Facebook access token grants far-reaching
  permission over your accou to anyone who obtains it.

 .Example
   # Write default connection data to default location
   Get-FBConnection | Write-FBConnection

 .Example
   # Write the connection data for the specified user to a file for that user
   $ConnectionArray[$UserId] | Write-FBConnection BasePath\$UserId
   
 .Link
  Read-FBConnection
  New-FBConnection
  Get-FBConnection
#>
function Write-FBConnection
{
    Param(
        [parameter(Position=0)][ValidateNotNullOrEmpty()][string]$FileName = $FB_DefaultConnectionFile,
        $Connection,
        [switch]$NoEncrypt
        )
    
    Write-Verbose "Write-FBConnection: Writing connection to file $FileName"
    
    if (-not $Connection)
    {
        $v = Get-Variable FB_FacebookCachedConnectio[n]
        if ($v -ne $null)
        {
            if ($v.Value)
            {
                Write-Debug "Write-FBConnection: Connection not specified, using cached connection"
                $Connection = $FB_FacebookCachedConnection
            }
        }
    }
    if (-not $Connection)
    {
        throw "Connection not specified and no connection is cached"
    }
    
    if (-not (Get-Member -InputObject $Connection -Name PageId))
    {
        Write-Verbose "Write-FBConnection: You are writing a connection which may not be a long-lived page connection, and may therefore expire."
    }

    $xmlContent = ConvertTo-Xml $Connection -As String
    Write-Debug "Write-FBConnection: Writing connection to file $FileName, contents:"
    Write-Debug $xmlContent
    if (-not $NoEncrypt)
    {
        $xmlContent = ConvertTo-SecureString $xmlContent -AsPlainText -Force | ConvertFrom-SecureString
    }
    Set-Content -Path $FileName -Value $xmlContent
}

function NewConnection
{
    param($AccessToken, $AppId, $RedirectUri, $PageId)
    Write-Debug "Read-FBConnection: New connection: AccessToken $AccessToken AppId $AppId RedirectUri $RedirectUri PageId $PageId"
    $connection = New-Object Facebook.FacebookClient -ArgumentList $AccessToken
    if ($AppId)
    {
        Write-Debug "Read-FBConnection: read AppId $AppId"
        $connection.AppId = $AppId
    }
    if ($RedirectUri)
    {
        Write-Debug "Read-FBConnection: read RedirectUri $redirectUri"
        $null = $connection | Add-Member -MemberType NoteProperty -Name RedirectUri -Value $RedirectUri
    }
    if ($PageId)
    {
        Write-Debug "Read-FBConnection: read PageId $pageId"
        $null = $connection | Add-Member -MemberType NoteProperty -Name PageId -Value $PageId
    }
    $connection
}

<# 
 .Synopsis
  Read a Facebook connection data from a file.

 .Description
  Read a Facebook connection data from a file. The file is decrypted using
  the current user's credentials via ConvertTo-SecureString.
  Unless -NoCache is specified, it will also make the connection
  the new default connection for this session and future sessions.
  This method does not require that there be an existing connection.

 .Parameter FileName
  Path to file where the connection data is located, default is
  $FB_DefaultConnectionFile.

 .Parameter NoCache
  If true, skip caching the Facebook connection data
  in PowerShell session state. This connection will need to
  be specified manually to subsequent cmdlet invocations.

 .Example
   # Read connection data from default location
   Read-FBConnection

 .Example
   # Read the connection data for the specified user
   $ConnectionArray[$UserId] = Read-FBConnection BasePath\$UserId -NoCache

 .Link
  Write-FBConnection
  New-FBConnection
  Get-FBConnection

 .Notes
  Read-FBConnection will read either encrypted or unencrypted connection files.
#>
function Read-FBConnection
{
    Param(
        [string][parameter(Position = 0)][ValidateNotNullOrEmpty()]$FileName = $FB_DefaultConnectionFile,
        [switch]$NoCache
        )
    
    Write-Verbose "Read-FBConnection: Reading access token from file $FileName, NoCache $NoCache"
    
    if (-not (Test-Path $FileName))
    {
        throw "Specified file does not exist: $FileName"
    }

    $fileContent = get-content $FileName -ErrorAction Stop
    $fileContentDecrypted = $null
    if (!$fileContent)
    {
        throw "Specified file is empty or could not be read"
    }
    try
    {
        $secureString = ConvertTo-SecureString -String $fileContent -ErrorAction SilentlyContinue
        $fileContentDecrypted = $secureString | %{[Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($_))}
    }
    catch
    {
    }
    if ($fileContentDecrypted)
    {
        $fileContent = $fileContentDecrypted
    }
    else
    {
        Write-Verbose "$FileName could not be decrypted, treating it as an unencrypted file"
    }

    [xml]$xmlContent = $null
    try
    {
        $xmlContent = [xml]$fileContent
    }
    catch [System.Management.Automation.PSInvalidCastException]
    {
        # Fallback to raw access token.
        # //!! TODO This code can be removed once back-compat with
        # Alpha 0.5.3 and earlier is no longer an issue. In those releases,
        # the file was just the encrypted AccessToken and not XML.
        $accessToken = $fileContent
        if (-not $accessToken)
        {
            throw "Read-FBConnection: Invalid token file: $FileName"
        }
        Write-Debug "Read-FBConnection: Read access token $accessToken from file $FileName"
        NewConnection -AccessToken $accessToken
        return
    }
    $accessToken = $null
    $appId = $null
    $redirectUri = $FB_DefaultRedirectUri
    $pageId = $null
    foreach ($property in $xmlContent.Objects.Object.Property)
    {
        switch ($property.Name)
        {
            "AccessToken"
            {
                $accessToken = $property.psobject.Properties["#text"].Value
                break
            }
            "AppId"
            {
                $appIdProperty = $property.psobject.Properties["#text"]
                if ($appIdProperty)
                {
                    $appId = $property.psobject.Properties["#text"].Value
                }
                break
            }
            "RedirectUri"
            {
                $redirectUri = $property.psobject.Properties["#text"].Value
                break
            }
            "PageId"
            {
                $pageId = $property.psobject.Properties["#text"].Value
                break
            }
            # ignore UseFacebookBeta and other values
        }
    }
    if (-not $accessToken)
    {
        Write-Error "Read-FBConnection: Invalid token file (no AccessToken): $FileName"
        return
    }
    Write-Debug "Read-FBConnection: read AccessToken $accessToken"
    $connection = NewConnection -AccessToken $accessToken -AppId $appid -RedirectUri $redirectUri -PageId $pageId
    if (-not $NoCache)
    {
        Write-Debug "Read-FBConnection: Caching access token $accessToken appid $AppId redirectUri $RedirectUri"
        if ($FileName -ne $FB_DefaultConnectionFile)
        {
            Write-FBConnection -Connection $connection
        }
        $script:FB_FacebookCachedConnection = $connection
    }
    $connection
}

<# 
 .Synopsis
  Clears the cached Facebook connection.

 .Description
  Clears the cached Facebook connection, including both the connection
  cached in the session, and the default connection cached in the user profile.
  Note that this does not revoke any existing access token in Facebook
  proper; the next New-FBConnection will probably complete silently
  unless the new permissions list contains additional items.
  This method does not require that there be an existing connection.

 .Parameter Force
  If true, proceed with operation without prompting for confirmation.

 .Example
   # Clear connection data from default location
   Clear-FBConnection

 .Example
   # Read the connection data for the specified cache file
   Clear-FBConnection $BasePath\$UserId
   
 .Link
  New-FBConnection
  Get-FBConnection
  Read-FBConnection
  Write-FBConnection
#>
function Clear-FBConnection
{
    [CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact="Low")]
    Param(
        [switch]$Force
        )
    
    Write-Debug "Clear-FBConnection: entering"

    if (-not $Force)
    {
        if (-not $pscmdlet.ShouldProcess($FB_DefaultConnectionFile, "Clear FaceBook cached token file"))
        {
            Write-Verbose "Clear-FBConnection: User did not confirm operation"
            return
        }
    }

    <#    
    # http://blog.prabir.me/post/Facebook-CSharp-SDK-Logout.aspx
    # Somehow the logout portion doesn't work yet, you're still logged on afterward
    # See http://bugs.developers.facebook.net/show_bug.cgi?id=17217

    if ($host.Runspace.ApartmentState -ne 'STA')
    {
        Write-Debug "Clear-FBConnection may only be run in PowerShell ISE, or PowerShell.exe -STA."
        throw "Clear-FBConnection may only be run in PowerShell ISE, or PowerShell.exe -STA."
    }

    $window = New-Object System.Windows.Forms.Form
    $window.Width = "500"
    $window.Height = "500"
    $browser = New-Object System.Windows.Forms.WebBrowser
    $browser.Dock = "Fill"
    $window.Controls.Add($browser)

    $connection = Get-FBConnection
    $currentToken = $connection.AccessToken

    $parameters = New-Object 'system.collections.generic.dictionary[string,object]'
    $parameters["access_token"] = $currentToken
    $redirectUriProperty = $connection.psobject.Properties["RedirectUri"]
    $redirectUri = $FB_DefaultRedirectUri
    if ($redirectUriProperty)
    {
        $redirectUri = $connection.RedirectUri
    }
    $parameters["next"] = $redirectUri

    $facebookClient = New-Object -TypeName Facebook.FacebookClient
    $logoutUrl = $facebookClient.GetLogoutUrl($parameters)

    $browser.Navigate($logoutUrl)
    $browser.add_Navigated({
        Write-Debug "Clear-FBConnection: Navigate event: Uri $($_.Url)"
        if ($_.Url -eq $redirectUri)
        {
            Write-Debug "Clear-FBConnection: Navigate event: closing window"
            $window.Close()
        }
        else
        {
            Write-Debug "Clear-FBConnection: Navigate event: other Url $($_.Url)"
            # error handling TBD
        }
    })

    Write-Debug "Clear-FBConnection: Entering GUI to clear access token"
    [System.Windows.Forms.Application]::Run($window)
    Write-Debug "Clear-FBConnection: Completed GUI to clear access token"
    #>
    
    $script:FB_FacebookCachedConnection = $null

    Write-Verbose "Clear-FBConnection: Clearing access token from file $FB_DefaultConnectionFile"
    if (Test-Path $FB_DefaultConnectionFile)
    {
        Write-Debug "Clear-FBConnection: Deleting file $FB_DefaultConnectionFile"
        Remove-Item $FB_DefaultConnectionFile
        Write-Debug "Clear-FBConnection: Deleted file $FB_DefaultConnectionFile"
    }
    
    Write-Debug "Clear-FBConnection: exiting"
}

# 2011.06.27 I had to move this from WPF to WinForms due to this issue:
# http://facebooksdk.codeplex.com/discussions/261528
# This also fixed the issue where ISE crashed on exit after running this command.
<# 
 .Synopsis
  Get a Facebook access token for the currently-logged-in Facebook user.

 .Description
  Get a Facebook access token for the currently-logged-in Facebook user.
  This method will report an error if it is not run in STA mode,
  which means either PowerShell ISE or PowerShell.exe -STA.
  This method returns an access token string if successful.
  This method does not require that there be an existing connection.

 .Parameter AppId
  Facebook application identifier, default is $FB_DefaultAppId.
  The default value is a pre-defined application ID.
  In some cases, information about the application will be exposed to the
  Facebook user, so you can define your own application with description
  etc. which is consistent with your purpose.

 .Parameter RedirectUri
  Facebook redirect URI, default is $FB_DefaultRedirectUri.
  The default value is a pre-defined value appropriate to $FB_DefaultAppId.
  If you redefine AppId, you will probably want to specify a RedirectUri
  appropriate to your application as defined in Facebook.
  Otherwise, you may encounter errors when Facebook refuses to redirect
  operations to a URI which does not belong to your application.

 .Parameter ExtendedPermissions
  Minimum Facebook permissions for this connection. The operations
  which can be performed using this connection will be limited to the
  permissions requested. Default permissions are
  $FB_DefaultExtendedPermissions which grant very broad access.

 .Parameter Logoff
  If true, log the current user off of Facebook before connecting.
  Use this option if you are automatically connecting to the wrong account.

 .Parameter LogoffConnection
  Use this connection to log the current Facebook user off of Facebook.
  If the currently cached token is not the token for the current Facebook user,
  you must specify a token with permission to log off the current Facebook user.

 .Example
   # Get a Facebook access token for the currently-logged-in Facebook user.
   Show-FBConnectionDialog

 .Example
   # Get a Facebook access token for the currently-logged-in Facebook user,
   # and make this the default Facebook access token.
   $accessToken = Show-FBConnectionDialog
   if ($accessToken)
   {
       $connection = New-FBConnection -AccessToken $accessToken
       Write-FBConnection $accessToken
   }

 .Link
  New-FBConnection
  Get-FBExtendedAccessToken

 .Notes
  Note that if you are currently logged into Facebook,
  Show-FBConnectionDialog will log you onto that account automatically.
  If that account has already granted all the requested permissions to the app,
  the dialog will close automatically and not give you any chance to change
  the account. If you want an access token for an account other than
  the logged-in Facebook account, you need to first log into that account
  in Facebook.
  
  See https://developers.facebook.com/docs/authentication/client-side/.
#>
function Show-FBConnectionDialog
{
    Param(
        [string]$AppId = $FB_DefaultAppId,
        [string]$RedirectUri = $FB_DefaultRedirectUri,
        [string[]]$ExtendedPermissions = $FB_DefaultExtendedPermissions,
        [switch]$Logoff,
        $LogoffConnection
        )
    
    Write-Debug "Show-FBConnectionDialog: entering"
    
    if ($host.Runspace.ApartmentState -ne 'STA')
    {
        Write-Debug "Show-FBConnectionDialog may only be run in PowerShell ISE, or PowerShell.exe -STA."
        throw "Show-FBConnectionDialog may only be run in PowerShell ISE, or PowerShell.exe -STA."
    }

    Write-Debug "Show-FBConnectionDialog: Preparing GUI to retrieve access token"
    Write-Debug "Show-FBConnectionDialog: AppId $AppId RedirectUri $RedirectUri ExtendedPermissions $ExtendedPermissions"
    
    Write-Debug "Show-FBConnectionDialog: Loading System.Windows.Forms v4.0"
    #$null = [System.Reflection.Assembly]::Load("System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
    
    $window = New-Object System.Windows.Forms.Form
    $window.Width = "500"
    $window.Height = "500"
    $browser = New-Object System.Windows.Forms.WebBrowser
    $browser.Dock = "Fill"
    $window.Controls.Add($browser)

    $parameters = New-Object 'system.collections.generic.dictionary[string,object]'
    $parameters["display"] = "popup"
    $parameters["response_type"] = "token"
    $parameters["client_id"] = $AppId
    $parameters["scope"] = $ExtendedPermissions -join ","
    $parameters["redirect_uri"] = $RedirectUri
    $facebookClient = New-Object -TypeName Facebook.FacebookClient
    $loginUrl = $facebookClient.GetLoginUrl($parameters)
    Write-Debug "Show-FBConnectionDialog: LoginUrl is $($loginUrl.AbsoluteUri)"

    # http://blog.prabir.me/post/Facebook-CSharp-SDK-Logout.aspx
    if ($Logoff)
    {
        if (-not $LogoffConnection)
        {
            $LogoffConnection = Get-FBConnection
        }
        if (-not $LogoffConnection)
        {
            throw "Show-FBConnectionDialog -Logoff: You cannot force logoff without specifying -LogoffConnection or having a current connection."
        }
        
        $parameters2 = New-Object 'system.collections.generic.dictionary[string,object]'
        $parameters2["access_token"] = $LogoffConnection.AccessToken

        $logoffRedirectUri = $RedirectUri
        $redirectUriProperty = $LogoffConnection.psobject.Properties["RedirectUri"]
        if ($redirectUriProperty)
        {
            $logoffRedirectUri = $LogoffConnection.RedirectUri
        }
        $parameters2["next"] = $logoffRedirectUri
        
        $logoutUrl = $facebookClient.GetLogoutUrl($parameters2)
        Write-Debug "Show-FBConnectionDialog: LogoutUrl is $($logoutUrl.AbsoluteUri)"
    }

    # We use script scope due to a variable scoping change in PowerShell V3
    $script:accessToken = $null
    $script:logoffFailure = $false
    $script:doLogoff = $Logoff

    $startUrl = if ($script:doLogoff) {$logoutUrl} else {$loginUrl}
    Write-Debug "Show-FBConnectionDialog: Navigating to $($startUrl.AbsoluteUri)"
    $browser.Navigate($startUrl)
    $browser.add_Navigated({
        $target = $_.Url
        Write-Debug "Show-FBConnectionDialog: Navigate event: Uri $target"
        if ($script:doLogoff)
        {
            if ($target -eq $redirectUri)
            {
                Write-Debug "Show-FBConnectionDialog: Logoff complete, navigating to $($loginUrl.AbsoluteUri)"
                $script:doLogoff = $false
                $browser.Navigate($loginUrl)
            }
            elseif ($target -eq "https://www.facebook.com/")
            {
                Write-Debug "Show-FBConnectionDialog: Logoff failed on navigation to $target"
                $script:logoffFailure = $true
                $window.Close()
            }
        }
        else
        {
            $oauthResult = [Facebook.FacebookOAuthResult]$null;
            if ($facebookClient.TryParseOAuthCallbackUrl($target, [ref]$oauthResult))
            {
                $script:accessToken = $oauthResult.AccessToken;
                Write-Debug "Show-FBConnectionDialog: Navigate event: access token is $script:accessToken, closing window"
                $window.Close()
            }
            else
            {
                Write-Debug "Show-FBConnectionDialog: Navigate event: could not interpret Uri $target"
                # This is an expected case and can be ignored
            }
        }
    })

    Write-Debug "Show-FBConnectionDialog: Entering GUI to retrieve access token"
    [System.Windows.Forms.Application]::Run($window)
    Write-Debug "Show-FBConnectionDialog: Completed GUI to retrieve access token"

    if ($script:accessToken)
    {
        Write-Debug "Show-FBConnectionDialog: access token is $script:accessToken"
        $script:accessToken
    }
    elseif ($script:logoffFailure)
    {
        Write-Error "New-FBConnection/Show-FBConnectionDialog: Logoff failed, probably because the current token is not the token for the logged-on user"
    }
    else
    {
        Write-Error "New-FBConnection/Show-FBConnectionDialog: User declined to permit access"
    }

    Write-Debug "Show-FBConnectionDialog: exiting"
}

<# 
 .Synopsis
  Get and cache Facebook connection data for the currently-logged-in Facebook user.

 .Description
  Get and cache Facebook connection data for the currently-logged-in Facebook user.
  This function may display a dialog unless you are logged onto Facebook as this user
  and have already granted all required permissions to the application.
  If this function displays a dialog, it will require manual intervention
  to complete, and will not succeed when run in batch mode.
  This method will report an error if it is not run in STA mode,
  which means either PowerShell ISE or PowerShell.exe -STA.
  (-STA mode is not required if -AccessToken is specified.)
  This method returns a connection if successful.
  Unless -NoCache is specified, it will also make the connection
  the new default connection for this session and future sessions.
  Cached Facebook connections (access tokens) become invalid if the user account
  changes its password. If this happens, use New-FBConnection to load
  a fresh connection.
  This method does not require that there be an existing connection.

 .Parameter AccessToken
  If you already have a Facebook Graph API access token, specify it here.
  This function will not display a dialog if AccessToken is specified,
  so it will run in batch mode and without being in STA mode.
  
 .Parameter Connection
  If you already have a user connection, but want to retrieve
  a page connection, specify the Connection and PageId parameters.
  This function will not display a dialog if Connection is specified,
  so it will run in batch mode and without being in STA mode.
  
 .Parameter PageId
  In order to fully manage Facebook Pages, you will need a connection
  specifically for the page, rather than for a user account. If specified,
  you will retrieve a connection specific to the a page for which
  the user is an administrator. If used with -AccessToken,
  the specified access token is assumed to be a user account
  which manages the requested page.

 .Parameter ExtendedPermissions
  Minimum Facebook permissions for this connection. The operations
  which can be performed using this connection will be limited to the
  permissions requested. Default permissions are
  $FB_DefaultExtendedPermissions which grant very broad access.

 .Parameter AppId
  Facebook application identifier, default is $FB_DefaultAppId.
  The default value is a pre-defined application ID.
  In some cases, information about the application will be exposed to the
  Facebook user, so you can define your own application with description
  etc. which is consistent with your purpose.

 .Parameter RedirectUri
  Facebook redirect URI, default is $FB_DefaultRedirectUri.
  The default value is a pre-defined value appropriate to $FB_DefaultAppId.
  If you redefine AppId, you will probably want to specify a RedirectUri
  appropriate to your application as defined in Facebook.
  Otherwise, you may encounter errors when Facebook refuses to redirect
  operations to a URI which does not belong to your application.

 .Parameter AppSecret
  Facebook application secret, default is $FB_DefaultAppSecret.
  The default value is a pre-defined application secret.
  This parameter is used to extend the default token lifetime,
  and is only needed if you specify ExtendToken.

 .Parameter ExtendToken
  If true, extend the Facebook token lifetime.
  This may result in a token which will last longer than a few hours.

 .Parameter NoCache
  If true, skip caching the Facebook connection data
  in PowerShell session state.

 .Parameter Logoff
  If true, log the current Facebook user off of Facebook before connecting.
  Use this option if you are automatically connecting to the wrong account.

 .Parameter LogoffConnection
  Use this connection to log the current Facebook user off of Facebook.
  If the currently cached token is not the token for the current Facebook user,
  you must specify a token with permission to log off the current Facebook user.

 .Example
   # Get and cache connection data for the currently-logged-in Facebook user.
   New-FBConnection

 .Example
   # Get and cache connection data for the currently-logged-in Facebook user.
   # Extend connection lifetime from 2 hours to 60 days.
   New-FBConnection -ExtendToken

 .Example
   # Get and cache connection data for a page which is managed
   # by the currently-logged-in Facebook user.
   New-FBConnection -PageId $pageId

 .Example
   # Get but do not cache connection data for a page which is managed
   # by the account specified by $connection.
   # Extended-lifetime page access tokens should last indefinitely.
   New-FBConnection -Connection $connection -PageId $pageId -NoCache -ExtendToken

 .Example
   # Get and cache a Facebook connection based on an access token
   # which was obtained by other means. This command does not display
   # a dialog, so it can be run in batch mode and outside -STA mode.
   New-FBConnection -AccessToken $accessTokenString

 .Example
   # Get a Facebook connection based on an access token
   # which was obtained by other means. Do not cache this connection.
   # Use the connection to obtain information about a specific object.
   $connection = New-FBConnection -AccessToken $accessTokenString -NoCache
   Get-FBObjectData -Id $id -Connection $connection
   
 .Link
  Show-FBConnectionDialog
  Get-FBConnection
  Read-FBConnection
  Write-FBConnection
  Get-FBExtendedAccessToken
 
 .Notes
  Note that if you are currently logged into Facebook,
  New-FBConnection (without the -AccessToken parameter)
  will log you onto that account automatically.
  If that account has already granted all the requested permissions to the app,
  the dialog will close automatically and not give you any chance to change
  the account. If you want an access token for an account other than
  the logged-in Facebook account, you need to first log into that account
  in Facebook.
#>
function New-FBConnection
{
    [CmdletBinding(DefaultParameterSetName="ShowConnectionDialog")]
    Param(
        [string][parameter(Mandatory=$true, Position=0, ParameterSetName="AccessToken")]$AccessToken,
        [parameter(Mandatory=$true, Position=0, ParameterSetName="Connection")]$Connection,
        [string]$PageId,
        [string[]][parameter(ParameterSetName="ShowConnectionDialog")]$ExtendedPermissions,
        [string]$AppId = $FB_DefaultAppId,
        [string]$RedirectUri = $FB_DefaultRedirectUri,
        [string]$AppSecret = $FB_DefaultAppSecret,
        [switch][parameter(ParameterSetName="ShowConnectionDialog")]$Logoff,
        [parameter(ParameterSetName="ShowConnectionDialog")]$LogoffConnection,
        [switch][parameter(ParameterSetName="ShowConnectionDialog")]$ExtendToken,
        [switch]$NoCache
        )

    Write-Debug "New-FBConnection: entering"
    
    if ($PSCmdlet.ParameterSetName -eq "ShowConnectionDialog") # support strict mode
    {
        if ($LogoffConnection -and (-not $Logoff))
        {
            throw "New-FBConnection -LogoffConnection may only be specified if -Logoff is also specified."
        }
        if (-not $ExtendedPermissions)
        {
            Write-Debug "New-FBConnection: ExtendedPermissions empty, using default permissions"
            $ExtendedPermissions = $FB_DefaultExtendedPermissions
        }
        Write-Debug "New-FBConnection: ExtendedPermissions count $($ExtendedPermissions.Count)"
    }
    else
    {
        Write-Debug "New-FBConnection: ExtendedPermissions not specified, using default permissions"
        $ExtendedPermissions = $FB_DefaultExtendedPermissions
    }
    
    if ($PSCmdlet.ParameterSetName -eq "ShowConnectionDialog") # support strict mode
    {
        # This test will happen again in Show-FBConnectionDialog, but
        # repeat it here so that the error message mentions New-FBConnection.
        if ($host.Runspace.ApartmentState -ne 'STA')
        {
            Write-Debug "New-FBConnection (except with -AccessToken) may only be run in PowerShell ISE, or PowerShell.exe -STA."
            throw "New-FBConnection (except with -AccessToken) may only be run in PowerShell ISE, or PowerShell.exe -STA."
        }
        
        $Params = @{
            AppId = $AppId;
            RedirectUri = $RedirectUri;
            ExtendedPermissions = $ExtendedPermissions;
            Logoff = $Logoff;
            LogoffConnection = $LogoffConnection
        }
        $AccessToken = Show-FBConnectionDialog @params
        if ($ExtendToken)
        {
            $AccessToken = Get-FBExtendedAccessToken -AccessToken $AccessToken -AppId $AppId -AppSecret $AppSecret
        }
    }
    elseif ($PSCmdlet.ParameterSetName -eq "Connection") # support strict mode
    {
        if (-not $PageId)
        {
            throw "New-FBConnection -Connection may only be specified if -PageId is also specified."
        }
        $AccessToken = $Connection.AccessToken
    }

    if ($AccessToken)
    {
        Write-Debug "New-FBConnection: access token is $accessToken"
        if ($PageId)
        {
            Write-Debug "New-FBConnection: retrieving page access token for $PageId"
            $userConnection = New-FBConnection -AccessToken $AccessToken -AppId $AppId -RedirectUri $RedirectUri -NoCache
            $pages = Get-FBPage -Connection $userConnection
            $page = $pages | Where-Object {$_.id -eq $PageId}
            if ($page)
            {
                $AccessToken = $page.access_token
                Write-Debug "New-FBConnection: retrieved page access token $AccessToken for $PageId"
            }
            else
            {
                throw "Specified account does not manage page $PageId"
            }
            $connection = NewConnection -AccessToken $AccessToken -AppId $AppId -RedirectUri $RedirectUri -PageId $PageId
        }
        else
        {
            $connection = NewConnection -AccessToken $AccessToken -AppId $AppId -RedirectUri $RedirectUri
        }
        if (-not $NoCache)
        {
            Write-Debug "New-FBConnection: Caching access token $accessToken appid $AppId redirectUri $RedirectUri"
            Write-FBConnection -Connection $connection
            $script:FB_FacebookCachedConnection = $connection
        }
        $connection
    }

    Write-Debug "New-FBConnection: exiting"
}


# Access token must be available, either already cached or stored in file
<# 
 .Synopsis
  Get Facebook connection data.

 .Description
  Get Facebook connection data, cached in the PowerShell session
  or in the user's Windows profile. If there is no cached data,
  Get-FBConnection will fail; the user should first call New-FBConnection.
   
 .Link
  New-FBConnection
  Read-FBConnection
  Write-FBConnection
#>
function Get-FBConnection
{
    $connection = TryGetFBConnection
    if ($connection)
    {
        return $connection
    }
    throw "Connect to Facebook using New-FBConnection before accessing Facebook data."
}

function TryGetFBConnection
{
    [OutputType([bool])]
    param() # required to use OutputType
    
    Write-Debug "Get-FBConnection: entering"

    $hasConnection = $false
    # The strange name pattern ensures that this check does not affect $error
    $v = Get-Variable FB_FacebookCachedConnectio[n]
    if ($v -ne $null)
    {
        if ($v.Value)
        {
            $hasConnection = $true
        }
    }

    if ($hasConnection)
    {
        Write-Debug "Get-FBConnection: client cached"
    }
    else
    {
        Write-Debug "Get-FBConnection: client not cached"

        if (-not (Test-Path $FB_DefaultConnectionFile))
        {
            return
        }

        $connection = Read-FBConnection -ErrorAction SilentlyContinue -NoCache
        if ($connection -eq $null)
        {
            return
        }
        else
        {
            $script:FB_FacebookCachedConnection = $connection
        }
    }
    
    $FB_FacebookCachedConnection

    Write-Debug "Get-FBConnection: Exiting"
}

<# 
 .Synopsis
  Tests whether a Facebook connection is working properly.

 .Description
  Tests whether a Facebook connection is working properly.
  Returns a boolean value, and a non-terminating error if false.
  Uses the default connection if no connection is specified.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Test the default connection
   Test-FBConnection

 .Example
   # Test the connection stored in $connfile
   $conn = Read-FBConnection -FileName $connfile
   Test-FBConnection -Connection $conn
#>
function Test-FBConnection
{
    [OutputType([bool])]
    Param(
        [parameter(Position=0)]$Connection
        )

    Write-Debug "Test-FBConnection: entering"

    if (-not $Connection)
    {
        $Connection = TryGetFBConnection
    }
    if (-not $Connection)
    {
        Write-Error "Connect to Facebook using New-FBConnection before accessing Facebook data."
        Write-Debug 'Get-FBConnection: no connection, returning $false'
        return $false
    }
    $obj = Get-FBObjectData -Connection $Connection
    if (-not $obj)
    {
        Write-Error "Failed to connect to Facebook using the provided connection."
        Write-Debug 'Get-FBConnection: failed to connect, returning $false'
        return $false
    }
    Write-Debug 'Get-FBConnection: returning $true'
    return $true
}

<# 
 .Synopsis
  Retrieves a long-lived access token starting with
  a valid existing (possibly short-lived) Facebook access token.

 .Description
  With the removal of offline_access per
  https://developers.facebook.com/roadmap/offline-access-removal/,
  access tokens will no longer last indefinitely.
  This method retrieves a connection with a longer-lived access token,
  but does not cache the connection.
  Note that the method will sometimes fail and return $null,
  especially if called at high frequency.
  This will not work if the token has already expired.
  At this writing (2012.08.21), Facebook documentation indicates that
  "long-lived" user account tokens will last 60 days,
  but that corresponding page tokens will last indefinitely.

 .Parameter AccessToken
  Facebook access token. You can use (Get-FBConnection).AccessToken.

 .Parameter AppId
  Facebook application identifier, default is $FB_DefaultAppId.
  The default value is a pre-defined application ID.
  In some cases, information about the application will be exposed to the
  Facebook user, so you can define your own application with description
  etc. which is consistent with your purpose.

 .Parameter AppSecret
  Facebook application secret, default is $FB_DefaultAppSecret.
  The default value is a pre-defined value appropriate to $FB_DefaultAppId.
  If you redefine AppId, you will probably want to specify an AppSecret
  appropriate to your application as defined in Facebook.

 .Example
   # Extends the default connection
   $conn = Get-FBExtendedAccessToken -AccessToken (Get-FBConnection).AccessToken
   if ($conn)
   {
       Write-FBConnection -Connection $conn
   }

 .Example
   # Reads the connection stored in $connfile, and replaces it with an longer-lived connection.
   $conn = Read-FBConnection -FileName $connfile
   $newToken = Get-FBExtendedAccessToken -AccessToken $conn.AccessToken
   if ($newToken)
   {
       $conn.AccessToken = $newToken
       Write-FBConnection -Connection $conn -FileName $connfile
   }
   
 .Link
   New-FBConnection
   Show-FBConnectionDialog

 .Notes
   Unfortunately the method for extending token lifetime requires use of the AppSecret.
#>
function Get-FBExtendedAccessToken
{
    Param(
        [string][parameter(Position=0,Mandatory=$true)]$AccessToken,
        [string]$AppId = $FB_DefaultAppId,
        [string]$AppSecret = $FB_DefaultAppSecret
        )

    Write-Debug "Get-FBExtendedAccessToken: entering"

    # http://stackoverflow.com/questions/11068538/how-to-renew-facebook-access-token-using-its-c-sharp-sdk
    $facebookClient = New-Object -TypeName Facebook.FacebookClient -ArgumentList $AccessToken
    $facebookClient.AppId = $AppId
    $facebookClient.AppSecret = $AppSecret
    $result = $facebookClient.Get("oauth/access_token?client_id=$AppId&client_secret=$AppSecret&grant_type=fb_exchange_token&fb_exchange_token=$AccessToken")
    $result["access_token"]
}


<# 
 .Synopsis
  Get raw Facebook data using Facebook Graph API query string.

 .Description
  Get raw Facebook data using Facebook Graph API query string.
  This is a low-level base function, most users should use
  specific functions such as Get-FBFriends instead.

 .Parameter Query
  Facebook Graph API query string.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.
#>
function Get-FBRawData
{
    Param(
        [parameter(Mandatory=$true, Position = 0)][string]$Query,
        $Connection
        )
    
    Write-Debug "Get-FBRawData: query $Query"

    try
    {
        if (-not $Connection)
        {
            $Connection = Get-FBConnection
        }
        $result = $Connection.Get($Query)
    }
    catch [System.Management.Automation.MethodInvocationException]
    {
        $e = $_.Exception.InnerException
        if ($null -eq $e)
        {
            $e = $_
        }
        $useErrorId = "RawData"
        $useCategory = "ReadError"
        if ($e -is [Facebook.FacebookOAuthException])
        {
            $useErrorId = "OAuth"
            $useCategory = "PermissionDenied"
            # No point in checking ErrorCode since they all seem to have error code 190
        }
        elseif ($e -is [Facebook.FacebookApiLimitException])
        {
            $useErrorId = "ApiLimit"
        }
        elseif ($e -is [Facebook.FacebookApiException])
        {
            $useErrorId = "Api"
        }
        Write-Error -Exception $e -ErrorId $useErrorId -Category $useCategory
        return
    }

    # for GetFBFriend, $result[0].Value.GetType() is JSONArray
    # for GetFBUserInfo, $result.GetType() is JSONObject
    $result
}

<#
Facebook C# SDK returns JSON objects which use the
.NET 4.0 dynamic object capability. PowerShell 2.0 does not recognize
these dynamic objects, so that the only way to access their properties
is by using the index operator ("['name']"). This does not work with
PowerShell capabilities such as ValueFromPipelineByPropertyName.
So, we convert the JSON object tree into a PSObject tree,
using recursive descent.
#>
<# 
 .Synopsis
  Converts raw Facebook data to PowerShell format.

 .Description
  Converts raw Facebook data to PowerShell format.
  This is a low-level base function,
  used as part of the implementation of functions such as Get-FBFriends.
#>
function Convert-FBJSON
{
    Param(
        [object][Parameter(Position = 0)]$Raw,
        [string][Parameter(Position = 1)]$Header = ""
    )
    
    if ($Raw -eq $null)
    {
        Write-Debug "Convert-FBJSON$Header     null"
        return
    }
    if ($Raw -is [Facebook.JSONObject])
    {
        Write-Debug "Convert-FBJSON$Header     JSONObject"
        $hash = @{}
        foreach ($keyValuePair in $Raw)
        {
            $key = $keyValuePair.Key
            $value = $keyValuePair.Value
            Write-Debug "Convert-FBJSON$Header     key $key"
            if ("created_time","updated_time","start_time" -contains $key)
            {
                Write-Debug "Convert-FBJSON$Header     time value"
                $convertedValue = [DateTime]$value
            }
            else
            {
                $convertedValue = Convert-FBJSON $value ($Header+"  ")
            }
            $hash[$key] = $convertedValue
        }
        $retval = Convert-FBJSONHash $hash $Header
        if ($null -ne $retval)
        {
            $retval.PSTypeNames.Insert(0,"Facebook.Object")
        }
        $retval
        return
    }
    if ($Raw -is [Facebook.JSONArray])
    {
        Write-Debug "Convert-FBJSON$Header     JSONArray"
        $array = @()
        foreach ($element in $Raw)
        {
            $convertedElement = Convert-FBJSON $element ($Header+"  ")
            $convertedElement
        }
        return
    }
    if ($Raw -is [System.Collections.Generic.KeyValuePair[string,object]])
    {
        $key = $Raw.Key
        $value = $Raw.Value
        Write-Debug "Convert-FBJSON$Header     KeyValuePair $key $value"
        $retval = Convert-FBJSON $value ($Header+"  ")
        $retval
        return
    }
    if ($raw -is [object[]])
    {
        Write-Debug "Convert-FBJSON$Header     Array Count $($Raw.Count)"
        Write-Debug "Convert-FBJSON$Header     First object type $($Raw[0].GetType().FullName)"
        $hash = @{}
        foreach ($keyValuePair in $Raw)
        {
            $key = $keyValuePair.Key
            $value = $keyValuePair.Value
            Write-Debug "Convert-FBJSON$Header     key $key"
            $convertedValue = Convert-FBJSON $value ($Header+"  ")
            $hash[$key] = $convertedValue
        }
        $retval = Convert-FBJSONHash $hash $Header
        $retval.PSTypeNames.Insert(0,"Facebook.Object")
        $retval
        return
    }
    Write-Debug "Convert-FBJSON$Header     $($Raw.GetType().FullName) $Raw"
    $Raw
}

<# 
 .Synopsis
  Converts raw Facebook data to PowerShell format.

 .Description
  Converts raw Facebook data to PowerShell format.
  This is a low-level base function,
  used as part of the implementation of functions such as Get-FBFriends.
#>
function Convert-FBJSONHash
{
    Param(
        [object][Parameter(Position = 0)]$Hash,
        [string][Parameter(Position = 1)]$Header = ""
    )
    
    if ($Hash.ContainsKey("data"))
    {
        Write-Debug "Convert-FBJSONHash$Header trimming array to data value only"
        if ($Hash.ContainsKey("paging"))
        {
            <#
            Ugh, what to do about this? Instead of getting all the data,
            we got some data and a database cursor. For now, we'll just
            present what we have.
            #>
            Write-Debug "Convert-FBJSONHash$Header encountered data/paging pair"
            if ($Hash.Count -gt 2)
            {
                Write-Error "Convert-FBJSONHash$Header data/paging plus unexpected fields"
            }
        }
        <# could have both Data and Count
        else
        {
            if ($Hash.Count -gt 1)
            {
                Write-Error "Convert-FBJSONHash$Header data plus unexpected fields"
            }
        }
        #>
        foreach ($obj in $Hash["data"])
        {
            $retval = Convert-FBJSON $obj $Header
            $retval
        }
        return
    }
    Write-Debug "Convert-FBJSONHash$Header hash not containing data value"
    $retval = New-Object PSObject -Property $Hash
    $retval
}

function MarkWithAdditionalId
{
    Param(
        [object][Parameter(ValueFromPipeline=$true)]$Raw,
        [string][Parameter(Mandatory=$true)]$TypeName,
        [string][Parameter(Mandatory=$true)]$Id
        )
    Process
    {
        Add-Member -InputObject $Raw -MemberType noteProperty -Name ($TypeName + "Id") -Value $Id -PassThru
    }
}

<#
This logic adds a typename and type-specific ID. The typename enables
future scenarios around type-specific formatting. The type-specific ID
enables pipelining.
#>
function MarkWithType
{
    Param(
        [object][Parameter(ValueFromPipeline=$true)]$Raw,
        [string][Parameter(Position=0)]$TypeName
        )
    Process
    {
        if ($Raw)
        {
            if ($TypeName)
            {
                $Raw.PSTypeNames.Insert(0,"Facebook.$TypeName")
                Add-Member -InputObject $Raw -MemberType noteProperty -Name ($TypeName + "Id") -Value $Raw.id -Passthru
            }
            else
            {
                $Raw
            }
        }
    }
}


<# 
 .Synopsis
  Retrieves Facebook data about specific objects.

 .Description
  Retrieves Facebook data about specific objects.
  Note that Get-FBObjectData does not know the type of the object,
  so it will not return some type-specific fields usedful in pipelining.

 .Parameter Id
  Facebook ID for specified object(s), default is "me" which is
  the object referenced by the current Facebook connection.

 .Parameter TypeName
  Type of the object in question, for example "User" or "Photo".
  Use this optional parameter to produce an object instance which
  works well with pipelines and type-specific behavior.

 .Parameter Fields
  Fields to retrieve for the specified object(s).
  Note that these are case-sensitive.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Read the current user's birthday.
   Get-FBObjectData -Fields name,id,birthday
   
 .Link
  New-FBConnection
#>
function Get-FBObjectData
{
    Param(
        [string][parameter(ValueFromPipelineByPropertyName=$true, Position = 0)]$Id = "me",
        [string[]]$Fields,
        [string]$TypeName,
        $Connection
        )
    Process
    {
        Write-Debug "Get-FBObjectData id $Id"
        
        $query = $Id
        if ($Fields -ne $null)
        {
            $query = $query + "?fields=$($Fields -join "","")"
        }
        
        try
        {
            $raw = Get-FBRawData -Query $query -Connection $Connection
        }
        catch [MethodException]
        {
            $f = $_.InnerException
            if ($null -eq $f)
            {
                $f = $_
            }
            Write-Error -Exception $f -ErrorId RawData -Category NotSpecified
        }
        
        Convert-FBJSON $raw | MarkWithType $TypeName
    }
}

<# 
 .Synopsis
  Retrieves Facebook data about the list of objects
  which are related to another object.

 .Description
  Retrieves Facebook data about the list of objects
  which are related to another object.
  This is a base function; in general it is better to use a function
  specific to the relationship, e.g. "Get-FBFriend".

 .Parameter Type
  Facebook name for the relationship, e.g. "Friends" or "Feed".

 .Parameter TypeName
  PowerShell name for the relationship, which will be encoded into
  a type-specific ID property (e.g. UserId)
  and into PSTypeNames for the returned objects.

 .Parameter Id
  Facebook ID for specified object(s), default is "me" which is
  the object referenced by the current Facebook connection.

 .Parameter Fields
  Fields to retrieve for the list of objects.
  Note that these are case-sensitive.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Read the friends of the current user.
   Get-FBAssociation -Type Friends -TypeName Friends
#>
function Get-FBAssociation
{
    Param(
        [string][parameter(Mandatory=$true)]$Type,
        [string]$TypeName,
        [string][parameter(ValueFromPipelineByPropertyName=$true)]$Id = "me",
        [string[]]$Fields,
        $Connection
        )
    process
    {
        Write-Debug "Get-FBAssociation: type $Type id $Id"

        $query = "$Id/$Type"
        if ($Fields -ne $null)
        {
            $query = $query + "?fields=$($Fields -join "","")"
            $query = $query + "&limit=1000" # TODO Limit mechanism
        }
        else
        {
            $query = $query + "?limit=1000" # TODO Limit mechanism
        }
        $raw = Get-FBRawData -Query $query -Connection $Connection
        Convert-FBJSON $raw | MarkWithType $TypeName
    }
}


<#
I'm a little conflicted about the noun choice here. On the one hand,
I know that nouns aren't generally supposed to be plural.
On the other hand, the Id isn't for the friend itself,
but for the user whose children are being retrieved.
If you just want a single friend, use Get-FBObjectData.
#>

<#
Note that many users do not grant their friends permission
to view their list of friends.
#>


<# 
 .Synopsis
  Lists all friends of the specified user.

 .Description
  Lists all friends of the specified user.

 .Parameter Id
  Facebook ID of the user whose friends should be listed, default "me".

 .Parameter Fields
  Fields which should be retrieved.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Show simple list of friends of user "Me"
   Get-FBFriend

 .Example
   # Count the genders of friends
   Get-FBFriend -Fields gender | group gender
#>
function Get-FBFriend
{
    Param(
        [string][parameter(ValueFromPipelineByPropertyName=$true,Position=0)]$Id = "me",
        [string[]]$Fields,
        $Connection
        )
    process
    {
        Write-Debug "Get-FBFriend: id $Id"

        $retval = Get-FBAssociation -Id $Id -Fields $Fields -Type "Friends" -TypeName "User" -Connection $Connection
        $retval
    }
}


<# 
 .Synopsis
  Lists all events of the specified user.

 .Description
  Lists all events of the specified user.
  This includes both events created by the specified user,
  and events to which the specified user is invited and has not
  specifically declined.

 .Parameter Id
  Facebook ID of the user whose events should be listed, default "me".

 .Parameter Fields
  Fields which should be retrieved.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Show simple list of events of user "Me"
   Get-FBEvent

 .Example
   # Get events with starting times more than a week from now
   Get-FBEvent -Fields name,id,start_time | where {$_.start_time -gt ((get-date) + (new-timespan -days 7))}
   
 .Link
  New-FBEvent
  New-FBEventInvite
#>
function Get-FBEvent
{
    Param(
        [string][parameter(ValueFromPipelineByPropertyName=$true,Position=0)]$Id = "me",
        [string[]]$Fields,
        $Connection
        )
    process
    {
        Write-Debug "Get-FBEvent: id $Id"

        $retval = Get-FBAssociation -Id $Id -Fields $Fields -Type "Events" -TypeName "Event" -Connection $Connection
        $retval
    }
}


<# 
 .Synopsis
  Lists the feed of the specified user.

 .Description
  Lists the feed of the specified user.
  This includes both feed entries (or "status") created by the specified user,
  and feed entries of friends and groups.

 .Parameter Id
  Facebook ID of the user/group/??? whose feed should be listed, default "me".

 .Parameter Fields
  Fields which should be retrieved.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Show feed of user "Me"
   Get-FBFeed

 .Example
   # Get links from feed
   get-fbfeed | where {$_.type -eq "link"}
   
 .Link
  New-FBFeed
#>
function Get-FBFeed
{
    Param(
        [string][parameter(ValueFromPipelineByPropertyName=$true,Position=0)]$Id = "me",
        [string[]]$Fields,
        $Connection
        )
    process
    {
        Write-Debug "Get-FBFeed: id $Id"

        $retval = Get-FBAssociation -Id $Id -Fields $Fields -Type "Feed" -TypeName "Feed" -Connection $Connection
        $retval
    }
}


<# 
 .Synopsis
  Lists the posts of the specified user.

 .Description
  Lists the posts of the specified user.

 .Parameter Id
  Facebook ID of the user/page/??? whose posts should be listed, default "me".

 .Parameter Fields
  Fields which should be retrieved.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Show posts of user "Me"
   Get-FBPost
#>
function Get-FBPost
{
    Param(
        [string][parameter(ValueFromPipelineByPropertyName=$true,Position=0)]$Id = "me",
        [string[]]$Fields,
        $Connection
        )
    process
    {
        Write-Debug "Get-FBPost: id $Id"

        $retval = Get-FBAssociation -Id $Id -Fields $Fields -Type "Posts" -TypeName "Post" -Connection $Connection
        $retval
    }
}

<# 
 .Synopsis
  Lists the groups to which the specified user belongs.

 .Description
  Lists the groups to which the specified user belongs.

 .Parameter UserId
  Facebook ID of the user whose groups should be listed, default "me".

 .Parameter Fields
  Fields which should be retrieved.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Show groups of user "Me"
   Get-FBGroup
#>
function Get-FBGroup
{
    Param(
        [string][parameter(ValueFromPipelineByPropertyName=$true,Position=0)]$UserId = "me",
        [string[]]$Fields,
        $Connection
        )
    process
    {
        Write-Verbose "Get-FBGroup: userid $UserId"

        $retval = Get-FBAssociation -Id $UserId -Fields $Fields -Type "Groups" -TypeName "Group" -Connection $Connection
        $retval
    }
}

<# 
 .Synopsis
  Lists the members of the specified group.

 .Description
  Lists the members of the specified group.

 .Parameter GroupId
  Facebook ID of the group whose members should be listed.

 .Parameter Fields
  Fields which should be retrieved.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Show groups of user "Me"
   Get-FBMember
   
 .Link
  Get-FBGroup
#>
function Get-FBMember
{
    Param(
        [string][parameter(Mandatory=$true,ValueFromPipelineByPropertyName=$true,Position=0)]$GroupId,
        [string[]]$Fields,
        $Connection
        )
    process
    {
        Write-Verbose "Get-FBMember: groupid $GroupId"

        $retval = Get-FBAssociation -Id $GroupId -Fields $Fields -Type "Members" -TypeName "User" -Connection $Connection
        $retval
    }
}


# Can other types than user have albums?
<# 
 .Synopsis
  Lists the albums of the specified user.

 .Description
  Lists the albums of the specified user.

 .Parameter Id
  Facebook ID of the user whose albums should be listed, default "me".

 .Parameter Fields
  Fields which should be retrieved.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Show albums of user "Me"
   Get-FBAlbum
   
 .Link
  Get-FBPhoto
#>
function Get-FBAlbum
{
    Param(
        [string][parameter(ValueFromPipelineByPropertyName=$true,Position=0)]$Id = "me",
        [string[]]$Fields,
        $Connection
        )
    process
    {
        Write-Verbose "Get-FBAlbum: id $Id"

        $retval = Get-FBAssociation -Id $Id -Fields $Fields -Type "Albums" -TypeName "Album" -Connection $Connection
        $retval
    }
}

<# 
 .Synopsis
  Lists the pages managed by the specified user.

 .Description
  Lists the pages managed by the specified user, that is, those for which
  this users is an administrator. In particular, this function will return
  field "access_token", which is an access token which can be used to perform
  operations specifically against a page.

 .Parameter Id
  Facebook ID of the user whose managed pages should be listed, default "me".
  It is unlikely that this information is available for users other than "me".

 .Parameter Fields
  Fields which should be retrieved.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Show pages managed by user "Me"
   Get-FBPage
#>
function Get-FBPage
{
    Param(
        [string][parameter(ValueFromPipelineByPropertyName=$true,Position=0)]$Id = "me",
        [string[]]$Fields,
        $Connection
        )
    process
    {
        Write-Verbose "Get-FBPage: id $Id"

        $retval = Get-FBAssociation -Id $Id -Fields $Fields -Type "accounts" -TypeName "Page" -Connection $Connection
        $retval
    }
}

#region FriendList
<# 
 .Synopsis
  Lists the friendlists for the specified user.

 .Description
  Lists the friendlists for the specified user.

 .Parameter Id
  Facebook ID of the user whose friendlists should be listed, default "me".
  It is unlikely that this information is available for users other than "me".

 .Parameter Fields
  Fields which should be retrieved.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Show friendlists for user "Me"
   Get-FBFriendList
   
 .Link
  New-FBFriendList
  Remove-FBFriendList
  Get-FBFriendListMember
  Add-FBFriendListMember
  Remove-FBFriendListMember
#>
function Get-FBFriendList
{
    Param(
        [string][parameter(ValueFromPipelineByPropertyName=$true,Position=0)]$Id = "me",
        [string[]]$Fields,
        $Connection
        )
    process
    {
        Write-Verbose "Get-FBFriendList: id $Id"

        $retval = Get-FBAssociation -Id $Id -Fields $Fields -Type "friendlists" -TypeName "FriendList" -Connection $Connection
        $retval
    }
}

<# 
 .Synopsis
  Create a new friendlist.

 .Description
  Create a new friendlist.

 .Parameter Name
  Name of the friendlist to be created.
  
 .Parameter Force
  Create friendlist without prompting regardless of confirmation settings.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Create friendlist named "BFF"
   New-FBFriendList -Name BFF
 
 .Example
   # Create a friendlist "NotBFF" which contains all friends not in "BFF"
   $BFF = Get-FBFriendList | Where-Object {$_.Name -eq "BFF"}
   $notBFF = New-FBFriendList -Name NotBFF
   $BFFMembers = $BFF | Get-FBFriendListMember
   $friends = Get-FBFriend
   Compare-Object -ReferenceObject $friends -DifferenceObject $BFFMembers | % {Add-FBFriendListMember -FriendListId $notBFF.FriendListId -UserId $_.InputObject.UserId}
   
 .Link
  Get-FBFriendList
  Remove-FBFriendList
  Get-FBFriendListMember
  Add-FBFriendListMember
  Remove-FBFriendListMember
#>
function New-FBFriendList
{
    [CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact="Low")]
    Param(
        [string][Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true,Position=0)]$Name,
        [switch]$Force,
        $Connection
        )
    process
    {
        Write-Debug "New-FBFriendList: name $Name force $Force"

        if (-not $Force)
        {
            if (-not $pscmdlet.ShouldProcess($name, "Add FaceBook FriendList"))
            {
                Write-Verbose "New-FBFriendList: User did not confirm operation"
                return
            }
        }
        
        if (-not $Connection)
        {
            $Connection = Get-FBConnection
        }
        if ($Connection)
        {
            $parameters = New-Object 'system.collections.generic.dictionary[string,object]'
            $parameters["name"] = $Name
            $parameters["access_token"] = $Connection.AccessToken
            $raw = $Connection.Post("me/friendlists", $parameters)
            if (-not $raw)
            {
                Write-Error -Message "New-FBFriendList: Failed to create friendlist"
            }
            else
            {
                Convert-FBJSON $raw | MarkWithType FriendList
            }
        }
    }
}

<# 
 .Synopsis
  Delete a friendlist.

 .Description
  Delete a friendlist.
  Returns a boolean value, and a non-terminating error if false.
  Deleting will probably fail unless your application created the friendlist.

 .Parameter FriendListId
  Id of the friendlist to be deleted.
  
 .Parameter Force
  Delete friendlist without prompting regardless of confirmation settings

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Delete friendlist "BFF"
   Get-FBFriendList | ? {$_.name -eq "BFF"} | Remove-FBFriendList

 .Link
  Get-FBFriendList
  New-FBFriendList
  Get-FBFriendListMember
  Add-FBFriendListMember
  Remove-FBFriendListMember
#>
function Remove-FBFriendList
{
    [CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact="Medium")]
    Param(
        [string][Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true,Position=0)]$FriendListId,
        [switch]$Force,
        $Connection
        )
    process
    {
        Write-Debug "Remove-FBFriendList: id $FriendListId force $Force"

        if (-not $Force)
        {
            if (-not $pscmdlet.ShouldProcess($FriendListId, "Delete FaceBook FriendList"))
            {
                Write-Verbose "Remove-FBFriendList: User did not confirm operation"
                return
            }
        }
        
        if (-not $Connection)
        {
            $Connection = Get-FBConnection
        }
        if ($Connection)
        {
            $parameters = New-Object 'system.collections.generic.dictionary[string,object]'
            $parameters["access_token"] = $Connection.AccessToken
            $raw = $Connection.Delete("/$FriendListId", $parameters)
            if (-not $raw)
            {
                Write-Error -Message "Remove-FBFriendList: Failed to delete friendlist"
            }
            $raw # boolean is expected
        }
    }
}

<# 
 .Synopsis
  Lists the members of the specified friendlist.

 .Description
  Lists the members of the specified friendlist.

 .Parameter FriendListId
  Friendlist whose members should be listed.

 .Parameter Fields
  Fields which should be retrieved.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # Show members of friendlist "BFF"
   Get-FBFriendList | ? {$_.name -eq "BFF"} | Get-FBFriendListMember

 .Link
  Get-FBFriendList
  New-FBFriendList
  Remove-FBFriendList
  Add-FBFriendListMember
  Remove-FBFriendListMember
#>
function Get-FBFriendListMember
{
    Param(
        [string][parameter(Mandatory=$true,ValueFromPipelineByPropertyName=$true,Position=0)]$FriendListId,
        [string[]]$Fields,
        $Connection
        )
    process
    {
        Write-Verbose "Get-FBFriendListMember: id $FriendListId"

        $retval = Get-FBAssociation -Id $FriendListId -Fields $Fields -Type "members" -TypeName "User" -Connection $Connection
        $retval
    }
}

<# 
 .Synopsis
  Add one or more friends to a friendlist.

 .Description
  Add one or more friends to a friendlist.

 .Parameter FriendListId
  Facebook ID of friendlist

 .Parameter UserId
  Facebook ID of friend to join to friendlist

 .Parameter Force
  Join friendlist without prompting regardless of confirmation settings

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.
   
 .Example
   # Join all friends to friendlist "BFF"
   $bff = Get-FBFriendList | ? {$_.name -eq "BFF"}
   Get-FBFriend | Add-FBFriendListMember -FriendListId $bff.FriendListId

 .Link
  Get-FBFriendList
  New-FBFriendList
  Remove-FBFriendList
  Get-FBFriendListMember
  Remove-FBFriendListMember
#>  
function Add-FBFriendListMember
{
    [CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact="Low")]
    Param(
        [string][ValidateNotNullOrEmpty()]$FriendListId,
        [string][Parameter(ValueFromPipelineByPropertyName=$true)]$UserId,
        [switch]$Force,
        $Connection
        )
    process
    {

        if (-not $Force)
        {
            if (-not $pscmdlet.ShouldProcess("Join member to FaceBook FriendList"))
            {
                Write-Verbose "Add-FBFriendListMember: User did not confirm operation"
                return
            }
        }
        
        if (-not $Connection)
        {
            $Connection = Get-FBConnection
        }
        if ($Connection)
        {
            $parameters = New-Object 'system.collections.generic.dictionary[string,object]'
            $parameters["access_token"] = $Connection.AccessToken
            $raw = $Connection.Post("/$FriendListId/members/$UserId", $parameters)
            if (-not $raw)
            {
                Write-Error -Message "Remove-FBFriendList: Failed to remove member from friendlist"
            }
        }
    }
}

<# 
 .Synopsis
  Remove one or more friends from a friendlist.

 .Description
  Remove one or more friends from a friendlist.

 .Parameter FriendListId
  Facebook ID of friendlist

 .Parameter UserId
  Facebook ID of friend to remove from friendlist

 .Parameter Force
  Exit friendlist without prompting regardless of confirmation settings

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.
   
 .Example
   # Remove all friends from friendlist "BFF"
   $bff = Get-FBFriendList | ? {$_.name -eq "BFF"}
   Get-FBFriendListMember -FriendListId $bff.FriendListId | Remove-FBFriendListMember -FriendListId $bff.FriendListId

 .Link
  Get-FBFriendList
  New-FBFriendList
  Remove-FBFriendList
  Get-FBFriendListMember
  Add-FBFriendListMember
#>  
function Remove-FBFriendListMember
{
    [CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact="Low")]
    Param(
        [string][ValidateNotNullOrEmpty()]$FriendListId,
        [string][Parameter(ValueFromPipelineByPropertyName=$true)]$UserId,
        [switch]$Force,
        $Connection
        )
    process
    {

        if (-not $Force)
        {
            if (-not $pscmdlet.ShouldProcess("Remove member from FaceBook FriendList"))
            {
                Write-Verbose "Remove-FBFriendListMember: User did not confirm operation"
                return
            }
        }
        
        if (-not $Connection)
        {
            $Connection = Get-FBConnection
        }
        if ($Connection)
        {
            $parameters = New-Object 'system.collections.generic.dictionary[string,object]'
            $parameters["access_token"] = $Connection.AccessToken
            $raw = $Connection.Delete("/$FriendListId/members/$UserId", $parameters)
            if (-not $raw)
            {
                Write-Error -Message "Remove-FBFriendList: Failed to remove member from friendlist"
            }
        }
    }
}
#endregion FriendList

<# 
 .Synopsis
  Lists the photos in the specified album.

 .Description
  Lists the photos in the specified album.

 .Parameter AlbumId
  Facebook ID of the album whose photos should be listed.
  
 .Parameter AllAlbums
  List all photos in all albums. Cannot be used with AlbumId.

 .Parameter Fields
  Fields which should be retrieved.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
   # List photos in album "Wall photos"
   Get-FBPhoto -AlbumId "Wall photos"
   
 .Link
  Get-FBAlbum
#>
function Get-FBPhoto
{
    Param(
        [switch][Parameter(Mandatory=$true,ParameterSetName="AllAlbums")]$AllAlbums = $false,
        [string][Parameter(ValueFromPipelineByPropertyName=$true,ParameterSetName="AllAlbums")]$UserId = "me",
        [string][Parameter(Mandatory=$true,Position=0,ValueFromPipelineByPropertyName=$true,ParameterSetName="AlbumId")]$AlbumId,
        [string[]]$Fields,
        $Connection
        )
    Process
    {

        if ($PSCmdlet.ParameterSetName -eq "AllAlbums")
        {
            Write-Verbose "Get-FBPhoto: AllAlbums $AllAlbums -UserId $UserId Fields $Fields"
            $albums = Get-FBAlbum -Id $UserId -Connection $Connection
            if ($albums)
            {
                $albums = @($albums)
                Write-Debug "Get-FBPhoto: album count $($albums.Count)"
                $i = 1
                $albums | % {
                    Write-Debug "Get-FBPhoto: listing photos in album id $($_.AlbumId)"
                    Write-Progress -Activity "Listing all photos" -PercentComplete ($i*100/$albums.Length) -Status "Listing photos in album $($_.name)" -Id 0
                    Get-FBPhoto -AlbumId $_.AlbumId -Fields $Fields -Connection $Connection
                    $i++
                }
                Write-Progress -Activity "Listing all photos" -Completed -Status "Listed all photos" -Id 0
            }
            else
            {
                Write-Debug "Get-FBPhoto: album count 0"
            }
            return
        }
        
        Write-Verbose "Get-FBPhoto: AlbumId $AlbumId Fields $Fields"

        $photos = Get-FBAssociation -Id "$AlbumId" -Fields $Fields -Type "Photos" -TypeName "Photo" -Connection $Connection
        if ($null -eq $photos)
        {
            $photos = @()
        }
        $photos = @($photos)
        Write-Debug "Get-FBPhoto: AlbumId $AlbumId photo count $($photos.Count)"
        $photos | MarkWithAdditionalId -TypeName Album -Id $AlbumId
        <#
        foreach ($photo in $photos)
        {
            Add-Member -InputObject $photo -MemberType noteProperty -Name ("AlbumId") -Value $AlbumId
        }
        $photos
        #>
    }
}

# Maintain a pool of unique paths
function NewPath
{
    Param(
        [string][Parameter(Mandatory=$true)]$InitialPath,
        [ref]$PathList
    )

    $i = 2
    $currentPath = $InitialPath
    while ($PathList.Value -contains $currentPath)
    {
        $currentPath = [System.IO.Path]::GetFileNameWithoutExtension($InitialPath) + " ($i)" + [System.IO.Path]::GetExtension($InitialPath)
        $i++
    }
    if ($i -gt 2)
    {
        Write-Debug "Read-FBBulkPhotos: NewPath: renaming $InitialPath to $currentPath"
    }
    $PathList.Value += $currentPath
    $currentPath
}

# TODO Improve behavior when multiple files share the same name
<# 
 .Synopsis
  Read all photos from one Facebook album or all of them, and save all photos
  to a directory.

 .Description
  Read all photos from one Facebook album or all of them, and save all photos
  to a directory. The names of the photo files will be the names in Facebook.
  If multiple files have the same name, only the last will appear.

 .Parameter AlbumId
  Facebook album identifier. May be pipelined from Get-FBAlbum
  or New-FBAlbum (once this is implemented).

 .Parameter AllAlbums
  If specified, all photos from all albums are saved,
  and each album gets its own subdirectory. May not be used with -AlbumId.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Parameter Path
  Path to directory where the photos should be saved, default is current directory.
   
 .Link
  Get-FBPhoto
  Get-FBAlbum
#>
function Read-FBBulkPhotos
{
    Param(
        [switch][Parameter(Mandatory=$true,ParameterSetName="AllAlbums")]$AllAlbums = $false,
        [string][Parameter(ValueFromPipelineByPropertyName=$true,ParameterSetName="AllAlbums")]$UserId = "me",
        [string][Parameter(Mandatory=$true,Position=0,ValueFromPipelineByPropertyName=$true,ParameterSetName="AlbumId")]$AlbumId,
        [string][Parameter(Position=1)]$Path = ".",
        $Connection
    )
    Begin
    {
        $webClient = New-Object System.Net.WebClient
    }
    Process
    {

        if ($PSCmdlet.ParameterSetName -eq "AllAlbums")
        {
            Write-Verbose "Read-FBBulkPhotos: AllAlbums $AllAlbums Path $Path -UserId $UserId"
            Write-Progress -Activity "Downloading all albums" -Status "Reading album list" -Id 0
            $albums = Get-FBAlbum -Id $UserId -Connection $Connection
            if ($albums)
            {
                $albums = @($albums)
                Write-Debug "Read-FBBulkPhotos: album count $($albums.Count)"
                $i = 1
                $albumList = @()
                $albums | % {
                    Write-Debug "Read-FBBulkPhotos: Downloading album id $($_.AlbumId) name $($_.name)"
                    $albumPath = Join-Path $Path (NewPath $_.name ([ref]$albumList))
                    Write-Progress -Activity "Downloading all albums" -PercentComplete ($i*100/$albums.Length) -Status "Downloading album $($_.name) to $albumPath" -Id 0
                    # BUGBUG Something strange happens here where the recursive call
                    # gets AllAlbums = true, worked around above
                    
                    ReadAlbumPhotos -AlbumId $_.AlbumId -Path $albumPath -Connection $Connection -ChildCall
                    $i++
                }
            Write-Progress -Activity "Downloading all albums" -Completed -Status "Downloaded all albums" -Id 0
            }
            else
            {
                Write-Debug "Read-FBBulkPhotos: album count 0"
            }
            return
        }
        
        ReadAlbumPhotos -AlbumId $AlbumId -Path $Path -Connection $Connection
    }
}

function ReadAlbumPhotos
{
    Param(
        [string]$AlbumId,
        [string]$Path,
        $Connection,
        [switch]$ChildCall
    )
    Write-Verbose "Read-FBBulkPhotos: AlbumId $AlbumId Path $Path"
    $albumData = Get-FBObjectData -Id $AlbumId -Fields name,photos -Connection $Connection
    $albumName = $albumData.name
    $photos = $albumData.photos
    if ($null -eq $photos)
    {
        $photos = @()
    }
    $photos = @($photos)
    Write-Debug "Read-FBBulkPhotos: albumName $albumName photo count $($photos.Count)"
    $progressParams = @{
        Activity = "Downloading album $albumName";
        Id = 1
    }
    if ($ChildCall)
    {
        $progressParams["ParentId"] = 0
    }
    
    if (-not (Test-Path -Path $path -PathType Container))
    {
        Write-Verbose "Read-FBBulkPhotos: attempting to create $path"
        # James Brundage: this is faster than Out-Null
        $null = New-Item -Path $path -ItemType Container
    }
    if (Test-Path $path -PathType Container)
    {
        # http://www.thomasmaurer.ch/2010/10/how-to-download-files-with-powershell/
        # This could be a separate primitive, although it isn't strictly
        # scoped to Facebook.
        $i = 1
        $photoList = @()
        foreach ($photo in $photos)
        {
            $source = $photo.source
            $extension = [System.IO.Path]::GetExtension($source.Split("/")[-1])
            $photoName = NewPath $photo.name ([ref]$photoList)
            $filename = Join-Path $path $photoName
            if (-not $filename.EndsWith($extension))
            {
                $filename = $filename + $extension
            }
            Write-Debug "Read-FBBulkPhotos: downloading $source to $filename"
            Write-Progress @progressParams -PercentComplete ($i*100/$photos.Length) -Status "Downloading photo $($photo.Name) to $filename"
            $webClient.DownloadFile($source,$filename)
            $i++
        }
        Write-Progress @progressParams -Completed -Status "Downloaded all photos"
    }
    else
    {
        Write-Verbose "Read-FBBulkPhotos: failed to create $path"
    }
}

<# 
 .Synopsis
  Add all photos in a directory to a Facebook album.

 .Description
  Add all photos in a directory to a Facebook album. Only files with extensions
  .jpg and .png are considered to be photos.

 .Parameter AlbumId
  Facebook album identifier. May be pipelined from Get-FBAlbum
  or New-FBAlbum (once this is implemented).

 .Parameter Path
  Path to directory where the photos are located, default is current directory.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.
   
 .Link
  Get-FBAlbum
#>
function Add-FBBulkPhotos
{
    Param(
        [string][Parameter(Mandatory=$true,Position=0,ValueFromPipelineByPropertyName=$true)]$AlbumId,
        [string][Parameter(Position=1)]$Path = ".",
        $Connection
    )
    Process
    {

        Write-Verbose "Add-FBBulkPhotos: AlbumId $AlbumId Path $Path"
        $photoFiles = @(gci -Path $Path\* -Include *.jpg,*.png)
        if (0 -ge $photoFiles.Count)
        {
            Write-Debug "Add-FBBulkPhotos: no photos found"
            return
        }
        $i = 1
        foreach ($photoFile in $photoFiles)
        {
            Write-Progress -Activity "Adding photos from $Path to album $AlbumId" -PercentComplete ($i*100/$photoFiles.Length) -Status "Uploading $($photoFile.Name)"
            $photoName = $photoFile.Name
            if ($photoFile.Extension -eq '.jpg')
            {
                $photoName = $photoName.Substring(0,$photoName.Length - 4)
            }
            $photo = New-FBPhoto -AlbumId $AlbumId -Name $photoName -Message "AutoUpload $($photoName)" -Path $photoFile.FullName -Connection $Connection
            $photo
            $i++
        }
        Write-Progress -Activity "Adding photos from $Path to album $AlbumId" -Completed -Status "Uploaded all photos"
    }
}

# also see https://developers.facebook.com/blog/post/560/
# Should EndTime be mandatory? Facebook doesn't require it but
#   it's a good practice which is often neglected.
<# 
 .Synopsis
  Create a new event.

 .Description
  Create a new event.

 .Parameter Name
  Name of the event to be created.

 .Parameter StartTime
  Time when the event starts
  
 .Parameter EndTime
  Time when the event ends
  
 .Parameter Message
  Event message
  
 .Parameter Location
  Event location
  
 .Parameter Link
  Event URL link
  
 .Parameter Picture
  Event thumbnail image
  
 .Parameter Caption
  Event picture caption
  
 .Parameter Description
  Event description
  
 .Parameter Privacy
  Event privacy settings
  
 .Parameter Force
  Create event without prompting regardless of confirmation settings

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.
   
 .Link
  Get-FBEvent
  New-FBEventInvite
#>
function New-FBEvent
{
    [CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact="Low")]
    Param(
        [string][Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true,Position=0)]$Name,
        [DateTime][Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true,Position=1)]$StartTime,
        [DateTime][Parameter(ValueFromPipelineByPropertyName=$true)]$EndTime,
        [string][Parameter(ValueFromPipelineByPropertyName=$true)]$Message,
        [string][Parameter(ValueFromPipelineByPropertyName=$true)]$Location,
        [string][Parameter(ValueFromPipelineByPropertyName=$true)]$Link,
        [string][Parameter(ValueFromPipelineByPropertyName=$true)]$Picture,
        [string][Parameter(ValueFromPipelineByPropertyName=$true)]$Caption,
        [string][Parameter(ValueFromPipelineByPropertyName=$true)]$Description,
        [string][Parameter(ValueFromPipelineByPropertyName=$true)]$Privacy,
        [switch]$Force,
        $Connection
        )
    process
    {
        Write-Debug "New-FBEvent: name $Name start_time $StartTime end_time $EndTime message $Message location $Location link $Link picture $Picture caption $Caption description $Description privacy $Privacy force $Force"

        if (-not $Force)
        {
            if (-not $pscmdlet.ShouldProcess($name, "Add FaceBook Event"))
            {
                Write-Verbose "New-FBEvent: User did not confirm operation"
                return
            }
        }
        $parameters = New-Object 'system.collections.generic.dictionary[string,object]'
        $parameters["name"] = $Name
        $parameters["start_time"] = $StartTime.ToString("s")
        if ($EndTime) { $parameters["end_time"] = $EndTime.ToString("s") }
        if ($message) { $parameters["message"] = $Message }
        if ($location) { $parameters["location"] = $Location }
        if ($link) { $parameters["link"] = $Link }
        if ($picture) { $parameters["picture"] = $Picture }
        if ($caption) { $parameters["caption"] = $Caption }
        if ($description) { $parameters["description"] = $Description }
        
        if ($Privacy)
        {
            $parameters["privacy_type"] = $Privacy
        }
        
        if (-not $Connection)
        {
            $Connection = Get-FBConnection
        }
        if ($Connection)
        {
            $parameters["access_token"] = $Connection.AccessToken
            $raw = $Connection.Post("me/events", $parameters)
            if (-not $raw)
            {
                Write-Error -Message "New-FBEvent: Failed to create event"
            }
            else
            {
                Convert-FBJSON $raw | MarkWithType Event
            }
        }
    }
}

<# 
 .Synopsis
  Invite users to an event.

 .Description
  Invite users to an event. This function does not return a value.

 .Parameter EventId
  Facebook event identifier. May be pipelined from Get-FBEvent.
  or New-FBEvent.

 .Parameter UserId
  Facebook user identifier. May be pipelined from Get-FBFriend.

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.

 .Example
  # Invite all friends to an event
  $e = Get-FBEvent
  Get-FBFriend | New-FBEventInvite -EventId $e.EventId
   
 .Link
  Get-FBEvent
  New-FBEvent
#>
function New-FBEventInvite
{
    Param(
        [string][Parameter(Mandatory=$true,Position=0,ValueFromPipelineByPropertyName=$true)]$EventId,
        [string][Parameter(Mandatory=$true,Position=1,ValueFromPipelineByPropertyName=$true)]$UserId,
        $Connection
    )
    Process
    {
        Write-Verbose "New-FBEventInvite: EventId $EventId UserId $UserId"
        $parameters = New-Object 'system.collections.generic.dictionary[string,object]'
        $parameters["users"] = $UserId
        if (-not $Connection)
        {
            $Connection = Get-FBConnection
        }
        if ($Connection)
        {
            $parameters["access_token"] = $Connection.AccessToken
            $raw = $Connection.Post("$EventId/invited", $parameters)
            if (-not $raw)
            {
                Write-Error -Message "New-FBEventInvite: Failed to invite user ""$UserId"" to event ""$EventId"""
            }
        }
    }
}

# 11.05.10 Thanks to scriptwarrior.codepress.com for the first version!
<# 
 .Synopsis
  Create a new feed entry for the current user.

 .Description
  Create a new feed entry for the current user.

 .Parameter Message
  Message to post to feed

 .Parameter PicturePath
  Path to picture file to attach to feed (for pictures not already on Facebook)
  
 .Parameter PictureId
  Facebook ID of picture file to attach to feed (for pictures already on Facebook)

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.
   
 .Link
  Get-FBFeed
#>  
function New-FBFeed
{
    [CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact="Low",DefaultParameterSetName = "Neither")]
    Param(
        [string][Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]$Message,
        [string][Parameter(ValueFromPipelineByPropertyName=$true,ParameterSetName = "PicturePath")]$PicturePath,
        [string][Parameter(ValueFromPipelineByPropertyName=$true,ParameterSetName = "PictureId")]$PictureId,
        [switch]$Force,
        $Connection
        )
    process
    {

        if (-not $Force)
        {
            if (-not $pscmdlet.ShouldProcess("Set FaceBook Status"))
            {
                Write-Verbose "New-FBFeed: User did not confirm operation"
                return
            }
        }

        $parameters = New-Object 'system.collections.generic.dictionary[string,object]'
        $parameters["message"] = $Message
        if (-not $Connection)
        {
            $Connection = Get-FBConnection
        }
        if ($Connection)
        {
            $parameters["access_token"] = $Connection.AccessToken
            switch ($PsCmdlet.ParameterSetName)
            {
                "PicturePath" {$parameters["picture"] = $PicturePath; break}
                "PictureId"   {$parameters["object_attachment"] = $PictureId; break}
            }
            $raw = $Connection.Post("me/feed", $parameters)
            if (-not $raw)
            {
                Write-Error -Message "New-FBFeed: Failed to create feed item"
            }
            else
            {
                Convert-FBJSON $raw | MarkWithType Feed
            }
        }
    }
}

# see http://facebooksdk.blogspot.com/2011/04/facebook-album.html
<# 
 .Synopsis
  Add a photo to an album.

 .Description
  Add a photo to an existing album.

 .Parameter AlbumId
  Facebook ID of album

 .Parameter Name
  Name to attach to photo

 .Parameter message
  Message to attach to photo

 .Parameter Path
  Path to picture file (e.g. "C:\path\file.jpg")

 .Parameter Connection
  Facebook C# SDK connection object. If not specified, the default connection
  is loaded via Get-FBConnection. Use this field to control
  which Facebook account should be used.
   
 .Link
  Get-FBPhoto
#>  
function New-FBPhoto
{
    [CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact="Low")]
    Param(
        [string][ValidateNotNullOrEmpty()]$AlbumId,
        [string][Parameter(ValueFromPipelineByPropertyName=$true)]$Name,
        [string][Parameter(ValueFromPipelineByPropertyName=$true)]$Message,
        [string][Parameter(ValueFromPipelineByPropertyName=$true,Mandatory=$true)]$Path,
        [switch]$Force,
        $Connection
        )
    process
    {

        if (-not $Force)
        {
            if (-not $pscmdlet.ShouldProcess("New FaceBook Photo"))
            {
                Write-Verbose "New-FBPhoto: User did not confirm operation"
                return
            }
        }
        
        $media = New-Object Facebook.FacebookMediaObject
        $media.FileName = $Path
        $media.ContentType = "image/jpeg"
        $fileBytes = [System.IO.File]::ReadAllBytes($Path)
        $null = $media.SetValue($fileBytes)

        $parameters = New-Object 'system.collections.generic.dictionary[string,object]'
        $parameters["name"] = $Name
        $parameters["message"] = $Message
        # 2011.07.18 It is important to use ".psobject.BaseObject",
        # otherwise you get a mysterious StackOverflowException
        # in the serializer
        $parameters["@file.jpg"] = $media.psobject.BaseObject
        if (-not $Connection)
        {
            $Connection = Get-FBConnection
        }
        $raw = $Connection.Post("/$AlbumId/photos", $parameters)
        $photo = Convert-FBJSON $raw | MarkWithType Photo | MarkWithAdditionalId -TypeName Album -Id $AlbumId
        $photo
    }
}
# 2011.09.12 IMO New is more correct than Add.
# Technically the photo already existed, but the Facebook photo object did not exist.

# Facebook Graph API does not appear to support deleting photos

<# 
 .Synopsis
  Display one or more Message Dialogs.

 .Description
  Display one or more Message Dialogs. Each Message Dialog can send a message
  to up to 50 users who are friends of the currently-logged-in Facebook account.
  You will see a series of dialog windows,
  each covering one batch of target users. For each dialog, you will need to
  enter a Message and click Send or Cancel or close the window,
  after which the next dialog will appear.
  No objects are returned. There is no feedback on who was or was not
  sent a message.
  This method does not require that there be an existing connection.

 .Parameter AppId
  Facebook application identifier, default is $FB_DefaultAppId.
  The default value is a pre-defined application ID.
  In some cases, information about the application will be exposed to the
  Facebook user, so you can define your own application with description
  etc. which is consistent with your purpose.
  
 .Parameter RedirectUri
  Facebook redirect URI, default is $FB_DefaultRedirectUri.
  The default value is a pre-defined value appropriate to $FB_DefaultAppId.
  If you redefine AppId, you will probably want to specify a RedirectUri
  appropriate to your application as defined in Facebook.
  Otherwise, you may encounter errors when Facebook refuses to redirect
  operations to a URI which does not belong to your application.

 .Parameter UserId
  Account(s) which should receive the message. If there are more than
  RecipientBatchSize targets, they will be batched into multiple dialogs.
  If there are no specified users, you will need to enter one or more
  target users in the dialog.

 .Parameter Link
  The link to send in the Message Dialog.
  Note that some Links are not permitted, in particular links under
  facebook.com. If this happens, the window may display something like
  "Sorry, something went wrong", and you will need to close the dialog
  to continue.

 .Parameter Description
  Optional description which is displayed to the recipients.
  Default text is extracted from the link.

 .Parameter Name
  Optional link name which is displayed to the recipients.
  Default text is extracted from the link.

 .Parameter Picture
  Optional picture which is displayed to the recipients.
  Default picture is extracted from the link. Note that the links
  to Facebook photos at FBCDN addresses may not be permitted.

 .Parameter FriendsOnly
  Filter out all UserIds who are not friends of the current account.
  Note that if any specified user is not a friend, the dialog will fail
  for one entire batch of targets. The call will fail if no
  specified users are friends.

 .Parameter RecipientBatchSize
  The maximum number of friends who can be chosen in one dialog.
  This defaults to 50, but users of some older browsers may need to choose 25.

 .Example
   # Send a message to all friends
   Get-Friends | Show-FBMessageDialog -Link http://www.example.com
   
 .Notes
  Note that if you are currently logged into Facebook,
  Show-FBMessageDialog will use that account automatically.
  If you want to send messages from an account other than
  the logged-in Facebook account, you need to first log into that account
  in Facebook.
#>
function Show-FBMessageDialog
{
    Param(
        [string]$AppId = $FB_DefaultAppId,
        [string]$RedirectUri = $FB_DefaultRedirectUri,
        [string][Parameter(ValueFromPipelineByPropertyName=$true)]$UserId,
        [string][Parameter(Mandatory=$true)]$Link, # API fails without this
        [string]$Description,
        [string]$Name,
        [string]$Picture,
        [switch]$FriendsOnly,
        [int]$RecipientBatchSize = 50
        )
    Begin
    {
        Write-Debug "Show-FBMessageDialog: entering"
        Write-Debug "Show-FBMessageDialog: appId $AppId redirectUri $RedirectUri Link $Link Description $Description Name $Name Picture $Picture FriendsOnly $FriendsOnly RecipientBatchSize $RecipientBatchSize"
    
        if ($host.Runspace.ApartmentState -ne 'STA')
        {
            Write-Debug "Show-FBMessageDialog may only be run in PowerShell ISE, or PowerShell.exe -STA."
            throw "Show-FBMessageDialog may only be run in PowerShell ISE, or PowerShell.exe -STA."
        }

        Write-Debug "Show-FBMessageDialog: Loading System.Windows.Forms v2.0"
        $null = [System.Reflection.Assembly]::Load("System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
    
        $UserIds = @()
        
        if ($FriendsOnly)
        {
            $FriendIds = @(Get-FBFriend -Fields id -ErrorAction Stop | select-object -ExpandProperty id)
            if (-not $FriendIds)
            {
                throw "Failed to retrieve friends list"
            }
        }
    }
    Process
    {
        if ($FriendsOnly)
        {
            if (-not ($FriendIds -contains $UserId))
            {
                return
            }
        }
        
        $UserIds += $UserId
        if ($UserIds.Count -ge $RecipientBatchSize)
        {
            ShowMessageDialog -AppId $AppId -RedirectUri $RedirectUri -UserIds $UserIds -Link $Link -Description $Description -Name $Name -Picture $Picture
            $UserIds = @()
        }
    }
    End
    {
        if ($FriendsOnly -and (-not $FriendIds))
        {
            throw "-FriendsOnly was specified and none of the users are friends"
        }
        
        ShowMessageDialog -AppId $AppId -RedirectUri $RedirectUri -UserIds $UserIds -Link $Link -Description $Description -Name $Name -Picture $Picture

        Write-Debug "Show-FBMessageDialog: exiting"
    }
}

function ShowMessageDialog
{
    Param(
        [string]$AppId,
        [string]$RedirectUri,
        [string[]]$UserIds,
        [string]$Link,
        [string]$Description,
        [string]$Name,
        [string]$Picture
        )
    
    Write-Debug "Show-FBMessageDialog: ShowMessageDialog: UserIds $UserIds"
    
    $userIdString = $userIds -join ','

    $escapedAppId = [System.Uri]::EscapeDataString($AppId)
    $escapedRedirectUri = [System.Uri]::EscapeDataString($RedirectUri)
    #$userIdString = [System.Uri]::EscapeDataString($userIdString)
    # $Link = [System.Uri]::EscapeDataString($Link) # do not escape per documentation
    # $Picture = [System.Uri]::EscapeDataString($Picture) guessing this isn't escaped either

    $uri = "http://www.facebook.com/dialog/send?app_id=$escapedAppId&link=$Link&redirect_uri=$escapedRedirectUri&display=popup"
    if ($userIdString)
    {
        $uri += "&to=$userIdString"
    }
    
    if ($Name)
    {
        $escapedName = [System.Uri]::EscapeDataString($Name)
        $uri += "&name=$escapedName"
    }
    if ($Description)
    {
        $escapedDescription = [System.Uri]::EscapeDataString($Description)
        $uri += "&description=$escapedDescription"
    }
    if ($Picture)
    {
        $uri += "&picture=$Picture"
    }
    # response_type only relevant to connect dialog
    Write-Debug "Show-FBMessageDialog: Initial Uri: $uri"

    $window = New-Object System.Windows.Forms.Form
    $window.Width = "700"
    $window.Height = "600"
    $browser = New-Object System.Windows.Forms.WebBrowser
    $browser.Dock = "Fill"
    $window.Controls.Add($browser)

    $browser.Navigate($uri)
    $front = $false
    $browser.add_Navigated({
        Write-Debug "Show-FBMessageDialog: Navigate event: Uri $($_.Url)"
        $oauthResult = [Facebook.FacebookOAuthResult]$null
        if (-not $front)
        {
            $window.BringToFront()
            $front = $true
        }
        if ($_.Url.AbsoluteUri.StartsWith($RedirectUri))
        {
            Write-Debug "Show-FBMessageDialog: Navigate event: closing window"
            $window.Close()
        }
        else
        {
            # This is an expected case and can be ignored
        }
    })
    
    Write-Debug "Show-FBMessageDialog: Entering GUI to send messages"
    [System.Windows.Forms.Application]::Run($window)
    Write-Debug "Show-FBMessageDialog: Completed GUI to send messages"

# need to return some value, but what?

    Write-Debug "Show-FBMessageDialog: ShowMessageDialog: exit"

}


# 2011.03.07 http://developers.facebook.com/docs/reference/api/user/
Write-Debug "FacebookPSModule loading: Setting FB_AllUserProperties"
[string[]]$FB_AllUserProperties = @(
    "id",
    "name",
    "first_name",
    "last_name",
    "gender",
    "locale",
    "link",
    "third_party_id",
    "timezone",
    "updated_time",
    "verified",
    "about",
    "bio",
    "birthday",
    "education",
    "email",
    "hometown",
    "interested_in",
    "location",
    "meeting_for",
    "political",
    "quotes",
    "relationship_status",
    "religion",
    "significant_other",
    "website",
    "work"
    )
    
Write-Debug "FacebookPSModule loading: Setting FB_AllUserConnections"
[string[]]$FB_AllUserConnections = @(
    "picture",
    "friends",
    "accounts",
    "apprequests",
    "activities",
    "albums",
    "books",
    "checkins",
    "events",
    "feed",
    "friendlists",
    "home",
    "inbox",
    "interests",
    "likes",
    "links",
    "movies",
    "music",
    "notes",
    "outbox",
    "photos",
    "posts",
    "statuses",
    "tagged",
    "television",
    "updates",
    "videos"
    )
