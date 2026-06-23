using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meditation.Models;
using Meditation.Services;
using System.Collections.ObjectModel;

namespace Meditation.ViewModels;

/// <summary>
/// MainPage にバインドする ViewModel。
/// タイマー・BGM（環境音 / クラシック）・カスタム時間入力・シークを管理する。
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly CountdownTimerService _timer;
    private readonly IAudioService _audioService;
    private readonly IDispatcher _dispatcher;
    private readonly IDiaryService _diaryService;

    // ================================================================
    // タイマー状態
    // ================================================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RemainingDisplay))]
    private TimeSpan _remaining;

    public string RemainingDisplay => FormatTime(Remaining);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PrimaryButtonText))]
    private bool _isRunning;

    partial void OnIsRunningChanged(bool value) => UpdateScreenLock();

    public string PrimaryButtonText => IsRunning ? "II" : "▶";

    // ================================================================
    // アラーム状態（目覚まし機能）
    // ================================================================

    [ObservableProperty]
    private bool _isAlarmActive;

    partial void OnIsAlarmActiveChanged(bool value) => UpdateScreenLock();

    private CancellationTokenSource? _vibrationCts;

    [RelayCommand]
    private void StopAlarm()
    {
        IsAlarmActive = false;
        _vibrationCts?.Cancel();
        _vibrationCts?.Dispose();
        _vibrationCts = null;
    }

    private async Task StartPersistentVibrationAsync()
    {
        _vibrationCts?.Cancel();
        _vibrationCts?.Dispose();
        _vibrationCts = new CancellationTokenSource();
        var token = _vibrationCts.Token;

        IsAlarmActive = true;

        try
        {
            while (!token.IsCancellationRequested)
            {
                // バイブレーション実行（1秒間）
                Vibration.Default.Vibrate(TimeSpan.FromSeconds(1));
                
                // 2秒待機して繰り返す
                await Task.Delay(2000, token);
            }
        }
        catch (TaskCanceledException) { /* 停止時はここに来る */ }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Vibration] {ex.Message}");
        }
    }

    // ================================================================
    // プリセット
    // ================================================================

    [ObservableProperty]
    private int _selectedPresetMinutes = 10;

    partial void OnSelectedPresetMinutesChanged(int value)
    {
        _timer.SetDuration(TimeSpan.FromMinutes(value));
    }

    // ================================================================
    // カスタム時間入力
    // ================================================================

    [ObservableProperty]
    private bool _isCustomMode;

    [ObservableProperty]
    private string _customMinuteText = "10";

    [ObservableProperty]
    private string _customSecondText = "00";

    // ================================================================
    // BGM — 再生状態
    // ================================================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BgmButtonText))]
    private bool _isBgmPlaying;

    public string BgmButtonText => IsBgmPlaying ? "⏹ 停止" : "▶ 再生";

    // ================================================================
    // BGM — 環境音
    // ================================================================

    public ObservableCollection<AmbientSound> AmbientSounds { get; } = new()
    {
        new AmbientSound { Key = "none",  DisplayName = "無音", Icon = "◯", Description = "静寂を楽しむ",                              VolumeScale = 1.0  },
        new AmbientSound { Key = "rain",  DisplayName = "雨",   Icon = "☂", Description = "静かな雨音",   AssetFileName = "rain.mp3",  VolumeScale = 1.0  },
        new AmbientSound { Key = "waves", DisplayName = "波",   Icon = "〜", Description = "穏やかな海",   AssetFileName = "wave.mp3",  VolumeScale = 1.0  },
    };

    [ObservableProperty]
    private AmbientSound? _selectedAmbientSound;

    partial void OnSelectedAmbientSoundChanged(AmbientSound? value)
    {
        if (value is not null)
        {
            IsClassicMode = false; // 明示的に環境音モードにする
        }
        IsBgmPlaying = !string.IsNullOrEmpty(value?.AssetFileName);
        PlayAudio(value?.AssetFileName, value?.VolumeScale ?? 1.0);
    }

    // ================================================================
    // BGM — クラシック
    // ================================================================

    public ObservableCollection<ClassicTrack> ClassicTracks { get; } = new()
    {
        new ClassicTrack { Key = "canon",           Title = "カノン",              Composer = "パッヘルベル",     AssetFileName = "Canon.mp3",                                                                         DurationSeconds = 286.0, VolumeScale = 1.00 },
        new ClassicTrack { Key = "chopin_etude",    Title = "別れの曲 Op.10 No.3", Composer = "ショパン",         AssetFileName = "04 - Chopin - Etude in E Major, Op. 10, No. 3.mp3",                               DurationSeconds = 266.0, VolumeScale = 1.00 },
        new ClassicTrack { Key = "chopin_nocturne", Title = "夜想曲 Op.9 No.2",    Composer = "ショパン",         AssetFileName = "Chopin_Nocturne_No.04_in_EfM_Op.9_2_SDRodrian.mp3",                               DurationSeconds = 179.0, VolumeScale = 1.00 },
        new ClassicTrack { Key = "debussy_clair",   Title = "月の光",              Composer = "ドビュッシー",     AssetFileName = "saturn-3-music-claire-de-lune-debussy-piano-411227.mp3",                           DurationSeconds = 285.0, VolumeScale = 1.00 },
        new ClassicTrack { Key = "tchaiko_waltz",   Title = "花のワルツ",          Composer = "チャイコフスキー", AssetFileName = "(19) [Tchaikovsky] The Nutcracker- Act 2- No. 13 Waltz of the Flowers.mp3",         DurationSeconds = 386.0, VolumeScale = 0.95 },
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NowPlayingText))]
    private ClassicTrack? _selectedClassicTrack;

    partial void OnSelectedClassicTrackChanged(ClassicTrack? value)
    {
        UpdateClassicPlayingStates();
        if (value is not null)
        {
            IsClassicMode = true; // 明示的にクラシックモードにする
            IsBgmPlaying = true;
            PlayAudio(value.AssetFileName, value.VolumeScale);
        }
        else
        {
            IsBgmPlaying = false;
            StopAudio();
        }
    }

    public string NowPlayingText
    {
        get
        {
            if (SelectedClassicTrack is null)
                return "トラックを選択してください";
            return SelectedClassicTrack.Composer is not null
                ? $"♫  {SelectedClassicTrack.Title}  /  {SelectedClassicTrack.Composer}"
                : $"♫  {SelectedClassicTrack.Title}";
        }
    }

    private void UpdateClassicPlayingStates()
    {
        foreach (var t in ClassicTracks)
            t.IsCurrentlyPlaying = t == SelectedClassicTrack;
    }

    // ================================================================
    // BGM — カテゴリ切り替え
    // ================================================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAmbientMode))]
    private bool _isClassicMode;

    public bool IsAmbientMode => !IsClassicMode;

    // ================================================================
    // 音量
    // ================================================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VolumePercent))]
    private double _volume = 0.7;

    partial void OnVolumeChanged(double value)
    {
        _audioService.Volume = value;
    }

    public string VolumePercent => $"{(int)(Volume * 100)}%";

    // ================================================================
    // 再生位置（シーク）
    // ================================================================

    private IDispatcherTimer? _positionTimer;
    /// <summary>ユーザーがシークバーをドラッグ中は true。ポーリング更新を停止する。</summary>
    private bool _isDragging;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TrackPositionDisplay))]
    [NotifyPropertyChangedFor(nameof(TrackRemainingDisplay))]
    private double _trackPosition;
    // ※ シークは OnSeekDragStarted / SeekTo 経由でのみ行う（TwoWay バインドなし）

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TrackRemainingDisplay))]
    private double _trackDuration = 1.0;

    public string TrackPositionDisplay => FormatTime(TimeSpan.FromSeconds(TrackPosition));
    public string TrackDurationDisplay => FormatTime(TimeSpan.FromSeconds(Math.Max(TrackDuration, 0)));
    public string TrackRemainingDisplay => "-" + FormatTime(TimeSpan.FromSeconds(Math.Max(0.0, TrackDuration - TrackPosition)));

    private void StartPositionPolling()
    {
        if (_positionTimer is not null)
        {
            _positionTimer.Stop();
            _positionTimer.Tick -= OnPositionTick;
        }
        _positionTimer = _dispatcher.CreateTimer();
        _positionTimer.Interval = TimeSpan.FromMilliseconds(500);
        _positionTimer.IsRepeating = true;
        _positionTimer.Tick += OnPositionTick;
        _positionTimer.Start();
    }

    private void StopPositionPolling()
    {
        _positionTimer?.Stop();
        _isDragging   = false;
        TrackPosition = 0;
        TrackDuration = 1.0;
    }

    private void OnPositionTick(object? sender, EventArgs e)
    {
        // ユーザーがシークバーをドラッグ中はポーリングを完全スキップ。
        if (_isDragging) return;

        double current = _audioService.CurrentPosition;

        if (IsClassicMode && SelectedClassicTrack != null)
        {
            TrackDuration = SelectedClassicTrack.DurationSeconds;

            // 再生中で、現在の秒数が 1.0秒 以上経過しており、曲の残り時間が 0.8秒 以下（または終端到達）になった場合に
            // 自動的に次の曲を再生する（Android等における PlaybackEnded イベント不発対策のフォールバック）
            if (IsBgmPlaying && current > 1.0 && (TrackDuration - current <= 0.8 || current >= TrackDuration))
            {
                // 次の曲へ切り替える前に、タイマー（ポーリング）を明示的に一度止めてリソース競合を防ぐ
                StopPositionPolling();
                PlayNextClassicTrack();
                return;
            }
        }
        else
        {
            double duration = _audioService.Duration;
            if (duration > 0.5)
                TrackDuration = duration;
        }

        // ループ後やロード直後に CurrentPosition が Duration をわずかに超える場合にクランプして
        // Slider.Maximum オーバーによる表示崩れを防ぐ
        TrackPosition = Math.Clamp(current, 0.0, TrackDuration);
    }

    // ================================================================
    // シーク（DragStarted / DragCompleted から呼ばれる）
    // ================================================================

    /// <summary>
    /// シークバーのドラッグ開始時に Page から呼ぶ。
    /// ポーリング更新を止めてスライダー操作を優先させる。
    /// </summary>
    public void OnSeekDragStarted() => _isDragging = true;

    /// <summary>
    /// シークバーのドラッグ完了時に Page から呼ぶ。
    /// <paramref name="positionSeconds"/> の位置にシークして、ポーリングを再開する。
    /// </summary>
    public void SeekTo(double positionSeconds)
    {
        _isDragging = false;
        if (IsBgmPlaying)
        {
            _audioService.Seek(positionSeconds);
            // シーク完了直後につまみの位置を即時更新し、ポーリング待機によるガタつきを防ぐ（最適化）
            TrackPosition = positionSeconds;
        }
    }

    // ================================================================
    // 再生ヘルパー（PlayAsync + ポーリング管理を一元化）
    // ================================================================

    private void PlayAudio(string? file, double scale)
    {
        _audioService.PlayAsync(file, scale, !IsClassicMode);
        if (!string.IsNullOrEmpty(file))
            StartPositionPolling();
        else
            StopPositionPolling();
    }

    private void StopAudio()
    {
        _audioService.Stop();
        StopPositionPolling();
    }

    // ================================================================
    // コマンド
    // ================================================================

    [RelayCommand]
    private void SelectPreset(string minsStr)
    {
        if (int.TryParse(minsStr, out int mins))
        {
            IsCustomMode = false;
            SelectedPresetMinutes = mins;
        }
    }

    [RelayCommand]
    private void StartPause()
    {
        if (_timer.State == TimerState.Running) _timer.Pause();
        else                                    _timer.Start();
    }

    [RelayCommand]
    private void Reset() => _timer.Reset();

    [RelayCommand]
    private void ToggleCustomMode() => IsCustomMode = !IsCustomMode;

    [RelayCommand]
    private void SetCustomTime()
    {
        int mins  = int.TryParse(CustomMinuteText, out int m) ? Math.Clamp(m, 0, 99) : 0;
        int secs  = int.TryParse(CustomSecondText, out int s) ? Math.Clamp(s, 0, 59) : 0;
        var total = TimeSpan.FromMinutes(mins) + TimeSpan.FromSeconds(secs);
        if (total <= TimeSpan.Zero) return;
        CustomMinuteText = mins.ToString();
        CustomSecondText = secs.ToString("D2");
        _timer.SetDuration(total);
    }

    [RelayCommand]
    private void IncrMinutes()
    {
        int v = int.TryParse(CustomMinuteText, out int m) ? m : 0;
        CustomMinuteText = Math.Clamp(v + 1, 0, 99).ToString();
    }

    [RelayCommand]
    private void DecrMinutes()
    {
        int v = int.TryParse(CustomMinuteText, out int m) ? m : 0;
        CustomMinuteText = Math.Clamp(v - 1, 0, 99).ToString();
    }

    [RelayCommand]
    private void IncrSeconds()
    {
        int v = int.TryParse(CustomSecondText, out int s) ? s : 0;
        CustomSecondText = v >= 59 ? "00" : (v + 1).ToString("D2");
    }

    [RelayCommand]
    private void DecrSeconds()
    {
        int v = int.TryParse(CustomSecondText, out int s) ? s : 0;
        CustomSecondText = v <= 0 ? "59" : (v - 1).ToString("D2");
    }

    [RelayCommand]
    private void SelectAmbientCategory()
    {
        if (IsAmbientMode) return;
        IsClassicMode = false;
        if (IsBgmPlaying)
        {
            string? file  = SelectedAmbientSound?.AssetFileName;
            IsBgmPlaying  = !string.IsNullOrEmpty(file);
            PlayAudio(file, SelectedAmbientSound?.VolumeScale ?? 1.0);
        }
    }

    [RelayCommand]
    private void SelectClassicCategory()
    {
        if (IsClassicMode) return;
        IsClassicMode = true;
        if (IsBgmPlaying)
        {
            var track = SelectedClassicTrack ?? (ClassicTracks.Count > 0 ? ClassicTracks[0] : null);
            if (track is not null)
                SelectedClassicTrack = track;
        }
    }

    [RelayCommand]
    private void ToggleBgm()
    {
        if (IsBgmPlaying)
        {
            StopAudio();
            IsBgmPlaying = false;
        }
        else
        {
            string? file = IsClassicMode
                ? SelectedClassicTrack?.AssetFileName
                : SelectedAmbientSound?.AssetFileName;
            double scale = IsClassicMode
                ? (SelectedClassicTrack?.VolumeScale ?? 1.0)
                : (SelectedAmbientSound?.VolumeScale ?? 1.0);
            if (!string.IsNullOrEmpty(file))
            {
                IsBgmPlaying = true;
                PlayAudio(file, scale);
            }
        }
    }

    // ================================================================
    // 完了通知
    // ================================================================

    public event EventHandler? TimerCompleted;

    // ================================================================
    // コンストラクタ
    // ================================================================

    public MainViewModel(CountdownTimerService timer, IAudioService audioService, IDispatcher dispatcher, IDiaryService diaryService)
    {
        _timer        = timer;
        _audioService = audioService;
        _dispatcher   = dispatcher;
        _diaryService = diaryService;

        _audioService.Volume = _volume;

        _timer.Tick         += OnTimerTick;
        _timer.StateChanged += OnTimerStateChanged;
        _timer.Completed    += OnTimerCompleted;

        _audioService.PlaybackEnded += OnAudioPlaybackEnded;

        _timer.SetDuration(TimeSpan.FromMinutes(SelectedPresetMinutes));

        SelectedAmbientSound = AmbientSounds.Count > 0 ? AmbientSounds[0] : null;
    }

    // ================================================================
    // イベントハンドラ
    // ================================================================

    private void OnTimerTick(object? sender, TimeSpan remaining)       => Remaining = remaining;
    private void OnTimerStateChanged(object? sender, TimerState state) => IsRunning = state == TimerState.Running;

    private void OnTimerCompleted(object? sender, EventArgs e)
    {
        // 瞑想完了をカレンダーに記録する（fire-and-forget）
        _ = _diaryService.RecordMeditationAsync(DateTime.Today, _timer.Duration);

        // 継続的なバイブレーションを開始
        _ = StartPersistentVibrationAsync();

        TimerCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void OnAudioPlaybackEnded(object? sender, EventArgs e)
    {
        _dispatcher.Dispatch(() =>
        {
            if (IsClassicMode)
            {
                PlayNextClassicTrack();
            }
        });
    }

    private void PlayNextClassicTrack()
    {
        if (ClassicTracks.Count == 0) return;

        int currentIndex = SelectedClassicTrack != null ? ClassicTracks.IndexOf(SelectedClassicTrack) : -1;
        int nextIndex = currentIndex + 1;

        if (nextIndex < ClassicTracks.Count)
        {
            SelectedClassicTrack = ClassicTracks[nextIndex];
        }
        else
        {
            // 最後の曲が終了した場合は再生を停止する
            SelectedClassicTrack = null;
            IsBgmPlaying = false;
            StopAudio();
        }
    }

    // ================================================================
    // 後片付け
    // ================================================================

    public void Dispose()
    {
        _timer.Tick         -= OnTimerTick;
        _timer.StateChanged -= OnTimerStateChanged;
        _timer.Completed    -= OnTimerCompleted;
        _audioService.PlaybackEnded -= OnAudioPlaybackEnded;
        StopAudio();
        if (_positionTimer is not null)
        {
            _positionTimer.Stop();
            _positionTimer.Tick -= OnPositionTick;
            _positionTimer = null;
        }
        _vibrationCts?.Cancel();
        _vibrationCts?.Dispose();
        _vibrationCts = null;
    }

    // ================================================================
    // ヘルパー
    // ================================================================

    private void UpdateScreenLock()
    {
        // タイマー動作中、またはアラーム鳴動中のみ画面をオンに維持する
        bool shouldKeepScreenOn = IsRunning || IsAlarmActive;
        
        if (DeviceDisplay.Current.KeepScreenOn != shouldKeepScreenOn)
        {
            DeviceDisplay.Current.KeepScreenOn = shouldKeepScreenOn;
        }
    }

    private static string FormatTime(TimeSpan t)
    {
        if (t < TimeSpan.Zero) t = TimeSpan.Zero;
        return t.TotalHours >= 1
            ? $"{(int)t.TotalHours}:{t.Minutes:D2}:{t.Seconds:D2}"
            : $"{t.Minutes:D2}:{t.Seconds:D2}";
    }
}
