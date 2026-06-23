using Plugin.Maui.Audio;

namespace Meditation.Services;

/// <summary>
/// Plugin.Maui.Audio を使った BGM 再生サービス。
/// すべての音源をループ再生する。
/// </summary>
public sealed class AudioService : IAudioService, IDisposable
{
    private IAudioPlayer? _player;
    private double _volume = 0.7;           // デフォルト音量
    private double _currentVolumeScale = 1.0;
    private bool _loop;


    /// <inheritdoc/>
    public double Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0.0, 1.0);
            if (_player is not null)
                _player.Volume = Math.Clamp(_volume * _currentVolumeScale, 0.0, 1.0);
        }
    }

    /// <inheritdoc/>
    public event EventHandler? PlaybackEnded;

    /// <inheritdoc/>
    public async Task PlayAsync(string? assetFileName, double volumeScale = 1.0, bool loop = false)
    {
        StopAndDispose();

        if (string.IsNullOrWhiteSpace(assetFileName))
            return; // 無音選択時

        _currentVolumeScale = Math.Clamp(volumeScale, 0.0, 1.0);
        _loop = loop;

        try
        {
            var stream = await FileSystem.OpenAppPackageFileAsync(assetFileName);
            _player = AudioManager.Current.CreatePlayer(stream);
            _player.Volume = Math.Clamp(_volume * _currentVolumeScale, 0.0, 1.0);

            // ネイティブの Loop は使わず、PlaybackEnded で手動ループする。
            // これにより「曲の終端 → 先頭へ戻して再生」が明示的に制御でき、
            // CurrentPosition / Duration がシークバーと整合する。
            _player.Loop = false;
            _player.PlaybackEnded += OnPlaybackEnded;
            _player.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[AudioService] 再生失敗: {assetFileName} / {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public double CurrentPosition => _player?.CurrentPosition ?? 0.0;

    /// <inheritdoc/>
    public double Duration => _player?.Duration ?? 0.0;

    /// <inheritdoc/>
    public void Stop() => StopAndDispose();

    /// <inheritdoc/>
    public void Seek(double positionSeconds)
    {
        if (_player is null) return;
        double duration = _player.Duration;
        var clamped = duration > 0.0 
            ? Math.Clamp(positionSeconds, 0.0, duration) 
            : Math.Max(positionSeconds, 0.0);
        _player.Seek(clamped);
    }

    // ==================== 内部ヘルパー ====================

    /// <summary>
    /// 曲の再生が終わったときの処理。
    /// </summary>
    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        if (_player is null) return;
        if (_loop)
        {
            try
            {
                _player.Seek(0);
                _player.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioService] ループ再生失敗: {ex.Message}");
            }
        }
        else
        {
            PlaybackEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    private void StopAndDispose()
    {
        if (_player is null) return;
        _player.PlaybackEnded -= OnPlaybackEnded;
        
        try
        {
            _player.Stop();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioService] Stop error: {ex.Message}");
        }

        try
        {
            _player.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioService] Dispose error: {ex.Message}");
        }
        
        _player = null;
    }

    public void Dispose() => StopAndDispose();
}
