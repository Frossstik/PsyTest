using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Domain.Logic.Interfaces;
using Xceed.Document.NET;
using Xceed.Words.NET;
using ScottPlot;

namespace Psytest.ServiceMain.Domain.Logic
{
    public class SchmieschekTestProcessor : ITestProcessor, IChartGenerator
    {
        public TestResult Process(TestSession session, object answers)
        {
            var schm = (SchmieschekAnswers)answers;
            var (scaleNames, sums, average) = Compute(schm);

            var lines = new List<string>();
            for (int i = 0; i < scaleNames.Length; i++)
                lines.Add(string.Format("{0,-4} {1,-40} {2,6}",
                    i + 1, scaleNames[i] + ":", (int)sums[i]));
            lines.Add($"Среднее значение: {average:F2}");

            string resultText = string.Join(Environment.NewLine, lines);

            var chartBytes = GenerateBarChartBytes(scaleNames, sums.Select(s => (double)s).ToList());
            var reportBytes = GenerateDocxReport(session.Id, answers, resultText);

            return new TestResult
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ResultText = resultText,
                ReportBytes = reportBytes,
                Images = new List<byte[]> { chartBytes }
            };
        }


        public byte[] GenerateDocxReport(Guid sessionId, object answers, string resultText)
        {
            var schm = (SchmieschekAnswers)answers;
            var (scaleNames, sums, averages) = Compute(schm);
            var chartBytes = GenerateBarChartBytes(scaleNames, sums.Select(s => (double)s).ToList());
            double average = sums.Average();

            using var ms = new MemoryStream();
            using (var doc = DocX.Create(ms))
            {
                var title = doc.InsertParagraph("Результаты теста Шмишека");
                title.FontSize(16).Bold().Alignment = Xceed.Document.NET.Alignment.center;
                doc.InsertParagraph("");

                doc.InsertParagraph($"Дата тестирования: {DateTime.Now:dd.MM.yyyy HH:mm}")
                   .FontSize(12);
                doc.InsertParagraph("");

                var table = doc.AddTable(scaleNames.Length + 1, 3);
                table.Alignment = Xceed.Document.NET.Alignment.center;
                table.Design = TableDesign.ColorfulList;

                table.Rows[0].Cells[0].Paragraphs[0].Append("#").Bold();
                table.Rows[0].Cells[1].Paragraphs[0].Append("Шкала").Bold();
                table.Rows[0].Cells[2].Paragraphs[0].Append("Сумма").Bold();

                for (int i = 0; i < scaleNames.Length; i++)
                {
                    table.Rows[i + 1].Cells[0].Paragraphs[0].Append((i + 1).ToString());
                    table.Rows[i + 1].Cells[1].Paragraphs[0].Append(scaleNames[i]);
                    table.Rows[i + 1].Cells[2].Paragraphs[0].Append(((int)sums[i]).ToString());
                }

                if (chartBytes?.Length > 0)
                {
                    using var imgStream = new MemoryStream(chartBytes);
                    var img = doc.AddImage(imgStream);
                    var pic = img.CreatePicture(350, 400);
                    var p = doc.InsertParagraph();
                    p.AppendPicture(pic);
                    p.Alignment = Xceed.Document.NET.Alignment.center;
                    doc.InsertParagraph("");
                }

                doc.InsertParagraph("Таблица результатов:").FontSize(14).Bold();
                doc.InsertTable(table);
                doc.InsertParagraph("");

                doc.InsertParagraph("Заключение:").FontSize(14).Bold();
                doc.InsertParagraph(
                    $"Среднее значение по шкалам: {average:F2}"
                ).FontSize(12);

                doc.Save();
            }

            return ms.ToArray();
        }

