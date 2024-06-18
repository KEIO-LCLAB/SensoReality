using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Oculus.Interaction;
using Scenes.interactables.Sensor;
using Unity.VisualScripting;
using UnityEngine;

namespace Sensor
{
    public abstract class VirtualSensor : PointableElement
    {
        [Tooltip("to visualize the object when it is in active.")]
        [SerializeField] [AllowNull] private GameObject inActiveVisualization;
        [Tooltip("to visualize the object when it is in active.")]
        [SerializeField] [AllowNull] private GameObject preview;
        [Tooltip("to visualize the object when it is selected.")]
        [SerializeField] [AllowNull] private GameObject selectedVisualization;
        [Tooltip("to visualize the object when it is selected.")]
        [SerializeField] [AllowNull] private Rigidbody rigidbody;
        [Tooltip("duration for transform mode. grabbing time less than the duration, the sensor will be in selecting mode.")]
        [SerializeField] private float durationForTransformMode = 1.0f;
        [Tooltip("jitter duration. the sensor will jitter for 1 second to notify the user that the sensor is in transform mode.")]
        [SerializeField] private float jitterDuration = 0.2f;

        protected SensorDataCenter sensorDataCenter => SensorDataCenter.Instance;
        public Rigidbody Rigidbody => rigidbody;
        
