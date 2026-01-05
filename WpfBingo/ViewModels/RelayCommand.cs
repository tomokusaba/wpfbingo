using System.Windows.Input;

namespace WpfBingo.ViewModels;

/// <summary>
/// シンプルな <see cref="ICommand"/> 実装。
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    /// <summary>
    /// 実行デリゲートと条件デリゲートでコマンドを初期化します。
    /// </summary>
    /// <param name="execute">コマンド実行時のアクション。</param>
    /// <param name="canExecute">実行可否を判定する関数。</param>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _canExecute is null || _canExecute();

    /// <inheritdoc />
    public void Execute(object? parameter) => _execute();

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// CanExecute の変更を通知します。
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
