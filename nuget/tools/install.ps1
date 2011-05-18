param($installPath, $toolsPath, $package, $project)

Write-Host 'RoundhousE is a tool that can be local with your project or installed to the machine. If you want to do a machine install, please install using chocolatey' 
Write-Host "To get chocolatey just run 'Install-Package chocolatey' followed by 'Initialize-Chocolatey'"
Write-Host 'chocolatey install roundhouse'
Write-Host 'RoundhousE also comes with a DLL that you can include to run migrations. There is also an MSBuild version, but you will need to go to http://projectroundhouse.org and find the downloads to get that.'