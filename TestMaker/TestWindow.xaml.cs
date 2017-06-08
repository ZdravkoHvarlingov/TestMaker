using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TestMaker.Model;
using TestMaker.Data;
using System.IO;
using System.Timers;
using System.Windows.Threading;
using TestMaker.Properties;

namespace TestMaker
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        private MainWindow mainWindow;
        private int currentQuestion = 0;
        private List<Question> questions;
        private List<Question> specialQuestions;
        private TimeSpan testTime;
        private DispatcherTimer timeLeftTimer;
        private double receivedPoints;
        private bool isFinished;
        private const int questionFields = 8;

        public TestWindow(MainWindow mainWindow)
        {           
            this.mainWindow = mainWindow;            
            
            testTime = new TimeSpan(0, 20, 0);

            timeLeftTimer = new DispatcherTimer();
            timeLeftTimer.Tick += OnTimerTick;
            timeLeftTimer.Interval = TimeSpan.FromSeconds(1);      

            InitializeComponent();

            timeLeftTimer.Start();           

            FillDataBaseIfNeeded();

            using (var db = new TestMakerContext())
            {
                var rnd = new Random();

                var dbQuestions = new List<Question>();
                var dbSpecialQuestions = new List<Question>();

                for (int i = 0; i < mainWindow.QuestionFields.Count; i++)
                {
                    var field=mainWindow.QuestionFields[i];
                    TextBlockTestHeader.Text += " "+ConvertFromEnumToBulgarian(field) + ",";

                    var exactFieldQuestions = db.Questions.
                        Where(q => q.QuestionField ==field && q.QuestionType==QuestionType.ChooseAbleAnswer )
                        .ToList();
                    dbQuestions.AddRange(exactFieldQuestions);
                }
                TextBlockTestHeader.Text= TextBlockTestHeader.Text.Remove(TextBlockTestHeader.Text.Count() - 1,1);

                for (int i = 0; i < mainWindow.QuestionFields.Count; i++)
                {
                    var field = mainWindow.QuestionFields[i];
                    var exactFieldQuestions = db.Questions.
                        Where(q => q.QuestionField == field && q.QuestionType == QuestionType.OpenAnswer)
                        .ToList();
                    dbSpecialQuestions.AddRange(exactFieldQuestions);
                }

                questions = dbQuestions.OrderBy(q => rnd.Next()).Take(9).ToList();
                specialQuestions = dbSpecialQuestions.OrderBy(q => rnd.Next()).Take(1).ToList();

                foreach (var grid in this.GridsStackPanel.Children.OfType<Grid>())
                {
                    grid.Children.OfType<TextBlock>().First().Text = grid.Children.OfType<TextBlock>().First().Text+questions[currentQuestion].QuestionText;                  

                    var questionAnswers = new List<string>();

                    questionAnswers.Add(questions[currentQuestion].QuestionAnswerA);
                    questionAnswers.Add(questions[currentQuestion].QuestionAnswerB);
                    questionAnswers.Add(questions[currentQuestion].QuestionAnswerC);
                    questionAnswers.Add(questions[currentQuestion].QuestionAnswerD);

                    currentQuestion++;

                    foreach (var radioButton in grid.Children.OfType<RadioButton>())
                    {
                        var txtBlock = new TextBlock();
                        txtBlock.TextWrapping = TextWrapping.Wrap;

                        var answer = questionAnswers.OrderBy(q => rnd.Next()).Take(1).ToList();

                        txtBlock.Text = radioButton.Content + " " + answer.First();
                        radioButton.Content = txtBlock;
                        radioButton.FontFamily = new System.Windows.Media.FontFamily("Segoe Print");
                        radioButton.FontSize = 14;
                        
                        questionAnswers.Remove(answer.First());
                    }
                   
                    if (currentQuestion == 9)
                    {
                        break;
                    }
                }

                this.GridsStackPanel.Children.OfType<Grid>().Last().Children.OfType<TextBlock>().Last().Text +=                    
                    specialQuestions.Last().QuestionText;
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            testTime -= new TimeSpan(0, 0, 1);

            if (testTime.Minutes==0 && testTime.Seconds==0)
            {
                MessageBox.Show("Времето Ви изтече. Тестът ще бъде приключен.");
                FinishTest();
            }
            
            labelTimeLeft.Content = "Оставащо време: " + testTime.Minutes + ":" + testTime.Seconds;
        }  
  
        public void FillDataBaseIfNeeded()
        {          
            var lines = new List<string>();            

            using (var reader = new StringReader(Properties.Resources.TestMakerQuestions))
            {
                string line = reader.ReadLine();

                lines.Add(line);
                while (line != null)
                {
                    line = reader.ReadLine();

                    if (line != null)
                    {
                        lines.Add(line);
                    }
                }
            }

            using (var db= new TestMakerContext())
            {               
                if (lines.Count!=db.Questions.Count())
                {                    
                    db.Database.ExecuteSqlCommand("DELETE FROM Questions");                    

                    foreach (var line in lines)
                    {
                        var splittedLine = line.Split(';');
                        
                        if (splittedLine.Count() == questionFields)
                        {
                            db.Questions.Add(new Question()
                            {
                                QuestionText = splittedLine[0],
                                QuestionAnswerA = splittedLine[1],
                                QuestionAnswerB = splittedLine[2],
                                QuestionAnswerC = splittedLine[3],
                                QuestionAnswerD = splittedLine[4],
                                QuestionTrueAnswer = splittedLine[5],
                                QuestionType = (QuestionType)int.Parse(splittedLine[6]),
                                QuestionField = (QuestionField)int.Parse(splittedLine[7])
                            });
                        }
                                           
                    }

                    db.SaveChanges();
                    Settings.Default.Reset();
                    Settings.Default.Save();
                }
            }          
            

        }

        public string ConvertFromEnumToBulgarian(QuestionField questionField)
        {
            switch (questionField)
            {
                case QuestionField.Biology:
                    return "Биология";                    
                case QuestionField.Geography:
                    return "География";
                case QuestionField.English:
                    return "Английски език";
                case QuestionField.History:
                    return "История";
                case QuestionField.IT:
                    return "Информационни технологии";
                case QuestionField.Philosophy:
                    return "Философия";
                case QuestionField.Bulgarian:
                    return "Български език и литература";
                default:
                    return "Default reached, program is broken!";
            }
        }

        private void OnButtonFinishTestClick(object sender, RoutedEventArgs e)
        {
            FinishTest();
        }

        private void FinishTest()
        {     
            int currentPanel = 0;

            foreach (var grid in this.GridsStackPanel.Children.OfType<Grid>())
            {
                timeLeftTimer.Stop();

                bool trueAnswer = false;

                foreach (var radioButton in grid.Children.OfType<RadioButton>())
                {
                    radioButton.Foreground = Brushes.Red;
                    var currentTxtBlock = (TextBlock)radioButton.Content;
                    
                    if (questions[currentPanel].QuestionTrueAnswer == currentTxtBlock.Text.Remove(0,3))
                    {
                        radioButton.Foreground = Brushes.Green;
                        if (radioButton.IsChecked == true)
                        {
                            receivedPoints += 0.9;
                            trueAnswer = true;
                        }
                    }
                    radioButton.IsEnabled = false;
                }

                if (trueAnswer)
                {
                    grid.Children.OfType<Image>().First().Visibility = Visibility.Visible;
                }
                else grid.Children.OfType<Image>().Last().Visibility = Visibility.Visible;

                currentPanel++;
                if (currentPanel == 9)
                {
                    break;
                }
            }

            if (this.GridsStackPanel.Children.OfType<Grid>().Last().Children.OfType<TextBox>().First().Text.ToLower() == specialQuestions.First().QuestionTrueAnswer.ToLower())
            {
                this.GridsStackPanel.Children.OfType<Grid>().Last().Children.OfType<Image>().First().Visibility = Visibility.Visible;
                receivedPoints += 1.9;
            }
            else this.GridsStackPanel.Children.OfType<Grid>().Last().Children.OfType<Image>().Last().Visibility = Visibility.Visible;
            
            labelOpenQuestionTrueAnswer.Content = "Oтг. : " + specialQuestions.First().QuestionTrueAnswer;
            labelOpenQuestionTrueAnswer.Visibility = Visibility.Visible;


            this.GridsStackPanel.Children.OfType<Grid>().Last().Children.OfType<TextBox>().First().IsEnabled = false;
            buttonFinishTest.IsEnabled = false;

            labelReceivedPoints.Content = "Брой получени точки: " + receivedPoints;

            isFinished = true;

            Settings.Default.AllReceivedPoints += receivedPoints;
            Settings.Default.FinishedTestsCount+=1;
            Settings.Default.Save();     
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (isFinished==false)
            {
                if (Settings.Default.CanceledTestsCount>0)
                {
                    var areYouSure = MessageBox.Show("Сигурни ли сте, че искате да прекратите теста? Оставащи прекратявания за днес: " + Settings.Default.CanceledTestsCount,
               "Прекратяване на теста", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (areYouSure == MessageBoxResult.No) e.Cancel = true;
                    else
                    {
                        Settings.Default.CanceledTestsCount--;
                        Settings.Default.CanceledTestsDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        Settings.Default.Save();
                    }
                }
                else
                {
                    var areYouSure = MessageBox.Show("Сигурни ли сте, че искате да прекратите теста? Тестът ще бъде автоматично завършен. " ,
               "Прекратяване на теста", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (areYouSure == MessageBoxResult.No) e.Cancel = true;
                    else
                    {
                        e.Cancel = true;
                        FinishTest();
                    }
                }
            
            }
            
            //base.OnClosing(e);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (mainWindow.QuestionFields.Count == 7)
            {
                this.Height += 50;
                return;
            }
            if (mainWindow.QuestionFields.Count > 1)
            {
                this.Height += 30;
            }          
        }

    }
}
