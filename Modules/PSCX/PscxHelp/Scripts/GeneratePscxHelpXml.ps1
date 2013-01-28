#---------------------------------------------------------
# Desc: This script generates the Pscx.dll-Help.xml file
#---------------------------------------------------------
param([string]$outputDir = $(throw "You must specify the output path to emit the generated file"),
      [string]$localizedHelpPath = $(throw "You must specify the path to the localized help dir"),
      [string]$configuration = $(throw "You must specify the build configuration"))
      
$ModuleDir        = "$ScriptDir\..\..\Pscx\bin\$configuration"      
$PscxPath         = Join-Path $ModuleDir "Pscx"            
$PscxManifest     = "$PscxPath.psd1"            
$PscxModule       = "$PscxPath.dll"             
$outputDir        = Resolve-Path $outputDir
$ProviderHelpPath = Split-Path $outputDir -parent
$transformsDir    = Join-Path $ProviderHelpPath Transformations
$MergedHelpPath   = Join-Path $outputDir MergedHelp.xml
$PscxHelpPath     = Join-Path $outputDir Pscx.dll-Help.xml

Import-Module $PscxManifest

# Test the XML help files
gci $localizedHelpPath\*.xml  | Foreach {
	if (!(Test-Xml $_)) {
		Test-Xml $_ -verbose
		Write-Error "$_ is not a valid XML file"
		exit 1
	}
}
gci $providerHelpPath\Provider*.xml  | Foreach {
	if (!(Test-Xml $_)) {
		Test-Xml $_ -verbose
		Write-Error "$_ is not a valid XML file"
		exit 1
	}
}

Get-PSSnapinHelp $PscxModule -LocalizedHelpPath $localizedHelpPath > $MergedHelpPath

$contents = Get-Content $MergedHelpPath
$contents | foreach {$_ -replace 'PscxPathInfo','String'} > $MergedHelpPath

Convert-Xml $MergedHelpPath -xslt $transformsDir\Maml.xslt | Out-File $PscxHelpPath -Encoding Utf8

# Low tech approach to merging in the provider help
$helpfile = Get-Content $PscxHelpPath | ? {$_ -notmatch '</helpItems>'}
$providerHelp = @()
gci $providerHelpPath\Provider*.xml | ? {$_.Name -notmatch 'Provider_template'} | Foreach {
	Write-Host "Processing $_"
	$providerHelp += Get-Content $_
}

$helpfile += $providerHelp
$helpfile += '</helpItems>'
$helpfile > $PscxHelpPath
