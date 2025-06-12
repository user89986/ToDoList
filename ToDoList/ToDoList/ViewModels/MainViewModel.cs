using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using ToDoList.Data;
using ToDoList.Models;
using ToDoList.Services;
using Task = ToDoList.Models.Task;

namespace ToDoList.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly AppDbContext _db;
        private readonly FirestoreDbContext _firestoreDb;
        private readonly SyncService _syncService;
        private readonly AuthService _authService;
        private readonly TagService _tagService;
        private readonly TaskService _taskService;

        [ObservableProperty] private string _currentUserEmail = string.Empty;
        [ObservableProperty] private string newTaskTitle = string.Empty;
        [ObservableProperty] private string newTaskDescription = string.Empty;
        [ObservableProperty] private DateTime? newTaskDeadline;
        [ObservableProperty] private Category newTaskCategory;
        [ObservableProperty] private ObservableCollection<Task> tasks = new();
        [ObservableProperty] private ObservableCollection<Task> filteredTasks = new();
        [ObservableProperty] private ObservableCollection<Category> categories = new();
        [ObservableProperty] private ObservableCollection<Tag> tags = new();
        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private string syncStatus = "Готово";
        [ObservableProperty] private bool isSyncing;
        [ObservableProperty] private Category selectedCategory;
        [ObservableProperty] private ObservableCollection<TagItemViewModel> _availableTags;
        [ObservableProperty] private string selectedFilterStatus = "Все";
        [ObservableProperty] private DateTime? selectedDate;
        [ObservableProperty] private bool isDarkTheme;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private bool isLoggedIn;
        public List<string> FilterStatusOptions { get; } = new List<string> { "Все", "Активные", "Завершенные" };
        public MainViewModel(AuthService authService, TagService tagService)
        {
            _authService = authService;
            _tagService = tagService;
            _db = new AppDbContext();
            _db.Database.EnsureCreated();
            _firestoreDb = new FirestoreDbContext("todolist-4b5ce");
            _syncService = new SyncService(_db, _firestoreDb);
            System.Threading.Tasks.Task.Run(async () => await PeriodicSyncAsync());
            _taskService = new TaskService(_db, _syncService, _tagService);
            LoadLocalData();
            ApplyFilters();
            LoadTags();
        }

        private async void LoadTags()
        {
            Tags = new ObservableCollection<Tag>(await _tagService.GetAllTagsAsync());
        }

        private async System.Threading.Tasks.Task PeriodicSyncAsync()
        {
            while (true)
            {
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromMinutes(5));
                await SyncCommand.ExecuteAsync(null);
            }
        }

        private void LoadLocalData()
        {
            Tasks = new ObservableCollection<Task>(_db.Tasks
                .Include(t => t.Category)
                .Include(t => t.Tags)
                .ToList());

            ApplyFilters();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task AddTask()
        {

            await _taskService.AddTaskAsync(
            NewTaskTitle,
            NewTaskDescription,
            NewTaskDeadline,
            NewTaskCategory,
            Tasks,
            ClearTaskForm,
            ApplyFilters);
        }

        [RelayCommand]
        private void EditTask(Task task)
        {
            _taskService.EditTask(task, Tasks, Categories, ApplyFilters);
        }

        [RelayCommand]
        private void DeleteTask(Task task)
        {
            _taskService.DeleteTask(task, Tasks, ApplyFilters);
        }

        [RelayCommand]
        private void ToggleTaskStatus(Task task)
        {
            using (var db = new AppDbContext())
            {
                var taskFromDb = db.Tasks.FirstOrDefault(t => t.Id == task.Id);
                if (taskFromDb != null)
                {
                    taskFromDb.IsCompleted = !taskFromDb.IsCompleted;
                    taskFromDb.UpdatedAt = DateTime.UtcNow;
                    db.SaveChanges();

                    task.IsCompleted = taskFromDb.IsCompleted;
                    ApplyFilters();
                }
            }
        }

        [RelayCommand]
        private void AddCategory()
        {
            var categoryName = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите название категории:",
                "Новая категория",
                "");

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var category = new Category { Name = categoryName };
                _db.Categories.Add(category);
                _db.SaveChanges();
                Categories.Add(category);
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task AddTag()
        {
            var tag = await _tagService.CreateTagWithUI();
            if (tag != null)
            {
                Tags.Add(tag);
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task AddTagToTask(Task task)
        {
            if (await _tagService.AddTagToTaskWithUI(task, Tags))
            {
                await RefreshTask(task);
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task RefreshTask(Task task)
        {
            var updatedTask = await _db.Tasks
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == task.Id);

            if (updatedTask != null)
            {
                var index = Tasks.IndexOf(task);
                if (index >= 0)
                {
                    Tasks[index] = updatedTask;
                }
                ApplyFilters();
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task Sync()
        {
            if (IsSyncing) return;
            IsSyncing = true;
            SyncStatus = "Синхронизация...";
            await _syncService.FullSyncAsync();
            LoadLocalData();
            SyncStatus = "Синхронизация завершена";
        }

        [RelayCommand]
        private void Search()
        {
            ApplyFilters();
        }

        [RelayCommand]
        private void ApplyFilters()
        {
            var query = Tasks.AsQueryable();

            switch (SelectedFilterStatus)
            {
                case "Активные":
                    query = query.Where(t => !t.IsCompleted);
                    break;
                case "Завершенные":
                    query = query.Where(t => t.IsCompleted);
                    break;
            }
            if (SelectedCategory != null)
            {
                query = query.Where(t => t.Category != null && t.Category.Id == SelectedCategory.Id);
            }

            if (SelectedDate != null)
            {
                query = query.Where(t => t.Deadline != null &&
                    t.Deadline.Value.Date == SelectedDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(t => t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description != null && t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            FilteredTasks = new ObservableCollection<Task>(query.ToList());
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedCategory = null;
            SelectedDate = null;
            SelectedFilterStatus = "Все";
            SearchText = string.Empty;
            ApplyFilters();
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(IsDarkTheme ? BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);
        }

        [RelayCommand]
        private void Logout()
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                IsLoggedIn = false;
                Application.Current.MainWindow.Close();
                var authWindow = new AuthWindow();
                authWindow.Show();
            }
        }

        [RelayCommand]
        private void SetReminder(Task task)
        {
            if (task.Deadline.HasValue)
            {
                MessageBox.Show($"Напоминание установлено на {task.Deadline:dd.MM.yyyy HH:mm}",
                    "Напоминание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("У задачи не установлен дедлайн",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        [RelayCommand]
        private void FilterByTag(Tag tag)
        {
            FilteredTasks = new ObservableCollection<Task>(
                Tasks.Where(t => t.Tags.Any(tg => tg.Id == tag.Id)).ToList());
        }
        private void ClearTaskForm()
        {
            NewTaskTitle = string.Empty;
            NewTaskDescription = string.Empty;
            NewTaskDeadline = null;
            NewTaskCategory = null;
        }
    }
}