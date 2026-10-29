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
    /// Логика взаимодействия для SignUpPage.xaml
    /// </summary>
    public partial class SignUpPage : Page
    {
        //добавим новое поле, которое будет хранить в себе экземпляр добавляемого сервиса
        private Service _currentService=new Service();
        private ClientService _currentClientService = new ClientService();
        public SignUpPage(Service SelectedService)
        {
            InitializeComponent();
            if (SelectedService != null)
                this._currentService = SelectedService;

            //при инициализации установим datacontext страницы - этот созданный обьект
            //чтобы на форму подгрузить выбранные наименование услуги и длительность
            DataContext = _currentService;

            //вытащим из БД таблицу клиент
            var _currentClient = BaevAutoserviceEntities.GetContext().Client.ToList();
            //свяжем ее с комбобоксом
            ComboClient.ItemsSource = _currentClient;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors=new StringBuilder();

            if (ComboClient.SelectedItem == null)
                errors.AppendLine("Укажите ФИО клиента");
            if (StartDate.Text == "")
                errors.AppendLine("Укажите дату услуги");
            if (TBStart.Text== "")
                errors.AppendLine("Укажите время начала услуги");
            if (errors.Length>0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }
            //добавить текущие значения новой записи
            _currentClientService.ClientID = ComboClient.SelectedIndex + 1;//тк нумерация с 0
            _currentClientService.ServiceID = _currentService.ID;
            _currentClientService.StartTime=Convert.ToDateTime(StartDate.Text+" "+TBStart.Text);

            if (_currentClientService.ID == 0)
                BaevAutoserviceEntities.GetContext().ClientService.Add(_currentClientService);

            //сохранить эти изменения если никаких ошибок при этом не получилось
            try
            {
                BaevAutoserviceEntities.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена.");
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }

        private void TBStart_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = TBStart.Text;
            if (s.Contains(':'))
            {
                string[] start = s.Split(new char[] { ':' });
                if (start[0].Length > 0 && start[0].Length <= 2 && start[1].Length == 2
                    && Convert.ToInt32(start[0]) >= 0 && Convert.ToInt32(start[0]) <= 23
                    && Convert.ToInt32(start[1]) >= 0 && Convert.ToInt32(start[1]) <= 59)
                {
                    int startHour = Convert.ToInt32(start[0].ToString()) * 60;
                    int startMin = Convert.ToInt32(start[1].ToString());
                    int sum = startHour + startMin + _currentService.Duration;
                    int EndHour = sum / 60;
                    int EndMin = sum % 60;
                    if (EndMin == 0)
                        s = EndHour.ToString() + ":" + "00";
                    else
                        s = EndHour.ToString() + ":" + EndMin.ToString();
                    TBEnd.Text = s;
                }
                else
                {
                    TBEnd.Text = "";
                }
            }
        }
    }
}
