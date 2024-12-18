using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;



#if UNITY_EDITOR
using UnityEditor;
#endif

using PoseControl = UnityEngine.InputSystem.XR.PoseControl;


/// OpenXR HTC vive tracker interaction specification
///     <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_HTCX_vive_tracker_interaction"/>
/// original HTC Vive Tracker Interaction Profile with only device Pose
///     <see href="https://discussions.unity.com/t/openxr-and-openvr-together/841675/21"/>
/// Vive's official OpenXR interaction profile for the Ultimate Tracker
///     <see href="https://github.com/ViveSoftware/VIVE-OpenXR/blob/5ac252bf2edfcbc04d952e183bfaa49d35534b2e/com.htc.upm.vive.openxr/Runtime/Features/Tracker/XR/Scripts/ViveXRTracker.cs"/>

namespace UnityEngine.XR.OpenXR.Features.Interactions
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of the HTC Vive Tracker interaction profiles in OpenXR
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(
        UiName = "HTC Vive Tracker OpenXR Profile",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA },
        Company = "MASSIVE",
        Desc = "Support for enabling the Vive XR Tracker interaction profile. Registers the controller map for tracker if enabled.",
        DocumentationLink = Constants.k_DocumentationManualURL,
        OpenxrExtensionStrings = extensionName,
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = featureId,
        Version = "0.0.1")]
