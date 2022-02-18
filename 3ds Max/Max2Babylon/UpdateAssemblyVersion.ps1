$folder = split-path -parent $MyInvocation.MyCommand.Definition 
$csPath = Join-Path -Path $folder -ChildPath "\AssemblyInfo.cs"
$xmlPath = Join-Path -Path $folder -ChildPath "\app.config"

$p4 = "C:\Program Files\Perforce\p4.exe"
&$p4 edit $csPath 

$file = get-item $xmlPath        
$xml = [xml](get-content $file)
$CurrentVersion = "1.0.0.0"
foreach ($keyMap in  $xml.Configuration.AppSettings.ChildNodes) {
    if($keyMap.key -eq "version"){
        $CurrentVersion = $keyMap.value
    }    
}

$pattern = '\[assembly: AssemblyVersion\("(.*)"\)\]'
(Get-Content $csPath) | ForEach-Object {
    if ($_ -match $pattern) {
        # We have found the matching line, edit the version number and put back.
        '[assembly: AssemblyVersion("{0}")]' -f $CurrentVersion
    }
    else {
        # Output line as is
        $_
    }
} | Set-Content $csPath

Write-Output "Changing AssemblyVersion"
