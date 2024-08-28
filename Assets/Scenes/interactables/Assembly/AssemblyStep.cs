using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Animations;
using Sensor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.interactables.Assembly
{
    public class AssemblyStep : MonoBehaviour
    {
        public class StepRecord
        {
            public Dictionary<string, SensorData[]> sensorData;
            public HandGestureAnimation gestureAnimation;
        }

        [SerializeField] 
        private TextMeshProUGUI timeText;
        [SerializeField]
        private TextMeshProUGUI numberText;
        [SerializeField]
        private AssemblyConsole console;

        [SerializeField] private Image numberTag;
        [SerializeField] private GameObject actionGroup;
        [SerializeField] private GameObject sensorGroup;
        
        
        // runtime
        [AllowNull]
        private StepRecord _stepRecord;
        public StepRecord Record => _stepRecord;
        private bool isRecording;
        private float recordingTime;
        private bool isSensorView;
        private int stepIndex;
        public int StepIndex
        {
            get => stepIndex;
            set
            {
                stepIndex = value;
                numberText.text = (stepIndex + 1).ToString();
            }
        }

        public AssemblyStep PreviousStep { get; set; }
        public AssemblyStep NextStep { get; set; }

        void Start()
        {
            StepIndex = stepIndex;
            ChangeView(false);
        }
        
        public void SwitchView()
        {
            ChangeView(!isSensorView);
        }
        
        public void ChangeView(bool isSensorView)
        {
            if (_stepRecord == null)
            {
                isSensorView = false;
            }
            this.isSensorView = isSensorView;
            numberTag.color = isSensorView ? Color.green: new Color(206/255f, 207/255f,208/255f);
            actionGroup.SetActive(!isSensorView);
            sensorGroup.SetActive(isSensorView);
        }

        public void SetupRecord(StepRecord record)
        {
            _stepRecord = record;
            var leftPlayer = HandRecordingCenter.Instance.LeftHandAnimationPlayer; 
            leftPlayer.SetAnimation(record.gestureAnimation);
            leftPlayer.PlayAnimation();
            var rightPlayer = HandRecordingCenter.Instance.RightHandAnimationPlayer;
            rightPlayer.SetAnimation(record.gestureAnimation);
            rightPlayer.PlayAnimation();
        }
        
        public void StartRecording()
        {
            isRecording = true;
            SensorDataCenter.Instance.StartRecording();
            HandRecordingCenter.Instance.StartRecording();
            timeText.text = Utils.FormatTime((int)recordingTime);
        }

        public void StopRecording()
        {
            isRecording = false;
            var sensorData = SensorDataCenter.Instance.StopRecording();
            var gestureAnimation = HandRecordingCenter.Instance.StopRecording();
            var record = new StepRecord()
            {
                sensorData = null,
                gestureAnimation = gestureAnimation
            };
            SetupRecord(record);
            ChangeView(true);
        }

        public void RemoveStep()
        {
            console.RemoveStep(this);
        }

        private void Update()
        {
            if (isRecording)
            {
                recordingTime += Time.deltaTime;
                timeText.text = Utils.FormatTime((int)recordingTime);
                // breath light effect
                numberTag.color = Color.Lerp(Color.green, new Color(206/255f, 207/255f,208/255f), 
                    Mathf.PingPong(recordingTime, 1));
            }
        }
    }
}
