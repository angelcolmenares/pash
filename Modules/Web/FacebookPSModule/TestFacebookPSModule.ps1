$MainAppId = "179873125388138"
$MainRedirectUri = 'http://jonnewman.com/'
$AltAppId = '422525854448508'
$AltAppRedirectUri = 'http://jonnewman.com/'
$NoOfflineAccessAppId = '420973254607136'
$NoOfflineAccessRedirectUri = 'http://jonnewman.com/'
$PageId = '146403025457939'
$PageName = 'JonTest Corp'
$UserId = '100002097205662'
$UserName = 'JonTest NewmanTest'
$NoEncrypt = $false

if ($NoEncrypt)
{
    $v011usertoken = 'FBv011_user.opentoken'
    $v060pagetoken = 'FBv060_page.opentoken'
    $v060usertoken = 'FBv060_user.opentoken'
    $v063pagetoken = 'FBv063_page.opentoken'
    $v063usertoken = 'FBv063_user.opentoken'
    #$v063altappusertoken = 'FBv063_altapp_user.opentoken'
    $v063altappusertokenexpired = 'FBv063_altapp_user_expired.opentoken'
    #$v063noofflineaccessusertoken = 'FBv063_noofflineaccess_user.opentoken'
}
else
{
    $v011usertoken = 'FBv011_user.token'
    $v060pagetoken = 'FBv060_page.token'
    $v060usertoken = 'FBv060_user.token'
    $v063pagetoken = 'FBv063_page.token'
    $v063usertoken = 'FBv063_user.token'
    #$v063altappusertoken = 'FBv063_altapp_user.token'
    $v063altappusertokenexpired = 'FBv063_altapp_user_expired.token'
    #$v063noofflineaccessusertoken = 'FBv063_noofflineaccess_user.token'
}

foreach ($file in
    $v011usertoken,
    $v060pagetoken,
    $v060usertoken,
    #$v063altappusertoken,
    $v063altappusertokenexpired
    )
{
    if (-not (Test-Path $file))
    {
        throw "Missing file $file"
    }
}


function Assert
{
    Param(
        [bool][Parameter(Mandatory=$true)]$test,
        [string]$condition = "<condition>"
    )
    
    Write-Host "Assert: $condition"
    
    if (-not $test)
    {
        throw "Assert failed: $condition"
    }
}

function AssertNotNull
{
    Param(
        [parameter(Mandatory = $true)]$test,
        [string]$condition = "<condition>"
    )
    
    Write-Host "AssertNotNull: $condition"
    
    if ($null -eq $test)
    {
        throw "AssertNotNull failed: $condition"
    }
}

function AssertNull
{
    Param(
        $test,
        [string]$condition = "<condition>"
    )
    
    Write-Host "AssertNull: $condition"
    
    if ($null -ne $test)
    {
        throw "AssertNull failed: $condition"
    }
}

function AssertEqual
{
    Param(
        $test1,
        $test2,
        [string]$condition = "<condition>"
    )
    
    Write-Host "AssertEqual: $condition"
    
    if ($test1 -ne $test2)
    {
        throw "AssertEqual failed: $test1 $test2 $condition"
    }
}

function AssertNotEqual
{
    Param(
        $test1,
        $test2,
        [string]$condition = "<condition>"
    )
    
    Write-Host "AssertEqual: $condition"
    
    if ($test1 -eq $test2)
    {
        throw "AssertEqual failed: $test1 $test2 $condition"
    }
}

function AssertNotEmpty
{
    Param(
        [array]$test,
        [string]$condition = "<condition>"
    )
    
    Write-Host "AssertNotEmpty: $condition"
    
    if ($test.Count -eq 0)
    {
        throw "AssertNotEmpty failed: $condition"
    }
}

function AssertAssociation
{
    Param(
        [array]$test,
        [string]$type,
        [string]$condition = "<condition>"
    )
    
    Write-Host "AssertAssociation: $type $condition"
    
    if ($test.Count -eq 0)
    {
        throw "AssertAssociation failed empty: $condition"
    }

    foreach ($t in $test)
    {
        AssertEqual ($t | select-object -expandproperty "id") ($t | select-object -expandproperty "$($Type)Id") "Id: $($Type)Id"
        AssertEqual $t.PSTypeNames[0] "Facebook.$Type" "Type1: Facebook.$Type"
        AssertEqual $t.PSTypeNames[1] "Facebook.Object" "Type: Facebook.Object"
    }
}

