using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskManager
{
    class TaskService
    {
        private object listLock = new object();
        private int id = 1;
        private List<TaskItem> tasks = new List<TaskItem>();
        public void AddTask(string description, string priority, DateTime? dateTime)
        {
            var information = new TaskItem()
            {
                Description = description,
                Priority = priority,
                Id = id++,
                DueDate = dateTime,
                IsCompleted = false,
                History = new List<string> { $"Created at {DateTime.Now}" }
            };
            tasks.Add(information);
        }
        public List<TaskItem> ChangeStatus(int id)
        {
            int taskFound = 0; 
            var taskStatus = new List<TaskItem>();
            Parallel.ForEach(tasks, task =>
            {
                if (task.Id == id && !task.IsCompleted)
                {
                    lock (listLock)
                    {
                        task.IsCompleted = true;
                        task.History.Add($"Marked as completed at {DateTime.Now}");
                        taskStatus.Add(task);
                    }
                    Interlocked.Exchange(ref taskFound, 1);
                    return;
                }
            });
            return taskFound == 1 ? taskStatus : null;
        }
        public bool DeleteTask(int id)
        {
            int taskFound = 0;
            Parallel.ForEach(tasks, task =>
            {
                if (task.Id == id)
                {
                    lock(listLock) tasks.Remove(task);
                    Interlocked.Exchange(ref taskFound, 1);
                    return;
                }
            });
            return taskFound == 1;
        }
        public List<TaskItem> FindTask(string part)
        {
            int taskFound = 0;
            var _taskFound = new List<TaskItem>();
            Parallel.ForEach(tasks, task =>
            {
                var taskDescriptions = task.Description.Contains(part);
                if (taskDescriptions)
                {
                    lock (listLock) _taskFound.Add(task);
                    Interlocked.Exchange(ref taskFound, 1);
                }
            });
            return taskFound == 1 ? _taskFound : null;
        }
        public List<TaskItem> SortTask(string input)
        {
            return input switch
            {
                "priority" => tasks.OrderBy(x => PriorityNumber(x.Priority)).ToList(),
                "status" => tasks.Where(x => x.IsCompleted).ToList(),
                "due" => tasks.OrderBy(x => x.DueDate ?? DateTime.MaxValue).ToList(),
                _ => null
            };
        }
        public List<string> HistoryTask(int id)
        {
            int taskFound = 0;
            var listHistory = new List<string>();
            Parallel.ForEach(tasks, task =>
            {
                if(task.Id == id)
                {
                    Interlocked.Exchange(ref taskFound, 1);
                    lock(listLock)
                    {
                        foreach (var history in task.History)
                            listHistory.Add(history);
                    }
                }
            });
            return taskFound == 1 ? listHistory : null;
        }
        private int PriorityNumber(string input)
        {
            return input switch
            {
                "low" => 1,
                "normal" => 2,
                "high" => 3,
                _ => 0
            };
        }
        public List<TaskItem> GetAllTasks() { return tasks; }
        public void LoadTasks(List<TaskItem> task)
        {
            tasks = task;
            id = tasks.Any() ? tasks.Max(t => t.Id) + 1 : 1;
        }
    }
}
