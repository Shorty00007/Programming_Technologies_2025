using BookStore.Contracts;

public interface IEventLogService
{
    Task<IEnumerable<EventLogDto>> GetAllLogsAsync();
}
