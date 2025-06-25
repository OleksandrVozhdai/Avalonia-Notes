using Avalonia.Media;

namespace MyNotepad.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	private IBrush _backgroundColorUpperPanel = new SolidColorBrush(Color.Parse("#171a1f"));
	private IBrush _backgroundMainGrid = new SolidColorBrush(Color.Parse("#23272f"));
	private IBrush _textBoxBackgroundColor = new SolidColorBrush(Color.Parse("#1c1f26"));
	private IBrush _foregroundColor = new SolidColorBrush(Color.Parse("#ffffff"));

	public IBrush BackgroundColorUpperPanel
	{
		get => _backgroundColorUpperPanel;
		set
		{
			if (_backgroundColorUpperPanel != value)
			{
				_backgroundColorUpperPanel = value;
				OnPropertyChanged(nameof(BackgroundColorUpperPanel));
			}
		}
	}
	public IBrush BackgroundMainGrid
	{
		get => _backgroundMainGrid;
		set
		{
			if (_backgroundMainGrid != value)
			{
				_backgroundMainGrid = value;
				OnPropertyChanged(nameof(BackgroundMainGrid));
			}
		}
	}
	public IBrush TextBoxBackgroundColor
	{
		get => _textBoxBackgroundColor;
		set
		{
			if (_textBoxBackgroundColor != value)
			{
				_textBoxBackgroundColor = value;
				OnPropertyChanged(nameof(TextBoxBackgroundColor));
			}
		}
	}
	public IBrush ForegroundColor
	{
		get => _foregroundColor;
		set
		{
			if (_foregroundColor != value)
			{
				_foregroundColor = value;
				OnPropertyChanged(nameof(ForegroundColor));
			}
		}
	}


}
