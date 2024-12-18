using UnityEditor;
using UnityEngine;

namespace ViveTrackersOpenXR
{
    [CustomEditor(typeof(ViveTrackerManager))]
    public class ViveTrackerManagerEditor : Editor
    {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            ViveTrackerManager manager = (ViveTrackerManager)target;
            if (GUILayout.Button("Generate Vive Trackers")) {
                manager.GenerateViveTrackers();
            }
        }
    }
}