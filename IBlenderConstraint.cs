using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlenderConstraints
{
    public interface IBlenderConstraint
    {
        public float Weight { get; set; }
        public Transform Constrained { get; set; }
        public Transform Target { get; set;  }
    }

    public interface IBlenderConstraintSimple : IBlenderConstraint
    {
        public bool UpdateInEditMode { get; set; }
        public UpdateMode UpdateMode { get; set; }

        public void ApplyConstraint();
    }
}
