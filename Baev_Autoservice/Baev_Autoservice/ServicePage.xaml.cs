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
    /// Логика взаимодействия для ServicePage.xaml
    /// </summary>
    public partial class ServicePage : Page
    {
        int CountRecords;//колво записей в таблице
        int CountPage;//общее колво страниц
        int CurrentPage = 0;//текущая страница
        List<Service> CurrentPageList = new List<Service>();
        List<Service> TableList;
        public ServicePage()
        {
            InitializeComponent();
            //добавляем строки
            // загрузить в список из бд
            var currentServices = BaevAutoserviceEntities.GetContext().Service.ToList();
            // связать с нашим листвью
            ServiceListView.ItemsSource = currentServices;
            //добавили строки
            ComboType.SelectedIndex = 0;

            //вызываем UpdateServices()
            UpdateServices();
        }
        private void UpdateServices()
        {
            // берем из бд данные таблиц сервис
            var currentServices = BaevAutoserviceEntities.GetContext().Service.ToList();
            //прописываем фильтрациб по условию задачи
            if (ComboType.SelectedIndex == 0)
                currentServices = currentServices.Where(p => (p.Discount >= 0 && p.Discount <= 1)).ToList();
            if (ComboType.SelectedIndex == 1)
                currentServices = currentServices.Where(p => (p.Discount >= 0 && p.Discount <= 0.05)).ToList();
            if (ComboType.SelectedIndex == 2)
                currentServices = currentServices.Where(p => (p.Discount >= 0.05 && p.Discount <= 0.15)).ToList();
            if (ComboType.SelectedIndex == 3)
                currentServices = currentServices.Where(p => (p.Discount >= 0.15 && p.Discount <= 0.30)).ToList();
            if (ComboType.SelectedIndex == 4)
                currentServices = currentServices.Where(p => (p.Discount >= 0.30 && p.Discount <= 0.70)).ToList();
            if (ComboType.SelectedIndex == 5)
                currentServices = currentServices.Where(p => (p.Discount >= 0.70 && p.Discount <= 1)).ToList();

            // реализуем поиск данных в листвью при вводе текста в окно поиска
            currentServices=currentServices.Where(p=>p.Title.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();
            //реализуем сортировку по убыванию
            if (RButtonDown.IsChecked.Value)
                currentServices = currentServices.OrderByDescending(p => p.Cost).ToList();
            if (RButtonUp.IsChecked.Value)
                currentServices = currentServices.OrderBy(p => p.Cost).ToList();

            // отображаем итоги поиска/фильтрации/сортировки
            ServiceListView.ItemsSource = currentServices;
            //заполнение таблицы для постраничного вывода
            TableList = currentServices;
            //вызов функции отобр кол-ва страниц с параметрами:
            //направление 0 - начальная загрузка
            //0 - выбранная страница
            ChangePage(0, 0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null));
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateServices();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateServices();
        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateServices();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateServices();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // открыть окно редактирования добавления услуг
            Manager.MainFrame.Navigate(new AddEditPage(null));
        }
        private void EditButtonClick(object sender, RoutedEventArgs e)
        {
            // открыть окно редактирования добавления услуг
            Manager.MainFrame.Navigate(new AddEditPage((sender as Button).DataContext as Service));
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility==Visibility.Visible)
            {
                BaevAutoserviceEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                ServiceListView.ItemsSource=BaevAutoserviceEntities.GetContext().Service.ToList();
            }
            UpdateServices();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            //забираем сервис, для которого нажата кнопка "Удалить"
            var currentService=(sender as Button).DataContext as Service;

            //проверка на возможность удаления
            var currentClientServices = BaevAutoserviceEntities.GetContext().ClientService.ToList();
            currentClientServices = currentClientServices.Where(p => p.ServiceID == currentService.ID).ToList();

            if (currentClientServices.Count != 0)//если есть записи на этот сервис
                MessageBox.Show("Невозможно выполнить удаление, тк существуют записи на эту услугу");
            else
            {
                if (MessageBox.Show("Вы точно хотите выполнить удаление?", "Внимание!",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        BaevAutoserviceEntities.GetContext().Service.Remove(currentService);
                        BaevAutoserviceEntities.GetContext().SaveChanges();
                        //выводим в листвью измененную  таблицу сервис
                        ServiceListView.ItemsSource = BaevAutoserviceEntities.GetContext().Service.ToList();
                        //чтобы применились фильтры и поиск, если они были на форме изначально
                        UpdateServices();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void ChangePage(int diretion, int? selectedPage)//функция отвечает за разделение list'a
        {
            //direction - направление. 0-начало 1-пред страница 2-след страница
            //selectedpage - при нажатии на стрелочки передается null
            // при выборе опред страницы в этой переменной находится номер страницы

            CurrentPageList.Clear();//начальная очистка листа
            CountRecords = TableList.Count;//опредлене количества записей во всем списке
            //определение колва страниц
            if (CountRecords % 10 > 0)
                CountPage = CountRecords / 10 + 1;
            else
                CountPage = CountRecords / 10;

            Boolean Ifupdate = true;
            //проверка на правильность - если
            //CurrentPage(номер текущей страницы) "правильный"

            int min;
            if (selectedPage.HasValue) //проверка на значение не ноль так как может быть ноль
            {
                if (selectedPage >= 0 && selectedPage <= CountPage) 
                {
                    CurrentPage = (int)selectedPage;
                    min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                    for (int i = CurrentPage * 10; i < min; i++)
                        CurrentPageList.Add(TableList[i]);
                }
            }
            else//если нажата стрелка
            {
                switch(diretion)
                {
                    case 1: //если нажата кнопка "пред страница"
                        if (CurrentPage > 0) //если кнопка нажата правильно и "назад" можно идти
                        {
                            CurrentPage--;
                            min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                            for (int i = CurrentPage * 10; i < min; i++)
                                CurrentPageList.Add(TableList[i]);
                        }
                        else
                        {
                            Ifupdate = false;
                            //если currentpage попытается выйти из диапазона внесения данных не произойдет
                        }
                        break;

                    case 2://если нажата кнопка "след страница"
                        if(CurrentPage<CountPage-1)//если вперед идти можно
                        {
                            CurrentPage++;
                            min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                            for (int i = CurrentPage * 10; i < min; i++)
                                CurrentPageList.Add(TableList[i]);
                        }
                        else
                            Ifupdate = false;
                        break;
                }
            }
            if (Ifupdate)//если currentPage не вышел из диапазона то
            {
                PageListBox.Items.Clear();
                //удаление старых значений из listbox номеров страниц, нужно чтобы при изменении
                //колва записей колво страниц динамически изменялось
                for (int i = 1; i <= CountPage; i++)
                    PageListBox.Items.Add(i);
                PageListBox.SelectedIndex = CurrentPage;

                //вывод кол-ва записей на странице и общего количества
                min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                TBCount.Text = min.ToString();
                TBALLRecords.Text = " из " + CountRecords.ToString();

                ServiceListView.ItemsSource = CurrentPageList;
                //обновить отображение списка услуг
                ServiceListView.Items.Refresh();
            }
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            // открыть окно записи клиента на выбранную услугу
            Manager.MainFrame.Navigate(new SignUpPage((sender as Button).DataContext as Service));
        }
    }
}
