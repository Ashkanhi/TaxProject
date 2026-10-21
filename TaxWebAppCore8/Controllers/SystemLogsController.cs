using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using TaxWebAppCore8.Models;

namespace TaxWebAppCore8.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class SystemLogsController : Controller
    {
        private static readonly Regex EntryStart = new(
            @"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3})",
            RegexOptions.Compiled);

        public IActionResult Index(string? fileName)
        {
            // لاگ‌ها ممکن است اطلاعات فاکتورها و خطاهای داخلی داشته باشند.
            // درخواست رمز موقتاً غیرفعال است؛ محدودیت دسترسی از همین کامپیوتر حفظ می‌شود.
            IPAddress? remoteIp = HttpContext.Connection.RemoteIpAddress;
            if (remoteIp == null || !IPAddress.IsLoopback(remoteIp))
                return StatusCode(403);

            var model = new SystemLogsPageModel();

            string directory = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData),
                "ArghamFinanceCompany",
                "Logs");

            if (!Directory.Exists(directory))
                return View(model);

            List<FileInfo> files;

            try
            {
                // فقط فایل‌های txt همین پوشه؛ مسیر دلخواه کاربر خوانده نمی‌شود.
                files = new DirectoryInfo(directory)
                    .EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly)
                    .Where(f =>
                        (f.Attributes & FileAttributes.ReparsePoint) == 0)
                    .OrderByDescending(f => f.LastWriteTime)
                    .ToList();
            }
            catch (IOException)
            {
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return View(model);
            }

            model.Files = files.Select(f => new SystemLogFileModel
            {
                FileName = f.Name,
                LastWriteTime = f.LastWriteTime
            }).ToList();

            IEnumerable<FileInfo> selectedFiles;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                // در حالت «همه»، جدیدترین ۳۰ فایل بررسی می‌شوند.
                selectedFiles = files.Take(30);
            }
            else
            {
                // نام فایل باید دقیقاً در فهرست فایل‌های همین پوشه باشد.
                FileInfo? selected = files.FirstOrDefault(f =>
                    string.Equals(
                        f.Name,
                        fileName,
                        StringComparison.OrdinalIgnoreCase));

                if (selected == null)
                    return BadRequest("فایل لاگ معتبر نیست.");

                model.SelectedFileName = selected.Name;
                selectedFiles = new[] { selected };
            }

            model.Entries = selectedFiles
                .SelectMany(ReadEntries)
                .OrderByDescending(e => e.Timestamp)
                .Take(300)
                .ToList();

            return View(model);
        }

        private static List<SystemLogEntryModel> ReadEntries(FileInfo file)
        {
            var entries = new List<SystemLogEntryModel>();
            var lastLines = new Queue<string>();

            try
            {
                // هنگام نوشتن لاگ توسط برنامه هم امکان خواندن فایل باقی می‌ماند.
                using var stream = new FileStream(
                    file.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);

                using var reader = new StreamReader(
                    stream,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true);

                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    lastLines.Enqueue(line);

                    // برای جلوگیری از سنگین‌شدن صفحه، انتهای هر فایل کافی است.
                    if (lastLines.Count > 3000)
                        lastLines.Dequeue();
                }
            }
            catch (IOException)
            {
                return entries;
            }
            catch (UnauthorizedAccessException)
            {
                return entries;
            }

            StringBuilder? current = null;
            DateTime currentTime = default;

            void AddCurrent()
            {
                if (current == null)
                    return;

                entries.Add(new SystemLogEntryModel
                {
                    Timestamp = currentTime,
                    FileName = file.Name,
                    Text = current.ToString()
                });
            }

            foreach (string item in lastLines)
            {
                Match match = EntryStart.Match(item);

                if (match.Success &&
                    DateTime.TryParseExact(
                        match.Groups[1].Value,
                        "yyyy-MM-dd HH:mm:ss,fff",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime timestamp))
                {
                    // خطوط بعد از یک تاریخ، از جمله stack trace، متعلق
                    // به همان رخداد هستند و ترتیبشان حفظ می‌شود.
                    AddCurrent();
                    currentTime = timestamp;
                    current = new StringBuilder(item);
                }
                else if (current != null)
                {
                    current.AppendLine().Append(item);
                }
            }

            AddCurrent();
            return entries;
        }
    }
}
