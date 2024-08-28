using UnityEngine;

namespace Animations
{
    public class HandGestureAnimation
    {
        private HandGestureKeyFrame[] lefHandGestureKeyFrames;
        private HandGestureKeyFrame[] rightHandGestureKeyFrames;
        private float duration;
        
        public HandGestureKeyFrame[] LefHandGestureKeyFrames => lefHandGestureKeyFrames;
        public HandGestureKeyFrame[] RightHandGestureKeyFrames => rightHandGestureKeyFrames;
        public float Duration => duration;
        
        public HandGestureAnimation(HandGestureKeyFrame[] lefHandGestureKeyFrames, HandGestureKeyFrame[] rightHandGestureKeyFrames)
        {
            this.lefHandGestureKeyFrames = lefHandGestureKeyFrames;
            this.rightHandGestureKeyFrames = rightHandGestureKeyFrames;
            duration = Mathf.Max(lefHandGestureKeyFrames[^1].time, rightHandGestureKeyFrames[^1].time);
        }
    }
}