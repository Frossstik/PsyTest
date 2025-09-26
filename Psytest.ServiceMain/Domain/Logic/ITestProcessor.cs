using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Domain.Logic
{
    public interface ITestProcessor
    {
        TestResult Process(TestSession session, Object answers);
    }
}
