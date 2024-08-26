using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.interactables.Assembly
{
    public class AssemblyArrow : MonoBehaviour
    {
        [SerializeField] private AssemblyConsole console;
        [SerializeField] private Button _button;
        
        // runtime
        private AssemblyStep previousStep;
        private AssemblyStep nextStep;
        public AssemblyStep PreviousStep => previousStep;
        public AssemblyStep NextStep => nextStep;

        private void Start()
        {
            _button.onClick.AddListener(() =>
            {
                if (previousStep == null || nextStep == null) return;
                console.AppendNewStep(previousStep);
            });
        }

        public void setup(AssemblyStep previous, AssemblyStep next)
        {
            previousStep = previous;
            nextStep = next;
        }
    }
}
