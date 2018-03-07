using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{

    public Vector3 test(KinectInterop.JointType jointType)
    {
        int joint = (int)jointType;
        KinectManager kinectManager = KinectManager.Instance;
        if (kinectManager && kinectManager.IsInitialized())
        {
            if (kinectManager.IsUserDetected())
            {
                long userId = kinectManager.GetPrimaryUserID();
                if (kinectManager.IsJointTracked(userId, joint))
                {
                    Vector3 jointPostion = kinectManager.GetJointKinectPosition(userId, joint);
                    if (jointPostion != Vector3.zero)
                    {
                        Vector2 depthPosition = kinectManager.MapSpacePointToDepthCoords(jointPostion);
                        ushort depthValue = kinectManager.GetDepthForPixel((int)depthPosition.x, (int)depthPosition.y);
                        if (depthValue > 0)
                        {
                            Vector2 colorPosition = kinectManager.MapDepthPointToColorCoords(depthPosition, depthValue);
                            float xNorm = colorPosition.x / kinectManager.GetColorImageWidth();
                            float yNorm = colorPosition.x / kinectManager.GetColorImageHeight();
                            Vector3 v3 = Camera.main.ViewportToScreenPoint(new Vector3(xNorm, yNorm, 0));
                            return v3;
                        }
                    }
                }
            }
        }
        return Vector3.zero;
    }
}