using System;
using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Scenes.interactables.Assembly;
using Sensor;
using UnityEngine;

namespace Animations
{
    public class HandAnimationPlayer : MonoBehaviour
    {
        [SerializeField]
        private Handedness handedness;
        [SerializeField] 
        private Transform _root;
        [SerializeField]
        private SkinnedMeshRenderer skinnedMeshRenderer;
        [HideInInspector]
        [SerializeField]
        private List<Transform> _jointTransforms = new();
        public Handedness Handedness => handedness;
        public Transform Root => _root;
        public IList<Transform> Joints => _jointTransforms;
        
        // runtime
        private int lastIndex = -1;
        private HandGestureAnimation _animation;
        private bool _isPlaying;
        private float _time;
        
        private HandGestureAnimation Animation => _animation;

        private Dictionary<VirtualSensor, SensorReplayData> _virtualSensors = new();
        private VirtualSensor selectedSensor;
        
        // Start is called before the first frame update
        void Start()
        {
            skinnedMeshRenderer.enabled = false;
        }
        
        public void SetAnimation(HandGestureAnimation animation)
        {
            _animation = animation;
            lastIndex = -1;
        }
        
        public void AddSensors(List<SensorReplayData> sensors, Action<SensorReplayData, bool> onSensorSelected)
        {
            foreach (var replayData in sensors.Where(replayData => !replayData.isLeftHand || handedness == Handedness.Left))
            {
                foreach (var joint in Joints)
                {
                    if (joint.name != replayData.skeletonName) continue;
                    var sensor = Instantiate(replayData.prefab, joint).GetComponent<VirtualSensor>();
                    sensor.showSelectedVisualization = false;
                    sensor.onSelectedChanged += isSelected =>
                    {
                        if (isSelected)
                        {
                            selectedSensor = sensor;
                            onSensorSelected?.Invoke(replayData, true);
                        }
                    };
                    sensor.transform.SetPose(replayData.localPose, Space.Self);
                    _virtualSensors.Add(sensor, replayData);
                    break;
                }
            }

            if (_virtualSensors.Count > 0)
            {
                _virtualSensors.Keys.First().isSelected = true;
            }
        }
        
        public void updateSensorDataByAnimation()
        {
            var lastTime = _time;
            Pause();
            PlayTo(0);
            foreach (var sensorPair in _virtualSensors)
            {
                var sensor = sensorPair.Key;
                sensor.ClearData();
                sensor.ClearSmoothCache();
                sensor.StartRecording();
            }
            var time = 0f;
            var deltaTime = 1/60f;
            while (time < _animation.Duration)
            {
                PlayTo(time);
                foreach (var sensorPair in _virtualSensors)
                {
                    var sensor = sensorPair.Key;
                    sensor.UpdateWorking(time, deltaTime);
                }
                time += deltaTime;
            }
            foreach (var sensorPair in _virtualSensors)
            {
                var sensor = sensorPair.Key;
                sensor.StopRecording();
                var sensorData = sensor.Data;
                sensorPair.Value.sensorData = sensorData.ToArray();
                sensorPair.Value.localPose = sensor.transform.GetPose(Space.Self);
            }
            PlayTo(lastTime);
            // notify sensor data changed
            if (selectedSensor != null)
            {
                selectedSensor.isSelected = false;
                selectedSensor.isSelected = true;
            }
        }
        
        public void ClearSensors()
        {
            foreach (var sensor in _virtualSensors)
            {
                Destroy(sensor.Key.gameObject);
            }
            _virtualSensors.Clear();
            selectedSensor = null;
        }
        
        public void PlayAnimation()
        {
            Play();
            PlayTo(0);
            skinnedMeshRenderer.enabled = true;
        }
        
        public void StopAnimation()
        {
            Pause();
            skinnedMeshRenderer.enabled = false;
        }

        public void PlayToProgress(float progress)
        {
            PlayTo(progress * _animation?.Duration ?? 0);
        }

        public float GetProgress()
        {
            return _time / (_animation?.Duration ?? 1);
        }

        public void PlayTo(float time)
        {
            if (_animation == null)
                return;
            _time = time;
            var keyFrames = handedness == Handedness.Left ? 
                _animation.LefHandGestureKeyFrames : _animation.RightHandGestureKeyFrames;
            if (keyFrames.Length == 0)
                return;
            // find key frame
            if (lastIndex < 0 || lastIndex >= keyFrames.Length)
                lastIndex = 0;
            if (keyFrames[lastIndex].time > time)
            {
                lastIndex = 0;
            }
            var lastFrame = keyFrames[lastIndex];
            for (var i = lastIndex; i < keyFrames.Length; i++)
            {
                var keyFrame = keyFrames[i];
                if (Mathf.Approximately(keyFrame.time, time))
                {
                    lastFrame = keyFrame;
                    break;
                }

                if (keyFrame.time > time)
                {
                    var t = (time - lastFrame.time) / (keyFrame.time - lastFrame.time);
                    lastFrame = HandGestureKeyFrame.Lerp(lastFrame, keyFrame, t);
                    break;
                } 
                lastFrame = keyFrame;
                lastIndex = i;
            }
            // apply key frame
            _root.localPosition = lastFrame.rootPose.position;
            _root.localRotation = lastFrame.rootPose.rotation;
            for (var i = 0; i < _jointTransforms.Count; i++)
            {
                _jointTransforms[i].localPosition = lastFrame.handJointPoses[i].position;
                _jointTransforms[i].localRotation = lastFrame.handJointPoses[i].rotation;
            }
        }


        public void Play()
        {
            _isPlaying = true;
        }

        public void Pause()
        {
            _isPlaying = false;
        }
        
        // Update is called once per frame
        void Update()
        {
            if (_isPlaying)
            {
                if (_time >= _animation.Duration)
                {
                    PlayTo(0);
                }
                else
                {
                    PlayTo(_time + Time.deltaTime);
                }
            }
        }
    }
}
