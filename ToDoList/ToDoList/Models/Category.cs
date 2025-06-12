using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList.Models
{
    public class Category
    {
        public string Id { get; set; } = $"category_{Guid.NewGuid()}";
        public string Name { get; set; }
        public string Color { get; set; } = "#FF5722";
        public List<Task> Tasks { get; set; } = new();
    }
}
