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

            if (_currentService.Discount == 0)
                errors.AppendLine("Укажите скидку");
            if (String.IsNullOrWhiteSpace(_currentService.Duration))
                errors.AppendLine("Укажите длительность услуги");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            // добавить в контекст текущие значения новой услуги
            if (_currentService.ID == 0)
                BaevAutoserviceEntities.GetContext().Service.Add(_currentService);

            //сохранить изменения если никаких ошибок не получилось при этом
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
    }
}
