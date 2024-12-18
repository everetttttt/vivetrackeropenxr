using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ViveTrackersOpenXR
{
    public enum TrackerRole
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

    public class ViveTrackerManager : MonoBehaviour
    {
        public InputActionAsset ViveTrackerInputAsset = null;
        public Mesh ViveTracker3dMesh = null;
        public Material ViveTrackerMaterial = null;

        public ViveTracker GetViveTracker(TrackerRole role) {
            return Array.Find(GetComponentsInChildren<ViveTracker>(), vt => vt.role == role);
        }

        public void GenerateViveTrackers() {
            if (ViveTrackerInputAsset != null && ViveTracker3dMesh != null && ViveTrackerMaterial != null) {
                // first, delete all children
                for (int i = transform.childCount; i > 0; i--) {
                    DestroyImmediate(transform.GetChild(i - 1).gameObject);
                }

                foreach (TrackerRole role in Enum.GetValues(typeof(TrackerRole))) {
                    GameObject vt = new(role.ToString());
                    vt.transform.SetParent(transform);
                    vt.AddComponent<ViveTracker>();
                    vt.GetComponent<ViveTracker>().InputActions = ViveTrackerInputAsset;
                    vt.GetComponent<ViveTracker>().role = role;

                    vt.AddComponent<MeshFilter>();
                    vt.GetComponent<MeshFilter>().mesh = ViveTracker3dMesh;
                    vt.AddComponent<MeshRenderer>();
                    vt.GetComponent<MeshRenderer>().sharedMaterial = ViveTrackerMaterial;
                    vt.AddComponent<MeshCollider>();
                }
            }
            else {
                Debug.LogError("INVALID: All three variables must be filled to generate trackers");
            }
        }
    }
}