$error.Clear()

Set-StrictMode -Version 3.0




Write-Output "Tests Begin"

$ErrorPreference = "Stop"
$WarningPreference = "Stop"
$VerbosePreference = "Continue"
#$DebugPreference = "Continue"
$DebugPreference = "SilentlyContinue"

$scriptDir = Split-Path $MyInvocation.InvocationName
Push-Location $scriptDir


Write-Output "Module tests"

$module = Import-Module Facebook -Passthru
AssertNotNull $module "Import-Module"

Remove-Module Facebook

$module = Get-Module Facebook
AssertEqual $module $null "Remove-Module"

$module = Import-Module Facebook -Passthru
AssertNotNull $module "Import-Module"


Write-Output "Connection tests"

Clear-FBConnection

AssertEqual $error.Count 0
$conn = $null
$err = $null
try
{
    $conn = Get-FBConnection
}
catch
{
    $err = $_
}
AssertNotNull $err "Errors from Get-FBConnection after Clear-FBConnection"
AssertNull $conn "Get-FBConnection after Clear-FBConnection"
$error.Clear()

$connection = New-FBConnection
AssertNotNull $connection "New-FBConnection"

$extendedToken = Get-FBExtendedAccessToken -AccessToken $connection.AccessToken
AssertNotNull $extendedToken "Get-FBExtendedAccessToken"

<# BUGBUG logoff not working
$connection = New-FBConnection -Logoff
AssertNotNull $connection "New-FBConnection -Logoff"
#>

$connection = New-FBConnection
AssertNotNull $connection "New-FBConnection"

$connection = Get-FBConnection
AssertNotNull $connection "Get-FBConnection"

$filename = [System.IO.Path]::GetTempFileName()
if (Test-Path $filename)
{
    Remove-Item $filename
}
Assert (-not (Test-Path $filename)) "no file"

Write-Output "NoEncrypt tests"
Write-FBConnection -Connection $connection -FileName $filename -NoEncrypt
Assert (Test-Path $filename) "Write-FBConnection NoEncrypt"
$connection = Read-FBConnection -FileName $filename
AssertNotNull $connection "Read-FBConnection after NoEncrypt"
AssertNotEqual $error.Count 0
$error.Clear()
Assert (Test-FBConnection $connection) 'Test-FBConnection NoEncrypt tempfile failed'
$obj = Get-FBObjectData -Connection $connection
AssertEqual $obj.Id $UserId
Remove-Item $filename
Assert (-not (Test-Path $filename)) "NoEncrypt no file"

Write-Output "Write-Connection tests"
Write-FBConnection -Connection $connection -FileName $filename
Assert (Test-Path $filename) "Write-FBConnection"
$connection = Read-FBConnection -FileName $filename
AssertNotNull $connection "Read-FBConnection"
$error.Clear()
$obj = Get-FBObjectData -Connection $connection
AssertEqual $obj.Id $UserId

Assert (Test-FBConnection $connection) 'Test-FBConnection tempfile failed'

AssertEqual $error.Count 0
$badconnection = New-FBConnection -AccessToken "Not Really a token" -NoCache
Assert (-not (Test-FBConnection $badconnection -ErrorAction SilentlyContinue)) 'Test-FBConnection bad connection'
AssertNotEqual $error.Count 0
$error.Clear()


Write-Output "Downlevel Connection tests"

# can write an error to $error
$v1token = Read-FBConnection -FileName $v011usertoken -NoCache
$error.Clear()
$user = Get-FBObjectData -Connection $v1token
AssertEqual $UserName $user.name
$error.Clear()

$v2token = Read-FBConnection -FileName $v060usertoken -NoCache
$error.Clear()
$user = Get-FBObjectData -Connection $v2token
AssertEqual $UserName $user.name

$v2token = Read-FBConnection -FileName $v060pagetoken -NoCache
$error.Clear()
$user = Get-FBObjectData -Connection $v2token
AssertEqual $PageName $user.name


Write-Output "Basic tests"

<# BUGBUG logoff not working
$conn = Show-FBConnectionDialog -Logoff
AssertNotNull $conn "Show-FBConnectionDialog -Logoff"
#>

$conn = Show-FBConnectionDialog
AssertNotNull $conn "Show-FBConnectionDialog"

