
Write-Output "Building Facebook Module Part 1"

Write-Debug "FacebookPSModule loading: CLRVersion: $($PSVersionTable.CLRVersion)"
Write-Verbose "FacebookPSModule loading: Importing Facebook C# SDK for .NET 4.0: $PSScriptRoot/Facebook.dll)"
Import-Module $PSScriptRoot/Facebook.dll

. $PSScriptRoot/Facebook.ps1

Write-Output "Building Facebook Module Part 2"

Set-Alias -Name Get-FBFriends -Value Get-FBFriend
Set-Alias -Name Get-FBEvents  -Value Get-FBEvent
Set-Alias -Name Get-FBFeeds   -Value Get-FBFeed
Set-Alias -Name Get-FBPosts   -Value Get-FBPost
Set-Alias -Name Get-FBAlbums  -Value Get-FBAlbum
Set-Alias -Name Get-FBPhotos  -Value Get-FBPhoto
Set-Alias -Name Set-FBStatus  -Value New-FBFeed
Set-Alias -Name Add-FBPhoto   -Value New-FBPhoto

$functions = @(
    'Get-FBObjectData',
    'Clear-FBConnection',
    'Get-FBConnection',
    'New-FBConnection',
    'Read-FBConnection',
    'Test-FBConnection',
    'Write-FBConnection',
    'Get-FBExtendedAccessToken',
    'Get-FBAssociation',
    'Get-FBFriend',
    'Get-FBEvent',
    'Get-FBFeed',
    'Get-FBPage',
    'Get-FBPost',
    'Get-FBGroup',
    'Get-FBMember',
    'Get-FBAlbum',
    'Get-FBPhoto',
    'Add-FBBulkPhotos',
    'Read-FBBulkPhotos',
    'Get-FBEvent',
    'New-FBEvent',
    'New-FBEventInvite',
    'New-FBFeed',
    'New-FBPhoto',
    'Get-FBObjectData',
    'Show-FBConnectionDialog',
    'Show-FBMessageDialog',
    'Get-FBFriendList',
    'New-FBFriendList',
    'Remove-FBFriendList',
    'Get-FBFriendListMember',
    'Add-FBFriendListMember',
    'Remove-FBFriendListMember'
)
# not exporting Get-FBRawData, Convert-FBJSON, Convert-FBJSONHash

$aliases = @(
'Get-FBFriends',
'Get-FBEvents',
'Get-FBFeeds',
'Get-FBPosts',
'Get-FBAlbums',
'Get-FBPhotos',
'Set-FBStatus',
'Add-FBPhoto'
)

$variables = @(
'FB_DefaultExtendedPermissions',
'FB_DefaultAppId',
'FB_DefaultConnectionFile'
)

Export-ModuleMember -Function $functions -Alias $aliases -Variable $variables
