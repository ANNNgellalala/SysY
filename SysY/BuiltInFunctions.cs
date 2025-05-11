namespace SysY;

public static class BuiltInFunctions
{
    public static List<string> BuiltInFunctionNames { get; } = new()
    {
        "getint",
        "getch",
        "getfloat",
        "getarray",
        "getfarray",
        "putint",
        "putch",
        "putfloat",
        "putarray",
        "putfarray",
        "putf", 
        "starttime",
        "stoptime",
    };

    public static Dictionary<string, Entry> BuiltInEntries { get; } = new Dictionary<string, Entry>
        {
            {"getint", Entry.CreateFunction(SysYType.Int, [])}, 
            {"getch", Entry.CreateFunction(SysYType.Int, [])},
            {"getfloat", Entry.CreateFunction(SysYType.Float, [])}, 
            {"getarray", Entry.CreateFunction(SysYType.Int, [Entry.CreateArray(SysYType.Int, [])])},
            {"getfarray", Entry.CreateFunction(SysYType.Float, [Entry.CreateArray(SysYType.Float, [])])},
            {"putint", Entry.CreateFunction(SysYType.Void, [Entry.CreateArray(SysYType.Int, [])])},
            {"putch", Entry.CreateFunction(SysYType.Void, [Entry.CreateArray(SysYType.Int, [])])},
            {"putfloat", Entry.CreateFunction(SysYType.Void, [Entry.CreateArray(SysYType.Float, [])])},
            {"putarray", Entry.CreateFunction(SysYType.Void, [Entry.CreateVariable(SysYType.Int), Entry.CreateArray(SysYType.Int, []),])},
            {"putfarray", Entry.CreateFunction(SysYType.Void, [Entry.CreateVariable(SysYType.Int), Entry.CreateArray(SysYType.Float, []),])},
            {"putf", Entry.CreateFunction(SysYType.Void, [])},
            {"starttime", Entry.CreateFunction(SysYType.Void, [])},
            {"stoptime", Entry.CreateFunction(SysYType.Void, [])},
        };
}
