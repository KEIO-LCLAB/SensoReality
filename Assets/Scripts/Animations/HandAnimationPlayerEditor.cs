using Oculus.Interaction.Input;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Animations
{
    [CustomEditor(typeof(HandAnimationPlayer))]
    public class HandAnimationPlayerEditor: Editor
    {
        private SerializedProperty _rootProperty;

        private void OnEnable()
        {
            _rootProperty = serializedObject.FindProperty("_root");
        }

        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject);
            serializedObject.ApplyModifiedProperties();

            var player = (HandAnimationPlayer)target;
            InitializeSkeleton(player);

            if (GUILayout.Button("Auto Map Joints"))
            {
                AutoMapJoints(player);
                EditorUtility.SetDirty(player);
                EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
            }

            EditorGUILayout.LabelField("Joints", EditorStyles.boldLabel);
            HandJointId start = HandJointId.HandStart;
            HandJointId end = HandJointId.HandEnd;

            for (int i = (int)start; i < (int)end; ++i)
            {
                string jointName = HandJointLabelFromJointId((HandJointId)i);
                player.Joints[i] = (Transform)EditorGUILayout.ObjectField(jointName,
                    player.Joints[i], typeof(Transform), true);
            }
        }

        private static readonly string[] _fbxHandSidePrefix = { "l_", "r_" };
        private static readonly string _fbxHandBonePrefix = "b_";

        private static readonly string[] _fbxHandBoneNames =
        {
            "wrist",
            "forearm_stub",
            "thumb0",
            "thumb1",
            "thumb2",
            "thumb3",
            "index1",
            "index2",
            "index3",
            "middle1",
            "middle2",
            "middle3",
            "ring1",
            "ring2",
            "ring3",
            "pinky0",
            "pinky1",
            "pinky2",
            "pinky3"
        };

        private static readonly string[] _fbxHandFingerNames =
        {
            "thumb",
            "index",
            "middle",
            "ring",
            "pinky"
        };

        private void InitializeSkeleton(HandAnimationPlayer player)
        {
            if (player.Joints.Count == 0)
            {
                for (int i = (int)HandJointId.HandStart; i < (int)HandJointId.HandEnd; ++i)
                {
                    player.Joints.Add(null);
                }
            }
        }

        private void AutoMapJoints(HandAnimationPlayer player)
        {
            InitializeSkeleton(player);

            Transform rootTransform = player.transform;
            if (_rootProperty.objectReferenceValue != null)
            {
                rootTransform = _rootProperty.objectReferenceValue as Transform;
            }

            for (int i = (int)HandJointId.HandStart; i < (int)HandJointId.HandEnd; ++i)
            {
                string fbxBoneName = FbxBoneNameFromHandJointId(player, (HandJointId)i);
                Transform t = rootTransform.FindChildRecursive(fbxBoneName);
                player.Joints[i] = t;
            }
        }

        private string FbxBoneNameFromHandJointId(HandAnimationPlayer player, HandJointId handJointId)
        {
            if (handJointId >= HandJointId.HandThumbTip && handJointId <= HandJointId.HandPinkyTip)
            {
                return _fbxHandSidePrefix[(int)player.Handedness] + _fbxHandFingerNames[(int)handJointId - (int)HandJointId.HandThumbTip] + "_finger_tip_marker";
            }
            else
            {
                return _fbxHandBonePrefix + _fbxHandSidePrefix[(int)player.Handedness] + _fbxHandBoneNames[(int)handJointId];
            }
        }

        // force aliased enum values to the more appropriate value
        private static string HandJointLabelFromJointId(HandJointId handJointId)
        {
            switch (handJointId)
            {
                case HandJointId.HandWristRoot:
                    return "HandWristRoot";
                case HandJointId.HandForearmStub:
                    return "HandForearmStub";
                case HandJointId.HandThumb0:
                    return "HandThumb0";
                case HandJointId.HandThumb1:
                    return "HandThumb1";
                case HandJointId.HandThumb2:
                    return "HandThumb2";
                case HandJointId.HandThumb3:
                    return "HandThumb3";
                case HandJointId.HandIndex1:
                    return "HandIndex1";
                case HandJointId.HandIndex2:
                    return "HandIndex2";
                case HandJointId.HandIndex3:
                    return "HandIndex3";
                case HandJointId.HandMiddle1:
                    return "HandMiddle1";
                case HandJointId.HandMiddle2:
                    return "HandMiddle2";
                case HandJointId.HandMiddle3:
                    return "HandMiddle3";
                case HandJointId.HandRing1:
                    return "HandRing1";
                case HandJointId.HandRing2:
                    return "HandRing2";
                case HandJointId.HandRing3:
                    return "HandRing3";
                case HandJointId.HandPinky0:
                    return "HandPinky0";
                case HandJointId.HandPinky1:
                    return "HandPinky1";
                case HandJointId.HandPinky2:
                    return "HandPinky2";
                case HandJointId.HandPinky3:
                    return "HandPinky3";
                case HandJointId.HandThumbTip:
                    return "HandThumbTip";
                case HandJointId.HandIndexTip:
                    return "HandIndexTip";
                case HandJointId.HandMiddleTip:
                    return "HandMiddleTip";
                case HandJointId.HandRingTip:
                    return "HandRingTip";
                case HandJointId.HandPinkyTip:
                    return "HandPinkyTip";
                default:
                    return "HandUnknown";
            }
        }

    }
}