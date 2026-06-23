using System.Text.Json;
using System.Text.Json.Serialization;
using Meditation.Models;

namespace Meditation.Services;

/// <summary>
/// JSON ファイルに全記録を保存する <see cref="IDiaryService"/> 実装。
/// 保存先: FileSystem.AppDataDirectory/diary.json
/// </summary>
public sealed class DiaryService : IDiaryService
{
    private const string FileName = "diary.json";
    private readonly string _filePath;

    private Dictionary<string, DayRecord> _cache = new();
    private bool _loaded;
    private readonly SemaphoreSlim _ioLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters    = { new JsonStringEnumConverter() },
    };

    public DiaryService()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, FileName);
    }

    // ── 読み込み ──────────────────────────────────────────────────────

    private async Task EnsureLoadedAsync()
    {
        if (_loaded) return;
        if (File.Exists(_filePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
                _cache = JsonSerializer.Deserialize<Dictionary<string, DayRecord>>(json, JsonOptions)
                         ?? new Dictionary<string, DayRecord>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DiaryService] Load failed: {ex.Message}");
                _cache = new Dictionary<string, DayRecord>();
            }
        }
        _loaded = true;
    }

    // ── 書き込み ──────────────────────────────────────────────────────

    private async Task PersistAsync()
    {
        await _ioLock.WaitAsync().ConfigureAwait(false);
        try
        {
            var json = JsonSerializer.Serialize(_cache, JsonOptions);
            await File.WriteAllTextAsync(_filePath, json).ConfigureAwait(false);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    // ── 公開メソッド ──────────────────────────────────────────────────

    public async Task<DayRecord> GetOrCreateRecordAsync(DateTime date)
    {
        await EnsureLoadedAsync().ConfigureAwait(false);
        var key = date.Date.ToString("yyyy-MM-dd");
        if (!_cache.TryGetValue(key, out var record))
        {
            record      = new DayRecord { Date = date.Date };
            _cache[key] = record;
        }
        return record;
    }

    public async Task SaveRecordAsync(DayRecord record)
    {
        await EnsureLoadedAsync().ConfigureAwait(false);
        var key     = record.Date.Date.ToString("yyyy-MM-dd");
        _cache[key] = record;
        await PersistAsync().ConfigureAwait(false);
    }

    public async Task RecordMeditationAsync(DateTime date, TimeSpan duration)
    {
        var record = await GetOrCreateRecordAsync(date).ConfigureAwait(false);
        record.MeditationCompleted = true;
        record.MeditationDuration  = duration;
        await SaveRecordAsync(record).ConfigureAwait(false);
    }

    public async Task<IReadOnlyDictionary<string, DayRecord>> GetAllRecordsAsync()
    {
        await EnsureLoadedAsync().ConfigureAwait(false);
        return _cache;
    }
}
