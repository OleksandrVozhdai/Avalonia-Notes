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


namespace MyNotepad.Views;

public partial class MainWindow : Window
{
	private int fontSize = 15;
	private bool newFile = true;
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

		BoldButton.AddHandler(InputElement.PointerPressedEvent, ControlButton_PointerPressed, RoutingStrategies.Tunnel);
		ItalicButton.AddHandler(InputElement.PointerPressedEvent, ControlButton_PointerPressed, RoutingStrategies.Tunnel);
		UnderLineButton.AddHandler(InputElement.PointerPressedEvent, ControlButton_PointerPressed, RoutingStrategies.Tunnel);

		FontTextBox.Text = fontSize.ToString();
		FontTextBlock.Text = fontSize.ToString();
		BaseTextBlock.FontSize = fontSize;
		BaseTextBox.FontSize = fontSize;
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

	private void Rectangle_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
		{
			this.BeginMoveDrag(e);
		}
	}

	[Obsolete]
	private async void OpenFile_Button_Click(object sender, RoutedEventArgs e)
	{
		newFile = false;

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
			BaseTextBlock.Text = content;
			RawText = content; 
			ApplyFormattedText(BaseTextBlock, RawText); 
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

	[Obsolete]
	private async void SaveFile_Button_Click(object sender, RoutedEventArgs e)
	{
		var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
		if (window is null)
			return;

		if (newFile)
		{
			newFile = false;
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
		fontSize = Convert.ToInt32(textBox?.Text);
		FontTextBlock.Text = fontSize.ToString();
		FontTextBlock.IsVisible = true;
		FontTextBox.IsVisible = false;
		BaseTextBlock.FontSize = fontSize;
		BaseTextBox.FontSize = fontSize;
	}

	private void ApplyFormattedText(TextBlock textBlock, string content)
	{
		textBlock.Inlines?.Clear();

		var pattern = @"([*_^]{1,3}[^*_^\r\n]+[*_^]{1,3})"; 
		var parts = Regex.Split(content, pattern);

		foreach (var part in parts)
		{
			if (string.IsNullOrEmpty(part)) continue;

			string text = part;
			var fontWeight = FontWeight.Normal;
			var fontStyle = FontStyle.Normal;
			TextDecorationCollection? decorations = null;

		
			bool isBold = text.StartsWith("*") && text.EndsWith("*");
			bool isItalic = text.StartsWith("_") && text.EndsWith("_");
			bool isUnderline = text.StartsWith("^") && text.EndsWith("^");

		
			while (
				(text.StartsWith("*") || text.StartsWith("_") || text.StartsWith("^")) &&
				(text.EndsWith("*") || text.EndsWith("_") || text.EndsWith("^"))
			)
			{
				if (text.StartsWith("*") && text.EndsWith("*"))
				{
					fontWeight = FontWeight.Bold;
					text = text.Substring(1, text.Length - 2);
				}
				else if (text.StartsWith("_") && text.EndsWith("_"))
				{
					fontStyle = FontStyle.Italic;
					text = text.Substring(1, text.Length - 2);
				}
				else if (text.StartsWith("^") && text.EndsWith("^"))
				{
					decorations = TextDecorations.Underline;
					text = text.Substring(1, text.Length - 2);
				}
				else break;
			}

			textBlock.Inlines?.Add(new Run
			{
				Text = text,
				FontWeight = fontWeight,
				FontStyle = fontStyle,
				TextDecorations = decorations
			});
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
	}
}
