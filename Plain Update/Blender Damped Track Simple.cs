#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using UnityEngine;

namespace BlenderConstraints
{
    [ExecuteAlways]
    public class BlenderDampedTrackSimple : MonoBehaviour, IBlenderConstraintSimple
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

            var simpleConstraint = Undo.AddComponent<BlenderDampedTrack>(this.gameObject);
            Undo.RecordObject(simpleConstraint, "clone constraint settings");
            simpleConstraint.weight = weight;
            simpleConstraint.data.constrained = constrained;
            simpleConstraint.data.target = target;
            simpleConstraint.data.savedRotationConstrained = constrainedRestPose.eulerAngles;
            simpleConstraint.data.axis = axis;
            Undo.DestroyObjectImmediate(this);

            PrefabUtility.RecordPrefabInstancePropertyModifications(simpleConstraint);

            return simpleConstraint;
        }
#endif //UNITY_EDITOR

        [Range(0f, 1f)] public float weight = 1f;

        [Space]

        public Transform constrained;
        public Transform target;
        [HideInInspector] public Quaternion constrainedRestPose = Quaternion.identity; // local (parent) space
        public SignedAxis axis = SignedAxis.Y;

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

        //// debug stuff
        //Vector3 aimAxisAtRest;
        //Vector3 aimAxisTarget;
        //Quaternion aimCorrection;

        public void ApplyConstraint()
        {
            // compute aim axis at rest, relative to constrained's parent (or world if no parent exists)
            Vector3 aimAxisAtRest = constrainedRestPose * AxisUtils.vectorFromAxis[(int)axis];
            // compute properly aimed axis, relative to constrained's parent (or world if no parent exists)
            Quaternion parentRotationWorld = constrained.rotation * Quaternion.Inverse(constrained.localRotation);
            Vector3 aimAxisTarget = Quaternion.Inverse(parentRotationWorld) * (target.position - constrained.position);
            // compute swing from aim axis at rest to properly aimed axis
            Quaternion aimCorrection = Quaternion.FromToRotation(aimAxisAtRest, aimAxisTarget);
            // compute final aim oriëntation relative to constrained's parent (or world if no parent exists)
            Quaternion aimedRotation = aimCorrection * constrainedRestPose;
            // write weighted result back to scene
            constrained.localRotation = Quaternion.Lerp(constrainedRestPose, aimedRotation, weight);
        }

        //// debug stuff
        //public void OnDrawGizmos()
        //{
        //    Gizmos.color = Color.blue;
        //    Gizmos.DrawLine(constrained.position, constrained.position + constrained.parent.TransformDirection(aimAxisAtRest));
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawLine(constrained.position, constrained.position + constrained.parent.TransformDirection(aimAxisTarget));
        //}
    }
}