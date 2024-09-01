using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using Sensor;
using Unity.VisualScripting;
using UnityEngine;

namespace Scenes.interactables.Sensor
{
    public class SensorPalmPreview : PointableElement, IGameObjectFilter
    {
        [SerializeField]
        private Vector3 _leftAnchorPoint = new Vector3(-0.0608603321f, 0.09f, 0.000258127693f);
        [SerializeField]
        private Vector3 _leftAimPoint = new Vector3(-0.0749258399f, 0.7f, 0.000258127693f);
        [SerializeField]
        private HandGrabInteractable _handGrabInteractable;

        // runtime
        private GameObject sensorPrefab;
        private GameObject sensorPreView;

        public void Setup(GameObject sensor)
        {
            sensorPrefab = sensor;
            sensorPreView = Instantiate(sensor, transform);
            var virtualSensor = sensorPreView.GetComponent<VirtualSensor>();
            _handGrabInteractable.InjectRigidbody(virtualSensor.Rigidbody);
            virtualSensor.ShowPreview = true;
            foreach (var interactable in sensorPreView.GetComponentsInChildren<HandGrabInteractable>())
            {
                Destroy(interactable);
            }
        }
        
        public override void ProcessPointerEvent(PointerEvent evt)
        {
            if (evt.Identifier == DevicesRef.Instance.LeftHandGrabInteractor.Identifier) return;
            if (evt.Type != PointerEventType.Select) return;
            var newSensor = Instantiate(sensorPrefab);
            newSensor.transform.position = sensorPreView.transform.position;
            newSensor.transform.rotation = sensorPreView.transform.rotation;
            if (newSensor.TryGetComponent(out VirtualSensor virtualSensor))
            {
                virtualSensor.prefab = sensorPrefab;
                virtualSensor.Rigidbody.AddComponent<SensorPlacement>().SetSensor(virtualSensor, DevicesRef.Instance.RightHand, true);
                SensorDataCenter.Instance.RegisterSensor(virtualSensor);
            }
            else
            {
                Destroy(newSensor);
            }
            base.ProcessPointerEvent(evt);  
        }

        void Update()
        {
            if (!DevicesRef.Instance.LeftHand.GetJointPose(HandJointId.HandWristRoot, out var wristPose)) return;
            var anchorPose = new Pose(_leftAnchorPoint, Quaternion.identity).GetTransformedBy(wristPose);
            var aimPose = new Pose(_leftAimPoint, Quaternion.identity).GetTransformedBy(wristPose);
            transform.SetPositionAndRotation(anchorPose.position, Quaternion.LookRotation((aimPose.position - anchorPose.position).normalized));
        }

        public bool Filter(GameObject gameObject)
        {
            if (!gameObject.TryGetComponent<HandGrabInteractor>(out var interactor)) return false;
            return interactor.Hand.Handedness == Handedness.Right;
        }
    }
}
