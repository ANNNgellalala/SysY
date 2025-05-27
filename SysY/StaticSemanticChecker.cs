using System.Diagnostics.CodeAnalysis;
using SysY.Reporter;
using SysY.Visitors;

namespace SysY;

public class StaticSemanticChecker : SysYBaseListener
{
    public int LoopCount { get; set; } = 0;

    [NotNull]
    public ErrorReporter ErrorReporter { get; set; }

    public List<Dictionary<string, Entry>> SymbolTables { get; set; } = [];

    public int BlockCount { get; set; }
    
    // 开始编译单元时初始化全局的符号表
    public override void EnterCompilationUnit(
        SysYParser.CompilationUnitContext context)
    {
        SymbolTables.Add(new Dictionary<string, Entry>(BuiltInFunctions.BuiltInEntries));
        // 建立全局变量和函数的符号表
        foreach (var declarationContext in context._globalValues)
            EnterDeclaration(declarationContext);

        foreach (var functionDefinitionContext in context._functions)
        {
            var name = functionDefinitionContext.Identifier()!.GetText();
            if (SymbolTables[^1].ContainsKey(name))
            {
                var error = new ErrorInfo()
                {
                    ErrorType = ErrorType.FuncDuplicated,
                    Start = functionDefinitionContext.Identifier().Symbol,
                    Stop = functionDefinitionContext.Identifier().Symbol,
                };
                ErrorReporter.ReportError(error);
                continue;
            }

            var returnType = functionDefinitionContext.functionType().GetText() switch
            {
                "int" => SysYType.Int,
                "void" => SysYType.Void,
                "float" => SysYType.Float,
                _ => throw new ArgumentException("未知的函数返回类型")
            };
            var @params = functionDefinitionContext._parameters;
            var names = new HashSet<string>();
            foreach (var param in @params)
            {
                var paramName = param.Identifier()!.GetText();
                if (names.Add(paramName))
                    continue;

                var error = new ErrorInfo()
                {
                    ErrorType = ErrorType.VarDuplicated, Start = param.Identifier().Symbol, Stop = param.Identifier().Symbol,
                };
                ErrorReporter.ReportError(error);
            }

            if (names.Count != @params.Count)
                continue;

            var entries = @params.Select(param => Entry.CreateVariable(param.bType().GetText() switch
                                     {
                                         "int" => SysYType.Int,
                                         "float" => SysYType.Float,
                                         _ => throw new ArgumentException("未知的函数返回类型")
                                     },
                                     false))
                                 .ToList();
            var functionEntry = Entry.CreateFunction(returnType, entries);
            SymbolTables[^1].Add(name, functionEntry);
        }
    }

    // 扫描结束时清除全局符号表
    public override void ExitCompilationUnit(
        SysYParser.CompilationUnitContext context)
    {
        SymbolTables.RemoveAt(SymbolTables.Count - 1);
        if (SymbolTables.Count > 0)
            throw new InvalidOperationException("退出编译单元时符号表未清空，存在作用域未正确退出");
    }

    public override void EnterConstantDeclaration(
        SysYParser.ConstantDeclarationContext context)
    {
        var isConst = true;
        var table = SymbolTables[^1];
        var type = context.bType().GetText() switch
        {
            "int" => SysYType.Int,
            "float" => SysYType.Float,
            _ => throw new ArgumentException("未知的常量类型")
        };
        foreach (var definition in context._constants)
        {
            var name = definition.Identifier()!.GetText();
            if (SymbolTables[^1].ContainsKey(name))
            {
                var error = new ErrorInfo()
                {
                    ErrorType = ErrorType.VarDuplicated, Start = definition.Identifier().Symbol, Stop = definition.Identifier().Symbol,
                };
                ErrorReporter.ReportError(error);
                continue;
            }

            // 普通变量
            if (definition._dimensions.Count == 0)
            {
                var ret = ConstantExpressionVisitor.Default.VisitConstExpression(definition.constInitValue().constExpression());
                var entry = Entry.CreateVariable(type, isConst);
                entry.ConstantValue = ret.Value;
                table.Add(name, entry);
                continue;
            }

            // 数组
            var dimensions = definition._dimensions.Select(dim => ConstantExpressionVisitor.Default.VisitConstExpression(dim))
                                       .Select(ret => (int)ret.Value)
                                       .ToList();
            var elements = ArrayInitialVisitor.Default.VisitArrayInitial(definition.constInitValue(), dimensions);
            var arrayEntry = Entry.CreateArray(type, dimensions, isConst);
            arrayEntry.ArrayElements = elements;
            table.Add(name, arrayEntry);
        }
    }

