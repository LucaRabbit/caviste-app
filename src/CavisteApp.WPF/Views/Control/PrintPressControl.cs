using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CavisteApp.WPF.Views.Control
{
    public partial class PrintPressControl : UserControl
    {
        private DispatcherTimer _timer;
        private double _t = 0;
        private DrawingVisual _visual;

        public PrintPressControl() => InitializeComponent();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _visual = new DrawingVisual();
            var host = new VisualHost(_visual);
            MainCanvas.Children.Add(host);
            Canvas.SetLeft(host, 0); Canvas.SetTop(host, 0);

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += (s, _) => { _t++; Render(); };
            _timer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) => _timer?.Stop();

        private void Render()
        {
            double W = MainCanvas.ActualWidth;
            double H = MainCanvas.ActualHeight;
            if (W < 1 || H < 1) return;

            // Met à jour la taille du host
            if (MainCanvas.Children[0] is VisualHost vh)
            {
                vh.Width = W; vh.Height = H;
            }

            using var dc = _visual.RenderOpen();

            // Fond
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0xDD, 0xE6, 0xED)), null, new Rect(0, 0, W, H));

            double cx = W / 2;
            double cy = H / 2 + H * 0.05;

            // Balancement
            double swing = Math.Sin(_t * 0.04) * 18.0;
            double swingRad = swing * Math.PI / 180.0;
            double waveShift = Math.Sin(_t * 0.04) * 14.0;

            // Transformation rotation autour du centre du verre
            var transform = new RotateTransform(swing, cx, cy);
            dc.PushTransform(transform);

            double scale = Math.Min(W, H) / 480.0;
            var sc = new ScaleTransform(scale, scale, cx, cy);
            dc.PushTransform(sc);

            // ── Pen traits blancs ──────────────────────────────────────────
            var pen = new Pen(Brushes.Black, 2.2)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round
            };

            // ── VIN (dessiné AVANT le contour pour qu'il soit derrière) ───
            var wineGeom = BuildWineGeometry(cx, cy, waveShift);
            var wineClip = BuildChaliceGeometry(cx, cy);
            dc.PushClip(new GeometryGroup { Children = { wineClip } }
                         .GetFlattenedPathGeometry());
            // plus simple : on clip avec la géométrie du calice
            dc.PushClip(wineClip);
            dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(0xB8, 0x10, 0x30)),
                            null, wineGeom);
            // Reflet
            var reflectBrush = new SolidColorBrush(Color.FromArgb(100, 220, 80, 110));
            dc.DrawEllipse(reflectBrush, null,
                new Point(cx - 15, cy + 10 + waveShift * 0.5 + 6 - 80), 18, 5);
            dc.Pop(); // clip vin
            dc.Pop(); // clip calice

            // ── CALICE (contour blanc) ─────────────────────────────────────
            var chalice = BuildChaliceGeometry(cx, cy);
            dc.DrawGeometry(null, pen, chalice);

            // Bord ouverture
            dc.DrawEllipse(null, pen, new Point(cx, cy - 80), 52, 10);

            // ── TIGE ──────────────────────────────────────────────────────
            var stem = new PathGeometry();
            var sf = new PathFigure { StartPoint = new Point(cx - 6, cy + 80) };
            sf.Segments.Add(new BezierSegment(new Point(cx - 5, cy + 110),
                                               new Point(cx + 5, cy + 110),
                                               new Point(cx + 6, cy + 80), true));
            sf.Segments.Add(new LineSegment(new Point(cx + 6, cy + 150), true));
            sf.Segments.Add(new LineSegment(new Point(cx - 6, cy + 150), true));
            sf.IsClosed = true;
            stem.Figures.Add(sf);
            dc.DrawGeometry(null, pen, stem);

            // ── BASE ──────────────────────────────────────────────────────
            dc.DrawEllipse(null, pen, new Point(cx, cy + 152), 38, 7);

            dc.Pop(); // scale
            dc.Pop(); // rotation
            var ft = new FormattedText(
        "Projet Caviste",
        System.Globalization.CultureInfo.CurrentCulture,
        FlowDirection.LeftToRight,
        new Typeface("Segoe UI Light"),
        Math.Min(W, H) * 0.055,
        Brushes.Black,
        VisualTreeHelper.GetDpi(this).PixelsPerDip);

            dc.DrawText(ft, new Point(cx - ft.Width / 2, cy - H * 0.38));
        }

        // ── Géométrie du calice ────────────────────────────────────────────
        private static PathGeometry BuildChaliceGeometry(double cx, double cy)
        {
            var pg = new PathGeometry();
            var fig = new PathFigure { StartPoint = new Point(cx - 52, cy - 80) };
            fig.Segments.Add(new BezierSegment(
                new Point(cx - 62, cy - 20),
                new Point(cx - 55, cy + 60),
                new Point(cx - 10, cy + 80), true));
            fig.Segments.Add(new LineSegment(new Point(cx + 10, cy + 80), true));
            fig.Segments.Add(new BezierSegment(
                new Point(cx + 55, cy + 60),
                new Point(cx + 62, cy - 20),
                new Point(cx + 52, cy - 80), true));
            fig.IsClosed = false;
            pg.Figures.Add(fig);
            return pg;
        }

        // ── Géométrie du vin (trapèze incliné) ────────────────────────────
        private static PathGeometry BuildWineGeometry(double cx, double cy, double waveShift)
        {
            double surfY = cy + 10;
            var pg = new PathGeometry();
            var fig = new PathFigure
            {
                StartPoint = new Point(cx - 70, surfY + waveShift)
            };
            fig.Segments.Add(new LineSegment(new Point(cx + 70, surfY - waveShift), true));
            fig.Segments.Add(new LineSegment(new Point(cx + 70, cy + 100), true));
            fig.Segments.Add(new LineSegment(new Point(cx - 70, cy + 100), true));
            fig.IsClosed = true;
            pg.Figures.Add(fig);
            return pg;
        }
    }

    // ── VisualHost : expose un DrawingVisual au Canvas ─────────────────────
    internal sealed class VisualHost : FrameworkElement
    {
        private readonly DrawingVisual _v;
        public VisualHost(DrawingVisual v) { _v = v; AddVisualChild(v); AddLogicalChild(v); }
        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index) => _v;
    }
}