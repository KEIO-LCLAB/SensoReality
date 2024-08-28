using System.Collections.Generic;
using UnityEngine;

namespace Scenes.interactables.Assembly
{
    public class AssemblyConsole : MonoBehaviour
    {
        [SerializeField]
        private GameObject contentContainer;
        [SerializeField]
        private AssemblyStep stepPrefab;
        [SerializeField]
        private AssemblyArrow arrowPrefab;
        [SerializeField]
        private GameObject appendStepButton;
        
        // runtime
        private List<AssemblyStep> steps = new();
        public List<AssemblyStep> Steps => steps;
        // arrow dictionary: key is the step, value is the out arrow
        private Dictionary<AssemblyStep, AssemblyArrow> arrows = new();
        public Dictionary<AssemblyStep, AssemblyArrow> Arrows => arrows;

        public void RemoveStep(AssemblyStep step)
        {
            var nextStep = step.NextStep;
            var previousStep = step.PreviousStep;
            if (previousStep != null)
            {
                var previousArrow = arrows[previousStep];
                previousStep.NextStep = nextStep;
                if (nextStep != null)
                {
                    nextStep.PreviousStep = previousStep;
                    previousArrow.setup(previousStep, nextStep);
                    
                }
                else
                {
                    Destroy(previousArrow.gameObject);
                    arrows.Remove(previousStep);
                }
            }
            if (arrows.Remove(step, out var arrow))
            {
                Destroy(arrow.gameObject);
            }
            if (steps.Remove(step))
            {
                for (var i = step.StepIndex; i < steps.Count; i++)
                {
                    steps[i].StepIndex = i;
                }
            }
            Destroy(step.gameObject);
        }
        
        public void AppendLastStep()
        {
            AppendNewStep(null);
        }
        
        public void AppendNewStep(AssemblyStep after = null)
        {
            var newStep = Instantiate(stepPrefab, contentContainer.transform);
            newStep.gameObject.SetActive(true);
            // if there is no arrow after the specified step, set after to null
            if (after != null && Arrows[after] == null)
            {
                after = null;
            }
            if (after == null)
            {
                // add to the end
                steps.Add(newStep);
                newStep.StepIndex = steps.Count - 1;
                var index = appendStepButton.transform.GetSiblingIndex();
                newStep.transform.SetSiblingIndex(index);
                
                var previousStep = steps.Count > 1 ? steps[^2] : null;
                if (previousStep != null)
                {
                    var arrow = Instantiate(arrowPrefab, contentContainer.transform);
                    arrow.gameObject.SetActive(true);
                    arrows.Add(previousStep, arrow);
                    arrow.setup(previousStep, newStep);
                    arrow.transform.SetSiblingIndex(index);
                    previousStep.NextStep = newStep;
                    newStep.PreviousStep = previousStep;
                }
            }
            else
            {
                // add after the specified step
                var nextStep = after.NextStep;
                steps.Insert(after.StepIndex + 1, newStep);
                for (var i = after.StepIndex + 1; i < steps.Count; i++)
                {
                    steps[i].StepIndex = i;
                }
                var index = after.transform.GetSiblingIndex();
                newStep.transform.SetSiblingIndex(index + 2);
                
                var previousArrow = arrows[after];
                previousArrow.setup(after, newStep);
                newStep.PreviousStep = after;
                after.NextStep = newStep;
                
                var arrow = Instantiate(arrowPrefab, contentContainer.transform);
                arrow.gameObject.SetActive(true);
                arrows.Add(newStep, arrow);
                arrow.setup(newStep, nextStep);
                arrow.transform.SetSiblingIndex(index + 3);
                newStep.NextStep = nextStep;
                nextStep.PreviousStep = newStep;
            }
        }
    }
}
