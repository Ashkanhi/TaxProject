using System;
using System.Collections.Generic;

namespace TaxWebAppCore8.Models
{
    public sealed class SystemLogFileModel
    {
        public string FileName { get; set; } = "";
        public DateTime LastWriteTime { get; set; }
    }

    public sealed class SystemLogEntryModel
    {
        public DateTime Timestamp { get; set; }
        public string FileName { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public sealed class SystemLogsPageModel
    {
        public string? SelectedFileName { get; set; }
        public List<SystemLogFileModel> Files { get; set; } = new();
        public List<SystemLogEntryModel> Entries { get; set; } = new();
    }
}