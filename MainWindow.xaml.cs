using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using MacroMechanicsHub.Components;




namespace MacroMechanicsHub
{
    public partial class MainWindow : Window
    {
        // Constants
        private const string FOLDER_NAME = "MacroMechanicsHub";
        private const string SS_NAME = "screenshot.png";
        private string SS_PATH = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FOLDER_NAME, SS_NAME);
        private const string REGION_FILE_NAME = "region.txt";
        private string REGION_FILE_PATH = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FOLDER_NAME, REGION_FILE_NAME);


        private NotifyIcon _notifyIcon;
        private Rect MapRegion { get; set; } = new Rect(0, 0, 0, 0); // Inicializar el mapa de región
        private bool LogicRunning { get; set; } = false; // Variable para controlar el estado de la lógica

        public MainWindow()
        {
            InitializeComponent();
            InitializeSystemTray();
            //HookKeyboard();
            EnsureMainDirectoryExists();
            RefreshMapRegion();
            InitTimer();
        }

        #region Minimizar a la bandeja del sistema
        private void InitializeSystemTray()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(Properties.Resources.Icon, 40, 40), // Ícono de la bandeja
                Visible = true,
                Text = "MacroMechanicsHub",
                BalloonTipIcon = ToolTipIcon.Info,
                BalloonTipTitle = "MacroMechanicsHub",
                BalloonTipText = "Aplicación en ejecución"
            };

            _notifyIcon.DoubleClick += (s, e) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            };

            this.StateChanged += (s, e) =>
            {
                if (this.WindowState == WindowState.Minimized)
                {
                    this.Hide();
                }
            };
        }
        #endregion

        #region Hook global de teclado
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

                // Verificar si la tecla presionada es Tab (código virtual 0x09)
                if (vkCode == 0x09) // Código virtual de la tecla Tab
                {
                    HandleTabPressed();
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }


        protected override void OnClosed(EventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            _notifyIcon.Dispose();
            base.OnClosed(e);
        }
        #endregion

        #region Manejo de eventos
        private void HandleTabPressed()
        {
          //
        }

        private void InitTimer()
        {
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5); // Intervalo de 5 segundos
            timer.Tick += (s, e) =>
            {
                Logic();
            };
            timer.Start();
        }

        private void ShowNotification(string title, string message)
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.ShowBalloonTip(3000); // Mostrar notificación por 3 segundos
        }

        private void EnsureMainDirectoryExists()
        {
            string directoryPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FOLDER_NAME);
            if (!System.IO.Directory.Exists(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            }
        }

        private void Logic()
        {
            if (LogicRunning)
                return;

            LogicRunning = true;
            TakeSSofRegion();
            LogicRunning = false;
        }

        private void TakeSSofRegion()
        {
            if (!IsRegionValid(MapRegion))
            {
                Console.WriteLine("La región no es válida.");
                return;
            }

            var region = MapRegion;

            using (var bmp = new Bitmap((int)region.Width, (int)region.Height))
            {
                using (var graphics = Graphics.FromImage(bmp))
                {
                    graphics.CopyFromScreen((int)region.X, (int)region.Y, 0, 0, bmp.Size);
                }

                bmp.Save(SS_PATH, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private bool IsRegionValid(Rect region)
        {
            return region.Width > 0 && region.Height > 0 && region.X >= 0 && region.Y >= 0;
        }


        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var regionSelector = new RegionSelectorWindow();
            if (regionSelector.ShowDialog() == true)
            {
                var region = regionSelector.GetAdjustedSelectedRegion();
                Console.WriteLine($"Selected Region: {region}");
                SaveMapRegionToFile(region);
            }
        }

        private void RefreshMapRegion()
        {
            this.MapRegion = GetMapRegionFromFile();
        }

        private void SaveMapRegionToFile(Rect region)
        {
            using (var writer = new System.IO.StreamWriter(REGION_FILE_PATH))
            {
                writer.AutoFlush = true;
                writer.WriteLine($"{region.X},{region.Y},{region.Width},{region.Height}");
            }
            RefreshMapRegion();
        }

        private Rect GetMapRegionFromFile()
        {
            if (!System.IO.File.Exists(REGION_FILE_PATH))
            {
                Console.WriteLine("El archivo de región no existe.");
                return new Rect(0, 0, 0, 0);
            }

            using (var reader = new System.IO.StreamReader(REGION_FILE_PATH))
            {
                string line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    Console.WriteLine("El archivo de región está vacío.");
                    return new Rect(0, 0, 0, 0);
                }

                string[] parts = line.Split(',');

                if (parts.Length != 4)
                {
                    Console.WriteLine("El formato del archivo de región es incorrecto.");
                    return new Rect(0, 0, 0, 0);
                }

                if (!double.TryParse(parts[0], out double x) || !double.TryParse(parts[1], out double y) ||
                    !double.TryParse(parts[2], out double width) || !double.TryParse(parts[3], out double height))
                {
                    Console.WriteLine("Error al analizar las coordenadas de la región.");
                    return new Rect(0, 0, 0, 0);
                }

                return new Rect(x, y, width, height);
            }
        }
    }
}
