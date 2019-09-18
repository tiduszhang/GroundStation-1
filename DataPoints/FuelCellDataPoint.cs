using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speedometer.DataPoints {
    class FuelCellDataPoint : BaseDataPoint {

        public static string FUEL_CELL_DATA_TYPE_KEY = "FUEL_CELL_DATA_POINT";

        public float voltage { get; private set; }
        public float current { get; private set; }
        public float watt { get; private set; }
        public float energy { get; private set; }
        public float[] temperatures { get; private set; }
        public float pressure { get; private set; }
        public string status { get; private set; }

        public FuelCellDataPoint(ushort timeStamp, float voltage, float current, float watt, float energy, 
            float[] temperatures, float pressure, string status) : base(FUEL_CELL_DATA_TYPE_KEY, timeStamp) {
            this.voltage = voltage;
            this.current = current;
            this.watt = watt;
            this.energy = energy;
            this.temperatures = temperatures;
            this.pressure = pressure;
            this.status = status;
        }
    }
}
