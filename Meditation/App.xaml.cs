namespace Meditation
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Windows/Android/iOS/MacCatalyst すべてで常にダークモードで表示する。
            UserAppTheme = AppTheme.Dark;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            // PC(Windows/Mac) で起動したときにスマホのような縦長ウィンドウにする。
            // ※ iOS/Android 端末では Width/Height 指定は実機のサイズに上書きされるため影響なし。
            const double phoneWidth = 412;
            const double phoneHeight = 860;

            window.Width = phoneWidth;
            window.Height = phoneHeight;
            window.MinimumWidth = 360;
            window.MinimumHeight = 640;

            return window;
        }
    }
}
