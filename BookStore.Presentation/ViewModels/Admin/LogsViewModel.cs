using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using BookStore.Contracts;
using BookStore.Logic.Abstractions;

namespace BookStore.Presentation.ViewModels.Admin;

public class LogsViewModel : INotifyPropertyChanged
{
    private readonly IEventLogService _service;

    public ObservableCollection<EventLogDto> Logs { get; } = new();

    public LogsViewModel(IEventLogService service)
    {
        _service = service;
        _ = LoadLogsAsync();
    }

    private async Task LoadLogsAsync()
    {
        Logs.Clear();
        var all = await _service.GetAllLogsAsync();
        foreach (var log in all)
        {
            Logs.Add(log);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
