using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using MyNotepad.ViewModels;
using MyNotepad.Views;
using Avalonia.Styling;
using MyNotepad.Services;

namespace MyNotepad;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			DisableAvaloniaDataAnnotationValidation();

			var viewModel = new MainWindowViewModel();

			var mainWindow = new MainWindow
			{
				DataContext = viewModel
			};

			var themeChanger = new ThemeChanger(SettingsManager.LoadSettings().Theme, viewModel);
			themeChanger.ChangeTheme();

			desktop.MainWindow = mainWindow;
		}

		base.OnFrameworkInitializationCompleted();
	}


	private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}