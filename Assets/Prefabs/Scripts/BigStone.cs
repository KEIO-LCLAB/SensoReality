using UnityEngine;

namespace Prefabs.Scripts
{
    public class BigStone : MonoBehaviour
    {
        public float SelfRotationSpeed = 10f;
        
        // Update is called once per frame
        void Update()
        {
            transform.Rotate(Vector3.up, SelfRotationSpeed * Time.deltaTime);
        }
    }
}