$userconn = Read-FBConnection -FileName $v063usertoken
AssertNotNull $userconn "Read-FBConnection Testuser.token"
$error.Clear()
$user = Get-FBObjectData -Connection $userconn
AssertEqual $UserName $user.name

Assert (Test-FBConnection $userconn) 'Test-FBConnection $userconn failed'

$data = Get-FBObjectData
AssertNotNull $data "Get-FBObjectData"

$friends = Get-FBFriend
Assert ($friends.Count -gt 2) ">2 friends"
AssertAssociation $friends "User" "Get-FBFriend"

$data = $friends | Get-FBObjectData
AssertEqual $friends.Count $data.Count "pipeline"

$data = Get-FBFriend -fields gender | group gender
AssertEqual $data.Count 3 "group test"

$filename = [System.IO.Path]::GetTempFileName()
$friends = Get-FBFriend
$friends | Export-Csv $filename
$data = Import-Csv $filename
AssertEqual $friends.Count $data.Count "import/export"

$friends2 = @(Get-FBFriends)
AssertEqual $friends.Count $friends2.Count "alias"
AssertAssociation $friends2 "User" "Get-FBFriends"

$data = Get-FBEvent
AssertAssociation $data "Event" "Get-FBEvent"

$data = Get-FBFeed
AssertAssociation $data "Feed" "Get-FBFeed"

$data = Get-FBPost
AssertAssociation $data "Post" "Get-FBPost"

$albums = Get-FBAlbum
AssertAssociation $albums "Album" "Get-FBAlbum"

$wallAlbum = $albums | ? {$_.type -eq "wall"}
Assert ($wallAlbum.PSTypeNames -contains "Facebook.Album") "Get-FBAlbum Wall Photos"
$testAlbum = $albums | ? {$_.name -eq "Test Album"}
Assert ($testAlbum.PSTypeNames -contains "Facebook.Album") "Get-FBAlbum Test Album"

$photosBefore = @($wallAlbum | Get-FBPhoto)
AssertAssociation $photosBefore "Photo" "Get-FBPhoto photosBefore"

$data = Get-FBPhoto -AllAlbums
AssertAssociation $data "Photo" "Get-FBPhoto -AllAlbums"

$ReadPathname = $env:temp + [System.IO.Path]::GetRandomFileName()
Assert (-not (Test-Path $ReadPathname)) "Random path"
$data = $wallAlbum | Read-FBBulkPhotos -Path $ReadPathname
Assert (Test-Path $ReadPathname) "Random path"


Write-Output "Page tests"

$pageconn = New-FBConnection -PageId $PageId -NoCache
AssertNotNull $pageconn "New-FBConnection -PageId; failure probably means you are not logged onto Facebook as jonn_msft_testuser@hotmail.com"

$pageconn = New-FBConnection -Connection $userconn -PageId $PageId -NoCache
AssertNotNull $pageconn "New-FBConnection -Connection -PageId"

$pageconn = New-FBConnection -AccessToken $userconn.AccessToken -PageId $PageId -NoCache
AssertNotNull $pageconn "New-FBConnection -AccessToken -PageId"

$pageconn = Read-FBConnection -FileName $v063pagetoken
AssertNotNull $pageconn "Read-FBConnection $v063pagetoken"
$error.Clear()
$page = Get-FBObjectData -Connection $pageconn
AssertEqual $PageName $page.name

Assert (Test-FBConnection $userconn) 'Test-FBConnection $pageconn failed'

$data = Get-FBObjectData
AssertNotNull $data "Get-FBObjectData"

$data = Get-FBFeed
AssertAssociation $data "Feed" "Get-FBFeed"

$data = Get-FBPost
AssertAssociation $data "Post" "Get-FBPost"

$userconn = Read-FBConnection -FileName $v063usertoken
AssertNotNull $userconn "Read-FBConnection $v063usertoken"
$error.Clear()
$user = Get-FBObjectData -Connection $userconn
AssertEqual $UserName $user.name

Write-Output "Write tests"

# Will need to write Remove-Photo soon!
$photosAdded = @($testAlbum | Add-FBBulkPhotos -Path $ReadPathname)
AssertAssociation $photosAdded "Photo" "Get-FBPhoto photosAdded"
$photosAfter = @($testAlbum | Get-FBPhoto)
AssertAssociation $photosAfter "Photo" "Get-FBPhoto photosAfter"
# This doesn't work, presumably due to loose consistency in FB database
# AssertEqual ($photosBefore.Count + $photosAdded.Count) $photosAfter.Count "photo count"

