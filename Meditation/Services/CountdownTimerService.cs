using System;
using Microsoft.Maui.Dispatching;

namespace Meditation.Services;

/// <summary>
/// タイマーの状態。UI 側のボタン表示切り替えなどに使用。
/// </summary>
public enum TimerState
{
    Stopped,
    Running,
    Paused,
    Completed
}

/// <summary>
/// カウントダウンタイマーのコアロジック。
/// UI (MainPage / ViewModel) から分離することで、将来的に以下のような拡張が容易になります:
///   - 瞑想 BGM 再生サービス（IAudioService 等）と連携
///   - Android 固有の通知サービス（INotificationService）と連携
///   - ViewModel への組み込み（MVVM 化）
///   - ユニットテスト（IDispatcher をモック化）
/// </summary>
public class CountdownTimerService
{
    private readonly IDispatcher _dispatcher;
    private IDispatcherTimer? _timer;
    private TimeSpan _remaining;
    private TimeSpan _duration;

    public TimerState State { get; private set; } = TimerState.Stopped;
    public TimeSpan Remaining => _remaining;
    public TimeSpan Duration => _duration;

    /// <summary>残り時間が変化するたびに発火（1 秒刻み）。</summary>
    public event EventHandler<TimeSpan>? Tick;

    /// <summary>カウントダウン完了時に 1 回発火。</summary>
    public event EventHandler? Completed;

    /// <summary>タイマー状態が変化したときに発火。</summary>
    public event EventHandler<TimerState>? StateChanged;

    public CountdownTimerService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    /// <summary>指定した長さでタイマーをセット（セットのみ。開始はしない）。</summary>
    public void SetDuration(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration));

        StopInternal();
        _duration = duration;
        _remaining = duration;
        Tick?.Invoke(this, _remaining);
        SetState(TimerState.Stopped);
    }

    /// <summary>カウントダウン開始（または一時停止からの再開）。</summary>
    public void Start()
    {
        if (_remaining <= TimeSpan.Zero)
            return;

        EnsureTimer();
        _timer!.Start();
        SetState(TimerState.Running);
    }

    /// <summary>一時停止。</summary>
    public void Pause()
    {
        if (State != TimerState.Running) return;
        _timer?.Stop();
        SetState(TimerState.Paused);
    }

    /// <summary>リセット（設定済みの duration に戻す）。</summary>
    public void Reset()
    {
        _timer?.Stop();
        _remaining = _duration;
        Tick?.Invoke(this, _remaining);
        SetState(TimerState.Stopped);
    }

    private void EnsureTimer()
    {
        if (_timer != null) return;

        _timer = _dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.IsRepeating = true;
        _timer.Tick += OnDispatcherTick;
    }

    private void OnDispatcherTick(object? sender, EventArgs e)
    {
        _remaining -= TimeSpan.FromSeconds(1);

        if (_remaining <= TimeSpan.Zero)
        {
            _remaining = TimeSpan.Zero;
            _timer?.Stop();
            Tick?.Invoke(this, _remaining);
            SetState(TimerState.Completed);
            Completed?.Invoke(this, EventArgs.Empty);
            return;
        }

        Tick?.Invoke(this, _remaining);
    }

    private void StopInternal()
    {
        _timer?.Stop();
    }

    private void SetState(TimerState newState)
    {
        if (State == newState) return;
        State = newState;
        StateChanged?.Invoke(this, newState);
    }
}
