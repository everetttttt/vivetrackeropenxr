using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;

// ---------- ----------- ----------- ----------- ----------- //
// NOTE: As of now, only pose data is accessible for vive trackers.
// you can get pogo pin data in, but only if the tracker's role in SteamVR is set to held in hand
// which would then make it register as a right/left hand controller in Unity, not a prop.
// Thus, we are currently getting pogo pin data in from a separate application and sending it to Unity
// Then matching it up with the correct tracker by cross referencing the position data
// we use Unity's pose data because it's much quicker and more accurate than sending over UDP
// ---------- ----------- ----------- ----------- ----------- //

public class ViveTracker : MonoBehaviour
{
    // Public variables
    public InputActionAsset ViveTrackerInputAsset;

    public Role role;
    [Tooltip("May not be available to application")] public bool System;
    public bool Menu;
    public bool Grip;
    public bool Trigger;
    public bool Pad;
    public bool PadTouch;
    public float TriggerValue;
    public float PadXValue;
    public float PadYValue;

    // Private variables
    private InputActionMap inputActions = null;
    private bool actionMapFound = false;

    private Vector3 _position = Vector3.zero;
    private Quaternion _rotation = Quaternion.identity;
    private bool _system = false;
    private bool _menu = false;
    private bool _grip = false;
    private bool _trigger = false;
    private bool _pad = false;
    private bool _padTouch = false;
    private float _triggerValue = 0f;
    private float _padXValue = 0f;
    private float _padYValue = 0f;

    private InputAction hapticOutputAction = null;
    private const float defaultHapticAmplitude = 10f;     // how strong the vibration is
    private const float defaultHapticFrequency = 120f;    // in Hz
    private const float defaultHapticDuration = 0.5f;     // in seconds


    // missing handheld_object, left_wrist, right_wrist, left_ankle, right_ankle
    public enum Role
    {
        LeftFoot,
        RightFoot,
        LeftShoulder,
        RightShoulder,
        LeftElbow,
        RightElbow,
        LeftKnee,
        RightKnee,
        Waist,
        Chest,
        Camera,
        Keyboard
    }


    private void Update() {
        transform.localPosition = _position;
        transform.localRotation = _rotation;
        System = _system;
        Menu = _menu;
        Grip = _grip;
        Trigger = _trigger;
        Pad = _pad;
        PadTouch = _padTouch;
        TriggerValue = _triggerValue;
        PadXValue = _padXValue;
        PadYValue = _padYValue;
    }

    private void OnEnable() {
        try {
            inputActions = ViveTrackerInputAsset.FindActionMap("Vive Tracker " + role.ToString(), true);
            actionMapFound = true;
        }
        catch {
            actionMapFound = false;
            Debug.LogWarning("No input action map was found that matches the role selected!" + Environment.NewLine +
                "Ensure your role is set correctly and that the Input Action Asset is filled out correctly");
        }

        if (actionMapFound) {
            try {
                inputActions.Enable();

                inputActions["Position"].performed += OnPosition;
                inputActions["Rotation"].performed += OnRotation;
                inputActions["System"].performed += OnSystem;
                inputActions["Menu"].performed += OnMenu;
                inputActions["Trigger"].performed += OnTrigger;
                inputActions["Grip"].performed += OnGrip;
                inputActions["Pad"].performed += OnPad;
                inputActions["Pad Touch"].performed += OnPadTouch;
                inputActions["Trigger Value"].performed += OnTriggerValue;
                inputActions["Pad X Value"].performed += OnPadXValue;
                inputActions["Pad Y Value"].performed += OnPadYValue;

                hapticOutputAction = inputActions["Haptics"];
                inputActions["Haptics"].performed += OnHaptics;

            }
            catch {
                Debug.LogWarning("Input actions were unable to be correctly assigned for role " + role.ToString() + ". Try disabling and re-enabling the object to try again.");
            }
        }
    }

    private void OnDisable() {
        if (actionMapFound) {
            try {
                inputActions.Enable();

                inputActions["Position"].performed -= OnPosition;
                inputActions["Rotation"].performed -= OnRotation;
                inputActions["System"].performed -= OnSystem;
                inputActions["Menu"].performed -= OnMenu;
                inputActions["Trigger"].performed -= OnTrigger;
                inputActions["Grip"].performed -= OnGrip;
                inputActions["Pad"].performed -= OnPad;
                inputActions["Pad Touch"].performed -= OnPadTouch;
                inputActions["Trigger Value"].performed -= OnTriggerValue;
                inputActions["Pad X Value"].performed -= OnPadXValue;
                inputActions["Pad Y Value"].performed -= OnPadYValue;
                inputActions["Haptics"].performed -= OnHaptics;
            }
            catch {
                Debug.LogWarning("Input actions were unable to be unassigned for role " + role.ToString());
            }
        }
    }

    private void OnPosition(InputAction.CallbackContext context) {
        _position = context.ReadValue<Vector3>();
    }
    private void OnRotation(InputAction.CallbackContext context) {
        // rotate 180 degrees about y axis
        _rotation = context.ReadValue<Quaternion>() * Quaternion.Euler(0f, 180f, 0);
    }
    private void OnSystem(InputAction.CallbackContext context) {
        _system = context.ReadValue<bool>();
    }
    private void OnMenu(InputAction.CallbackContext context) {
        _menu = context.ReadValue<bool>();
    }
    private void OnGrip(InputAction.CallbackContext context) {
        _grip = context.ReadValue<bool>();
    }
    private void OnTrigger(InputAction.CallbackContext context) {
        _trigger = context.ReadValue<bool>();
    }
    private void OnPad(InputAction.CallbackContext context) {
        _pad = context.ReadValue<bool>();
    }
    private void OnPadTouch(InputAction.CallbackContext context) {
        _padTouch = context.ReadValue<bool>();
    }
    private void OnTriggerValue(InputAction.CallbackContext context) {
        _triggerValue = context.ReadValue<float>();
    }
    private void OnPadXValue(InputAction.CallbackContext context) {
        _padXValue = context.ReadValue<float>();
    }
    private void OnPadYValue(InputAction.CallbackContext context) {
        _padYValue = context.ReadValue<float>();
    }
    private void OnHaptics(InputAction.CallbackContext context) {
        Debug.Log("not sure what to do here, I didn't think this would ever be called");
    }


    /// <summary>
    /// Untested haptic output
    /// </summary>
    /// <param name="amplitude">Strength of the vibration</param>
    /// <param name="frequency">Frequency/pitch of the vibration in Hz</param>
    /// <param name="duration">Length of the vibration in seconds</param>
    public void SendHapticImpulse(float amplitude = defaultHapticAmplitude, float frequency = defaultHapticFrequency, float duration = defaultHapticDuration) {
        OpenXRInput.SendHapticImpulse(hapticOutputAction, amplitude, frequency, duration);
    }
}
