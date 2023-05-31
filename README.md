Description
-
This is an easy to set up and use CLI menu.

Just give it a function and it will request proper input for each argument and validate it on it's own.

Note that arguments have to be simple types. For functions receiving objects, wrapper is needed.

You can also re-run command from history submenu without re-typing arguments

Instruction
-
To create menu call the constructor: `public AutoMenu(string? name, bool isMain = false)`

To add menu item call AddOption function on menu object: 
`public void AddOption(string? name, MethodInfo callback, object? target)`

If name is null, function name will be converted from PascalCase/CamelCase to normal text.

After the setup, call main menu's `Show()` method.

Example
-
```
using AutoMenu;

namespace AutoMenu;

public static class App
{
    public static void Main(string[] args) {
        var mainMenu = new AutoMenu("Main", true);
        var subMenu = new AutoMenu("Second Level");
        mainMenu.AddOption("Go To Second", typeof(AutoMenu).GetMethod(nameof(subMenu.Show)), subMenu);
        mainMenu.AddOption("Say Hello", typeof(App).GetMethod(nameof(SayHello)), null);    
        subMenu.AddOption(null, typeof(App).GetMethod(nameof(LotsOfArgs)), null);            
        mainMenu.Show();
    }

    public static string SayHello(string name) {
        return $"Hello {name}";
    }

    public static string LotsOfArgs(bool they, string are, int indeed, double validated) {
        return $"They {they} are {are} indeed {indeed} validated {validated}";
    }
}
```
Sample output:
```
Main:
0) Say Hello
1) Go To Second
2) History
3) Exit

Selection: 0
```
```
Selection: 0) Say Hello
Arguments:
        Name (String): Your input here

Hello Your input here

Main:
0) Say Hello
1) Go To Second
2) History
3) Exit

Selection: 1
```
```
Selection: 1) Go To Second


Second Level:
0) Lots Of Args
1) Back

Selection: 0
```
```
Selection: 0) Lots Of Args
Arguments:
        They (Boolean): ff
Failed to convert 'ff' into type Boolean! Reason: String 'ff' was not recognized as a valid Boolean.

Second Level:
0) Lots Of Args
1) Back

Selection: 1
```
```
Main:
0) Say Hello
1) Go To Second
2) History
3) Exit

Selection: 2
```
```
Selection: 2) History


History:
0) Say Hello -> Name: Your input here
1) Back

Selection:"
```




