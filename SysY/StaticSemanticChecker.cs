namespace SysY;

public class StaticSemanticChecker : SysYBaseListener
{
    public bool AllowBreakOrContinue { get; set; } = false;

    public List<Dictionary<string, Entry>> SymbolTables { get; set; } = new List<Dictionary<string, Entry>>();
    
    public override void EnterCompilationUnit(
        SysYParser.CompilationUnitContext context)
    {
        base.EnterCompilationUnit(context);
    }
    
    public override void ExitCompilationUnit(
        SysYParser.CompilationUnitContext context)
    {
        base.ExitCompilationUnit(context);
    }
}