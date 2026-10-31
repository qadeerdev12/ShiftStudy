using AwesomeAssertions;
using NodaTime;

namespace ShiftStudy.Scheduling.Tests;

// Pins down the Australia/Sydney changeover weekends (NFR-4) that engine tests will build on.
public class SydneyDaylightSavingTests
{
    private static readonly DateTimeZone Sydney = DateTimeZoneProviders.Tzdb["Australia/Sydney"];

    private static Duration ClockShiftLength(LocalDateTime start, LocalDateTime end) =>
        end.InZoneStrictly(Sydney).ToInstant() - start.InZoneStrictly(Sydney).ToInstant();

    [Fact]
    public void Night_shift_across_October_change_is_seven_hours()
    {
        var start = new LocalDateTime(2026, 10, 3, 22, 0);
        var end = new LocalDateTime(2026, 10, 4, 6, 0);

        ClockShiftLength(start, end).Should().Be(Duration.FromHours(7));
    }

    [Fact]
    public void Night_shift_across_April_change_is_nine_hours()
    {
        var start = new LocalDateTime(2027, 4, 3, 22, 0);
        var end = new LocalDateTime(2027, 4, 4, 6, 0);

        ClockShiftLength(start, end).Should().Be(Duration.FromHours(9));
    }
}
