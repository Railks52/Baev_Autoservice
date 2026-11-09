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

namespace Baev_Autoservice
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        //добавим новое поле, которое будет хранить в себе экземпляр добавляемого сервиса
        private Service _currentService = new Service();
        public AddEditPage(Service SelectedService)
        {
            InitializeComponent();

            if (SelectedService != null)
                _currentService = SelectedService;
            //при инициализации установим DataContext страницы - этот созданный обьект
            DataContext = _currentService;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentService.Title))
                errors.AppendLine("Укажите название услуги");
            if (_currentService.Cost == 0)
                errors.AppendLine("Укажите стоимость услуги");

            if (_currentService.Duration == 0)
                errors.AppendLine("Укажите длительность услуги");
            if (_currentService.Duration > 240 || _currentService.Duration < 0)
                errors.AppendLine("Длительность не может быть больше 240 минут или меньше 0");

            if (_currentService.Discount < 0 || _currentService.Discount > 100)
                errors.AppendLine("Укажите скидку от 0 до 100");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            var allServices = BaevAutoserviceEntities.GetContext().Service.ToList();
            allServices = allServices.Where(p => p.Title == _currentService.Title && p.ID != _currentService.ID).ToList();

            if (allServices.Count >0)
            {
                MessageBox.Show("Уже существует такая услуга");
                return;
            }
            if (allServices.Count==0)
            {
                if (_currentService.ID == 0)
                    BaevAutoserviceEntities.GetContext().Service.Add(_currentService);
            }
            try
            {
                BaevAutoserviceEntities.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена");
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                //если не цифры
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }
        }
    }
}
