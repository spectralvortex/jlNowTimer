using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace jlNowTimer
{
    internal class TimeDisk : Shape
    {
                
        public double CentreX
        {
            get { return (double)GetValue(CentreXProperty); }
            set { SetValue(CentreXProperty, value); }
        }
        // Using a DependencyProperty as the backing store for "MyProperty".  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CentreXProperty = DependencyProperty.Register("CentreXProperty", typeof(double), typeof(TimeDisk),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));


        public double CentreY
        {
            get { return (double)GetValue(CentreYProperty); }
            set { SetValue(CentreYProperty, value); }
        }
        public static readonly DependencyProperty CentreYProperty = DependencyProperty.Register("CentreYProperty", typeof(double), typeof(TimeDisk), 
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));


        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("RadiusProperty", typeof(double), typeof(TimeDisk),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double VisibleDegrees
        {
            get { return (double)GetValue(VisibleDegreesProperty); }
            set
            {
                // Dersom vi havner på 360 eller mer, så bikker vi over startpunktet og begynner
                // på ny runde. Da "nuller den ferdige runde seg ut". Det kan kanskje være greit
                // i noen tilfeller, men ikke her. Derfor sørger vi for at om vi havner på 360 eller
                // mer, så forblir vi på full disk.
                if (value >= 360) value = 359.9999;
                SetValue(VisibleDegreesProperty, value);
            }
        }
        public static readonly DependencyProperty VisibleDegreesProperty = DependencyProperty.Register("VisibleDegreesProperty", typeof(double), typeof(TimeDisk),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double RotationAngle
        {
            get { return (double)GetValue(RotationAngleProperty); }
            set { SetValue(RotationAngleProperty, value); }
        }
        public static readonly DependencyProperty RotationAngleProperty = DependencyProperty.Register("RotationAngleProperty", typeof(double), typeof(TimeDisk),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));


        // " Overrides "

        protected override System.Windows.Media.Geometry DefiningGeometry
        {
            get
            {
                // / Create a StreamGeometry for describing the shape.
                StreamGeometry geometry = new();
                using ( StreamGeometryContext context = geometry.Open() )
                {
                    DrawGeometry(context);
                }
                geometry.Freeze();
                return geometry;
            }
        }


        // Methods

        private void DrawGeometry(StreamGeometryContext context)
        {
            if (this.VisibleDegrees > 0)
            {
                Point startPoint = new Point(CentreX, CentreY);
                Point outerArcStartPoint = CartesianCoordinatePoint(RotationAngle, Radius);
                outerArcStartPoint.Offset(CentreX, CentreY);
                Point outerArcEndPoint = CartesianCoordinatePoint(RotationAngle + VisibleDegrees, Radius);
                outerArcEndPoint.Offset(CentreX, CentreY);
                Size outerArcSize = new Size(Radius, Radius);

                bool largeArc = VisibleDegrees > 180.0;

                context.BeginFigure(startPoint, true, true);
                context.LineTo(outerArcStartPoint, true, true);
                context.ArcTo(outerArcEndPoint, outerArcSize, 0, largeArc, SweepDirection.Clockwise, true, true);
                context.LineTo(startPoint, true, true);
            }
        } // DrawGeometry


        private Point CartesianCoordinatePoint(double angle, double radius)
        {
            double angleRad = (Math.PI / 180.0) * (angle - 90);
            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);
            return new Point(x, y);
        } // CartesianCoordinatePoint

    }
}