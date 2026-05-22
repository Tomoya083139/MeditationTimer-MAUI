using Plugin.Maui.Audio;

namespace Meditation.Services;

/// <summary>
/// Plugin.Maui.Audio を使った BGM 再生サービス。
/// すべての音源をループ再生する。
/// </summary>
public sealed class AudioService : IAudioService, IDisposable
{
    private IAudioPlayer? _player;
    private double _volume = 0.45;          // デフォルト音量：瞑想向けに低め
    private double _currentVolumeScale = 1.0;

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
    public async Task PlayAsync(string? assetFileName, double volumeScale = 1.0)
    {
        StopAndDispose();

        if (string.IsNullOrWhiteSpace(assetFileName))
            return; // 無音選択時

        _currentVolumeScale = Math.Clamp(volumeScale, 0.0, 1.0);

        try
        {
            var stream = await FileSystem.OpenAppPackageFileAsync(assetFileName);
            _player = AudioManager.Current.CreatePlayer(stream);
            _player.Volume = Math.Clamp(_volume * _currentVolumeScale, 0.0, 1.0);
            _player.Loop   = true;
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
        var clamped = Math.Clamp(positionSeconds, 0.0, _player.Duration);
        _player.Seek(clamped);
    }

    // ==================== 内部ヘルパー ====================

    private void StopAndDispose()
    {
        if (_player is null) return;
        _player.Stop();
        _player.Dispose();
        _player = null;
    }

    public void Dispose() => StopAndDispose();
}
