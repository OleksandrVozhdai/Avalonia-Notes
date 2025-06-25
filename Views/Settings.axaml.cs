using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MyNotepad.Services;
using MyNotepad.ViewModels;

namespace MyNotepad;

public partial class Settings : Window
{

	private bool isSettings = false;
	private bool isDarkTheme = true;

	public Settings()
    {
        InitializeComponent();
		DataContext = new MainWindowViewModel();
	}

	private void Rectangle_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
		{
			this.BeginMoveDrag(e);
		}
	}

	private void Minimize_Button_Click(object sender, RoutedEventArgs e)
	{
		if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			if (desktop.MainWindow != null)
			{
				desktop.MainWindow.WindowState = WindowState.Minimized;
			}
		}
	}

	private void Maximize_Button_Click(object sender, RoutedEventArgs e)
	{
		if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			if (desktop.MainWindow != null)
			{
				if (desktop.MainWindow.WindowState == WindowState.Maximized)
				{
					desktop.MainWindow.WindowState = WindowState.Normal;
				}
				else
				{
					desktop.MainWindow.WindowState = WindowState.Maximized;
				}
			}
		}
	}

	private void Exit_Button_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void ChangeTheme_Button_Click(object sender, RoutedEventArgs e)
	{
		var viewModel = DataContext as MainWindowViewModel;
		if (isDarkTheme)
		{
			var changer = new ThemeChanger("light", viewModel);
			changer.ChangeTheme();
			isDarkTheme = false;
		}
		else
		{
			var changer = new ThemeChanger("dark", viewModel);
			changer.ChangeTheme();
			isDarkTheme = true;
		}
	}
}