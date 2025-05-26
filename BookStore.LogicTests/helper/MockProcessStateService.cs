using BookStore.Contracts;
using BookStore.Logic.Abstractions;

namespace BookStore.LogicTests.helper;
public class MockProcessStateService : IProcessStateService
{
    private readonly List<ProcessStateDto> _snapshots = new();

    public Task<ProcessStateDto?> GetLatestSnapshotAsync()
    {
        var latest = _snapshots
            .OrderByDescending(s => s.RecordedAt)
            .FirstOrDefault();

        return Task.FromResult<ProcessStateDto?>(latest);
    }

    public Task<ProcessStateDto?> GetSnapshotByDateAsync(DateTime date)
    {
        var match = _snapshots
            .Where(s => s.RecordedAt.Date == date.Date)
            .OrderByDescending(s => s.RecordedAt)
            .FirstOrDefault();

        return Task.FromResult<ProcessStateDto?>(match);
    }

    // Do testów — dodanie snapshotu
    public void AddSnapshot(ProcessStateDto snapshot)
    {
        _snapshots.Add(snapshot);
    }

    // Do testów — dostęp do wszystkich snapshotów
    public IEnumerable<ProcessStateDto> GetAllSnapshots()
    {
        return _snapshots;
    }
}
