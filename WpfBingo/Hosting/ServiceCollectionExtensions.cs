using Microsoft.Extensions.DependencyInjection;
using WpfBingo.ViewModels;
using WpfBingo.Views;

namespace WpfBingo.Hosting;

/// <summary>
/// DI コンテナへのサービス登録を拡張するメソッド群です。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// アプリケーションで使用する ViewModel を登録します。
    /// </summary>
    /// <param name="services">サービスコレクション。</param>
    /// <returns>チェーン呼び出し用のサービスコレクション。</returns>
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<BingoViewModel>();
        return services;
    }

    /// <summary>
    /// アプリケーションで使用する View（ウィンドウ）を登録します。
    /// </summary>
    /// <param name="services">サービスコレクション。</param>
    /// <returns>チェーン呼び出し用のサービスコレクション。</returns>
    public static IServiceCollection AddViews(this IServiceCollection services)
    {
        services.AddTransient<MainWindow>();
        return services;
    }

    /// <summary>
    /// アプリケーションの全サービスを一括登録します。
    /// </summary>
    /// <param name="services">サービスコレクション。</param>
    /// <returns>チェーン呼び出し用のサービスコレクション。</returns>
    public static IServiceCollection AddWpfBingoServices(this IServiceCollection services)
    {
        services.AddViewModels();
        services.AddViews();
        return services;
    }
}
