using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speedometer.DataPoints {
    abstract class BaseDataPoint {
        private int _timeStamp;
        public int timeStamp { get { return _timeStamp; } set { if (value < 0) _timeStamp = 0; else _timeStamp = value; } }
        private string dataType;

        public BaseDataPoint(string dataType, int timeStamp) {
            this.timeStamp = timeStamp;
            this.dataType = dataType;
        }

        public int getTimeStamp() {
            return this._timeStamp;
        }

        public string getDataType() {
            return this.dataType;
        }
    }
}
