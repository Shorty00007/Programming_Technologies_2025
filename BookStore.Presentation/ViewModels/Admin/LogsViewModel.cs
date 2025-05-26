using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BookStore.Contracts;
using BookStore.Logic.Abstractions;
using BookStore.Presentation.Commands;

namespace BookStore.Presentation.ViewModels.Admin;

public class LogsViewModel : INotifyPropertyChanged
{
    private readonly IEventLogService _service;
    private List<EventLogDto> _allLogs = new();

    public ObservableCollection<EventLogDto> Logs { get; } = new();

    private string _filterText = "";
    public string FilterText
    {
        get => _filterText;
        set
        {
            _filterText = value;
            OnPropertyChanged(nameof(FilterText));
        }
    }

    public ICommand FilterCommand { get; }

    public LogsViewModel(IEventLogService service)
    {
        _service = service;
        FilterCommand = new RelayCommand(ApplyFilter);
        _ = LoadLogsAsync();
    }

    private async Task LoadLogsAsync()
    {
        _allLogs = (await _service.GetAllLogsAsync()).ToList();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Logs.Clear();
        var filtered = string.IsNullOrWhiteSpace(FilterText)
            ? _allLogs
            : _allLogs.Where(l => l.EventType.Contains(FilterText, StringComparison.OrdinalIgnoreCase));

        foreach (var log in filtered)
            Logs.Add(log);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}

