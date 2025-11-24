using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls; // Added for Canvas

namespace WpfBingo;

/// <summary>
/// UI ルートを司り、抽選時の各種アニメーションとコンフェッティを制御するメインウィンドウです。
/// </summary>
public partial class MainWindow : Window
{
    private readonly BingoViewModel _viewModel;
    private readonly Random _rand = new();
    private readonly ConfettiProfile _defaultConfettiProfile;
    private readonly Dictionary<string, ConfettiProfile> _confettiProfiles;
    private int _lastBackgroundPatternIndex = -1;

    /// <summary>
    /// 依存リソースの初期化と ViewModel とのバインドを行います。
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new BingoViewModel();
        DataContext = _viewModel;

        _defaultConfettiProfile = new ConfettiProfile
        {
            Count = 150,
            SizeMin = 8,
            SizeMax = 16,
            DurationMin = 1.2,
            DurationMax = 1.9,
            DelayMax = 0.25,
            HorizontalDrift = 180,
            VerticalOvershoot = 240,
            StartOffsetMin = 40,
            StartOffsetMax = 160,
            OpacityMin = 0.82,
            OpacityMax = 1.0,
            RotationMin = 180,
            RotationMax = 540,
            Colors = new[] { "#FFEB3B", "#FF9800", "#FF7043", "#FFF176" },
            Shapes = new[] { ConfettiShape.Rectangle, ConfettiShape.Circle, ConfettiShape.Triangle }
        };

