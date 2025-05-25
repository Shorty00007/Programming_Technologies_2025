using BookStore.Contracts;
using BookStore.Data.Abstractions;
using BookStore.Logic.Abstractions;
using Microsoft.EntityFrameworkCore;

public class ProcessStateService : IProcessStateService
{
    private readonly IBookStoreContext _context;

    public ProcessStateService(IBookStoreContext context)
    {
        _context = context;
    }

    public async Task<ProcessStateDto?> GetLatestSnapshotAsync()
    {
        var query =
            from state in _context.ProcessStates
            orderby state.RecordedAt descending
            select new ProcessStateDto
            {
                RecordedAt = state.RecordedAt,
                TotalBooksInStock = state.TotalBooksInStock,
                TotalOrders = state.TotalOrders,
                TotalRevenue = state.TotalRevenue
            };

        return await query.FirstOrDefaultAsync();
    }

    public async Task<ProcessStateDto?> GetSnapshotByDateAsync(DateTime date)
    {
        var query =
            from state in _context.ProcessStates
            where state.RecordedAt.Date == date.Date
            orderby state.RecordedAt descending
            select new ProcessStateDto
            {
                RecordedAt = state.RecordedAt,
                TotalBooksInStock = state.TotalBooksInStock,
                TotalOrders = state.TotalOrders,
                TotalRevenue = state.TotalRevenue
            };

        return await query.FirstOrDefaultAsync();
    }
}
