using System.Collections.Generic;

namespace Psytest.ServiceMain.Domain.Logic.Interfaces
{
    public interface IChartGenerator
    {
        byte[] GenerateBarChartBytes(string[] labels, List<double> values);
    }
}
