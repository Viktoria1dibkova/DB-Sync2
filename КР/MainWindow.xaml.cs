using ClassLibrary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ClassLibrary.Models;

namespace КР
{
    public partial class MainWindow : Window
    {
        private string CurrentConn = DbConfig.UniversityConn;
        private string CurrentLevel = "Ректорат";

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DateGrade.SelectedDate = DateTime.Now;
            DateOrder.SelectedDate = DateTime.Now;
            await InitDatabaseAsync();
            await LoadAllDataAsync();
            ApplyPermissions();
        }

        /// <summary>
        /// Инициализация соединения
        /// </summary>
        /// <returns></returns>
        private async Task InitDatabaseAsync()
        {
            try
            {
                await DbService.EnsureSchemaAsync(CurrentConn);
                StatusText.Text = $"Подключено: {CurrentLevel}";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка подключения: " + ex.Message;
            }
        }

        /// <summary>
        /// Выбор уровня (компьютера)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Level_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                if (rb == RadioUniversity) 
                { 
                    CurrentConn = DbConfig.UniversityConn; 
                    CurrentLevel = "Ректорат"; 
                }
                else if (rb == RadioFaculty) 
                { 
                    CurrentConn = DbConfig.FacultyConn; 
                    CurrentLevel = "Факультет ИТ"; 
                }
                else if (rb == RadioDept) 
                { 
                    CurrentConn = DbConfig.DeptConn; 
                    CurrentLevel = "Кафедра ПО"; 
                }
                await InitDatabaseAsync();
                await LoadAllDataAsync();
                ApplyPermissions();
            }
        }

        /// <summary>
        /// Применение ограничений
        /// </summary>
        private void ApplyPermissions()
        {
            bool isUniversity = CurrentLevel == "Ректорат";
            bool isFaculty = CurrentLevel == "Факультет ИТ";
            bool isDept = CurrentLevel == "Кафедра ПО";

            BtnAddFaculty.Visibility = isUniversity ? Visibility.Visible : Visibility.Hidden;
            BtnDeleteFaculty.Visibility = isUniversity ? Visibility.Visible : Visibility.Hidden;

            BtnAddDept.Visibility = (isUniversity || isFaculty) ? Visibility.Visible : Visibility.Hidden;
            BtnDeleteDept.Visibility = (isUniversity || isFaculty) ? Visibility.Visible : Visibility.Collapsed;

            BtnAddProgram.Visibility = (isUniversity || isFaculty) ? Visibility.Visible : Visibility.Hidden;
            BtnDeleteProgram.Visibility = (isUniversity || isFaculty) ? Visibility.Visible : Visibility.Hidden;

            if (isDept)
            {
                CmbOrderScope.SelectedIndex = 2;
                CmbOrderScope.IsEnabled = false;
            }
            else
            {
                CmbOrderScope.IsEnabled = true;
                CmbOrderScope.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Загрузка данных
        /// </summary>
        /// <returns></returns>
        private async Task LoadAllDataAsync()
        {
            try
            {
                GridFaculties.ItemsSource = await DbService.LoadFacultiesAsync(CurrentConn);
                GridDepartments.ItemsSource = await DbService.LoadDepartmentsAsync(CurrentConn);
                var programs = await DbService.LoadProgramsAsync(CurrentConn);
                GridPrograms.ItemsSource = programs;
                CmbPrograms.ItemsSource = programs;
                GridStudents.ItemsSource = await DbService.LoadStudentsAsync(CurrentConn);
                CmbStudents.ItemsSource = await DbService.LoadStudentsAsync(CurrentConn);
                GridOrders.ItemsSource = await DbService.LoadOrdersAsync(CurrentConn);
                GridGrades.ItemsSource = await DbService.LoadGradesAsync(CurrentConn);
                StatusText.Text = $"Данные загружены ({CurrentLevel})";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка загрузки: " + ex.Message;
            }
        }

        /// <summary>
        /// Добавление факультета
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddFaculty_Click(object sender, RoutedEventArgs e)
        {
            var name = TxtFacultyName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название факультета!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await DbService.AddFacultyAsync(CurrentConn, name);
                TxtFacultyName.Clear();
                await LoadAllDataAsync();
                StatusText.Text = $"Факультет '{name}' добавлен";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавление кафедры
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (GridFaculties.SelectedItem is not FacultyItem f)
            {
                MessageBox.Show("Сначала выделите факультет в таблице!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var name = TxtDeptName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название кафедры!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await DbService.AddDepartmentAsync(CurrentConn, f.Id, name);
                TxtDeptName.Clear();
                await LoadAllDataAsync();
                StatusText.Text = $"Кафедра '{name}' добавлена";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавление направления
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddProgram_Click(object sender, RoutedEventArgs e)
        {
            if (GridDepartments.SelectedItem is not DeptItem d)
            {
                MessageBox.Show("Сначала выделите кафедру в таблице!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var code = TxtProgCode.Text.Trim();
            var profile = TxtProgProfile.Text.Trim();

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(profile))
            {
                MessageBox.Show("Заполните код и профиль направления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await DbService.AddProgramAsync(CurrentConn, d.Id, code, profile);
                TxtProgCode.Clear();
                TxtProgProfile.Clear();
                await LoadAllDataAsync();
                StatusText.Text = $"Направление '{profile}' добавлено";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Добавление студента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            if (CmbPrograms.SelectedItem is not Models.ProgramItem p)
            {
                StatusText.Text = "Сначала выберите направление из списка!";
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtStudentName.Text) || string.IsNullOrWhiteSpace(TxtStudentGroup.Text))
            {
                StatusText.Text = "Заполните ФИО и группу!";
                return;
            }

            await DbService.AddStudentAsync(CurrentConn, p.Id, TxtStudentName.Text, TxtStudentGroup.Text, int.Parse(TxtStudentYear.Text));
            TxtStudentName.Clear();
            TxtStudentGroup.Clear();
            await LoadAllDataAsync();
        }

        /// <summary>
        /// Добавление оценки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddGrade_Click(object sender, RoutedEventArgs e)
        {
            if (CmbStudents.SelectedItem is not Models.StudentItem s)
            {
                StatusText.Text = "Выберите студента из списка!";
                return;
            }
            await DbService.AddGradeAsync(CurrentConn, s.Id, TxtGradeSubject.Text, int.Parse(TxtGradeValue.Text), DateGrade.SelectedDate ?? DateTime.Now, TxtGradeTeacher.Text);
            await LoadAllDataAsync();
        }

        /// <summary>
        /// Добавление приказа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtOrderText.Text))
            {
                StatusText.Text = "Введите текст приказа!";
                return;
            }

            var scope = (CmbOrderScope.SelectedItem as ComboBoxItem).Tag.ToString() ?? "ALL";
            var status = (CmbOrderStatus.SelectedItem as ComboBoxItem).Tag.ToString() ?? "active";

            await DbService.AddOrderAsync(CurrentConn, TxtOrderText.Text, DateOrder.SelectedDate ?? DateTime.Now, scope, status);
            TxtOrderText.Clear();
            await LoadAllDataAsync();
        }
        /// <summary>
        /// Удаление факультета
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private async void DeleteFaculty_Click(object sender, RoutedEventArgs e)
        {
            var item = GridFaculties.SelectedItem as FacultyItem;
            if (item == null) 
            { 
                StatusText.Text = "Выберите факультет в таблице"; 
                return; 
            }
            var result = MessageBox.Show($"Удалить факультет '{item.Name}' и все его кафедры/направления?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await DbService.DeleteFacultyAsync(CurrentConn, item.Id);
                await LoadAllDataAsync();
                StatusText.Text = $"Факультет '{item.Name}' удален";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка: " + ex.Message;
            }
        }

        /// <summary>
        /// Удаление кафедры
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentLevel == "Кафедра ПО")
            {
                StatusText.Text = "Кафедра не может удалять кафедры!";
                return;
            }

            var item = GridDepartments.SelectedItem as DeptItem;
            if (item == null) 
            { 
                StatusText.Text = "Выберите кафедру в таблице"; 
                return; 
            }
            var result = MessageBox.Show($"Удалить кафедру '{item.Name}'?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await DbService.DeleteDepartmentAsync(CurrentConn, item.Id);
                await LoadAllDataAsync();
                StatusText.Text = $"Кафедра '{item.Name}' удалена";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка (возможно, есть привязанные направления): " + ex.Message;
            }
        }

        /// <summary>
        /// Удаление направления
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteProgram_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentLevel == "Кафедра ПО")
            {
                StatusText.Text = "Кафедра не может удалять направления!";
                return;
            }

            var item = GridPrograms.SelectedItem as ProgramItem;
            if (item == null) 
            { 
                StatusText.Text = "Выберите направление в таблице"; 
                return; 
            }
            var result = MessageBox.Show($"Удалить направление '{item.Code} {item.Profile}'?","Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await DbService.DeleteProgramAsync(CurrentConn, item.Id);
                await LoadAllDataAsync();
                StatusText.Text = $"Направление '{item.Code}' удалено";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка (возможно, есть привязанные студенты): " + ex.Message;
            }
        }

        /// <summary>
        /// Удаление студента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            var item = GridStudents.SelectedItem as StudentItem;
            if (item == null) 
            { 
                StatusText.Text = "Выберите студента в таблице"; 
                return; 
            }
            var result = MessageBox.Show($"Удалить студента '{item.FullName}'?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await DbService.DeleteStudentAsync(CurrentConn, item.Id);
                await LoadAllDataAsync();
                StatusText.Text = $"Студент '{item.FullName}' удален";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка: " + ex.Message;
            }
        }

        /// <summary>
        /// Удаление оценки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteGrade_Click(object sender, RoutedEventArgs e)
        {
            var item = GridGrades.SelectedItem as GradeItem;
            if (item == null)
            { 
                StatusText.Text = "Выберите оценку в таблице"; 
                return; 
            }

            var result = MessageBox.Show($"Удалить оценку '{item.Subject}' у студента '{item.StudentName}'?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await DbService.DeleteGradeAsync(CurrentConn, item.Id);
                await LoadAllDataAsync();
                StatusText.Text = "Оценка удалена";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка: " + ex.Message;
            }
        }

         
       /// <summary>
       /// Удаление приказа
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            var item = GridOrders.SelectedItem as OrderItem;
            if (item == null) 
            { 
                StatusText.Text = "Выберите приказ в таблице"; 
                return; 
            }

            var result = MessageBox.Show($"Удалить приказ '{item.Text}'?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await DbService.DeleteOrderAsync(CurrentConn, item.Id);
                await LoadAllDataAsync();
                StatusText.Text = "Приказ удален";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка: " + ex.Message;
            }
        }

        /// <summary>
        /// Синхронизация
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Sync_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentLevel == "Ректорат")
            {
                StatusText.Text = "⚠️ Ректорат является главным узлом. Синхронизация не требуется.";
                return;
            }

            StatusText.Text = "⏳ Выполняется синхронизация с родительским узлом...";
            try
            {
                string parentConn = CurrentLevel == "Факультет ИТ" ? DbConfig.UniversityConn : DbConfig.FacultyConn;
                await SyncService.SyncFullAsync(parentConn, CurrentConn);

                StatusText.Text = "Синхронизация успешно завершена!";
                await LoadAllDataAsync();
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка синхронизации: " + ex.Message;
            }
        }
    }
}