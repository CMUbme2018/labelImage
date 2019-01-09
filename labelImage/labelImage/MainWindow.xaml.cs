using System;
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
    };
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            translateChanged = new TranslateTransform
            {
                X = 0,
                Y = 0
            };
            scaleChanged = new ScaleTransform
            {
                ScaleX = 1,
                ScaleY = 1
            };

            rectangleNodes = new List<RemarkRectangleNode>();
            rectangles = new List<Rectangle>();

        }

        Canvas baseCanvas;//标记所需的图层
        TranslateTransform translateChanged;
        ScaleTransform scaleChanged;
        private List<RemarkRectangleNode> rectangleNodes;
        private List<Rectangle> rectangles;

        #region 打开图像
        string directory = null;
        string imgPath = null;
        List<string> imgArray = null;
        public string ImageSourceFileName = null;  //参考图像的路径文件名
        public string ImageTargetFileName = null;   //浮动图像的路径文件名

        //Ribbon界面，打开参考图像按钮
        private void RBOpenSourceImage_Click(object sender, RoutedEventArgs e)
        {
            ImageSourceFileName = OpenImage("打开参考图像");
            if (ImageSourceFileName != "")
            {
                ImageCanvas.Children.Remove(baseCanvas);
                baseCanvas = null;

                ImageSourceImage.Source = new BitmapImage(new Uri(ImageSourceFileName));

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
                rectangle.Width = 0;
                rectangle.Height = 0;
                //rectangle.RenderTransform = new TransformGroup
                //{
                //    Children = new TransformCollection()
                //    {
                //        new ScaleTransform(1,1),
                //        new TranslateTransform(0,0),
                //    }
                //};
                baseCanvas.Children.Add(rectangle);
                Matrix matrix = ImageSourceImage.RenderTransform.Value;
                Canvas.SetLeft(rectangle, (PreviousMousePoint.X  - translateChanged.X)/scaleChanged.ScaleX);
                Canvas.SetTop(rectangle, (PreviousMousePoint.Y  - translateChanged.Y)/scaleChanged.ScaleY);
                rectangles.Add(rectangle);
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


            
            

            //if (rectangle != null)
            //{
            //    //如果画出了矩形, 则将其存储在序列中, 先将base canvas 的矩形清除, 并显示序列中存储的所有矩形
            //    RemarkRectangleNode rectNode = new RemarkRectangleNode();
            //    rectNode.xmin = (int)PreviousMousePoint.X;
            //    rectNode.ymin = (int)PreviousMousePoint.Y;
            //    rectNode.xmax = (int)PreviousMousePoint.X + (int)rectangle.Width;
            //    rectNode.ymax = (int)PreviousMousePoint.Y + (int)rectangle.Height;
            //    rectangleNodes.Add(rectNode);


            //    rectangles.Add(rectangle);




            //    foreach (Rectangle rect in rectangles)
            //    {

            //        rect.MouseEnter += new System.Windows.Input.MouseEventHandler(RemarkRectangle_MouseEnter);
            //        rect.MouseLeave += new System.Windows.Input.MouseEventHandler(RemarkRectangle_MouseLeave);
            //        rect.MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(RemarkRectangle_MouseRightUp);

            //    }

            //}

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
        }

        private void RemarkRectangle_MouseLeave(object sender, System.EventArgs e)
        {
            System.Windows.Shapes.Rectangle rect = sender as System.Windows.Shapes.Rectangle;
            rect.Fill = new SolidColorBrush(Colors.Transparent);
        }

        private void RemarkRectangle_MouseRightUp(object sender, System.EventArgs e)
        {
            //生成右键菜单
            Console.WriteLine("123");
            //System.Windows.Controls.ContextMenu cm = this.FindResource("RemarkRectangleRightBtn") as System.Windows.Controls.ContextMenu;
            //cm.PlacementTarget = sender as System.Windows.Controls.Button;
            //cm.IsOpen = true;
        }

        #endregion

        #region 图像操作
        private void ZoomImage(TransformGroup group, System.Windows.Point point, double scale)
        {
            System.Windows.Point pointToContent = group.Inverse.Transform(point);
            ScaleTransform transform = group.Children[0] as ScaleTransform;
            if (transform.ScaleX + scale < 1)
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


            //foreach (UIElement ue in baseCanvas.Children)
            //{
            //    TransformGroup ns = (TransformGroup)ue.RenderTransform;
            //    if (ns != null)
            //    {
            //        Point controlPoint = ns.Inverse.Transform(point);
            //        ScaleTransform scaleTrans = ns.Children[0] as ScaleTransform;
            //        scaleTrans.ScaleX = transform.ScaleX;
            //        scaleTrans.ScaleY = transform.ScaleY;
            //        TranslateTransform translateTrans = ns.Children[1] as TranslateTransform;
            //        translateTrans.X = -1 * ((controlPoint.X * scaleTrans.ScaleX) - point.X);
            //        translateTrans.Y = -1 * ((controlPoint.Y * scaleTrans.ScaleY) - point.Y);


                    
                    
                    
            //        //translateTrans.X = transform1.X ;
            //        //translateTrans.Y = transform1.Y ;
            //        Console.WriteLine("transform1---x---{0}", transform1.X);
            //        Console.WriteLine("transform1---y---{0}", transform1.Y);




            //        //foreach (Transform trans in ns.Children)
            //        //{
            //        //    if (trans is ScaleTransform && scaleChanging != null)
            //        //    {
            //        //        ScaleTransform scale = (ScaleTransform)trans;
            //        //        scale.ScaleX += scaleChanged.ScaleX;
            //        //        scale.ScaleY += scaleChanged.ScaleY;


            //        //        TranslateTransform scaleTrans = ns.Children[0] as TranslateTransform;

            //        //        scaleTrans.X = scaleTrans.X + scaleTransformChanging.X - preScaleTransformChanging.X;
            //        //        scaleTrans.Y = scaleTrans.Y + scaleTransformChanging.Y - preScaleTransformChanging.Y;

            //        //        preScaleTransformChanging = scaleTransformChanging;

            //        //    }
            //        //}

            //    }
            //}

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
            XmlTools xmlTool = new XmlTools();
            //xmlTool.rectangleNodes = rectangleNodes;
            xmlTool.SaveXMLFilesAsLabelImageFormat("");
        }
        #endregion



    }
}
