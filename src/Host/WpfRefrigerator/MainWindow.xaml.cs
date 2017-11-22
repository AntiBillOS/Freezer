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
using System.IO;
using refrigerator;
namespace WpfRefrigerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        string regime = "";//режим по умолчанию
        Refrigerator rf;//создаеи холодильник
        internal Refrigerator Rf { get { return rf; } set { rf = value; } }//доступ к холодильнику
        TeamsJournal rfEvents;//класс подписчик
        internal TeamsJournal RfEvents { get { return rfEvents; } }//доступ к классу подписчику
        private void TemperatureEvent(Freezer f_freezer, TextBox f_tbox)
        {
            f_freezer.show += () => f_tbox.Text = f_freezer.ShowTemperature();//подписуемся на событие которое показувает температуру
        }
        private void MessageEvent(Freezer f_freezer, TeamsJournal f_journal)
        {
            //подписуемся на события холодильника
            f_freezer.message += f_journal.NewEvents;
            f_freezer.GetEngine.message += f_journal.NewEvents;
        }
        private void Load(object sender, RoutedEventArgs e)
        {
            //иннициализация холодильника и его событий
            rfEvents = new TeamsJournal();
            Freezer firstCam = new Freezer(new TemperatureControl(RegimeOfFreeze.min, "First Freezer"));
            Freezer secondCam = new Freezer(new TemperatureControl(RegimeOfFreeze.min, "SecondFreezer"));
            Freezer thirdCam = new Freezer(new TemperatureControl(RegimeOfFreeze.min, "ThirdFreezer"));
            TemperatureEvent(firstCam, firstFreezeCelsius);
            TemperatureEvent(secondCam, secondFreezeCelsius);
            TemperatureEvent(thirdCam, thirdFreezeCelsius);
            MessageEvent(firstCam, rfEvents);
            MessageEvent(secondCam, rfEvents);
            MessageEvent(thirdCam, rfEvents);
            Rf = new Refrigerator(firstCam, secondCam, thirdCam);
            Rf.ON();
        }
        private void ControlDoor(Freezer f_choice, Button f_current)
        {
            string ont = f_current.Content.ToString();
            if (ont == "close")
            {

                LinearGradientBrush ddd = new LinearGradientBrush(Color.FromRgb(246, 217,217), Color.FromRgb(218, 69,69), new Point(0.5, 0), new Point(0.5, 1));
                f_current.Content = "open";
                f_current.Background = ddd;
                f_choice.DoorOpen();
                Info.Text = f_choice.GetTC.FreezeName + " door open";
            }
            else
            {
                LinearGradientBrush ddd = new LinearGradientBrush(Color.FromRgb(218, 69, 69), Color.FromRgb(246, 217, 217), new Point(0.5, 0), new Point(0.5, 1));
                f_current.Content = "close";
                f_current.Background =ddd;
                f_choice.DoorClose();
                Info.Text = f_choice.GetTC.FreezeName + " door close";
            }
        }
        private void firstDoor_Click(object sender, RoutedEventArgs e)
        {
            ControlDoor(Rf.UpperShelf, firstDoor);
        }
        private void secondDoor_Click(object sender, RoutedEventArgs e)
        {
            ControlDoor(Rf.MiddleShelf, secondDoor);
        }
        private void thirdDoor_Click(object sender, RoutedEventArgs e)
        {
            ControlDoor(Rf.LowerShelf, thirdDoor);
        }
        private void ChangeRegimeOfFreeze(Freezer f_freezerCam)
        {
            double saveTemp = f_freezerCam.TakeTemperature;
            if(f_freezerCam.GetTC.CurRegime==RegimeOfFreeze.min)
            {
                first.IsChecked = true;
            }
            else if (f_freezerCam.GetTC.CurRegime == RegimeOfFreeze.middle)
            {
                second.IsChecked = true;
            }
            else
            {
                third.IsChecked = true;
            }
            switch (regime)
            {
                case "Minimum":
                    f_freezerCam.IdeaTemperature = RegimeOfFreeze.min;
                    break;
                case "Middle":
                    f_freezerCam.IdeaTemperature = RegimeOfFreeze.middle;
                    break;
                case "Maximum":
                    f_freezerCam.IdeaTemperature = RegimeOfFreeze.hight;
                    break;
            }
            f_freezerCam.GetTC.CurrentTemperature = saveTemp;
            Info.Text = f_freezerCam.GetTC.FreezeName + " thermostat positions changed";
        }
        private void ChangeCam()
        {
            int choice = Convert.ToInt32(FreezerBar.Value);
            switch (choice)
            {
                case 1:
                    ChangeRegimeOfFreeze(Rf.UpperShelf);
                    break;
                case 2:
                    ChangeRegimeOfFreeze(Rf.MiddleShelf);
                    break;
                case 3:
                    ChangeRegimeOfFreeze(Rf.LowerShelf);
                    break;
            }
        }

        private void RegimeChanged(object sender, RoutedEventArgs e)
        {
            //новый режим морозильной каммеры
            RadioButton userChoice = (RadioButton)sender;
            regime = userChoice.Content.ToString();
            ChangeCam();
            userChoice.IsChecked = false;
            
        }
        private void ViewJournal(object sender, RoutedEventArgs e)
        {
            //просмотр событий в новом окне
            Journal currentEvents = new Journal();
            currentEvents.Owner = this;
            currentEvents.ShowDialog();
        }
    }
}
