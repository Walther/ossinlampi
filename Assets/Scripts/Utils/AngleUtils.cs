using System;

public static class AngleUtils
{
    public static float ClampAngle180(float angle)
    {
        while (angle > 180f)
        {
            angle = angle - 360f;
        }

        while (angle < -180f)
        {
            angle = angle + 360f;
        }

        return angle;
    }

}


