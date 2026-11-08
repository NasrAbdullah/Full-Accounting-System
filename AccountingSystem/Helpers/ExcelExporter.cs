using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AccountingSystem.Helpers
{
    /// <summary>
    /// تصدير الجداول إلى Excel (بصيغة CSV متوافقة مع Excel)
    /// أو HTML ليُفتح في Excel مع التنسيق
    /// </summary>
    public static class ExcelExporter
    {
        /// <summary>
        /// تصدير DataGridView إلى ملف Excel
        /// </summary>
        public static void ExportDataGridView(DataGridView dgv, string fileName = null)
        {
            if (dgv.Rows.Count == 0)
            {
                UiHelper.Warn("لا توجد بيانات للتصدير.");
                return;
            }

            var sfd = new SaveFileDialog();
            sfd.Filter = "Excel Files (*.xls)|*.xls|CSV Files (*.csv)|*.csv";
            sfd.FileName = fileName != null
                ? fileName
                : "Export_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (sfd.ShowDialog() != DialogResult.OK) return;

            string path = sfd.FileName;

            if (path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                ExportCSV(dgv, path);
            else
                ExportExcelHTML(dgv, path);

            UiHelper.Info("تم التصدير بنجاح ✅\n\n" + path);

            try
            {
                System.Diagnostics.Process.Start(path);
            }
            catch { }
        }

        /// <summary>
        /// تصدير DataTable إلى Excel
        /// </summary>
        public static void ExportDataTable(DataTable dt, string title = "تقرير")
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                UiHelper.Warn("لا توجد بيانات للتصدير.");
                return;
            }

            var sfd = new SaveFileDialog();
            sfd.Filter = "Excel Files (*.xls)|*.xls";
            sfd.FileName = title.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (sfd.ShowDialog() != DialogResult.OK) return;

            string path = sfd.FileName;

            var sb = new StringBuilder();
            sb.AppendLine("<html dir='rtl'><head><meta charset='utf-8'>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; padding: 20px; }");
            sb.AppendLine("h1 { text-align: center; color: #1e40af; font-size: 18px; margin-bottom: 5px; }");
            sb.AppendLine("h2 { text-align: center; color: #666; font-size: 12px; font-weight: normal; margin-bottom: 20px; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
            sb.AppendLine("th { background-color: #1e40af; color: white; padding: 10px 8px; font-size: 12px; text-align: center; }");
            sb.AppendLine("td { border: 1px solid #ddd; padding: 8px; font-size: 11px; }");
            sb.AppendLine("tr:nth-child(even) { background-color: #f8f8f8; }");
            sb.AppendLine(".num { text-align: left; direction: ltr; }");
            sb.AppendLine(".footer { text-align: center; color: #999; margin-top: 20px; font-size: 10px; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<h1>" + EscapeHTML(title) + "</h1>");
            sb.AppendLine("<h2>تاريخ التصدير: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm") + "</h2>");

            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr>");

            foreach (DataColumn col in dt.Columns)
                sb.AppendLine("<th>" + EscapeHTML(col.ColumnName) + "</th>");

            sb.AppendLine("</tr></thead><tbody>");

            foreach (DataRow row in dt.Rows)
            {
                sb.AppendLine("<tr>");
                foreach (var val in row.ItemArray)
                {
                    string text = val == null || val == DBNull.Value ? "" : val.ToString();
                    string cls = IsNumeric(val) ? " class='num'" : "";
                    sb.AppendLine("<td" + cls + ">" + EscapeHTML(text) + "</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine("<div class='footer'>نظام المحاسبة المتكامل</div>");
            sb.AppendLine("</body></html>");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

            UiHelper.Info("تم التصدير بنجاح ✅\n\n" + path);

            try { System.Diagnostics.Process.Start(path); } catch { }
        }

        private static void ExportCSV(DataGridView dgv, string path)
        {
            var sb = new StringBuilder();

            // Headers
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (!dgv.Columns[i].Visible) continue;
                if (i > 0) sb.Append(",");
                sb.Append("\"" + dgv.Columns[i].HeaderText + "\"");
            }
            sb.AppendLine();

            // Rows
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;
                bool first = true;
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    if (!dgv.Columns[i].Visible) continue;
                    if (!first) sb.Append(",");
                    first = false;
                    var val = row.Cells[i].Value;
                    sb.Append("\"" + (val != null ? val.ToString().Replace("\"", "\"\"") : "") + "\"");
                }
                sb.AppendLine();
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static void ExportExcelHTML(DataGridView dgv, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html dir='rtl'><head><meta charset='utf-8'>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; padding: 20px; }");
            sb.AppendLine("h1 { text-align: center; color: #1e40af; font-size: 18px; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; margin-top: 15px; }");
            sb.AppendLine("th { background-color: #1e40af; color: white; padding: 10px 8px; font-size: 12px; text-align: center; border: 1px solid #ddd; }");
            sb.AppendLine("td { border: 1px solid #ddd; padding: 8px; font-size: 11px; }");
            sb.AppendLine("tr:nth-child(even) { background-color: #f8f8f8; }");
            sb.AppendLine(".footer { text-align: center; color: #999; margin-top: 20px; font-size: 10px; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<h1>نظام المحاسبة المتكامل</h1>");
            sb.AppendLine("<h2 style='text-align:center;color:#666;font-size:12px;font-weight:normal;'>" +
                DateTime.Now.ToString("yyyy/MM/dd HH:mm") + "</h2>");

            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr>");

            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (!dgv.Columns[i].Visible) continue;
                sb.AppendLine("<th>" + EscapeHTML(dgv.Columns[i].HeaderText) + "</th>");
            }
            sb.AppendLine("</tr></thead><tbody>");

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;
                sb.AppendLine("<tr>");
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    if (!dgv.Columns[i].Visible) continue;
                    var val = row.Cells[i].Value;
                    string text = val != null ? val.ToString() : "";
                    sb.AppendLine("<td>" + EscapeHTML(text) + "</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine("<div class='footer'>نظام المحاسبة المتكامل</div>");
            sb.AppendLine("</body></html>");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static bool IsNumeric(object val)
        {
            if (val == null || val == DBNull.Value) return false;
            decimal d;
            return decimal.TryParse(val.ToString(), out d);
        }

        private static string EscapeHTML(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;");
        }
    }
}