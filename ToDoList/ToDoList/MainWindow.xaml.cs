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
using Google.Cloud.Firestore;
using ToDoList.Data;
using ToDoList.Services;
using ToDoList.ViewModels;

namespace ToDoList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _dragStartPoint;
        public MainWindow()
        {
            InitializeComponent();
            var db = new AppDbContext();
        var firestoreDb = new FirestoreDbContext("todolist-4b5ce");
        
        // Создаем сервисы
        var authService = new AuthService();
        var tagService = new TagService(db, firestoreDb);
        
        // Создаем ViewModel и устанавливаем DataContext
        DataContext = new MainViewModel(authService, tagService);
        }
        private void TaskList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);

            // Убедимся, что выделен элемент под курсором
            var listBox = sender as ListBox;
            var item = GetObjectDataFromPoint<ToDoList.Models.Task>(listBox, e.GetPosition(listBox));
            if (item != null)
            {
                listBox.SelectedItem = item;
            }
        }

        private void TaskList_MouseMove(object sender, MouseEventArgs e)
        {
            var listBox = sender as ListBox;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(null);
                if ((Math.Abs(currentPosition.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(currentPosition.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    var selectedItem = listBox?.SelectedItem;
                    if (selectedItem != null)
                    {
                        DragDrop.DoDragDrop(listBox, selectedItem, DragDropEffects.Move);
                    }
                }
            }
        }

        private void TaskList_Drop(object sender, DragEventArgs e)
        {
            var listBox = sender as ListBox;
            var viewModel = DataContext as MainViewModel;
            if (listBox == null || viewModel == null) return;

            var droppedData = e.Data.GetData(typeof(ToDoList.Models.Task)) as ToDoList.Models.Task;
            var target = GetObjectDataFromPoint<ToDoList.Models.Task>(listBox, e.GetPosition(listBox));
            if (droppedData == null || target == null || droppedData == target) return;

            var tasks = viewModel.FilteredTasks;
            int oldIndex = tasks.IndexOf(droppedData);
            int newIndex = tasks.IndexOf(target);

            if (oldIndex != -1 && newIndex != -1)
            {
                tasks.Move(oldIndex, newIndex);
            }
        }

        // Utility to get item under mouse
        private T GetObjectDataFromPoint<T>(ItemsControl source, Point point) where T : class
        {
            // Находим элемент под курсором
            UIElement element = source.InputHitTest(point) as UIElement;

            // Поднимаемся по визуальному дереву, пока не найдем контейнер элемента
            while (element != null)
            {
                if (element is ListViewItem item)
                {
                    return item.Content as T;
                }
                element = VisualTreeHelper.GetParent(element) as UIElement;
            }

            return null;
        }

    }
}