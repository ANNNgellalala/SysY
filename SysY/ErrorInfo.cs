using System.ComponentModel;
using Antlr4.Runtime;
using NetEscapades.EnumGenerators;

namespace SysY;

[EnumExtensions]
public enum ErrorType
{
    [Description("使用未定义变量")]
    VarUnknown = 1,

    [Description("当前作用域重复定义变量，以及函数形参重复定义")]
    VarDuplicated,

    [Description("使用未定义函数")]
    FuncUnknown,

    [Description("函数重复定义")]
    FuncDuplicated,

    [Description("函数参数/类型不匹配，函数调用必须保证实际参数的个数和类型都与函数声明中的形式参数完全匹配")]
    FuncParamsNotMatch,

    [Description("函数返回值类型不匹配, 例如函数返回类型void/float时，函数内出现带返回值为int的return语句")]
    FuncReturnTypeNotMatch,

    [Description("数组下标不是整数")]
    ArrayIndexNotInt,

    [Description("break语句不在循环中")]
    BreakNotInLoop,

    [Description("continue语句不在循环中")]
    ContinueNotInLoop,

    [Description("对非数组变量采用下标变量的形式访问")]
    VisitVariableError,
};


public class ErrorInfo
{
    /// <summary>
    /// 错误类型
    /// </summary>
    public ErrorType ErrorType { get; set; }

    public IToken Start { get; set; }

    public IToken Stop { get; set; }

    /// <summary>
    /// 六个参数，依次是<see cref="ErrorType"/> <see cref="StartLine"/>, <see cref="StartColumn"/>, <see cref="EndLine"/>, <see cref="EndColumn"/>, 以及
    /// 类型为<see cref="Span{T}"/>的SourceCode
    /// </summary>
    public static string MessageFormat { get; set; } = "Error: {0} \"{5}\"";
}
