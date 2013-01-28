param([string]$RootDir)

$cert = Get-ChildItem Cert:\CurrentUser\My\7A8070850715FEBF739EF65F6F7AAB8C1AF4D2F1

Get-ChildItem $RootDir -Recurse -Include *.ps1,*.psm1,*.ps1xml | 
    Where {!$_.PSIsContainer -and $_.Name -ne 'Pscx.UserPreferences.ps1'} | 
    Foreach {
        Set-AuthenticodeSignature -Certificate $cert -TimestampServer http://timestamp.digicert.com $_.FullName
    }
    