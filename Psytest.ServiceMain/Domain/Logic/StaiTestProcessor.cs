using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Domain.Logic.Interfaces;
using ScottPlot;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace Psytest.ServiceMain.Domain.Logic
{
    public class StaiTestProcessor : ITestProcessor, IChartGenerator
    {
        // ====== Ключ пересчёта ответов в баллы (по позиции вопроса) ======
        private static readonly Dictionary<int, int[]> Points = new()
        {
            // Ситуативная тревожность (СТ) — 0..19
            { 0, new[] { 4, 3, 2, 1 } }, { 1, new[] { 4, 3, 2, 1 } },
            { 2, new[] { 1, 2, 3, 4 } }, { 3, new[] { 1, 2, 3, 4 } },
            { 4, new[] { 4, 3, 2, 1 } }, { 5, new[] { 1, 2, 3, 4 } },
            { 6, new[] { 1, 2, 3, 4 } }, { 7, new[] { 4, 3, 2, 1 } },
            { 8, new[] { 1, 2, 3, 4 } }, { 9, new[] { 4, 3, 2, 1 } },
            {10, new[] { 4, 3, 2, 1 } }, {11, new[] { 1, 2, 3, 4 } },
            {12, new[] { 1, 2, 3, 4 } }, {13, new[] { 1, 2, 3, 4 } },
            {14, new[] { 4, 3, 2, 1 } }, {15, new[] { 4, 3, 2, 1 } },
            {16, new[] { 1, 2, 3, 4 } }, {17, new[] { 1, 2, 3, 4 } },
            {18, new[] { 4, 3, 2, 1 } }, {19, new[] { 4, 3, 2, 1 } },
            // Личностная тревожность (ЛТ) — 20..39
            {20, new[] { 4, 3, 2, 1 } }, {21, new[] { 1, 2, 3, 4 } },
            {22, new[] { 1, 2, 3, 4 } }, {23, new[] { 1, 2, 3, 4 } },
            {24, new[] { 1, 2, 3, 4 } }, {25, new[] { 4, 3, 2, 1 } },
            {26, new[] { 4, 3, 2, 1 } }, {27, new[] { 1, 2, 3, 4 } },
            {28, new[] { 1, 2, 3, 4 } }, {29, new[] { 4, 3, 2, 1 } },
            {30, new[] { 1, 2, 3, 4 } }, {31, new[] { 1, 2, 3, 4 } },
            {32, new[] { 1, 2, 3, 4 } }, {33, new[] { 1, 2, 3, 4 } },
            {34, new[] { 1, 2, 3, 4 } }, {35, new[] { 4, 3, 2, 1 } },
            {36, new[] { 1, 2, 3, 4 } }, {37, new[] { 1, 2, 3, 4 } },
            {38, new[] { 4, 3, 2, 1 } }, {39, new[] { 1, 2, 3, 4 } },
        };

        // ====== Нормы: раздельно для СТ и ЛТ и по возрастным группам ======
        private enum Scale { ST, LT }
        private enum AgeBand { Y13_15, Y16_19, Adult20Plus }

        private record RawBand(int Min, int Max, int Sten, string Level);

        private static readonly Dictionary<(Scale, AgeBand), List<RawBand>> Norms = new()
{
    // ===================== ЛТ (личностная) =====================
    { (Scale.LT, AgeBand.Y13_15), new()
    {
        new( 0, 15, 1, "Низкий"),
        new(16, 21, 2, "Низкий"),
        new(22, 27, 3, "Средний с тенденцией к низкому"),
        new(28, 33, 4, "Средний с тенденцией к низкому"),
        new(34, 39, 5, "Средний с тенденцией к высокому"),
        new(40, 45, 6, "Средний с тенденцией к высокому"),
        new(46, 51, 7, "Высокий"),
        new(52, 57, 8, "Высокий"),
        new(58, 63, 9, "Очень высокий"),
        new(64, 80,10, "Очень высокий"),
    }},
    { (Scale.LT, AgeBand.Y16_19), new()
    {
        new( 0, 12, 1, "Низкий"),
        new(13, 18, 2, "Низкий"),
        new(19, 23, 3, "Средний с тенденцией к низкому"),
        new(24, 29, 4, "Средний с тенденцией к низкому"),
        new(30, 34, 5, "Средний с тенденцией к высокому"),
        new(35, 40, 6, "Средний с тенденцией к высокому"),
        new(41, 45, 7, "Высокий"),
        new(46, 51, 8, "Высокий"),
        new(52, 56, 9, "Очень высокий"),
        new(57, 80,10, "Очень высокий"),
    }},
    { (Scale.LT, AgeBand.Adult20Plus), new()
    {
        new( 0, 22, 1, "Низкий"),
        new(23, 26, 2, "Низкий"),
        new(27, 30, 3, "Средний с тенденцией к низкому"),
        new(31, 34, 4, "Средний с тенденцией к низкому"),
        new(35, 39, 5, "Средний с тенденцией к высокому"),
        new(40, 43, 6, "Средний с тенденцией к высокому"),
        new(43, 46, 7, "Высокий"),
        new(47, 50, 8, "Высокий"),
        new(51, 54, 9, "Очень высокий"),
        new(55, 80,10, "Очень высокий"),
    }},

    // ===================== СТ (ситуативная) =====================
    { (Scale.ST, AgeBand.Y13_15), new()
    {
        new( 0, 30, 1, "Низкий"),
        new(31, 33, 2, "Низкий"),
        new(34, 36, 3, "Средний с тенденцией к низкому"),
        new(37, 39, 4, "Средний с тенденцией к низкому"),
        new(40, 42, 5, "Средний с тенденцией к высокому"),
        new(43, 45, 6, "Средний с тенденцией к высокому"),
        new(46, 48, 7, "Высокий"),
        new(49, 51, 8, "Высокий"),
        new(52, 54, 9, "Очень высокий"),
        new(55, 80,10, "Очень высокий"),
    }},
    { (Scale.ST, AgeBand.Y16_19), new()
    {
        new( 0, 20, 1, "Низкий"),
        new(21, 25, 2, "Низкий"),
        new(26, 30, 3, "Средний с тенденцией к низкому"),
        new(31, 35, 4, "Средний с тенденцией к низкому"),
        new(36, 40, 5, "Средний с тенденцией к высокому"),
        new(41, 45, 6, "Средний с тенденцией к высокому"),
        new(46, 50, 7, "Высокий"),
        new(51, 55, 8, "Высокий"),
        new(56, 60, 9, "Очень высокий"),
        new(61, 80,10, "Очень высокий"),
    }},
    { (Scale.ST, AgeBand.Adult20Plus), new()
    {
        new( 0, 25, 1, "Низкий"),
        new(26, 30, 2, "Низкий"),
        new(31, 34, 3, "Средний с тенденцией к низкому"),
        new(35, 39, 4, "Средний с тенденцией к низкому"),
        new(40, 43, 5, "Средний с тенденцией к высокому"),
        new(44, 48, 6, "Средний с тенденцией к высокому"),
        new(49, 52, 7, "Высокий"),
        new(53, 57, 8, "Высокий"),
        new(58, 61, 9, "Очень высокий"),
        new(62, 80,10, "Очень высокий"),
    }},
};


        public TestResult Process(TestSession session, object answers)
        {
            var stai = (StaiAnswers)answers;
            var (names, sums, stens, interps) = ComputeWithNorms(stai);

            // --- Текст результата ---
            var lines = new List<string>();
            for (int i = 0; i < names.Length; i++)
                lines.Add($"{i + 1,2}. {names[i],-32} {sums[i],2} — {interps[i]}");
            string resultText = string.Join(Environment.NewLine, lines);

            // --- Два графика ---
            var rawChart = GenerateBarChartBytes(names, sums.Select(x => (double)x).ToList());
            var stenChart = GenerateBarChartBytes(names, stens.Select(x => (double)x).ToList());

            // --- Генерация отчёта (интерфейсный метод) ---
            var reportBytes = GenerateDocxReport(session.Id, answers, resultText);

            // --- Формирование итогового результата ---
            return new TestResult
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ResultText = resultText,
                ReportBytes = reportBytes,
                Images = new List<byte[]> { rawChart, stenChart }
            };
        }

        // ====== DOCX отчёт с таблицей и двумя графиками ======
        public byte[] GenerateDocxReport(Guid sessionId, object answers, string resultText)
        {
            var stai = (StaiAnswers)answers;
            var (names, sums, stens, interps) = ComputeWithNorms(stai);

            // Генерация двух графиков
            var rawChart = GenerateBarChartBytes(names, sums.Select(s => (double)s).ToList());
            var stenChart = GenerateBarChartBytes(names, stens.Select(s => (double)s).ToList());

            using var ms = new MemoryStream();
            using (var doc = DocX.Create(ms))
            {
                // --- Заголовок и метаданные ---
                var title = doc.InsertParagraph("Результаты теста Спилбергера–Ханина (STAI)");
                title.FontSize(16).Bold().Alignment = Xceed.Document.NET.Alignment.center;

                doc.InsertParagraph($"Дата тестирования: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(12);
                doc.InsertParagraph($"Возраст: {stai.Age}").FontSize(12);
                doc.InsertParagraph("");

                // --- Таблица результатов ---
                var table = doc.AddTable(names.Length + 1, 5);
                table.Alignment = Xceed.Document.NET.Alignment.center;
                table.Design = TableDesign.ColorfulList;

                table.Rows[0].Cells[0].Paragraphs[0].Append("#").Bold();
                table.Rows[0].Cells[1].Paragraphs[0].Append("Шкала").Bold();
                table.Rows[0].Cells[2].Paragraphs[0].Append("Сырые баллы").Bold();
                table.Rows[0].Cells[3].Paragraphs[0].Append("Стен").Bold();
                table.Rows[0].Cells[4].Paragraphs[0].Append("Интерпретация").Bold();

                for (int i = 0; i < names.Length; i++)
                {
                    table.Rows[i + 1].Cells[0].Paragraphs[0].Append((i + 1).ToString());
                    table.Rows[i + 1].Cells[1].Paragraphs[0].Append(names[i]);
                    table.Rows[i + 1].Cells[2].Paragraphs[0].Append(sums[i].ToString());
                    table.Rows[i + 1].Cells[3].Paragraphs[0].Append(stens[i].ToString());
                    table.Rows[i + 1].Cells[4].Paragraphs[0].Append(interps[i]);
                }

                doc.InsertParagraph("Таблица результатов:").FontSize(14).Bold();
                doc.InsertTable(table);
                doc.InsertParagraph("");

                // --- Вставляем графики ---
                if (rawChart?.Length > 0)
                {
                    doc.InsertParagraph("Сырые баллы:").FontSize(13).Bold().Alignment = Xceed.Document.NET.Alignment.center;
                    using var imgStream = new MemoryStream(rawChart);
                    var img = doc.AddImage(imgStream);
                    var pic = img.CreatePicture(400, 150);
                    var p = doc.InsertParagraph();
                    p.AppendPicture(pic);
                    p.Alignment = Xceed.Document.NET.Alignment.center;
                }

                if (stenChart?.Length > 0)
                {
                    doc.InsertParagraph("Стен-баллы:").FontSize(13).Bold().Alignment = Xceed.Document.NET.Alignment.center;
                    using var imgStream = new MemoryStream(stenChart);
                    var img = doc.AddImage(imgStream);
                    var pic = img.CreatePicture(400, 150);
                    var p = doc.InsertParagraph();
                    p.AppendPicture(pic);
                    p.Alignment = Xceed.Document.NET.Alignment.center;
                }

                doc.Save();
            }

            return ms.ToArray();
        }

        // ====== Пересчёт с нормами ======
        private static (string[] names, List<int> sums, List<int> stens, List<string> interps) ComputeWithNorms(StaiAnswers a)
        {
            if (a.Answers.Length != 40)
                throw new ArgumentException("Ожидается 40 ответов.");

            int st = 0, lt = 0;
            for (int i = 0; i < 40; i++)
            {
                int ans = a.Answers[i];
                if (ans < 1 || ans > 4)
                    throw new ArgumentException($"Недопустимый ответ {ans} в позиции {i + 1}.");

                int score = Points[i][ans - 1];
                if (i < 20) st += score; else lt += score;
            }

            var names = new[] { "Ситуативная тревожность (СТ)", "Личностная тревожность (ЛТ)" };
            var sums = new List<int> { st, lt };
            var stens = new List<int>();
            var interps = new List<string>();

            var band = PickAgeBand(a.Age);
            foreach (var (scale, raw) in new[] { (Scale.ST, st), (Scale.LT, lt) })
            {
                var table = Norms[(scale, band)];
                var found = table.FirstOrDefault(r => raw >= r.Min && raw <= r.Max)
                            ?? table.Last();

                interps.Add($"{found.Level} (сырые={raw}, стен={found.Sten})");
                stens.Add(found.Sten);
            }

            return (names, sums, stens, interps);
        }

        private static AgeBand PickAgeBand(int age)
        {
            if (age <= 15) return AgeBand.Y13_15;
            if (age <= 19) return AgeBand.Y16_19;
            return AgeBand.Adult20Plus;
        }

        // ====== График ======
        public byte[] GenerateBarChartBytes(string[] labels, List<double> values)
        {
            var plt = new Plot();
            var bars = plt.Add.Bars(values.ToArray());
            bars.Horizontal = true;

            double[] pos = Enumerable.Range(0, labels.Length).Select(i => (double)i).ToArray();
            plt.Axes.Left.SetTicks(pos, labels);
            plt.Axes.InvertY();
            plt.Axes.Bottom.Label.Text = "Значения";

            double max = values.Max();
            for (int i = 0; i < values.Count; i++)
            {
                var txt = plt.Add.Text(values[i].ToString("F0"), values[i] + max * 0.02, i);
                txt.LabelFontSize = 9;
                txt.Alignment = ScottPlot.Alignment.MiddleLeft;
            }

            string tmp = Path.ChangeExtension(Path.GetTempFileName(), ".png");
            plt.SavePng(tmp, 700, 200);
            var bytes = File.ReadAllBytes(tmp);
            try { File.Delete(tmp); } catch { }

            return bytes;
        }


    }
}
