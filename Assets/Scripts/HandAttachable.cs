using Oculus.Interaction;
using Oculus.Interaction.Input;
using Sensor;
using UnityEngine;

public class HandAttachable : MonoBehaviour
{
    [SerializeField] 
    private HandVisual handVisual;
    [SerializeField]
    private GameObject handAttachModel;
    [SerializeField]
    private HandPhysicsCapsules handPhysics;
    [SerializeField]
    private SensorAttachable.HandCondition handCondition = SensorAttachable.HandCondition.Both;
    
    // Start is called before the first frame update
    void Start()
    {
        handPhysics.WhenCapsulesGenerated += () =>
        {
            foreach (var capsule in handPhysics.Capsules)
            {
                var sensorAttachable = capsule.CapsuleCollider.gameObject.AddComponent<SensorAttachable>();
                SetupParentSkeleton(sensorAttachable);
                sensorAttachable.HoverEffect = handAttachModel;
                sensorAttachable.handCondition = 
                    handCondition switch
                    {
                        SensorAttachable.HandCondition.Left => SensorAttachable.HandCondition.Right,
                        SensorAttachable.HandCondition.Right => SensorAttachable.HandCondition.Left,
                        SensorAttachable.HandCondition.Both => SensorAttachable.HandCondition.None,
                        SensorAttachable.HandCondition.None => SensorAttachable.HandCondition.Both,
                        _ => handCondition
                    };
                sensorAttachable.controlledHand = sensorAttachable.handCondition;
            }
        };
    }

    private void SetupParentSkeleton(SensorAttachable attachable)
    {
        var colliderName = attachable.transform.parent.name;
        attachable.Parent = colliderName switch
        {
            "HandStart Rigidbody" => handVisual.Joints[(int)HandJointId.HandWristRoot].gameObject,
            "HandThumb0 Rigidbody" => handVisual.Joints[(int)HandJointId.HandThumb0].gameObject,
            "HandThumb1 Rigidbody" => handVisual.Joints[(int)HandJointId.HandThumb1].gameObject,
            "HandThumb2 Rigidbody" => handVisual.Joints[(int)HandJointId.HandThumb2].gameObject,
            "HandThumb3 Rigidbody" => handVisual.Joints[(int)HandJointId.HandThumb3].gameObject,
            "HandIndex1 Rigidbody" => handVisual.Joints[(int)HandJointId.HandIndex1].gameObject,
            "HandIndex2 Rigidbody" => handVisual.Joints[(int)HandJointId.HandIndex2].gameObject,
            "HandIndex3 Rigidbody" => handVisual.Joints[(int)HandJointId.HandIndex3].gameObject,
            "HandMiddle1 Rigidbody" => handVisual.Joints[(int)HandJointId.HandMiddle1].gameObject,
            "HandMiddle2 Rigidbody" => handVisual.Joints[(int)HandJointId.HandMiddle2].gameObject,
            "HandMiddle3 Rigidbody" => handVisual.Joints[(int)HandJointId.HandMiddle3].gameObject,
            "HandRing1 Rigidbody" => handVisual.Joints[(int)HandJointId.HandRing1].gameObject,
            "HandRing2 Rigidbody" => handVisual.Joints[(int)HandJointId.HandRing2].gameObject,
            "HandRing3 Rigidbody" => handVisual.Joints[(int)HandJointId.HandRing3].gameObject,
            "HandPinky0 Rigidbody" => handVisual.Joints[(int)HandJointId.HandPinky0].gameObject,
            "HandPinky1 Rigidbody" => handVisual.Joints[(int)HandJointId.HandPinky1].gameObject,
            "HandPinky2 Rigidbody" => handVisual.Joints[(int)HandJointId.HandPinky2].gameObject,
            "HandPinky3 Rigidbody" => handVisual.Joints[(int)HandJointId.HandPinky3].gameObject,
            _ => null
        };
    }
}
