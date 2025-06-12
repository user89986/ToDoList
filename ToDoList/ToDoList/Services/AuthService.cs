using FirebaseAdmin.Auth;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Firebase.Auth.Providers;
using ToDoList.Services;

public class AuthService
{

    public AuthService()
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("firebase-key.json")
            });
        }
    }

    public async Task<string> CreateUserAsync(string email, string password)
    {
        var userArgs = new UserRecordArgs
        {
            Email = email,
            Password = password,
            EmailVerified = false,
            Disabled = false
        };

        var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
        return userRecord.Uid;
    }
    public async Task<UserRecord> SignInUserAsync(string email, string password)
    {
        try
        {
            // Получаем пользователя по email
            var userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
            return userRecord;
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            throw new Exception("Пользователь не найден");
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка входа: {ex.Message}");
        }
    }
}

