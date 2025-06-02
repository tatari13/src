using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LAPS_WebUI.Models;

namespace LAPS_WebUI.Services
{
    public class AuditService
    {
        private readonly string _logFilePath;

        public AuditService(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public async Task LogAsync(string username, string action, string target)
        {
            var entry = new AuditEntry
            {
                Timestamp = DateTime.Now,
                Username = username,
                Action = action,
                Target = target
            };

            var line = JsonSerializer.Serialize(entry);
            await File.AppendAllTextAsync(_logFilePath, line + Environment.NewLine);
        }

        // 🔁 ОНОВЛЕНИЙ МЕТОД: відповідає виклику ReadAuditLogAsync()
        public Task<IEnumerable<AuditEntry>> ReadAuditLogAsync()
        {
            IEnumerable<AuditEntry> entries;

            if (!File.Exists(_logFilePath))
            {
                entries = Enumerable.Empty<AuditEntry>();
            }
            else
            {
                entries = File.ReadAllLines(_logFilePath)
                              .Select(line =>
                              {
                                  try
                                  {
                                      return JsonSerializer.Deserialize<AuditEntry>(line);
                                  }
                                  catch
                                  {
                                      return null;
                                  }
                              })
                              .Where(e => e != null)!;
            }

            return Task.FromResult(entries);
        }
    }
}
