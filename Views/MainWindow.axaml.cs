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


namespace MyNotepad.Views;

public partial class MainWindow : Window
{
	private bool newFile = true;
	private string RawText = "";
	private string? LastFile;
	private string selectedTextBeforeLosingFocus = "";
	private int selectionStartBeforeLosingFocus = 0;
	private int selectionLengthBeforeLosingFocus = 0;
	private bool bold, italic, underline, strikethrough;


	public MainWindow()
	{
		InitializeComponent();

		BoldButton.AddHandler(InputElement.PointerPressedEvent, ControlButton_PointerPressed, RoutingStrategies.Tunnel);
		ItalicButton.AddHandler(InputElement.PointerPressedEvent, ControlButton_PointerPressed, RoutingStrategies.Tunnel);
		UnderLineButton.AddHandler(InputElement.PointerPressedEvent, ControlButton_PointerPressed, RoutingStrategies.Tunnel);
	}

	private void ControlButton_PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed == true)
		{
			selectedTextBeforeLosingFocus = BaseTextBox.SelectedText;
			selectionStartBeforeLosingFocus = BaseTextBox.SelectionStart;
			selectionLengthBeforeLosingFocus = BaseTextBox.SelectedText.Length;
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

	[Obsolete("Use Window.StorageProvider API or TopLevel.StorageProvider API")]
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
					await File.WriteAllTextAsync(LastFile, BaseTextBlock.Text);
				}
				catch
				{
					// Exception intentionally ignored
				}
			}
		}
		else if (!string.IsNullOrEmpty(LastFile))
		{
			await File.WriteAllTextAsync(LastFile, BaseTextBlock.Text);
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

	private void BaseTextBox_LostFocus(object? sender, RoutedEventArgs e)
	{
		RawText = BaseTextBox.Text ?? "";
		ApplyFormattedText(BaseTextBlock, RawText);
		BaseTextBox.IsVisible = false;
		BaseTextBlock.IsVisible = true;
	}

	private void ApplyFormattedText(TextBlock textBlock, string content)
	{
		textBlock.Inlines.Clear();
		
		var boldParts = Regex.Split(content, @"(\*[^*]+\*)");
		var italicParts = Regex.Split(content, @"(_[^_]+_)");
		var underlineParts = Regex.Split(content, @"(\^[^^]+\^)");

		if (bold)
		{
			foreach (var part in boldParts)
			{
				if (Regex.IsMatch(part, @"^\*[^*]+\*$"))
				{
					string bold = part.Trim('*');
					textBlock.Inlines.Add(new Run { Text = bold, FontWeight = Avalonia.Media.FontWeight.Bold });
				}
				else if (Regex.IsMatch(part, @"^_[^_]+_$"))
				{
					string italic = part.Trim('_');
					textBlock.Inlines.Add(new Run { Text = italic, FontStyle = Avalonia.Media.FontStyle.Italic });
				}
				else
				{
					textBlock.Inlines.Add(new Run { Text = part });
				}
			}
		}
		if (italic)
		{
			foreach (var part in italicParts)
			{
				if (Regex.IsMatch(part, @"^_[^_]+_$"))
				{
					string italic = part.Trim('_');
					textBlock.Inlines.Add(new Run { Text = italic, FontStyle = Avalonia.Media.FontStyle.Italic });
				}
				else
				{
					textBlock.Inlines.Add(new Run { Text = part });
				}
			}
		}
		if (underline)
		{
			foreach (var part in underlineParts)
			{
				if (Regex.IsMatch(part, @"^\^[^^]+\^$"))
				{
					string underline = part.Trim('^');
					textBlock.Inlines.Add(new Run
					{
						Text = underline,
						TextDecorations = TextDecorations.Underline
					});
				}
				else
				{
					textBlock.Inlines.Add(new Run { Text = part });
				}
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

	private void BoldText_Button_Click(object? sender, RoutedEventArgs e)
	{
		bold = true;
		italic = false;
		underline = false;

		BaseTextBox.SelectionStart = selectionStartBeforeLosingFocus ;
		BaseTextBox.SelectionEnd = selectionStartBeforeLosingFocus - selectionLengthBeforeLosingFocus;

		string boldText = $"*{selectedTextBeforeLosingFocus}*";

		BaseTextBox.SelectedText = boldText;

		selectionStartBeforeLosingFocus = BaseTextBox.SelectionStart;
		selectionLengthBeforeLosingFocus = selectedTextBeforeLosingFocus.Length;

		BaseTextBox_LostFocus(sender, new RoutedEventArgs());
	}

	private void ItalicText_Button_Click(object? sender, RoutedEventArgs e)
	{
		bold = false;
		italic = true;
		underline = false;

		BaseTextBox.SelectionStart = selectionStartBeforeLosingFocus;
		BaseTextBox.SelectionEnd = selectionStartBeforeLosingFocus - selectionLengthBeforeLosingFocus;

		string italicText = $"_{selectedTextBeforeLosingFocus}_";

		BaseTextBox.SelectedText = italicText;

		selectionStartBeforeLosingFocus = BaseTextBox.SelectionStart;
		selectionLengthBeforeLosingFocus = selectedTextBeforeLosingFocus.Length;

		BaseTextBox_LostFocus(sender, new RoutedEventArgs());
	}

	private void UnderLineText_Button_Click(object? sender, RoutedEventArgs e)
	{
		bold = false;
		italic = false;
		underline = true;

		BaseTextBox.SelectionStart = selectionStartBeforeLosingFocus;
		BaseTextBox.SelectionEnd = selectionStartBeforeLosingFocus - selectionLengthBeforeLosingFocus;

		string underLineText = $"^{selectedTextBeforeLosingFocus}^";

		BaseTextBox.SelectedText = underLineText;

		selectionStartBeforeLosingFocus = BaseTextBox.SelectionStart;
		selectionLengthBeforeLosingFocus = selectedTextBeforeLosingFocus.Length;

		BaseTextBox_LostFocus(sender, new RoutedEventArgs());
	}

}
