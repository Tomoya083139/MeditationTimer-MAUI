using Meditation.Models;

namespace Meditation.Services;

/// <summary>日記・コンディション評価・瞑想記録の永続化を担うサービス。</summary>
public interface IDiaryService
{
    /// <summary>指定日の記録を取得する。存在しない場合は新規作成して返す（保存はしない）。</summary>
    Task<DayRecord> GetOrCreateRecordAsync(DateTime date);

    /// <summary>記録をローカルファイルに保存する。</summary>
    Task SaveRecordAsync(DayRecord record);

    /// <summary>指定日の瞑想完了を記録して保存する。</summary>
    Task RecordMeditationAsync(DateTime date, TimeSpan duration);

    /// <summary>全記録を返す（カレンダー表示用）。キーは "yyyy-MM-dd"。</summary>
    Task<IReadOnlyDictionary<string, DayRecord>> GetAllRecordsAsync();
}
