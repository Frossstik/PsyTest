using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Domain.Logic.Interfaces;
using Xceed.Document.NET;
using Xceed.Words.NET;
using ScottPlot;

namespace Psytest.ServiceMain.Domain.Logic
{
    public class BaiTestProcessor : ITestProcessor, IChartGenerator
    {
        public TestResult Process(TestSession session, object answers)
        {
            var bai = (BaiAnswers)answers;

            var (questions, values, total, level) = Compute(bai);

            var resultText = $"Суммарный балл: {total}\n" +
                             $"Уровень тревожности: {level}";

            var chartBytes = GenerateBarChartBytes(questions, values);
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

        private static (string[] questions, List<double> values, int total, string level) Compute(BaiAnswers bai)
        {
            if (bai == null || bai.Answers == null || bai.Answers.Length != 21)
                throw new ArgumentException("Ожидается массив из 21 значения (0–3).");

            string[] questions =
            {
                "Ощущение онемения или покалывания в теле",
                "Ощущение жары",
                "Дрожь в ногах",
                "Неспособность расслабиться",
                "Страх, что произойдет самое плохое",
                "Головокружение или ощущение легкости в голове",
                "Ускоренное сердцебиение",
                "Неустойчивость",
                "Ощущение ужаса",
                "Нервозность",
                "Дрожь в руках",
                "Ощущение удушья",
                "Шаткость походки",
                "Страх утраты контроля",
                "Затрудненность дыхания",
                "Страх смерти",
                "Испуг",
                "Желудочно-кишечные расстройства",
                "Обмороки",
                "Приливы крови к лицу",
                "Потоотделение (не вызванное жарой)"
            };

            var values = bai.Answers.Select(a => (double)a).ToList();
            int total = (int)values.Sum();

            string level = total switch
            {
                <= 7 => "Минимальная тревожность",
                <= 15 => "Легкая тревожность",
                <= 25 => "Умеренная тревожность",
                _ => "Высокая тревожность"
            };

            return (questions, values, total, level);
        }

        public byte[] GenerateDocxReport(Guid sessionId, object answers, string resultText)
        {
            var bai = (BaiAnswers)answers;
            var (questions, values, total, level) = Compute(bai);
            var chartBytes = GenerateBarChartBytes(questions, values);

            using var ms = new MemoryStream();
            using (var doc = DocX.Create(ms))
            {
                var title = doc.InsertParagraph("Результаты теста BAI (Шкала тревожности Бека)");
                title.FontSize(16).Bold().Alignment = Xceed.Document.NET.Alignment.center;
                doc.InsertParagraph("");

                doc.InsertParagraph($"Дата тестирования: {DateTime.Now:dd.MM.yyyy HH:mm}")
                   .FontSize(12);
                doc.InsertParagraph("");

                // Таблица
                var table = doc.AddTable(questions.Length + 1, 3);
                table.Alignment = Xceed.Document.NET.Alignment.center;
                table.Design = TableDesign.ColorfulList;

                table.Rows[0].Cells[0].Paragraphs[0].Append("#").Bold();
                table.Rows[0].Cells[1].Paragraphs[0].Append("Показатель").Bold();
                table.Rows[0].Cells[2].Paragraphs[0].Append("Баллы").Bold();

                for (int i = 0; i < questions.Length; i++)
                {
                    table.Rows[i + 1].Cells[0].Paragraphs[0].Append((i + 1).ToString());
                    table.Rows[i + 1].Cells[1].Paragraphs[0].Append(questions[i]);
                    table.Rows[i + 1].Cells[2].Paragraphs[0].Append(values[i].ToString("F0"));
                }

                doc.InsertParagraph("Таблица ответов:").FontSize(14).Bold();
                doc.InsertTable(table);
                doc.InsertParagraph("");

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

                // Итоги
                doc.InsertParagraph("Результаты:").FontSize(14).Bold();
                doc.InsertParagraph($"Суммарный балл: {total}").FontSize(12);
                doc.InsertParagraph($"Уровень тревожности: {level}").FontSize(12);

                doc.Save();
            }

            return ms.ToArray();
        }

        public byte[] GenerateBarChartBytes(string[] labels, List<double> values)
        {
            double total = values.Sum();
            double max = 63;
            double normalized = Math.Min(total / max, 1.0);

            var plt = new Plot();

            // Цвета через HEX
            var lightGreen = Color.FromHex("#90EE90");
            var yellowGreen = Color.FromHex("#ADFF2F");
            var gold = Color.FromHex("#FFD700");
            var orangeRed = Color.FromHex("#FF4500");
            var black = Color.FromHex("#000000");
            var white = Color.FromHex("#FFFFFF");

            // Цветные зоны
            (double start, double end, Color color)[] ranges =
            {
                (0, 7, lightGreen),
                (7, 15, yellowGreen),
                (15, 25, gold),
                (25, 63, orangeRed)
            };

            foreach (var (start, end, color) in ranges)
            {
                double startAngle = Math.PI * (1 - start / max);
                double endAngle = Math.PI * (1 - end / max);
                int segments = 40;
                double[] xs = new double[segments + 1];
                double[] ys = new double[segments + 1];

                for (int i = 0; i <= segments; i++)
                {
                    double t = startAngle + (endAngle - startAngle) * i / segments;
                    xs[i] = Math.Cos(t);
                    ys[i] = Math.Sin(t);
                }

                var path = plt.Add.Scatter(xs, ys);
                path.LineWidth = 15;
                path.Color = color;
            }

            // Стрелка
            double arrowAngle = Math.PI * (1 - normalized);
            double x2 = 0.8 * Math.Cos(arrowAngle);
            double y2 = 0.8 * Math.Sin(arrowAngle);

            var arrow = plt.Add.Arrow(0, 0, x2, y2);
            arrow.ArrowLineWidth = 5;
            arrow.ArrowLineColor = black;
            arrow.ArrowFillColor = black;

            // Подписи
            var text1 = plt.Add.Text($"{total:F0} баллов", 0, -0.25);
            text1.LabelFontSize = 16;
            text1.LabelFontColor = black;

            var text2 = plt.Add.Text("BAI уровень тревожности", 0, -0.45);
            text2.LabelFontSize = 12;
            text2.LabelFontColor = black;

            // Настройки отображения
            plt.HideGrid();
            plt.Axes.SetLimits(-1.2, 1.2, -0.6, 1.2);
            plt.FigureBackground.Color = white;
            plt.DataBackground.Color = white;


            string tmp = Path.ChangeExtension(Path.GetTempFileName(), ".png");
            plt.SavePng(tmp, 600, 400);
            var bytes = File.ReadAllBytes(tmp);
            try { File.Delete(tmp); } catch { }

            return bytes;
        }



    }
}
