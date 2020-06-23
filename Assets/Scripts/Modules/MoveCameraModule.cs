/*
This script manages the dialogue state between Kirby and the user

Reads:      

Writes:     kirby:isAttending:speech (BoolValue, whether or not Kirby is attending
                to human speech
            kirby:isAttending:gesture (BoolValue, whether or not Kirby is attending
                to human gesture               
*/

using UnityEngine;
using System.Timers;

using VoxSimPlatform.Core;
using VoxSimPlatform.Global;

public class MoveCameraModule : ModuleBase
{
    public float rotateSpeed;
    GameObject camera;
    Vector3 cameraLookAt;

    Timer checkServoTimer;
    public int checkServoTimerTime;

    bool rotate;

    // Use this for initialization
    void Start()
    {
        base.Start();

        camera = GameObject.FindGameObjectWithTag("MainCamera");
        if (camera == null)
        {
            Debug.LogError("MoveCameraModule.Start: Could not find main camera.  Expect errors!");
        }

        cameraLookAt = default;

        DataStore.Subscribe("user:intent:isClaw", EnterCameraMoveMode);
        DataStore.Subscribe("user:intent:isPosack", ExitCameraMoveMode);
        DataStore.Subscribe("user:intent:isServoLeft", RotateLeft);
        DataStore.Subscribe("user:intent:isServoRight", RotateRight);
        DataStore.Subscribe("user:intent:isServoBack", RotateBack);
        DataStore.Subscribe("user:intent:isNevermind", ReturnToHome);

        checkServoTimer = new Timer(checkServoTimerTime);
        checkServoTimer.Elapsed += CheckServo;
        checkServoTimer.Enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (rotate)
        {
            checkServoTimer.Interval = checkServoTimerTime;
            checkServoTimer.Enabled = true;

            if (cameraLookAt == default)
            {
                cameraLookAt = camera.transform.position + camera.transform.forward;
            }

            if (DataStore.GetBoolValue("user:intent:isServoLeft"))
            { 
                Debug.Log(string.Format("Rotating camera: Left; look at {0}",
                    GlobalHelper.VectorToParsable(cameraLookAt)));
                camera.transform.LookAt(cameraLookAt);
                camera.transform.Translate(Vector3.left * Time.deltaTime * rotateSpeed);
            }
            else if (DataStore.GetBoolValue("user:intent:isServoRight"))
            {
                Debug.Log(string.Format("Rotating camera: Right; look at {0}",
                    GlobalHelper.VectorToParsable(cameraLookAt)));
                camera.transform.LookAt(cameraLookAt);
                camera.transform.Translate(Vector3.right * Time.deltaTime * rotateSpeed);
            }
            if (DataStore.GetBoolValue("user:intent:isServoBack"))
            {
                Debug.Log(string.Format("Rotating camera: Back; look at {0}",
                    GlobalHelper.VectorToParsable(cameraLookAt)));
                camera.transform.LookAt(cameraLookAt);
                camera.transform.Translate(Vector3.up * Time.deltaTime * rotateSpeed);
            }
        }
        else
        {
            cameraLookAt = default;
        }
    }

    // callback when user:intent:isClaw changes
    void EnterCameraMoveMode(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue(key))
        {
            SetValue("user:isMovingCamera", true, string.Empty);
            Debug.Log("Entering camera move mode");
        }
    }

    // callback when user:intent:isPosack changes
    void ExitCameraMoveMode(string key, DataStore.IValue value)
    {
        if (!DataStore.GetBoolValue(key))
        {
            SetValue("user:isMovingCamera", false, string.Empty);
            Debug.Log("Exiting camera move mode");
        }
    }

    // callback when user:intent:isServoLeft changes
    void RotateLeft(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue("user:isMovingCamera"))
        {
            if (DataStore.GetBoolValue(key))
            {
                checkServoTimer.Enabled = true;
            }
            else
            {
                checkServoTimer.Enabled = false;
                rotate = false;
            }
        }
    }

    // callback when user:intent:isServoRight changes
    void RotateRight(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue("user:isMovingCamera"))
        {
            if (DataStore.GetBoolValue(key))
            {
                checkServoTimer.Enabled = true;
            }
            else
            {
                checkServoTimer.Enabled = false;
                rotate = false;
            }
        }
    }

    // callback when user:intent:isServoBack changes
    void RotateBack(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue("user:isMovingCamera"))
        {
            if (DataStore.GetBoolValue(key))
            {
                checkServoTimer.Enabled = true;
            }
            else
            {
                checkServoTimer.Enabled = false;
                rotate = false;
            }
        }
    }

    // callback when user:intent:isNevermind changes
    void ReturnToHome(string key, DataStore.IValue value)
    {
        if (DataStore.GetBoolValue("user:isMovingCamera"))
        {
            Debug.Log("Returning camera to home");
            GhostFreeRoamCamera cameraMovementScript = camera.GetComponent<GhostFreeRoamCamera>();

            camera.transform.position = cameraMovementScript.cameraPosOrigin;
            camera.transform.rotation = cameraMovementScript.cameraRotOrigin;
        }
    }

    void CheckServo(object sender, ElapsedEventArgs e)
    {
        checkServoTimer.Enabled = false;
        checkServoTimer.Interval = checkServoTimerTime;

        if (DataStore.GetBoolValue("user:intent:isServoLeft") ||
            DataStore.GetBoolValue("user:intent:isServoRight") ||
            DataStore.GetBoolValue("user:intent:isServoBack"))
        {
            rotate = true;
        }
    }
}
