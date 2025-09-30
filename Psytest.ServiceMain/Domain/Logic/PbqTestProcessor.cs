using Microsoft.Extensions.Options;
using Psytest.ServiceMain.Domain.DTOs;
using Psytest.ServiceMain.Domain.Entities;
using Psytest.ServiceMain.Domain.Options;
using ScottPlot;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace Psytest.ServiceMain.Domain.Logic
{
    public class PbqTestProcessor : ITestProcessor
    {
        private readonly string _reportsDirectory;

        public PbqTestProcessor(IOptions<ReportsOptions> options)
        {
            _reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), options.Value.Directory);

            if (!Directory.Exists(_reportsDirectory))
                Directory.CreateDirectory(_reportsDirectory);
        }
        public TestResult Process(TestSession session, object answers)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (answers == null) throw new ArgumentNullException(nameof(answers));

            var pbqAnswers = (PbqAnswers)answers ??
                throw new ArgumentException("Ожидается PbqAnswers", nameof(answers)); ;
            if (pbqAnswers.Answers == null || pbqAnswers.Answers.Length != 65)
                throw new ArgumentException("Ожидается массив из 65 значений (0..4).", nameof(pbqAnswers));

            string[] scaleNames = new[]
            {
                "Избегание",
                "Зависимость",
                "Пассивность - агрессивность",
                "Обсессивность - компульсивность",
                "Антисоциальность",
                "Нарциссизм",
                "Гистрионность",
                "Шизоидность",
                "Параноидность",
                "Пограничность"
            };

            List<double> sums = new List<double>();
            List<double> results = new List<double>();

            //1 2 5 31 33 39 43 Избегание
            sums.Add(pbqAnswers.Answers[0]
                + pbqAnswers.Answers[1]
                + pbqAnswers.Answers[4]
                + pbqAnswers.Answers[30]
                + pbqAnswers.Answers[32]
                + pbqAnswers.Answers[38]
                + pbqAnswers.Answers[42]);
            results.Add((sums[0] - 10.86) / 6.46);

            //15 18 44 45 56 62 63 Зависимость
            sums.Add(pbqAnswers.Answers[14]
                + pbqAnswers.Answers[17]
                + pbqAnswers.Answers[43]
                + pbqAnswers.Answers[44]
                + pbqAnswers.Answers[55]
                + pbqAnswers.Answers[61]
                + pbqAnswers.Answers[62]);
            results.Add((sums[1] - 9.26) / 6.12);

            //4 7 20 21 41 47 51 Пассивность - агрессивность
            sums.Add(pbqAnswers.Answers[3]
                + pbqAnswers.Answers[6]
                + pbqAnswers.Answers[19]
                + pbqAnswers.Answers[20]
                + pbqAnswers.Answers[40]
                + pbqAnswers.Answers[46]
                + pbqAnswers.Answers[50]);
            results.Add((sums[2] - 8.09) / 5.97);

            //6 9 11 19 30 40 57 Обсессивность - компульсивность
            sums.Add(pbqAnswers.Answers[5]
                + pbqAnswers.Answers[8]
                + pbqAnswers.Answers[10]
                + pbqAnswers.Answers[18]
                + pbqAnswers.Answers[29]
                + pbqAnswers.Answers[39]
                + pbqAnswers.Answers[56]);
            results.Add((sums[3] - 10.56) / 7.2);

            //23 32 35 38 42 59 61 Антисоциальность
            sums.Add(pbqAnswers.Answers[22]
                + pbqAnswers.Answers[31]
                + pbqAnswers.Answers[34]
                + pbqAnswers.Answers[37]
                + pbqAnswers.Answers[41]
                + pbqAnswers.Answers[58]
                + pbqAnswers.Answers[60]);
            results.Add((sums[4] - 4.25) / 4.3);

            //10 16 26 27 46 58 60 Нарциссизм
            sums.Add(pbqAnswers.Answers[9]
                + pbqAnswers.Answers[15]
                + pbqAnswers.Answers[25]
                + pbqAnswers.Answers[26]
                + pbqAnswers.Answers[45]
                + pbqAnswers.Answers[57]
                + pbqAnswers.Answers[59]);
            results.Add((sums[5] - 3.42) / 4.23);

            //8 22 34 37 52 54 55 Гистрионность
            sums.Add(pbqAnswers.Answers[7]
                + pbqAnswers.Answers[21]
                + pbqAnswers.Answers[33]
                + pbqAnswers.Answers[36]
                + pbqAnswers.Answers[51]
                + pbqAnswers.Answers[53]
                + pbqAnswers.Answers[54]);
            results.Add((sums[6] - 6.47) / 6.09);

            //12 25 28 29 36 50 53 Шизоидность
            sums.Add(pbqAnswers.Answers[11]
                + pbqAnswers.Answers[24]
                + pbqAnswers.Answers[27]
                + pbqAnswers.Answers[28]
                + pbqAnswers.Answers[35]
                + pbqAnswers.Answers[49]
                + pbqAnswers.Answers[52]);
            results.Add((sums[7] - 8.99) / 5.6);

            //3 13 14 17 24 48 49 Параноидность
            sums.Add(pbqAnswers.Answers[2]
                + pbqAnswers.Answers[12]
                + pbqAnswers.Answers[13]
                + pbqAnswers.Answers[16]
                + pbqAnswers.Answers[23]
                + pbqAnswers.Answers[47]
                + pbqAnswers.Answers[48]);
            results.Add((sums[8] - 6.99) / 6.22);

            //31 44 45 49 56 64 65 Пограничность
            sums.Add(pbqAnswers.Answers[30]
                + pbqAnswers.Answers[43]
                + pbqAnswers.Answers[44]
                + pbqAnswers.Answers[48]
                + pbqAnswers.Answers[55]
                + pbqAnswers.Answers[63]
                + pbqAnswers.Answers[64]);
            results.Add((sums[9] - 8.07) / 6.05);

            int maxIndex = results.IndexOf(results.Max());

            // Формируем ResultText (короткая таблица)
            var lines = new List<string>();
            for (int i = 0; i < scaleNames.Length; i++)
            {
                lines.Add(string.Format("{0,-4} {1,-40} {2,6} {3,12:F2}", i + 1, scaleNames[i], (int)sums[i], results[i]));
            }
            lines.Add($"Максимальная шкала: {scaleNames[maxIndex]} (Z = {results[maxIndex]:F2}, Сумма = {(int)sums[maxIndex]})");

            string resultText = string.Join(Environment.NewLine, lines);

            // создать график и docx
            var timeTag = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var chartPath = Path.Combine(_reportsDirectory, $"pbq_chart_{session.Id}_{timeTag}.png");

            GenerateBarChart(scaleNames, results, chartPath);
            var docPath = GenerateDocxReport(
                session.Id, 
                pbqAnswers, 
                chartPath, 
                scaleNames, 
                sums, 
                results, 
                maxIndex);

            var testResult = new TestResult
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ResultText = resultText,
                ReportPath = docPath
            };

            return testResult;
        }


        private void GenerateBarChart(string[] labels, List<double> results, string path)
        {
            var plt = new Plot();

            double[] values = results.ToArray();

            // Создаем горизонтальную столбчатую диаграмму
            var barPlot = plt.Add.Bars(values);
            barPlot.Horizontal = true;
            barPlot.Color = new Color(70, 130, 180);

            // Настраиваем метки оси Y (теперь это наши категории)
            double[] positions = Enumerable.Range(0, labels.Length).Select(i => (double)i).ToArray();
            plt.Axes.Left.SetTicks(positions, labels);

            // Подпись оси X (теперь это значения)
            plt.Axes.Bottom.Label.Text = "Значение";
            plt.Axes.Bottom.Label.FontSize = 12;
            plt.Axes.Bottom.Label.Bold = true;

            // Добавляем подписи значений с ограничением до двух знаков
            for (int i = 0; i < values.Length; i++)
            {
                string formattedValue = values[i].ToString("F2"); // Ограничиваем двумя нулями
                var text = plt.Add.Text(formattedValue, values[i] + values.Max() * 0.02, i);
                text.LabelFontColor = Colors.Black;
                text.LabelFontSize = 9;
                text.Alignment = ScottPlot.Alignment.MiddleLeft;
            }

            plt.SavePng(path, 700, 450);
        }


        private string GenerateDocxReport
            (Guid sessionId, 
            PbqAnswers answers, 
            string chartPath, 
            string[] scaleNames, 
            List<double> sums, 
            List<double> results, 
            int maxIndex)
        {
            var fileName = $"pbq_report_{sessionId}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
            var filePath = Path.Combine(_reportsDirectory, fileName);

            using (var doc = DocX.Create(filePath))
            {
                // Заголовок
                var title = doc.InsertParagraph("Результаты теста PBQ");
                title.FontSize(16).Bold().Alignment = Xceed.Document.NET.Alignment.center;
                doc.InsertParagraph("");

                // Информация о сессии
                var sessionInfo = doc.InsertParagraph($"Дата тестирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
                sessionInfo.FontSize(12);
                doc.InsertParagraph("");

                // Таблица с результатами
                var table = doc.AddTable(scaleNames.Length + 1, 4);
                table.Alignment = Xceed.Document.NET.Alignment.center;
                table.Design = TableDesign.ColorfulList;

                // Заголовки
                table.Rows[0].Cells[0].Paragraphs[0].Append("#").Bold();
                table.Rows[0].Cells[1].Paragraphs[0].Append("Шкала").Bold();
                table.Rows[0].Cells[2].Paragraphs[0].Append("Сумма").Bold();
                table.Rows[0].Cells[3].Paragraphs[0].Append("Z-значение").Bold();

                for (int i = 0; i < scaleNames.Length; i++)
                {
                    table.Rows[i + 1].Cells[0].Paragraphs[0].Append((i + 1).ToString());
                    table.Rows[i + 1].Cells[1].Paragraphs[0].Append(scaleNames[i]);
                    table.Rows[i + 1].Cells[2].Paragraphs[0].Append(((int)sums[i]).ToString());
                    table.Rows[i + 1].Cells[3].Paragraphs[0].Append(results[i].ToString("F2"));
                }

                doc.InsertParagraph("Таблица результатов:").FontSize(14).Bold();
                doc.InsertTable(table);
                doc.InsertParagraph("");

                // Вставляем картинку (график)
                if (File.Exists(chartPath))
                {
                    var img = doc.AddImage(chartPath);
                    var picture = img.CreatePicture(500, 350); // размеры картинки
                    var p = doc.InsertParagraph();
                    p.AppendPicture(picture);
                    p.Alignment = Xceed.Document.NET.Alignment.center;
                    doc.InsertParagraph("");
                }

                // Итоговое заключение
                doc.InsertParagraph("Заключение:").FontSize(14).Bold();
                doc.InsertParagraph($"Максимальная шкала: {scaleNames[maxIndex]} (Z = {results[maxIndex]:F2}, Сумма = {(int)sums[maxIndex]})")
                    .FontSize(12);

                doc.Save();
            }

            return filePath;
        }
    }

}
