$keyfile = "$env:userprofile\Documents\nuget-api-key.txt"
$scriptpath = split-path -parent $MyInvocation.MyCommand.Path
$nugetpath = resolve-path "$scriptpath/Tools/NuGet.exe"
$packagespath = resolve-path "$scriptpath/Output"
 
if(-not (test-path $keyfile)) {
  throw "Could not find the NuGet access key at $keyfile."
}
else {
  pushd $packagespath
 
  # get our secret key. This is not in the repository.
  $key = get-content $keyfile
 
  # Find all the packages and display them for confirmation
  $packages = dir "*.nupkg"
  write-host "Packages to upload:"
  $packages | % { write-host $_.Name }
 
  # Ensure we haven't run this by accident.
  $yes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "Uploads the packages."
  $no = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "Does not upload the packages."
  $options = [System.Management.Automation.Host.ChoiceDescription[]]($no, $yes)
 
  $result = $host.ui.PromptForChoice("Upload packages", "Do you want to upload the NuGet packages to the NuGet server?", $options, 0) 
 
  # Cancelled
  if($result -eq 0) {
    "Upload aborted"
  }
  # upload
  elseif($result -eq 1) {
    $packages | % { 
        $package = $_.Name
        write-host "Uploading $package"
        & $nugetpath push $package $key
        write-host ""
    }
  }
  popd
}