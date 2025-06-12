using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using System.Windows;
using ToDoList.Services;

namespace ToDoList.ViewModels
{
    public partial class AuthViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly MainViewModel _mainViewModel;

        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public AuthViewModel(AuthService authService, MainViewModel mainViewModel)
        {
            _authService = authService;
            _mainViewModel = mainViewModel;
        }

        [RelayCommand]
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Пожалуйста, введите email и пароль";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // 🔥 Используем новый метод входа
                var user = await _authService.SignInUserAsync(Email, Password);

                // Успешный вход - переключаемся на главное окно
                _mainViewModel.IsLoggedIn = true;
                Application.Current.MainWindow.Hide();

                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task Register()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Пожалуйста, введите email и пароль";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var auth = await _authService.CreateUserAsync(Email, Password);

                // После успешной регистрации автоматически входим
               
            }
            catch (FirebaseAuthException ex)
            {
                ErrorMessage = GetErrorMessage(ex.Reason);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GetErrorMessage(AuthErrorReason reason)
        {
            return reason switch
            {
                AuthErrorReason.UnknownEmailAddress => "Пользователь с таким email не найден",
                AuthErrorReason.WrongPassword => "Неверный пароль",
                AuthErrorReason.InvalidEmailAddress => "Некорректный email",
                AuthErrorReason.WeakPassword => "Пароль слишком простой (минимум 6 символов)",
                AuthErrorReason.EmailExists => "Пользователь с таким email уже существует",
                _ => "Произошла ошибка при аутентификации"
            };
        }
    }
}