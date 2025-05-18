using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MacroMechanicsHub.Components;
using MacroMechanicsHub.Models;
using MacroMechanicsHub.Services;

namespace MacroMechanicsHub
{
    public partial class MainWindow : Window
    {
        private const string FolderName = "MacroMechanicsHub";
        private const string ScreenshotFileName = "screenshot.png";
        private const string RegionFileName = "region.txt";
        private const int TimerInterval = 10;

        private static readonly string MainDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FolderName);

        private static readonly string ScreenshotPath = Path.Combine(MainDirectory, ScreenshotFileName);
        private static readonly string RegionFilePath = Path.Combine(MainDirectory, RegionFileName);

        private NotifyIcon _notifyIcon;
        private CaptureRegion _mapRegion = new CaptureRegion();
        private bool _logicRunning = false;

        // Servicios
        private readonly CaptureService _captureService = new CaptureService();
        private readonly RegionService _regionService = new RegionService();
        private readonly FileService _fileService = new FileService();
        private readonly LoLApiService _lolApiService = new LoLApiService();
        private readonly AssistantService _assistantService = new AssistantService();
        private NotificacionService _notificacionService;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSystemTray();
            _notificacionService = new NotificacionService(_notifyIcon);
            _fileService.EnsureDirectory(MainDirectory);
            LoadMapRegion();
            StartLogicTimer();
        }

        private void InitializeSystemTray()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon(Properties.Resources.Icon, 40, 40),
                Visible = true,
                Text = "MacroMechanicsHub",
                BalloonTipIcon = ToolTipIcon.Info,
                BalloonTipTitle = "MacroMechanicsHub",
                BalloonTipText = "Aplicación en ejecución"
            };

            _notifyIcon.DoubleClick += (s, e) =>
            {
                Show();
                WindowState = WindowState.Normal;
            };

            StateChanged += (s, e) =>
            {
                if (WindowState == WindowState.Minimized)
                    Hide();
            };
        }

        private void StartLogicTimer()
        {
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(TimerInterval)
            };
            timer.Tick += (s, e) => RunLogic();
            timer.Start();
        }

        private void RunLogic()
        {
            if (_logicRunning)
                return;

            _logicRunning = true;
            _captureService.CaptureRegion(_mapRegion, ScreenshotPath);
            Log("Captura del minimapa realizada");
            bool isApiAvailable = _lolApiService.IsApiAvailable();

            if (!isApiAvailable)
            {
                Log("API no disponible");
                _notificacionService.ShowNotification("API no disponible", "La API de LoL no está disponible.");
                _logicRunning = false;
                return;
            }

            SetPromptForIA();

            _logicRunning = false;
        }

        private void SetPromptForIA()
        {
            var prompt = _assistantService.GetAssistantResponse();
            if (prompt != null)
            {
                Log("Respuesta de IA obtenida");
                _notificacionService.ShowNotification("Respuesta de IA", "Si");
            }
            else
            {
                Log("No se pudo obtener la respuesta de IA");
                _notificacionService.ShowNotification("Error", "No se pudo obtener la respuesta de IA.");

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var regionSelector = new RegionSelectorWindow();
            if (regionSelector.ShowDialog() == true)
            {
                var region = regionSelector.GetAdjustedSelectedRegion();
                Log($"Región seleccionada: {region}");
                SaveMapRegion(new CaptureRegion
                {
                    X = region.X,
                    Y = region.Y,
                    Width = region.Width,
                    Height = region.Height
                });
            }
        }

        private void SaveMapRegion(CaptureRegion region)
        {
            _regionService.SaveRegion(RegionFilePath, region);
            LoadMapRegion();
        }

        private void LoadMapRegion()
        {
            _mapRegion = _regionService.LoadRegion(RegionFilePath);
            Log("Región del mapa actualizada desde el archivo.");
        }

        private void Log(string message)
        {
            Logs.Text += $"{DateTime.Now}: {message}\n";
        }

        protected override void OnClosed(EventArgs e)
        {
            _notifyIcon?.Dispose();
            base.OnClosed(e);
        }

        #region Hook de teclado (comentado)
        // Si necesitas el hook, descomenta y adapta estos métodos.
        /*
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static IntPtr _hookID = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private LowLevelKeyboardProc _proc;

        private void HookKeyboard()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == 0x09) // Tab
                {
                    // Acción al presionar Tab
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        */
        #endregion
    }
}
