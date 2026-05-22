namespace Meditation.Models;

/// <summary>
/// BGM（環境音）ライブラリの 1 項目。
/// Step 1 では Icon と DisplayName をもとに UI リストを構築するだけで、
/// 実際の再生は Step 2 以降（AudioService）で行う想定。
/// </summary>
public sealed class AmbientSound
{
    /// <summary>プログラム内で一意な識別キー（例: "rain"）。</summary>
    public required string Key { get; init; }

    /// <summary>表示名（例: "雨"）。</summary>
    public required string DisplayName { get; init; }

    /// <summary>リスト用アイコン（絵文字 1 字でも OK）。</summary>
    public required string Icon { get; init; }

    /// <summary>短い補足テキスト（例: "静かな雨音"）。</summary>
    public string? Description { get; init; }

    /// <summary>リソース名（Resources/Raw 配下のファイル名）。</summary>
    public string? AssetFileName { get; init; }

    /// <summary>
    /// グローバル音量に対する倍率（0.0〜1.0）。
    /// 音源ごとの録音レベル差を補正するために使用。
    /// </summary>
    public double VolumeScale { get; init; } = 1.0;
}
