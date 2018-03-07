using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UseKinectManager : MonoBehaviour
{
    private KinectManager kinectManager;
    public RawImage kinectImg;

    // Use this for initialization
    void Start()
    {
        kinectManager = KinectManager.Instance; //设备初始化
    }

    // Update is called once per frame
    void Update()
    {
        //kinectImg.transform.SetSiblingIndex(99);
        /*
        if (kinectManager && kinectManager.IsInitialized())
        {
            if (kinectImg != null&& kinectImg.texture == null)
            {
                //获取彩色数据
                //   Texture2D usersClrTex = kinectManager.GetUsersClrTex();  
                //获取深度数据 
                Texture2D usersLblTex = kinectManager.GetUsersLblTex();
                kinectImg.texture = usersLblTex;
            }
            if (kinectManager.IsUserDetected())
            {
                //获取用户ID
                long userId = kinectManager.GetPrimaryUserID();
                //获取用户相对Kinect的位置信息
                Vector3 userPosition = kinectManager.GetUserPosition(userId);

                int rightHandJoint = (int)KinectInterop.JointType.HandRight;
                if (kinectManager.IsJointTracked(userId, rightHandJoint))
                {
                    //获取右手信息
                    Vector3 rightHandPosition = kinectManager.GetJointKinectPosition(userId, rightHandJoint);
                    // print("X=" + rightHandPosition.x + ",Y=" + rightHandPosition.y + ",Z=" + rightHandPosition.z);
                }
            }
        }
        */
    }
}
