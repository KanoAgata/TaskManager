using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager
{
    class TaskItem
    {
        public int Id { get; set; }                    // Уникальный id задачи
        public string Description { get; set; } = "";  // Текст задачи(описание)
        public DateTime? DueDate { get; set; }         // Дата, когда нужно закончить
        public string Priority { get; set; } = "normal"; // Приоритет: low, normal, high
        public bool IsCompleted { get; set; } = false; // Статус выполнения
        public List<string> History { get; set; } = new(); // История изменений задачи
    }
}
