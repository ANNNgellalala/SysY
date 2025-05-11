namespace SysY.Utils;

public class ArrayInitialVisitor : SysYBaseVisitor<List<float>>
{
    public static ArrayInitialVisitor Default { get; set; } = new();
    
    public List<float> VisitArrayInitial(SysYParser.ConstInitValueContext context, List<int> dimensions)
    {
        var total = dimensions.Aggregate(1, (current, dimension) => current * dimension);
        return InternalVisitArrayInitial(context, new Span<int>(dimensions.ToArray()), total);
    }

    private List<float> InternalVisitArrayInitial(SysYParser.ConstInitValueContext context, Span<int> dimensions, int total)
    {
        if (dimensions.IsEmpty)
            return [];
        
        var result = new List<float>(total);
        var cur = new List<float>();
        foreach (var element in context._elements)
        {
            if (element._elements.Count == 0)
                cur.Add(ConstantExpressionVisitor.Default.VisitConstExpression(context.constExpression()).Value);
            else
            {
                cur.AddRange(Enumerable.Repeat(0f, total / dimensions[0] - cur.Count));
                result.AddRange(cur);
                var ret = InternalVisitArrayInitial(element, dimensions[1..], total / dimensions[0]);
                result.AddRange(ret);
                cur.Clear();
            }
        }
        
        if (cur.Count > 0)
        {
            cur.AddRange(Enumerable.Repeat(0f, total / dimensions[0] - cur.Count));
            result.AddRange(cur);
        }
        
        return result;
    }
}
