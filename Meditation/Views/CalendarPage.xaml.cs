using Meditation.ViewModels;

namespace Meditation.Views;

public partial class CalendarPage : ContentPage
{
    private readonly CalendarViewModel _viewModel;

    public CalendarPage(CalendarViewModel viewModel)
    {
        InitializeComponent();
        _viewModel  = viewModel;
        BindingContext = viewModel;
    }

    /// <summary>
    /// 画面が表示されるたびにカレンダーを再描画する。
    /// タイマー画面で瞑想を完了した後にカレンダーを開いた場合に、
    /// 当日のドットが正しく反映されるようにするため。
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.RefreshCalendarAsync();
    }

    /// <summary>
    /// 日記入力欄以外の背景をタップしたとき、Editor のフォーカスを外して
    /// ソフトウェアキーボードを閉じる。
    /// </summary>
    private void OnBackgroundTapped(object sender, TappedEventArgs e)
    {
        if (DiaryEditor.IsFocused)
            DiaryEditor.Unfocus();
    }
}
