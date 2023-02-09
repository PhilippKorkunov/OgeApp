using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using OgeApp.Windows;
using System.Data;
using OgeApp.Entyties;
using OgeApp.Windows.WindowsStructsAndEnums;
using OgeApp.Windows.WindowsClassesStructsAndEnums;
using System.Linq;
using System.IO;
using OgeApp.TaskProcessing;
using System.Windows.Documents;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading.Tasks;
using Task = OgeApp.Entyties.Task;

namespace OgeApp
{
    public partial class MainWindow : Window
    {
        private readonly EFDBContext eFDBContext = new();
        private TableState TableState { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            TableState = TableState.Topic;

            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };
            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            MaximizeButton.Click += (s, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (s, e) => Close();

            AddNewTasksButton.Click += (s, e) => AddNewTasks();
            ReturnButton.Click += (s, e) => Return();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //eFDBContext.Database.EnsureDeleted();
            eFDBContext.Database.EnsureCreated();

            eFDBContext.Pictures.Load();
            eFDBContext.Topics.Load();
            eFDBContext.Tasks.Load();

            Refresh();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Grid.SelectedIndex >= 0)
            {
                if (TableState == TableState.Topic)
                {
                    TableState = TableState.Task;
                    Refresh();
                }
                else if (TableState == TableState.Task)
                {
                    if (Grid.SelectedItems is not null)
                    {
                        var selectedItem = Grid.SelectedItems[0];
                        if (selectedItem is not null)
                        {
                            Task task = (Task)selectedItem;
                            TaskWindow taskWindow = new(eFDBContext, task.TopicId, task.Id, this);
                            taskWindow.Show();
                        }
                    }
                }
            }
        }

        private void Return()
        {
            TableState = TableState.Topic;
            Refresh();
        }

        private void AddNewTasks()
        {
            string? path = ChosePath();
            if (path is not null)
            {
                string? dirPath = Path.GetDirectoryName(path);
                PictureAdder.AddPicture(dirPath);

                new TaskAdder(path, eFDBContext).Add();

                Refresh();
            }
        }

        private void Refresh()
        {
            switch (TableState)
            {
                case TableState.Topic:
                    var topics = eFDBContext.Topics.Local.ToObservableCollection();
                    var tasks = eFDBContext.Tasks.Local.ToList();
                    foreach (var topic in topics)
                    {
                        topic.DoneTasks = (from Task task in tasks
                                           where task.TopicId == topic.Id && task.IsDone == true
                                           select task).Count();
                        topic.DoneTaskProcent = Math.Round((double)(topic.DoneTasks) / topic.TaskNumber * 100, 2);
                    }
                    Grid.ItemsSource = topics;
                    Header.Text = "Список тем";
                    Grid.Columns[0].Header = " Номер темы  ";
                    Grid.Columns[1].Header = " Тема  ";
                    Grid.Columns[2].Header = " Заданий всего ";
                    Grid.Columns[3].Header = " Выполнено ";
                    Grid.Columns[4].Header = " Выполнено в % ";
                    AddNewTasksButton.Visibility = Visibility.Visible;
                    ReturnButton.Visibility = Visibility.Collapsed;
                    break;
                case TableState.Task:
                    if (Grid.SelectedIndex >= 0 )
                    {
                        var selectedItem = Grid.SelectedItems[0];
                        if (selectedItem is not null)
                        {
                            Topic topic = (Topic)selectedItem;
                            Grid.ItemsSource = eFDBContext.Tasks.Local.ToObservableCollection().Where(task => task.TopicId == topic.Id);
                            Header.Text = "Список заданий";
                            Grid.Columns[0].Header = " Номер задания  ";
                            Grid.Columns[1].Header = " Название задания  ";
                            Grid.Columns[2].Header = " Выполнено ";
                            AddNewTasksButton.Visibility = Visibility.Collapsed;
                            ReturnButton.Visibility = Visibility.Visible;

                            for (int i = Grid.Columns.Count - 1; i >= 3; i--)
                            {
                                Grid.Columns[i].Visibility = Visibility.Collapsed;
                            }
                        } }
                    break;
                default:
                    return;
            }

            Grid.Items.Refresh();
        }

        private static string? ChosePath()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Json files (*.json) | *.json"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            else
            {
                return null;
            }
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
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                mmi.ptMinTrackSize.x = 1275;
                mmi.ptMinTrackSize.y = 875;
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }


        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
    }
}
