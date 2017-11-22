using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.Collections;
namespace refrigerator
{
    enum RegimeOfFreeze { min = 1, middle, hight }//перечисленние для режимов морозильной каммеры
    class TemperatureControl
    {
        double maxCelsius;//максимальный предел режима
        double minCelsius;//минимальный предел режима
        double currentTemperature = 7;//текущая температура в помещении
        string freezeName;//имя морозильной камеры для события
        RegimeOfFreeze curRegime;
        public RegimeOfFreeze CurRegime { get { return curRegime; } }
        public string FreezeName { get { return freezeName; } }//доступ к полю данных
        public double MaxCelsius { get { return maxCelsius; } }//доступ к полю данных
        public double MinCelsius { get { return minCelsius; } }//доступ к полю данных
        public double CurrentTemperature { get { return currentTemperature; } set { currentTemperature = value; } }//доступ к полю данных
        public TemperatureControl()//конструктор по умолчанию
        {
            maxCelsius = minCelsius = currentTemperature = 0;
        }
        public void CelsiusValueChange(RegimeOfFreeze f_regime)
        {
            switch (f_regime)
            {
                case RegimeOfFreeze.min://vegetables
                    minCelsius = 3;
                    maxCelsius = 6;
                    break;
                case RegimeOfFreeze.middle://half-finished product
                    minCelsius = 2;
                    maxCelsius = 4;
                    break;
                case RegimeOfFreeze.hight://fresh zone
                    minCelsius = 0;
                    maxCelsius = 1;
                    break;
                default: break;
            }
            curRegime = f_regime;
        }
        public TemperatureControl(RegimeOfFreeze f_regime, string f_name)//смена режима морозильной камеры
        {
            freezeName = f_name;
            CelsiusValueChange(f_regime);
        }
    }
    delegate void TempuratureEvent();//делегат для будущего события
    class Engine : IMessageEvent
    {
        TemperatureControl temperature;//экземпляр Терморегулятора, поскольку мотор управляет температурой
        public event RefregiratorEvents message;//событие основаное на сигнатуре делегата для журнала логов
        bool statusOfTheEngine;//текущее состояние мотора true-работает false-отключён
        bool crashStatus;//статус для указания поломки мотора
        public TemperatureControl getTc { get { return temperature; } }//доступ к терморегулятору
        public string GetFreezeName { get { return temperature.FreezeName; } }///доступ к имени полки
        public double GetCurrentTemperature { get { return temperature.CurrentTemperature; } }///доступ к текущей температуре в камере
        public bool StatusOfTheEngine { get { return statusOfTheEngine; } }//доступ к статусу мотора
        public bool CrashStatus { get { return crashStatus; } }///доступ к статусу работоспособности мотора
        public Engine()
        {
            temperature = null;
        }
        public Engine(TemperatureControl f_value)
        {
            temperature = f_value;
        }
        public void EngineOn()//включить мотор
        {
            statusOfTheEngine = true;//мотор работает
            //записать в журнал логов
            if (message != null) { message(this, new ListHandlerEventArgs(DateTime.Now, "Engine on", temperature.FreezeName)); }
            //таймер для управления мотором
            System.Windows.Threading.DispatcherTimer time = new System.Windows.Threading.DispatcherTimer();
            time.Interval = new TimeSpan(0, 0, 1);
            time.Tick += (Object myObject, EventArgs myEventArgs) =>
            {
                //неизвесное число для поломки мотора
                Random s = new Random();
                int num = s.Next(1, 10000);
                if (num == 543)
                {
                    /*если число совпало
                     * остановить мотор
                     * изменить статус мотора
                    */
                    time.Stop();
                    statusOfTheEngine = false;
                    CrushMotor();//сломать мотор
                }
                temperature.CurrentTemperature -= 0.125;//понижаем температуру
                //если понизили до минимального предела, остановить, следующее действие через булевскую переменную
                if (temperature.CurrentTemperature <= temperature.MinCelsius) { statusOfTheEngine = false; time.Stop(); }
            };
            time.Start();
        }
        public void EngineOff()
        {
            statusOfTheEngine = true;//статус-мотор отключён
            //событие для журнала логов
            if (message != null) { message(this, new ListHandlerEventArgs(DateTime.Now, "Engine off", temperature.FreezeName)); }
            //таймер для повышения температуры
            System.Windows.Threading.DispatcherTimer time = new System.Windows.Threading.DispatcherTimer();
            time.Interval = new TimeSpan(0, 0, 1);
            time.Tick += (Object myObject, EventArgs myEventArgs) =>
            {
                temperature.CurrentTemperature += 0.125;
                //если достигнут предел, освобождаем ресурс таймера ,следующее действие через булевскую переменную
                if (temperature.CurrentTemperature >= temperature.MaxCelsius) { statusOfTheEngine = false; time.Stop(); }
            };
            time.Start();
        }
        public void CrushMotor()
        {
            //событие для журнала логов
            if (message != null) { message(this, new ListHandlerEventArgs(DateTime.Now, "Engine crushed", temperature.FreezeName)); }
            //показываем что мотор сломан
            crashStatus = true;
            //подаем звуковой сигнал
            System.Media.SystemSounds.Hand.Play();
            //на последок поднимаем температуру в камере
            EngineOff();
        }
    }
    interface IThermometer
    {
        double TakeTemperature { get; }//играет роль термометра
    }
    interface IMessageEvent
    {
        event RefregiratorEvents message;//событие для журнала логов
    }
    delegate void RefregiratorEvents(object source, ListHandlerEventArgs args);//делегат на основе которого строится событие
    class Freezer : IThermometer, IMessageEvent
    {
        System.Windows.Threading.DispatcherTimer time;//таймер для дверцы холодильника
        Engine mazda;//морозильная камера управляет мотором
        public event RefregiratorEvents message;
        public event TempuratureEvent show;//индикатор температуры
        public Freezer()
        {
            time = new System.Windows.Threading.DispatcherTimer();
        }
        public Freezer(TemperatureControl f_choice)
        {
            mazda = new Engine(f_choice);
            time = new System.Windows.Threading.DispatcherTimer();
        }
        public RegimeOfFreeze IdeaTemperature//меняем положение терморегулятора
        {
            set
            {
                if (message != null)//событие терморегулятора
                {
                    message(this, new ListHandlerEventArgs(DateTime.Now, "Regime of freeze changed", mazda.GetFreezeName));
                }
                mazda.getTc.CelsiusValueChange(value);
            }
        }
        public double TakeTemperature { get { return mazda.GetCurrentTemperature; } }//играет роль термометра
        public string ShowTemperature()//значение термометра в строку
        {
            return TakeTemperature.ToString("0.00" + " °C");
        }
        public void MesuareTemperature()
        {
            //таймер для измерения температуры
            System.Windows.Threading.DispatcherTimer time = new System.Windows.Threading.DispatcherTimer();
            time.Interval = new TimeSpan(0, 0, 1);
            time.Tick += (Object myObject, EventArgs myEventArgs) =>
            {
                if (show != null) show();//событие которое показывает температуру
                if (!mazda.StatusOfTheEngine)//если мотор отключён
                {
                    //если максимально допустимая температура включить мотор
                    if (mazda.GetCurrentTemperature >= mazda.getTc.MaxCelsius && !mazda.CrashStatus) mazda.EngineOn();
                    //отключить мотор
                    if (mazda.GetCurrentTemperature <= mazda.getTc.MinCelsius && !mazda.CrashStatus) mazda.EngineOff();
                }
            };
            time.Start();
        }
        public void DoorOpen()
        {
            //дверца открыта
            if (message != null) { message(this, new ListHandlerEventArgs(DateTime.Now, "Door open", mazda.GetFreezeName)); }
            //Первоначальный интервал, после которого будет подаватся звуковой сигнал
            time = new System.Windows.Threading.DispatcherTimer();
            time.Interval = new TimeSpan(0, 0, 8);
            time.Start();
            time.Tick += (Object myObject, EventArgs myEventArgs) =>
            {
                //интервал прошёл, подать звуковой сигнал
                time.Interval = new TimeSpan(0, 0, 1);
                System.Media.SystemSounds.Hand.Play();
            };
        }
        public void DoorClose()
        {
            //дверца закрыта
            if (message != null) { message(this, new ListHandlerEventArgs(DateTime.Now, "Door close", mazda.GetFreezeName)); }
            //остановить таймер для дверцы холодильника
            time.Stop();
        }
        public Engine GetEngine { get { return mazda; } }//получить доступ к мотору
        public TemperatureControl GetTC { get { return mazda.getTc; } }//получить доступ к терморугулятор
    }
    class Refrigerator
    {
        Freezer upperShelf;//создаем
        Freezer middleShelf;//морозильные
        Freezer lowerShelf;//камеры
        public Refrigerator()
        {
            upperShelf = new Freezer();
            middleShelf = new Freezer();
            lowerShelf = new Freezer();
        }
        public Refrigerator(Freezer f_first, Freezer f_second, Freezer f_third)
        {
            upperShelf = f_first;
            middleShelf = f_second;
            lowerShelf = f_third;
        }
        public Freezer UpperShelf { get { return upperShelf; } }
        public Freezer LowerShelf { get { return lowerShelf; } }
        public Freezer MiddleShelf { get { return middleShelf; } }
        public void ON()
        {
            upperShelf.MesuareTemperature();//включаем
            middleShelf.MesuareTemperature();//термоментры
            lowerShelf.MesuareTemperature();//камер, которые запустят мотор
        }
    }
    class ListHandlerEventArgs : System.EventArgs
    {
        //добавляем поля к событию в виде авто.свойств
        public string FreezeName { get; set; }
        public DateTime Time { get; set; }
        public string Changes { get; set; }
        public ListHandlerEventArgs()
        {
            FreezeName = Changes = "";
            Time = DateTime.MinValue;
        }
        public ListHandlerEventArgs(DateTime f_time, string f_changes, string f_name)
        {
            FreezeName = f_name;
            Time = f_time;
            Changes = f_changes;
        }
    }
    class JournalEntry
    {
        //класс который будет хранить события
        public string FreezeName { get; set; }
        public DateTime Time { get; set; }
        string EventName { get; set; }
        public JournalEntry(DateTime f_time, string f_event, string f_name)
        {
            FreezeName = f_name;
            Time = f_time;
            EventName = f_event;
        }
        string Information()
        {
            string info = "Name->" + FreezeName + ";Time>" + Time + ";Changes->" + EventName + "\n";
            return info;
        }
        public override string ToString()
        {
            return Information();
        }
    }
    class TeamsJournal
    {
        //класс подписчик
        List<JournalEntry> events = new List<JournalEntry>();//список событий
        public void NewEvents(object sender, ListHandlerEventArgs f_info)//метод котоый реагирует на события
        {
            //событие произошло, формируем узел списка
            JournalEntry teamEvent = new JournalEntry(f_info.Time, f_info.Changes, f_info.FreezeName);
           //добавляем узел
            events.Add(teamEvent);
        }
        public IEnumerator GetEnumerator()//итератор для перебора событий
        {
            foreach (var item in events)
            {
                yield return item;
            }
        }
    }
}
