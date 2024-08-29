using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
            public List<SensorReplayData> sensors;
            public HandGestureAnimation gestureAnimation;
        }

        [SerializeField] private AssemblyReplayView replayView;
        
        [SerializeField] 
        private TextMeshProUGUI timeText;
        [SerializeField]
        private TextMeshProUGUI numberText;
        [SerializeField]
        private AssemblyConsole console;
        [SerializeField]
        private Button replayButton;
        [SerializeField] private Image numberTag;
        [SerializeField] private GameObject actionGroup;
        
        
        // runtime
        [AllowNull]
        private StepRecord _stepRecord;
        public StepRecord Record => _stepRecord;
        public bool HasRecord => _stepRecord != null;
        private bool isRecording;
        private float recordingTime;
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
            replayButton.onClick.AddListener(ReplayRecord);
        }

        public void ReplayRecord()
        {
            if (HasRecord)
            {
                replayView.Show();
                replayView.SetupRecord(_stepRecord);
            }
        }

        public void SetupRecord(StepRecord record)
        {
            replayButton.gameObject.SetActive(true);
            _stepRecord = record;
            HandRecordingCenter.Instance.SnapCanvasInFrontOfCamera();
            var leftPlayer = HandRecordingCenter.Instance.LeftHandAnimationPlayer; 
            leftPlayer.ClearSensors();
            leftPlayer.AddSensors(record.sensors);
            leftPlayer.SetAnimation(record.gestureAnimation);
            leftPlayer.PlayAnimation();
            var rightPlayer = HandRecordingCenter.Instance.RightHandAnimationPlayer;
            rightPlayer.ClearSensors();
            rightPlayer.AddSensors(record.sensors);
            rightPlayer.SetAnimation(record.gestureAnimation);
            rightPlayer.PlayAnimation();
        }
        
        public void StartRecording()
        {
            numberTag.color = Color.green;
            isRecording = true;
            SensorDataCenter.Instance.StartRecording();
            HandRecordingCenter.Instance.StartRecording();
            timeText.text = Utils.FormatTime((int)recordingTime);
        }

        public void StopRecording()
        {
            isRecording = false;
            var sensorData = SensorDataCenter.Instance.StopRecording();
            var sensors = sensorData.Select(pair => new SensorReplayData(pair.Key, pair.Value.ToArray())).ToList();
            var gestureAnimation = HandRecordingCenter.Instance.StopRecording();
            var record = new StepRecord()
            {
                sensors = sensors,
                gestureAnimation = gestureAnimation
            };
            SetupRecord(record);
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
