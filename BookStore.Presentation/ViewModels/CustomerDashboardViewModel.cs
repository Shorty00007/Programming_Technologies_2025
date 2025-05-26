using BookStore.Contracts;
using BookStore.Logic.Abstractions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using BookStore.Presentation.Commands;
using System.Threading.Tasks;
using System.Windows;


namespace BookStore.Presentation.ViewModels
{
    public class CustomerDashboardViewModel : INotifyPropertyChanged
    {
        private readonly IBookService _bookService;
        private readonly IOrderService _orderService;
        private readonly UserDto _currentUser;

        public ObservableCollection<BookDto> AvailableBooks { get; set; } = new();
        public ObservableCollection<OrderDto> PastOrders { get; set; } = new();


        private BookDto? _selectedBook;
        public BookDto? SelectedBook
        {
            get => _selectedBook;
            set
            {
                _selectedBook = value;
                OnPropertyChanged(nameof(SelectedBook));
                OnPropertyChanged(nameof(CanOrderSelectedBook));
                _orderSelectedBookCommand.RaiseCanExecuteChanged(); // ← to najważniejsze
            }
        }

        public bool CanOrderSelectedBook => SelectedBook != null;

        private readonly AsyncCommand _orderSelectedBookCommand;
        public ICommand OrderSelectedBookCommand => _orderSelectedBookCommand;


        public CustomerDashboardViewModel(
            IBookService bookService,
            IOrderService orderService,
            UserDto currentUser)
        {
            _bookService = bookService;
            _orderService = orderService;
            _currentUser = currentUser;

            _orderSelectedBookCommand = new AsyncCommand(OrderSelectedBookAsync, () => CanOrderSelectedBook);


            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var books = await _bookService.GetAllBooksAsync();
            AvailableBooks.Clear();
            foreach (var book in books)
                AvailableBooks.Add(book);

            await LoadOrdersAsync();
        }

        private async Task OrderSelectedBookAsync()
        {
            if (SelectedBook == null)
                return;

            var items = new List<(int bookId, int quantity)>
    {
        (SelectedBook.Id, 1)
    };

            try
            {
                await _orderService.PlaceOrderAsync(_currentUser.Id, items);

                MessageBox.Show($"Zamówienie na \"{SelectedBook.Title}\" zostało złożone pomyślnie!",
                                "Potwierdzenie", MessageBoxButton.OK, MessageBoxImage.Information);

                AvailableBooks.Remove(SelectedBook);
                SelectedBook = null; // wyczyść zaznaczenie

                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas składania zamówienia: {ex.Message}",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task LoadOrdersAsync()
        {
            var orders = await _orderService.GetOrdersForUserAsync(_currentUser.Id);
            PastOrders.Clear();
            foreach (var order in orders)
            {
                order.OrderDate = order.OrderDate.ToLocalTime(); // <- KONWERSJA CZASU
                PastOrders.Add(order);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
