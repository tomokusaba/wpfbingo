using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WpfBingo.Models;

namespace WpfBingo.ViewModels;

/// <summary>
/// ビンゴ抽選機の状態と UI 更新を司るメイン ViewModel。
/// </summary>
public class BingoViewModel : INotifyPropertyChanged
{
    private readonly Random _random = new();
    private readonly List<int> _availableNumbers = [];
    private int? _currentNumber;
    private string _currentNumberDisplay = "?";
    private double _currentNumberFontSize = 120;
    private int _rowsPerColumn = 10;
    private double _numberCellSize = 40;
    private double _numberFontSize = 16;
    private bool _isDrawing;

    /// <summary>
    /// ViewModel を初期化し、コマンドと番号リストをセットアップします。
    /// </summary>
    public BingoViewModel()
    {
        DrawnNumbers = [];
        AllNumbers = [];
        GroupedNumbers = [];
        Columns = [];
        DrawNumberCommand = new RelayCommand(DrawNumber, CanDrawNumber);
        ResetCommand = new RelayCommand(Reset);
        
        Reset();
    }

    /// <summary>抽選済み番号の履歴（新しい順）。</summary>
    public ObservableCollection<int> DrawnNumbers { get; }

    /// <summary>1～75 の全ビンゴ番号。</summary>
    public ObservableCollection<BingoNumber> AllNumbers { get; }

    /// <summary>10刻みのグループ表示用コレクション。</summary>
    public ObservableCollection<BingoNumberGroup> GroupedNumbers { get; }

    /// <summary>BINGO 列（1-15,16-30...）の静的コレクション。</summary>
    public ObservableCollection<BingoNumberColumn> Columns { get; }

    /// <summary>画面幅に応じて行数が変わる動的列。</summary>
    public ObservableCollection<BingoNumberColumn> DynamicColumns { get; } = [];

    /// <summary>現在表示されている番号文字列。</summary>
    public string CurrentNumberDisplay
    {
        get => _currentNumberDisplay;
        set { _currentNumberDisplay = value; OnPropertyChanged(); }
    }

    /// <summary>直近の抽選番号。未抽選時は null。</summary>
    public int? CurrentNumber
    {
        get => _currentNumber;
        set { _currentNumber = value; CurrentNumberDisplay = value?.ToString() ?? "?"; OnPropertyChanged(); }
    }

    /// <summary>中央表示用番号のフォントサイズ。</summary>
    public double CurrentNumberFontSize
    {
        get => _currentNumberFontSize;
        set { if (Math.Abs(_currentNumberFontSize - value) < 0.1) return; _currentNumberFontSize = value; OnPropertyChanged(); }
    }

    /// <summary>番号セルの一辺サイズ。</summary>
    public double NumberCellSize
    {
        get => _numberCellSize;
        private set { if (Math.Abs(_numberCellSize - value) < 0.5) return; _numberCellSize = value; OnPropertyChanged(); }
    }

    /// <summary>セル内のフォントサイズ。</summary>
    public double NumberFontSize
    {
        get => _numberFontSize;
        private set { if (Math.Abs(_numberFontSize - value) < 0.5) return; _numberFontSize = value; OnPropertyChanged(); }
    }

    /// <summary>抽選中かどうかを示すフラグ。</summary>
    public bool IsDrawing
    {
        get => _isDrawing;
        set
        {
            if (_isDrawing == value) return;
            _isDrawing = value;
            OnPropertyChanged();
            ((RelayCommand)DrawNumberCommand).RaiseCanExecuteChanged();
        }
    }

    /// <summary>番号を一つ抽選するためのコマンド。</summary>
    public ICommand DrawNumberCommand { get; }

    /// <summary>状態を初期化するコマンド。</summary>
    public ICommand ResetCommand { get; }

    /// <summary>
    /// 中央番号エリアのサイズに合わせてフォントサイズを調整します。
    /// </summary>
    /// <param name="width">領域の幅。</param>
    /// <param name="height">領域の高さ。</param>
    public void AdjustCurrentNumberFontSize(double width, double height)
    {
        var minSide = Math.Min(width, height);
        var newSize = Math.Clamp(minSide * 0.55, 60, 320);
        CurrentNumberFontSize = newSize;
    }

    /// <summary>
    /// ルーレット用に利用可能な番号の配列を取得します。
    /// </summary>
    /// <returns>シャッフルされた利用可能番号の配列。</returns>
    public int[] GetAvailableNumbersForRoulette()
    {
        var numbers = _availableNumbers.ToArray();
        // シャッフル（Fisher-Yates）
        for (int i = numbers.Length - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
        }
        return numbers;
    }

    /// <summary>
    /// 抽選中フラグを設定し、ルーレットアニメーション開始を通知します。
    /// </summary>
    public void StartDrawAnimation()
    {
        if (_availableNumbers.Count == 0) return;
        IsDrawing = true;
    }

    /// <summary>
    /// ルーレットで決定された番号を確定し、状態を更新します。
    /// </summary>
    /// <param name="number">確定する番号。</param>
    public void ConfirmDrawnNumber(int number)
    {
        if (!_availableNumbers.Contains(number)) return;
        
        _availableNumbers.Remove(number);
        CurrentNumber = number;
        DrawnNumbers.Insert(0, number);
        AllNumbers[number - 1].IsDrawn = true;
        IsDrawing = false;
        ((RelayCommand)DrawNumberCommand).RaiseCanExecuteChanged();
    }

    /// <summary>
    /// ウィンドウサイズに合わせて行数やセルサイズを再計算します。
    /// </summary>
    /// <param name="width">利用可能幅。</param>
    /// <param name="height">利用可能高さ。</param>
    public void UpdateLayout(double width, double height)
    {
        double horizontalMargin = 80;
        double verticalReserved = 320;
        double availableWidth = Math.Max(200, width - horizontalMargin);
        double availableHeight = Math.Max(200, height - verticalReserved);
        double spacing = 6;
        double columnPadding = 12;

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

    private bool CanDrawNumber() => _availableNumbers.Count > 0 && !_isDrawing;

    private void DrawNumber()
    {
        if (_availableNumbers.Count == 0 || _isDrawing) return;
        StartDrawAnimation();
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
            for (int i = startIndex; i <= endIndex; i++)
            {
                col.Numbers.Add(AllNumbers[i]);
            }
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

        // 10刻みグループ作成
        int current = 1;
        while (current <= 75)
        {
            int start = current;
            int end = Math.Min(((current - 1) / 10 + 1) * 10, 75);
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

        // BINGO列（1-15, 16-30, ...）作成
        int colStart = 1;
        while (colStart <= 75)
        {
            int colEnd = Math.Min(colStart + 14, 75);
            var col = new BingoNumberColumn { Label = colStart + "-" + colEnd };
            for (int n = colStart; n <= colEnd; n++)
            {
                col.Numbers.Add(AllNumbers[n - 1]);
            }
            Columns.Add(col);
            colStart = colEnd + 1;
        }

        RebuildDynamicColumns();
        CurrentNumber = null;
        ((RelayCommand)DrawNumberCommand).RaiseCanExecuteChanged();
        UpdateLayout(1000, 800);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// プロパティ変更イベントを発火します。
    /// </summary>
    /// <param name="propertyName">変更されたプロパティ名。</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
