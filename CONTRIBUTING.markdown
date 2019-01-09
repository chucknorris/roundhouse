Project RoundhousE - Guide for Contributors
=======
<img src="https://github.com/ferventcoder/roundhouse/raw/master/docs/logo/RoundhousE_Logo.jpg" height="200" alt="RoundhousE - Professional Database Management" />  
  
Thanks for your interest in contributing to the RoundhousE project! These guidelines will make it easier for the team to incorporate your submissions into the product.
  
# Related Guides
The guidelines for contributing to RoundhousE follow closely those for the Chocolatey project. Please start by reviewing the Chocolatey guide here.  
[Chocolatey Contributing Guide](https://github.com/chocolatey/choco/blob/master/CONTRIBUTING.md)

This guide contains much of what you will need to know.

## Code Style
As noted in the Chocolatey contributors guide, it is difficult to incorporate changes if the new code has been formatted in a different style from what is already prevailing in the existing code base.  
Please observe the following code style in your own contributions.

### Class, Interface, and Enum Names
Class, interface, and Enum names use **`UpperCaseStyle`** except for the automated test classes (see tests style below). Interface names do not use the letter I so they are named just like classes.  

```
    public interface KnownFolders  
	public sealed class DefaultKnownFolders : KnownFolders  
    public enum RecoveryMode
    {
        NoChange,
        Simple,
        Full
    }
```

### Other Names
Other items are named with **`all_lower_case`** style. This includes namespaces, parameters, methods, variables, properties, fields, and constants.

```
long version_the_database(string repository_path, string repository_version);
private readonly FileSystemAccess file_system;
string current_version = database.get_version(repository_path);
```

### Automated Test Class Names
The automated test class names also use the **`all_lower_case`** style.  
In addition to the case style the automated test classes use (BDD) behavior driven design naming. A review of the existing tests is the best way to get a feel for this so please look these over and use them as the basis for your own automated tests. 

```
public class when_asking_the_container_to_resolve_an_item_and_it_does_not_have_the_item_registered : concern_for_container
public void should_throw_an_exception()
``` 
### Indentation
Indentation is 4 spaces without tabs. Braces are on a new line using BSD style.

```
namespace N
{
    public class C
    {
        void foo();
        private int prop_name { get; set; }
    }
}
```

### Tooling Support
#### ReSharper
A .DotSettings file has been included in the solution for those using this tool. The ReSharper settings file contains the RoundhousE specific naming rules that differ from ReSharper defaults so if your own ReSharper settings differ significantly from the defaults the solution settings file will not cover all cases.

#### EditorConfig
The EditorConfig extension for Visual Studio can be used to control some common settings including the tab size and use of spaces instead of tabs. An .editorconfig file is included for those using this extension. The EditorConfig extension is available in the Visual Studio extensions gallery.  
**NOTE:** If you are new to EditorConfig please be aware that if you open a solution with an editorconfig file the extension will change and save your Visual Studio settings to match the file. Your Visual Studio settings will not revert back to their previous values automatically.
