using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Google.Cloud.Firestore;


namespace ToDoList.Models
{
    [FirestoreData]
    public class Task : ObservableObject
    {

        [FirestoreDocumentId]
        public string Id { get; set; } = $"task_{Guid.NewGuid()}"; // Генерация ID
        [FirestoreProperty]
        public string Title { get; set; }
        [FirestoreProperty]
        public string? Description { get; set; }
        [FirestoreProperty]
        public bool IsCompleted { get; set; }
        [FirestoreProperty]
        public DateTime? Deadline { get; set; }
        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [FirestoreProperty]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [FirestoreProperty]
        public bool IsDirty { get; set; } // Для синхронизации
        [FirestoreProperty]
        public int Priority { get; set; } = 1;

        // Связи
        [FirestoreProperty]
        public string? CategoryId { get; set; }
        public Category? Category { get; set; }
        public List<Tag> Tags { get; set; } = new();
    }
}
