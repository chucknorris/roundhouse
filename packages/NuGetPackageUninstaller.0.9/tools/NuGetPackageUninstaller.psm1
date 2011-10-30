function Uninstall-PackageAll {
param($packageName ='')

  if ($packageName -eq '') { 
@"
You must supply a package name to uninstall or all
"@ | Write-Host -ForegroundColor White -BackgroundColor DarkRed
    throw [System.Exception]
  }

  if ($packageName -like 'all') {
    $packages = get-package -update
  } else {
    $packages = get-package $packageName
  }
  
  $upNormal=Get-Command 'Uninstall-Package' -CommandType Cmdlet;

  foreach ($package in $packages) {
    write-host "Removing $($package.Id) from all referenced projects"
    $PackageID = $package.Id
    $packageManager = $host.PrivateData.packageManagerFactory.CreatePackageManager()

    foreach ($project in Get-Project -all) {
      $fileSystem = New-Object NuGet.PhysicalFileSystem($project.Properties.Item("FullPath").Value) 	
      $repo = New-Object NuGet.PackageReferenceRepository($fileSystem, $packageManager.LocalRepository)

      foreach ($package in $repo.GetPackages() | ? {$_.Id -eq $PackageID}) {
        & $upNormal $package.Id -Project:$project.Name
      }
    }
  }
}

export-modulemember -function Uninstall-PackageAll;