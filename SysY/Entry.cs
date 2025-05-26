using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using NetEscapades.EnumGenerators;

namespace SysY;

public class Entry
{
    public SysYType Type { get; set; }

    [MemberNotNullWhen(true, nameof(ConstantValue))]
    public bool IsConst { get; set; }

    [MemberNotNullWhen(true, nameof(FunctionParams))]
    public bool IsFunction { get; set; }

    [MemberNotNullWhen(true, nameof(ArrayDimensions))]
    public bool IsArray { get; set; }

    public List<int>? ArrayDimensions { get; set; }

    public List<Entry>? FunctionParams { get; set; }

    public float? ConstantValue { get; set; }

    public List<float>? ArrayElements { get; set; }

    public static Entry CreateVariable(
        SysYType type,
        bool isConst = false)
    {
        return new Entry
        {
            Type = type,
            IsConst = isConst,
            IsFunction = false,
            IsArray = false,
            ArrayDimensions = null,
            FunctionParams = null,
            ConstantValue = null
        };
    }

    public static Entry CreateFunction(
        SysYType returnType,
        List<Entry> paramsList)
    {
        return new Entry
        {
            Type = returnType,
            IsConst = false,
            IsFunction = true,
            IsArray = false,
            ArrayDimensions = null,
            FunctionParams = paramsList,
            ConstantValue = null
        };
    }

    public static Entry CreateArray(
        SysYType elementType,
        List<int> dimensions,
        bool isConst = false)
    {
        return new Entry
        {
            Type = elementType,
            IsConst = isConst,
            IsFunction = false,
            IsArray = true,
            ArrayDimensions = dimensions,
            FunctionParams = null,
            ConstantValue = null
        };
    }
}