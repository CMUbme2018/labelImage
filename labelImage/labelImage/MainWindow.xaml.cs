﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace labelImage
{
    public struct RemarkRectangleNode
    {
        public int xmin;
        public int ymin;
        public int xmax;
        public int ymax;
        public string name;
        public int tag;
    };
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            translateChanged = new TranslateTransform { X = 0, Y = 0 };
            scaleChanged = new ScaleTransform { ScaleX = 1, ScaleY = 1 };

            rectangleNodes = new List<RemarkRectangleNode>();
            rectangles = new List<Rectangle>();
        }

        Canvas baseCanvas;//标记所需的图层
        TranslateTransform translateChanged;
        ScaleTransform scaleChanged;
        private List<RemarkRectangleNode> rectangleNodes;
        private List<Rectangle> rectangles;
        private string imageName;
        FillContentBox fillContentBox;//标记完后弹出的填写标记信息的对话框
        private int rectangleTagIndex = 0;//标记框的tag, 用于导出XML时确定哪些rectangle还在屏幕上显示

        #region 打开图像
        //Ribbon界面，打开参考图像按钮
        private void RBOpenSourceImage_Click(object sender, RoutedEventArgs e)
        {
            string ImageSourceFileName = OpenImage("打开参考图像");

            //"E:\\wanglei\\code\\xml备用代码\\0.jpg"
            if (ImageSourceFileName != "")
            {
                ImageCanvas.Children.Remove(baseCanvas);
                baseCanvas = null;

                ImageSourceImage.Source = new BitmapImage(new Uri(ImageSourceFileName));

                string[] tempStrings = ImageSourceFileName.Split(new char[] { '\\' });
                imageName = tempStrings.Last();

                baseCanvas = new Canvas();
                baseCanvas.Height = ImageSourceImage.Source.Height;
                baseCanvas.Width = ImageSourceImage.Source.Width;
                baseCanvas.RenderTransform = new TransformGroup
                {
                    Children = new TransformCollection()
                    {
                        new ScaleTransform(1,1),
                        new TranslateTransform(0,0),
                    }
                };
                Canvas.SetTop(baseCanvas, 0);
                Canvas.SetLeft(baseCanvas, 0);
                ImageCanvas.Children.Add(baseCanvas);
            }
        }

        //打开一个图像文件
        private string OpenImage(string ofdTitle)
        {
            System.Windows.Forms.OpenFileDialog openDlg = new System.Windows.Forms.OpenFileDialog();
            openDlg.Filter = "JPEG图片|*.jpg; *.jpeg|" + "TIFF图片|*.tif; *.tiff|" + "BMP|*.bmp|" + "PNG|*.png|" + "所有文件|*.*";
            openDlg.Title = ofdTitle;
            openDlg.Multiselect = true;
            openDlg.ShowHelp = true;
            openDlg.ShowDialog();
            string fileName = openDlg.FileName;
            return fileName;
        }
        #endregion

        #region 鼠标操作
        System.Windows.Point PreviousMousePoint;
        bool IsMouseLeftButtonDown;
        Point RemarkRectanglePoint;
        private void ImageLeft_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            this.Cursor = System.Windows.Input.Cursors.ScrollAll;
            img.CaptureMouse();
            IsMouseLeftButtonDown = true;

            PreviousMousePoint = e.GetPosition(img);

            if (isRemarking)
            {


                Rectangle rectangle = new Rectangle();
                rectangle.Stroke = new SolidColorBrush(Colors.Black);
                rectangle.StrokeThickness = 3;
                rectangle.Width = 0;
                rectangle.Height = 0;
                baseCanvas.Children.Add(rectangle);
                rectangle.Tag = rectangleTagIndex;
                rectangleTagIndex++;
                Canvas.SetLeft(rectangle, (PreviousMousePoint.X - translateChanged.X) / scaleChanged.ScaleX);
                Canvas.SetTop(rectangle, (PreviousMousePoint.Y - translateChanged.Y) / scaleChanged.ScaleY);
                rectangles.Add(rectangle);

                //记录矩形左上角的坐标
                RemarkRectanglePoint.X = (PreviousMousePoint.X - translateChanged.X) / scaleChanged.ScaleX;
                RemarkRectanglePoint.Y = (PreviousMousePoint.Y - translateChanged.Y) / scaleChanged.ScaleY;

                rectangle.MouseEnter += new System.Windows.Input.MouseEventHandler(RemarkRectangle_MouseEnter);
                rectangle.MouseLeave += new System.Windows.Input.MouseEventHandler(RemarkRectangle_MouseLeave);
                rectangle.MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(RemarkRectangle_MouseRightUp);
            }


        }
        private void ImageLeft_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            Point positoin = e.GetPosition(img);
            this.Cursor = System.Windows.Input.Cursors.Arrow;
            img.ReleaseMouseCapture();
            IsMouseLeftButtonDown = false;

            if (isRemarking)
            {

                Rectangle rectangle = rectangles.Last();
                if (rectangle != null && (rectangle.Width > 0 || rectangle.Height > 0))
                {
                    //存储每个标记框的坐标
                    RemarkRectangleNode rectangleNode = new RemarkRectangleNode();
                    rectangleNode.xmin = (int)RemarkRectanglePoint.X;
                    rectangleNode.ymin = (int)RemarkRectanglePoint.Y;
                    rectangleNode.xmax = rectangleNode.xmin + (int)rectangle.Width;
                    rectangleNode.ymax = rectangleNode.ymin + (int)rectangle.Height;
                    rectangleNodes.Add(rectangleNode);

                    fillContentBox = new FillContentBox();
                    fillContentBox.okButton.Click += new RoutedEventHandler(fillContetnBoxOkButtonClick);
                    fillContentBox.cancelButton.Click += new RoutedEventHandler(fillContetnBoxCancelButtonClick);
                    ImageCanvas.Children.Add(fillContentBox);
                    Canvas.SetLeft(fillContentBox, 500);
                    Canvas.SetTop(fillContentBox, rectangleNode.ymax + 30);



                }
                else if (rectangle != null)
                {
                    rectangles.Remove(rectangle);
                }
            }


        }
        private void ImageLeft_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            if (IsMouseLeftButtonDown)
            {
                if (isRemarking)
                {
                    //画标注的矩形
                    Rectangle rectangle = rectangles.Last();
                    var point = e.GetPosition(img);
                    var rect = new Rect(PreviousMousePoint, point);
                    rectangle.Width = rect.Width / scaleChanged.ScaleX;
                    rectangle.Height = rect.Height / scaleChanged.ScaleY;

                }
                else
                {
                    DoImageMove(img, e);
                }
            }

        }
        private void ImageLeft_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ContentControl image = sender as ContentControl;
            if (image == null)
            {
                return;
            }
            TransformGroup group = ImageGird.FindResource("ImageTransformResource") as TransformGroup;
            System.Windows.Point point = e.GetPosition(image);
            double scale = e.Delta * 0.001;
            ZoomImage(group, point, scale);


        }

        private void RemarkRectangle_MouseEnter(object sender, System.EventArgs e)
        {
            System.Windows.Shapes.Rectangle rect = sender as System.Windows.Shapes.Rectangle;
            rect.Fill = new SolidColorBrush(Colors.Red);
            activeRectangle = sender as Rectangle;
        }

        private void RemarkRectangle_MouseLeave(object sender, System.EventArgs e)
        {
            System.Windows.Shapes.Rectangle rect = sender as System.Windows.Shapes.Rectangle;
            rect.Fill = new SolidColorBrush(Colors.Transparent);

        }

        private void RemarkRectangle_MouseRightUp(object sender, System.EventArgs e)
        {
            //生成右键菜单
            System.Windows.Controls.ContextMenu cm = this.FindResource("RemarkRectangleRightBtn") as System.Windows.Controls.ContextMenu;
            cm.PlacementTarget = sender as System.Windows.Controls.Button;
            cm.IsOpen = true;
        }

        #endregion

        #region 图像操作
        private void ZoomImage(TransformGroup group, System.Windows.Point point, double scale)
        {
            System.Windows.Point pointToContent = group.Inverse.Transform(point);
            ScaleTransform transform = group.Children[0] as ScaleTransform;
            if (transform.ScaleX + scale < GetMinScale(ImageSourceImage.Source.Width, ImageSourceImage.Source.Width))
            {
                return;
            }
            transform.ScaleX += scale;
            transform.ScaleY += scale;
            TranslateTransform transform1 = group.Children[1] as TranslateTransform;
            transform1.X = -1 * ((pointToContent.X * transform.ScaleX) - point.X);
            transform1.Y = -1 * ((pointToContent.Y * transform.ScaleY) - point.Y);

            scaleChanged = transform;
            translateChanged.X = transform1.X;
            translateChanged.Y = transform1.Y;


            TransformGroup transGroup = (TransformGroup)baseCanvas.RenderTransform;
            ScaleTransform scaleTrans = transGroup.Children[0] as ScaleTransform;
            scaleTrans.ScaleX = transform.ScaleX;
            scaleTrans.ScaleY = transform.ScaleY;
            TranslateTransform translateTrans = transGroup.Children[1] as TranslateTransform;
            translateTrans.X = transform1.X;
            translateTrans.Y = transform1.Y;

        }
        private void DoImageMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ContentControl image = sender as ContentControl;
            if (image == null)
            {
                return;
            }
            TransformGroup group = ImageGird.FindResource("ImageTransformResource") as TransformGroup;
            TranslateTransform transform = group.Children[1] as TranslateTransform;
            System.Windows.Point position = e.GetPosition(image);
            transform.X += position.X - PreviousMousePoint.X;
            transform.Y += position.Y - PreviousMousePoint.Y;
            PreviousMousePoint = position;

            translateChanged.X = transform.X;
            translateChanged.Y = transform.Y;


            TransformGroup transGroup = (TransformGroup)baseCanvas.RenderTransform;
            ScaleTransform scaleTrans = transGroup.Children[0] as ScaleTransform;
            TranslateTransform translateTrans = transGroup.Children[1] as TranslateTransform;
            translateTrans.X = transform.X;
            translateTrans.Y = transform.Y;





        }





        #endregion

        #region Ribbon按钮操作
        //标记图像按钮的显示, 通过布尔值判断当前是标记图像还是移动图像
        public bool isRemarking = false;
        private bool isPutBaseCanvas = false;
        private void RBRemarksImage_Click(object sender, RoutedEventArgs e)
        {
            //如果没有打开任何图片, 则不进行任何操作
            if (ImageSourceImage.Source == null)
                return;

            isRemarking = !isRemarking;
            if (isRemarking)
            {
                this.RBRemarksImage.Label = "取消标记";
                if (!isPutBaseCanvas)
                {
                    //baseCanvas = new Canvas();
                    ////baseCanvas.Background = new SolidColorBrush(Colors.AliceBlue);
                    //baseCanvas.Height = ImageCanvas.Height;
                    //baseCanvas.Width = ImageCanvas.Width;
                    //Canvas.SetTop(baseCanvas, 0);
                    //Canvas.SetLeft(baseCanvas, 0);
                    //ImageCanvas.Children.Add(baseCanvas);
                    isPutBaseCanvas = true;


                }
            }
            else
            {
                this.RBRemarksImage.Label = "标记图像";
            }


        }

        private void RBExportXML_Click(object sender, RoutedEventArgs e)
        {
            //如果没有打开任何图片, 则不进行任何操作
            if (ImageSourceImage.Source == null)
                return;

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            //可能要获取的路径名
            //string localFilePath = "", fileNameExt = "", newFileName = "", FilePath = "";

            //设置文件类型
            //书写规则例如：txt files(*.txt)|*.txt
            saveFileDialog.Filter = "xml files(*.xml)|*.xml|All files(*.*)|*.*";
            //设置默认文件名（可以不设置）
            saveFileDialog.FileName = imageName.Split(new char[] { '.'})[0];
            //主设置默认文件extension（可以不设置）
            saveFileDialog.DefaultExt = "xml";
            //获取或设置一个值，该值指示如果用户省略扩展名，文件对话框是否自动在文件名中添加扩展名。（可以不设置）
            saveFileDialog.AddExtension = true;

            //保存对话框是否记忆上次打开的目录
            saveFileDialog.RestoreDirectory = true;

            // Show save file dialog box
            bool? result = saveFileDialog.ShowDialog();
            //点了保存按钮进入
            if (result == true)
            {
                //获得文件路径
                string localFilePath = saveFileDialog.FileName.ToString();

                //获取文件名，不带路径
                string fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf("\\") + 1);

                //获取文件路径，不带文件名
                string FilePath = localFilePath.Substring(0, localFilePath.LastIndexOf("\\"));

                XmlTools xmlTool = new XmlTools();
                xmlTool.rectangleNodes = rectangleNodes;
                xmlTool.imageName = imageName;
                xmlTool.SaveXMLFilesAsLabelImageFormat(localFilePath);
            }


            
        }
        #endregion


        public SolidColorBrush GetRemartRectangleColorByTermNumber(string termNumber)
        {
            if (termNumber.Length == 0)
                return new SolidColorBrush(Colors.Black);

            int num = Convert.ToInt32(termNumber);
            switch (num)
            {
                case 1:
                    return new SolidColorBrush(Colors.Red);
                case 2:
                    return new SolidColorBrush(Colors.Orange);
                case 3:
                    return new SolidColorBrush(Colors.Yellow);
                case 4:
                    return new SolidColorBrush(Colors.Green);
                case 5:
                    return new SolidColorBrush(Colors.Blue);
                case 6:
                    return new SolidColorBrush(Colors.Indigo);
                case 7:
                    return new SolidColorBrush(Colors.Purple);
                default:
                    return new SolidColorBrush(Colors.Black);

            }
        }

        private Rectangle activeRectangle;
        private void RemarkDeleteBtnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(sender);
            DeleteSelectedRectangle();
        }

        private void DeleteSelectedRectangle()
        {
            baseCanvas.Children.Remove(activeRectangle);
            rectangles.Remove(activeRectangle);
            foreach (RemarkRectangleNode rectangleNode in rectangleNodes)
            {
                if (rectangleNode.tag == (int)activeRectangle.Tag)
                {
                    rectangleNodes.Remove(rectangleNode);
                    break;
                }
            }
            activeRectangle = null;
        }

        private double GetMinScale(double imageWidth, double imageHeight)
        {
            double horizonalScale = 500 / imageWidth;
            double verticalScale = 500 / imageHeight;
            return horizonalScale > verticalScale ? verticalScale : horizonalScale;
        }

        private void fillContetnBoxOkButtonClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("123");            
            RemarkRectangleNode rectangleNode = rectangleNodes.Last();
            rectangleNodes.RemoveAt(rectangleNodes.Count - 1);
            rectangleNode.name = fillContentBox.name;
            rectangleNodes.Add(rectangleNode);

            Rectangle rectangle = rectangles.Last();
            rectangle.Stroke = GetRemartRectangleColorByTermNumber(fillContentBox.termNumber);

            ImageCanvas.Children.Remove(fillContentBox);
            fillContentBox = null;
        }

        private void fillContetnBoxCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("123");
            ImageCanvas.Children.Remove(fillContentBox);
            fillContentBox = null;
            DeleteSelectedRectangle();
        }
    }
}
