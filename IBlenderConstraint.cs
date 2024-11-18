using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlenderConstraints
{
    public interface IBlenderConstraint
    {
        public GameObject GameObject { get; }
        public float Weight { get; set; }
        public Transform Constrained { get;}
        public Transform Target { get;}

#if (UNITY_EDITOR)
        /// <summary>
        /// Warning: editor fuction ONLY. Do NOT use during run-time. Includes undo history & prefab compatibility.
        /// </summary>
        /// <returns>
        /// the newly created or modified component
        /// </returns>
        public IBlenderConstraint ConvertComponent( bool useAnimationRigging, bool updateInEditMode = true, UpdateMode updateMode = UpdateMode.Update);
#endif // UNITY_EDITOR
    }

    public interface IBlenderConstraintSimple : IBlenderConstraint
    {
        public bool UpdateInEditMode { get; set; }
        public UpdateMode UpdateMode { get; set; }

        public void ApplyConstraint();
    }
}
