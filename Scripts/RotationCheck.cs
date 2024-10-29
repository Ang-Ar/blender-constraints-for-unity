using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[ExecuteInEditMode]
public class RotationCheck : MonoBehaviour
{
    [Flags]
    public enum Mode
    {
        None = 0,
        QuaternionToEuler = 1,
        EulerToQuaternion = 2,
    }

    public Transform src;
    public Transform dest;

    [Space]

    public Mode useCustomCodeFor;

    [Space]

    /// Notes On Order (using ZXY as an example): 
    /// - mask of Z only will ignore all local (intrinsic) Z rotations
    /// - mask of Y only will ignore all global (extrinsic) Y rotations
    /// - mask of X only is bugged when transform X = 0 and using custom QtoE (TODO: fix this in singularity-handling code)
    public BlenderConstraints.EulerAxisOrder order = BlenderConstraints.EulerAxisOrder.ZXY;
    public BlenderConstraints.IncludedAxes mask = BlenderConstraints.IncludedAxes.x | BlenderConstraints.IncludedAxes.y | BlenderConstraints.IncludedAxes.z;

    // Update is called once per frame
    void Update()
    {
        var axisOrder = BlenderConstraints.AxisUtils.eulerOrderFromEnum[(int)order];
        var axisIndices = BlenderConstraints.AxisUtils.axisIndexFromEnum[(int)order];

        // get source local rotation
        Quaternion quaternionSRC = src.localRotation;

        // convert to eulers (custom order, radians)
        Vector3 localEulersToCopy;
        if ((useCustomCodeFor & Mode.QuaternionToEuler) == Mode.QuaternionToEuler)
        {
            localEulersToCopy = BlenderConstraints.AxisUtils.QuaternionToCustomEulers(quaternionSRC, order);
        }
        else
        {
            Debug.Assert(order == BlenderConstraints.EulerAxisOrder.ZXY, "Unity's Quaternion to Euler conversion only supports ZXY order");
            localEulersToCopy = quaternionSRC.eulerAngles * Mathf.Deg2Rad;
            // switch from listing angles in {x, y, z} to listing in custom order
            localEulersToCopy = new Vector3(localEulersToCopy[axisOrder[0]], localEulersToCopy[axisOrder[1]], localEulersToCopy[axisOrder[2]]);
        }

        // set ignored axes to zero
        localEulersToCopy[axisIndices[0]] *= (mask & BlenderConstraints.IncludedAxes.x) == BlenderConstraints.IncludedAxes.x ? 1 : 0;
        localEulersToCopy[axisIndices[1]] *= (mask & BlenderConstraints.IncludedAxes.y) == BlenderConstraints.IncludedAxes.y ? 1 : 0;
        localEulersToCopy[axisIndices[2]] *= (mask & BlenderConstraints.IncludedAxes.z) == BlenderConstraints.IncludedAxes.z ? 1 : 0;

        // convert back to quaternion
        Quaternion quaternionDest;
        if ((useCustomCodeFor & Mode.EulerToQuaternion) == Mode.EulerToQuaternion)
        {
            quaternionDest = BlenderConstraints.AxisUtils.CustomEulersToQuaternion(localEulersToCopy, order);
        }
        else
        {
            Debug.Assert(order == BlenderConstraints.EulerAxisOrder.ZXY, "Unity's Quaternion to Euler conversion only supports ZXY order");
            // switch from listing angles in customorder to listing in {x, y, z}
            localEulersToCopy = new Vector3(localEulersToCopy[axisIndices[0]], localEulersToCopy[axisIndices[1]], localEulersToCopy[axisIndices[2]]);
            quaternionDest = Quaternion.Euler(localEulersToCopy * Mathf.Rad2Deg);
        }

        // write back to scene
        dest.localRotation = quaternionDest;
    }
}
