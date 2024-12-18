using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;

namespace ViveTrackersOpenXR
{
    public class ViveTracker : MonoBehaviour
    {
        // Public variables
        public bool IsActive {
            get { return _isActive; }
            private set {
                if (_isActive != value) {
                    _isActive = value;
                    if (TryGetComponent<MeshRenderer>(out var renderer)) {
                        renderer.enabled = value;
                    }
                    if (TryGetComponent<MeshCollider>(out var collider)) {
                        collider.enabled = value;
                    }
                }
            }
        }

        [HideInInspector] public InputActionAsset InputActions;

        public TrackerRole role;
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
        private InputActionMap inputActionMap = null;
        private bool actionMapFound = false;

        private bool _isActive = true;
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

        private void OnEnable() {
            try {
                inputActionMap = InputActions.FindActionMap("Vive Tracker " + role.ToString(), throwIfNotFound: true);
                actionMapFound = true;
            }
            catch {
                actionMapFound = false;
            }

            if (actionMapFound) {
                try {
                    inputActionMap.Enable();

                    inputActionMap["Position"].performed += OnPosition;
                    inputActionMap["Rotation"].performed += OnRotation;
                    inputActionMap["System"].performed += OnSystem;
                    inputActionMap["Menu"].performed += OnMenu;
                    inputActionMap["Trigger"].performed += OnTrigger;
                    inputActionMap["Grip"].performed += OnGrip;
                    inputActionMap["Pad"].performed += OnPad;
                    inputActionMap["PadTouch"].performed += OnPadTouch;
                    inputActionMap["TriggerValue"].performed += OnTriggerValue;
                    inputActionMap["PadXValue"].performed += OnPadXValue;
                    inputActionMap["PadYValue"].performed += OnPadYValue;

                    hapticOutputAction = inputActionMap["Haptics"];
                }
                catch {
                    Debug.LogWarning("Input actions were unable to be correctly assigned for role " + role.ToString() + ". Try disabling and re-enabling the object to try again.");
                }
            }
        }

        private void Update() {
            IsActive = !(_position == Vector3.zero);

            transform.SetLocalPositionAndRotation(_position, _rotation);
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

        private void OnDisable() {
            IsActive = false;

            if (actionMapFound) {
                try {
                    inputActionMap["Position"].performed -= OnPosition;
                    inputActionMap["Rotation"].performed -= OnRotation;
                    inputActionMap["System"].performed -= OnSystem;
                    inputActionMap["Menu"].performed -= OnMenu;
                    inputActionMap["Trigger"].performed -= OnTrigger;
                    inputActionMap["Grip"].performed -= OnGrip;
                    inputActionMap["Pad"].performed -= OnPad;
                    inputActionMap["PadTouch"].performed -= OnPadTouch;
                    inputActionMap["TriggerValue"].performed -= OnTriggerValue;
                    inputActionMap["PadXValue"].performed -= OnPadXValue;
                    inputActionMap["PadYValue"].performed -= OnPadYValue;

                    inputActionMap.Disable();
                }
                catch {
                    Debug.LogWarning("Input actions were unable to be unassigned for role " + role.ToString());
                }
            }
        }

        #region Input Action Callbacks
        /// <summary>
        /// Update transform position
        /// </summary>
        /// <param name="context">Vector3</param>
        private void OnPosition(InputAction.CallbackContext context) {
            _position = context.ReadValue<Vector3>();
        }

        /// <summary>
        /// Update transform orientation
        /// </summary>
        /// <param name="context">Quaternion</param>
        private void OnRotation(InputAction.CallbackContext context) {
            // rotate 180 degrees about y axis
            _rotation = context.ReadValue<Quaternion>() * Quaternion.Euler(0f, 180f, 0);
        }

        /// <summary>
        /// Update system button on top of tracker
        /// May not be accessible by application
        /// </summary>
        /// <param name="context"></param>
        private void OnSystem(InputAction.CallbackContext context) {
            _system = context.ReadValue<bool>();
        }

        /// <summary>
        /// Update menu button
        /// Set via USB or by shorting pogo pin 6 to pogo pin 2
        /// </summary>
        /// <param name="context">bool</param>
        private void OnMenu(InputAction.CallbackContext context) {
            _menu = context.ReadValue<bool>();
        }

        /// <summary>
        /// Update grip button
        /// Set via USB or by shorting pogo pin 3 to pogo pin 2
        /// </summary>
        /// <param name="context">bool</param>
        private void OnGrip(InputAction.CallbackContext context) {
            Debug.Log("grip");
            _grip = context.ReadValue<bool>();
        }

        /// <summary>
        /// Update trigger button
        /// Set via USB or by shorting pogo pin 4 to pogo pin 2
        /// </summary>
        /// <param name="context">bool</param>
        private void OnTrigger(InputAction.CallbackContext context) {
            _trigger = context.ReadValue<bool>();
        }

        /// <summary>
        /// Update trackpad button
        /// Set via USB or by shorting pogo pin 5 to pogo pin 2
        /// </summary>
        /// <param name="context">bool</param>
        private void OnPad(InputAction.CallbackContext context) {
            _pad = context.ReadValue<bool>();
        }

        /// <summary>
        /// Update trackpad touch
        /// Set via USB only
        /// </summary>
        /// <param name="context">bool</param>
        private void OnPadTouch(InputAction.CallbackContext context) {
            _padTouch = context.ReadValue<bool>();
        }

        /// <summary>
        /// Update trigger value
        /// Set via USB only
        /// </summary>
        /// <param name="context">float</param>
        private void OnTriggerValue(InputAction.CallbackContext context) {
            _triggerValue = context.ReadValue<float>();
        }

        /// <summary>
        /// Update trackpad x value
        /// Set via USB only
        /// </summary>
        /// <param name="context">float</param>
        private void OnPadXValue(InputAction.CallbackContext context) {
            _padXValue = context.ReadValue<float>();
        }

        /// <summary>
        /// Update trackpad y value
        /// Set via USB only
        /// </summary>
        /// <param name="context">float</param>
        private void OnPadYValue(InputAction.CallbackContext context) {
            _padYValue = context.ReadValue<float>();
        }
        #endregion

        /// <summary>
        /// Untested haptic output
        /// </summary>
        /// <param name="amplitude">Strength of the vibration. default 10f</param>
        /// <param name="frequency">Frequency/pitch of the vibration in Hz. default 120Hz</param>
        /// <param name="duration">Length of the vibration in seconds. default 0.5</param>
        public void SendHapticImpulse(float amplitude = defaultHapticAmplitude, float frequency = defaultHapticFrequency, float duration = defaultHapticDuration) {
            OpenXRInput.SendHapticImpulse(hapticOutputAction, amplitude, frequency, duration);
            //var command = SendHapticImpulseCommand.Create(0, amplitude, duration);
            //ExecuteCommand(ref command);
        }
    }
}
