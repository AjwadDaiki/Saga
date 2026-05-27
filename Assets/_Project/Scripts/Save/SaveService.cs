using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Saga.Save
{
    public sealed class SaveService
    {
        private const string FileName = "save.json";

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public async Task<SaveData> LoadAsync()
        {
            if (!File.Exists(FilePath))
            {
                return new SaveData();
            }

            var json = await File.ReadAllTextAsync(FilePath);
            return JsonConvert.DeserializeObject<SaveData>(json) ?? new SaveData();
        }

        public async Task SaveAsync(SaveData data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.None);
            await File.WriteAllTextAsync(FilePath, json);
        }
    }

    public class SaveData
    {
        public int SaveVersion { get; set; } = 1;
    }
}
