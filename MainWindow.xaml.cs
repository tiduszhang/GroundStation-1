using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using Speedometer.DataPoints;
using Speedometer.Model;
using Speedometer.Utility;
using Speedometer.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using static Speedometer.ViewModel.MainScreenViewModel;

namespace Speedometer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
  
        private static string comPortStr;

        // MainScreenViewModel object
        private static MainScreenViewModel mainScreenViewModel;
        // Callback method from the ViewModel
        private static ViewModelDataPointReceivedCallback dataPointReceivedCallback;

        private double _axisMax;
        private double _axisMin;
        // Initial DateTime
        private static DateTime initialDateTime;

        private ObservableValue VoltageValues;
        private ObservableValue CurrentValues;
        private ObservableValue WattValues;
        private ObservableValue EnergyValues;
        public ChartValues<MeasureModel> VoltageChartValues { get; set; } // Values of the Voltage Cartesian Chart
        public ChartValues<MeasureModel> CurrentChartValues { get; set; } // Values of the Current Cartesian Chart
        public ChartValues<MeasureModel> WattChartValues { get; set; } // Values of the Watt Cartesian Chart
        public ChartValues<MeasureModel> EnergyChartValues { get; set; } // Values of the Energy Cartesian Chart

        public Func<double, string> DateTimeFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }
        public bool IsReading { get; set; }

        public MainWindow() {
            InitializeComponent();

            this.DataContext = this; IsReading = true;
            initialDateTime = DateTime.Now;

            VoltageValues = new ObservableValue(0);
            CurrentValues = new ObservableValue(1);
            WattValues = new ObservableValue(2);
            EnergyValues = new ObservableValue(3);

            var mapper = Mappers.Xy<MeasureModel>()
           .X(model => model.DateTime.Ticks)   //use DateTime.Ticks as X
           .Y(model => model.Value);           //use the value property as Y

            //lets save the mapper globally.
            Charting.For<MeasureModel>(mapper);

            //the values property will store our values array
            VoltageChartValues = new ChartValues<MeasureModel>(); // For the voltage cartesian graph values
            CurrentChartValues = new ChartValues<MeasureModel>(); // For the current cartesian graph values
            WattChartValues = new ChartValues<MeasureModel>();
            EnergyChartValues = new ChartValues<MeasureModel>();

            //lets set how to display the X Labels
            DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss");

            //AxisStep forces the distance between each separator in the X axis
            AxisStep = TimeSpan.FromSeconds(1).Ticks;
            //AxisUnit forces lets the axis know that we are plotting seconds
            //this is not always necessary, but it can prevent wrong labeling
            AxisUnit = TimeSpan.TicksPerSecond;

            SetAxisLimits(DateTime.Now);
        }

        /// <summary>
        /// Listener for when the window is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            dataPointReceivedCallback = new ViewModelDataPointReceivedCallback(dataPointReceived);
            mainScreenViewModel = new MainScreenViewModel(" ", dataPointReceivedCallback);
            getAllComPorts();
            // FOR TESTING ONLY
            sendSampleVoltageData();         
        }

       /// <summary>
       /// Set the y-axis min and max values for the cartesian live graph
       /// </summary>
       /// <param name="now"></param>
        private void SetAxisLimits(DateTime now) {
            AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; // lets force the axis to be 1 second ahead
            AxisMin = now.Ticks - TimeSpan.FromSeconds(10).Ticks; // and 10 seconds behind
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null) {
            if (PropertyChanged != null) {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            } 
        }
        /// <summary>
        /// CartesianChart x-axis max value
        /// </summary>
        public double AxisMax {
            get { return _axisMax; }
            set {
                _axisMax = value;

                OnPropertyChanged("AxisMax");
            }
        }
        /// <summary>
        /// CartesianChart x-axis min value
        /// </summary>
        public double AxisMin {
            get { return _axisMin; }
            set {
                _axisMin = value;

                OnPropertyChanged("AxisMin");
            }
        }

        /// <summary>
        /// Set the Speed Value for the speed gauge
        /// </summary>
        /// <param name="speed"></param>
        private void setSpeedGaugeValue(int speed) {
            try {
                speedGauge.Value = speed;
            } catch { }
        }

        /// <summary>
        /// Callback method for when PortSelectionComboBox item is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PortSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            comPortStr = ((ComboBox)sender).SelectedItem.ToString();
            Console.WriteLine("TAG - Port " + comPortStr + " selected");
            mainScreenViewModel.comPortName = comPortStr;
        }

        /// <summary>
        /// Get all the COM Ports on the 
        /// </summary>
        private void getAllComPorts() {
            this.portSelectionComboBox.Items.Clear();
            // Get all ports
            string[] allPorts = SerialPort.GetPortNames();
            if (allPorts.Length == 0) {
                this.portSelectionComboBox.Items.Add("No COM Ports");
            } else {  
                // Put all the strings into the combobox
                foreach (string s in allPorts) {
                    this.portSelectionComboBox.Items.Add(s);
                }
            }
        }

        /// <summary>
        /// This method is called by the ViewModel whenever a new data point object is received
        /// </summary>
        /// <param name="baseDataPoint"></param>
        private void dataPointReceived(BaseDataPoint baseDataPoint) {
            if (baseDataPoint is SpeedDataPoint) {
                Console.WriteLine("SpeedDataPoint received by main window");
                updateSpeedWidgets((SpeedDataPoint)baseDataPoint);
            } else if (baseDataPoint is FuelCellDataPoint) {
                Console.WriteLine("FuelCellDataPoint received by main window");
                updateFuelCellWidgets((FuelCellDataPoint)baseDataPoint);
            }
        }
        /// <summary>
        /// Update all the widgets related to the SpeedDataPoint object
        /// </summary>
        /// <param name="speedDataPoint"></param>
        private void updateSpeedWidgets(SpeedDataPoint speedDataPoint) {
            setSpeedGaugeValue((int)speedDataPoint.getSpeed());
        }

        /// <summary>
        /// Update all the widgets related to the FuelCellDataPoint object
        /// </summary>
        /// <param name="fuelCellDataPoint"></param>
        private void updateFuelCellWidgets(FuelCellDataPoint fuelCellDataPoint) {
            DateTime now = DateTime.Now;
            SetAxisLimits(now);
            updateVoltageValues(fuelCellDataPoint.voltage, now);
            updateCurrentValues(fuelCellDataPoint.current, now);
            updateWattValues(fuelCellDataPoint.watt, now);
            updateEnergyValues(fuelCellDataPoint.energy, now); 
        }
        /// <summary>
        ///  Update the voltage cartesian graph
        /// </summary>
        /// <param name="voltageLevel"></param>
        /// <param name="now"></param>
        private void updateVoltageValues(float voltageLevel, DateTime now) {
            VoltageChartValues.Add(new MeasureModel {
                Value = voltageLevel,
                DateTime = now
            });
        }
        /// <summary>
        /// Update the current cartesian graph
        /// </summary>
        /// <param name="currentLevel"></param>
        /// <param name="now"></param>
        private void updateCurrentValues(float currentLevel, DateTime now) {           
            CurrentChartValues.Add(new MeasureModel {
                Value = currentLevel,
                DateTime = now
            });
        }
        /// <summary>
        /// Update the watt cartesian graph
        /// </summary>
        /// <param name="wattLevel"></param>
        /// <param name="now"></param>
        private void updateWattValues(float wattLevel, DateTime now) {
            WattChartValues.Add(new MeasureModel {
                Value = wattLevel,
                DateTime = now
            });
        }
        /// <summary>
        ///  Update the energy cartesian graph
        /// </summary>
        /// <param name="energyLevel"></param>
        /// <param name="now"></param>
        private void updateEnergyValues(float energyLevel, DateTime now) {
            EnergyChartValues.Add(new MeasureModel {
                Value = energyLevel,
                DateTime = now
            });
        }
        /// <summary>
        /// Update the pressure gauge
        /// </summary>
        /// <param name="pressureLevel"></param>
        private void updatePressureValues(float pressureLevel) {
            this.fuelCellPressureGauge.Value = pressureLevel;
        }

        /// <summary>
        /// JUST FOR TESTING
        /// </summary>
        private void sendSampleVoltageData() {
            float[] randomSpeeds = new float[] {
                0f,1f,4f,6f,4f,9f,10f,15f,25f,30f,35f,40f,50f,55f,49f,41f,44f,31f,33f,44f,25f,21f,
                55f,49f,41f,44f,31f,33f,44f,25f,21f,55f,49f,41f,44f,31f,33f,44f,25f,21f
            };

            int i = 0;
        
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Start();
            timer.Tick += (sender, args) => {
                timer.Stop();
                updateFuelCellWidgets(new FuelCellDataPoint(100,randomSpeeds[i], randomSpeeds[i], randomSpeeds[i], randomSpeeds[i]));
                updatePressureValues(randomSpeeds[i]);
                i += 1;
                timer.Start();
            };
        }
    }
}
