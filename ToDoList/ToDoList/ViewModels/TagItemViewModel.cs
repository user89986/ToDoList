using CommunityToolkit.Mvvm.ComponentModel;
using ToDoList.Models;

namespace ToDoList.ViewModels
{
    public partial class TagItemViewModel : ObservableObject
    {
        public Tag? Tag { get; set; }

        [ObservableProperty]
        private bool _isSelected;
    }
}