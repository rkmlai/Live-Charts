﻿//The MIT License(MIT)

//Copyright(c) 2015 Alberto Rodriguez

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Charts.Charts;
using Charts.Shapes;

namespace Charts.Series
{
    public class PieSerie : Serie
    {
        public override ObservableCollection<double> PrimaryValues { get; set; }
        public string[] Labels { get; set; } 

        public override void Plot(bool animate = true)
        {
            var pChart = Chart as PieChart;
            if (pChart == null) return;
            if (pChart.PieSum <= 0) return;
            var rotated = 0d;

            Chart.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var minDimension = Chart.DesiredSize.Width < Chart.DesiredSize.Height 
                ? Chart.DesiredSize.Width : Chart.DesiredSize.Height;
            minDimension -= 40;//padding
            minDimension = minDimension < 40 ? 40 : minDimension;

            var sliceId = 0;
            for (int index = 0; index < PrimaryValues.Count; index++)
            {
                var value = PrimaryValues[index];
                var participation = value/pChart.PieSum;
                if (index == 0) rotated = participation *-.5;

                var slice = new PieSlice
                {
                    CentreX = 0,
                    CentreY = 0,
                    RotationAngle = 360 * rotated,
                    WedgeAngle = 360 * participation,
                    Radius = minDimension / 2,
                    InnerRadius = pChart.InnerRadius,
                    Fill = new SolidColorBrush {Color = GetColorByIndex(sliceId), Opacity = .8},
                    Stroke = Chart.Background,
                    StrokeThickness = pChart.SlicePadding
                };

                var wa = new DoubleAnimation
                {
                    From = 0,
                    To = slice.WedgeAngle,
                    Duration = TimeSpan.FromMilliseconds(300)
                };
                var ra = new DoubleAnimation
                {
                    From = 0,
                    To = slice.RotationAngle,
                    Duration = TimeSpan.FromMilliseconds(300)
                };

                Canvas.SetTop(slice, Chart.ActualHeight/2);
                Canvas.SetLeft(slice, Chart.ActualWidth/2);

                Chart.Canvas.Children.Add(slice);
                Shapes.Add(slice);

                if (!Chart.DisableAnimation)
                {
                    if (animate)
                    {
                        slice.BeginAnimation(PieSlice.WedgeAngleProperty, wa);
                        slice.BeginAnimation(PieSlice.RotationAngleProperty, ra);
                    }
                }

                if (Chart.Hoverable)
                {
                    slice.MouseEnter += Chart.OnDataMouseEnter;
                    slice.MouseLeave += Chart.OnDataMouseLeave;
                    Chart.HoverableShapes.Add(new HoverableShape
                    {
                        Serie = this,
                        Shape = slice,
                        Target = slice,
                        Value = new Point(0, value),
                        Label = Labels.Length > index ? Labels[index] : ""
                    });
                }

                sliceId++;
                rotated += participation;
            }
        }
    }
}
