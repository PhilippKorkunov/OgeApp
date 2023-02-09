using Microsoft.EntityFrameworkCore;
using OgeApp.Entyties;
using OgeApp.Windows.WindowsClassesStructsAndEnums;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace OgeApp.Windows
{

    public partial class TaskWindow : Window
    {
        private readonly EFDBContext eFDBContext;
        Task CurrentTask { get; set; }
        Topic CurrentTopic { get; set; }
        MainWindow MainWindow { get; set; }
        public TaskWindow(EFDBContext context, int topicId, int taskId, MainWindow mainWindow)
        {
            InitializeComponent();
            eFDBContext = context;
            MainWindow = mainWindow;
            Loaded += MainWindow_Loaded;

            CurrentTask = eFDBContext.Tasks.Local.ToObservableCollection().Where(task => task.Id == taskId).First();
            CurrentTopic = eFDBContext.Topics.Local.ToObservableCollection().Where(topic => topic.Id == topicId).First();
            Header.Text = $"Задание {CurrentTask.Name}";

            TaskText.Text = CurrentTask.Text;

            Canvas.Height = 0;

            if (CurrentTask.PictureId is not null)
            {
                Picture picture = eFDBContext.Pictures.Local.ToObservableCollection()
                    .Where(picture => picture.Id == CurrentTask.PictureId).First();

                if (picture.PictureLink is not null)
                {
                    Image.Source = new BitmapImage(new Uri(picture.PictureLink, UriKind.RelativeOrAbsolute));
                    Image.MaxHeight = 250;
                    Image.MaxWidth = 400;

                    double ourHeight = Image.MaxHeight;
                    double ourWidth = Image.MaxWidth;

                    if (Image.Source.Width / ourWidth > Image.Source.Height / ourHeight)
                    {
                        Canvas.Width = ourWidth;
                        Canvas.Height = Image.Source.Height / (double)(Image.Source.Width / ourWidth);

                    }
                    else
                    {
                        Canvas.Height = ourHeight;
                        Canvas.Width = Image.Source.Width / (double)(Image.Source.Height / ourHeight);
                    }
                }
            }

            if (!String.IsNullOrEmpty(CurrentTask.UserAnswer)) 
            {
                AnswerBox.Text = CurrentTask.UserAnswer;
                CheckAnswer();
            }

            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };

            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            MaximizeButton.Click += (s, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (s, e) => Close();
            ComfirmButton.Click += (s, e) => CheckAnswer();
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            eFDBContext.Database.EnsureCreated();

            eFDBContext.Topics.Load();
            eFDBContext.Tasks.Load();
            eFDBContext.Pictures.Load();
        }

        private void CheckAnswer()
        {
            var stringHash = new HashConvertor(CurrentTask.Name, AnswerBox.Text).ToString();

            AnswerBox.BorderBrush =  stringHash.Equals(CurrentTask.RightAnswer) ?
                Brushes.PaleGreen : Brushes.Firebrick;

            CurrentTask.IsDone = stringHash.Equals(CurrentTask.RightAnswer) ?
                true: false;

            CurrentTask.UserAnswer = AnswerBox.Text;
            eFDBContext.Tasks.Update(CurrentTask);
            eFDBContext.Topics.Update(CurrentTopic);
            eFDBContext.SaveChanges();
            MainWindow.Grid.Items.Refresh();
        }

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;

                    break;
            }
            return (IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                mmi.ptMinTrackSize.x = 1275;
                mmi.ptMinTrackSize.y = 950;
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }
        

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
    }
}
