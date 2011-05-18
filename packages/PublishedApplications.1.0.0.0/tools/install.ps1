param($installPath, $toolsPath, $package, $project)

#http://www.dougfinke.com/blog/index.php/2010/05/16/accessing-visual-studios-automation-api-from-powershell/
#http://msdn.microsoft.com/en-us/library/vslangproj.aspx
#http://stackoverflow.com/questions/4511344/programmatically-adding-and-editing-the-targets-in-a-visual-studio-project-file
#YES http://stackoverflow.com/questions/3160113/how-can-i-add-an-msbuild-import-with-ivsbuildpropertystorage
#YES http://msdn.microsoft.com/en-us/library/microsoft.build.construction.projectrootelement.addimport.aspx

#$targetImport = [System.IO.Path]::GetDirectoryName($project.Object.Project.FullName) + "\targets\Microsoft.Application.targets"
$targetImport = ".\targets\Microsoft.Application.targets"
Write-Host 'Adding msbuild import of '$targetImport ' to ' $project.Object.Project.Name

[Reflection.Assembly]::LoadWithPartialName("Microsoft.Build")
$vsProject = [Microsoft.Build.Construction.ProjectRootElement]::Open($project.Object.project.FullName)
$vsProject.AddImport($targetImport)