using CommunityToolkit.Mvvm.ComponentModel;

namespace Meditation.Models;

/// <summary>カレンダーグリッドの1マス（日付セル）を表すビューモデル。</summary>
public partial class CalendarDayCell : ObservableObject
{
    /// <summary>null の場合は空のパディングセル（月初前・月末後）。</summary>
    public DateTime? Date { get; init; }

    /// <summary>現在表示月に属するか（false なら前後月の日付でグレー表示）。</summary>
    public bool IsCurrentMonth { get; init; }

    /// <summary>本日かどうか。</summary>
    public bool IsToday { get; init; }

    /// <summary>表示する日数。空セルは 0（ラベル非表示）。</summary>
    public string DayText => Date?.Day.ToString() ?? string.Empty;

    /// <summary>空セルかどうか（タップ無効化に使用）。</summary>
    public bool IsEmpty => !Date.HasValue;

    /// <summary>当日に瞑想完了記録があるか。</summary>
    [ObservableProperty] private bool _hasMeditation;

    /// <summary>このセルが現在選択されているか（UI ハイライト制御用）。</summary>
    [ObservableProperty] private bool _isSelected;
}
