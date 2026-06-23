using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meditation.Models;
using Meditation.Services;
using System.Collections.ObjectModel;

namespace Meditation.ViewModels;

/// <summary>
/// カレンダー画面の ViewModel。
/// 月カレンダー表示・日付選択・コンディション評価・日記の読み書きを担う。
/// </summary>
public partial class CalendarViewModel : ObservableObject
{
    private readonly IDiaryService _diaryService;
    private DayRecord? _currentRecord;

    // ================================================================
    // 表示月
    // ================================================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MonthTitle))]
    private DateTime _displayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    public string MonthTitle => DisplayMonth.ToString("yyyy年M月");

    // ================================================================
    // カレンダーセル（常に 42 個 = 6週 × 7日）
    // ================================================================

    public ObservableCollection<CalendarDayCell> DayCells { get; } = new();

    // ================================================================
    // 選択セル・詳細パネル
    // ================================================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailVisible))]
    [NotifyPropertyChangedFor(nameof(SelectedDateTitle))]
    private CalendarDayCell? _selectedCell;

    public bool   IsDetailVisible  => SelectedCell?.Date is not null;
    public string SelectedDateTitle
    {
        get
        {
            if (SelectedCell?.Date is not DateTime d) return string.Empty;
            string[] weekDays = { "日", "月", "火", "水", "木", "金", "土" };
            return $"{d.Month}月{d.Day}日（{weekDays[(int)d.DayOfWeek]}）";
        }
    }

