using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ToDoList.Models;
using ToDoList.Services;

namespace ToDoList.ViewModels
{
    public partial class EditTaskViewModel : ObservableObject
    {
        private readonly Window _window;
        private readonly TagService _tagService;
        private readonly Models.Task _originalTask;

        public Models.Task Task { get; set; }
        public ObservableCollection<Category> Categories { get; set; }

        [ObservableProperty]
        private ObservableCollection<TagItemViewModel> _availableTags = new();

        [ObservableProperty]
        private TagItemViewModel? _selectedTag;

        public EditTaskViewModel(
            Window window,
            Models.Task task,
            ObservableCollection<Category> categories,
            TagService tagService)
        {
            _window = window;
            _tagService = tagService;
            _originalTask = task;

            Task = new Models.Task
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                Category = task.Category,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Tags = task.Tags != null ? new List<Tag>(task.Tags) : new List<Tag>()
            };

            Categories = categories;
            _ = LoadTagsAsync();
        }

        private async System.Threading.Tasks.Task LoadTagsAsync()
        {
            var allTags = await _tagService.GetAllTagsAsync();
            if (allTags != null)
            {
                AvailableTags = new ObservableCollection<TagItemViewModel>(
                    allTags.Select(t => new TagItemViewModel
                    {
                        Tag = t,
                        IsSelected = Task.Tags?.Any(tt => tt.Id == t.Id) ?? false
                    }));
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task AddTagAsync(string? tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName)) return;

            var existingTag = AvailableTags.FirstOrDefault(t =>
                t.Tag?.Name?.Equals(tagName, StringComparison.OrdinalIgnoreCase) ?? false);

            if (existingTag != null)
            {
                existingTag.IsSelected = true;
                return;
            }

            var newTag = await _tagService.AddTagAsync(tagName);
            if (newTag != null)
            {
                AvailableTags.Add(new TagItemViewModel
                {
                    Tag = newTag,
                    IsSelected = true
                });
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task ApplyTagsAsync()
        {
            if (Task.Tags == null) return;

            var selectedTags = AvailableTags
                .Where(t => t.IsSelected && t.Tag != null)
                .Select(t => t.Tag!)
                .ToList();

            foreach (var tag in Task.Tags.ToList())
            {
                if (!selectedTags.Any(t => t.Id == tag.Id))
                {
                    await _tagService.RemoveTagFromTaskAsync(Task.Id, tag.Id);
                }
            }

            foreach (var tag in selectedTags)
            {
                if (!Task.Tags.Any(t => t.Id == tag.Id))
                {
                    await _tagService.AddTagToTaskAsync(Task.Id, tag.Id);
                }
            }

            Task.Tags = selectedTags;
        }

        [RelayCommand]
        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Task.Title))
            {
                MessageBox.Show("Название задачи не может быть пустым", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _originalTask.Title = Task.Title;
            _originalTask.Description = Task.Description;
            _originalTask.Deadline = Task.Deadline;
            _originalTask.Category = Task.Category;
            _originalTask.Tags = Task.Tags ?? new List<Tag>();

            _window.DialogResult = true;
            _window.Close();
        }

        [RelayCommand]
        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
}