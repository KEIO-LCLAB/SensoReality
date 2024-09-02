using System;
using System.Collections.Generic;
using Sensor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XCharts.Runtime;

namespace Scenes.interactables.Assembly
{
    public class AssemblyReplayView : MonoBehaviour
    {
        [SerializeField] private GameObject stepView;
        [SerializeField] private LineChart orientationChart;
        [SerializeField] private LineChart accelerationChart;
        [SerializeField] private Toggle playToggle;
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform left, right, indicator, trimLeft, trimRight;
        [SerializeField] private Toggle trimLeftToggle, trimRightToggle;
        
        // runtime
        private AssemblyStep.StepRecord record;
        
        void Awake()
        {
            playToggle.onValueChanged.AddListener(OnToggleClicked);
            orientationChart.onDrag = OnChartDrag;
            accelerationChart.onDrag = OnChartDrag;
        }
        
        void Start()
        {
            Hide();
        }

        void Update()
        {
            var leftPlayer = HandRecordingCenter.Instance.LeftHandAnimationPlayer; 
            var progress = Mathf.Clamp(leftPlayer.GetProgress(), 0, 1);
            var length = right.anchoredPosition.x - left.anchoredPosition.x;
            indicator.anchoredPosition = new Vector2(left.anchoredPosition.x + length * progress, indicator.anchoredPosition.y);
        }

        private void OnChartDrag(PointerEventData eventData, BaseGraph graph)
        {
            if (record == null) return;
            var leftPlayer = HandRecordingCenter.Instance.LeftHandAnimationPlayer; 
            var rightPlayer = HandRecordingCenter.Instance.RightHandAnimationPlayer;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
                    eventData.position,
                    canvas.worldCamera, out var position)) return;
            var length = right.anchoredPosition.x - left.anchoredPosition.x;
            var leftPos = left.anchoredPosition;
            var progress = Mathf.Clamp((position.x - leftPos.x) * 1f / length, 0, 1);
            playToggle.isOn = false;
            leftPlayer.PlayToProgress(progress);
            rightPlayer.PlayToProgress(progress);
        }

        private void OnToggleClicked(bool isOn)
        {
            if (record == null) return;
            var leftPlayer = HandRecordingCenter.Instance.LeftHandAnimationPlayer; 
            var rightPlayer = HandRecordingCenter.Instance.RightHandAnimationPlayer;
            if (isOn)
            {
                leftPlayer.Play();
                rightPlayer.Play();
            }
            else
            {
                leftPlayer.Pause();
                rightPlayer.Pause();
            }
        }

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

        public void ClearRecord()
        {
            record = null;
            playToggle.isOn = false;
            orientationChart.ClearData();
            accelerationChart.ClearData();
            
            var leftPlayer = HandRecordingCenter.Instance.LeftHandAnimationPlayer; 
            leftPlayer.ClearSensors();
            leftPlayer.StopAnimation();
            var rightPlayer = HandRecordingCenter.Instance.RightHandAnimationPlayer;
            rightPlayer.ClearSensors();
            rightPlayer.StopAnimation();
        }

        public void OnReRecord()
        {
            var leftPlayer = HandRecordingCenter.Instance.LeftHandAnimationPlayer; 
            leftPlayer.updateSensorDataByAnimation();
            var rightPlayer = HandRecordingCenter.Instance.RightHandAnimationPlayer;
            rightPlayer.updateSensorDataByAnimation();
        }

        public void SetupRecord(AssemblyStep.StepRecord stepRecord)
        {
            record = stepRecord;
            playToggle.isOn = true;
            // animation
            HandRecordingCenter.Instance.SnapCanvasInFrontOfCamera();
            var leftPlayer = HandRecordingCenter.Instance.LeftHandAnimationPlayer; 
            leftPlayer.ClearSensors();
            leftPlayer.AddSensors(record.sensors, OnReplaySensorSelected);
            leftPlayer.SetAnimation(record.gestureAnimation);
            leftPlayer.PlayAnimation();
            var rightPlayer = HandRecordingCenter.Instance.RightHandAnimationPlayer;
            rightPlayer.ClearSensors();
            rightPlayer.AddSensors(record.sensors, OnReplaySensorSelected);
            rightPlayer.SetAnimation(record.gestureAnimation);
            rightPlayer.PlayAnimation();
        }

        private void OnReplaySensorSelected(SensorReplayData sensorReplayData, bool isSelected)
        {
            if (isSelected)
            {
                var orientationData = new List<Tuple<float, float[]>>();
                var accelerationData = new List<Tuple<float, float[]>>();
                foreach (var sensorData in sensorReplayData.sensorData)
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
            else
            {
                orientationChart.ClearData();
                accelerationChart.ClearData();
            }
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
