using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speedometer.DataPoints {
    abstract class BaseDataPoint {
        private int timeStamp;
        private string dataType;

        public BaseDataPoint(string dataType, int timeStamp) {
            this.timeStamp = timeStamp;
            this.dataType = dataType;
        }

        public int getTimeStamp() {
            return this.timeStamp;
        }

        public string getDataType() {
            return this.dataType;
        }
    }
}
