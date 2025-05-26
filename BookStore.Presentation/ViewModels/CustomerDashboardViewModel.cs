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
        public ObservableCollection<BookDto> SelectedBooks { get; set; } = new();

        

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

        public bool CanOrderSelectedBook => SelectedBooks.Any();


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

            SelectedBooks.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(CanOrderSelectedBook));
                _orderSelectedBookCommand.RaiseCanExecuteChanged();
            };

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var books = await _bookService.GetAllBooksAsync();
            AvailableBooks.Clear();
            foreach (var book in books.Where(b => b.Stock > 0))
                AvailableBooks.Add(book);

            await LoadOrdersAsync();
        }

        private async Task OrderSelectedBookAsync()
        {
            if (!SelectedBooks.Any())
                return;

            var items = SelectedBooks.Select(b => (b.Id, 1)).ToList();

            try
            {
                await _orderService.PlaceOrderAsync(_currentUser.Id, items);

                string titles = string.Join(", ", SelectedBooks.Select(b => $"\"{b.Title}\""));

                MessageBox.Show($"Order for: {titles} was placed successfully!",
                                "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);

                foreach (var book in SelectedBooks.ToList())
                    AvailableBooks.Remove(book);

                SelectedBooks.Clear();
                await LoadOrdersAsync();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while placing the order: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                await LoadOrdersAsync();
                await LoadDataAsync();
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
