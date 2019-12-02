using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FakeTouchInputDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.TouchDown += MainWindow_TouchDown;
            this.TouchMove += MainWindow_TouchMove;
            this.TouchUp += MainWindow_TouchUp;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NativeMethods.InitializeTouchInjection();
        }

        private Line _proxyLine;
        private void MainWindow_TouchDown(object sender, TouchEventArgs e)
        {
            System.Windows.Input.TouchPoint oPos = e.GetTouchPoint(this);
            Line oLine = new Line();
            oLine.Stroke = new SolidColorBrush(Colors.Red);
            oLine.StrokeThickness = 2;
            oLine.X1 = oPos.Position.X;
            oLine.Y1 = oPos.Position.Y;
            _proxyLine = oLine;
            Console.WriteLine("TouchDown;TouchID " + e.TouchDevice.Id + "  TouchDown " + oPos.Position.X + "    " + oPos.Position.Y);
        }

        private void MainWindow_TouchMove(object sender, TouchEventArgs e)
        {
            System.Windows.Input.TouchPoint movedPoint = e.GetTouchPoint(this);
            Console.WriteLine("TouchMove:TouchID " + e.TouchDevice.Id + " TouchMove " + movedPoint.Position.X + "    " + movedPoint.Position.Y);
        }

        private void MainWindow_TouchUp(object sender, TouchEventArgs e)
        {
            System.Windows.Input.TouchPoint oPos = e.GetTouchPoint(this);
            this._proxyLine.X2 = oPos.Position.X;
            this._proxyLine.Y2 = oPos.Position.Y;
            RootGrid.Children.Add(this._proxyLine);
            Console.WriteLine("TouchUp:TouchID " + e.TouchDevice.Id + " TouchUp " + oPos.Position.X + "    " + oPos.Position.Y);
        }

        #region Fake

        private void TouchWriteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var oneText = TouchWriteOneTextBox.Text;
            var oneStrings = oneText.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var twoText = TouchWriteTwoTextBox.Text;
            var towStrings = twoText.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            FakeTouchWrite(Convert.ToInt32(oneStrings[0]), Convert.ToInt32(oneStrings[1]), Convert.ToInt32(towStrings[0]), Convert.ToInt32(towStrings[1]));
        }

        private void FakeTouchWrite(int fromX, int fromY, int toX, int toY)
        {
            // Touch Down
            PointerTouchInfo contact = MakePointerTouchInfo(fromX, fromY, 5, 1);
            PointerFlags oFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
            contact.PointerInfo.PointerFlags = oFlags;
            NativeMethods.InjectTouchInput(1, new[] { contact });

            // Touch Move
            int movedX = toX - fromX;
            int movedY = toY - fromY;
            contact.Move(movedX, movedY);
            oFlags = PointerFlags.INRANGE | PointerFlags.INCONTACT | PointerFlags.UPDATE;
            contact.PointerInfo.PointerFlags = oFlags;
            NativeMethods.InjectTouchInput(1, new[] { contact });

            // Touch Up
            contact.PointerInfo.PointerFlags = PointerFlags.UP;
            NativeMethods.InjectTouchInput(1, new[] { contact });
        }
        private PointerTouchInfo MakePointerTouchInfo(int x, int y, int radius,
            uint orientation = 90, uint pressure = 32000)
        {
            PointerTouchInfo contact = new PointerTouchInfo();
            contact.PointerInfo.pointerType = PointerInputType.TOUCH;
            contact.TouchFlags = TouchFlags.NONE;
            contact.Orientation = orientation;
            contact.Pressure = pressure;
            contact.TouchMasks = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;
            contact.PointerInfo.PtPixelLocation.X = x;
            contact.PointerInfo.PtPixelLocation.Y = y;
            uint unPointerId = IdGenerator.GetUinqueUInt();
            Console.WriteLine("PointerId    " + unPointerId);
            contact.PointerInfo.PointerId = unPointerId;
            contact.ContactArea.left = x - radius;
            contact.ContactArea.right = x + radius;
            contact.ContactArea.top = y - radius;
            contact.ContactArea.bottom = y + radius;
            return contact;
        }

        #endregion

    }
}
