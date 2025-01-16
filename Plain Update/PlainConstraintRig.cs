using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlenderConstraints
{
    [ExecuteAlways]
    public class PlainConstraintRig : MonoBehaviour
    {
        public enum EditModeBehavior
        {
            force_on, force_off, auto,
        }

        public UpdateMode updateMode;
        public EditModeBehavior evaluateInEditMode = EditModeBehavior.auto;
        private IBlenderConstraintSimple[] constraints = { };

        private void OnValidate()
        {
            if (updateMode == UpdateMode.Ordered)
            {
                Debug.LogError($"PlainConstraintRig does not support update mode {updateMode}, reverting to {UpdateMode.Update}");
                updateMode = UpdateMode.Update;
            }
            BuildConstraintList();
        }

        void Start()
        {
            BuildConstraintList();
        }

        public void BuildConstraintList()
        {
            // GetComponentsInChildren does a preorder traversal of the transform hierarchy, which is the exact order we need
            constraints = GetComponentsInChildren<IBlenderConstraintSimple>(includeInactive: true);
        }

        void Update()
        {
            if (updateMode == UpdateMode.Update)
            {
                ApplyConstraints();
            }
        }

        void FixedUpdate()
        {
            if (updateMode == UpdateMode.FixedUpdate)
            {
                ApplyConstraints();
            }
        }

        void ApplyConstraints()
        {
            // respect Edit mode behavior override
            if (evaluateInEditMode == EditModeBehavior.force_off && !Application.isPlaying) return;

            // evaluate constraints in the correct order (ordering is handled by BuildConstraintList())
            foreach (IBlenderConstraintSimple constraint in constraints)
            {
                // only evaluate if constraint is active and set to use Ordered execution
                if (constraint.GameObject.activeInHierarchy && constraint.MonoBehaviour.enabled && constraint.UpdateMode == UpdateMode.Ordered)
                {
                    // respect edit mode behavior override
                    if (Application.isPlaying || evaluateInEditMode == EditModeBehavior.force_on || constraint.UpdateInEditMode==true)
                    {
                        constraint.ApplyConstraint();
                    }
                }
            }
        }
    }
}
