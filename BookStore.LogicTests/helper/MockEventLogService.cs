using BookStore.Contracts;
using BookStore.Logic.Abstractions;

namespace BookStore.LogicTests.helper;
public class MockEventLogService : IEventLogService
{
    private readonly List<EventLogDto> _logs = new();

    public Task<IEnumerable<EventLogDto>> GetAllLogsAsync()
    {
        var ordered = _logs.OrderByDescending(e => e.Timestamp);
        return Task.FromResult<IEnumerable<EventLogDto>>(ordered);
    }

    // Pomocnicza metoda do dodawania logów w testach
    public void AddLog(string eventType, string description, int? userId = null)
    {
        _logs.Add(new EventLogDto
        {
            Id = _logs.Count + 1,
            Timestamp = DateTime.UtcNow,
            EventType = eventType,
            Description = description,
            UserId = userId
        });
    }
}
