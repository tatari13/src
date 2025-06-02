using LAPS_WebUI.Models;
using LAPS_WebUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using static LAPS_WebUI.Dialogs.AuditFilterDialog;
using System.Text;

namespace LAPS_WebUI.Pages
{
    public partial class Audit
    {
        private List<AuditEntry> auditEntries = new();
        private List<AuditEntry> filteredEntries = new();

        private string FilterUsername = "";
        private string FilterTarget = "";
        private DateTime? FilterDateFrom = null;
        private DateTime? FilterDateTo = null;

        protected override async Task OnInitializedAsync()
        {
            auditEntries = (await AuditService.ReadAuditLogAsync()).ToList();
            filteredEntries = auditEntries;
        }

        private void ApplyFilters()
        {
            filteredEntries = auditEntries
                .Where(entry =>
                    (string.IsNullOrEmpty(FilterUsername) || entry.Username.Contains(FilterUsername, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(FilterTarget) || entry.Target.Contains(FilterTarget, StringComparison.OrdinalIgnoreCase)) &&
                    (!FilterDateFrom.HasValue || entry.Timestamp >= FilterDateFrom.Value) &&
                    (!FilterDateTo.HasValue || entry.Timestamp <= FilterDateTo.Value))
                .ToList();
        }

        private void ClearFilters()
        {
            FilterUsername = "";
            FilterTarget = "";
            FilterDateFrom = null;
            FilterDateTo = null;
            filteredEntries = auditEntries;
        }

        private async Task ExportToCsv()
        {
            var csv = new StringBuilder();
            csv.AppendLine("Username,Target,Date");

            foreach (var entry in filteredEntries)
            {
                var line = $"{Escape(entry.Username)},{Escape(entry.Target)},{entry.Timestamp:dd.MM.yyyy HH:mm}";
                csv.AppendLine(line);
            }

            var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
            var base64 = Convert.ToBase64String(csvBytes);
            await JSRuntime.InvokeVoidAsync("downloadFileFromBlazor", "audit-log.csv", base64);
        }

        private string Escape(string input) => $"\"{input.Replace("\"", "\"\"")}\"";

        private async Task OpenFilterDialog()
        {
            var parameters = new DialogParameters
            {
                [nameof(FilterResult.Username)] = FilterUsername,
                [nameof(FilterResult.Target)] = FilterTarget,
                [nameof(FilterResult.DateFrom)] = FilterDateFrom,
                [nameof(FilterResult.DateTo)] = FilterDateTo
            };

            var dialog = DialogService.Show<Dialogs.AuditFilterDialog>("Налаштувати фільтр", parameters);
            var result = await dialog.Result;

            if (!result.Cancelled && result.Data is FilterResult filter)
            {
                FilterUsername = filter.Username;
                FilterTarget = filter.Target;
                FilterDateFrom = filter.DateFrom;
                FilterDateTo = filter.DateTo;
                ApplyFilters();
            }
        }
    }
}
