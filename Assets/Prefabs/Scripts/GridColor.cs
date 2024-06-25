using Sensor;
using UnityEngine;
using UnityEngine.UI;

namespace Prefabs.Scripts
{
    public class GridColor : MonoBehaviour
    {
        public VirtualDistanceSensor DistanceSensor;
        public Gradient color;
        public Image image;
        
        // Start is called before the first frame update
        void Start()
        {
            image = GetComponent<Image>();
            image.color = color.Evaluate(0);
        }

        // Update is called once per frame
        void Update()
        {
            if (DistanceSensor == null) return;
            image.color = color.Evaluate(DistanceSensor.Progress);
        }
    }
}
