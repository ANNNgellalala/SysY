using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace SysY;

public class ErrorReporter
{
    private List<ErrorInfo> _errors = [];
    
    public void ReportError(ErrorInfo error)
    {
        _errors.Add(error);
    }
    
    public void ShowErrors()
    {
        foreach (var error in _errors)
        {
            var sourceStream = error.Start.InputStream;
            Console.WriteLine(ErrorInfo.MessageFormat,
                error.ErrorType,
                error.Start.Line,
                error.Start.Column,
                error.Stop.Line,
                error.Stop.Column,
                sourceStream.GetText(new Interval(error.Start.StartIndex, error.Stop.StopIndex)));
        }
    }
}
