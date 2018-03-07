using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : MonoBehaviour {
    private Canvas canvas;
    public Image rHandImage;
    public Sprite[] rHandStateSprites;
    public Image DojoButton;
    public Image NewGameButton;
    public Image quitButton;
    public Image circle1;
    public Image circle2;
    public Image circle3;

    private int gravityScale = 20;

    private bool isRHandClose = false;
    private int upForce = 2000;

    private KinectManager kinectManager;
    private PanelCenter panelCenter;
    private Image curButton; //记录被切中的水果
    private AudioSource menuAudioSource;

    // Use this for initialization
    void Start()
    {
        kinectManager = KinectManager.Instance;
        GameObject canvasObj = GameObject.FindWithTag("Canvas");
        canvas = canvasObj.GetComponent<Canvas>();
        panelCenter = canvasObj.GetComponent<PanelCenter>();
        menuAudioSource = GetComponent<AudioSource>();
    }

	// Update is called once per frame
	void Update () {
            if (kinectManager.IsUserDetected()) 
            {
                //获取用户ID
                long userId = kinectManager.GetPrimaryUserID();
                int rHandJoint = (int)KinectInterop.JointType.HandRight;
                    if (kinectManager.IsJointTracked(userId, rHandJoint))
                    {
                    // 获取用户相对Kinect的位置信息
                    Vector3 rHandPosition = kinectManager.GetJointKinectPosition(userId, rHandJoint);
                    Vector3 screenPositionV3 = Camera.main.WorldToScreenPoint(rHandPosition); //右手位置信息转换成在屏幕上的三维坐标
                    Vector2 screenPositionV2 = new Vector2(screenPositionV3.x, screenPositionV3.y); //转换成屏幕上的二维坐标
                    Vector2 rHandLocalPosition;  //UGUI坐标
                    if(RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, screenPositionV2, Camera.main, out rHandLocalPosition))
                    {
                        // 将屏幕坐标转换成UGUI坐标
                        RectTransform rightRectTf = rHandImage.transform as RectTransform;
                        rightRectTf.anchoredPosition = rHandLocalPosition; //将右手的UGUI坐标赋给rightHandImage的坐标
                    }
                    KinectInterop.HandState rHandState = kinectManager.GetRightHandState(userId);
                    rHandImage.sprite = rHandStateSprites[0];
                    if (rHandState == KinectInterop.HandState.Closed)
                    {
                        rHandImage.sprite = rHandStateSprites[1];  //握拳状态
                        isRHandClose = true;
                        if (circle1.enabled == true && RectTransformUtility.RectangleContainsScreenPoint(DojoButton.rectTransform, screenPositionV2, Camera.main))
                        {
                            //if true 表示手在Button上，也就是手在Button所表示的矩形范围内
                            clickFruit(DojoButton);

                        }
                        else if (circle2.enabled == true && RectTransformUtility.RectangleContainsScreenPoint(NewGameButton.rectTransform, screenPositionV2, Camera.main))
                        {
                            clickFruit(NewGameButton);
                        }
                        else if(circle3.enabled == true && RectTransformUtility.RectangleContainsScreenPoint(quitButton.rectTransform, screenPositionV2, Camera.main))
                        {
                            clickFruit(quitButton);
                        }
                    }
                }
            }
        detectCurBtn();
        }

    private void detectCurBtn()
    {
        if (curButton != null)
        {
            RectTransform crt = curButton.transform as RectTransform;
            if (crt.anchoredPosition.y < -290)
            {
                if (curButton != quitButton)
                {
                    panelCenter.showGamePanel();
                    Destroy(gameObject);//销毁首页界面
                }
                else
                {
                    Destroy(gameObject);//销毁首页界面
                    Process.GetCurrentProcess().Kill();
                }
            }
        }
    }

    //点击到水果时
    private void clickFruit(Image fruit)
    {
        menuAudioSource.Stop();
        Rigidbody2D r1 = DojoButton.GetComponent<Rigidbody2D>();
        Rigidbody2D r2 = NewGameButton.GetComponent<Rigidbody2D>();
        Rigidbody2D r3 = quitButton.GetComponent<Rigidbody2D>();
        r1.gravityScale = gravityScale;
        r2.gravityScale = gravityScale;
        r3.gravityScale = gravityScale;
        circle1.enabled = false;
        circle2.enabled = false;
        circle3.enabled = false;
        curButton = fruit;
        if (fruit == DojoButton)
        {
            r1.AddForce(new Vector2(0, upForce));
        }
        else if (fruit == NewGameButton)
        {
            r2.AddForce(new Vector2(0, upForce));
        }
        else
        {
            r3.AddForce(new Vector2(0, upForce));
        }
    }
}
