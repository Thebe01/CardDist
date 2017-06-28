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
using System.Windows.Threading;

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
        Image[] _images;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _images = new Image[52];
                var canvas = new Canvas();
                this.Content = canvas;
                var hgt = 100;
                for (var suit = 0; suit < 4; suit++)
                {
                    for (var denom = 0; denom < 13; denom++)
                    {
                        var img = new Image()
                        {
                            Source = Cards.GetCard((Cards.Suit)suit, denom),
                            Height = hgt
                        };
                        canvas.Children.Add(img);
                        Canvas.SetLeft(img, denom * 100);
                        Canvas.SetTop(img, suit * hgt);
                        _images[suit * 4 + denom] = img;

                    }
                }
                for (int i = 0; i < Cards.NumBacks; i++)
                {
                    var img = new Image()
                    {
                        Source = Cards.GetCardBack(i),
                        Height = hgt
                    };
                    canvas.Children.Add(img);
                    Canvas.SetTop(img, hgt * 5);
                    Canvas.SetLeft(img, i * 100);
                }
                var rand = new Random(1);
                var timer = new DispatcherTimer(
                    TimeSpan.FromMilliseconds(100),
                    DispatcherPriority.Normal,
                    (o, args) =>
                    {
                        //                        canvas.Children.Clear();
                        for (int i = 0; i < 52; i++)
                        {
                            //int nSuit = i / 13;
                            //int nDenom = i - nSuit * 13;

                            var tempNdx = rand.Next(52);
                            var tempSrc = ((Image)canvas.Children[tempNdx]).Source;
                            ((Image)canvas.Children[tempNdx]).Source = ((Image)(canvas.Children[i])).Source;
                            ((Image)canvas.Children[i]).Source = tempSrc;

                            //var temp = canvas.Children[tempNdx];
                            //canvas.Children[tempNdx] = canvas.Children[i];
                            //canvas.Children[i] = temp;
                            //var temp = _images[tempNdx];
                            //_images[tempNdx] = _images[i];
                            //_images[i] = temp;
                        }
                        foreach (var img in _images)
                        {

                        }
                        //for (var suit = 0; suit < 4; suit++)
                        //{
                        //    for (var denom = 0; denom < 13; denom++)
                        //    {
                        //        _im
                        //    }
                        //}

                    },
                    this.Dispatcher);
                //                timer.Start();
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
            public BitmapSource[] _bitmapCardBacks;
            private static Cards _instance;

            public static int NumBacks { get { return _instance._bitmapCardBacks.Length; } }

            public Cards()
            {
                _bitmapCards = new BitmapSource[4, 13];
                var hmodCards = LoadLibraryEx("cards.dll", IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
                if (hmodCards == IntPtr.Zero)
                {
                    throw new FileNotFoundException("Couldn't find cards.dll");
                }
                // the cards are resources from 1 - 52. The other images (like card backs) are from 53 - 65
                Func<int, BitmapSource> GetBmpSrc = (rsrc) =>
                {
                    // we first load the bitmap as a native resource, and get a ptr to it
                    var bmRsrc = LoadBitmap(hmodCards, rsrc);
                    // now we create a System.Drawing.Bitmap from the native bitmap
                    var bmp = System.Drawing.Bitmap.FromHbitmap(bmRsrc);
                    // we can now delete the LoadBitmap
                    DeleteObject(bmRsrc);
                    // now we get a GDI bitmap object from the System.Drawing.Bitmap
                    var hbmp = bmp.GetHbitmap();
                    // we can create a WPF Bitmap source now
                    var bmpSrc = Imaging.CreateBitmapSourceFromHBitmap(
                        hbmp,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    // we're done with the GDI bmp
                    DeleteObject(hbmp);
                    return bmpSrc;
                };
                for (Suit suit = Suit.Clubs; suit <= Suit.Spades; suit++)
                {
                    for (int denom = 0; denom < 13; denom++)
                    {
                        _bitmapCards[(int)suit, denom] = GetBmpSrc(13 * (int)suit + denom + 1);
                    }
                }
                _bitmapCardBacks = new BitmapSource[65 - 53 + 1];
                for (int i = 53; i <= 65; i++)
                {
                    _bitmapCardBacks[i - 53] = GetBmpSrc(i);
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

            internal static ImageSource GetCardBack(int i)
            {
                return _instance._bitmapCardBacks[i];
            }
        }

        public const int LOAD_LIBRARY_AS_DATAFILE = 2;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFileReserved, uint dwFlags);

        [DllImport("User32.dll")]
        public static extern IntPtr LoadBitmap(IntPtr hInstance, int uID);

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);
    }
}
