using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfBingo.Models;

/// <summary>
/// 単一のビンゴ番号と抽選状態を表すモデル。
/// </summary>
/// <param name="value">ビンゴ番号。</param>
public class BingoNumber(int value) : INotifyPropertyChanged
{
    private bool _isDrawn;

    /// <summary>番号の数値。</summary>
    public int Value { get; } = value;

    /// <summary>抽選済みかどうか。</summary>
    public bool IsDrawn
    {
        get => _isDrawn;
        set
        {
            if (_isDrawn == value) return;
            _isDrawn = value;
            OnPropertyChanged();
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>プロパティ変更通知を送信します。</summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

/// <summary>
/// 10刻みの番号グループを表します。
/// </summary>
public class BingoNumberGroup
{
    /// <summary>グループを識別するラベル。</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>所属する番号リスト。</summary>
    public ObservableCollection<BingoNumber> Numbers { get; } = [];
}

/// <summary>
/// 列単位で番号を保持するモデル。
/// </summary>
public class BingoNumberColumn
{
    /// <summary>列のラベル (範囲や BINGO 文字)。</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>列に含まれる番号。</summary>
    public ObservableCollection<BingoNumber> Numbers { get; } = [];
}
