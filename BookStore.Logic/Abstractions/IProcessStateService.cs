using BookStore.Contracts;

public interface IProcessStateService
{
    Task<ProcessStateDto?> GetLatestSnapshotAsync();
    Task<ProcessStateDto?> GetSnapshotByDateAsync(DateTime date);
}
