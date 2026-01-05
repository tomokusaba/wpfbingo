using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WpfBingo.Views;

namespace WpfBingo.Hosting;

/// <summary>
/// WPF アプリケーションのライフサイクルを管理する HostedService です。
/// </summary>
/// <param name="serviceProvider">DI コンテナのサービスプロバイダー。</param>
public sealed class WpfHostedService(IServiceProvider serviceProvider) : IHostedService
{
    private MainWindow? _mainWindow;

    /// <summary>
    /// ホスト開始時にメインウィンドウを表示します。
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        _mainWindow.Show();
        return Task.CompletedTask;
    }

    /// <summary>
    /// ホスト停止時のクリーンアップを行います。
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _mainWindow?.Close();
        return Task.CompletedTask;
    }
}
