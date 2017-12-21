using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HawkeyeLogo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string DefaultDirectory = @"c:\temp";

        /// <summary>
        /// The exported size property
        /// </summary>
        public static readonly DependencyProperty ExportedSizeProperty = DependencyProperty.Register(
            "ExportedSize", typeof(int), typeof(MainWindow), new PropertyMetadata(256));

        /// <summary>
        /// The save all sizes property
        /// </summary>
        public static readonly DependencyProperty SaveAllSizesProperty = DependencyProperty.Register(
            "SaveAllSizes", typeof(bool?), typeof(MainWindow), new PropertyMetadata(true));

        /// <summary>
        /// The output directory property
        /// </summary>
        public static readonly DependencyProperty OutputDirectoryProperty = DependencyProperty.Register(
            "OutputDirectory", typeof(string), typeof(MainWindow), new PropertyMetadata(DefaultDirectory));

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        /// <summary>
        /// Gets or sets the size of the exported.
        /// </summary>
        /// <value>
        /// The size of the exported.
        /// </value>
        public int ExportedSize
        {
            get => (int) GetValue(ExportedSizeProperty);
            set => SetValue(ExportedSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the save all sizes.
        /// </summary>
        /// <value>
        /// The save all sizes.
        /// </value>
        public bool? SaveAllSizes
        {
            get => (bool?) GetValue(SaveAllSizesProperty);
            set => SetValue(SaveAllSizesProperty, value);
        }

        /// <summary>
        /// Gets or sets the output directory.
        /// </summary>
        /// <value>
        /// The output directory.
        /// </value>
        public string OutputDirectory
        {
            get => (string) GetValue(OutputDirectoryProperty);
            set => SetValue(OutputDirectoryProperty, value);
        }

        private void Save(int size)
        {
            var controls = new Control[]
            {
                h1, h2, h3, h4
            };

            Cursor saved = Cursor;
            Cursor = Cursors.Wait;
            try
            {
                foreach (Control control in controls)
                {
                    string filename = Path.Combine(OutputDirectory, control.Name + "_" + size + @".png");
                    control.SaveTo(size, size, filename);
                }
            }
            finally
            {
                Cursor = saved;
            }
        }

        private void SaveAll()
        {
            var sizes = new[] {16, 24, 32, 64, 128, 256, 512, 1024};
            foreach (int size in sizes)
            {
                Save(size);
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveAllSizes.HasValue && SaveAllSizes.Value)
            {
                SaveAll();
            }
            else
            {
                Save(ExportedSize);
            }
        }
    }
}