        _confettiProfiles = new Dictionary<string, ConfettiProfile>
        {
            ["BackgroundAnimationPattern1"] = _defaultConfettiProfile,
            ["BackgroundAnimationPattern2"] = new ConfettiProfile
            {
                Count = 120,
                SizeMin = 10,
                SizeMax = 18,
                DurationMin = 1.6,
                DurationMax = 2.4,
                DelayMax = 0.35,
                HorizontalDrift = 140,
                VerticalOvershoot = 200,
                StartOffsetMin = 30,
                StartOffsetMax = 140,
                OpacityMin = 0.7,
                OpacityMax = 0.95,
                RotationMin = 120,
                RotationMax = 420,
                Colors = new[] { "#E1F5FE", "#B3E5FC", "#4FC3F7", "#80DEEA", "#E0F7FA" },
                Shapes = new[] { ConfettiShape.Diamond, ConfettiShape.Circle, ConfettiShape.Bar }
            },
            ["BackgroundAnimationPattern3"] = new ConfettiProfile
            {
                Count = 90,
                SizeMin = 12,
                SizeMax = 22,
                DurationMin = 1.4,
                DurationMax = 2.1,
                DelayMax = 0.2,
                HorizontalDrift = 240,
                VerticalOvershoot = 260,
                StartOffsetMin = 60,
                StartOffsetMax = 180,
                OpacityMin = 0.8,
                OpacityMax = 1.0,
                RotationMin = 90,
                RotationMax = 360,
                Colors = new[] { "#F8BBD0", "#FFCDD2", "#F48FB1", "#E1BEE7" },
                Shapes = new[] { ConfettiShape.Bar, ConfettiShape.Rectangle, ConfettiShape.Circle }
            },
            ["BackgroundAnimationPattern4"] = new ConfettiProfile
            {
                Count = 70,
                SizeMin = 16,
                SizeMax = 28,
                DurationMin = 1.3,
                DurationMax = 1.9,
                DelayMax = 0.18,
                HorizontalDrift = 260,
                VerticalOvershoot = 300,
                StartOffsetMin = 70,
                StartOffsetMax = 200,
                OpacityMin = 0.85,
                OpacityMax = 1.0,
                RotationMin = 240,
                RotationMax = 720,
                Colors = new[] { "#FFF8E1", "#FFE082", "#FFD54F", "#FFECB3" },
                Shapes = new[] { ConfettiShape.Diamond, ConfettiShape.Triangle, ConfettiShape.Bar }
            },
            ["BackgroundAnimationPattern5"] = new ConfettiProfile
            {
                Count = 180,
                SizeMin = 6,
                SizeMax = 14,
                DurationMin = 1.1,
                DurationMax = 1.6,
                DelayMax = 0.28,
                HorizontalDrift = 200,
                VerticalOvershoot = 220,
                StartOffsetMin = 30,
                StartOffsetMax = 120,
                OpacityMin = 0.75,
                OpacityMax = 0.98,
                RotationMin = 150,
                RotationMax = 540,
                Colors = new[] { "#69F0AE", "#00E5FF", "#FF80AB", "#F4FF81", "#B388FF" },
                Shapes = new[] { ConfettiShape.Circle, ConfettiShape.Rectangle, ConfettiShape.Triangle, ConfettiShape.Diamond }
            }
        };

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(BingoViewModel.CurrentNumber) && _viewModel.CurrentNumber.HasValue)
            {
                var backgroundKey = GetNextBackgroundPatternKey();
                var confettiProfile = GetConfettiProfile(backgroundKey);

                OverlayNumberText.Text = _viewModel.CurrentNumberDisplay;
                OverlayGrid.Visibility = Visibility.Visible;
                SpawnConfetti(confettiProfile);

                // Select random patterns
                var overlayKey = _overlayPatterns[_rand.Next(_overlayPatterns.Length)];
                var pulseKey = _pulsePatterns[_rand.Next(_pulsePatterns.Length)];

                // Start overlay animation
                var overlayStoryboard = (Storyboard)FindResource(overlayKey);
                overlayStoryboard.Completed += OverlayStoryboard_Completed;
                overlayStoryboard.Begin();

                // Start pulse animation
                var pulseStoryboard = (Storyboard)FindResource(pulseKey);
                pulseStoryboard.Begin();

                // Prepare background effect visibility resets
                ResetBackgroundEffects();
                if (!string.IsNullOrEmpty(backgroundKey))
                {
                    var bgStoryboard = (Storyboard)FindResource(backgroundKey);
                    bgStoryboard.Begin();
                }
            }
        };
    }

    /// <summary>
    /// 背景パターンキーに紐づくコンフェッティ設定を解決します。
    /// </summary>
    /// <param name="backgroundKey">背景アニメーションのリソースキー。</param>
    /// <returns>一致する <see cref="ConfettiProfile"/>。見つからない場合は既定値。</returns>
    private ConfettiProfile GetConfettiProfile(string backgroundKey)
    {
        if (!string.IsNullOrEmpty(backgroundKey) && _confettiProfiles.TryGetValue(backgroundKey, out var profile))
        {
            return profile;
        }

        return _defaultConfettiProfile;
    }

    /// <summary>
    /// 前回と異なる背景アニメーションキーをランダムに選択します。
    /// </summary>
    private string GetNextBackgroundPatternKey()
    {
        if (_backgroundPatterns.Length == 0)
        {
            return string.Empty;
        }

        var index = _rand.Next(_backgroundPatterns.Length);
        if (_backgroundPatterns.Length > 1)
        {
            while (index == _lastBackgroundPatternIndex)
            {
                index = _rand.Next(_backgroundPatterns.Length);
            }
        }

        _lastBackgroundPatternIndex = index;
        return _backgroundPatterns[index];
    }

    /// <summary>
    /// 背景演出要素を非表示状態に戻し、再生準備を行います。
    /// </summary>
    private void ResetBackgroundEffects()
    {
        // Ensure elements start hidden before animation
        if (BgColorRect.Fill is SolidColorBrush colorBrush)
        {
            colorBrush.Color = (Color)ColorConverter.ConvertFromString("#F5F5F5")!;
        }

        BgRadialFlash.Opacity = 0;
        BgRadialFlash.RenderTransform = new ScaleTransform(0, 0);

        BgGradientSweep.Opacity = 0;
        BgGradientSweep.RenderTransform = new TranslateTransform(-1000, 0);

        BgShimmer.Opacity = 0;
        BgShimmer.RenderTransform = new TranslateTransform(-800, 0);

        BgPulseRing.Opacity = 0;
        BgPulseRing.RenderTransform = new ScaleTransform(0.3, 0.3);

        BgDiagonalGlow.Opacity = 0;
        BgDiagonalGlow.RenderTransform = new TranslateTransform(-1200, 0);
    }

    private readonly string[] _overlayPatterns =
    {
        "OverlayAnimationPattern1",
        "OverlayAnimationPattern2",
        "OverlayAnimationPattern3",
        "OverlayAnimationPattern4",
        "OverlayAnimationPattern5"
    };

    private readonly string[] _pulsePatterns =
    {
        "NumberPulsePattern1",
        "NumberPulsePattern2",
        "NumberPulsePattern3",
        "NumberPulsePattern4",
        "NumberPulsePattern5"
    };

    private readonly string[] _backgroundPatterns =
    {
        "BackgroundAnimationPattern1",
        "BackgroundAnimationPattern2",
        "BackgroundAnimationPattern3",
        "BackgroundAnimationPattern4",
        "BackgroundAnimationPattern5"
    };

    /// <summary>
    /// オーバーレイアニメーション終了時に可視状態とコンフェッティをリセットします。
    /// </summary>
    private void OverlayStoryboard_Completed(object? sender, EventArgs e)
    {
        OverlayGrid.Visibility = Visibility.Collapsed;
        ConfettiCanvas.Children.Clear();
        if (sender is Storyboard sb) sb.Completed -= OverlayStoryboard_Completed;
    }

    /// <summary>
    /// 指定プロファイルに従ったコンフェッティを生成・落下させます。
    /// </summary>
    /// <param name="profile">描画に使用するプロファイル。</param>
    private void SpawnConfetti(ConfettiProfile profile)
    {
        ConfettiCanvas.Children.Clear();
        var width = ActualWidth;
        var height = ActualHeight;
        if (width <= 0 || height <= 0) { width = 1000; height = 800; }

        var shapes = profile.Shapes.Length > 0 ? profile.Shapes : _defaultConfettiProfile.Shapes;
        var colors = profile.Colors.Length > 0 ? profile.Colors : _defaultConfettiProfile.Colors;

        for (int i = 0; i < profile.Count; i++)
        {
            var sizeRange = Math.Max(0.1, profile.SizeMax - profile.SizeMin);
            var size = profile.SizeMin + _rand.NextDouble() * sizeRange;
            var shapeType = shapes[_rand.Next(shapes.Length)];
            var shape = CreateConfettiShape(shapeType, size);

            var colorHex = colors[_rand.Next(colors.Length)];
            shape.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)!);
            var opacityRange = Math.Max(0.01, profile.OpacityMax - profile.OpacityMin);
            shape.Opacity = profile.OpacityMin + _rand.NextDouble() * opacityRange;

            var startX = _rand.NextDouble() * width;
            var startOffsetRange = Math.Max(1, profile.StartOffsetMax - profile.StartOffsetMin);
            var startY = -(profile.StartOffsetMin + _rand.NextDouble() * startOffsetRange);
            Canvas.SetLeft(shape, startX);
            Canvas.SetTop(shape, startY);
            ConfettiCanvas.Children.Add(shape);

            var fallDistance = height + profile.VerticalOvershoot;
            var durationRange = Math.Max(0.1, profile.DurationMax - profile.DurationMin);
            var durationSeconds = profile.DurationMin + _rand.NextDouble() * durationRange;
            var duration = TimeSpan.FromSeconds(durationSeconds);
            var delay = TimeSpan.FromSeconds(_rand.NextDouble() * profile.DelayMax);

            var yAnim = new DoubleAnimation
            {
                From = startY,
                To = startY + fallDistance,
                Duration = new Duration(duration),
                BeginTime = delay,
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseIn }
            };

            var drift = (_rand.NextDouble() - 0.5) * profile.HorizontalDrift;
            var xAnim = new DoubleAnimation
            {
                From = startX,
                To = startX + drift,
                Duration = new Duration(duration),
                BeginTime = delay,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            var rotateTransform = new RotateTransform(0);
            shape.RenderTransform = rotateTransform;
            shape.RenderTransformOrigin = new Point(0.5, 0.5);
            var rotationRange = Math.Max(0, profile.RotationMax - profile.RotationMin);
            var rotationTarget = profile.RotationMin + _rand.NextDouble() * rotationRange;
            var rotAnim = new DoubleAnimation
            {
                From = 0,
                To = rotationTarget,
                Duration = new Duration(duration),
                BeginTime = delay,
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeAnim = new DoubleAnimation
            {
                From = shape.Opacity,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.6)),
                BeginTime = delay + TimeSpan.FromSeconds(Math.Max(0.2, durationSeconds - 0.6))
            };

            var sb = new Storyboard();
            Storyboard.SetTarget(yAnim, shape);
            Storyboard.SetTargetProperty(yAnim, new PropertyPath(Canvas.TopProperty));
            Storyboard.SetTarget(xAnim, shape);
            Storyboard.SetTargetProperty(xAnim, new PropertyPath(Canvas.LeftProperty));
            Storyboard.SetTarget(rotAnim, shape);
            Storyboard.SetTargetProperty(rotAnim, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            Storyboard.SetTarget(fadeAnim, shape);
            Storyboard.SetTargetProperty(fadeAnim, new PropertyPath(UIElement.OpacityProperty));
            sb.Children.Add(yAnim);
            sb.Children.Add(xAnim);
            sb.Children.Add(rotAnim);
            sb.Children.Add(fadeAnim);
            sb.Begin();
        }
    }

    /// <summary>
    /// プロファイルで指定された形状から対応する <see cref="Shape"/> を生成します。
    /// </summary>
    /// <param name="shapeType">生成する図形種別。</param>
    /// <param name="size">基準となるサイズ。</param>
    /// <returns>描画する <see cref="Shape"/>。</returns>
    private Shape CreateConfettiShape(ConfettiShape shapeType, double size)
    {
        Shape shape = shapeType switch
        {
            ConfettiShape.Rectangle => new Rectangle { Width = size * 0.7, Height = size, RadiusX = size * 0.18, RadiusY = size * 0.18 },
            ConfettiShape.Circle => new Ellipse { Width = size, Height = size },
            ConfettiShape.Triangle => new Polygon
            {
                Points = new PointCollection { new Point(0.5, 0), new Point(1, 1), new Point(0, 1) },
                Stretch = Stretch.Fill,
                Width = size,
                Height = size
            },
            ConfettiShape.Diamond => new Polygon
            {
                Points = new PointCollection { new Point(0.5, 0), new Point(1, 0.5), new Point(0.5, 1), new Point(0, 0.5) },
                Stretch = Stretch.Fill,
                Width = size,
                Height = size
            },
            ConfettiShape.Bar => new Rectangle { Width = size * 1.6, Height = size * 0.35, RadiusX = size * 0.1, RadiusY = size * 0.1 },
            _ => new Ellipse { Width = size, Height = size }
        };

        shape.SnapsToDevicePixels = true;
        return shape;
    }

    /// <summary>
    /// 中央表示枠のサイズ変化に合わせてフォントサイズを再調整します。
    /// </summary>
    private void CurrentNumberBorder_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _viewModel.AdjustCurrentNumberFontSize(e.NewSize.Width, e.NewSize.Height);
    }

    /// <inheritdoc />
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        _viewModel.UpdateLayout(sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);
    }
}

