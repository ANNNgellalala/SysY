using System.Diagnostics.CodeAnalysis;

namespace SysY.Utils;

public class ArrayDimensionVisitor : SysYBaseVisitor<bool>
{
    public static ArrayDimensionVisitor Default { get; set; } = new();

    public List<Dictionary<string, Entry>> SymbolTables { get; set; }

    [MemberNotNull(nameof(SymbolTables))]
    public bool AnalyseArrayDimension(
        SysYParser.ArrayAccessContext context,
        List<Dictionary<string, Entry>> symbolTable)
    {
        SymbolTables = symbolTable;
        foreach (var expressionContext in context.expression())
        {
            if (!VisitExpression(expressionContext))
                return false;
        }

        return true;
    }

    public override bool VisitExpression(
        SysYParser.ExpressionContext context)
    {
        return VisitAdditiveExpression(context.additiveExpression());
    }

    public override bool VisitAdditiveExpression(
        SysYParser.AdditiveExpressionContext context)
    {
        if (context.additiveExpression() is null)
            return VisitMultiplicativeExpression(context.multiplicativeExpression());

        return VisitAdditiveExpression(context.additiveExpression()) && VisitMultiplicativeExpression(context.multiplicativeExpression());
    }

    public override bool VisitMultiplicativeExpression(
        SysYParser.MultiplicativeExpressionContext context)
    {
        if (context.multiplicativeExpression() is null)
            return Visit(context.unaryExpression());

        return VisitMultiplicativeExpression(context.multiplicativeExpression()) && Visit(context.unaryExpression());
    }

    public override bool VisitUnaryPlus(
        SysYParser.UnaryPlusContext context) =>
        VisitExpression(context.expression());

    public override bool VisitUnaryMinus(
        SysYParser.UnaryMinusContext context) =>
        VisitExpression(context.expression());

    public override bool VisitFunctionCall(
        SysYParser.FunctionCallContext context)
    {
        var name = context.Identifier()!.GetText()!;
        return SymbolTables[0].TryGetValue(name, out var entry) && entry.Type is SysYType.Int;
    }

    public override bool VisitPrimary(
        SysYParser.PrimaryContext context)
    {
        return Visit(context.primaryExpression());
    }

    public override bool VisitLeftValueAccess(
        SysYParser.LeftValueAccessContext context)
    {
        return Visit(context.leftValue());
    }

    public override bool VisitConstant(
        SysYParser.ConstantContext context)
    {
        return Int32.TryParse(context.GetText(), out var result) && result >= 0;
    }

    public override bool VisitParentheses(
        SysYParser.ParenthesesContext context)
    {
        return VisitExpression(context.expression());
    }

    public override bool VisitArrayAccess(
        SysYParser.ArrayAccessContext context)
    {
        var name = context.Identifier()!.GetText()!;
        for (var i = SymbolTables.Count - 1; i >= 0; i--)
            if (SymbolTables[i].TryGetValue(name, out var entry))
                return entry.Type is not SysYType.Float;

        return true;
    }

    public override bool VisitVariableAccess(
        SysYParser.VariableAccessContext context)
    {
        var name = context.Identifier()!.GetText()!;
        for (var i = SymbolTables.Count - 1; i >= 0; i--)
            if (SymbolTables[i].TryGetValue(name, out var entry))
                return entry.Type is not SysYType.Float;

        return true;
    }
}
