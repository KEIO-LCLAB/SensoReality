using Oculus.Interaction;
using UnityEngine;

public class OnGrabVisulization : PointableElement
{
    [SerializeField] private ARCanvasFeature arCanvasFeature;
    
    public override void ProcessPointerEvent(PointerEvent evt)
    {
        base.ProcessPointerEvent(evt);
        if (evt.Type == PointerEventType.Select)
        {
            if (arCanvasFeature.IsOpen)
            {
                arCanvasFeature.CloseCanvas();
            }
            else
            {
                arCanvasFeature.OpenCanvas();
            }
        }
    }
}