/// <summary>
/// コンフェッティで使用する図形種別です。
/// </summary>
internal enum ConfettiShape
{
    Rectangle,
    Circle,
    Triangle,
    Diamond,
    Bar
}

/// <summary>
/// コンフェッティ挙動を決める各種パラメーターを保持します。
/// </summary>
internal sealed class ConfettiProfile
{
    /// <summary>生成するパーティクル数。</summary>
    public int Count { get; init; }
    /// <summary>サイズの最小値。</summary>
    public double SizeMin { get; init; }
    /// <summary>サイズの最大値。</summary>
    public double SizeMax { get; init; }
    /// <summary>落下に掛かる時間の下限（秒）。</summary>
    public double DurationMin { get; init; }
    /// <summary>落下に掛かる時間の上限（秒）。</summary>
    public double DurationMax { get; init; }
    /// <summary>開始遅延の最大値（秒）。</summary>
    public double DelayMax { get; init; }
    /// <summary>左右への振れ幅。</summary>
    public double HorizontalDrift { get; init; }
    /// <summary>画面下に抜ける距離。</summary>
    public double VerticalOvershoot { get; init; }
    /// <summary>生成位置の最小オフセット。</summary>
    public double StartOffsetMin { get; init; }
    /// <summary>生成位置の最大オフセット。</summary>
    public double StartOffsetMax { get; init; }
    /// <summary>不透明度の最小値。</summary>
    public double OpacityMin { get; init; }
    /// <summary>不透明度の最大値。</summary>
    public double OpacityMax { get; init; }
    /// <summary>回転量の最小値。</summary>
    public double RotationMin { get; init; }
    /// <summary>回転量の最大値。</summary>
    public double RotationMax { get; init; }
    /// <summary>使用する色パレット。</summary>
    public string[] Colors { get; init; } = Array.Empty<string>();
    /// <summary>使用する図形の種類リスト。</summary>
    public ConfettiShape[] Shapes { get; init; } = Array.Empty<ConfettiShape>();
}