using CommunityToolkit.Mvvm.ComponentModel;

namespace Meditation.Models;

/// <summary>
/// クラシック音楽プレイリストの 1 トラック。
/// IsCurrentlyPlaying は ViewModel が制御し、UI のハイライトに使う。
/// </summary>
public partial class ClassicTrack : ObservableObject
{
    public required string Key           { get; init; }
    public required string Title         { get; init; }
    public          string? Composer     { get; init; }
    public required string AssetFileName { get; init; }

    /// <summary>
    /// 曲の長さ（秒）。
    /// </summary>
    public required double DurationSeconds { get; init; }

    /// <summary>
    /// グローバル音量に対する倍率（0.0〜1.0）。
    /// 音源ごとの録音レベル差を補正するために使用。
    /// </summary>
    public double VolumeScale { get; init; } = 1.0;

    [ObservableProperty]
    private bool _isCurrentlyPlaying;
}
