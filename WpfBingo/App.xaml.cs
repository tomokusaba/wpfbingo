using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WpfBingo;

/// <summary>
/// Generic Host を利用してアプリケーションのライフサイクルと DI を管理するエントリポイントです。
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    /// <summary>
    /// ホストを構築し、サービスを登録します。
    /// </summary>
    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // ViewModel をシングルトン登録
                services.AddSingleton<BingoViewModel>();

                // MainWindow をトランジェントで登録（必要に応じてシングルトンに変更可）
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    /// <summary>
    /// サービスプロバイダーを公開し、任意の場所からサービスを解決できるようにします。
    /// </summary>
    public static IServiceProvider Services => ((App)Current)._host.Services;

    /// <inheritdoc />
    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    /// <inheritdoc />
    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }

        base.OnExit(e);
    }
}
