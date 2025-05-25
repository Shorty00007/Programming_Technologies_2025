using BookStore.Contracts;
using BookStore.Data.Abstractions;
using BookStore.Logic.Abstractions;
using Microsoft.EntityFrameworkCore;

public class EventLogService : IEventLogService
{
    private readonly IBookStoreContext _context;

    public EventLogService(IBookStoreContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventLogDto>> GetAllLogsAsync()
    {
        var logs =
            from e in _context.EventLogs
            orderby e.Timestamp descending
            select e;

        var entities = await logs.ToListAsync();

        var dtos = entities.Select(e => new EventLogDto
        {
            Id = e.Id,
            Timestamp = e.Timestamp,
            EventType = e.EventType,
            Description = e.Description,
            UserId = e.UserId
        });

        return dtos;
    }

}