    // ── 瞑想ステータス（読み取り専用・選択日の記録から算出） ──

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MeditationStatusText))]
    private bool _selectedDayHasMeditation;

    public string MeditationStatusText
    {
        get
        {
            if (_currentRecord is null || !_currentRecord.MeditationCompleted)
                return string.Empty;
            var d = _currentRecord.MeditationDuration;
            string dur = d.TotalHours >= 1
                ? $"{(int)d.TotalHours}時間{d.Minutes}分"
                : $"{(int)d.TotalMinutes}分";
            return $"🧘 瞑想 {dur} 完了";
        }
    }

    // ================================================================
    // コンディション評価（朝・昼・夜）
    // ================================================================

    [ObservableProperty] private ConditionLevel? _morningCondition;
    [ObservableProperty] private ConditionLevel? _noonCondition;
    [ObservableProperty] private ConditionLevel? _eveningCondition;

    partial void OnMorningConditionChanged(ConditionLevel? value)
    {
        OnPropertyChanged(nameof(IsMorningGood));
        OnPropertyChanged(nameof(IsMorningNormal));
        OnPropertyChanged(nameof(IsMorningBad));
    }
    partial void OnNoonConditionChanged(ConditionLevel? value)
    {
        OnPropertyChanged(nameof(IsNoonGood));
        OnPropertyChanged(nameof(IsNoonNormal));
        OnPropertyChanged(nameof(IsNoonBad));
    }
    partial void OnEveningConditionChanged(ConditionLevel? value)
    {
        OnPropertyChanged(nameof(IsEveningGood));
        OnPropertyChanged(nameof(IsEveningNormal));
        OnPropertyChanged(nameof(IsEveningBad));
    }

    // ── 選択状態（DataTrigger 用 bool プロパティ）──

    public bool IsMorningGood   => MorningCondition == ConditionLevel.Good;
    public bool IsMorningNormal => MorningCondition == ConditionLevel.Normal;
    public bool IsMorningBad    => MorningCondition == ConditionLevel.Bad;

    public bool IsNoonGood      => NoonCondition == ConditionLevel.Good;
    public bool IsNoonNormal    => NoonCondition == ConditionLevel.Normal;
    public bool IsNoonBad       => NoonCondition == ConditionLevel.Bad;

    public bool IsEveningGood   => EveningCondition == ConditionLevel.Good;
    public bool IsEveningNormal => EveningCondition == ConditionLevel.Normal;
    public bool IsEveningBad    => EveningCondition == ConditionLevel.Bad;

    // ================================================================
    // 日記テキスト・保存ステータス
    // ================================================================

    [ObservableProperty] private string _diaryText = string.Empty;
    [ObservableProperty] private string _saveStatus = string.Empty;

    // ================================================================
    // コンストラクタ
    // ================================================================

    public CalendarViewModel(IDiaryService diaryService)
    {
        _diaryService = diaryService;
        _ = RefreshCalendarAsync();
    }

    // ================================================================
    // コマンド — 月移動
    // ================================================================

    [RelayCommand]
    private async Task PreviousMonth()
    {
        DisplayMonth = DisplayMonth.AddMonths(-1);
        await RefreshCalendarAsync();
    }

    [RelayCommand]
    private async Task NextMonth()
    {
        DisplayMonth = DisplayMonth.AddMonths(1);
        await RefreshCalendarAsync();
    }

    // ================================================================
    // コマンド — 日付選択
    // ================================================================

    [RelayCommand]
    private async Task SelectDay(CalendarDayCell? cell)
    {
        if (cell is null || !cell.Date.HasValue) return;

        // 選択ハイライトを更新
        foreach (var c in DayCells)
            c.IsSelected = false;
        cell.IsSelected = true;
        SelectedCell = cell;

        // その日の記録を読み込む
        _currentRecord = await _diaryService.GetOrCreateRecordAsync(cell.Date.Value);

        MorningCondition = _currentRecord.MorningCondition;
        NoonCondition    = _currentRecord.NoonCondition;
        EveningCondition = _currentRecord.EveningCondition;
        DiaryText        = _currentRecord.DiaryText;
        SaveStatus       = string.Empty;

        SelectedDayHasMeditation = _currentRecord.MeditationCompleted;
        OnPropertyChanged(nameof(MeditationStatusText));
    }

    // ================================================================
    // コマンド — コンディション設定
    // ================================================================

    [RelayCommand]
    private void SetMorningCondition(string levelStr) => MorningCondition = Parse(levelStr);

    [RelayCommand]
    private void SetNoonCondition(string levelStr) => NoonCondition = Parse(levelStr);

    [RelayCommand]
    private void SetEveningCondition(string levelStr) => EveningCondition = Parse(levelStr);

    // ================================================================
    // コマンド — 記録保存
    // ================================================================

    [RelayCommand]
    private async Task SaveRecord()
    {
        if (_currentRecord is null || SelectedCell?.Date is null) return;

        _currentRecord.MorningCondition = MorningCondition;
        _currentRecord.NoonCondition    = NoonCondition;
        _currentRecord.EveningCondition = EveningCondition;
        _currentRecord.DiaryText        = DiaryText;

        await _diaryService.SaveRecordAsync(_currentRecord);

        SaveStatus = "保存しました ✓";
        _ = Task.Delay(2500).ContinueWith(_ =>
            MainThread.BeginInvokeOnMainThread(() => SaveStatus = string.Empty));
    }

    // ================================================================
    // カレンダー構築（外部から呼べる：OnAppearing で再描画）
    // ================================================================

    public async Task RefreshCalendarAsync()
    {
        var allRecords = await _diaryService.GetAllRecordsAsync();

        DayCells.Clear();

        int year  = DisplayMonth.Year;
        int month = DisplayMonth.Month;
        var today = DateTime.Today;
        var firstDay  = new DateTime(year, month, 1);
        int daysInMonth = DateTime.DaysInMonth(year, month);

        // 月曜始まり（0=月 … 6=日）
        int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;

        // 前月の空セル
        for (int i = 0; i < startOffset; i++)
            DayCells.Add(new CalendarDayCell { IsCurrentMonth = false });

        // 当月のセル
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);
            var key  = date.ToString("yyyy-MM-dd");
            DayCells.Add(new CalendarDayCell
            {
                Date           = date,
                IsCurrentMonth = true,
                IsToday        = date == today,
                HasMeditation  = allRecords.TryGetValue(key, out var r) && r.MeditationCompleted,
            });
        }

        // 42セルになるよう末尾を空セルで埋める
        while (DayCells.Count < 42)
            DayCells.Add(new CalendarDayCell { IsCurrentMonth = false });

        // 月移動後も選択状態を維持できる場合は再選択
        if (SelectedCell?.Date is DateTime prev
            && prev.Year == year && prev.Month == month)
        {
            foreach (var c in DayCells)
            {
                if (c.Date == prev) { c.IsSelected = true; SelectedCell = c; break; }
            }
        }
        else
        {
            SelectedCell = null;
        }
    }

    // ================================================================
    // ヘルパー
    // ================================================================

    private static ConditionLevel? Parse(string levelStr) => levelStr switch
    {
        "Good"   => ConditionLevel.Good,
        "Normal" => ConditionLevel.Normal,
        "Bad"    => ConditionLevel.Bad,
        _        => null,
    };
}
