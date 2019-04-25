using System.Collections;
using System.Collections.Generic;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR;

public class filmbackCamera : MonoBehaviour
{
    [Header("Camera Settings")]

    [SerializeField]
    Transform cameraOperator;
    [SerializeField]
    PlayableDirector associatedPlayableDirector;
    [SerializeField]
    float defaultFOV = 55f;
    [SerializeField]
    bool leftHanded;

    [Header("Core Components")]
    [SerializeField]
    Camera cameraComponent;
    [SerializeField]
    GameObject cameraGameObject;
    [SerializeField]
    GameObject viewport;
    [SerializeField]
    GameObject fovWheel;
    [SerializeField]
    Transform rightHandRoot;
    Material recordingBulb;
    Material timelineBulb;
    GameObject mainCamera;

    float triggerPress;
    bool lastIsRecording;
    bool isRecording;
    bool lastIsPlayingSequence;
    bool isPlayingSequence;

    bool canRecord = false;

    RecorderController filmbackRecorder;
    RecorderControllerSettings filmbackRecorderSettings;

    AnimationRecorderSettings cameraMovementRecorderSettings;

    void Start()
    {
        VRSafetyCheck();
        filmbackSetup();
    }

    void VRSafetyCheck()
    {
        XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
    }

    void SetupNewRecorder()
    {
        // creating animation recorder and assigning the camera to it
        cameraMovementRecorderSettings = ScriptableObject.CreateInstance("AnimationRecorderSettings") as AnimationRecorderSettings;
        cameraMovementRecorderSettings.animationInputSettings.recursive = false;
        cameraMovementRecorderSettings.animationInputSettings.gameObject = this.gameObject;
        cameraMovementRecorderSettings.animationInputSettings.AddComponentToRecord(typeof(Transform));
        cameraMovementRecorderSettings.animationInputSettings.AddComponentToRecord(typeof(Camera));


        // creating recorder settings and assigning our animation recorder settings to it
        filmbackRecorderSettings = ScriptableObject.CreateInstance("RecorderControllerSettings") as RecorderControllerSettings;
        filmbackRecorderSettings.AddRecorderSettings(cameraMovementRecorderSettings);
        filmbackRecorderSettings.SetRecordModeToManual();

        // creating a new recorder instance with the correct settings
        filmbackRecorder = new RecorderController(filmbackRecorderSettings);

    }

    void filmbackSetup()
    {

        mainCamera = Camera.main.gameObject;
        cameraComponent.fieldOfView = defaultFOV;
        recordingBulb = cameraGameObject.GetComponent<MeshRenderer>().materials[2];
        timelineBulb = cameraGameObject.GetComponent<MeshRenderer>().materials[1];
        recordingBulb.SetColor("_EmissionColor", Color.black);
        timelineBulb.SetColor("_EmissionColor", Color.black);

        if (leftHanded == false)
        {
            viewport.transform.parent = rightHandRoot;
            viewport.transform.localPosition = Vector3.zero;
        }

        canRecord = true;
    }

    void Update()
    {
        lastIsRecording = isRecording;
        lastIsPlayingSequence = isPlayingSequence;


        List<XRNodeState> xrNodes = new List<XRNodeState>();
        InputTracking.GetNodeStates(xrNodes);

        for (int i = 0; i < xrNodes.Count; i++)
        {

            if (leftHanded)
            {
                if (xrNodes[i].nodeType == XRNode.LeftHand)
                {
                    Vector3 newHandPosition;
                    xrNodes[i].TryGetPosition(out newHandPosition);
                    transform.localPosition = newHandPosition;

                    Quaternion newHandRotation;
                    xrNodes[i].TryGetRotation(out newHandRotation);
                    transform.localRotation = newHandRotation;

                    InputDevices.GetDeviceAtXRNode(xrNodes[i].nodeType).TryGetFeatureValue(CommonUsages.trigger, out triggerPress);
                    InputDevices.GetDeviceAtXRNode(xrNodes[i].nodeType).TryGetFeatureValue(CommonUsages.gripButton, out isPlayingSequence);

                    if (triggerPress > 0.6f)
                    {
                        isRecording = true;
                    }
                    else
                    {
                        isRecording = false;
                    }

                }

            }
            else
            {

            }

        }

        if (canRecord)
        {
            if (isRecording == true && lastIsRecording == false)
            {
                startRecording();
            }
            if (lastIsRecording == true && isRecording == false)
            {
                stopRecording();
            }
            if (isPlayingSequence == true && lastIsPlayingSequence == false)
            {
                startTimelinePlayback();
            }
            if (lastIsPlayingSequence == true && isPlayingSequence == false)
            {
                stopTimelinePlayback();
            }
        }

        Vector3 newFocusRotation = new Vector3(0f, 0f, cameraComponent.fieldOfView);
        fovWheel.transform.localEulerAngles = newFocusRotation;



    }

    void startRecording()
    {

        SetupNewRecorder();

        if (filmbackRecorder.IsRecording() == false)
        {
            recordingBulb.SetColor("_EmissionColor", Color.red);
            filmbackRecorder.StartRecording();
        }

    }

    void stopRecording()
    {
        if (filmbackRecorder.IsRecording() == true)
        {
            filmbackRecorder.StopRecording();
            recordingBulb.SetColor("_EmissionColor", Color.black);

        }
    }

    void startTimelinePlayback()
    {
        timelineBulb.SetColor("_EmissionColor", Color.green);
        if (associatedPlayableDirector != null)
        {
            associatedPlayableDirector.Play();
        }

    }

    void stopTimelinePlayback()
    {
        timelineBulb.SetColor("_EmissionColor", Color.black);
        if (associatedPlayableDirector != null)
        {
            associatedPlayableDirector.Pause();
        }
    }
}