using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Security.Cryptography;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.ConstrainedExecution;
using System.Net;
using System.Data;
using System.Net.WebSockets;
using System.Security;
using System.Dynamic;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime;
using System.Net.NetworkInformation;


namespace TaskManager
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var service = new TaskService();
            var storage = new JsonStorageService();
            var tasks = await storage.LoadTasksAsync();
            service.LoadTasks(tasks);

            Console.WriteLine("==============================");
            Console.WriteLine("        TASK MANAGER");
            Console.WriteLine("==============================");
            Console.WriteLine();
            Console.WriteLine("Available commands:\n- add \"Task description\" priority:[low|normal|high] due:YYYY-MM-DD\n- list\n- complete [taskId]\n- delete [taskId]\n- find \"keyword\"\n- sort [priority|date|status]\n- history [taskId]\n- exit");
            Console.WriteLine();

            while (true)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();
                if (input == null) continue;

                var parts = input.Split(' ');
                var commandName = parts[0];

                switch (commandName)
                {
                    case "add":
                        await AddTask(input, service);
                        break;
                    case "list":
                        ListTasks(service.GetAllTasks());
                        break;
                    case "complete":
                        await ChangeStatus(input, service);
                        break;
                    case "delete":
                        await DeleteTask(input, service);
                        break;
                    case "find":
                        await FindTask(input, service);
                        break;
                    case "sort":
                        await SortTask(input, service);
                        break;
                    case "history":
                        await TaskHistory(input, service);
                        break;
                    case "exit":
                        await storage.SaveTasksAsync(service.GetAllTasks()); // пока не будет написана команда "exit", данные не сохранятся в json
                        Console.WriteLine("[+] success");
                        return;
                    default:
                        Console.WriteLine("[-] Error");
                        break;
                }
            }
        }
        private static Task AddTask(string input, TaskService service)
        {
            var listPriority = new[] { "low", "normal", "high" };
            string description = "";
            string priority = "normal";
            DateTime? dueDate = null;

            var matchDescription = Regex.Match(input, "\"(.+?)\"");
            var matchPriority = Regex.Match(input, @"priority:(low|normal|high)");
            var matchDue = Regex.Match(input, @"due:(\d{4}-\d{2}-\d{2})");

            if (matchDescription.Success && matchPriority.Success && matchDue.Success)
            {
                DateTime.TryParse(matchDue.Groups[1].Value, out DateTime parsedDate);
                description = matchDescription.Groups[1].Value;
                priority = matchPriority.Groups[1].Value;
                if(!listPriority.Contains(priority))
                {
                    Console.WriteLine("[-] Error");
                    return Task.CompletedTask;
                }
                dueDate = parsedDate;
            }
            else { Console.WriteLine("[-] Error"); return Task.CompletedTask; }

            service.AddTask(description, priority, dueDate);
            Console.WriteLine("[+] Task added.");
            return Task.CompletedTask;
        }
        private static Task ChangeStatus(string input, TaskService service)
        {
            var numberMatch = Regex.Match(input, @"complete (\d)");
            if (!numberMatch.Success) { Console.WriteLine("[-] Error"); return Task.CompletedTask; }
            int.TryParse(numberMatch.Groups[1].Value, out int IdResult);

            var statusTask = service.ChangeStatus(IdResult);
            if (statusTask == null) { Console.WriteLine("[-] Error"); return Task.CompletedTask; }
            ListTasks(statusTask);
            return Task.CompletedTask;
        }
        private static Task DeleteTask(string input, TaskService service)
        {
            var numberMatch = Regex.Match(input, @"delete (\d)");
            if (!numberMatch.Success) { Console.WriteLine("[-] Error"); return Task.CompletedTask; }
            int.TryParse(numberMatch.Groups[1].Value, out int IdResult);

            var boolTask = service.DeleteTask(IdResult);
            if (!boolTask) Console.WriteLine("[-] Id is wrong");
            else Console.WriteLine("[+] success");
            return Task.CompletedTask;
        }
        private static Task FindTask(string input, TaskService service)
        {
            var inputMatch = Regex.Match(input, "find \"(.+?)\"");
            if (!inputMatch.Success) { Console.WriteLine("[-] Error"); return Task.CompletedTask; }
            var part = inputMatch.Groups[1].Value;
            if (string.IsNullOrWhiteSpace(part)) { Console.WriteLine("[-] Error"); return Task.CompletedTask; ; }

            var task = service.FindTask(part);
            if (task == null) { Console.WriteLine("[-] Does not exist"); return Task.CompletedTask; }
            ListTasks(task);
            return Task.CompletedTask;
        }
        private static Task SortTask(string input, TaskService service)
        {
            var sortMatch = Regex.Match(input, @"sort (priority|due|status)");
            var part = sortMatch.Groups[1].Value;
            if (!sortMatch.Success) { Console.WriteLine("[-] Error"); return Task.CompletedTask; }

            var tasks = service.SortTask(part);
            if (tasks == null) { Console.WriteLine("[-] Error"); return Task.CompletedTask; }
            ListTasks(tasks);
            return Task.CompletedTask;
        }
        private static Task TaskHistory(string input, TaskService service)
        {
            var idMatch = Regex.Match(input, @"history (\d)");
            if (!idMatch.Success || !int.TryParse(idMatch.Groups[1].Value, out int IdResult)) { Console.WriteLine("[-] Error"); return Task.CompletedTask; }

            var historyList = service.HistoryTask(IdResult);

            for (int i = 0; i < historyList.Count; i++)
                Console.WriteLine($"- {historyList[i]}");
            return Task.CompletedTask;
        }
        private static void ListTasks(List<TaskItem> tasks)
        {
            if (tasks.Count == 0) { Console.WriteLine("[-] The list is empty"); return; }

            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("| ID | Description                 | Due       | Priority | Done |");
            Console.WriteLine("-----------------------------------------------------------------");

            foreach (var task in tasks)
            {
                Console.WriteLine($"| {task.Id,-1} | {task.Description,-28} | {task.DueDate?.ToString("yyyy-MM-dd"),-8} | {task.Priority,-6} | {(task.IsCompleted ? "Yes" : "No"),-2} |");
            }

            Console.WriteLine("-----------------------------------------------------------------");
        }

    }
}