        /// <summary>
        /// to check if the sensor is working, if ture the sensor will collect data, otherwise it wont.
        /// </summary>
        private bool isActive = true;
        /// <summary>
        /// Sensor is active or not. if it is not active, it will not collect data.
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (IsActive == value) return;
                isActive = value;
                inActiveVisualization?.SetActive(!value);
                onActiveChanged?.Invoke(value);
            }
        }
        public Action<bool> onActiveChanged;

        /// <summary>
        /// to show the preview of the sensor.
        /// </summary>
        private bool showPreview;
        public bool ShowPreview
        {
            get => showPreview;
            set
            {
                if (ShowPreview == value) return;
                showPreview = value;
                preview?.SetActive(value);
                onShowPreviewChanged?.Invoke(value);
            }
        }

        public Action<bool> onShowPreviewChanged;

        /// <summary>
        /// Only one sensor can be selected at a time.
        /// </summary>
        private static VirtualSensor _SelectedSensor;
        public static VirtualSensor SelectedSensor => _SelectedSensor;
        public bool isSelected
        {
            get => _SelectedSensor == this;
            set
            {
                if (value)
                {
                    if (_SelectedSensor != null)
                    {
                        _SelectedSensor.isSelected = false;
                    }
                    onSelectedChanged?.Invoke(true);
                    selectedVisualization?.SetActive(true);
                    _SelectedSensor = this;
                }
                else if (_SelectedSensor == this)
                {
                    onSelectedChanged?.Invoke(false);
                    selectedVisualization?.SetActive(false);
                    _SelectedSensor = null;
                }
            }
        }
        public Action<bool> onSelectedChanged;


        // run-time
        /// <summary>
        /// it will be used to record the time when the sensor is selected.
        /// -1 means the sensor is not selected.
        /// >=0 means the sensor is selecting.
        /// if selectedTime bigger than durationForTransformMode, the sensor will be in transform mode,
        /// else it will be in selecting mode.
        /// </summary>
        private float selectedTime = -1;
        private SensorPlacement sensorPlacement;
        private Vector3 lastPosition;
        private readonly List<SensorData> _sensorData = new();
        public List<SensorData> Data => _sensorData;
        private bool isRecording;
        public BoneMeshAttachment boneMeshAttachment = null;
    
        protected void AppendData(float time, ISensorData data)
        {
            if (!isRecording) return;
            _sensorData.Add(new SensorData
            {
                time = time,
                sensorID = name,
                data = data
            });
        }
    
        public virtual void ClearData()
        {
            _sensorData.Clear();
        }

        /// <summary>
        /// it will be called if and only if the sensor is working.
        /// </summary>
        public abstract void UpdateWorking(float time, float deltaTime);

        /// <summary>
        /// sensors with the same definition will be saved in the same file.
        /// </summary>
        public abstract ISensorDefinition SensorDefinition();

        public override void ProcessPointerEvent(PointerEvent evt)
        {
            if (evt.Type != PointerEventType.Move || selectedTime >= durationForTransformMode + jitterDuration)
            {
                // if the sensor is in transform mode (after jitter), it will process the Move event.
                base.ProcessPointerEvent(evt);
            }
            base.ProcessPointerEvent(evt);
            if (evt.Type == PointerEventType.Select)
            {
                selectedTime = 0;
                lastPosition = transform.position;
            }
            else if (selectedTime >= 0 && evt.Type == PointerEventType.Unselect)
            {
                if (selectedTime < durationForTransformMode)
                {
                    // selecting mode
                    isSelected = !isSelected;
                }
                selectedTime = -1;
                sensorPlacement = null;
            }
        }

        // protected override void PointableElementUpdated(PointerEvent evt)
        // {
        //     if (evt.Type != PointerEventType.Move || selectedTime >= durationForTransformMode + jitterDuration)
        //     {
        //         // if the sensor is in transform mode (after jitter), it will process the Move event.
        //         base.PointableElementUpdated(evt);
        //     }
        //     // base.PointableElementUpdated(evt);
        // }

        void Update()
        {
            if (boneMeshAttachment != null && sensorDataCenter.alwaysUpdateBoneMeshAttachment)
            {
                boneMeshAttachment.UpdateTransform();
            }
            // sensor is selected
            if (selectedTime >= 0)
            {
                selectedTime += Time.deltaTime;
                if (selectedTime >= durationForTransformMode)
                {
                    // transform mode
                    if (sensorPlacement == null)
                    { // create sensor placement for the first time.
                        sensorPlacement = Rigidbody.AddComponent<SensorPlacement>();
                        sensorPlacement.SetSensor(this, true);
                    }
                    isSelected = false;
                    if (selectedTime <= durationForTransformMode + jitterDuration)
                    {
                        // jitter 1 second to notify the user that the sensor is in transform mode.
                        var value = (durationForTransformMode + jitterDuration - selectedTime) / jitterDuration;
                        transform.position = lastPosition + Vector3.up * (0.007f * Mathf.Sin(value * Mathf.PI * 8));
                    }
                }
            }
        }
    
        void FixedUpdate()
        {
            if (IsActive)
            {
                UpdateWorking(Time.fixedTime, Time.fixedDeltaTime);
            }
        }
    
        public void StartRecording()
        {
            isRecording = true;
        }
    
        public void StopRecording()
        {
            isRecording = false;
        }

        public virtual void ClearSmoothCache()
        {
        
        }

        // public void bindToBone()
        // {
        //     var from = transform.position;
        //     var direction = transform.forward;
        //     var hitResult1 = Physics.Raycast(new Ray(from, direction), out var hit1, 10);
        //     var hitResult2 = Physics.Raycast(new Ray(from, direction * -1) , out var hit2, 10);
        //     var hit = new RaycastHit();
        //     if (hitResult1 && hitResult2)
        //     {
        //         if (hit1.distance < hit2.distance)
        //         {
        //             hit = hit1;
        //         }
        //         else
        //         {
        //             hit = hit2;
        //         }
        //     } else if (hitResult1)
        //     {
        //         hit = hit1;
        //     } else if (hitResult2)
        //     {
        //         hit = hit2;
        //     }
        //     if (hit.distance > 0) {
        //         Debug.DrawLine(hit.point, hit.point + hit.normal * 100, Color.green, 10);
        //         var skinnedMeshRenderer = hit.collider.GetComponentInChildren<SkinnedMeshRenderer>();
        //         if (skinnedMeshRenderer != null && hit.collider is MeshCollider collider)
        //         {
        //             // Cache used values rather than accessing straight from the mesh on the loop below
        //             var sharedMesh = skinnedMeshRenderer.sharedMesh;
        //             var triangles = sharedMesh.triangles;
        //             var verticesIndex = triangles[hit.triangleIndex * 3 + 0];
        //         
        //             var bw = sharedMesh.boneWeights[verticesIndex];
        //             var boneIndex = bw.boneIndex0;
        //             var boneWeight = bw.weight0;
        //             if (bw.weight1 > boneWeight)
        //             {
        //                 boneIndex = bw.boneIndex1;
        //                 boneWeight = bw.weight1;
        //             }
        //             if (bw.weight2 > boneWeight)
        //             {
        //                 boneIndex = bw.boneIndex2;
        //                 boneWeight = bw.weight2;
        //             }
        //             if (bw.weight3 > boneWeight)
        //             {
        //                 boneIndex = bw.boneIndex3;
        //                 boneWeight = bw.weight3;
        //             }
        //             if (boneWeight > 0)
        //             {
        //                 var boneObject = skinnedMeshRenderer.bones[boneIndex].gameObject;
        //                 transform.parent = boneObject.transform;
        //                 boneMeshAttachment = new BoneMeshAttachment(this.gameObject, hit.triangleIndex, boneIndex, skinnedMeshRenderer, collider)
        //                 {
        //                     positionOffset = transform.position - hit.collider.transform.TransformPoint(collider.sharedMesh.vertices[verticesIndex]),
        //                     rotationZ = gameObject.transform.localRotation.eulerAngles.z,
        //                 };
        //             }
        //         }
        //     }
        // }
    }
}