    public override void EnterVariableDeclaration(
        SysYParser.VariableDeclarationContext context)
    {
        var type = context.bType().GetText() switch
        {
            "int" => SysYType.Int,
            "float" => SysYType.Float,
            _ => throw new ArgumentException("未知的常量类型")
        };
        foreach (var variable in context._variables)
        {
            if (variable is SysYParser.DefinitionContext definition)
            {
                var name = definition.Identifier()!.GetText();
                if (SymbolTables[^1].ContainsKey(name))
                {
                    var error = new ErrorInfo()
                    {
                        ErrorType = ErrorType.VarDuplicated, Start = definition.Identifier().Symbol, Stop = definition.Identifier().Symbol,
                    };
                    ErrorReporter.ReportError(error);
                    continue;
                }

                // 普通变量
                if (definition._dimensions.Count == 0)
                {
                    var entry = Entry.CreateVariable(type);
                    SymbolTables[^1].Add(name, entry);
                    continue;
                }

                // 数组
                var dimensions = definition._dimensions.Select(dim => ConstantExpressionVisitor.Default.VisitConstExpression(dim))
                                           .Select(ret => (int)ret.Value)
                                           .ToList();
                var arrayEntry = Entry.CreateArray(type, dimensions);
                SymbolTables[^1].Add(name, arrayEntry);
            }
            else
            {
                var declare = (variable as SysYParser.DeclareContext)!;
                var name = declare.Identifier()!.GetText();
                if (SymbolTables[^1].ContainsKey(name))
                {
                    var error = new ErrorInfo()
                    {
                        ErrorType = ErrorType.VarDuplicated, Start = declare.Identifier().Symbol, Stop = declare.Identifier().Symbol,
                    };
                    ErrorReporter.ReportError(error);
                    continue;
                }

                // 普通变量
                if (declare._dimensions.Count == 0)
                {
                    var entry = Entry.CreateVariable(type);
                    SymbolTables[^1].Add(name, entry);
                    continue;
                }

                // 数组
                var dimensions = declare._dimensions.Select(dim => ConstantExpressionVisitor.Default.VisitConstExpression(dim))
                                        .Select(ret => (int)ret.Value)
                                        .ToList();
                var arrayEntry = Entry.CreateArray(type, dimensions);
                SymbolTables[^1].Add(name, arrayEntry);
            }
        }
    }

    public override void EnterFunctionDefinition(
        SysYParser.FunctionDefinitionContext context)
    {
        SymbolTables.Add(new Dictionary<string, Entry>());
        var name = context.Identifier()!.GetText();
        if (!SymbolTables[^1].TryGetValue(name, out var funcEntry))
            return;

        // Make NullAnalyzer Happy
        if (!funcEntry.IsFunction)
            return;

        var table = SymbolTables[^1];
        for (var i = 0; i < funcEntry.FunctionParams.Count; i++)
            table.Add(context._parameters[i].Identifier().GetText(), funcEntry.FunctionParams[i]);

        EnterBlock(context.block());
    }

    public override void ExitFunctionDefinition(
        SysYParser.FunctionDefinitionContext context)
    {
        SymbolTables.RemoveAt(SymbolTables.Count - 1);
    }

