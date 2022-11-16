using System.Reflection;
using System.Text.RegularExpressions;

namespace AutoMenu;

public class SubMenu
{
    private readonly List<OptionHolder> _options = new List<OptionHolder>();
    private readonly bool _isMain;

    private readonly string? _name;
    private readonly string? _historyName = "History";

    public delegate void MethodCalledHandler(OptionHolder option, List<object> args);

    public static event MethodCalledHandler MethodCalled;

    public delegate object HistoryCall();

    public SubMenu(string? name, bool isMain = false)
    {
        _isMain = isMain;
        _name = name;
        AddOption(isMain ? "Exit" : "Back", null, null);
        if (!isMain) return;


        SubMenu? historyMenu = new SubMenu(_historyName);
        AddOption(_historyName, typeof(SubMenu).GetMethod(nameof(historyMenu.Show)), historyMenu);

        MethodCalled += (option, args) =>
        {
            if (option.Target?.GetType() != typeof(SubMenu))
            {
                var argsInfo = string.Join(", ", option.Args.Select((param, i) => $"{ToHumanCase(param.Name)}: {args[i]}"));
                HistoryCall call = () => option.Callback.Invoke(option.Target, args.ToArray());
                historyMenu.AddOption(option.Name + (argsInfo.Length > 0 ? " -> " : "") + argsInfo, call.Method, call.Target);
            }
        };
    }

    public void AddOption(string? name, MethodInfo callback, object? target)
    {
        if (callback != null)
        {
            name ??= ToHumanCase(callback.Name);
        }

        if (!name.Equals(_isMain ? "Exit" : "Back") && callback == null)
        {
            throw new Exception("Callback can't be null!");
        }

        ParameterInfo[] parameters = callback?.GetParameters();
        if (parameters != null && parameters.Any(p => !p.ParameterType.IsPrimitive && p.ParameterType != typeof(string)))
        {
            throw new Exception("Function parameters must be simple types!");
        }

        OptionHolder option = new OptionHolder()
        {
            Name = name,
            Callback = callback,
            Target = target,
            Args = parameters?.Select(p => new ParamHolder() { Name = p.Name, Type = p.ParameterType, DefaultValue = p.DefaultValue }).ToList()
        };
        _options.Insert(0, option);
    }

    public void RemoveOption(string name)
    {
        int index = _options.FindIndex(oh => oh.Name.Equals(name));
        if (index >= 0)
        {
            _options.RemoveAt(index);
        }
    }

    public void ClearMenu()
    {
        _options.Clear();
        AddOption(_isMain ? "Exit" : "Back", null, null);
    }

    public void Show()
    {
        while (true)
        {
            Console.WriteLine($"\n{_name}:");

            for (int i = 0; i < _options.Count; i++)
            {
                Console.WriteLine($"{i}) {_options[i].Name}");
            }

            Console.Write("\nSelection: ");
            string input = Console.ReadLine();
            Console.Clear();
            Console.Write($"Selection: {input}");

            if (!int.TryParse(input, out var selection))
            {
                Console.WriteLine("\nSelection has to be a number!");
                continue;
            }

            if (selection < 0 || selection >= _options.Count)
            {
                Console.WriteLine("\nSelection is out of range!");
                continue;
            }

            OptionHolder selectedOption = _options[selection];
            Console.WriteLine($") {selectedOption.Name}");
            if (selectedOption.Name.Equals(_isMain ? "Exit" : "Back"))
            {
                Console.Clear();
                break;
            }

            List<object> args = GetArgs(selectedOption.Args);

            if (args == null) continue;

            Console.WriteLine();

            if (!_name.Equals(_historyName)) MethodCalled?.Invoke(selectedOption, args);
            try
            {
                var res = selectedOption.Callback.Invoke(selectedOption.Target, args.ToArray());
                if (!string.IsNullOrEmpty(res?.ToString()))
                {
                    Console.WriteLine(res);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetBaseException());
            }
        }
    }

    private List<object> GetArgs(List<ParamHolder> argsDesc)
    {
        if (argsDesc.Count > 0) Console.WriteLine("Arguments:");
        List<object> res = new List<object>();
        foreach (ParamHolder ph in argsDesc)
        {
            Console.Write($"\t{ToHumanCase(ph.Name)} ({ph.Type.Name}): " + (DBNull.Value.Equals(ph.DefaultValue) ? "" : $"<{ph.DefaultValue}> "));
            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input) && !DBNull.Value.Equals(ph.DefaultValue))
            {
                res.Add(ph.DefaultValue);
            }
            else
            {
                try
                {
                    res.Add(Convert.ChangeType(input, ph.Type));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to convert '{input}' into type {ph.Type.Name}! Reason: {ex.Message}");
                    return null;
                }
            }
        }

        return res;
    }

    private void PrintArgs(List<ParamHolder> argsDesc, List<object> args)
    {
        if (argsDesc.Count > 0) Console.WriteLine("Arguments:");
        for (int i = 0; i < argsDesc.Count; i++)
        {
            ParamHolder ph = argsDesc[i];
            object argValue = args[i];
            Console.WriteLine($"\t{ToHumanCase(ph.Name)} ({ph.Type.Name}): {argValue}");
        }
    }

    private string? ToHumanCase(string camelCase)
    {
        if (string.IsNullOrEmpty(camelCase)) return "";
        string spaced = Regex.Replace(camelCase, "(\\B[A-Z])", " $1");
        return char.ToUpper(spaced[0]) + spaced[1..];
    }

    public class OptionHolder
    {
        public string? Name { get; set; }
        public MethodInfo Callback { get; set; }
        public object? Target { get; set; }
        public List<ParamHolder> Args { get; set; }
    }

    public class ParamHolder
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public object DefaultValue { get; set; }
    }
}