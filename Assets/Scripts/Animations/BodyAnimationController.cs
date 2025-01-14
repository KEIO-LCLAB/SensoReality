using UnityEngine;

namespace Animations
{
    public class BodyAnimationController : MonoBehaviour
    {
        [SerializeField] private TextAsset animationFile;
        [SerializeField] private float fps = 24;
        [SerializeField] public float speed = 1;
        [SerializeField] public bool globalTranslation = false;
        [SerializeField] public float amplitude = 1f;
        [SerializeField] private SkinCollider skinCollider;
    
        // runtime
        private Transform _root;
        private Transform[] _bones;
        private Quaternion[] _initialBones;
        private Quaternion[] _initialMotionBones;
        private RawAnimation _rawAnimation;
        private float deltaTime
        {
            get
            {
                if (_rawAnimation is { fps: > 0 })
                {
                    return 1f / _rawAnimation.fps;
                }
                return 1f / fps;
            }
        }
    
        public string animationName => _rawAnimation?.name ?? "no_animation";

        private int frameCount => _rawAnimation?.frames.Length ?? 0;
        private AnimationFrame getFrame(int index) => _rawAnimation.frames[index % frameCount];
        public float time { set; get; }
        public float normalizedTime
        {
            get => time / (deltaTime * frameCount);
            set => time = value * (deltaTime * frameCount);
        }
    
        public Quaternion[] GetInitialPose => _initialBones;
        public Quaternion[] GetInitialMotionPose => _initialMotionBones;

        public void setAnimation(string name, string animation)
        {
            setAnimation(new RawAnimation
            {
                name = name,
                fps = -1,
                frames = AnimationUtils.ParseAnimation(animation)
            });
        }
    
        public void setAnimation(RawAnimation animation)
        {
            _rawAnimation = animation;
            SetInitialMotionPose(getPose(0));
        }
        
        public void removeAnimation()
        {
            _rawAnimation = null;
        }
    
        public RawAnimation getAnimation()
        {
            return _rawAnimation;
        }

        public void Start()
        {
            Init();
        }

        public void Init()
        {
            _bones = new Transform[AnimationUtils.BoneNames.Length];
            _root = Utils.FindFirstDeepChild(transform, "f_avg_root");
            if (_root == null)
            {
                _root = Utils.FindFirstDeepChild(transform, "m_avg_root");
            }

            if (_root != null)
            {
                // smpl bones
                var prefix = _root.name.Substring(0, 6);
                for (var i = 0; i < AnimationUtils.BoneNames.Length; i++)
                {
                    var bone = AnimationUtils.DeepFind(_root, prefix + AnimationUtils.BoneNames[i]);
                    if (bone == null)
                    {
                        Debug.LogError("Bone not found: " + AnimationUtils.BoneNames[i]);
                    }
                    _bones[i] = bone;
                }
                _initialBones = GetCurrentPose();
            }
            else
            {
                // banana bones
                _root = Utils.FindFirstDeepChild(transform, "Root");
                if (_root != null)
                {
                    for (var i = 0; i < AnimationUtils.BoneNamesBanana.Length; i++)
                    {
                        var bone = AnimationUtils.DeepFind(_root, AnimationUtils.BoneNamesBanana[i]);
                        if (bone == null)
                        {
                            Debug.LogError("Bone not found: " + AnimationUtils.BoneNamesBanana[i]);
                        }
                        _bones[i] = bone;
                    }
                    _initialBones = GetCurrentPose();
                }
            }
            if (animationFile != null)
            {
                setAnimation(animationFile.name, animationFile.text);
            }
        }

        public void SetInitialMotionPose(Quaternion[] pose)
        {
            _initialMotionBones = pose;
        }

        public Quaternion[] getPose(float animationTime, bool normalize = false)
        {
            if (normalize)
            {
                animationTime *= deltaTime * frameCount;
            }
            var frameIndex = (int) (animationTime / deltaTime);
            var lerp = (animationTime % deltaTime) / deltaTime;
            var lastFrame = getFrame(frameIndex);
            var nextFrame = getFrame(frameIndex + 1);
            var poses = new Quaternion[AnimationUtils.BoneNames.Length];
            for (var i = 0; i < lastFrame.boneRotations.Length; i++)
            {
                var bone = _bones[i];
                if (bone != null)
                {
                    var targetRotation = Quaternion.Slerp(lastFrame.boneRotations[i], nextFrame.boneRotations[i], lerp);
                    poses[i] = targetRotation;
                }
            }
            return poses;
        }
    
        public Quaternion[] GetCurrentPose()
        {
            var poses = new Quaternion[AnimationUtils.BoneNames.Length];
            for (var i = 0; i < _bones.Length; i++)
            {
                var bone = _bones[i];
                if (bone != null)
                {
                    poses[i] = bone.localRotation;
                }
            }
            return poses;
        }
    
        public void SetCurrentPose(Quaternion[] poses)
        {
            if (_root != null)
            {
                for (var i = 0; i < _bones.Length; i++)
                {
                    var bone = _bones[i];
                    if (bone != null)
                    {
                        bone.localRotation = poses[i];
                    }
                }
            }
        }

        void UpdateBodyPose(float animationTime)
        {
            if (_root != null)
            {
                var frameIndex = (int) (animationTime / deltaTime);
                var lerp = (animationTime % deltaTime) / deltaTime;
                var lastFrame = getFrame(frameIndex);
                var nextFrame = getFrame(frameIndex + 1);
                if (globalTranslation)
                {
                    var translation = Vector3.Lerp(lastFrame.translation, nextFrame.translation, lerp) - _rawAnimation.frames[0].translation;
                    // translation = new Vector3(translation.y, translation.x, -translation.z);
                    translation = new Vector3(-translation.x, -translation.y, -translation.z);
                    // translation = new Vector3(-translation.y, translation.z, translation.x);
                    _root.localPosition = translation;
                }
                else
                {
                    _root.localPosition = Vector3.zero;
                }
                for (var i = 0; i < lastFrame.boneRotations.Length; i++)
                {
                    var bone = _bones[i];
                    if (bone != null)
                    {
                        var targetRotation = Quaternion.Slerp(lastFrame.boneRotations[i], nextFrame.boneRotations[i], lerp);
                        if (amplitude is (< 1 or > 1) and >= 0)
                        {
                            targetRotation = Quaternion.Slerp(_initialMotionBones[i], targetRotation, amplitude);
                        }  
                        bone.localRotation = targetRotation;
                    }
                }
                skinCollider?.ScheduleColliderUpdating();
            }
        }    
    
        private void FixedUpdate()
        {
            RunNextFrame(Time.fixedDeltaTime);
        }

        /// <summary>
        /// used for replay, run to the next frame
        /// </summary>
        /// <param name="fixedDeltaTime"></param>
        public void RunNextFrame(float fixedDeltaTime)
        {
            if (_rawAnimation == null || frameCount == 0)
            {
                return;
            }
            time += fixedDeltaTime * speed;
            UpdateBodyPose(time);
        }

    }
}
