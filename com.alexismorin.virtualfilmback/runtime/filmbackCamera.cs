using System.Collections;
using System.Collections.Generic;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR;

public class filmbackCamera : MonoBehaviour {
    [Header ("Camera Settings")]

    [SerializeField]
    PlayableDirector associatedPlayableDirector;
    [SerializeField]
    float defaultFOV = 55f;
    [SerializeField]
    bool leftHanded;

    [Header ("Core Components")]
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

    void Start () {
        filmbackSetup ();
    }

    void VRSafetyCheck () {

    }

    void filmbackSetup () {
        mainCamera = Camera.main.gameObject;
        cameraComponent.fieldOfView = defaultFOV;
        recordingBulb = cameraGameObject.GetComponent<MeshRenderer> ().materials[2];
        timelineBulb = cameraGameObject.GetComponent<MeshRenderer> ().materials[1];
        recordingBulb.SetColor ("_EmissionColor", Color.black);
        timelineBulb.SetColor ("_EmissionColor", Color.black);

        if (leftHanded == false) {
            viewport.transform.parent = rightHandRoot;
            viewport.transform.localPosition = Vector3.zero;
        }

        VRSafetyCheck ();
    }

    void Update () {

        Vector3 viewportTargetLookat = new Vector3 (viewport.transform.localPosition.x, mainCamera.transform.position.y, mainCamera.transform.position.z);
        viewport.transform.LookAt (viewportTargetLookat);

        Vector3 newFocusRotation = new Vector3 (0f, 0f, cameraComponent.fieldOfView);
        fovWheel.transform.localEulerAngles = newFocusRotation;
    }

    void startRecording () {
        recordingBulb.SetColor ("_EmissionColor", Color.red);
    }

    void stopRecording () {
        recordingBulb.SetColor ("_EmissionColor", Color.black);
    }

    void startTimelinePlayback () {
        timelineBulb.SetColor ("_EmissionColor", Color.green);
        associatedPlayableDirector.Play ();
    }

    void stopTimelinePlayback () {
        timelineBulb.SetColor ("_EmissionColor", Color.black);
        associatedPlayableDirector.Pause ();
    }
}