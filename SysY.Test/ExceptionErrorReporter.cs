using SysY.Reporter;

namespace SysY.Test;

public class ExceptionErrorReporter : IErrorReporter
{
    public void ReportError(
        ErrorInfo error)
    {
        throw new ReportErrorException(error);
    }
}

public class ReportErrorException(ErrorInfo errorInfo)
    : Exception($"Error: {errorInfo.ErrorType} at {errorInfo.Start.Line}:{errorInfo.Start.Column} - {errorInfo.Stop.Line}:{errorInfo.Stop.Column}");
