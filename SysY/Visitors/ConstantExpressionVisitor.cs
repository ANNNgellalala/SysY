namespace SysY.Visitors;

public class ConstantExpressionVisitor : SysYBaseVisitor<(bool Result, float Value)>
{
    public static ConstantExpressionVisitor Default { get; set; } = new();

    public override (bool Result, float Value) VisitConstExpression(
        SysYParser.ConstExpressionContext context)
    {
        return VisitAdditiveExpression(context.additiveExpression());
    }

    public override (bool Result, float Value) VisitAdditiveExpression(
        SysYParser.AdditiveExpressionContext context)
    {
        if (context.additiveExpression() is null)
            return VisitMultiplicativeExpression(context.multiplicativeExpression());
        var left = VisitAdditiveExpression(context.additiveExpression());
        var right = VisitMultiplicativeExpression(context.multiplicativeExpression());
        if (left.Result && right.Result)
            return (true, context.children[1].GetText() is "+" ? left.Value + right.Value : left.Value - right.Value);

        return (false, Single.NaN);
    }

    public override (bool Result, float Value) VisitMultiplicativeExpression(
        SysYParser.MultiplicativeExpressionContext context)
    {
        if (context.multiplicativeExpression() is null)
            return Visit(context.unaryExpression());

        var left = VisitMultiplicativeExpression(context.multiplicativeExpression());
        var right = Visit(context.unaryExpression());
        if (!left.Result || !right.Result)
            return (false, Single.NaN);

        if (context.children[1].GetText() is not "%")
            return MathF.Abs(right.Value) < 0.0001f
                ? (false, Single.NaN)
                : (true, context.children[1].GetText() is "*" ? left.Value * right.Value : left.Value / right.Value);
        if (MathF.Abs(left.Value - (int)left.Value) < 0.0001f && MathF.Abs(right.Value - (int)right.Value) < 0.0001f && MathF.Abs(right.Value) < 0.0001f)
            return (true, left.Value % right.Value);

        return (false, Single.NaN);
    }

    public override (bool Result, float Value) VisitUnaryMinus(
        SysYParser.UnaryMinusContext context)
    {
        var ret =  VisitExpression(context.expression());
        return ret.Result ? ret : (false, Single.NaN);
    }
    
    public override (bool Result, float Value) VisitUnaryPlus(
        SysYParser.UnaryPlusContext context)
    {
        return VisitExpression(context.expression());
    }

    public override (bool Result, float Value) VisitParentheses(
        SysYParser.ParenthesesContext context)
    {
        return VisitExpression(context.expression());
    }

    public override (bool Result, float Value) VisitConstant(
        SysYParser.ConstantContext context)
    {
        return (Single.TryParse(context.Constant().GetText(), out var result), result);
    }
}
