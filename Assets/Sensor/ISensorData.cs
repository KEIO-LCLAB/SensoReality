namespace Sensor
{
    public interface ISensorData
    {
        string ToCsvLine();
    }
    
    public struct SensorData
    {
        public float time;
        public string sensorID;
        public ISensorData data;
        
        public string ToCsvLine()
        {
            return $"{sensorID},{time},{data.ToCsvLine()}";
        }
    }
}