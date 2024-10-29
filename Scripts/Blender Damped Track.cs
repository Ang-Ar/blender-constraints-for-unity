using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace BlenderConstraints
{
    public enum SignedAxis
    {
        /// <summary>Positive X Axis (1, 0, 0)</summary>
        X,
        /// <summary>Negative X Axis (-1, 0, 0)</summary>
        X_NEG,
        /// <summary>Positive Y Axis (0, 1, 0)</summary>
        Y,
        /// <summary>Negative Y Axis (0, -1, 0)</summary>
        Y_NEG,
        /// <summary>Positive Z Axis (0, 0, 1)</summary>
        Z,
        /// <summary>Negative Z Axis (0, 0, -1)</summary>
        Z_NEG
    }

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
        public Vector3Property savedRotationConstrained; // local Unity eulers
        public IntProperty trackAxis;

        public Vector3 axisVector (SignedAxis axis) => axis switch
        {
            SignedAxis.X => new Vector3(1, 0, 0),
            SignedAxis.X_NEG => new Vector3(-1, 0, 0),
            SignedAxis.Y => new Vector3(0, 1, 0),
            SignedAxis.Y_NEG => new Vector3(0, -1, 0),
            SignedAxis.Z => new Vector3(0, 0, 1),
            SignedAxis.Z_NEG => new Vector3(0, 0, -1),
            _ => throw new System.Exception($"Unsupported axis enum value {axis} ({(int)axis})"),
        };

        public FloatProperty jobWeight { get; set; }

        public void ProcessAnimation(AnimationStream stream)
        {
            Vector3 aimAxisStart = Quaternion.Euler(savedRotationConstrained.Get(stream)) * axisVector((SignedAxis)trackAxis.Get(stream));
            Vector3 targetDir = target.GetPosition(stream) - constrained.GetPosition(stream);
            Debug.Log($"rotate {aimAxisStart} to {targetDir}");
            Quaternion aimedRotation = Quaternion.FromToRotation(aimAxisStart, targetDir);
            aimedRotation *= Quaternion.Euler(savedRotationConstrained.Get(stream));
            constrained.SetRotation(stream, Quaternion.Lerp(constrained.GetRotation(stream), aimedRotation, jobWeight.Get(stream)));
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