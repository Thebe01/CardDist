using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

/*
 File->new->Project->C#->WPF App "CardDist"
 Solution->Add Existing Item Cards.dll (Properties: Copy to Output Directory=Copy If Newer)
 Add reference to System.Drawing
     * 
     * * */
namespace CardDist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Width = 900;
            Height = 900;
            WindowState = WindowState.Maximized;
            this.Loaded += MainWindow_Loaded;
        }
        Image[,] _images;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _images = new Image[4, 13];
                var canvas = new Canvas();
                this.Content = canvas;
                for (var suit = 0; suit < 4; suit++)
                {
                    for (var denom = 0; denom < 13; denom++)
                    {
                        var img = new Image()
                        {
                            Source = Cards.GetCard((Cards.Suit)suit, denom),
                            //                            Height = 100
                        };
                        canvas.Children.Add(img);
                        Canvas.SetLeft(img, denom * 100);
                        Canvas.SetTop(img, suit * 100);
                        _images[suit, denom] = img;

                    }

                }
                //var img = new System.Windows.Controls.Image();
                //img.Source = Cards.GetCard(Cards.Suit.Diamonds, 4);
                //img.Height = 200;
                ////img.Width = 500;
                //var sp = new StackPanel()
                //{
                //    Orientation = Orientation.Vertical
                //};
                //sp.Children.Add(new TextBlock() { Text = "top" });
                //sp.Children.Add(img);
                //sp.Children.Add(new TextBlock() { Text = "bottom" });
                //this.Content = sp;

            }
            catch (Exception ex)
            {
                this.Content = ex.ToString();
            }
        }

        public class Cards
        {
            public enum Suit
            {
                Clubs = 0,
                Diamonds = 1,
                Hearts = 2,
                Spades = 3
            }
            private BitmapSource[,] _bitmapCards;
            private static Cards _instance;
            public Cards()
            {
                _bitmapCards = new BitmapSource[4, 13];
                var hmodCards = LoadLibraryEx("cards.dll", IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
                if (hmodCards == IntPtr.Zero)
                {
                    throw new FileNotFoundException("Couldn't find cards.dll");
                }
                for (Suit suit = Suit.Clubs; suit <= Suit.Spades; suit++)
                {
                    for (int denom = 0; denom < 13; denom++)
                    {
//                        var bmRsrc = LoadBitmap(hmodCards, 12);
                        var bmRsrc = LoadBitmap(hmodCards, 13 * (int)suit + denom + 1);
                        var bmpSrc = Imaging.CreateBitmapSourceFromHBitmap(
                            bmRsrc,
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());

                        var x = bmpSrc.Palette;
                        if (x == null)
                        {
                            var wbmp = new WriteableBitmap(bmpSrc);
                            var arr = new byte[(int)wbmp.Width * (int)wbmp.Height * wbmp.Format.BitsPerPixel];
                            wbmp.CopyPixels(arr, wbmp.Format.BitsPerPixel * (int)wbmp.Width, 0);
                            var lstColors = new[] { Colors.Red, Colors.Blue, Colors.Green };
                            BitmapPalette pal = new BitmapPalette(lstColors);
                            wbmp = new WriteableBitmap((int)wbmp.Width, (int)wbmp.Height, 96, 96, PixelFormats.Bgra32, null);
                            wbmp.WritePixels(new Int32Rect(0, 0, (int)wbmp.Width, (int)wbmp.Height), arr, wbmp.Format.BitsPerPixel * (int)wbmp.Width, 0);
                            bmpSrc = wbmp;
                        }
                        _bitmapCards[(int)suit, denom] = bmpSrc;
                    }
                }
            }

            /// <summary>
            /// Return a BitmapSource
            /// </summary>
            /// <param name="nSuit"></param>
            /// <param name="nDenom">1-13 = 2,3,4,J,Q,K,A</param>
            /// <returns></returns>
            public static BitmapSource GetCard(Suit nSuit, int nDenom)
            {
                if (_instance == null)
                {
                    _instance = new Cards();
                }
                if (nDenom < 0 || nDenom > 12)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return _instance._bitmapCards[(int)nSuit, nDenom];
            }

        }

        public const int LOAD_LIBRARY_AS_DATAFILE = 2;
        [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        static extern int GdiplusStartup(out IntPtr token, ref StartupInput input,
                out StartupOutput output);
        [StructLayout(LayoutKind.Sequential)]
        struct StartupOutput
        {
            public IntPtr hook;
            public IntPtr unhook;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct StartupInput
        {
            public int GdiplusVersion;
            public IntPtr DebugEventCallback;
            public bool SuppressBackgroundThread;
            public bool SuppressExternalCodecs;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFileReserved, uint dwFlags);
        [DllImport("User32.dll")]
        public static extern IntPtr LoadImage(IntPtr hInstance, int uID, uint type, int width, int height, int load);
        [DllImport("User32.dll")]
        public static extern IntPtr LoadBitmap(IntPtr hInstance, int uID);
        [DllImport("kernel32.dll")]
        static extern IntPtr FindResource(IntPtr hModule, int lpName, int lpType);
        [DllImport("kernel32.dll")]
        static extern IntPtr FindResource(IntPtr hModule, string lpName, string lpType);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int SizeofResource(IntPtr hModule, IntPtr hResInfo);
    }
}
