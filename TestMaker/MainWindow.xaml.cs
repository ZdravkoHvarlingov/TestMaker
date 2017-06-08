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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity;
using TestMaker.Data;
using TestMaker.Model;
using TestMaker.Properties;
using TestMaker.Data.Migrations;
using System.Windows.Controls.Primitives;
using System.IO;
using System.ComponentModel;


namespace TestMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TestWindow testWindow;
        private List<QuestionField> questionFields;
        private BackgroundWorker backGroundWorker;

        public MainWindow()
        {            
            questionFields = new List<QuestionField>();
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TestMakerContext, Configuration>());

            backGroundWorker = new BackgroundWorker();
            backGroundWorker.DoWork += backGroundWorker_DoWork;
            backGroundWorker.RunWorkerCompleted += backGroundWorker_RunWorkerCompleted;

            #region -Make folder if needed -

            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folderPath = appDataFolder + @"\TestMakerData";
            if (!Directory.Exists(folderPath))
            {
                DirectoryInfo dirIn = Directory.CreateDirectory(folderPath);
            }
            #endregion

            InitializeComponent();           

            if (new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day)!=Settings.Default.CanceledTestsDateTime)
            {
                Settings.Default.CanceledTestsDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                Settings.Default.CanceledTestsCount = 3;
                Settings.Default.Save();
            }

            labelSumTestsInfo.Content = "Брой решени тестове: " +
                Settings.Default.FinishedTestsCount + 
                ", \r\n среден брой точки: " +
                Math.Round((Settings.Default.AllReceivedPoints / Settings.Default.FinishedTestsCount), 2);
        }

        void backGroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (testWindow==null)
            {
                testWindow = new TestWindow(this);
                testWindow.Closed+=testWindow_Closed;
            }
            this.Hide();
            testWindow.ShowDialog();
        }

        void backGroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            using (var db= new TestMakerContext())
            {
                var question = db.Questions.Where(q=>q.QuestionId%2==0);
            }                 
        }
      
        public List<QuestionField> QuestionFields { get { return this.questionFields; } set { this.questionFields=value;} }

        private void OnButtonCLick(object sender, RoutedEventArgs e)
        {                  
            if (questionFields.Count == 0)
            {
                MessageBox.Show("Не сте избрали тематика на теста!");
                return;
            }

            labelSumTestsInfo.Visibility = Visibility.Hidden;
            imageLoading.Visibility = Visibility.Visible;
            labelLoading.Visibility = Visibility.Visible;
            FieldButtons.IsEnabled = false;
            buttonStartTest.IsEnabled = false;

            backGroundWorker.RunWorkerAsync();

        }

        void testWindow_Closed(object sender, EventArgs e)
        {                     
            testWindow = null;
            labelSumTestsInfo.Content = "Брой решени тестове: " +
                Settings.Default.FinishedTestsCount +
                ", \r\n среден брой точки: " +
                Math.Round((Settings.Default.AllReceivedPoints / Settings.Default.FinishedTestsCount),2);
            this.Show();

            labelSumTestsInfo.Visibility = Visibility.Visible;
            imageLoading.Visibility = Visibility.Hidden;
            labelLoading.Visibility = Visibility.Hidden;
            FieldButtons.IsEnabled = true;
            buttonStartTest.IsEnabled = true;
        }

        private void OnButtonPhilosophyClick(object sender, RoutedEventArgs e)
        {
            FieldButtonsClick(buttonPhilosophy, QuestionField.Philosophy);            
        }

        private void OnButtonBiologyClick(object sender, RoutedEventArgs e)
        {
            FieldButtonsClick(buttonBiology, QuestionField.Biology);     
        }

        public void FieldButtonsClick(ToggleButton button, QuestionField questionField)
        {            
            if (button.IsChecked==true)
            {
                questionFields.Add(questionField);
            }
            else
            {
                questionFields.Remove(questionField);
            }
        }

        private void OnButtonBulgarianClick(object sender, RoutedEventArgs e)
        {
            FieldButtonsClick(buttonBulgarian, QuestionField.Bulgarian);
        }

        private void OnButtonHistoryCLick(object sender, RoutedEventArgs e)
        {
            FieldButtonsClick(buttonHistory, QuestionField.History);
        }

        private void OnButtonEnglishClick(object sender, RoutedEventArgs e)
        {
            FieldButtonsClick(buttonEnglish, QuestionField.English);
        }

        private void OnButtonITClick(object sender, RoutedEventArgs e)
        {
            FieldButtonsClick(buttonIT, QuestionField.IT);
        }

        private void OnMenuItemExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnMenuItemRefreshClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
            Settings.Default.Save();

            labelSumTestsInfo.Content = "Брой решени тестове: " +
                Settings.Default.FinishedTestsCount +
                ", \r\n среден брой точки: " +
                Math.Round((Settings.Default.AllReceivedPoints / Settings.Default.FinishedTestsCount), 2);
        }

        private void OnButtonGeographyClick(object sender, RoutedEventArgs e)
        {
            FieldButtonsClick(buttonGeography, QuestionField.Geography);
        }
        
    }
}
