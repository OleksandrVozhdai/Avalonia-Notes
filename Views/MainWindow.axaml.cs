using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Collections.Generic;
using Avalonia.Controls.ApplicationLifetimes;
using System.IO;
using Avalonia;
using System;
using Avalonia.Controls.Documents;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Tmds.DBus.Protocol;
using Avalonia.Media;
using MyNotepad.Services;
using MyNotepad.ViewModels;
using MyNotepad.Models;
using System.Linq;


namespace MyNotepad.Views;

public partial class MainWindow : Window
{
	private int fontSize = 15;
	private bool newFile = true;
	private bool isDarkTheme;
	private bool isTextSaved = true;
	private string RawText = "";
	private string? LastFile;
	private string selectedTextBeforeLosingFocus = "";
	private int selectionStartBeforeLosingFocus = 0;
	private int selectionLengthBeforeLosingFocus = 0;
	private int selectionEndBeforeLosingFocus = 0;

	private UndoManager _undoManager = new();

	public MainWindow()
	{
		InitializeComponent();

		BaseTextBox.TextChanged += TextBox_TextChanged;

		BoldButton.AddHandler(InputElement.PointerPressedEvent, ControlButton_PointerPressed, RoutingStrategies.Tunnel);
		ItalicButton.AddHandler(InputElement.PointerPressedEvent, ControlButton_PointerPressed, RoutingStrategies.Tunnel);
		UnderLineButton.AddHandler(InputElement.PointerPressedEvent, ControlButton_PointerPressed, RoutingStrategies.Tunnel);

		FontTextBox.Text = fontSize.ToString();
		FontTextBlock.Text = fontSize.ToString();
		BaseTextBlock.FontSize = fontSize;
		BaseTextBox.FontSize = fontSize;

		var settings = SettingsManager.LoadSettings();
		string theme = settings.Theme;

#if DEBUG
		Debug.WriteLine($"Current theme: {theme}");
#endif

		if (theme == "dark")
		{
			isDarkTheme = true;
			ThemeSwitcher.IsChecked = false;
		}
		else
		{
			isDarkTheme = false;
			ThemeSwitcher.IsChecked = true;
		}

	}


