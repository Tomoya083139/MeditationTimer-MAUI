namespace Meditation.Services;

/// <summary>
/// BGM の再生・停止・音量制御を担うサービスのインターフェース。
/// </summary>
public interface IAudioService
{
    /// <summary>音量（0.0 〜 1.0）。</summary>
    double Volume { get; set; }

    /// <summary>現在の再生位置（秒）。再生していない場合は 0。</summary>
    double CurrentPosition { get; }

    /// <summary>再生中のファイルの総時間（秒）。不明の場合は 0。</summary>
    double Duration { get; }

    /// <summary>
    /// 指定ファイルをループ再生する。
    /// null または空文字を渡すと無音（停止）になる。
    /// <paramref name="volumeScale"/> はグローバル音量への乗数（録音レベル差の補正用）。
    /// </summary>
    Task PlayAsync(string? assetFileName, double volumeScale = 1.0);

    /// <summary>再生を停止する。</summary>
    void Stop();

    /// <summary>
    /// 指定した位置（秒）にシークする。
    /// 再生中でない場合は何もしない。
    /// </summary>
    void Seek(double positionSeconds);
}
