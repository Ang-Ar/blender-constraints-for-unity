using System;
using UnityEngine;

namespace BlenderConstraints
{
    public enum SignedAxis
    {
        X,
        Y,
        Z,
        X_NEG,
        Y_NEG,
        Z_NEG,
    }

    public enum UpdateMode
    {
        Update,
        FixedUpdate,
        Animation, // currently non functional
    }

    public enum EulerAxisOrder
    {
        // applied front-to-back as extrinsic eulers
        // blender default = XYZ; unity default = ZXY
        XYZ, XZY, YXZ, YZX, ZXY, ZYX
    }


    [Flags]
    public enum IncludedAxes
    {
        none, x = 1, y = 2, z = 4
    }

    public static class AxisUtils
    {
        static public byte[][] eulerOrderFromEnum =
        {
            new byte[] {0, 1, 2}, // XYZ, 
            new byte[] {0, 2, 1}, // XZY, 
            new byte[] {1, 0, 2}, // YXZ, 
            new byte[] {1, 2, 1}, // YZX, 
            new byte[] {2, 0, 1}, // ZXY,
            new byte[] {2, 1, 0}, // ZYX,
        };

        static public byte[][] axisIndexFromEnum =
        {
            new byte[] {0, 1, 2}, // XYZ, 
            new byte[] {0, 2, 1}, // XZY, 
            new byte[] {1, 0, 2}, // YXZ, 
            new byte[] {2, 0, 1}, // YZX, 
            new byte[] {1, 2, 0}, // ZXY,
            new byte[] {2, 1, 0}, // ZYX,
        };

        static public Vector3[] vectorFromAxis =
        {
            new (1, 0, 0),  // X
            new (0, 1, 0),  // Y
            new (0, 0, 1),  // Z
            new (-1, 0, 0), // -X
            new (0, -1, 0), // -Y
            new (0, 0, -1), // -Z
        };

        static public Vector3 QuaternionToCustomEulers(Quaternion quaternion, EulerAxisOrder eulerOrder)
        {
            // implemented after Bernardes, E., & Viollet, S. (2022). Quaternion to Euler angles conversion: A direct, general and computationally efficient method. PloS one, 17(11), e0276302.
            // https://www.ncbi.nlm.nih.gov/pmc/articles/PMC9648712/
            // some ques taken form the Python implementation to implement {x, y, z, w} indexing of quaternions
            // https://github.com/evbernardes/quaternion_to_euler
            // EulerAxisOrder enum only supports non-proper oriëntations, but this code still supports proper ones too
            // euler axes should be read front-to-back as extrinsics, or back-to-front as intrinsics

            // NOTE: resulting Vector3 lists angles in the order given by eulerOrder and in radians

            // i, j, k order of axes
            int i, j, k;
            i = eulerOrderFromEnum[(int)eulerOrder][0];
            j = eulerOrderFromEnum[(int)eulerOrder][1];
            k = eulerOrderFromEnum[(int)eulerOrder][2];

            // standardise computation for both proper & non-proper axis orders

            bool isProper = i == k;
            if (!isProper)
            {
                k = 3-i- j;
            }
            int e = (i - j) * (j - k) * (k - i) / 2; // will only ever be -1 or +1
            float a, b, c, d;
            if (isProper)
            {
                a = quaternion.w;
                b = quaternion[i];
                c = quaternion[j];
                d = quaternion[k] * e;
            }
            else
            {
                a = quaternion.w - quaternion[j];
                b = quaternion[i] + quaternion[k] * e;
                c = quaternion[j] + quaternion.w;
                d = quaternion[k] * e - quaternion[i];
            }

            // set resulting angles in custom order & resolve singularities

            Vector3 result = new ();
            result[1] = Mathf.Acos(2f * (a*a + b*b) / (a*a + b*b + c*c + d*d) - 1);
            float thetaPos = Mathf.Atan2(b, a);
            float thetaNeg = Mathf.Atan2(d, c);
            if (Mathf.Approximately(result[1], 0))
            {
                // singularity (aka gimball lock)
                result[0] = 0; // arbitrary value
                result[2] = 2 * thetaPos - result[0];
            }
            else if (Mathf.Approximately(result[1], Mathf.PI))
            {
                // singularity (aka gimball lock)
                result[0] = 0; // arbitrary value
                result[2] = 2 * thetaNeg + result[0];
            }
            else
            {
                // no singularity (aka no gimball lock)
                result[0] = thetaPos - thetaNeg;
                result[2] = thetaPos + thetaNeg;
            }

            // final angle fixes

            if (!isProper)
            {
                // fix angles for non-proper axis orders
                result[2] = e * result[2];
                result[1] = result[1] - Mathf.PI/2;
            }
            // optionally limit angle to [0, 2*PI] here

            return result;
        }

        static public Quaternion CustomEulersToQuaternion(Vector3 eulerAngles, EulerAxisOrder eulerOrder)
        {
            // custom algorithm based on  equivalency of intrinsic & extrinsic rotation
            // instead of applying extrinsic rotations front-to-back,
            // applies rotations back to front using intrinsic eulers
            // TODO: look into a potentially more optimised ways to do this

            // NOTE: eulerAngles should list angles in the order given by eulerOrder and in radians

            // get all axes as Vector3 before any rotation, in their custom order
            Vector3 axis0 = vectorFromAxis[eulerOrderFromEnum[(int)eulerOrder][0]];
            Vector3 axis1 = vectorFromAxis[eulerOrderFromEnum[(int)eulerOrder][1]];
            Vector3 axis2 = vectorFromAxis[eulerOrderFromEnum[(int)eulerOrder][2]];

            // apply first intrinsic rotation
            Quaternion result = Quaternion.AngleAxis(eulerAngles[2] * Mathf.Rad2Deg, axis2);
            // apply second intrinsic rotation after (& relative to) first
            result *= Quaternion.AngleAxis(eulerAngles[1] * Mathf.Rad2Deg, axis1);
            // apply third intrinsic rotation after (& relative to) previous two
            result *= Quaternion.AngleAxis(eulerAngles[0] * Mathf.Rad2Deg, axis0);

            return result;
        }
    }
}
