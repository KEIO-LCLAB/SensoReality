using System;
using System.Collections.Generic;
using System.Linq;
using Animations;
using Scenes.Replay.sensor;
using UnityEngine;
using XCharts.Runtime;

namespace Sensor
{
    public class SensorDataCenter : MonoBehaviour
    {
        public enum UpdateType
        {
            Update,
            FixedUpdate,
            LateUpdate
        }
    
        private static SensorDataCenter INSTANCE;
        public static SensorDataCenter Instance => INSTANCE;
    
        [SerializeField] public int samplingRate = 20; // 20Hz 
        [SerializeField] public UpdateType updateType = UpdateType.FixedUpdate;
        [SerializeField] private LineChart accelerationPreview;
        [SerializeField] private LineChart orientationPreview;
        [SerializeField] public float smoothWindowSize = 0.2f;
        [SerializeField] private int cacheTime = 4;
        [SerializeField] public bool alwaysUpdateBoneMeshAttachment = true;
    
        public float SamplingInterval => 1.0f / samplingRate;
        // run-time
        private readonly List<Tuple<float, Vector3>> accelerationData = new();
        private readonly List<Tuple<float, Vector3>> orientationData = new();
        private bool isSimulating;
        public bool IsSimulating => isSimulating;

        public void Start()
        {
            if (INSTANCE != null)
            {
                Debug.LogError("There are multiple SensorDataCollector in the scene.");
            }
            INSTANCE = this;
        }

        public void UploadIMUData(VirtualSensor virtualSensor, float time, Vector3 angular, Vector3 acceleration)
        {
            if (isSimulating) return;
            accelerationData.Add(new Tuple<float, Vector3>(time, acceleration));
            orientationData.Add(new Tuple<float, Vector3>(time, angular));
            var startTime = accelerationData[0].Item1;
            while (time - startTime > cacheTime)
            {
                accelerationData.RemoveAt(0);
                startTime = accelerationData[0].Item1;
            }
            startTime = orientationData[0].Item1;
            while (time - startTime > cacheTime)
            {
                orientationData.RemoveAt(0);
                startTime = orientationData[0].Item1;
            }
            if (accelerationPreview != null)
            {
                UpdateLineChart(accelerationPreview, accelerationData);
            }
        
            if (orientationPreview != null)
            {
                UpdateLineChart(orientationPreview, orientationData);
            }
        }
  
        private void UpdateLineChart(LineChart lineChart, List<Tuple<float, Vector3>> data) {
            if (lineChart == null) return;
            lineChart.ClearData();
            data.ForEach(item => {
                lineChart.AddXAxisData(item.Item1.ToString());
                lineChart.AddData(0, item.Item2.x);
                lineChart.AddData(1, item.Item2.y);
                lineChart.AddData(2, item.Item2.z);
            });
        }

        public Dictionary<ISensorDefinition, List<SensorData>> SimulateSensorData(IAnimationRuntime animationRuntime, SimulationConfig config)
        {
            isSimulating = true;
            var sensors = animationRuntime.getGameObject().GetComponentsInChildren<VirtualSensor>();
            // prepare for simulation
            foreach (var sensor in sensors)
            {
                sensor.ClearData();
                sensor.ClearSmoothCache();
                sensor.StartRecording();
            }
        
            // simulation
            var animationController = animationRuntime.getAnimationController();
            var lastSpeed = animationController.speed;
            var lastTime = animationController.normalizedTime;
            var lastSamplingRate = samplingRate;
            var lastSmooth = smoothWindowSize;
            var lastAmplitude = animationController.amplitude;
            var lastInitialMotionBones = animationController.GetInitialMotionPose;
        
            var time = 0f;
            var deltaTime = config.deltaTime;
            samplingRate = config.samplingRate;
            smoothWindowSize = config.smoothWindowSize;
            animationController.normalizedTime = config.startTime;
            animationController.speed = config.speed;
            animationController.amplitude = config.amplitude;
            animationController.SetInitialMotionPose(animationController.getPose(config.startTime, true));
            animationRuntime.SetBodyShape(config.bodyShapes);
        
            var currentTime = animationController.normalizedTime;
            while (currentTime < config.endTime)
            {
                time += deltaTime;
                animationController.RunNextFrame(deltaTime);
                foreach (var sensor in sensors)
                {
                    sensor.UpdateWorking(time, deltaTime);
                }
                var normalizedTime = animationController.normalizedTime;
                if (normalizedTime < currentTime)
                {
                    break;
                }
                currentTime = normalizedTime;
            }
        
            animationController.speed = lastSpeed;
            animationController.normalizedTime = lastTime;
            animationController.amplitude = lastAmplitude;
            animationController.SetInitialMotionPose(lastInitialMotionBones);
            samplingRate = lastSamplingRate;
            smoothWindowSize = lastSmooth;

            Dictionary<ISensorDefinition, List<SensorData>> results = new(); 
        
            foreach (var sensorGroup in sensors.GroupBy(sensor => sensor.SensorDefinition()))
            {
                var senorData = sensorGroup.SelectMany(sensor => sensor.Data).OrderBy(data => data.time).ToList();
                if (senorData.Count == 0) continue;
                results[sensorGroup.Key] = senorData;
            }
        
            foreach (var sensor in sensors)
            {
                sensor.StopRecording();
            }
            isSimulating = false;
        
            return results;
        }
    }
}
