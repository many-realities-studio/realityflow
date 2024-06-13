using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit;
using System;

/// <summary>
/// Restricts the movement of the manipulation to stay within the bounds of the target. Specifically,
/// ensures that the target of the object manipulator
/// </summary>
public class StayWithinRect : TransformConstraint
{
    [SerializeField]
    RectTransform container;
    public RectTransform Container { get => container; set => container = value; }

    [SerializeField]
    RectTransform contained;
    public RectTransform Contained { get => contained; set => contained = value; }

    public override TransformFlags ConstraintType => TransformFlags.Move;

    readonly Vector3[] corners = new Vector3[4];
    readonly Matrix4x4[] cornerLocalMatrices = new Matrix4x4[4];
    public override void OnManipulationStarted(MixedRealityTransform worldPose)
    {
        if (container == null)
            container = (RectTransform)contained.parent;

        contained.GetWorldCorners(corners);

        for (int i = 0; i < 4; i++)
            cornerLocalMatrices[i] = Matrix4x4.TRS(corners[i] - worldPose.Position, Quaternion.identity, Vector3.one);

        base.OnManipulationStarted(worldPose);
    }

    readonly Matrix4x4[] manipulatedCorners = new Matrix4x4[4];
    public override void ApplyConstraint(ref MixedRealityTransform transform)
    {
        Matrix4x4 newMatrix = Matrix4x4.TRS(transform.Position, transform.Rotation, transform.Scale);

        for (int i = 0; i < 4; i++)
        {
            Matrix4x4 cornerWorldMatrix = cornerLocalMatrices[i] * newMatrix;

            manipulatedCorners[i] = cornerWorldMatrix;
        }

        Vector3 offsetToConstrain = GetOffsetToConstrain(Container, manipulatedCorners);

        transform.Position += offsetToConstrain;
    }

    readonly Vector3[] containerCorners = new Vector3[4];
    Vector3 GetOffsetToConstrain(RectTransform container, Matrix4x4[] containedCorners)
    {
        container.GetWorldCorners(containerCorners);

        Vector3 up = containerCorners[1] - containerCorners[0];
        Vector3 right = containerCorners[3] - containerCorners[0];

        Debug.DrawRay(containerCorners[0], up, Color.green);
        Debug.DrawRay(containerCorners[0], right, Color.green);

        float upMag = up.magnitude;
        float rightMag = right.magnitude;

        Vector2 maxOutside = Vector2.zero;
        for (int i = 0; i < 4; i++)
        {
            Vector2 distOutside = DistancePastPoints(
                containedCorners[i].GetPosition() - containerCorners[0],
                up,
                right,
                upMag,
                rightMag
            );
            if (distOutside.sqrMagnitude > maxOutside.sqrMagnitude)
                maxOutside = -distOutside;
        }

        Vector3 upNorm = up / upMag;
        Vector3 rightNorm = right / rightMag;

        return (rightNorm * maxOutside.x) + (upNorm * maxOutside.y);
    }

    Vector2 DistancePastPoints(
        Vector3 point,
        Vector3 up,
        Vector3 right,
        float upMag,
        float rightMag
    )
    {
        float upScalarProj = Vector3.Dot(point, up) / upMag;
        float rightScalarProj = Vector3.Dot(point, right) / rightMag;

        float upDist;
        if (upScalarProj < 0)
            upDist = upScalarProj;
        else if (upScalarProj > upMag)
            upDist = upScalarProj - upMag;
        else
            upDist = 0;

        float rightDist;
        if (rightScalarProj < 0)
            rightDist = rightScalarProj;
        else if (rightScalarProj > rightMag)
            rightDist = rightScalarProj - rightMag;
        else
            rightDist = 0;

        return new Vector2(rightDist, upDist);
    }
}
