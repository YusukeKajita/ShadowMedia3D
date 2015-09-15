using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowKinectSensor
{
    public struct Vector4D
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }

    public struct Vector3D
    {
        public float x;
        public float y;
        public float z;
    }

    public struct Matrix3x3
    {
        public float m0;
        public float m1;
        public float m2;
        public float m3;
        public float m4;
        public float m5;
        public float m6;
        public float m7;
        public float m8;
    }



    static class RotateFunctions
    {
        public static Vector3D Rotate(float x, float y, float z, float qx, float qy, float qz, float qw)
        {
            Matrix3x3 matrix = QuaternionToRotationMatrix(new Vector4D { x = qx, y = qy, z = qz, w = qw });

            Vector3D newVector = Vec3MatrixMult(new Vector3D { x = x, y = y, z = z }, matrix);

            return newVector;
        }

        public static Matrix3x3 QuaternionToRotationMatrix(Vector4D q)
        {
            Matrix3x3 newMatrix = new Matrix3x3();

            newMatrix.m0 = 1-           2*q.y*q.y-  2*q.z*q.z;
            newMatrix.m1 = 2*q.x*q.y-   2*q.w*q.z;
            newMatrix.m2 = 2*q.x*q.z+   2*q.w*q.y;
            newMatrix.m3 = 2*q.x*q.y+   2*q.w*q.z;
            newMatrix.m4 = 1-           2*q.x*q.x-  2*q.z*q.z;
            newMatrix.m5 = 2*q.y*q.z-   2*q.w*q.x;
            newMatrix.m6 = 2*q.x*q.z-   2*q.w*q.y;
            newMatrix.m7 = 2*q.y*q.z+   2*q.w*q.x;
            newMatrix.m8 = 1-           2*q.x*q.x-  2*q.y*q.y;

            return newMatrix;
        }

        public static Vector3D Vec3MatrixMult(Vector3D v, Matrix3x3 m)
        {
            Vector3D newVector = new Vector3D();

            newVector.x = v.x * m.m0 + v.y * m.m1 + v.z * m.m2;
            newVector.y = v.x * m.m3 + v.y * m.m4 + v.z * m.m5;
            newVector.z = v.x * m.m6 + v.y * m.m7 + v.z * m.m8;

            return newVector;
        }
    }
}
