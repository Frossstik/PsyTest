using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;

namespace Psytest.ServiceMain.Domain.Logic.Interfaces
{
    public interface ITestProcessor
    {
        TestResult Process(TestSession session, object answers);
    }
}
