using Microsoft.Extensions.Logging;
using Moq;

namespace SL.DesafioPagueVeloz.Application.Tests.Fixtures
{
    public static class MockLogger
    {
        public static ILogger<T> Create<T>()
        {
            return new Mock<ILogger<T>>().Object;
        }
    }
}
