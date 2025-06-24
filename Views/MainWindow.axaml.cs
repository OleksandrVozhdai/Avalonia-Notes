using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Collections.Generic;
using Avalonia.Controls.ApplicationLifetimes;
using System.IO;
using Avalonia;
using System;

namespace MyNotepad.Views;

public partial class MainWindow : Window
{
	public bool newFile = true;
	public string? LastFile; // Nullable to satisfy CS8618

	public MainWindow()
	{
		InitializeComponent();
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
			BaseTextBox.Text = content;
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
}
