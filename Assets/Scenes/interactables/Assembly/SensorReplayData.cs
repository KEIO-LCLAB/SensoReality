using System;
using Oculus.Interaction;
using OVRSimpleJSON;
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
            prefab = sensor.prefab;
        }

        public JSONObject serialize(float leftTrimProgress = 0, float rightTrimProgress = 1)
        {
            var json = new JSONObject();
            json["skeletonName"] = skeletonName;
            json["hand"] = isLeftHand ? "left" : "right";
            json["localPose"] = Utils.SerializePose(localPose);
            var sensorDataArray = new JSONArray();
            var maxTime = sensorData[^1].time;
            var minTime = sensorData[0].time;
            var duration = maxTime - minTime;
            foreach (var data in sensorData)
            {
                if (data.time < minTime + duration * leftTrimProgress || data.time > minTime + duration * rightTrimProgress)
                {
                    continue;
                }
                sensorDataArray.Add(data.serialize());
            }
            json["sensorData"] = sensorDataArray;
            return json;
        }
    }
}