using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fruit : MonoBehaviour {
    private int type;
    public Sprite[] fruitSprites;
    public GameObject particleObj;
    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    // Use this for initialization
    void Start () {
        image.sprite = fruitSprites[type];
        image.SetNativeSize();
        if (type == Constant.Type_Bomb)
        {
            particleObj.SetActive(true);
        }
        else
        {
            particleObj.SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //碰撞器触发器的回调方法
    void OnTriggerEnter2D(Collider2D other)
    {
        //表示水果碰到了标识为“ColliderBound”的Collider
        if (other.transform.tag == "ColliderBound")
        {
            if(type!= Constant.Type_Bomb && type!=Constant.Type_Apple&& type != Constant.Type_Banana && type != Constant.Type_Basaha && type != Constant.Type_Peach && type != Constant.Type_Sandia)
            {
                //销毁被切成两半的水果
                if (gameObject != null)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    public void setType(int type)
    {
        this.type = type;
    }

    public int getType()
    {
        return type;
    }
}
