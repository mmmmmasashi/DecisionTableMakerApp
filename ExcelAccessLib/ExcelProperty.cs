using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelAccessLib
{
    public record ExcelProperty
    {
        public string Title { get; }
        public string SheetName { get; }
        public string Author { get; }

        /// <summary>
        /// 任意の数だけシートに書き込むプロパティを追加できる
        /// </summary>
        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();

        public ExcelProperty(string sheetName, string title, string creatorName, Dictionary<string, string>? properties = null)
        {
            if (string.IsNullOrEmpty(sheetName))
            {
                throw new ArgumentException("Sheet name cannot be null or empty.", nameof(sheetName));
            }

            Title = title;
            SheetName = sheetName;

            if (string.IsNullOrEmpty(creatorName))
            {
                Author = "Unknown";
            }
            else
            {
                Author = creatorName;
            }

            Properties = properties ?? new Dictionary<string, string>();
        }
    }
}
