using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ToDoList.Models;

namespace ToDoList.Data
{
    public class FirestoreDbContext
    {
        private readonly FirestoreDb _db;

        public FirestoreDbContext(string projectId)
        {
            // Путь к файлу учетных данных
            string credentialPath = "firebase-key.json";

            // Загружаем учетные данные
            GoogleCredential credential;
            using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream);
            }

            // Инициализируем Firestore
            _db = FirestoreDb.Create(projectId, new FirestoreClientBuilder
            {
                Credential = credential
            }.Build());
        }

        // Добавление/обновление задачи
        public async System.Threading.Tasks.Task SyncTaskAsync(Models.Task task)
        {
            DocumentReference taskRef = _db.Collection("tasks").Document(task.Id);
            Dictionary<string, object> data = new()
    {
        { "Title", task.Title },
        { "Description", task.Description },
        { "IsCompleted", task.IsCompleted },
        { "Deadline", task.Deadline.HasValue ? Timestamp.FromDateTime(task.Deadline.Value.ToUniversalTime()) : null },
        { "CreatedAt", Timestamp.FromDateTime(task.CreatedAt.ToUniversalTime()) },
        { "UpdatedAt", Timestamp.FromDateTime(task.UpdatedAt.ToUniversalTime()) },
        { "CategoryId", task.CategoryId },
        { "Priority", task.Priority },
        { "IsDirty", false },
        // 🔽 Добавим список ID тегов
        { "TagIds", task.Tags?.Select(t => t.Id).ToList() ?? new List<string>() }
    };

            await taskRef.SetAsync(data, SetOptions.MergeAll);
        }

        // Получение всех задач
        public async Task<List<Models.Task>> GetAllTasksAsync()
        {
            // Получаем все теги
            var allTags = await _db.Collection("tags").GetSnapshotAsync();
            var tagDict = allTags.Documents.ToDictionary(
                d => d.Id,
                d => new Tag { Id = d.Id, Name = d.GetValue<string>("Name") }
            );

            QuerySnapshot snapshot = await _db.Collection("tasks").GetSnapshotAsync();
            return snapshot.Documents.Select(doc =>
            {
                var task = doc.ConvertTo<Models.Task>();
                task.Id = doc.Id;

                // Получаем список ID тегов
                if (doc.ContainsField("TagIds"))
                {
                    var tagIds = doc.GetValue<List<string>>("TagIds");
                    task.Tags = tagIds
                        .Where(id => tagDict.ContainsKey(id))
                        .Select(id => tagDict[id])
                        .ToList();
                }

                return task;
            }).ToList();
        }

        // Удаление задачи
        public async System.Threading.Tasks.Task DeleteTaskAsync(string taskId)
        {
            await _db.Collection("tasks").Document(taskId).DeleteAsync();
        }
    }
}