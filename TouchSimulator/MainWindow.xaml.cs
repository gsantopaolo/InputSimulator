using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Threading;

namespace WpfApplication1
{
    enum Gesture : int
    { 
        Tap = 1,
        Pinch = 2,
        Zoom = 3
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Random __randomizer = new Random();
        private static bool __continuie = true;

        public MainWindow()
        {
            InitializeComponent();

            // Alt-Q to quit.
            HotKeyManager.RegisterHotKey(Keys.S, KeyModifiers.Control);
            HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

            //initialize with default settings
            TouchInjector.InitializeTouchInjection();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int sleeptimeInMs = 2000;

            try
            {
                if (string.IsNullOrEmpty(txtProtocol.Text) == false)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = @txtProtocol.Text;
                    p.Start();
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("Something went wrong trying to oopen your app. You have 20 seconds to manually open you app. After that gesture will be injected. To stop press CTRL + S ");
                sleeptimeInMs = 20000;
            }
            finally
            {

                //wait for two seconds, so the user can switch windows
                //await Task.Delay(2000);
                Thread.Sleep(sleeptimeInMs);

                StartSimulation();
            }
        }

        async Task StartSimulation()
        {
            

            while (__continuie)
            {
                Screen screen = Screen.AllScreens[__randomizer.Next(Screen.AllScreens.Length)];
                int x = __randomizer.Next(screen.Bounds.Left, screen.Bounds.Right);
                int y = __randomizer.Next(screen.Bounds.Top, screen.Bounds.Bottom);
                SimulateTouch(x, y);
                //for (int i = 0; i < 50; i++)
                //{
                //    SimulateTouch(x, y);
                //    x = __randomizer.Next(screen.Bounds.Left, screen.Bounds.Right);
                //    y = __randomizer.Next(screen.Bounds.Top, screen.Bounds.Bottom);
                //    Thread.Sleep(100);
                //}

                //Gesture gesture = (Gesture)__randomizer.Next(1, 3);

                //switch (gesture)
                //{
                //    case Gesture.Tap:
                //        SimulateTouch(x, y);
                //        break;
                //    case Gesture.Pinch:
                //        SimulatePinch(true, x, y);
                //        break;
                //    case Gesture.Zoom:
                //        SimulatePinch(false, x, y);
                //        break;
                //    default:
                //        SimulatePinch(false, x, y);
                //        break;
                //}

                //hread.Sleep(100);
                await Task.Delay(100);
            }
        
        }

        private void SimulateTouch(int x, int y)
        {
            PointerTouchInfo contact = MakePointerTouchInfo(x,y, 5, 1);
            TouchInjector.InjectTouchInput(1, new []{contact});
//            await Task.Delay(3);
            Thread.Sleep(3);
            contact.PointerInfo.PointerFlags = PointerFlags.UP;
            TouchInjector.InjectTouchInput(1, new[] { contact });
        }

        private void SimulatePinch(bool pinch, int x, int y)
        {
            
            // todo: have to verify that x+150 is inside the screen
            PointerTouchInfo[] contacts = new PointerTouchInfo[2];
            int deltaX = pinch == false ? 100 : 150;
            uint orientation = (uint)__randomizer.Next(0,360);
            contacts[0] = MakePointerTouchInfo(x, y, 2, orientation);
            contacts[1] = MakePointerTouchInfo(x + deltaX, y, 2, orientation);

            TouchInjector.InjectTouchInput(2, contacts);

            contacts[0].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
            contacts[1].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;

            //drag them from/to each other
            for (int i = 0; i < 100; i++)
            {
                if (pinch)
                {
                    contacts[0].Move(+1, 0);
                    contacts[1].Move(-1, 0);
                }
                else//simulate zoom
                {
                    contacts[0].Move(-1, 0);
                    contacts[1].Move(+1, 0);
                }
                bool s = TouchInjector.InjectTouchInput(2, contacts);
                //await Task.Delay(3);
                Thread.Sleep(3);
            }

            //release them
            contacts[0].PointerInfo.PointerFlags = PointerFlags.UP;
            contacts[1].PointerInfo.PointerFlags = PointerFlags.UP;

            TouchInjector.InjectTouchInput(2, contacts);
        }

        private PointerTouchInfo MakePointerTouchInfo(int x, int y, int radius, uint orientation = 90, uint pressure = 32000)
        {
            PointerTouchInfo contact = new PointerTouchInfo();
            contact.PointerInfo.pointerType = PointerInputType.TOUCH;
            contact.TouchFlags = TouchFlags.NONE;
            contact.Orientation = orientation;
            contact.Pressure = pressure;
            contact.PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
            contact.TouchMasks = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;
            contact.PointerInfo.PtPixelLocation.X = x;
            contact.PointerInfo.PtPixelLocation.Y = y;
            contact.PointerInfo.PointerId = IdGenerator.GetUinqueUInt();
            contact.ContactArea.left = x - radius;
            contact.ContactArea.right = x + radius;
            contact.ContactArea.top = y - radius;
            contact.ContactArea.bottom = y + radius;
            return contact;
        }

        void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            __continuie = !__continuie;

        }
    }
}
