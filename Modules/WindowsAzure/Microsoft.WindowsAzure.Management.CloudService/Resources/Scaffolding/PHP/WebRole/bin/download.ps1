$manifestUrl = $env:MANIFESTURL
$datacenter = $args[0]
$version = $args[1]
$runtimeUrl = $args[2]
$current = [string] (Get-Location -PSProvider FileSystem)
$client = New-Object System.Net.WebClient

function downloadWithRetry {
	param([string]$url, [string]$dest, [int]$retry) 
	Write-Host
	Write-Host "Attempt: $retry\r\n"
    Write-Host
    trap {
    	Write-Host $_.Exception.ToString() + "\r\n"
	    if ($retry -lt 5) {
	    	$retry=$retry+1
	    	Write-Host
	    	Write-Host "Waiting 5 seconds and retrying\r\n"
	    	Write-Host
	    	Start-Sleep -s 5
	    	downloadWithRetry $url $dest $retry $client
	    }
	    else {
	    	Write-Host "Download failed \r\n"
	   		throw "Max number of retries downloading [5] exceeded\r\n" 	
	    }
    }
    $client.downloadfile($url, $dest)
}

function download($url) {
    $dest = $current + "\runtime.exe"
	Write-Host "Downloading $url \r\n"
	downloadWithRetry $url $dest 1
}

function getBlobUrl {
    param([string]$datacenter, [string]$version)
    Write-Host "Retrieving runtime manifest from $manifestUrl\r\n"
    $dest = $current + "\runtimemanifest.xml" 
    $client.downloadFile($manifestUrl, $dest)
    $manifest = New-Object System.Xml.XmlDocument
    $manifest.load($dest)
    $datacenterElement = $manifest.SelectSingleNode("//blobcontainer[@datacenter='" + $datacenter + "']")
    return $datacenterElement.{uri} + "php/" + $version + ".exe"
}

if ($runtimeUrl) {
    Write-Host "Using override url: $runtimeUrl \r\n"
	$url = $runtimeUrl
}
else {
	$url = getBlobUrl $datacenter $version
}
download $url
