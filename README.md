# vivetrackeropenxr

According to [this post on the Steam forums](https://steamcommunity.com/app/250820/discussions/8/560232227703656040/) which references [this github project](https://github.com/danwillm/openxr-vive-tracker-ext/tree/main), pogo pins and usb input for the vive trackers in SteamVR has been enabled as of a couple months ago and I should be able to access them through OpenXR. This was corroborated by another user. 
----
I wanted to get this implemented in Unity, the scripts above are my results. 
1. To recreate, make a new Unity project, open the package manager and import the OpenXR plugin. Next, copy my assets and put them in your project.

2. Open Edit->Project Settings, under XR Plug-in Management toggle OpenXR on. It'll have a caution sign, click it to fix the PoseControl thing. Then under the OpenXR submenu, add HTC Vive Tracker OpenXR Profile as an interaction profile.

3. Create an empty game object in your scene and add the ViveTrackerManager script. Drag the input actions asset over and the mesh and material under ViveTracker3.obj, then click generate vive trackers. This should generate 13 vive trackers as a child (later todo is add the updated lsit of roles including wrists and ankles).

4. Upon clicking play, the active trackers should accept in position and rotation data, but not pogo pin/usb input.


Notes:\
The interaction profile script was edited from [this script from Unity staff](https://discussions.unity.com/t/openxr-and-openvr-together/841675/21) and I tried to match it to [the builtin HTC Controller profile](https://github.com/needle-mirror/com.unity.xr.openxr/blob/72c94edf593e5d4ae100d0d6b8e05b7245eabf18/Runtime/Features/Interactions/HTCViveControllerProfile.cs#L34)\
The 3d model is taken from [HTC's Vive Input Utility Unity package](https://github.com/ViveSoftware/ViveInputUtility-Unity/blob/28700d98c99bec35ad192057765a6bfadaabc78d/Assets/HTC.UnityPlugin/ViveInputUtility/Resources/Models/ObjModelViveTracker3.obj)
