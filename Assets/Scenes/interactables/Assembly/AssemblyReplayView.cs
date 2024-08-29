using System;
using System.Collections.Generic;
using Sensor;
using UnityEngine;
using XCharts.Runtime;

namespace Scenes.interactables.Assembly
{
    public class AssemblyReplayView : MonoBehaviour
    {
        [SerializeField] private GameObject stepView;
        [SerializeField] private LineChart orientationChart;
        [SerializeField] private LineChart accelerationChart;
        
        public void Show()
        {
            gameObject.SetActive(true);
            stepView.SetActive(false);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            stepView.SetActive(true);
        }

        public void SetupRecord(AssemblyStep.StepRecord record)
        {
            var sensor = record.sensors[0];
            var orientationData = new List<Tuple<float, float[]>>();
            var accelerationData = new List<Tuple<float, float[]>>();
            foreach (var sensorData in sensor.sensorData)
            {
                if (sensorData.data is VirtualIMUSensor.IMUSensorData)
                {
                    var imuData = (VirtualIMUSensor.IMUSensorData)sensorData.data;
                    orientationData.Add(new Tuple<float, float[]>(
                        sensorData.time, 
                        new [] {imuData.Orientation.x, imuData.Orientation.y, imuData.Orientation.z}));
                    accelerationData.Add(new Tuple<float, float[]>(
                        sensorData.time,
                        new []{imuData.Acceleration.x, imuData.Acceleration.y, imuData.Acceleration.z}));
                }
            }
            UpdateLineChart(orientationChart, orientationData);
            UpdateLineChart(accelerationChart, accelerationData);
        }
        
        public void UpdateLineChart(LineChart lineChartController, List<Tuple<float, float[]>> data) {
            if (lineChartController == null) return;
            lineChartController.ClearData();
            data.ForEach(item => {
                lineChartController.AddXAxisData(item.Item1.ToString());
                for (var i = 0; i < item.Item2.Length; i++)
                {
                    lineChartController.AddData(i, item.Item2[i]);
                }
            });
        }
    }
}
