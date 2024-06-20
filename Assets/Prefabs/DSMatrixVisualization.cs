using System.Collections.Generic;
using Oculus.Interaction;
using Sensor;
using UnityEngine;
using UnityEngine.UI;

namespace Prefabs
{
    public class DSMatrixVisualization : MonoBehaviour
    {
        [SerializeField, Tooltip("matrix size")] 
        private int size;
        [SerializeField] 
        private GridLayoutGroup GridLayoutGroup;
        [SerializeField] 
        private GridColor gridTemplate;
        [SerializeField] 
        private List<VirtualDistanceSensor> sensors;
        
        private ARCanvasFeature _arCanvasFeature;
        
        // Start is called before the first frame update
        void Start()
        {
            var cellWidth = 400 / size - 5;
            GridLayoutGroup.cellSize = new Vector2(cellWidth, cellWidth);
            GridLayoutGroup.spacing = new Vector2(5, 5);
            for (var i = 0; i < size; i++)
            {
                if (i * size >= sensors.Count) break;
                for (var j = 0; j < size; j++)
                {
                    if (i * size + j >= sensors.Count) break;
                    var grid = Instantiate(gridTemplate, GridLayoutGroup.transform);
                    grid.gameObject.SetActive(true);
                    grid.DistanceSensor = sensors[i * size + j];
                }
            }
            
            _arCanvasFeature = GetComponent<ARCanvasFeature>();
        }
        
    }
}
