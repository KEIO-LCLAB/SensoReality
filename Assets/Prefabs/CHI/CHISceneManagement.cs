using System.Collections;
using System.Linq;
using Sensor;
using UnityEngine;

namespace Prefabs.CHI
{
    public class CHISceneManagement : MonoBehaviour
    {
        private static CHISceneManagement INSTANCE;
        public static CHISceneManagement Instance => INSTANCE;
        
        public SceneManager Scene1;
        public SceneManager Scene2;
        
        public void Awake()
        {
            if (INSTANCE != null)
            {
                Debug.LogError("There are multiple CHISceneManagement in the scene.");
            }
            INSTANCE = this;
        }
        
        // Start is called before the first frame update
        IEnumerator Start()
        {
            yield return new WaitForSeconds(1);
            Scene1.SelectScene();
        }

        public void ResetCurrentScene()
        {
            if (SceneManager.SelectedScene != null)
            {
                SceneManager.SelectedScene.Reset();
            }
        }
        
        public void ResetCurrentScenePosition()
        {
            if (SceneManager.SelectedScene != null)
            {
                SceneManager.SelectedScene.ResetPosition();
            }
        }
        
        public void SelectScene1()
        {
            ClearScene();
            Scene1.SelectScene();
        }
        
        public void SelectScene2()
        {
            ClearScene();
            Scene2.SelectScene();
        }
        
        public static void ClearScene()
        {
            HandRecordingCenter.Instance.LeftHandAnimationPlayer.StopAnimation();
            HandRecordingCenter.Instance.LeftHandAnimationPlayer.ClearSensors();
            HandRecordingCenter.Instance.RightHandAnimationPlayer.StopAnimation();
            HandRecordingCenter.Instance.RightHandAnimationPlayer.ClearSensors();
            HandRecordingCenter.Instance.StopRecording();
            foreach (var sensor in SensorDataCenter.Instance.Sensors.Values.SelectMany(sensors => sensors))
            {
                Destroy(sensor.gameObject);
            }
            SensorDataCenter.Instance.Sensors.Clear();
            SensorDataCenter.Instance.StopRecording();
        }
    }
    
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(CHISceneManagement))]
    public class CHISceneManagementEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var management = (CHISceneManagement) target;
            if (GUILayout.Button("Select Scene 1"))
            {
                management.SelectScene1();
            }
            if (GUILayout.Button("Select Scene 2"))
            {
                management.SelectScene2();
            }
        }
    }
#endif
}
