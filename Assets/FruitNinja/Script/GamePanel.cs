using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour
{
    public Fruit fruitPrefab;
    private int minForceX = 600;
    private int maxForceX = 1000;
    private int minForceY = 4800;
    private int maxForceY = 6000;
    private int halfForceX = 600;
    private int halfForceY = 100;

    public Transform leftTrail;
    public Transform rightTrail;
    public LifeContent lifeContent;
    public Text scoreTxt;
    public Image gameOverImg;
    public AudioSource splatterAudio;
    public AudioSource boomAudio;
    public AudioSource overAudio;
    public AudioSource throwAudio;
    public AudioSource startAudio;

    private int minX = -286;
    private int maxX = 286;
    private int fruitInitY = -200;
    private const int fruitOutY = -290;
    private Fruit curFruit;
    private int lifeCount = 3;
    private int score = 0;

    private KinectManager kinectManager;
    private PanelCenter panelCenter;

    // Use this for initialization
    void Start()
    {
        kinectManager = KinectManager.Instance;
        gameOverImg.gameObject.SetActive(false);
        GameObject canvasObj = GameObject.FindWithTag("Canvas");
        panelCenter = canvasObj.GetComponent<PanelCenter>();
        startAudio.Play();
        createFruit();
    }

    // Update is called once per frame
    void Update()
    {
        bool needDestroyFruit = false;
        bool isOutScreen = false;
        if (curFruit != null)
        {
            RectTransform curFruitRt = curFruit.transform as RectTransform;
            if (kinectManager && kinectManager.IsInitialized())
            {
                isCutFruit(curFruitRt, ref needDestroyFruit);
            }
            destroyFruit(curFruitRt, needDestroyFruit, isOutScreen);
        }
        clickGameOver();
    }

    private void isCutFruit(RectTransform curFruitRt, ref bool needDestroyFruit)
    {
        if (kinectManager.IsUserDetected())
        {
            //获取用户ID
            long userId = kinectManager.GetPrimaryUserID();
            int jointType = (int)KinectInterop.JointType.HandRight;
            if (kinectManager.IsJointTracked(userId, jointType))
            {
                // 获取用户相对Kinect的位置信息
                Vector3 rHandPosition = kinectManager.GetJointKinectPosition(userId, jointType);
                rightTrail.position = rHandPosition;
                Vector3 screenPositionV3 = Camera.main.WorldToScreenPoint(rHandPosition); //右手位置信息转换成在屏幕上的三维坐标
                Vector2 screenPositionV2 = new Vector2(screenPositionV3.x, screenPositionV3.y); //转换成屏幕上的二维坐标
                KinectInterop.HandState rHandState = kinectManager.GetRightHandState(userId);
                if (rHandState == KinectInterop.HandState.Open && RectTransformUtility.RectangleContainsScreenPoint(curFruitRt, screenPositionV2, Camera.main))
                {
                    //右手切中水果了
                    needDestroyFruit = true;
                    if (curFruit.getType() != Constant.Type_Bomb)
                    {
                        splatterAudio.Play();
                    }
                    else
                    {
                        boomAudio.Play();
                    }
                }
            }
            jointType = (int)KinectInterop.JointType.HandLeft;
            if (kinectManager.IsJointTracked(userId, jointType))
            {
                Vector3 lHandPosition = kinectManager.GetJointKinectPosition(userId, jointType);
                leftTrail.position = lHandPosition;
                Vector3 screenPositionV3 = Camera.main.WorldToScreenPoint(lHandPosition);
                Vector2 screenPositionV2 = new Vector2(screenPositionV3.x, screenPositionV3.y);
                KinectInterop.HandState lHandState = kinectManager.GetLeftHandState(userId);
                if (lHandState == KinectInterop.HandState.Open && RectTransformUtility.RectangleContainsScreenPoint(curFruitRt, screenPositionV2, Camera.main))
                {
                    needDestroyFruit = true;
                    if (curFruit.getType() != Constant.Type_Bomb)
                    {
                        splatterAudio.Play();
                    }
                    else
                    {
                        boomAudio.Play();
                    }
                }
            }
        }
    }

    private void destroyFruit(RectTransform curFruitRt, bool needDestroyFruit, bool isOutScreen)
    {
        float curFruitY = curFruitRt.anchoredPosition.y; //每一帧获取当前水果的Y值；
        if (curFruitY < fruitOutY)
        {
            needDestroyFruit = true;
            isOutScreen = true;
        }
        //切中水果或者水果出界，需要被销毁
        if (needDestroyFruit)
        {
            if (isOutScreen == false)
            {
                if (curFruit.getType() != Constant.Type_Bomb)
                {
                    createCutFruit();
                }
            }
            computeScoreLife(ref isOutScreen);
            Destroy(curFruit.gameObject);
            if (lifeCount > 0)
            {
                createFruit();
            }
            else
            {
                overAudio.Play();
                gameOverImg.gameObject.SetActive(true);
            }
        }
    }

    private void createFruit()
    {
        curFruit = Instantiate(fruitPrefab);  //调用预制件，克隆了一个物体,默认克隆出来的物体处于Scene层级
        curFruit.transform.SetParent(transform);   //设置克隆出来的物体处于对应的层级
        RectTransform fruitRt = curFruit.transform as RectTransform;
        int fruitX = Random.Range(minX, maxX);
        fruitRt.anchoredPosition = new Vector2(fruitX, fruitInitY);
        fruitRt.localScale = new Vector3(1, 1, 1);
        int[] fruitTypes = { Constant.Type_Bomb, Constant.Type_Apple, Constant.Type_Banana, Constant.Type_Basaha, Constant.Type_Peach, Constant.Type_Sandia };
        int typesIndex = Random.Range(0, fruitTypes.Length);
        int fruitType = fruitTypes[typesIndex];
        //fruitType = Constant.Type_Bomb;
        curFruit.setType(fruitType);
        Rigidbody2D fruitR2d = curFruit.GetComponent<Rigidbody2D>();
        int forceX = Random.Range(minForceX, maxForceX);
        int forceY = Random.Range(minForceY, maxForceY);
        if (fruitX > 0)
        {
            fruitR2d.AddForce(new Vector2(-forceX, forceY));
        }
        else
        {
            fruitR2d.AddForce(new Vector2(forceX, forceY));
        }
        throwAudio.Play();
    }

    private void createCutFruit()
    {
        //以当前水果为模板克隆出被切中后的左右两块水果
        Fruit leftCutFruit = Instantiate(curFruit, curFruit.transform.position, curFruit.transform.rotation);
        leftCutFruit.setType(curFruit.getType() + 1);
        initCutFruit(leftCutFruit, true);

        Fruit rightCutFruit = Instantiate(curFruit, curFruit.transform.position, curFruit.transform.rotation);
        rightCutFruit.setType(curFruit.getType() + 2);
        initCutFruit(rightCutFruit, false);
    }

    private void initCutFruit(Fruit fruit, bool isLeft)
    {
        RectTransform curRtf = curFruit.transform as RectTransform;
        RectTransform rtf = fruit.transform as RectTransform;
        fruit.transform.SetParent(transform);
        rtf.anchoredPosition = curRtf.anchoredPosition;
        rtf.localScale = curRtf.localScale;
        Rigidbody2D r2D = fruit.GetComponent<Rigidbody2D>();
        if (isLeft)
        {
            r2D.AddForce(new Vector2(-halfForceX, halfForceY));
        }
        else
        {
            r2D.AddForce(new Vector2(halfForceX, halfForceY));
        }
    }

    private void computeScoreLife(ref bool isOutScreen)
    {
        //出界
        if (isOutScreen == true)
        {
            if (curFruit.getType() != Constant.Type_Bomb)
            {
                //水果出界，扣生命值
                lifeCount--;
            }
        }
        //切中
        else
        {
            if (curFruit.getType() == Constant.Type_Bomb)
            {
                //切中炸弹，扣生命值
                lifeCount--;
            }
            else
            {
                //切中水果，加分
                score++;
            }
        }
        lifeContent.setLife(lifeCount);
        scoreTxt.text = "" + score;
    }

    private void clickGameOver()
    {
        if (lifeCount > 0)
        {
            return;
        }
        if (kinectManager.IsUserDetected())
        {
            long userId = kinectManager.GetPrimaryUserID();
            int jointType = (int)KinectInterop.JointType.HandRight;
            if (kinectManager.IsJointTracked(userId, jointType))
            {
                Vector3 rHandPosition = kinectManager.GetJointKinectPosition(userId, jointType);
                rightTrail.position = rHandPosition;
                Vector3 screenPositionV3 = Camera.main.WorldToScreenPoint(rHandPosition);
                Vector2 screenPositionV2 = new Vector2(screenPositionV3.x, screenPositionV3.y);
                KinectInterop.HandState rHandState = kinectManager.GetRightHandState(userId);
                RectTransform gameOverRt = gameOverImg.transform as RectTransform;
                if (rHandState == KinectInterop.HandState.Closed && RectTransformUtility.RectangleContainsScreenPoint(gameOverRt, screenPositionV2, Camera.main))
                {
                    Destroy(gameObject);
                    panelCenter.showStartPanel();
                }
            }
        }
    }
}
