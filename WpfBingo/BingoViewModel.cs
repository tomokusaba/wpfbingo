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
    private double _currentNumberFontSize = 120; // responsive font size
    private int _rowsPerColumn = 10;
    private double _numberCellSize = 40;
    private double _numberFontSize = 16;

    public BingoViewModel()
    {
        DrawnNumbers = new ObservableCollection<int>();
        AllNumbers = new ObservableCollection<BingoNumber>();
        GroupedNumbers = new ObservableCollection<BingoNumberGroup>();
        Columns = new ObservableCollection<BingoNumberColumn>();
        DrawNumberCommand = new RelayCommand(DrawNumber, CanDrawNumber);
        ResetCommand = new RelayCommand(Reset);
        
        Reset();
    }

    public ObservableCollection<int> DrawnNumbers { get; }
    public ObservableCollection<BingoNumber> AllNumbers { get; }
    public ObservableCollection<BingoNumberGroup> GroupedNumbers { get; }
    public ObservableCollection<BingoNumberColumn> Columns { get; }
    public ObservableCollection<BingoNumberColumn> DynamicColumns { get; } = new();

    public string CurrentNumberDisplay
    {
        get => _currentNumberDisplay;
        set { _currentNumberDisplay = value; OnPropertyChanged(); }
    }

    public int? CurrentNumber
    {
        get => _currentNumber;
        set { _currentNumber = value; CurrentNumberDisplay = value?.ToString() ?? "?"; OnPropertyChanged(); }
    }

    public double CurrentNumberFontSize
    {
        get => _currentNumberFontSize;
        set { if (Math.Abs(_currentNumberFontSize - value) < 0.1) return; _currentNumberFontSize = value; OnPropertyChanged(); }
    }

    public double NumberCellSize { get => _numberCellSize; private set { if (Math.Abs(_numberCellSize - value) < 0.5) return; _numberCellSize = value; OnPropertyChanged(); } }
    public double NumberFontSize { get => _numberFontSize; private set { if (Math.Abs(_numberFontSize - value) < 0.5) return; _numberFontSize = value; OnPropertyChanged(); } }

    public void AdjustCurrentNumberFontSize(double width, double height)
    {
        var minSide = Math.Min(width, height);
        var newSize = Math.Clamp(minSide * 0.55, 60, 320);
        CurrentNumberFontSize = newSize;
    }

    public ICommand DrawNumberCommand { get; }
    public ICommand ResetCommand { get; }

    private bool CanDrawNumber() => _availableNumbers.Count > 0;

    private void DrawNumber()
    {
        if (_availableNumbers.Count == 0) return;
        var index = _random.Next(_availableNumbers.Count);
        var number = _availableNumbers[index];
        _availableNumbers.RemoveAt(index);
        CurrentNumber = number;
        DrawnNumbers.Insert(0, number);
        AllNumbers[number - 1].IsDrawn = true; // same instance shared inside groups so it updates
        ((RelayCommand)DrawNumberCommand).RaiseCanExecuteChanged();
    }

    public void UpdateLayout(double width, double height)
    {
        double horizontalMargin = 80; // approximate outer margins
        double verticalReserved = 320; // title + current number area
        double availableWidth = Math.Max(200, width - horizontalMargin);
        double availableHeight = Math.Max(200, height - verticalReserved);
        double spacing = 6; // item margin approximation
        double columnPadding = 12; // internal padding per column

        // Evaluate both layout options (5 or 10 rows)
        (int rows, int cols, double cellSize) Eval(int r)
        {
            int colsNeeded = (int)Math.Ceiling(75.0 / r);
            double cellSizeHeight = (availableHeight - columnPadding) / r - spacing;
            double cellSizeWidth = (availableWidth - (colsNeeded * spacing)) / colsNeeded - spacing;
            double size = Math.Max(24, Math.Min(cellSizeHeight, cellSizeWidth));
            return (r, colsNeeded, size);
        }
        var opt5 = Eval(5);
        var opt10 = Eval(10);
        var chosen = opt5.cellSize > opt10.cellSize ? opt5 : opt10;
        if (chosen.rows != _rowsPerColumn)
        {
            _rowsPerColumn = chosen.rows;
            RebuildDynamicColumns();
        }
        NumberCellSize = chosen.cellSize;
        NumberFontSize = Math.Clamp(chosen.cellSize * 0.42, 12, 48);
    }

    private void RebuildDynamicColumns()
    {
        DynamicColumns.Clear();
        int rows = _rowsPerColumn;
        int total = AllNumbers.Count;
        int colCount = (int)Math.Ceiling(total / (double)rows);
        for (int c = 0; c < colCount; c++)
        {
            int startIndex = c * rows;
            int endIndex = Math.Min(startIndex + rows - 1, total - 1);
            var col = new BingoNumberColumn
            {
                Label = AllNumbers[startIndex].Value.ToString("00") + "-" + AllNumbers[endIndex].Value.ToString("00")
            };
            for (int i = startIndex; i <= endIndex; i++) col.Numbers.Add(AllNumbers[i]);
            DynamicColumns.Add(col);
        }
        OnPropertyChanged(nameof(DynamicColumns));
    }

    private void Reset()
    {
        _availableNumbers.Clear();
        DrawnNumbers.Clear();
        AllNumbers.Clear();
        GroupedNumbers.Clear();
        Columns.Clear();
        DynamicColumns.Clear();

        for (int i = 1; i <= 75; i++)
        {
            _availableNumbers.Add(i);
            var bn = new BingoNumber(i);
            AllNumbers.Add(bn);
        }

        // Populate tens groups (legacy, can be removed later)
        int current = 1;
        while (current <= 75)
        {
            int start = current;
            int end = Math.Min(((current - 1) / 10 + 1) * 10, 75); // end of tens or 75
            var group = new BingoNumberGroup
            {
                Label = start.ToString("00") + "-" + end.ToString("00")
            };
            for (int n = start; n <= end; n++)
            {
                group.Numbers.Add(AllNumbers[n - 1]);
            }
            GroupedNumbers.Add(group);
            current = end + 1;
        }

        // Populate vertical bingo columns (1-15,16-30,...)
        int colStart = 1;
        while (colStart <= 75)
        {
            int colEnd = Math.Min(colStart + 14, 75);
            var col = new BingoNumberColumn { Label = colStart + "-" + colEnd }; // label, could be B,I,N,G,O
            for (int n = colStart; n <= colEnd; n++) col.Numbers.Add(AllNumbers[n - 1]);
            Columns.Add(col);
            colStart = colEnd + 1;
        }

        // Build initial dynamic columns
        RebuildDynamicColumns();

        CurrentNumber = null;
        ((RelayCommand)DrawNumberCommand).RaiseCanExecuteChanged();
        UpdateLayout(1000, 800); // initial sizing
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class BingoNumberGroup
{
    public string Label { get; set; } = string.Empty;
    public ObservableCollection<BingoNumber> Numbers { get; } = new();
}

public class BingoNumberColumn
{
    public string Label { get; set; } = string.Empty;
    public ObservableCollection<BingoNumber> Numbers { get; } = new();
}

public class BingoNumber : INotifyPropertyChanged
{
    private bool _isDrawn;
    public int Value { get; }
    public bool IsDrawn
    {
        get => _isDrawn;
        set { if (_isDrawn == value) return; _isDrawn = value; OnPropertyChanged(); }
    }
    public BingoNumber(int value) => Value = value;
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;
    public RelayCommand(Action execute, Func<bool>? canExecute = null) { _execute = execute ?? throw new ArgumentNullException(nameof(execute)); _canExecute = canExecute; }
    public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
