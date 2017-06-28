using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

/*
 File->new->Project->C#->WPF App "CardDist"
 download cards.dll from https://onedrive.live.com/redir?resid=D69F3552CEFC21!74629&authkey=!AGaX84aRcmB1fB4&ithint=file%2cDll
 Solution->Add Existing Item Cards.dll (Properties: Copy to Output Directory=Copy If Newer)
 Add Project->Add Reference to System.Drawing
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
            Width = 1100;
            Height = 800;
            Title = "CardDist";
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var sp = new StackPanel() { Orientation = Orientation.Vertical };
                sp.Children.Add(new Label() { Content = "Card Dealing Program. Click to toggle dealing" });
                var canvas = new Canvas();
                sp.Children.Add(canvas);
                this.Content = sp;
                var hghtCard = 100;
                var wdthCard = 80;
                for (var suit = 0; suit < 4; suit++)
                {
                    for (var denom = 0; denom < 13; denom++)
                    {
                        // create a new image for a card
                        var img = new Image()
                        {
                            Source = Cards.GetCard((Cards.Suit)suit, denom),
                            Height = hghtCard
                        };
                        // add it to the canvas
                        canvas.Children.Add(img);
                        // set it's position on the canvas
                        Canvas.SetLeft(img, denom * wdthCard);
                        Canvas.SetTop(img, suit * hghtCard);
                    }
                }
                for (int i = 0; i < Cards.NumCardBacks; i++)
                {
                    var img = new Image()
                    {
                        Source = Cards.GetCardBack(i),
                        Height = hghtCard
                    };
                    canvas.Children.Add(img);
                    Canvas.SetTop(img, hghtCard * 5);
                    Canvas.SetLeft(img, i * wdthCard);
                }
                var rand = new Random(1);
                var timer = new DispatcherTimer(
                    TimeSpan.FromMilliseconds(40),
                    DispatcherPriority.Normal,
                    (o, args) =>
                    {
                        for (int n = 0; n < 52; n++)
                        {
                            //get a random number 0-51
                            var tempNdx = rand.Next(52);
                            // exchange the Image.Source of the nth one with the tempNdx
                            // the child of a canvas is a UIElement, so we need to cast it to an Image
                            var tempSrc = ((Image)canvas.Children[tempNdx]).Source;
                            ((Image)canvas.Children[tempNdx]).Source = ((Image)(canvas.Children[n])).Source;
                            ((Image)canvas.Children[n]).Source = tempSrc;
                        }
                    },
                    this.Dispatcher);
                this.MouseUp += (om, em) =>
                {
                    timer.IsEnabled = !timer.IsEnabled;
                };
            }
            catch (Exception ex)
            {
                this.Content = ex.ToString();
            }
        }
        public class Hand
        {

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

            public static int NumCardBacks => _instance._bitmapCardBacks.Length;

            public Cards()
            {
                _bitmapCards = new BitmapSource[4, 13];
                var hmodCards = LoadLibraryEx("cards.dll", IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
                if (hmodCards == IntPtr.Zero)
                {
                    throw new FileNotFoundException("Couldn't find cards.dll");
                }
                // the cards are resources from 1 - 52.
                // here is a func to load an int rsrc and return it as a BitmapSource
                Func<int, BitmapSource> GetBmpSrc = (rsrc) =>
                {
                    // we first load the bitmap as a native resource, and get a ptr to it
                    var bmRsrc = LoadBitmap(hmodCards, rsrc);
                    // now we create a System.Drawing.Bitmap from the native bitmap
                    var bmp = System.Drawing.Bitmap.FromHbitmap(bmRsrc);
                    // we can now delete the LoadBitmap
                    DeleteObject(bmRsrc);
                    // now we get a handle to a GDI System.Drawing.Bitmap
                    var hbmp = bmp.GetHbitmap();
                    // we can create a WPF Bitmap source now
                    var bmpSrc = Imaging.CreateBitmapSourceFromHBitmap(
                        hbmp,
                        palette: IntPtr.Zero,
                        sourceRect: Int32Rect.Empty,
                        sizeOptions: BitmapSizeOptions.FromEmptyOptions());

                    // we're done with the GDI bmp
                    DeleteObject(hbmp);
                    return bmpSrc;
                };
                // now we call our function for the cards and the backs
                for (Suit suit = Suit.Clubs; suit <= Suit.Spades; suit++)
                {
                    for (int denom = 0; denom < 13; denom++)
                    {
                        _bitmapCards[(int)suit, denom] = GetBmpSrc(13 * (int)suit + denom + 1);
                    }
                }
                //The card backs are from 53 - 65
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
            /// <param name="nDenom">1-13 = A, 2,3,4,J,Q,K</param>
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
