using Microsoft.Extensions.Logging;
using Moq;

namespace Homely.Tests.Unit.Base;

/// <summary>
/// Base class for unit tests providing common test infrastructure and helpers
/// </summary>
public abstract class UnitTestBase
{
    /// <summary>
    /// Creates a mock logger for the specified type
    /// </summary>
    protected static Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Verifies that a logger was called with specific log level and message
    /// </summary>
    protected static void VerifyLoggerWasCalled<T>(
        Mock<ILogger<T>> mockLogger,
        LogLevel logLevel,
        Times times)
    {
        mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            times);
    }

    /// <summary>
    /// Creates test DateOnly from year, month, day
    /// </summary>
    protected static DateOnly CreateDate(int year, int month, int day)
    {
        return new DateOnly(year, month, day);
    }

    /// <summary>
    /// Creates test Guid from integer for easier debugging
    /// </summary>
    protected static Guid CreateTestGuid(int value)
    {
        return new Guid($"00000000-0000-0000-0000-{value:D12}");
    }
}
