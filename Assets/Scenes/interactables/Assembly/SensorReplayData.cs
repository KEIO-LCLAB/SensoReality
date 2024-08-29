using System;
using Oculus.Interaction;
using Sensor;
using UnityEngine;

namespace Scenes.interactables.Assembly
{
    public class SensorReplayData
    {
        public SensorData[] sensorData;
        public String skeletonName;
        public Pose localPose;
        public bool isLeftHand;
        public GameObject prefab;
        
        public SensorReplayData(VirtualSensor sensor, SensorData[] data)
        {
            sensorData = data;
            skeletonName = sensor.transform.parent.name;
            localPose = sensor.transform.GetPose(Space.Self);
            isLeftHand = sensor.controlledHand == SensorAttachable.HandCondition.Right;
            prefab = sensor.Prefab;
        }
    }
}