$start = ((get-date) + (New-TimeSpan -Day 2))
$end = $start + (New-TimeSpan -Hour 1)
$event = @(New-FBEvent -Name "TestName" -StartTime $start -EndTime $end)
AssertAssociation $event "Event" "New-FBEvent"

$invite = $event | New-FBEventInvite -UserId $friends[0].UserId
Assert ($null -eq $invite) "New-FBEventInvite doesn't return anything"

$newFeed = New-FBFeed -Message ("Test Feed " + [System.IO.Path]::GetRandomFileName())
AssertAssociation $newFeed "Feed" "New-FBFeed"

$connection = Read-FBConnection
AssertNotNull $connection "Read-FBConnection returned null"
$error.Clear()
Assert (Test-FBConnection) 'Read-FBConnection failed'
$user = Get-FBObjectData -Connection $connection
AssertEqual $UserName $user.name

$newFeed = New-FBFeed -Message ("Test Feed " + [System.IO.Path]::GetRandomFileName()) -Connection $Connection
AssertAssociation $newFeed "Feed" "New-FBFeed"

$none = Get-FBFriend | Show-FBMessageDialog -Link http://www.facebook.com/JonTestCorp -Description "Desc" -Name "Name" -Picture 'http://jonnewman.com/jonnthmb.jpg' -RecipientBatchSize 35
Assert ($null -eq $none) "Show-FBMessageDialog returns null"

Write-Output "FriendList tests"

$friendlists = Get-FBFriendList
AssertAssociation $friendlists "FriendList" "Get-FBFriendList"
$newfriendlist = New-FBFriendList -Name "TestFriendList"
AssertAssociation $newfriendlist "FriendList" "Get-FBFriendList"
$friendlists2 = Get-FBFriendList
AssertAssociation $friendlists2 "FriendList" "Get-FBFriendList 2"
AssertEqual ($friendlists.Count+1) $friendlists2.Count
$members = $newfriendlist | Get-FBFriendListMember
AssertNull $members
Get-FBFriend | Add-FBFriendListMember -FriendListId $newfriendlist.FriendListId
$members = $newfriendlist | Get-FBFriendListMember
Assert (0 -ne $members.Count) 
$newfriendlist | Get-FBFriendListMember | Remove-FBFriendListMember -FriendListId $newfriendlist.FriendListId
$members2 = $newfriendlist | Get-FBFriendListMember
AssertNull $members2
$newfriendlist | Remove-FBFriendList
$friendlists3 = Get-FBFriendList
AssertAssociation $friendlists3 "FriendList" "Get-FBFriendList 2"
AssertEqual $friendlists.Count $friendlists3.Count


<#
Write-Output "Alternate App test"

$altapptoken = Read-FBConnection -FileName $v063altappusertoken -NoCache
$error.Clear()
$user = Get-FBObjectData -Connection $altapptoken
AssertEqual $UserName $user.name
$feed = New-FBFeed -Connection $altapptoken -Message ("AltApp Test Feed " + [System.IO.Path]::GetRandomFileName())
$feeditem = Get-FBObjectData -Id $feed.FeedId
AssertEqual $feeditem.application.name "FBPSModule Unit Test App"


Write-Output "No Offline Access App test"

$noofflineaccesstoken = Read-FBConnection -FileName $v063noofflineaccessusertoken -NoCache
$error.Clear()
$user = Get-FBObjectData -Connection $noofflineaccesstoken
AssertEqual $UserName $user.name
$feed = New-FBFeed -Connection $noofflineaccesstoken -Message ("NoOfflineAccess Test Feed " + [System.IO.Path]::GetRandomFileName())
$feeditem = Get-FBObjectData -Id $feed.FeedId
AssertEqual $feeditem.application.name "FBPSModule Unit Test App"
#>


Write-Output "Expired token test"

$expiredtoken = Read-FBConnection -FileName $v063altappusertokenexpired -NoCache
$error.Clear()
$user = Get-FBObjectData -Connection $expiredtoken -ErrorAction SilentlyContinue
AssertNotEqual 0 $error.Count
AssertEqual "PermissionDenied" $error[0].CategoryInfo.Category
AssertEqual "OAuth,Get-FBRawData" $error[0].FullyQualifiedErrorId
$error.Clear()


AssertEqual $error.Count 0

Write-Output "PASS"
