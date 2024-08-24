using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using Sensor;
using UnityEngine;

public class HandAttachable : MonoBehaviour
{
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
            }
        };
    }
}
