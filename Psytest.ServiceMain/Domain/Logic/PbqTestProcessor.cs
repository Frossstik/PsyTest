using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Domain.Logic.Interfaces;
using Xceed.Document.NET;
using Xceed.Words.NET;
using ScottPlot;

namespace Psytest.ServiceMain.Domain.Logic
{
    public class PbqTestProcessor : ITestProcessor, IChartGenerator
    {
        public TestResult Process(TestSession session, object answers)
        {
            var pbq = (PbqAnswers)answers;

            var (scaleNames, sums, z, maxIdx) = Compute(pbq);

            var lines = new List<string>();
            for (int i = 0; i < scaleNames.Length; i++)
                lines.Add(string.Format("{0,-4} {1,-40} {2,6} {3,12:F2}",
                    i + 1, scaleNames[i] + ":", (int)sums[i], z[i]));
            lines.Add($"Максимальная шкала: {scaleNames[maxIdx]} (Z = {z[maxIdx]:F2}, Сумма = {(int)sums[maxIdx]})");

            string resultText = string.Join(Environment.NewLine, lines);

            var chartBytes = GenerateBarChartBytes(scaleNames, z);

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
            var pbq = (PbqAnswers)answers;
            var (scaleNames, sums, z, maxIdx) = Compute(pbq);
            var chartBytes = GenerateBarChartBytes(scaleNames, z);

            using var ms = new MemoryStream();
            using (var doc = DocX.Create(ms))
            {
                var title = doc.InsertParagraph("Результаты теста PBQ");
                title.FontSize(16).Bold().Alignment = Xceed.Document.NET.Alignment.center;
                doc.InsertParagraph("");

                doc.InsertParagraph($"Дата тестирования: {DateTime.Now:dd.MM.yyyy HH:mm}")
                   .FontSize(12);
                doc.InsertParagraph("");

                // Таблица
                var table = doc.AddTable(scaleNames.Length + 1, 4);
                table.Alignment = Xceed.Document.NET.Alignment.center;
                table.Design = TableDesign.ColorfulList;

                table.Rows[0].Cells[0].Paragraphs[0].Append("#").Bold();
                table.Rows[0].Cells[1].Paragraphs[0].Append("Шкала").Bold();
                table.Rows[0].Cells[2].Paragraphs[0].Append("Сумма").Bold();
                table.Rows[0].Cells[3].Paragraphs[0].Append("Z-значение").Bold();

                for (int i = 0; i < scaleNames.Length; i++)
                {
                    table.Rows[i + 1].Cells[0].Paragraphs[0].Append((i + 1).ToString());
                    table.Rows[i + 1].Cells[1].Paragraphs[0].Append(scaleNames[i]);
                    table.Rows[i + 1].Cells[2].Paragraphs[0].Append(((int)sums[i]).ToString());
                    table.Rows[i + 1].Cells[3].Paragraphs[0].Append(z[i].ToString("F2"));
                }

                // Картинка
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


                // Заключение
                doc.InsertParagraph("Заключение:").FontSize(14).Bold();
                doc.InsertParagraph(
                    $"Максимальная шкала: {scaleNames[maxIdx]} (Z = {z[maxIdx]:F2}, Сумма = {(int)sums[maxIdx]})"
                ).FontSize(12);

                doc.Save();
            }

            return ms.ToArray();
        }

