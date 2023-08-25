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
        , ThermalPrinter_DefaultResolutionDpi
        , $"./outbox/image{DateTime.Now.Ticks}.png"
    );
}

public static async Task RenderToBitmap(
    string pathToXamlFile
    , object dataContext
    , IContentRender renderer
    , double resolutionDpi
    , string saveToPath )
{
    using Stream stream = File.OpenRead( pathToXamlFile );
    using MemoryStream imageStream =
        await renderer.RenderAsync(
            stream, dataContext,
            resolutionDpi, resolutionDpi );

    File.WriteAllBytes( saveToPath, imageStream.GetBuffer() );
}

public const double ThermalPrinter_DefaultResolutionDpi = 203;
public const double ThermalPrinter_HiResolutionDpi = 304;
```
