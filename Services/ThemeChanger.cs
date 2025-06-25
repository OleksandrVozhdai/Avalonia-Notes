using Avalonia.Media;
using MyNotepad.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MyNotepad.Services
{
	internal class ThemeChanger
	{
		private string _themeName { get; set; }
		private MainWindowViewModel _viewModel;

		public ThemeChanger(string themeName, MainWindowViewModel viewModel)
		{
			_themeName = themeName;
			_viewModel = viewModel;
		}

		public string ThemeName
		{
			get { return _themeName; }
			set { _themeName = value; }
		}

		public void ChangeTheme()
		{
			switch (_themeName)
			{
				case "dark":
					if (_viewModel != null)
					{
						_viewModel.BackgroundColorUpperPanel = new SolidColorBrush(Color.Parse("#171a1f"));
						_viewModel.BackgroundMainGrid = new SolidColorBrush(Color.Parse("#23272f"));
						_viewModel.TextBoxBackgroundColor = new SolidColorBrush(Color.Parse("#1c1f26"));
						_viewModel.ForegroundColor = new SolidColorBrush(Color.Parse("#ffffff"));
					}
					break;
				case "light":
					if (_viewModel != null)
					{
						_viewModel.BackgroundColorUpperPanel = new SolidColorBrush(Color.Parse("#f7deb7"));
						_viewModel.BackgroundMainGrid = new SolidColorBrush(Color.Parse("#fff4e3"));
						_viewModel.TextBoxBackgroundColor = new SolidColorBrush(Color.Parse("#d1c8ba"));
						_viewModel.ForegroundColor = new SolidColorBrush(Color.Parse("#000000"));
					}
					break;
				default:
					break;
			}
		}
	}
}
