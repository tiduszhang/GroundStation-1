using Speedometer.DataPoints;
using Speedometer.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speedometer.Utility {
    /// <summary>
    /// This class has 2 static methods that is responsible to parsing the two string into either a SpeedDataPoint object 
    /// or a FuelCellDataPoint object
    /// </summary>
    class DataParser {
        
        /// <summary>
        /// Converts raw string into a SpeedDataPoint object
        /// </summary>
        /// <param name="rawString"></param>
        /// <returns></returns>
        public static SpeedDataPoint ParseSpeedometerData(string rawString) {

            Console.WriteLine("ParseSpeedometerData() called");

            int timeStamp;
            float speed;
            SpeedDataPoint speedDataPoint;

            if(rawString != null && rawString.Length != 0) {
                // Split up the incoming raw string up at the tab character (should get 3 substrings)
                string[] splitStrings = rawString.Split('\t');

                // Parse the time stamp from string HEX into Int32
                try {
                    timeStamp = Convert.ToInt32(splitStrings[1].Trim(), 16);
                } catch { timeStamp = 0; }

                // Parse the Speed (km/h) string into float
                try {    
                    var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    culture.NumberFormat.NumberDecimalSeparator = "."; // The parse uses the culture settings by default -- some places use comma as seperator like 5,2 to represent 5.2
                    speed = float.Parse(splitStrings[2].Trim(), culture);
                } catch { speed = 0f; }

                speedDataPoint = new SpeedDataPoint(timeStamp, speed);

            } else {
                speedDataPoint = new SpeedDataPoint(0, 0);
            }

            Console.WriteLine("Parsed SpeedDataPoint object, TimeStamp - " + speedDataPoint.getTimeStamp() + ", Speed - " + speedDataPoint.getSpeed());

            return speedDataPoint;
        }

        public static FuelCellDataPoint ParseFuelCellData(string rawString) {

            Console.WriteLine("ParseFuelCellData() called");

            FuelCellDataPoint fuelCellDataPoint = new FuelCellDataPoint(0,0,0,0,0);
            // TODO
            return fuelCellDataPoint;
        }
     
    }
}