        private static (string[] names, List<double> sums, List<double> z, int maxIdx) Compute(PbqAnswers pbq)
        {
            if (pbq == null || pbq.Answers == null || pbq.Answers.Length != 65)
                throw new ArgumentException("Ожидается массив из 65 значений (0..4).");

            string[] scaleNames = new[]
            {
                "Избегание","Зависимость","Пассивность - агрессивность","Обсессивность - компульсивность",
                "Антисоциальность","Нарциссизм","Гистрионность","Шизоидность","Параноидность","Пограничность"
            };

            var sums = new List<double>();
            var z = new List<double>();

            // индексы те же, что у тебя (0-based)
            sums.Add(pbq.Answers[0] + pbq.Answers[1] + pbq.Answers[4] + pbq.Answers[30] + pbq.Answers[32] + pbq.Answers[38] + pbq.Answers[42]);
            z.Add((sums[0] - 10.86) / 6.46);

            sums.Add(pbq.Answers[14] + pbq.Answers[17] + pbq.Answers[43] + pbq.Answers[44] + pbq.Answers[55] + pbq.Answers[61] + pbq.Answers[62]);
            z.Add((sums[1] - 9.26) / 6.12);

            sums.Add(pbq.Answers[3] + pbq.Answers[6] + pbq.Answers[19] + pbq.Answers[20] + pbq.Answers[40] + pbq.Answers[46] + pbq.Answers[50]);
            z.Add((sums[2] - 8.09) / 5.97);

            sums.Add(pbq.Answers[5] + pbq.Answers[8] + pbq.Answers[10] + pbq.Answers[18] + pbq.Answers[29] + pbq.Answers[39] + pbq.Answers[56]);
            z.Add((sums[3] - 10.56) / 7.2);

            sums.Add(pbq.Answers[22] + pbq.Answers[31] + pbq.Answers[34] + pbq.Answers[37] + pbq.Answers[41] + pbq.Answers[58] + pbq.Answers[60]);
            z.Add((sums[4] - 4.25) / 4.3);

            sums.Add(pbq.Answers[9] + pbq.Answers[15] + pbq.Answers[25] + pbq.Answers[26] + pbq.Answers[45] + pbq.Answers[57] + pbq.Answers[59]);
            z.Add((sums[5] - 3.42) / 4.23);

            sums.Add(pbq.Answers[7] + pbq.Answers[21] + pbq.Answers[33] + pbq.Answers[36] + pbq.Answers[51] + pbq.Answers[53] + pbq.Answers[54]);
            z.Add((sums[6] - 6.47) / 6.09);

            sums.Add(pbq.Answers[11] + pbq.Answers[24] + pbq.Answers[27] + pbq.Answers[28] + pbq.Answers[35] + pbq.Answers[49] + pbq.Answers[52]);
            z.Add((sums[7] - 8.99) / 5.6);

            sums.Add(pbq.Answers[2] + pbq.Answers[12] + pbq.Answers[13] + pbq.Answers[16] + pbq.Answers[23] + pbq.Answers[47] + pbq.Answers[48]);
            z.Add((sums[8] - 6.99) / 6.22);

            sums.Add(pbq.Answers[30] + pbq.Answers[43] + pbq.Answers[44] + pbq.Answers[48] + pbq.Answers[55] + pbq.Answers[63] + pbq.Answers[64]);
            z.Add((sums[9] - 8.07) / 6.05);

            int maxIdx = z.IndexOf(z.Max());
            return (scaleNames, sums, z, maxIdx);
        }

        public byte[] GenerateBarChartBytes(string[] labels, List<double> values)
        {
            var plt = new Plot();
            double[] data = values.ToArray();

            var bars = plt.Add.Bars(data);
            bars.Horizontal = true;

            double[] pos = Enumerable.Range(0, labels.Length).Select(i => (double)i).ToArray();
            plt.Axes.Left.SetTicks(pos, labels);

            plt.Axes.Bottom.Label.Text = "Значение";
            for (int i = 0; i < data.Length; i++)
            {
                string t = data[i].ToString("F2");
                var txt = plt.Add.Text(t, data[i] + data.Max() * 0.02, i);
                txt.LabelFontColor = Colors.Black;
                txt.LabelFontSize = 9;
                txt.Alignment = ScottPlot.Alignment.MiddleLeft;
            }

            // Сохраняем во временный PNG и читаем байты
            string tmp = Path.ChangeExtension(Path.GetTempFileName(), ".png");
            plt.SavePng(tmp, 700, 450);
            var bytes = File.ReadAllBytes(tmp);
            try { File.Delete(tmp); } catch { /* no-op */ }
            return bytes;
        }
    }
}
