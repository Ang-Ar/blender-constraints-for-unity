#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using UnityEngine;


namespace BlenderConstraints
{
    [ExecuteAlways]
    public class BlenderCopyRotationSimple : MonoBehaviour, IBlenderConstraintSimple
    {
        public GameObject GameObject { get => this.gameObject; }
        public float Weight { get => weight; set => weight = Mathf.Clamp(value, 0f, 1f); }
        public Transform Constrained { get => constrained; }
        public Transform Target { get => target; }
        public bool UpdateInEditMode { get => updateInEditMode; set => updateInEditMode = value; }
        public UpdateMode UpdateMode { get => updateMode; set => updateMode = value; }

#if (UNITY_EDITOR)
        public IBlenderConstraint ConvertComponent(bool useAnimationRigging, bool updateInEditMode = true, UpdateMode updateMode = UpdateMode.Update)
        {
            Undo.SetCurrentGroupName("convert blender constraint");

            if (!useAnimationRigging)
            {
                Undo.RecordObject(this, "update constraint settings");
                this.updateInEditMode = updateInEditMode;
                this.updateMode = updateMode;
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
                return this;
            }

            var animRigConstraint = Undo.AddComponent<BlenderCopyRotation>(this.gameObject);
            Undo.RecordObject(animRigConstraint, "clone constraint settings");
            animRigConstraint.weight = weight;
            animRigConstraint.data.constrained = constrained;
            animRigConstraint.data.target = target;
            animRigConstraint.data.savedRotationConstrained = constrainedRestPose.eulerAngles;
            animRigConstraint.data.savedRotationTarget = targetRestPose.eulerAngles;
            animRigConstraint.data.eulerAxisOrder = order;
            animRigConstraint.data.includedAxes = mask;
            Undo.DestroyObjectImmediate(this);

            PrefabUtility.RecordPrefabInstancePropertyModifications(animRigConstraint);

            return animRigConstraint;
        }
#endif // UNITY_EDITOR

        [Range(0f, 1f)] public float weight = 1f;

        [Space]

        public Transform constrained;
        public Transform target;
        [HideInInspector] public Quaternion constrainedRestPose = Quaternion.identity;
        [HideInInspector] public Quaternion targetRestPose = Quaternion.identity;

        [Space]

        public EulerAxisOrder order = EulerAxisOrder.ZXY;
        public IncludedAxes mask = IncludedAxes.x | IncludedAxes.y | IncludedAxes.z;

        [Space]

        public bool updateInEditMode = false;
        public UpdateMode updateMode = UpdateMode.Update;

        void Update()
        {
            if ((updateMode == UpdateMode.Update && Application.isPlaying) || (updateInEditMode && !Application.isPlaying))
            {
                ApplyConstraint();
            }
        }

        private void FixedUpdate()
        {
            if (updateMode == UpdateMode.FixedUpdate)
            {
                ApplyConstraint();
            }
        }

        public void ApplyConstraint()
        {
            var axisOrder = AxisUtils.eulerOrderFromEnum[(int)order];
            var axisIndices = AxisUtils.axisIndexFromEnum[(int)order];

            // get target's local rotation (relative to rest pose)
            Quaternion targetRotation = Quaternion.Inverse(targetRestPose) * target.localRotation;

            // convert to eulers (custom order, radians)
            Vector3 localEulersToCopy = AxisUtils.QuaternionToCustomEulers(targetRotation, order);

            // set ignored axes to zero
            localEulersToCopy[axisIndices[0]] *= (mask & IncludedAxes.x) == IncludedAxes.x ? 1 : 0;
            localEulersToCopy[axisIndices[1]] *= (mask & IncludedAxes.y) == IncludedAxes.y ? 1 : 0;
            localEulersToCopy[axisIndices[2]] *= (mask & IncludedAxes.z) == IncludedAxes.z ? 1 : 0;

            // convert back to quaternion
            Quaternion quaternionDest = AxisUtils.CustomEulersToQuaternion(localEulersToCopy, order);

            // apply result on top of rest pose
            constrained.localRotation = constrainedRestPose * quaternionDest;
        }
    }
}
