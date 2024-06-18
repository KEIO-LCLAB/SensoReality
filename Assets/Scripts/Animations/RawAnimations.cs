using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Animations
{
    public struct AnimationFrame
    {
        public Vector3 translation;
        public Quaternion[] boneRotations;
    }
    
    public class RawAnimation
    {
        public string name;
        public float fps;
        public AnimationFrame[] frames;
    }
    
    public static class AnimationUtils
    {
        
        public static readonly string[] BoneNames =
        {
            "Pelvis", // 0
            "L_Hip", // 1
            "R_Hip", // 2
            "Spine1", // 3
            "L_Knee", // 4
            "R_Knee", // 5
            "Spine2", // 6
            "L_Ankle", // 7
            "R_Ankle", // 8
            "Spine3", // 9
            "L_Foot", // 10
            "R_Foot", // 11
            "Neck", // 12
            "L_Collar", // 13
            "R_Collar", // 14
            "Head", // 15
            "L_Shoulder", // 16
            "R_Shoulder", // 17
            "L_Elbow", // 18
            "R_Elbow", // 19
            "L_Wrist", // 20
            "R_Wrist", // 21
            "L_Hand", // 22
            "R_Hand" // 23
        };
            
        public static AnimationFrame[] ParseAnimation(string json)
        {
            var data = JsonConvert.DeserializeObject<JObject>(json);
            var frames = data["animation"].Value<JArray>();
            var frameCount = frames.Count;
            var rawAnimations = new AnimationFrame[frameCount];
            for (var i = 0; i < frameCount; i++)
            {
                var frame = frames[i].Value<JObject>();
                var trans = frame["trans"];
                var rotations = frame["rotations"].Value<JArray>();
                var bones = new Quaternion[rotations.Count];
                for (var j = 0; j < rotations.Count; j++)
                {
                    var rot = rotations[j].Value<JArray>();
                    var angles = new Quaternion(rot[0].Value<float>(), rot[1].Value<float>(), rot[2].Value<float>(), rot[3].Value<float>()).eulerAngles;
                    bones[j] = Quaternion.Euler(angles.x, -angles.y, -angles.z);
                    if (j == 0) // if is root bone
                    {
                        bones[j] = Quaternion.Euler(0, -90, 90) * bones[j];
                    }

                }
                rawAnimations[i] = new AnimationFrame()
                {
                    translation = new Vector3(trans[0].Value<float>(), trans[1].Value<float>(), trans[2].ToObject<float>()),
                    boneRotations = bones
                };
            }
            return rawAnimations;
        }
        
        public static Transform DeepFind(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                var found = DeepFind(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}