namespace Meditation.Models;

/// <summary>1日分の瞑想実績・コンディション評価・日記を保持するデータモデル。</summary>
public class DayRecord
{
    /// <summary>記録日（時刻部分は 00:00:00）。</summary>
    public DateTime Date { get; set; }

    /// <summary>当日に瞑想を完了したか。</summary>
    public bool MeditationCompleted { get; set; }

    /// <summary>瞑想の実施時間。</summary>
    public TimeSpan MeditationDuration { get; set; }

    /// <summary>朝のコンディション評価。null = 未記録。</summary>
    public ConditionLevel? MorningCondition { get; set; }

    /// <summary>昼のコンディション評価。null = 未記録。</summary>
    public ConditionLevel? NoonCondition { get; set; }

    /// <summary>夜のコンディション評価。null = 未記録。</summary>
    public ConditionLevel? EveningCondition { get; set; }

    /// <summary>日記・メモテキスト。</summary>
    public string DiaryText { get; set; } = string.Empty;
}
