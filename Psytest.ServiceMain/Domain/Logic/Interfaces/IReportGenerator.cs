namespace Psytest.ServiceMain.Domain.Logic.Interfaces
{
    public interface IReportGenerator
    {
        byte[] GenerateDocxReport(Guid sessionId, object answers, string resultText);
    }
}
