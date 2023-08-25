# XamlToBitmap.Render

Renders XAML with binded data context into the in-memory bitmap stream.

## How to use

```csharp
public async Task Main()
{
    await RenderToBitmap(
        "./assets/weather-template.xaml"
        // Use streams not strings!
        , JsonSerializer.Deserialize<WeatherForecast>(
            File.ReadAllText( "./data/forecast.json" ) )
        , new XamlRender()
        , $"./outbox/image{DateTime.Now.Ticks}.png"
    );
}

public static async Task RenderToBitmap(
    string pathToXamlFile
    , object dataContext
    , IContentRender renderer
    , string saveToPath )
{
    using Stream stream = File.OpenRead( pathToXamlFile );
    using MemoryStream imageStream =
        await renderer.RenderAsync(
            stream, dataContext,
            XamlRender.ThermalPrinter_CommonDpi, XamlRender.ThermalPrinter_CommonDpi );

    File.WriteAllBytes( saveToPath, imageStream.GetBuffer() );
}

public const double ThermalPrinter_HiResDpi = 304;
```
