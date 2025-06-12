using System.Windows;
using System.Windows.Controls;
using ToDoList.Data;
using ToDoList.Services;
using ToDoList.ViewModels;

namespace ToDoList
{
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();

            var db = new AppDbContext();
            var firestoreDb = new FirestoreDbContext("todolist-4b5ce");
            var authService = new AuthService();
            var tagService = new TagService(db, firestoreDb);

            DataContext = new AuthViewModel(authService, new MainViewModel(authService, tagService));
       
        

           

            // Привязка PasswordBox к ViewModel
            PasswordBox.PasswordChanged += (sender, e) =>
            {
                if (DataContext is AuthViewModel vm)
                {
                    vm.Password = PasswordBox.Password;
                }
            };
        }
    }
}