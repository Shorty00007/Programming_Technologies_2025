using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BookStore.Presentation.Helpers;

public static class PasswordBoxHelper
{
    public static readonly DependencyProperty BoundPassword =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxHelper),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

    public static string GetBoundPassword(DependencyObject obj)
        => (string)obj.GetValue(BoundPassword);

    public static void SetBoundPassword(DependencyObject obj, string value)
        => obj.SetValue(BoundPassword, value);

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox passwordBox)
        {
            passwordBox.PasswordChanged -= PasswordChanged;
            passwordBox.PasswordChanged += PasswordChanged;

            var newPassword = e.NewValue as string;
            if (passwordBox.Password != newPassword)
                passwordBox.Password = newPassword ?? string.Empty;
        }
    }

    private static void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            SetBoundPassword(passwordBox, passwordBox.Password);
            var binding = BindingOperations.GetBindingExpression(passwordBox, BoundPassword);
            binding?.UpdateSource(); // <-- ✅ force update to view model
        }
    }
}
