using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace BlenderConstraints
{
    public class BlenderDampedTrack : RigConstraint<BlenderDampedTrackJob, BlenderDampedTrackData, BlenderDampedTrackBinder>
    {
    }

    // a custom animation job to use w/ animation rigging package
    // runs on the animation thread
    // TODO: burst compile for performance
    // [Unity.Burst.BurstCompile]
    public struct BlenderDampedTrackJob : IWeightedAnimationJob
    {
        public ReadWriteTransformHandle constrained;
        public ReadOnlyTransformHandle target;
        public Vector3Property savedRotationConstrained; // local (parent) Unity eulers
        public IntProperty trackAxis;

        public FloatProperty jobWeight { get; set; }

        public void ProcessAnimation(AnimationStream stream)
        {
            Quaternion restPoseLocal = Quaternion.Euler(savedRotationConstrained.Get(stream));

            // compute aim axis at rest, relative to constrained's parent (or world if no parent exists)
            Vector3 aimAxisAtRest = restPoseLocal * AxisUtils.vectorFromAxis[trackAxis.Get(stream)];
            // compute properly aimed axis, relative to constrained's parent (or world if no parent exists)
            Quaternion parentRotationWorld = constrained.GetRotation(stream) * Quaternion.Inverse(constrained.GetLocalRotation(stream));
            Vector3 aimAxisTarget = Quaternion.Inverse(parentRotationWorld) * (target.GetPosition(stream) - constrained.GetPosition(stream));
            // compute swing from aim axis at rest to properly aimed axis
            Quaternion aimCorrection = Quaternion.FromToRotation(aimAxisAtRest, aimAxisTarget);
            // compute final aim oriëntation relative to constrained's parent (or world if no parent exists)
            Quaternion aimedRotation = aimCorrection * restPoseLocal;
            // write back weighted result
            constrained.SetLocalRotation(stream, Quaternion.Lerp(constrained.GetRotation(stream), aimedRotation, jobWeight.Get(stream)));
        }

        public void ProcessRootMotion(AnimationStream stream) { }
    }

    // all of the data required for the custom animation constraint (to be configured in editor)
    [System.Serializable]
    public struct BlenderDampedTrackData : IAnimationJobData
    {

        public Transform constrained;
        public Transform target;
        [HideInInspector] public Vector3 savedRotationConstrained;
        public SignedAxis axis;

        public bool IsValid()
        {
            return constrained != null && target != null;
        }

        public void SetDefaultValues()
        {
            constrained = null;
            target = null;
            savedRotationConstrained = Quaternion.identity.eulerAngles;
            axis = SignedAxis.Y;
        }
    }

    // binds data to job so data can be accessed from the animation stream
    public class BlenderDampedTrackBinder : AnimationJobBinder<BlenderDampedTrackJob, BlenderDampedTrackData>
    {
        public override BlenderDampedTrackJob Create(Animator animator, ref BlenderDampedTrackData data, Component component)
        {
            return new BlenderDampedTrackJob
            {
                constrained = ReadWriteTransformHandle.Bind(animator, data.constrained),
                target = ReadOnlyTransformHandle.Bind(animator, data.target),
                savedRotationConstrained = Vector3Property.Bind(animator, component,
                    ConstraintsUtils.ConstructConstraintDataPropertyName(nameof(data.savedRotationConstrained))),
                trackAxis = IntProperty.Bind(animator, component,
                    ConstraintsUtils.ConstructConstraintDataPropertyName(nameof(data.axis))),
            };
        }

        public override void Destroy(BlenderDampedTrackJob job) { }
    }
}