#endif


    public class ViveTrackerOpenXRProfile : OpenXRInteractionFeature
    {
        /// <summary>
        /// OpenXR specification that supports the vive tracker
        /// <see href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_HTCX_vive_tracker_interaction"/>
        /// </summary>
        public const string extensionName = "XR_HTCX_vive_tracker_interaction";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference
        /// </summary>
        public const string featureId = "com.massive.openxr.feature.input.htcvivetracker";

        /// <summary>
        /// The interaction profile path for the HTC Vive Tracker in OpenXR
        /// </summary>
        public const string profile = "/interaction_profiles/htc/vive_tracker_htcx";

        /// <summary>
        /// 
        /// </summary>
        private const string kDeviceLocalizedName = "HTC Vive Tracker OpenXR";

        /// <summary>
        /// OpenXR user path definitions for XR_HTCX_vive_tracker_interaction
        /// </summary>
        public struct TrackerUserPaths
        {
            // if you add the below missing paths, also add the path name to commonUsages of XRViveTracker
            // as of the creation of this script, left_wrist, right_wrist, left_ankle, and right_ankle are not supported by SteamVR

            // missing XR_NULL_PATH
            // missing handheld_object
            public const string leftFoot = "/user/vive_tracker_htcx/role/left_foot";
            public const string rightFoot = "/user/vive_tracker_htcx/role/right_foot";
            public const string leftShoulder = "/user/vive_tracker_htcx/role/left_shoulder";
            public const string rightShoulder = "/user/vive_tracker_htcx/role/right_shoulder";
            public const string leftElbow = "/user/vive_tracker_htcx/role/left_elbow";
            public const string rightElbow = "/user/vive_tracker_htcx/role/right_elbow";
            public const string leftKnee = "/user/vive_tracker_htcx/role/left_knee";
            public const string rightKnee = "/user/vive_tracker_htcx/role/right_knee";
            public const string leftWrist = "/user/vive_tracker_htcx/role/left_wrist";
            public const string rightWrist = "/user/vive_tracker_htcx/role/right_wrist";
            public const string leftAnkle = "/user/vive_tracker_htcx/role/left_ankle";
            public const string rightAnkle = "/user/vive_tracker_htcx/role/right_ankle";
            public const string waist = "/user/vive_tracker_htcx/role/waist";
            public const string chest = "/user/vive_tracker_htcx/role/chest";
            public const string camera = "/user/vive_tracker_htcx/role/camera";
            public const string keyboard = "/user/vive_tracker_htcx/role/keyboard";
        }

        /// <summary>
        /// OpenXR component path definitions for XR_HTCX_vive_tracker_interaction.
        /// Component paths are used by input subsystem to bind physical inputs to actions.
        /// </summary>
        public struct TrackerComponentPaths
        {
            // type PoseControl
            public const string devicepose = "/input/grip/pose";

            // type ButtonControl
            public const string system = "/input/system/click"; // may not be available for application use
            public const string menu = "/input/menu/click";
            public const string grip = "/input/squeeze/click";
            public const string trigger = "/input/trigger/click";
            public const string pad = "/input/trackpad/click";
            public const string padTouch = "/input/trackpad/touch";

            // type AxisControl
            public const string triggerValue = "/input/trigger/value";
            public const string padXValue = "/input/trackpad/x";
            public const string padYValue = "/input/trackpad/y";

            //type HapticControl
            public const string haptic = "/output/haptic";
        }

        /// <summary>
        /// A set of bit flags describing XR.InputDevice characteristics
        /// </summary>
        [Flags]
        public enum InputDeviceTrackerCharacteristics : uint
        {
            TrackerLeftFoot = 0x1000u,
            TrackerRightFoot = 0x2000u,
            TrackerLeftShoulder = 0x4000u,
            TrackerRightShoulder = 0x8000u,
            TrackerLeftElbow = 0x10000u,
            TrackerRightElbow = 0x20000u,
            TrackerLeftKnee = 0x40000u,
            TrackerRightKnee = 0x80000u,
            // add left_wrist
            // add right_wrist
            // add left_ankle
            // add right_ankle
            TrackerWaist = 0x100000u,
            TrackerChest = 0x200000u,
            TrackerCamera = 0x400000u,
            TrackerKeyboard = 0x800000u
        }

        /// <summary>
        /// An Input System device based off the <a href="https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_HTCX_vive_tracker_interaction">HTC Vive Tracker OpenXR specifications
        /// </summary>
        [Preserve, InputControlLayout(displayName = "Vive Tracker OpenXR", commonUsages = new[] { "Left Foot", "Right Foot", "Left Shoulder", "Right Shoulder", "Left Elbow", "Right Elbow", "Left Knee", "Right Knee", "Waist", "Chest", "Camera", "Keyboard" }, isGenericTypeOfDevice = true)]
        public class ViveTracker : TrackedDevice
        {
            #region Pose
            /// <summary>
            /// device pose. Contains isTracked, trackingState, position, rotation, velocity, and angularVelocity 
            /// <see cref="TrackerComponentPaths.devicepose"/>
            /// </summary>
            [Preserve, InputControl(offset = 0, aliases = new[] { "device", "entityPose" }, usage = "Device", noisy = true)]
            public PoseControl devicePose { get; private set; }

            /// <summary>
            /// if data is valid, equivalent to devicePose/isTracked
            /// necessary for compatibility with XRSDK layouts
            /// </summary>
            [Preserve, InputControl(offset = 0, usage = "IsTracked")]
            new public ButtonControl isTracked { get; private set; }

            /// <summary>
            /// represents what data is valid, equivalent to devicePose/trackingState
            /// necessary for compatibility with XRSDK layouts
            /// </summary>
            [Preserve, InputControl(offset = 4, usage = "TrackingState")]
            new public IntegerControl trackingState { get; private set; }

            /// <summary>
            /// position of device, equivalent to devicePose/position
            /// necessary for compatibility with XRSDK layouts
            /// </summary>
            [Preserve, InputControl(offset = 8, alias = "gripPosition")]
            new public Vector3Control devicePosition { get; private set; }

            /// <summary>
            /// rotation/orientation of device, equivalent to devicePose/rotation
            /// necessary for compatibility with XRSDK layouts
            /// </summary>
            [Preserve, InputControl(offset = 20, alias = "gripOrientation")]
            new public QuaternionControl deviceRotation { get; private set; }
            #endregion

            #region Boolean Inputs
            /// <summary>
            /// System button on top of tracker. May not be available for application use
            /// <see cref="TrackerComponentPaths.system"/>
            /// </summary>
            [Preserve, InputControl(alias = "systemButton", usage = "SystemButton")]
            public ButtonControl system { get; private set; }

            /// <summary>
            /// Menu button. Accessed on vive tracker 3.0 via USB or by shorting pins 2 and 6
            /// <see cref="TrackerComponentPaths.menu"/>
            /// </summary>
            [Preserve, InputControl(alias = "menuButton", usage = "MenuButton")]
            public ButtonControl menu { get; private set; }

            /// <summary>
            /// Grip button. Accessed on vive tracker 3.0 via USB or by shorting pins 2 and 3
            /// <see cref="TrackerComponentPaths.grip"/>
            /// </summary>
            [Preserve, InputControl(alias = "gripButton", usage = "GripButton")]
            public ButtonControl grip { get; private set; }

            /// <summary>
            /// Trigger button. Accessed on vive tracker 3.0 via USB or by shorting pins 2 and 4
            /// <see cref="TrackerComponentPaths.trigger"/>
            /// </summary>
            [Preserve, InputControl(alias = "triggerButton", usage = "TriggerButton")]
            public ButtonControl trigger { get; private set; }

            /// <summary>
            /// Trackpad button. Accessed on vive tracker 3.0 via USB or by shorting pins 2 and 5
            /// <see cref="TrackerComponentPaths.pad"/>
            /// </summary>
            [Preserve, InputControl(alias = "trackpadButton", usage = "TrackpadButton")]
            public ButtonControl pad { get; private set; }

            /// <summary>
            /// Trackpad touch. Only accessible on vive tracker 3.0 via USB
            /// <see cref="TrackerComponentPaths.padTouch"/>
            /// </summary>
            [Preserve, InputControl(alias = "trackpadTouch", usage = "TrackpadTouch")]
            public ButtonControl padTouch { get; private set; }
            #endregion

            #region Float Inputs
            /// <summary>
            /// Trigger pull analog value. Only accessible on vive tracker 3.0 via USB
            /// <see cref="TrackerComponentPaths.triggerValue"/>
            /// </summary>
            [Preserve, InputControl(alias = "triggerValue", usage = "TriggerValue")]
            public AxisControl triggerValue { get; private set; }

            /// <summary>
            /// Trackpad X/horizontal analog value. Only accessible on vive tracker 3.0 via USB
            /// <see cref="TrackerComponentPaths.padXValue"/>
            /// </summary>
            [Preserve, InputControl(alias = "trackpadXValue", usage = "TrackpadXValue")]
            public AxisControl padXValue { get; private set; }

            /// <summary>
            /// Trackpad Y/vertical analog value. Only accessible on vive tracker 3.0 via USB
            /// <see cref="TrackerComponentPaths.padYValue"/>
            /// </summary>
            [Preserve, InputControl(alias = "trackpadYValue", usage = "TrackpadYValue")]
            public AxisControl padYValue { get; private set; }
            #endregion

            #region device Outputs
            /// <summary>
            /// Haptic outputs. Accessed on vive tracker 3.0 by pogo pin 1. Untested via USB
            /// <see cref="TrackerComponentPaths.haptic"/>
            /// </summary>
            [Preserve, InputControl(usage = "Haptic")]
            public HapticControl haptic { get; private set; }
            #endregion

            /// <inheritdoc cref="OpenXRDevice"/>
            protected override void FinishSetup() {
                base.FinishSetup();

                devicePose = GetChildControl<PoseControl>("devicePose");
                isTracked = GetChildControl<ButtonControl>("isTracked");
                trackingState = GetChildControl<IntegerControl>("trackingState");
                devicePosition = GetChildControl<Vector3Control>("devicePosition");
                deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");

                system = GetChildControl<ButtonControl>("system");
                menu = GetChildControl<ButtonControl>("menu");
                grip = GetChildControl<ButtonControl>("grip");
                trigger = GetChildControl<ButtonControl>("trigger");
                pad = GetChildControl<ButtonControl>("pad");
                padTouch = GetChildControl<ButtonControl>("padTouch");

                triggerValue = GetChildControl<AxisControl>("triggerValue");
                padXValue = GetChildControl<AxisControl>("padXValue");
                padYValue = GetChildControl<AxisControl>("padYValue");

                haptic = GetChildControl<HapticControl>("haptic");

                var deviceDescriptor = XRDeviceDescriptor.FromJson(description.capabilities);

                if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerLeftFoot) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Left Foot");
                else if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerRightFoot) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Right Foot");
                else if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerLeftShoulder) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Left Shoulder");
                else if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerRightShoulder) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Right Shoulder");
                else if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerLeftElbow) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Left Elbow");
                else if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerRightElbow) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Right Elbow");
                else if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerLeftKnee) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Left Knee");
                else if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerRightKnee) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Right Knee");
                else if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerWaist) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Waist");
                else if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerChest) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Chest");
                else if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerCamera) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Camera");
                else if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerKeyboard) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Keyboard");
                else {
                    Debug.Log("No tracker role could be found that matches this device");
                }

                Debug.Log("Device added");
            }
        }

        /// <summary>
        /// Registers the <see cref="ViveTracker"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout() {
            InputSystem.InputSystem.RegisterLayout(typeof(ViveTracker),
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="ViveTracker"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout() {
            InputSystem.InputSystem.RemoveLayout(nameof(ViveTracker));
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime() {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "htcvivetracker",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "HTC",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerLeftFoot),
                        userPath = TrackerUserPaths.leftFoot
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerRightFoot),
                        userPath = TrackerUserPaths.rightFoot
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerLeftShoulder),
                        userPath = TrackerUserPaths.leftShoulder
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerRightShoulder),
                        userPath = TrackerUserPaths.rightShoulder
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerLeftElbow),
                        userPath = TrackerUserPaths.leftElbow
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerRightElbow),
                        userPath = TrackerUserPaths.rightElbow
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerLeftKnee),
                        userPath = TrackerUserPaths.leftKnee
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerRightKnee),
                        userPath = TrackerUserPaths.rightKnee
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerWaist),
                        userPath = TrackerUserPaths.waist
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerChest),
                        userPath = TrackerUserPaths.chest
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerCamera),
                        userPath = TrackerUserPaths.camera
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerKeyboard),
                        userPath = TrackerUserPaths.keyboard
                    }
                },
                actions = new List<ActionConfig>()
                {
                    // Device Pose
                    new ActionConfig()
                    {
                        name = "devicePose",
                        localizedName = "Device Pose",
                        type = ActionType.Pose,
                        usages = new List<string>() { "Device" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = TrackerComponentPaths.devicepose,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // System button
                    new ActionConfig()
                    {
                        name = "system",
                        localizedName = "System",
                        type = ActionType.Binary,
                        usages = new List<string>() { "SystemButton" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = TrackerComponentPaths.system,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Menu button
                    new ActionConfig()
                    {
                        name = "menu",
                        localizedName = "Menu",
                        type = ActionType.Binary,
                        usages = new List<string>() { "MenuButton" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = TrackerComponentPaths.menu,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Grip button
                    new ActionConfig()
                    {
                        name = "grip",
                        localizedName = "Grip",
                        type = ActionType.Binary,
                        usages = new List<string>() { "GripButton" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = TrackerComponentPaths.grip,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Trigger button
                    new ActionConfig()
                    {
                        name = "trigger",
                        localizedName = "Trigger",
                        type = ActionType.Binary,
                        usages = new List<string>()  { "TriggerButton" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = TrackerComponentPaths.trigger,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Trackpad button
                    new ActionConfig()
                    {
                        name = "pad",
                        localizedName = "Pad",
                        type = ActionType.Binary,
                        usages = new List<string>() { "TrackpadButton" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = TrackerComponentPaths.pad,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Trackpad touch
                    new ActionConfig()
                    {
                        name = "padTouch",
                        localizedName = "PadTouch",
                        type = ActionType.Binary,
                        usages = new List<string>() { "TrackpadTouch" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = TrackerComponentPaths.padTouch,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Trigger pull analog value
                    new ActionConfig()
                    {
                        name = "triggerValue",
                        localizedName = "TriggerValue",
                        type = ActionType.Axis1D,
                        usages = new List<string>() { "TrackpadValue" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = TrackerComponentPaths.triggerValue,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Trackpad X/horizontal analog value
                    new ActionConfig()
                    {
                        name = "padXValue",
                        localizedName = "Trackpad X Value",
                        type = ActionType.Axis1D,
                        usages = new List<string>() { "TrackpadXValue" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = TrackerComponentPaths.padXValue,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Trackpad Y/vertical analog value
                    new ActionConfig()
                    {
                        name = "padYValue",
                        localizedName = "Trackpad Y Value",
                        type = ActionType.Axis1D,
                        usages = new List<string>() { "TrackpadYValue" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = TrackerComponentPaths.padYValue,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Haptic output
                    new ActionConfig()
                    {
                        name = "haptic",
                        localizedName = "Haptic Output",
                        type = ActionType.Vibrate,
                        usages = new List<string>() { "Haptic" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = TrackerComponentPaths.haptic,
                                interactionProfileName = profile,
                            }
                        }
                    }
                }
            };

            AddActionMap(actionMap);
        }

        protected override bool OnInstanceCreate(ulong xrInstance) {
            bool res = base.OnInstanceCreate(xrInstance);

            string debug = "HTC Vive Tracker OpenXR Extension ";
            if (OpenXRRuntime.IsExtensionEnabled(extensionName)) {
                Debug.Log(debug + "Enabled");
            }
            else {
                Debug.LogWarning(debug + "Not Enabled");
            }

            return res;
        }
    }
}
