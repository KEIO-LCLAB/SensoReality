using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Animations;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Serialization;

public class HandRecordingCenter : MonoBehaviour
{
    private static HandRecordingCenter INSTANCE;
    public static HandRecordingCenter Instance => INSTANCE;
    
    [SerializeField]
    private HandVisual leftVisualHand;
    [SerializeField]
    private HandVisual rightVisualHand;

    [SerializeField]
    private HandAnimationPlayer leftHandAnimationPlayer;
    [SerializeField]
    private HandAnimationPlayer rightHandAnimationPlayer;
    
    public HandAnimationPlayer LeftHandAnimationPlayer => leftHandAnimationPlayer;
    public HandAnimationPlayer RightHandAnimationPlayer => rightHandAnimationPlayer;
    
    // runtime
    private bool isRecording;
    private float recordingTime;
    private Pose leftHandInitialPose;
    private List<HandGestureKeyFrame> leftHandGestureKeyFrames = new();
    private List<HandGestureKeyFrame> rightHandGestureKeyFrames = new();
    
    void Start()
    {
        StartCoroutine(SnapCanvasInFrontOfCameraCoroutine());
        if (INSTANCE != null)
        {
            Debug.LogError("There are multiple HandRecordingCenter in the scene.");
        }
        INSTANCE = this;
    }

    public void StartRecording()
    {
        isRecording = true;
        leftHandInitialPose = leftVisualHand.Root.GetPose();
        leftHandGestureKeyFrames.Clear();
        rightHandGestureKeyFrames.Clear();
        recordingTime = 0;
        RecordHandsGesture();
    }

    public HandGestureAnimation StopRecording()
    {
        isRecording = false;
        return new HandGestureAnimation(leftHandGestureKeyFrames.ToArray(), rightHandGestureKeyFrames.ToArray());
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isRecording)
        {
            recordingTime += Time.deltaTime;
            RecordHandsGesture();
        }
    }

    private void RecordHandsGesture()
    {
        var leftHandGesture = CaptureHandGesture(leftVisualHand, leftHandInitialPose, recordingTime);
        var rightHandGesture = CaptureHandGesture(rightVisualHand, leftHandInitialPose, recordingTime);
        leftHandGestureKeyFrames.Add(leftHandGesture);
        rightHandGestureKeyFrames.Add(rightHandGesture);
    }

    private HandGestureKeyFrame CaptureHandGesture(HandVisual handVisual, Pose initialPose, float time)
    {
        var handJointPoses = handVisual.Joints.Select(joint => joint.GetPose(Space.Self)).ToList();
        var rootPose = handVisual.Root.GetPose();
        rootPose.position -= initialPose.position;
        return new HandGestureKeyFrame()
        {
            time = time,
            rootPose =  rootPose,
            worldOffset = initialPose.position,
            handJointPoses = handJointPoses
        };
    }

    public void SnapCanvasInFrontOfCamera()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        // var cameraRig = DevicesRef.Instance.CameraRigRef.CameraRig;
        // transform.position = cameraRig.centerEyeAnchor.transform.position +
        //                      cameraRig.centerEyeAnchor.transform.forward * 0.4f;
    }
    
    public IEnumerator SnapCanvasInFrontOfCameraCoroutine()
    {
        yield return 0; // wait one frame to make sure the camera is set up
        SnapCanvasInFrontOfCamera();
    }
}
