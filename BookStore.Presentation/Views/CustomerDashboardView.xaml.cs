using System.Windows.Controls;
using BookStore.Presentation.ViewModels;
using BookStore.Contracts;
namespace BookStore.Presentation.Views;
public partial class CustomerDashboardView : UserControl
{
    public CustomerDashboardView()
    {
        InitializeComponent();

        this.Loaded += (_, __) =>
        {
            if (this.DataContext is CustomerDashboardViewModel vm)
            {
                BookListBox.SelectionChanged += (s, e) =>
                {
                    vm.SelectedBooks.Clear();
                    foreach (var item in BookListBox.SelectedItems)
                        vm.SelectedBooks.Add((BookDto)item);
                };
            }
        };
    }
}

