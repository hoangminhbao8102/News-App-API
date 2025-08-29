using System.Text;

namespace NewsAppApi.Utils
{
    public static class CsvUtil
    {
        public static string Escape(string? input)
        {
            if (input is null) return "";
            var needsQuote = input.Contains(',') || input.Contains('"') || input.Contains('\n') || input.Contains('\r');
            var s = input.Replace("\"", "\"\"");
            return needsQuote ? $"\"{s}\"" : s;
        }

        public static string ToCsv<T>(IEnumerable<T> rows, IReadOnlyList<(string Header, Func<T, string> Selector)> cols)
        {
            var sb = new StringBuilder();

            // header
            sb.AppendLine(string.Join(",", cols.Select(c => Escape(c.Header))));

            // rows
            foreach (var r in rows)
            {
                var cells = cols.Select(c => Escape(c.Selector(r)));
                sb.AppendLine(string.Join(",", cells));
            }
            return sb.ToString();
        }
    }
}
