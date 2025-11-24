using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace WpfBingo;

public class BingoViewModel : INotifyPropertyChanged
{
    private readonly Random _random = new();
    private readonly List<int> _availableNumbers = [];
    private int? _currentNumber;
    private string _currentNumberDisplay = "?";

    public BingoViewModel()
    {
        DrawnNumbers = new ObservableCollection<int>();
        DrawNumberCommand = new RelayCommand(DrawNumber, CanDrawNumber);
        ResetCommand = new RelayCommand(Reset);
        
        Reset();
    }

    public ObservableCollection<int> DrawnNumbers { get; }

    public string CurrentNumberDisplay
    {
        get => _currentNumberDisplay;
        set
        {
            _currentNumberDisplay = value;
            OnPropertyChanged();
        }
    }

    public int? CurrentNumber
    {
        get => _currentNumber;
        set
        {
            _currentNumber = value;
            CurrentNumberDisplay = value?.ToString() ?? "?";
            OnPropertyChanged();
        }
    }

    public ICommand DrawNumberCommand { get; }
    public ICommand ResetCommand { get; }

    private bool CanDrawNumber()
    {
        return _availableNumbers.Count > 0;
    }

    private void DrawNumber()
    {
        if (_availableNumbers.Count == 0)
            return;

        var index = _random.Next(_availableNumbers.Count);
        var number = _availableNumbers[index];
        _availableNumbers.RemoveAt(index);

        CurrentNumber = number;
        DrawnNumbers.Insert(0, number);

        ((RelayCommand)DrawNumberCommand).RaiseCanExecuteChanged();
    }

    private void Reset()
    {
        _availableNumbers.Clear();
        DrawnNumbers.Clear();
        
        // Standard bingo uses numbers 1-75
        for (int i = 1; i <= 75; i++)
        {
            _availableNumbers.Add(i);
        }

        CurrentNumber = null;
        ((RelayCommand)DrawNumberCommand).RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute();
    }

    public void Execute(object? parameter)
    {
        _execute();
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
