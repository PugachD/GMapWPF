using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GMapWPF;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using GMapWPF.CustomMarker;
using System.Data;

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
        //Способ передвижения
        string mode = "driving";
        // marker
        GMapMarker currentMarker, startMarker, endMarker;
        DataTable dtRouter;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Настройки для компонента GMap.
            gMapControl1.Bearing = 0;
            //CanDragMap - Если параметр установлен в True,
            //пользователь может перетаскивать карту с помощью правой кнопки мыши. 
            gMapControl1.CanDragMap = true;
            //Указываем, что перетаскивание карты осуществляется 
            //с использованием левой клавишей мыши.
            //По умолчанию - правая.
            gMapControl1.DragButton = MouseButton.Left;
            //Устанавливаем центр приближения/удаления для
            //курсора мыши.
            gMapControl1.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;

            gMapControl1.Zoom = 3;

            gMapControl1.ShowTileGridLines = false;
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            
            /*//Если вы используете интернет через прокси сервер,
            //указываем свои учетные данные.
            GMap.NET.MapProviders.GMapProvider.WebProxy =
                System.Net.WebRequest.GetSystemWebProxy();
            GMap.NET.MapProviders.GMapProvider.WebProxy.Credentials =
                System.Net.CredentialCache.DefaultCredentials;*/

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
            //инициализируем новую таблицу,
            //для хранения данных о маршруте.
            dtRouter = new DataTable();

            //Добавляем в инициализированную таблицу,
            //новые колонки.
            dtRouter.Columns.Add("Шаг");
            dtRouter.Columns.Add("Нач. точка (latitude)");
            dtRouter.Columns.Add("Нач. точка (longitude)");
            dtRouter.Columns.Add("Кон. точка (latitude)");
            dtRouter.Columns.Add("Кон. точка (longitude)");
            dtRouter.Columns.Add("Время пути");
            dtRouter.Columns.Add("Расстояние");
            dtRouter.Columns.Add("Описание маршрута");
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
            //Очищаем таблицу перед загрузкой данных.
            dtRouter.Rows.Clear();
            //Фрмируем запрос к API маршрутов Google.
            string url = string.Format(
                "http://maps.googleapis.com/maps/api/directions/xml?origin={0},&destination={1}&sensor=false&language=ru&mode={2}",
                Uri.EscapeDataString(textBoxDeparture.Text), Uri.EscapeDataString(textBoxArrival.Text), Uri.EscapeDataString(mode));

            //Выполняем запрос к универсальному коду ресурса (URI).
            System.Net.HttpWebRequest request =
                (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

            //Получаем ответ от интернет-ресурса.
            System.Net.WebResponse response =
                request.GetResponse();

            //Экземпляр класса System.IO.Stream 
            //для чтения данных из интернет-ресурса.
            System.IO.Stream dataStream =
                response.GetResponseStream();

            //Инициализируем новый экземпляр класса 
            //System.IO.StreamReader для указанного потока.
            System.IO.StreamReader sreader =
                new System.IO.StreamReader(dataStream);

            //Считываем поток от текущего положения до конца.            
            string responsereader = sreader.ReadToEnd();

            //Закрываем поток ответа.
            response.Close();

            System.Xml.XmlDocument xmldoc =
                new System.Xml.XmlDocument();

            xmldoc.LoadXml(responsereader);
            if (xmldoc.GetElementsByTagName("status")[0].ChildNodes[0].InnerText == "OK")
            {
                System.Xml.XmlNodeList nodes =
                    xmldoc.SelectNodes("//leg//step");

                //Формируем строку для добавления в таблицу.
                object[] dr;
                for (int i = 0; i < nodes.Count; i++)
                {
                    //Указываем что массив будет состоять из 
                    //восьми значений.
                    dr = new object[8];
                    //Номер шага.
                    dr[0] = i;
                    //Получение координат начала отрезка.
                    dr[1] = xmldoc.SelectNodes("//start_location").Item(i).SelectNodes("lat").Item(0).InnerText.ToString();
                    dr[2] = xmldoc.SelectNodes("//start_location").Item(i).SelectNodes("lng").Item(0).InnerText.ToString();
                    //Получение координат конца отрезка.
                    dr[3] = xmldoc.SelectNodes("//end_location").Item(i).SelectNodes("lat").Item(0).InnerText.ToString();
                    dr[4] = xmldoc.SelectNodes("//end_location").Item(i).SelectNodes("lng").Item(0).InnerText.ToString();
                    //Получение времени необходимого для прохождения этого отрезка.
                    dr[5] = xmldoc.SelectNodes("//duration").Item(i).SelectNodes("text").Item(0).InnerText.ToString();
                    //Получение расстояния, охватываемое этим отрезком.
                    dr[6] = xmldoc.SelectNodes("//distance").Item(i).SelectNodes("text").Item(0).InnerText.ToString();
                    //Получение инструкций для этого шага, представленные в виде текстовой строки HTML.
                    dr[7] = HtmlToPlainText(xmldoc.SelectNodes("//html_instructions").Item(i).InnerText.ToString());
                    //Добавление шага в таблицу.
                    dtRouter.Rows.Add(dr);
                }

                //Переменные для хранения координат начала и конца пути.
                double latStart = 0.0;
                double lngStart = 0.0;
                double latEnd = 0.0;
                double lngEnd = 0.0;

                //Получение координат начала пути.
                latStart = System.Xml.XmlConvert.ToDouble(xmldoc.GetElementsByTagName("start_location")[nodes.Count].ChildNodes[0].InnerText);
                lngStart = System.Xml.XmlConvert.ToDouble(xmldoc.GetElementsByTagName("start_location")[nodes.Count].ChildNodes[1].InnerText);
                //Получение координат конечной точки.
                latEnd = System.Xml.XmlConvert.ToDouble(xmldoc.GetElementsByTagName("end_location")[nodes.Count].ChildNodes[0].InnerText);
                lngEnd = System.Xml.XmlConvert.ToDouble(xmldoc.GetElementsByTagName("end_location")[nodes.Count].ChildNodes[1].InnerText);

                //Выводим в текстовое поле координаты начала пути.
                textBlockStartPoint.Text = latStart + ";" + lngStart;
                //Выводим в текстовое поле координаты конечной точки.
                textBlockEndPoint.Text = latEnd + ";" + lngEnd;
                
                //Устанавливаем позицию карты на начало пути.
                gMapControl1.Position = new PointLatLng(latStart, lngStart);
                
                //Инициализация нового ЗЕЛЕНОГО маркера, с указанием координат начала пути.
                 GMapMarker markerG = new GMapMarker(new PointLatLng(latStart, lngStart));
                //Формируем подсказку для маркера.
                string[] wordsG = textBoxDeparture.Text.Split(',');
                string dataMarkerG = string.Empty;
                foreach (string word in wordsG)
                {
                    dataMarkerG += word + ";\n";
                }
                markerG.Shape = new CustomMarkerRed(this, markerG, dataMarkerG);
                gMapControl1.Markers.Add(markerG);

                //Инициализация нового Красного маркера, с указанием координат конца пути.
                GMapMarker markerR = new GMapMarker(new PointLatLng(latEnd, lngEnd));

                //Формируем подсказку для маркера.
                string[] wordsR = textBoxArrival.Text.Split(',');
                string dataMarkerR = string.Empty;
                foreach (string word in wordsR)
                {
                    dataMarkerR += word + ";\n";
                }
                markerR.Shape = new CustomMarkerRed(this, markerR, dataMarkerR);
                gMapControl1.Markers.Add(markerR);

                //Создаем список контрольных точек для прокладки маршрута.
                List<GMap.NET.PointLatLng> list = new List<GMap.NET.PointLatLng>();

                //Проходимся по определенным столбцам для получения
                //координат контрольных точек маршрута и занесением их
                //в список координат.
                for (int i = 0; i < dtRouter.Rows.Count; i++)
                {
                    double dbStartLat = double.Parse(dtRouter.Rows[i].ItemArray[1].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    double dbStartLng = double.Parse(dtRouter.Rows[i].ItemArray[2].ToString(), System.Globalization.CultureInfo.InvariantCulture);

                    list.Add(new GMap.NET.PointLatLng(dbStartLat, dbStartLng));

                    double dbEndLat = double.Parse(dtRouter.Rows[i].ItemArray[3].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    double dbEndLng = double.Parse(dtRouter.Rows[i].ItemArray[4].ToString(), System.Globalization.CultureInfo.InvariantCulture);

                    list.Add(new GMap.NET.PointLatLng(dbEndLat, dbEndLng));
                }
                
                //Создаем маршрут на основе списка контрольных точек.
                GMapRoute r = new GMapRoute(list);

                //Добавляем в компонент, список маркеров и маршрутов.
                gMapControl1.Markers.Add(r);
                //gMapControl1.ReloadMap();
            }

        }
        //Удаляем HTML теги.
        public string HtmlToPlainText(string html)
        {
            html = html.Replace("</b>", "");
            return html.Replace("<b>", "");
        }
    }
}
