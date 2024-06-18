using UnityEngine;

public abstract class IPreviewModel : MonoBehaviour
{
    public abstract void SetAsPreviewView();

    public virtual bool CanBePlacedAt(Ray ray, RaycastHit hit)
    {
        return true;
    }
    
    public virtual Pose GetPlacementPose(Ray ray, RaycastHit hit)
    {
        return new Pose(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
    }
}
