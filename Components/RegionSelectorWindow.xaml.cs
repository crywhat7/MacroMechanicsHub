using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MacroMechanicsHub.Components
{
    public partial class RegionSelectorWindow : Window
    {
        private Point _startPoint;
        private Rectangle _selectionRectangle;
        private Rect SelectedRegion { get; set; }

        public RegionSelectorWindow()
        {
            InitializeComponent();
            Loaded += RegionSelectorWindow_Loaded;
        }

        private void RegionSelectorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Obtener las dimensiones reales de la pantalla
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            Console.WriteLine($"Screen Dimensions: Width={screenWidth}, Height={screenHeight}");

            // Validar si la resolución es típica
            if (!IsTypicalResolution(screenWidth, screenHeight))
            {
                // Aplicar un ajuste para compensar el escalado (asumimos un 125% como ejemplo)
                screenWidth = screenWidth * 1.25;
                screenHeight = screenHeight * 1.25;

                Console.WriteLine($"Adjusted Screen Dimensions: Width={screenWidth}, Height={screenHeight}");
            }

            // Configurar el Canvas para cubrir toda la pantalla
            SelectionCanvas.Width = screenWidth;
            SelectionCanvas.Height = screenHeight;
            MaskCanvas.Width = screenWidth;
            MaskCanvas.Height = screenHeight;

            Console.WriteLine($"Canvas Dimensions: Width={SelectionCanvas.Width}, Height={SelectionCanvas.Height}");

            // Configurar la ventana para ocupar toda la pantalla
            this.Width = screenWidth;
            this.Height = screenHeight;
            this.WindowState = WindowState.Maximized;

            Console.WriteLine($"Window Dimensions: Width={this.Width}, Height={this.Height}");
        }

        private bool IsTypicalResolution(double width, double height)
        {
            // Lista de resoluciones típicas
            var typicalResolutions = new[]
            {
                (1920.0, 1080.0),
                (1366.0, 768.0),
                (1600.0, 900.0),
                (1280.0, 720.0),
                (2560.0, 1440.0),
                (3840.0, 2160.0)
            };

            foreach (var (typicalWidth, typicalHeight) in typicalResolutions)
            {
                if (Math.Abs(width - typicalWidth) < 1 && Math.Abs(height - typicalHeight) < 1)
                {
                    return true; // Resolución coincide con una típica
                }
            }

            return false; // Resolución no es típica
        }

        public Rect GetAdjustedSelectedRegion() {
            var rect = new Rect();
            rect.X = SelectedRegion.X * (SelectionCanvas.Width / SystemParameters.PrimaryScreenWidth);
            rect.Y = SelectedRegion.Y * (SelectionCanvas.Height / SystemParameters.PrimaryScreenHeight);
            rect.Width = SelectedRegion.Width * (SelectionCanvas.Width / SystemParameters.PrimaryScreenWidth);
            rect.Height = SelectedRegion.Height * (SelectionCanvas.Height / SystemParameters.PrimaryScreenHeight);

            if (IsTypicalResolution(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight))
            {
                rect.X = SelectedRegion.X;
                rect.Y = SelectedRegion.Y;
                rect.Width = SelectedRegion.Width;
                rect.Height = SelectedRegion.Height;
            }

            return rect;

        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Eliminar cualquier rectángulo existente
            if (_selectionRectangle != null)
            {
                SelectionCanvas.Children.Remove(_selectionRectangle);
                _selectionRectangle = null;
            }

            // Registrar el punto inicial
            _startPoint = e.GetPosition(SelectionCanvas);

            // Crear un nuevo rectángulo
            _selectionRectangle = new Rectangle
            {
                Stroke = Brushes.Green,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(50, 43, 92, 30))
            };

            // Agregar el rectángulo al Canvas
            Canvas.SetLeft(_selectionRectangle, _startPoint.X);
            Canvas.SetTop(_selectionRectangle, _startPoint.Y);
            SelectionCanvas.Children.Add(_selectionRectangle);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _selectionRectangle != null)
            {
                // Obtener la posición actual del mouse
                var currentPoint = e.GetPosition(SelectionCanvas);

                // Validar que las coordenadas estén dentro de los límites del Canvas
                double screenWidth = SelectionCanvas.Width;
                double screenHeight = SelectionCanvas.Height;

                currentPoint.X = Math.Max(0, Math.Min(currentPoint.X, screenWidth));
                currentPoint.Y = Math.Max(0, Math.Min(currentPoint.Y, screenHeight));

                // Calcular las coordenadas y dimensiones del rectángulo
                var x = Math.Min(currentPoint.X, _startPoint.X);
                var y = Math.Min(currentPoint.Y, _startPoint.Y);
                var width = Math.Abs(currentPoint.X - _startPoint.X);
                var height = Math.Abs(currentPoint.Y - _startPoint.Y);

                // Asegurarnos de que las dimensiones no excedan los límites del Canvas
                width = Math.Min(width, screenWidth - x);
                height = Math.Min(height, screenHeight - y);

                // Actualizar la posición y dimensiones del rectángulo
                Canvas.SetLeft(_selectionRectangle, x);
                Canvas.SetTop(_selectionRectangle, y);
                _selectionRectangle.Width = width;
                _selectionRectangle.Height = height;

                // Actualizar la máscara
                UpdateMask(x, y, width, height);
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_selectionRectangle != null)
            {
                // Guardar las coordenadas y dimensiones del rectángulo seleccionado
                var x = Canvas.GetLeft(_selectionRectangle);
                var y = Canvas.GetTop(_selectionRectangle);
                var width = _selectionRectangle.Width;
                var height = _selectionRectangle.Height;

                Console.WriteLine($"Selected Region: X={x}, Y={y}, Width={width}, Height={height}");

                SelectedRegion = new Rect(x, y, width, height);

                // Mostrar el botón de confirmación
                ConfirmButton.Visibility = Visibility.Visible;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; // Cierra la ventana y confirma la selección
        }

        private void UpdateMask(double x, double y, double width, double height)
        {
            // Limpia la máscara actual
            MaskCanvas.Children.Clear();

            // Crea rectángulos para las áreas fuera del rectángulo de selección
            var topRect = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Width = MaskCanvas.Width,
                Height = y
            };

            var bottomRect = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Width = MaskCanvas.Width,
                Height = MaskCanvas.Height - (y + height)
            };
            Canvas.SetTop(bottomRect, y + height);

            var leftRect = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Width = x,
                Height = height
            };
            Canvas.SetTop(leftRect, y);

            var rightRect = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Width = MaskCanvas.Width - (x + width),
                Height = height
            };
            Canvas.SetTop(rightRect, y);
            Canvas.SetLeft(rightRect, x + width);

            // Agrega los rectángulos a la máscara
            MaskCanvas.Children.Add(topRect);
            MaskCanvas.Children.Add(bottomRect);
            MaskCanvas.Children.Add(leftRect);
            MaskCanvas.Children.Add(rightRect);
        }
    }
}
