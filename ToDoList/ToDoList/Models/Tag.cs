using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList.Models
{
    public class Tag
    {
        public string Id { get; set; } = $"tag_{Guid.NewGuid()}";
        public string Name { get; set; }
        public List<Task> Tasks { get; set; } = new();
    }
}
