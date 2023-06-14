using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace RGB.Util;

public partial class ColorPicker : ContentView
{
    private static readonly SKPoint[] vertices = new SKPoint[180 * 3];
    private static readonly SKColor[] colors = new SKColor[180 * 3];
    private static readonly SKPaint paint = new SKPaint()
    {
        Style = SKPaintStyle.Fill,
        Color = new SKColor(255, 255, 255),
        IsAntialias = true,
    };

    public static readonly BindableProperty PickedColorProperty = BindableProperty.Create(
            nameof(PickedColor),
            typeof(Color),
            typeof(ColorPicker), null, BindingMode.TwoWay, coerceValue: (bindable, value) =>
            {
                var picker = (ColorPicker)bindable;
                if (!picker.Inited && value != null)
                {
                    float val = 0;
                    ColorUtil.ColorToHSV((Color)value, out picker.hue, out picker.saturation, out val);

                    picker.BrightnessSlider.Value = val;
                    picker.Inited = true;
                }

                return value;
            });


    public Color PickedColor
    {
        get => (Color)GetValue(PickedColorProperty);
        set
        {
            SetValue(PickedColorProperty, value);
            OnPropertyChanged("PickedColor");
            CanvasView.InvalidateSurface();
        }
    }

    private float hue = 0;
    private float saturation = 1;
    private int w = 0;
    private int h = 0;
    private float r = 0;
    public bool Inited = false;

    public ColorPicker()
    {
        for (int i = 0; i < 180; i++)
        {
            vertices[i * 3 + 0] = new SKPoint(1, 1);
            vertices[i * 3 + 1] = new SKPoint(((float)Math.Cos(i / 90f * Math.PI) + 1), (float)(Math.Sin(i / 90f * Math.PI) + 1));
            vertices[i * 3 + 2] = new SKPoint(((float)Math.Cos((i + 1) / 90f * Math.PI) + 1), (float)(Math.Sin((i + 1) / 90f * Math.PI) + 1));
            colors[i * 3 + 0] = new SKColor(255, 255, 255);
            colors[i * 3 + 1] = SKColor.FromHsv(i * 2, 100, 100);
            colors[i * 3 + 2] = SKColor.FromHsv((i + 1) * 2, 100, 100);
        }

        InitializeComponent();
        CanvasView.InvalidateSurface();
        var tapGestureRecognizer = new TapGestureRecognizer();
        var panGestureRecognizer = new PanGestureRecognizer();
        tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
        panGestureRecognizer.PanUpdated += PanGestureRecognizer_PanUpdated;
        GestureRecognizers.Add(tapGestureRecognizer);
        GestureRecognizers.Add(panGestureRecognizer);
    }

    private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
    {

    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {

    }


    private void CanvasView_Touch(object sender, SkiaSharp.Views.Maui.SKTouchEventArgs e)
    {
        if (!e.InContact) return;

        e.Handled = true;

        float x = e.Location.X;
        float y = e.Location.Y;
        float dx = x - w / 2;
        float dy = y - r;

        hue = (float)(Math.Atan2(-dy, -dx) / Math.PI * 180 + 180);
        saturation = (float)Math.Min(1, Math.Sqrt(dx * dx + dy * dy) / r);

        UpdateColor();

        CanvasView.InvalidateSurface();
    }

    private void CanvasView_PaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        SKCanvas ctx = e.Surface.Canvas;
        w = e.Info.Width;
        h = e.Info.Height;
        r = Math.Min(w, h) / 2;

        ctx.Clear();
        ctx.Translate(w / 2 - r, 0);
        ctx.Scale(r);
        ctx.DrawVertices(SKVertexMode.Triangles, vertices, colors, paint);

        SKPoint point = new SKPoint(((float)Math.Cos(hue / 180 * Math.PI) * saturation + 1), ((float)Math.Sin(hue / 180 * Math.PI) * saturation + 1));

        ctx.DrawCircle(point, .03f, new SKPaint() { Color = new SKColor(10, 10, 10), Style = SKPaintStyle.Fill, IsAntialias = true });
        ctx.DrawCircle(point, .02f, new SKPaint() { Color = SKColor.FromHsv(hue, saturation * 100, 100), Style = SKPaintStyle.Fill, IsAntialias = true });
    }

    private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        UpdateColor();
    }

    void UpdateColor()
    {
        SKColor color = SKColor.FromHsv(hue, saturation * 100, (float)BrightnessSlider.Value * 100);
        PickedColor = color.ToMauiColor();
    }
}