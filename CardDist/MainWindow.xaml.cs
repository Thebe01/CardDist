using System;
using System.Collections.Generic;
using System.Drawing;
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
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = new Cards(this);

            }
            catch (Exception ex)
            {
                this.Content = ex.ToString();
            }
        }

        public class Cards
        {

            public Cards(Window wind)
            {
                //IntPtr hToken;
                //StartupInput sinput = new StartupInput() { GdiplusVersion = 1, DebugEventCallback = IntPtr.Zero, SuppressBackgroundThread = false, SuppressExternalCodecs = false };
                //StartupOutput soutput = new StartupOutput() { hook = IntPtr.Zero, unhook = IntPtr.Zero };
                //GdiplusStartup(out hToken, ref sinput, out soutput);
                var hmodCards = LoadLibraryEx("cards.dll", IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
                if (hmodCards == IntPtr.Zero)
                {
                    throw new FileNotFoundException("Couldn't find cards.dll");
                }
                var bmRes = FindResource(hmodCards, 1, 2);
                var bmSize = SizeofResource(hmodCards, bmRes);
                var bmIntPtr = LoadResource(hmodCards, bmRes);
                var hb = LoadBitmap(hmodCards, 1);

                //var bytes = new byte[bmSize];
                //Marshal.Copy(bmIntPtr, bytes, 0, bmSize);
                //using (var msb = new MemoryStream())
                //{
                //    var bi = new BitmapImage();
                //    bi.BeginInit();
                //    bi.StreamSource = msb;
                //    bi.EndInit();
                //    var ximg = new System.Windows.Controls.Image();
                //    ximg.Source = bi;
                //    wind.Content = ximg;
                //}
                //return;

                //var bytes = new byte[bmSize];
                //Marshal.Copy(hmodCards, bytes, 0, bmSize);
                //var sss = Bitmap.FromHbitmap(bmIntPtr);

                //using (var mstr = new MemoryStream(bytes))
                //{
                //    var bmp = (Bitmap)Bitmap.FromStream(mstr);
                //}


                var bmSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hb, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                //                var bmSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hb, IntPtr.Zero, new Int32Rect(0, 0, 30, 30), BitmapSizeOptions.FromWidthAndHeight(30, 30));
                var img = new System.Windows.Controls.Image();
                img.Source = bmSrc;
                img.Height = 100;
                img.Width = 100;
                //var sp = new StackPanel()
                //{
                //    Orientation = Orientation.Vertical
                //};
                //sp.Children.Add(new TextBlock() { Text = "top" });
                //sp.Children.Add(img);
                //sp.Children.Add(new TextBlock() { Text = "bottom" });
                wind.Content = img;

                //var imsrc = new BitmapImage(new Uri(@"C:\t.jpg"));
                //var image = new System.Windows.Controls.Image()
                //{
                //    Source = imsrc
                //};
                //wind.Content = image;


                //var xx = Bitmap.FromResource(hmodCards, "10");
                //var bytes = new byte[bmSize];
                //Marshal.Copy(bmIntPtr, bytes, 0, bmSize);
                //var sss = Bitmap.FromHbitmap(bmIntPtr);

                //using (var mstr = new MemoryStream(bytes))
                //{
                //    var bmp = (Bitmap)Bitmap.FromStream(mstr);
                //}
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
