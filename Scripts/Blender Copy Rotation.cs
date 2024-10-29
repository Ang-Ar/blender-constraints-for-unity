using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace BlenderConstraints
{
    [DisallowMultipleComponent]
    public class BlenderCopyRotation : RigConstraint<BlenderCopyRotationJob, BlenderCopyRotationData, BlenderCopyRotationBinder>
    {
    }

    // a custom animation job to use w/ animation rigging package
    // runs on the animation thread
    // TODO: burst compile for performance
    // [Unity.Burst.BurstCompile]
    public struct BlenderCopyRotationJob : IWeightedAnimationJob
    {
        public ReadWriteTransformHandle constrained;
        public ReadOnlyTransformHandle target;
        public Vector3Property savedRotationConstrained; // local Unity eulers
        public Vector3Property savedRotationTarget; // local Unity eulers
        public IntProperty eulerAxisOrder;
        public IntProperty includedAxes;

        public FloatProperty jobWeight { get; set; }

        // not needed for this constraint
        public void ProcessRootMotion(AnimationStream stream) { }

        // where the constraint is actually calculated & applied
        public void ProcessAnimation(AnimationStream stream)
        {
            // get rest pose of constrained object
            Quaternion defaultRotation = Quaternion.Euler(savedRotationConstrained.Get(stream));

            // get target rotation relative to its rest pose
            Quaternion targetRotation = target.GetLocalRotation(stream);
            targetRotation = Quaternion.Inverse(Quaternion.Euler(savedRotationTarget.Get(stream))) * targetRotation;
            // convert to appropriate euler axes
            EulerAxisOrder eulerAxisOrder = (EulerAxisOrder)this.eulerAxisOrder.Get(stream);
            Vector3 targetEulers = AxisUtils.QuaternionToCustomEulers(targetRotation, eulerAxisOrder);
            //Debug.Log($"Target Rotation in {eulerAxisOrder} eulers (degrees): {targetEulers * Mathf.Rad2Deg}");
            // set ignored axes to zero
            IncludedAxes mask = (IncludedAxes)includedAxes.Get(stream);
            byte[] axisIndices = AxisUtils.axisIndexFromEnum[(int)eulerAxisOrder];
            targetEulers[axisIndices[0]] *= (mask & IncludedAxes.x) == IncludedAxes.x ? 1 : 0;
            targetEulers[axisIndices[1]] *= (mask & IncludedAxes.y) == IncludedAxes.y ? 1 : 0;
            targetEulers[axisIndices[2]] *= (mask & IncludedAxes.z) == IncludedAxes.z ? 1 : 0;
            // update target rotation with ignored axes
            targetRotation = AxisUtils.CustomEulersToQuaternion(targetEulers, eulerAxisOrder);
            // apply target rotation on top of rest pose
            targetRotation = defaultRotation * targetRotation;

            // calculate final weighted rotation of constrained object
            float weight = jobWeight.Get(stream);
            constrained.SetLocalRotation(stream, Quaternion.Lerp(defaultRotation, targetRotation, weight));
        }
    }

    // all of the data required for the custom animation constraint (to be configured in editor)
    [System.Serializable]
    public struct BlenderCopyRotationData : IAnimationJobData
    {
        public Transform constrained;
        public Transform target;
        [HideInInspector] [SyncSceneToStream] public Vector3 savedRotationConstrained;
        [HideInInspector] [SyncSceneToStream] public Vector3 savedRotationTarget;
        public EulerAxisOrder eulerAxisOrder;
        public IncludedAxes includedAxes;

        public bool IsValid()
        {
            return (constrained != null && target != null);
        }

        public void SetDefaultValues()
        {
            constrained = null;
            target = null;
            savedRotationConstrained = Quaternion.identity.eulerAngles;
            savedRotationTarget = Quaternion.identity.eulerAngles;
            eulerAxisOrder = (EulerAxisOrder)0;
            includedAxes = IncludedAxes.x | IncludedAxes.y | IncludedAxes.z;
        }
    }

    // binds data to job so data can be accessed from the animation stream
    public class BlenderCopyRotationBinder : AnimationJobBinder<BlenderCopyRotationJob, BlenderCopyRotationData>
    {
        public override BlenderCopyRotationJob Create(Animator animator, ref BlenderCopyRotationData data, Component component)
        {
            return new BlenderCopyRotationJob
            {
                constrained = ReadWriteTransformHandle.Bind(animator, data.constrained),
                target = ReadOnlyTransformHandle.Bind(animator, data.target),
                savedRotationConstrained = Vector3Property.Bind(animator, component,
                    ConstraintsUtils.ConstructConstraintDataPropertyName(nameof(data.savedRotationConstrained))),
                savedRotationTarget = Vector3Property.Bind(animator, component,
                    ConstraintsUtils.ConstructConstraintDataPropertyName(nameof(data.savedRotationTarget))),
                eulerAxisOrder = IntProperty.Bind(animator, component,
                    ConstraintsUtils.ConstructConstraintDataPropertyName(nameof(data.eulerAxisOrder))),
                includedAxes = IntProperty.Bind(animator, component,
                    ConstraintsUtils.ConstructConstraintDataPropertyName(nameof(data.includedAxes))),
            };
        }

        public override void Destroy(BlenderCopyRotationJob job)
        {
            // de-allocate memory & free resources here, if any
        }
    }
}
