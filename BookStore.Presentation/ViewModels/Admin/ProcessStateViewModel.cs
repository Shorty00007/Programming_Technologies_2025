using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BookStore.Contracts;
using BookStore.Logic.Abstractions;
using BookStore.Presentation.Commands;

namespace BookStore.Presentation.ViewModels.Admin;

public class ProcessStateViewModel : INotifyPropertyChanged
{
    private readonly IProcessStateService _service;

    public DateTime SelectedDate { get; set; } = DateTime.Today;

    private string _snapshotText = "Loading...";
    public string SnapshotText
    {
        get => _snapshotText;
        set
        {
            _snapshotText = value;
            OnPropertyChanged(nameof(SnapshotText));
        }
    }

    public ICommand LoadByDateCommand { get; }

    public ProcessStateViewModel(IProcessStateService service)
    {
        _service = service;
        LoadByDateCommand = new AsyncCommand(LoadSnapshotByDateAsync);
        _ = LoadSnapshotAsync(); // load latest on init
    }

    private async Task LoadSnapshotByDateAsync()
    {
        var snapshot = await _service.GetSnapshotByDateAsync(SelectedDate);

        if (snapshot == null)
        {
            SnapshotText = $"No snapshot found for {SelectedDate:d}.";
            return;
        }

        SnapshotText = FormatSnapshot(snapshot);
    }

    private async Task LoadSnapshotAsync()
    {
        var snapshot = await _service.GetLatestSnapshotAsync();

        if (snapshot == null)
        {
            SnapshotText = "No snapshot found.";
            return;
        }

        SnapshotText = FormatSnapshot(snapshot);
    }

    private string FormatSnapshot(ProcessStateDto snapshot)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"📦 Total Books In Stock: {snapshot.TotalBooksInStock}");
        sb.AppendLine($"📬 Total Orders: {snapshot.TotalOrders}");
        sb.AppendLine($"💰 Total Revenue: {snapshot.TotalRevenue:C}");
        sb.AppendLine($"🕒 Recorded At: {snapshot.RecordedAt:g}");
        return sb.ToString();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}

