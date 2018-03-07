using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeContent : MonoBehaviour {
    public Toggle life1;
    public Toggle life2;
    public Toggle life3;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setLife(int lifeCount)
    {
        switch (lifeCount)
        {
            case 0:
                life1.isOn = false;
                life2.isOn = false;
                life3.isOn = false;
                break;
            case 1:
                life1.isOn = false;
                life2.isOn = false;
                life3.isOn = true;
                break;
            case 2:
                life1.isOn = false;
                life2.isOn = true;
                life3.isOn = true;
                break;
            case 3:
                life1.isOn = true;
                life2.isOn = true;
                life3.isOn = true;
                break;
            default:
                life1.isOn = false;
                life2.isOn = false;
                life3.isOn = false;
                break;
        }
    }
}
