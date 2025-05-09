using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace TaskManager
{
    class JsonStorageService
    {
        private string filePath = "storage.json";

        public async Task<List<TaskItem>> LoadTasksAsync()
        {
            using (var stream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                if (stream.Length == 0) return new List<TaskItem>();

                var jsonAnswer = await JsonSerializer.DeserializeAsync<List<TaskItem>>(stream);
                return jsonAnswer;
            }
        }

        public async Task SaveTasksAsync(List<TaskItem> taskItems)
        {
            using (var stream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                await JsonSerializer.SerializeAsync(stream, taskItems, new JsonSerializerOptions 
                { 
                    WriteIndented = true, 
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
                });
            }
        }
    }
}
