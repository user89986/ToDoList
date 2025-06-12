using System.Configuration;
using System.Data;
using System.Windows;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.DependencyInjection;
using ToDoList.Data;
using ToDoList.Services;
using ToDoList.ViewModels;

namespace ToDoList
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            // Инициализация Firebase
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("todolist-4b5ce-firebase-adminsdk-fbsvc-e5046e05ec.json"),
            });

            base.OnStartup(e);
           
        }
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<AppDbContext>();
          

            services.AddSingleton<AuthService>();
            services.AddSingleton<TagService>();
            services.AddSingleton<SyncService>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<AuthViewModel>();
        }
    }


}
    