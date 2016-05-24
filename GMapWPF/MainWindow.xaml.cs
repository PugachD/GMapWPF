using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMapWPF;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using GMapWPF.CustomMarker;

namespace GMapWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        PointLatLng start;
        PointLatLng end;
        // marker
        GMapMarker currentMarker, startMarker, endMarker;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            gMapControl1.Bearing = 0;
            gMapControl1.CanDragMap = true;
            gMapControl1.DragButton = MouseButton.Left;

            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.Position = new PointLatLng(45.03333, 41.96667);
            gMapControl1.FromLatLngToLocal(gMapControl1.Position);

            currentMarker = new GMapMarker(gMapControl1.Position);
            {
                currentMarker.Shape = new CustomMarkerRed(this, currentMarker, "custom position marker");
                currentMarker.Offset = new System.Windows.Point(-15, -15);
                currentMarker.ZIndex = int.MaxValue;
                gMapControl1.Markers.Add(currentMarker);
            }
            if (gMapControl1.Markers.Count > 1)
            {
                gMapControl1.ZoomAndCenterMarkers(null);
            }
        }

        private void gMapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                System.Windows.Point p = e.GetPosition(gMapControl1);
                currentMarker.Position = gMapControl1.FromLocalToLatLng((int)p.X, (int)p.Y);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(gMapControl1);
            currentMarker.Position = gMapControl1.FromLocalToLatLng((int)p.X, (int)p.Y);
        }

        private void buttonStartPoint_Click(object sender, RoutedEventArgs e)
        {
            AddMarker(ref startMarker, textBlockStartPoint);
        }

        private void buttonEndPoint_Click(object sender, RoutedEventArgs e)
        {
            AddMarker(ref endMarker, textBlockEndPoint);
        }

        private void AddMarker(ref GMapMarker gMapMarker, TextBlock textBlock)
        {
            if (gMapMarker != null)
                gMapControl1.Markers.Remove(gMapMarker);
            gMapMarker = new GMapMarker(currentMarker.Position);
            {
                gMapMarker.Shape = new CustomMarkerRed(this, gMapMarker, "marker");
                gMapMarker.Offset = new System.Windows.Point(-15, -15);
                gMapMarker.ZIndex = int.MaxValue;
                gMapControl1.Markers.Add(gMapMarker);
                textBlock.Text = gMapMarker.Position.Lat.ToString("1.0000")+ ";" + gMapMarker.Position.Lng.ToString("1.0000");
            }
        }

        private void buttonGetDirections_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
