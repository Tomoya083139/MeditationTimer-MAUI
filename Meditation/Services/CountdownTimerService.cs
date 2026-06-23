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
/// バックグラウンドで動作する System.Timers.Timer を用い、
/// 実際の経過時間を基準に計算することでミリ秒単位のズレをなくした高精度な実装。
/// </summary>
public class CountdownTimerService : IDisposable
{
    private readonly IDispatcher _dispatcher;
    private System.Timers.Timer? _timer;
    private TimeSpan _remaining;
    private TimeSpan _duration;
    private DateTime _startTime;
    private TimeSpan _remainingAtStart;

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

        if (State == TimerState.Running) return;

        _startTime = DateTime.UtcNow;
        _remainingAtStart = _remaining;

        EnsureTimer();
        _timer!.Start();
        SetState(TimerState.Running);
    }

    /// <summary>一時停止。</summary>
    public void Pause()
    {
        if (State != TimerState.Running) return;
        _timer?.Stop();
        
        // 現在の残り時間を経過時間から再計算して保持（精度維持のため）
        var elapsed = DateTime.UtcNow - _startTime;
        var remainingCalculated = _remainingAtStart - elapsed;
        _remaining = remainingCalculated < TimeSpan.Zero ? TimeSpan.Zero : remainingCalculated;
        
        SetState(TimerState.Paused);
    }

    /// <summary>リセット（設定済みの duration に戻す）。</summary>
    public void Reset()
    {
        StopInternal();
        _remaining = _duration;
        Tick?.Invoke(this, _remaining);
        SetState(TimerState.Stopped);
    }

    private void EnsureTimer()
    {
        if (_timer != null) return;

        // 100msごとに高精度で時間経過を監視
        _timer = new System.Timers.Timer(100);
        _timer.AutoReset = true;
        _timer.Elapsed += OnTimerElapsed;
    }

    private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        var elapsed = DateTime.UtcNow - _startTime;
        var newRemaining = _remainingAtStart - elapsed;

        if (newRemaining <= TimeSpan.Zero)
        {
            newRemaining = TimeSpan.Zero;
            StopInternal();
            
            // UIスレッドで安全に通知を発火
            _dispatcher.Dispatch(() =>
            {
                _remaining = TimeSpan.Zero;
                Tick?.Invoke(this, _remaining);
                SetState(TimerState.Completed);
                Completed?.Invoke(this, EventArgs.Empty);
            });
            return;
        }

        // 秒単位での切り替えが発生したタイミングのみ UI へ通知する
        if ((int)newRemaining.TotalSeconds != (int)_remaining.TotalSeconds)
        {
            _dispatcher.Dispatch(() =>
            {
                _remaining = newRemaining;
                Tick?.Invoke(this, _remaining);
            });
        }
        else
        {
            // 秒数は変わっていなくても内部の Remaining は最新に更新しておく
            _remaining = newRemaining;
        }
    }

    private void StopInternal()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Dispose();
            _timer = null;
        }
    }

    private void SetState(TimerState newState)
    {
        if (State == newState) return;
        State = newState;
        StateChanged?.Invoke(this, newState);
    }

    public void Dispose()
    {
        StopInternal();
    }
}
