using Meditation.Services;
using Meditation.ViewModels;

namespace Meditation;

public partial class MainPage : ContentPage
{
    public MainViewModel ViewModel { get; }

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        BindingContext = ViewModel;

        ViewModel.TimerCompleted += OnTimerCompleted;
    }

    private async void OnTimerCompleted(object? sender, EventArgs e)
    {
        // ===== バイブレーション（Android / iOS のみ。Windows では FeatureNotSupportedException） =====
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromSeconds(1.5));
        }
        catch (FeatureNotSupportedException) { /* デスクトップでは無視 */ }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Vibration] {ex.Message}");
        }

        await DisplayAlert(
            "瞑想完了 🔔",
            "お疲れ様でした。\n素晴らしい時間でした。",
            "OK");
    }

    // ================================================================
    // シークスライダー イベント
    // ================================================================

    /// <summary>
    /// ドラッグ開始：ポーリングを止め、ユーザー操作を優先させる。
    /// </summary>
    private void SeekSlider_DragStarted(object sender, EventArgs e)
        => ViewModel.OnSeekDragStarted();

    /// <summary>
    /// ドラッグ完了：スライダーの現在値でシークし、ポーリングを再開する。
    /// </summary>
    private void SeekSlider_DragCompleted(object sender, EventArgs e)
    {
        if (sender is Slider slider)
            ViewModel.SeekTo(slider.Value);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ViewModel.TimerCompleted -= OnTimerCompleted;
        ViewModel.Dispose();
    }
}
