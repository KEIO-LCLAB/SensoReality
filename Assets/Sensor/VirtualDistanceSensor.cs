using Sensor.visualization;
using UnityEngine;

namespace Sensor
{
    public class VirtualDistanceSensor : VirtualSensor
    {
        public static readonly ISensorDefinition DEFINITION = ISensorDefinition.create("DISTANCE", "d");
 
        [SerializeField] private float validDistance = 2;
        [SerializeField] private BarChart barChart;
        [SerializeField] private GameObject indicatorLine;
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private Transform YAxis;
        [SerializeField] private Transform XAxis;

        public float YRotation
        {
            get => YAxis.rotation.eulerAngles.y;
            set => YAxis.rotation = Quaternion.Euler(0, value, 0);
        }
        public float XRotation 
        {
            get => XAxis.rotation.eulerAngles.x;
            set => XAxis.rotation = Quaternion.Euler(value, 0, 0);
        }
        
        public Vector3 StartPoint => startPoint.position;

        public Vector3 LookDirection
        {
            get
            {
                var direction = endPoint.position - startPoint.position;
                direction.Normalize();
                return direction;
            }
        }

        protected override void Start()
        {
            base.Start();
            barChart.ProgressTextFormatter = f => (f >= 1 ? ">= " + validDistance.ToString("F2") : (f * validDistance).ToString("F2")) + "m";
            onShowPreviewChanged += showPreview => { indicatorLine?.SetActive(showPreview); };
            indicatorLine.SetActive(ShowPreview);
        }

        public override void UpdateWorking(float time, float deltaTime)
        {
            // calculate distance on the fly
            var eyePosition = StartPoint;
            var lookingForward = LookDirection;
            var ray = new Ray(eyePosition, lookingForward);
            if (Physics.Raycast(ray, out var hit, validDistance))
            {
                if (isSelected)
                {
                    Debug.DrawRay(eyePosition, lookingForward * hit.distance, Color.green, 0.1f);
                }
                barChart.progress = hit.distance / validDistance;
            }
            else
            {
                barChart.progress = 1;
            }
            indicatorLine.transform.localScale = new Vector3(1F, 1F, 333.333f * barChart.progress * validDistance);
        }

        public override ISensorDefinition SensorDefinition()
        {
            return DEFINITION;
        }
    }
}