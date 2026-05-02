using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;

namespace DiscreteMathToolkit.Infrastructure.Export;

public sealed class TabularData
{
    public string Title { get; }
    public IReadOnlyList<string> Headers { get; }
    public IReadOnlyList<IReadOnlyList<string>> Rows { get; }
    public IReadOnlyList<string> FootNotes { get; }

    public TabularData(string title, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows, IReadOnlyList<string>? footNotes = null)
    {
        Title = title;
        Headers = headers;
        Rows = rows;
        FootNotes = footNotes ?? Array.Empty<string>();
    }
}

public enum ExportFormat { Csv, Markdown, Html }

public interface IExportService
{
    void Export(string path, TabularData data, ExportFormat format);
    string Render(TabularData data, ExportFormat format);
}

public sealed class FileExportService : IExportService
{
    public void Export(string path, TabularData data, ExportFormat format)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, Render(data, format), new UTF8Encoding(false));
    }

    public string Render(TabularData data, ExportFormat format) => format switch
    {
        ExportFormat.Csv => RenderCsv(data),
        ExportFormat.Markdown => RenderMarkdown(data),
        ExportFormat.Html => RenderHtml(data),
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };

    private static string RenderCsv(TabularData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", data.Headers.Select(EscapeCsv)));
        foreach (var row in data.Rows)
            sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));
        return sb.ToString();
    }

    private static string EscapeCsv(string s)
    {
        if (s.Any(c => c == ',' || c == '"' || c == '\n' || c == '\r'))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }

    private static string RenderMarkdown(TabularData data)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(data.Title))
            sb.AppendLine($"# {data.Title}").AppendLine();
        sb.Append("| ").Append(string.Join(" | ", data.Headers.Select(EscapeMd))).AppendLine(" |");
        sb.Append("| ").Append(string.Join(" | ", data.Headers.Select(_ => "---"))).AppendLine(" |");
        foreach (var row in data.Rows)
            sb.Append("| ").Append(string.Join(" | ", row.Select(EscapeMd))).AppendLine(" |");
        if (data.FootNotes.Count > 0)
        {
            sb.AppendLine();
            foreach (var n in data.FootNotes) sb.AppendLine($"> {n}");
        }
        return sb.ToString();
    }

    private static string EscapeMd(string s) => s.Replace("|", "\\|").Replace("\n", " ");

    private static string RenderHtml(TabularData data)
    {
        var enc = HtmlEncoder.Default;
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\">");
        sb.Append("<title>").Append(enc.Encode(data.Title)).AppendLine("</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body{font-family:system-ui,Segoe UI,Roboto,sans-serif;margin:2rem;color:#1f2933;background:#fafbfc;}");
        sb.AppendLine("h1{font-weight:600;margin-bottom:0.5rem;}");
        sb.AppendLine("table{border-collapse:collapse;margin-top:1rem;background:white;box-shadow:0 1px 3px rgba(0,0,0,0.06);}");
        sb.AppendLine("th,td{border:1px solid #d8dee4;padding:0.45rem 0.75rem;text-align:left;font-size:0.95rem;}");
        sb.AppendLine("th{background:#f1f4f7;font-weight:600;}");
        sb.AppendLine("tr:nth-child(even) td{background:#fbfcfd;}");
        sb.AppendLine(".footnote{margin-top:1rem;color:#5b6671;font-size:0.85rem;}");
        sb.AppendLine("</style></head><body>");
        sb.Append("<h1>").Append(enc.Encode(data.Title)).AppendLine("</h1>");
        sb.AppendLine("<table><thead><tr>");
        foreach (var h in data.Headers) sb.Append("<th>").Append(enc.Encode(h)).Append("</th>");
        sb.AppendLine("</tr></thead><tbody>");
        foreach (var row in data.Rows)
        {
            sb.Append("<tr>");
            foreach (var c in row) sb.Append("<td>").Append(enc.Encode(c)).Append("</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody></table>");
        foreach (var n in data.FootNotes)
            sb.Append("<div class=\"footnote\">").Append(enc.Encode(n)).AppendLine("</div>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    public static string DefaultExportDirectory =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "DiscreteMathToolkit", "exports");

    public static string TimestampedFileName(string baseName, ExportFormat format)
    {
        string ext = format switch
        {
            ExportFormat.Csv => "csv",
            ExportFormat.Markdown => "md",
            ExportFormat.Html => "html",
            _ => "txt"
        };
        var now = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        return $"{baseName}-{now}.{ext}";
    }
}