    public override void EnterBlock(
        SysYParser.BlockContext context)
    {
        BlockCount++;
        if (BlockCount != 1)
            SymbolTables.Add(new Dictionary<string, Entry>());

        foreach (var blockItem in context._statements)
        {
            if (blockItem.statement() != null)
            {
                switch (blockItem.statement())
                {
                    case SysYParser.AssignmentContext assignment:
                        EnterAssignment(assignment);
                        break;
                    case SysYParser.NormalExpressionContext normalExpression:
                        EnterNormalExpression(normalExpression);
                        break;
                    case SysYParser.IfContext ifStatement:
                        EnterIf(ifStatement);
                        break;
                    case SysYParser.WhileContext whileStatement:
                        EnterWhile(whileStatement);
                        break;
                    case SysYParser.BreakContext breakStatement:
                        EnterBreak(breakStatement);
                        break;
                    case SysYParser.ContinueContext continueStatement:
                        EnterContinue(continueStatement);
                        break;
                    case SysYParser.ReturnContext returnStatement:
                        EnterReturn(returnStatement);
                        break;
                }
            }
            else
            {
                var declaration = blockItem.declaration();
                if (declaration.constantDeclaration() != null)
                    EnterConstantDeclaration(declaration.constantDeclaration());
                else if (declaration.variableDeclaration() != null)
                    EnterVariableDeclaration(declaration.variableDeclaration());
            }
        }
    }
    
    public override void ExitBlock(
        SysYParser.BlockContext context)
    {
        BlockCount--;
        if (BlockCount != 1)
            SymbolTables.RemoveAt(SymbolTables.Count - 1);
    }

    public override void EnterVariableAccess(
        SysYParser.VariableAccessContext context)
    {
        var identifier = context.Identifier()!.GetText();
        for (var i = SymbolTables.Count - 1; i >= 0; i--)
        {
            if (SymbolTables[i].TryGetValue(identifier, out var entry) && entry is { IsArray: false, IsFunction: false })
                return;
        }

        var error = new ErrorInfo()
        {
            ErrorType = ErrorType.VarUnknown, Start = context.Identifier().Symbol, Stop = context.Identifier().Symbol,
        };
        ErrorReporter.ReportError(error);
    }

    public override void EnterArrayAccess(
        SysYParser.ArrayAccessContext context)
    {
        var identifier = context.Identifier()!.GetText();
        for (var i = SymbolTables.Count - 1; i >= 0; i--)
        {
            if (SymbolTables[i].TryGetValue(identifier, out var entry))
            {
                if (entry.IsArray && !ArrayDimensionVisitor.Default.AnalyseArrayDimension(context, SymbolTables))
                {
                    var indexError = new ErrorInfo()
                    {
                        ErrorType = ErrorType.ArrayIndexNotInt, Start = context.Identifier().Symbol, Stop = context.Identifier().Symbol,
                    };
                    ErrorReporter.ReportError(indexError);
                    return;
                }

                if (entry.IsArray)
                    return;
                var visitVariableError = new ErrorInfo()
                {
                    ErrorType = ErrorType.VisitVariableError, Start = context.Identifier().Symbol, Stop = context.Identifier().Symbol,
                };
                ErrorReporter.ReportError(visitVariableError);

                return;
            }
        }

        var error = new ErrorInfo()
        {
            ErrorType = ErrorType.VarUnknown, Start = context.Identifier().Symbol, Stop = context.Identifier().Symbol,
        };
        ErrorReporter.ReportError(error);
    }

    public override void EnterFunctionCall(
        SysYParser.FunctionCallContext context)
    {
        var name = context.Identifier()!.GetText();
        for (var i = SymbolTables.Count - 1; i >= 0; i--)
        {
            if (!SymbolTables[i].TryGetValue(name, out var entry))
                continue;
            if (entry.IsFunction)
                break;

            var error = new ErrorInfo()
            {
                ErrorType = ErrorType.FuncUnknown, Start = context.Identifier().Symbol, Stop = context.Identifier().Symbol,
            };
            ErrorReporter.ReportError(error);
            return;
        }
    }

    public override void EnterWhile(
        SysYParser.WhileContext context)
    {
        LoopCount++;
    }

    public override void ExitWhile(
        SysYParser.WhileContext context)
    {
        LoopCount--;
    }

    public override void EnterBreak(
        SysYParser.BreakContext context)
    {
        if (LoopCount > 0)
            return;

        var error = new ErrorInfo()
        {
            ErrorType = ErrorType.BreakNotInLoop, Start = context.Start, Stop = context.Stop,
        };
        ErrorReporter.ReportError(error);
    }

    public override void EnterContinue(
        SysYParser.ContinueContext context)
    {
        if (LoopCount > 0)
            return;

        var error = new ErrorInfo()
        {
            ErrorType = ErrorType.ContinueNotInLoop, Start = context.Start, Stop = context.Stop,
        };
        ErrorReporter.ReportError(error);
    }
}
