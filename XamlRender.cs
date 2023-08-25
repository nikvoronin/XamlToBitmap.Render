using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace XamlToBitmap.Render
{    
    public interface IContentRender
    {
        /// <summary>
        /// Render XAML template with binded data context
        /// </summary>
        /// <param name="stream">Stream with content of XAML template</param>
        /// <param name="dataContext">Data object to bind to XAML template</param>
        /// <param name="dpiX">Horizontal resolution by X</param>
        /// <param name="dpiY">Vertical resolution by Y</param>
        /// <returns>Binary stream with PNG image content</returns>
        public Task<MemoryStream> RenderAsync(
            Stream stream,
            object dataContext,
            double dpiX, double dpiY );
    }
    
    public class XamlRender : IContentRender
    {
        public static FrameworkElement LoadFrameworkElement( Stream stream )
            => XamlReader.Load( stream ) as FrameworkElement
            ?? throw new Exception( "Can not load XAML! Root have to be instance of the FrameworkElement class." );

        public static MemoryStream RenderContent(
            FrameworkElement content,
            double dpiX, double dpiY )
        {
            var widthPx =
                content.ActualWidth
                .ToPixelsExact( dpiX );

            var heightPx =
                content.ActualHeight
                .ToPixelsExact( dpiY );

            // Negative margins in XAML might lead to issues
            // when a content of printing lays out of template bounds
            var targetBitmap =
                new RenderTargetBitmap(
                    widthPx, heightPx,
                    dpiX, dpiY,
                    PixelFormats.Pbgra32 );

            targetBitmap.Render( content );

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add( BitmapFrame.Create( targetBitmap ) );

            MemoryStream mem = new();
            encoder.Save( mem );

            return mem;
        }

        public static MemoryStream Render(
            Stream stream, object dataContext,
            double dpiX, double dpiY )
        {
            var root =
                LoadFrameworkElement( stream )
                .AssignDataContext( dataContext )
                .ResizeLayout();

            var mem = RenderContent( root, dpiX, dpiY );

            root.DataContext = null;
            root.RaiseEvent( new RoutedEventArgs( FrameworkElement.UnloadedEvent ) );
            root.Dispatcher.InvokeShutdown();

            return mem;
        }

        public async Task<MemoryStream> RenderAsync(
            Stream stream, object dataContext,
            double dpiX, double dpiY )
        {
            MemoryStream mem =
                await StaTask.Run(
                    () => Render( stream, dataContext, dpiX, dpiY ) );

            return mem
                ?? throw new Exception( "Can not render source." );
        }

        public const float WindowsScreen_DefaultDpi = 96f;
        public const float ThermalPrinter_CommonDpi = 203f;
    }

    public static class StaTask
    {
        public static Task<T> Run<T>( Func<T> func )
        {
            var tcs = new TaskCompletionSource<T>();
            Thread thread = new( () => {
                try {
                    tcs.SetResult( func() );
                }
                catch ( Exception e ) {
                    tcs.SetException( e );
                }
            } );
            thread.SetApartmentState( ApartmentState.STA );
            thread.Start();

            return tcs.Task;
        }
    }

    file static class Extensions
    {
        public static int ToPixelsExact( this double actualSize, double dpi )
            => (int)( actualSize * ( dpi / XamlRender.WindowsScreen_DefaultDpi ) );

        public static FrameworkElement AssignDataContext( this FrameworkElement e, object dataContext )
        {
            e.DataContext = dataContext;
            return e;
        }

        public static FrameworkElement ResizeLayout( this FrameworkElement content )
        {
            content.Measure(
                new Size(
                    double.PositiveInfinity
                    , double.PositiveInfinity ) );

            content.Arrange(
                new Rect( content.DesiredSize ) );

            content.UpdateLayout();

            return content;
        }
    }
}
