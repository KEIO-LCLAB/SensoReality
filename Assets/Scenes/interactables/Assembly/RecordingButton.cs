using UnityEngine;
using UnityEngine.UI;

namespace Scenes.interactables.Assembly
{
    public class RecordingButton : MonoBehaviour
    {
        [SerializeField]
        private Image recordingImage;
        [SerializeField]
        private GameObject recordingIcon;
        [SerializeField]
        private Button recordingButton;
        [SerializeField]
        private AssemblyStep assemblyStep;
        
        // runtime
        private bool isRecording;
        
        // Start is called before the first frame update
        void Start()
        {
            recordingButton.onClick.AddListener(() =>
            {
                isRecording = !isRecording;
                recordingImage.enabled = !isRecording;
                recordingIcon.SetActive(isRecording);
                if (isRecording)
                {
                    assemblyStep.StartRecording();
                }
                else
                {
                    assemblyStep.StopRecording();
                }
            });
        }

    }
}
