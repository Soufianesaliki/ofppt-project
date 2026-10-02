using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Puts the XR camera back at a known spawn point whenever the scene loads (including
    /// after AppController.RestartScene()).
    ///
    /// Why it's needed: the camera's local pose is not stored in the scene — a TrackedPoseDriver
    /// rewrites it every frame from the tracked/simulated HMD. So a scene reload restores the
    /// XR Origin root, but the camera can still end up elsewhere. This script moves the
    /// XR Origin ROOT (never the camera or Camera Offset) so the camera lands on the spawn point.
    ///
    /// Behaviour:
    ///   - Yaw: the origin is rotated around the camera so the camera faces the spawn point's forward.
    ///   - Position: XZ only. Y is left alone, so eye height / floor level from Camera Offset
    ///     and the tracking origin mode are preserved.
    ///   - Runs after a short wait (default 1 frame) so the first tracked pose has been applied.
    ///
    /// Setup: add to the XR Origin, create an empty "SpawnPoint" at the camera's first-load
    /// world position (copy the Main Camera's position and Y rotation) and assign it below.
    /// </summary>
    [RequireComponent(typeof(XROrigin))]
    public class XROriginSpawnAligner : MonoBehaviour
    {
        [Tooltip("Empty Transform placed where the camera should be at scene start. Its Y rotation sets the facing direction; its Y position is ignored.")]
        [SerializeField] private Transform _spawnPoint;

        [Tooltip("Frames to wait before aligning, so the TrackedPoseDriver / XR Device Simulator has applied its first pose. Increase if the camera is still off after Restart.")]
        [SerializeField, Min(1)] private int _framesToWait = 1;

        [Tooltip("Also align the camera's yaw to the spawn point's forward direction.")]
        [SerializeField] private bool _alignYaw = true;

        private XROrigin _origin;

        private void Awake()
        {
            _origin = GetComponent<XROrigin>();
        }

        private IEnumerator Start()
        {
            for (int i = 0; i < _framesToWait; i++)
                yield return null;

            Align();
        }

        /// <summary>Realigns the camera to the spawn point. Can also be called manually.</summary>
        public void Align()
        {
            if (_spawnPoint == null)
            {
                Debug.LogWarning("XROriginSpawnAligner: no spawn point assigned.", this);
                return;
            }

            if (_origin == null || _origin.Camera == null)
            {
                Debug.LogWarning("XROriginSpawnAligner: XR Origin or its camera is missing.", this);
                return;
            }

            Transform cam = _origin.Camera.transform;

            // 1) Yaw first: RotateAroundCameraUsingOriginUp pivots on the camera, so the
            //    camera position doesn't change and the translation below stays correct.
            if (_alignYaw)
            {
                float yaw = Mathf.DeltaAngle(cam.eulerAngles.y, _spawnPoint.eulerAngles.y);
                _origin.RotateAroundCameraUsingOriginUp(yaw);
            }

            // 2) Translate the origin root in XZ only.
            Vector3 delta = _spawnPoint.position - cam.position;
            delta.y = 0f;
            _origin.transform.position += delta;
        }
    }
}
