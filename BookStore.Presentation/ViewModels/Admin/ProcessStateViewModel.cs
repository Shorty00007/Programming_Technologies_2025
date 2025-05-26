using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using BookStore.Contracts;
using BookStore.Logic.Abstractions;

namespace BookStore.Presentation.ViewModels.Admin;

public class ProcessStateViewModel : INotifyPropertyChanged
{
    private readonly IProcessStateService _service;

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

    public ProcessStateViewModel(IProcessStateService service)
    {
        _service = service;
        _ = LoadSnapshotAsync();
    }

    private async Task LoadSnapshotAsync()
    {
        var snapshot = await _service.GetLatestSnapshotAsync();

        if (snapshot == null)
        {
            SnapshotText = "No snapshot found.";
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📦 Total Books In Stock: {snapshot.TotalBooksInStock}");
        sb.AppendLine($"📬 Total Orders: {snapshot.TotalOrders}");
        sb.AppendLine($"💰 Total Revenue: {snapshot.TotalRevenue:C}");
        sb.AppendLine($"🕒 Recorded At: {snapshot.RecordedAt:g}");

        SnapshotText = sb.ToString();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