	private void ControlButton_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed == true)
		{
			selectedTextBeforeLosingFocus = BaseTextBox.SelectedText;
			selectionStartBeforeLosingFocus = BaseTextBox.SelectionStart;
			selectionLengthBeforeLosingFocus = BaseTextBox.SelectedText.Length;
			selectionEndBeforeLosingFocus = BaseTextBox.SelectionEnd;
		}
	}

	private void Exit_Button_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void DragWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
		{
			this.BeginMoveDrag(e);
		}
	}

	[Obsolete]
	private async void OpenFile_Button_Click(object sender, RoutedEventArgs e)
	{

		var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
		if (window is null)
			return;

		var files = await new OpenFileDialog
		{
			AllowMultiple = false
		}.ShowAsync(window);

		if (files != null && files.Length > 0)
		{
			LastFile = files[0];
			NameTextBlock.Text = Path.GetFileName(LastFile);
			string content = await File.ReadAllTextAsync(LastFile);
			newFile = false;
			RawText = content; 
			ApplyFormattedText(BaseTextBlock, RawText); 
		}
	}

	private void ChangeNameTextBoxSaveStatus()
	{
		if (Path.GetFileName(LastFile) != null)
		{
			if (isTextSaved)
				NameTextBlock.Text = Path.GetFileName(LastFile);
			else NameTextBlock.Text = Path.GetFileName(LastFile) + "*";
		}
	}

	private void TextBox_TextChanged(object? sender, EventArgs e)
	{
		isTextSaved = false;
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

	[Obsolete]
	private async void SaveFile_Button_Click(object sender, RoutedEventArgs e)
	{
		var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
		if (window is null)
			return;

		if (newFile)
		{
			var dialog = new SaveFileDialog
			{
				DefaultExtension = "txt"
			};

			LastFile = await dialog.ShowAsync(window);
			NameTextBlock.Text = Path.GetFileName(LastFile);

			if (!string.IsNullOrEmpty(LastFile))
			{
				try
				{
					await File.WriteAllTextAsync(LastFile, BaseTextBox.Text);
					newFile = false;
					isTextSaved = true;
					ChangeNameTextBoxSaveStatus();
				}
				catch
				{
					
					// Exception intentionally ignored
				}
			}
		}
		else if (!string.IsNullOrEmpty(LastFile))
		{
			await File.WriteAllTextAsync(LastFile, BaseTextBox.Text);
			isTextSaved = true;
			ChangeNameTextBoxSaveStatus();
		}
	}

	[Obsolete]
	private async void SaveFileAs_Button_Click(object sender, RoutedEventArgs e)
	{
		var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
		if (window is null)
			return;

		var dialog = new SaveFileDialog
		{
			DefaultExtension = "txt"
		};

		LastFile = await dialog.ShowAsync(window);
		NameTextBlock.Text = Path.GetFileName(LastFile);

		if (!string.IsNullOrEmpty(LastFile))
		{
			try
			{
				await File.WriteAllTextAsync(LastFile, BaseTextBox.Text);
				newFile = false;
				isTextSaved = true;
				ChangeNameTextBoxSaveStatus();
			}
			catch
			{
				// Exception intentionally ignored
			}
		}
	}

	private void Settings_Button_Click(object sender, RoutedEventArgs e)
	{
		SettingsGrid.IsVisible = !SettingsGrid.IsVisible;
	}



	private void ChangeTheme_Switch_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		var viewModel = DataContext as MainWindowViewModel;

		if (viewModel == null) return;

		if (isDarkTheme)
		{
			var changer = new ThemeChanger("light", viewModel);
			changer.ChangeTheme();
			isDarkTheme = false;

			SettingsManager.SaveSettings(new AppSettings { Theme = "light" });
		}
		else
		{
			var changer = new ThemeChanger("dark", viewModel);
			changer.ChangeTheme();
			isDarkTheme = true;

			SettingsManager.SaveSettings(new AppSettings { Theme = "dark" });
		}
	}

	private void BaseTextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
	{
		BaseTextBox.Text = BaseTextBlock.Text;
		BaseTextBlock.IsVisible = false;
		BaseTextBox.Text = RawText;	
		BaseTextBox.IsVisible = true;
		BaseTextBox.Focus();
	}

	private void FontTextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
	{
		FontTextBox.Text = FontTextBlock.Text;
		FontTextBlock.IsVisible = false;
		FontTextBox.IsVisible = true;
		FontTextBox.Focus();
	}

	private void BaseTextBox_LostFocus(object? sender, RoutedEventArgs e)
	{
		RawText = BaseTextBox.Text ?? "";
		ApplyFormattedText(BaseTextBlock, RawText);
		BaseTextBox.IsVisible = false;
		BaseTextBlock.IsVisible = true;
	}

	private void FontTextBox_LostFocus(object? sender, RoutedEventArgs e)
	{
		var textBox = sender as TextBox;
		try
		{
			fontSize = Convert.ToInt32(textBox?.Text);
		}
		catch {
		
		}

		FontTextBlock.Text = fontSize.ToString();
		FontTextBlock.IsVisible = true;
		FontTextBox.IsVisible = false;
		BaseTextBlock.FontSize = fontSize;
		BaseTextBox.FontSize = fontSize;
	}

	private void ApplyFormattedText(TextBlock textBlock, string content)
	{
		ChangeNameTextBoxSaveStatus();
		textBlock.Inlines?.Clear();

		int i = 0;
		while (i < content.Length)
		{
		
			if ((content[i] == '*' || content[i] == '_' || content[i] == '^'))
			{
				var markers = new List<char>();
				int markerStart = i;

				while (i < content.Length && (content[i] == '*' || content[i] == '_' || content[i] == '^'))
					markers.Add(content[i++]);

				int textStart = i;
				int textEnd = -1;

				
				while (i < content.Length)
				{
					if (i + markers.Count > content.Length)
						break;

					string end = content.Substring(i, markers.Count);
					string reversed = new string(markers.AsEnumerable().Reverse().ToArray());

					if (end == reversed)
					{
						textEnd = i;
						break;
					}

					i++;
				}

				if (textEnd != -1)
				{
					string innerText = content.Substring(textStart, textEnd - textStart);
					var run = new Run { Text = innerText };

					if (markers.Contains('*')) run.FontWeight = FontWeight.Bold;
					if (markers.Contains('_')) run.FontStyle = FontStyle.Italic;
					if (markers.Contains('^')) run.TextDecorations = TextDecorations.Underline;

					textBlock.Inlines?.Add(run);
					i += markers.Count;
				}
				else
				{
					
					string fallback = content.Substring(markerStart, content.Length - markerStart);
					textBlock.Inlines?.Add(new Run { Text = fallback });
					break;
				}
			}
			else
			{
				int start = i;
				while (i < content.Length && content[i] != '*' && content[i] != '_' && content[i] != '^')
					i++;

				string normal = content.Substring(start, i - start);
				textBlock.Inlines?.Add(new Run { Text = normal });
			}
		}
	}





	private void BaseTextBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			BaseTextBox_LostFocus(sender, new RoutedEventArgs());
		}
	}

	private void FontTextBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter )
		{
			FontTextBox_LostFocus(sender, new RoutedEventArgs());
		}
	}

	private void BoldText_Button_Click(object? sender, RoutedEventArgs e)
	{

		// Save the old one for undo 
		string? oldText = BaseTextBox.Text;
		//int oldSelectionStart = BaseTextBox.SelectionStart;

		if (selectionStartBeforeLosingFocus > selectionEndBeforeLosingFocus)
		{
			int temp = selectionStartBeforeLosingFocus;
			selectionStartBeforeLosingFocus = selectionEndBeforeLosingFocus;
			selectionEndBeforeLosingFocus = temp;

			string boldText = $"*{selectedTextBeforeLosingFocus}*";
			BaseTextBox.SelectedText = boldText;


			int selectionStart = selectionStartBeforeLosingFocus;
			string currentText = BaseTextBox.Text ?? "";

			if (selectionStart + boldText.Length - 2 <= currentText.Length)
			{
				BaseTextBox.Text = currentText.Remove(selectionStart, boldText.Length - 2);
				BaseTextBox.SelectionStart = selectionStart;
			}
		}
		else
		{
			string boldText = $"*{selectedTextBeforeLosingFocus}*";
			int selectionStart = selectionStartBeforeLosingFocus;
			int selectionLength = selectionLengthBeforeLosingFocus;
			string currentText = BaseTextBox.Text ?? "";

			if (selectionStart >= 0 && selectionStart + selectionLength <= currentText.Length)
			{
				BaseTextBox.Text = currentText.Remove(selectionStart, selectionLength)
				.Insert(selectionStart, boldText);
				BaseTextBox.SelectionStart = selectionStart + boldText.Length;
			}
		}

		_undoManager.AddAction(new ButtonAction(() =>
		{
#if DEBUG
			Debug.WriteLine("boldText undo");
#endif
			BaseTextBlock.Text = oldText;
			BaseTextBox.Text = oldText;
			RawText = oldText;
		}));

		BaseTextBox_LostFocus(sender, new RoutedEventArgs());
	}

	private void ItalicText_Button_Click(object? sender, RoutedEventArgs e)
	{

		// Save the old one for undo 
		string? oldText = BaseTextBox.Text;
		//int oldSelectionStart = BaseTextBox.SelectionStart;

		if (selectionStartBeforeLosingFocus > selectionEndBeforeLosingFocus)
		{
			int temp = selectionStartBeforeLosingFocus;
			selectionStartBeforeLosingFocus = selectionEndBeforeLosingFocus;
			selectionEndBeforeLosingFocus = temp;

			string italicText = $"_{selectedTextBeforeLosingFocus}_";
			BaseTextBox.SelectedText = italicText;

			int selectionStart = selectionStartBeforeLosingFocus;
			string currentText = BaseTextBox.Text ?? "";

			if (selectionStart + italicText.Length - 2 <= currentText.Length)
			{
				BaseTextBox.Text = currentText.Remove(selectionStart, italicText.Length - 2);
				BaseTextBox.SelectionStart = selectionStart;
			}
		}
		else
		{
			string italicText = $"_{selectedTextBeforeLosingFocus}_";
			int selectionStart = selectionStartBeforeLosingFocus;
			int selectionLength = selectionLengthBeforeLosingFocus;
			string currentText = BaseTextBox.Text ?? "";

			if (selectionStart >= 0 && selectionStart + selectionLength <= currentText.Length)
			{
				BaseTextBox.Text = currentText.Remove(selectionStart, selectionLength)
				.Insert(selectionStart, italicText);
				BaseTextBox.SelectionStart = selectionStart + italicText.Length;
			}
		}
		
		_undoManager.AddAction(new ButtonAction(() =>
		{
#if DEBUG
			Debug.WriteLine("ItalicText undo");
#endif
			BaseTextBlock.Text = oldText;
			BaseTextBox.Text = oldText;
			RawText = oldText;
		}));

		BaseTextBox_LostFocus(sender, new RoutedEventArgs());
	}

	private void UnderLineText_Button_Click(object? sender, RoutedEventArgs e)
	{
		// Save the old one for undo 
		string? oldText = BaseTextBox.Text;
		//int oldSelectionStart = BaseTextBox.SelectionStart;

		if (selectionStartBeforeLosingFocus > selectionEndBeforeLosingFocus)
		{
			int temp = selectionStartBeforeLosingFocus;
			selectionStartBeforeLosingFocus = selectionEndBeforeLosingFocus;
			selectionEndBeforeLosingFocus = temp;

			string underlineText = $"^{selectedTextBeforeLosingFocus}^";
			BaseTextBox.SelectedText = underlineText;

			int selectionStart = selectionStartBeforeLosingFocus;
			string currentText = BaseTextBox.Text ?? "";

			if (selectionStart + underlineText.Length - 2 <= currentText.Length)
			{
				BaseTextBox.Text = currentText.Remove(selectionStart, underlineText.Length - 2);
				BaseTextBox.SelectionStart = selectionStart;
			}
		}
		else
		{
			string underlineText = $"^{selectedTextBeforeLosingFocus}^";
			int selectionStart = selectionStartBeforeLosingFocus;
			int selectionLength = selectionLengthBeforeLosingFocus;
			string currentText = BaseTextBox.Text ?? "";

			if (selectionStart >= 0 && selectionStart + selectionLength <= currentText.Length)
			{
				BaseTextBox.Text = currentText.Remove(selectionStart, selectionLength)
				.Insert(selectionStart, underlineText);
				BaseTextBox.SelectionStart = selectionStart + underlineText.Length;
			}
		}

		_undoManager.AddAction(new ButtonAction(() =>
		{
#if DEBUG
			Debug.WriteLine("underLineText undo");
#endif
			BaseTextBlock.Text = oldText;
			BaseTextBox.Text = oldText;
			RawText = oldText;
		}));

		BaseTextBox_LostFocus(sender, new RoutedEventArgs());
	}

	private void Increase_Button_Click(object sender, RoutedEventArgs e)
	{
		fontSize++;
		FontTextBox.Text = fontSize.ToString();
		FontTextBlock.Text = fontSize.ToString();
		BaseTextBlock.FontSize = fontSize;
		BaseTextBox.FontSize = fontSize;
	}

	private void Decrease_Button_Click(object sender, RoutedEventArgs e)
	{
		fontSize--;
		FontTextBox.Text = fontSize.ToString();
		FontTextBlock.Text = fontSize.ToString();
		BaseTextBlock.FontSize = fontSize;
		BaseTextBox.FontSize = fontSize;
	}


	private void Window_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.Z)
		{
#if DEBUG
			Debug.WriteLine("Undo action triggered");
#endif
			_undoManager.UndoLast();
		}

		if(e.KeyModifiers == KeyModifiers.Control && e.Key == Key.S)
		{
			SaveFile_Button_Click(sender, new RoutedEventArgs());
			e.Handled = true; // Prevent further processing of this key event
		}
	}

	
}