        private static (string[] names, List<int> sums, double average) Compute(SchmieschekAnswers a)
        {
            if (a.Positive.Length != 88 || a.Negative.Length != 88)
                throw new ArgumentException("Ожидается массив Positive/Negative длиной 88.");

            string[] scaleNames = new[]
            {
                "Гипертимность (Г)",
                "Дистимность (Дис)",
                "Циклотимность (Ц)",
                "Возбудимость (В)",
                "Застревание (З)",
                "Эмотивность (Эм)",
                "Экзальтированность (Эк)",
                "Тревожность (Т)",
                "Педантичность (П)",
                "Демонстративность (Дем)"
            };

            var sums = new List<int>();

            // Гипертимность (Г)
            sums.Add((a.Positive[0] + a.Positive[10] + a.Positive[22] + a.Positive[32] +
                      a.Positive[44] + a.Positive[54] + a.Positive[66] + a.Positive[77]) * 3);

            // Дистимность (Дис)
            sums.Add((a.Positive[8] + a.Positive[20] + a.Positive[42] + a.Positive[74] +
                      a.Positive[86] + a.Negative[30] + a.Negative[52] + a.Negative[64]) * 3);

            // Циклотимность (Ц)
            sums.Add((a.Positive[5] + a.Positive[17] + a.Positive[27] + a.Positive[39] +
                      a.Positive[49] + a.Positive[61] + a.Positive[72] + a.Positive[83]) * 3);

            // Возбудимость (В)
            sums.Add((a.Positive[7] + a.Positive[19] + a.Positive[29] + a.Positive[41] +
                      a.Positive[51] + a.Positive[63] + a.Positive[75] + a.Positive[85]) * 3);

            // Застревание (З)
            sums.Add((a.Positive[1] + a.Positive[14] + a.Positive[23] + a.Positive[33] + a.Positive[36] +
                      a.Positive[55] + a.Positive[67] + a.Positive[78] + a.Positive[80] +
                      a.Negative[11] + a.Negative[45] + a.Negative[58]) * 2);

            // Эмотивность (Эм)
            sums.Add((a.Positive[2] + a.Positive[12] + a.Positive[34] + a.Positive[46] +
                      a.Positive[56] + a.Positive[68] + a.Positive[79] + a.Negative[24]) * 3);

            // Экзальтированность (Эк)
            sums.Add((a.Positive[9] + a.Positive[31] + a.Positive[53] + a.Positive[76]) * 6);

            // Тревожность (Т)
            sums.Add((a.Positive[15] + a.Positive[26] + a.Positive[37] + a.Positive[48] +
                      a.Positive[59] + a.Positive[70] + a.Positive[81] + a.Negative[4]) * 3);

            // Педантичность (П)
            sums.Add((a.Positive[3] + a.Positive[13] + a.Positive[16] + a.Positive[25] +
                      a.Positive[35] + a.Positive[47] + a.Positive[57] + a.Positive[60] +
                      a.Positive[69] + a.Positive[80] + a.Negative[38]) * 2);

            // Демонстративность (Дем)
            sums.Add((a.Positive[6] + a.Positive[18] + a.Positive[21] + a.Positive[28] +
                      a.Positive[40] + a.Positive[43] + a.Positive[62] + a.Positive[65] +
                      a.Positive[73] + a.Positive[84] + a.Negative[50]) * 2);

            double average = sums.Average();

            return (scaleNames, sums, average);
        }

        public byte[] GenerateBarChartBytes(string[] labels, List<double> values)
        {
            var plt = new Plot();
            double[] data = values.ToArray();
            var bars = plt.Add.Bars(data);
            bars.Horizontal = true;

            double[] pos = Enumerable.Range(0, labels.Length).Select(i => (double)i).ToArray();
            plt.Axes.Left.SetTicks(pos, labels);

            plt.Axes.InvertY();

            plt.Axes.Bottom.Label.Text = "Баллы";

            for (int i = 0; i < data.Length; i++)
            {
                string t = data[i].ToString("F0");
                var txt = plt.Add.Text(t, data[i] + data.Max() * 0.02, i);
                txt.LabelFontSize = 9;
                txt.Alignment = ScottPlot. Alignment.MiddleLeft;
            }

            string tmp = Path.ChangeExtension(Path.GetTempFileName(), ".png");
            plt.SavePng(tmp, 700, 450);
            var bytes = File.ReadAllBytes(tmp);
            try { File.Delete(tmp); } catch { }
            return bytes;
        }
    }
}
