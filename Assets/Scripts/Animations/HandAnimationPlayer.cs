using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Animations
{
    public class HandAnimationPlayer : MonoBehaviour
    {
        [SerializeField]
        private Handedness handedness;
        [SerializeField] 
        private Transform _root;
        [SerializeField]
        private SkinnedMeshRenderer skinnedMeshRenderer;
        [HideInInspector]
        [SerializeField]
        private List<Transform> _jointTransforms = new();
        public Handedness Handedness => handedness;
        public Transform Root => _root;
        public IList<Transform> Joints => _jointTransforms;
        
        // runtime
        private HandGestureAnimation _animation;
        private bool _isPlaying;
        private float _time;
        
        // Start is called before the first frame update
        void Start()
        {
            skinnedMeshRenderer.enabled = false;
        }
        
        public void SetAnimation(HandGestureAnimation animation)
        {
            _animation = animation;
        }
        
        public void PlayAnimation()
        {
            Play();
            PlayTo(0);
            skinnedMeshRenderer.enabled = true;
        }

        public void PlayTo(float time)
        {
            if (_animation == null)
                return;
            _time = time;
            var keyFrames = handedness == Handedness.Left ? 
                _animation.LefHandGestureKeyFrames : _animation.RightHandGestureKeyFrames;
            if (keyFrames.Length == 0)
                return;
            // find key frame
            var lastFrame = keyFrames[0];
            foreach (var keyFrame in keyFrames)
            {
                if (keyFrame.time == time)
                {
                    lastFrame = keyFrame;
                    break;
                }

                if (keyFrame.time > time)
                {
                    var t = (time - lastFrame.time) / (keyFrame.time - lastFrame.time);
                    lastFrame = HandGestureKeyFrame.Lerp(lastFrame, keyFrame, t);
                    break;
                } 
                lastFrame = keyFrame;
            }
            // apply key frame
            _root.localPosition = lastFrame.rootPose.position;
            _root.localRotation = lastFrame.rootPose.rotation;
            for (var i = 0; i < _jointTransforms.Count; i++)
            {
                _jointTransforms[i].localPosition = lastFrame.handJointPoses[i].position;
                _jointTransforms[i].localRotation = lastFrame.handJointPoses[i].rotation;
            }
        }


        public void Play()
        {
            _isPlaying = true;
        }

        public void Pause()
        {
            _isPlaying = false;
        }
        
        // Update is called once per frame
        void Update()
        {
            if (_isPlaying)
            {
                if (_time >= _animation.Duration)
                {
                    PlayTo(0);
                }
                else
                {
                    PlayTo(_time + Time.deltaTime);
                }
            }
        }
    }
}
