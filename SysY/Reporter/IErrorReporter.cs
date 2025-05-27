namespace SysY.Reporter;

public interface IErrorReporter
{
    public void ReportError(
        ErrorInfo error);
}
