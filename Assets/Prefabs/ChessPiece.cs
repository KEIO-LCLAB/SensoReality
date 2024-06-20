using System.Collections.Generic;
using UnityEngine;

namespace Prefabs
{
    public class ChessPiece : IPreviewModel
    {
        public List<MeshRenderer> previewRenderers;
        public MeshRenderer chessRenderer;
        
        public override void SetAsPreviewView()
        {
            base.SetAsPreviewView();
            chessRenderer.enabled = false;
            foreach (var meshRenderer in previewRenderers)
            {
                meshRenderer.enabled = true;
            }
        }
    }
}
