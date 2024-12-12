using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Controls;

namespace SchoolPlanner
{
    public class HomeworkTask
    {
        public string Subject { get; set; }
        public string Task { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletionDate { get; set; }  
    }

    public class HistoryEntry
    {
        public string Action { get; set; }
        public DateTime Date { get; set; }

        public string Subject { get; set; }
        public string TaskDescription { get; set; }
        public DateTime Deadline { get; set; } 

        public HistoryEntry(string action, DateTime date, string subject = "", string taskDescription = "", DateTime? deadline = null)
        {
            Action = action;
            Date = date;
            Subject = subject;
            TaskDescription = taskDescription;
            Deadline = deadline ?? DateTime.MinValue; 
        }
    }

    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<string> AvailableSubjects { get; set; } = new ObservableCollection<string>
        {
            "Математика", "Физика", "Химия", "Русский язык", "История", "Биология", "География"
        };

        public ObservableCollection<HomeworkTask> HomeworkList { get; set; } = new ObservableCollection<HomeworkTask>();

        public ObservableCollection<HistoryEntry> History { get; set; } = new ObservableCollection<HistoryEntry>();

        private string _selectedHomeworkSubject;
        public string SelectedHomeworkSubject
        {
            get { return _selectedHomeworkSubject; }
            set
            {
                if (_selectedHomeworkSubject != value)
                {
                    _selectedHomeworkSubject = value;
                    OnPropertyChanged(nameof(SelectedHomeworkSubject));
                }
            }
        }

        public MainViewModel()
        {
            // Загрузка данных из файла
            LoadData();
        }

        // Добавление домашнего задания
        public void AddHomework(string subject, string task, DateTime deadline)
        {
            var homework = new HomeworkTask { Subject = subject, Task = task, Deadline = deadline };
            HomeworkList.Add(homework);

            var historyEntry = new HistoryEntry(
                $"Добавлено задание: {task} по предмету '{subject}'", 
                DateTime.Now,
                subject,  
                task,     
                deadline  
            );
            History.Insert(0, historyEntry);

            SaveData();
        }

        // отметка заданий каr выполненные
        public void MarkAsCompleted(HomeworkTask task)
        {
            if (task != null && !task.IsCompleted)
            {
                task.IsCompleted = true;
                task.CompletionDate = DateTime.Now;  
                var historyEntry = new HistoryEntry(
                    $"Задание по предмету '{task.Subject}' выполнено: {task.Task}",
                    DateTime.Now,
                    task.Subject,  
                    task.Task,      
                    task.Deadline   
                );
                History.Insert(0, historyEntry);

                HomeworkList.Remove(task);

                SaveData();
            }
        }

        // Сохранение данных в JSON файл
        private void SaveData()
        {
            try
            {
                var filePath = "data.json";
                var data = new
                {
                    HomeworkList,
                    History
                };

                var json = JsonConvert.SerializeObject(data, Formatting.Indented);

                File.WriteAllText(filePath, json);

                Console.WriteLine("Данные успешно сохранены в файл.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
            }
        }

        // Загрузка данных из JSON файла
        private void LoadData()
        {
            try
            {
                var filePath = "data.json";
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);

                    var data = JsonConvert.DeserializeObject<dynamic>(json);

                    HomeworkList.Clear();
                    History.Clear();

                    foreach (var item in data.HomeworkList)
                    {
                        var task = new HomeworkTask
                        {
                            Subject = item.Subject,
                            Task = item.Task,
                            Deadline = item.Deadline,
                            IsCompleted = item.IsCompleted,
                            CompletionDate = item.CompletionDate == null ? (DateTime?)null : DateTime.Parse(item.CompletionDate.ToString())
                        };
                        HomeworkList.Add(task);
                    }

                    foreach (var historyItem in data.History)
                    {
                        var historyEntry = new HistoryEntry(
                            historyItem.Action.ToString(),
                            DateTime.Parse(historyItem.Date.ToString()),
                            historyItem.Subject.ToString(),
                            historyItem.TaskDescription.ToString(),
                            DateTime.Parse(historyItem.Deadline.ToString()) 
                        );
                        History.Add(historyEntry);
                    }

                    Console.WriteLine("Данные успешно загружены.");
                }
                else
                {
                    Console.WriteLine("Файл не найден, данные не были загружены.");
                }

                OnPropertyChanged(nameof(HomeworkList));
                OnPropertyChanged(nameof(History));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
            }
        }
    }

    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = ViewModel;
        }

        // Обработчик кнопки "Добавить ДЗ"
        private void AddHomeworkButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, что все данные введены
            if (string.IsNullOrEmpty(ViewModel.SelectedHomeworkSubject) || string.IsNullOrEmpty(HomeworkTaskTextBox.Text) || HomeworkDeadlinePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            try
            {
                ViewModel.AddHomework(ViewModel.SelectedHomeworkSubject, HomeworkTaskTextBox.Text, HomeworkDeadlinePicker.SelectedDate.Value);

                HomeworkTaskTextBox.Clear();
                HomeworkDeadlinePicker.SelectedDate = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении задания: {ex.Message}");
            }
        }

        // Обработчик кнопки "Выполнено"
        private void MarkAsCompletedButton_Click(object sender, RoutedEventArgs e)
        {
            var homework = (HomeworkTask)((Button)sender).CommandParameter;

            ViewModel.MarkAsCompleted(homework);
        }

        // Обработчик события "GotFocus" для HomeworkTaskTextBox
        private void HomeworkTaskTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "Задание")
            {
                textBox.Clear();
            }
        }
    }
}
