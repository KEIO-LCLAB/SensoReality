using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;

namespace Scenes.interactables.Assembly
{
    public class AssemblyStep : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI numberText;
        [SerializeField]
        private AssemblyConsole console;
        
        // runtime
        private int stepIndex;
        public int StepIndex
        {
            get => stepIndex;
            set
            {
                stepIndex = value;
                numberText.text = (stepIndex + 1).ToString();
            }
        }

        public AssemblyStep PreviousStep { get; set; }
        public AssemblyStep NextStep { get; set; }

        void Start()
        {
            StepIndex = stepIndex;
        }

    }
}
