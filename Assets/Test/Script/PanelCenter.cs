using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelCenter : MonoBehaviour {
    public RectTransform startPanel;
    public RectTransform gamePanel;
	// Use this for initialization
	void Start () {
        createStartPanel();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void createStartPanel()
    {
        RectTransform startPanelClone = Instantiate(startPanel, startPanel.position, startPanel.rotation);
        startPanelClone.SetParent(transform);
        startPanelClone.anchoredPosition3D = new Vector3(0, 0, 0);
        startPanelClone.localScale = new Vector3(1,1,1);
        startPanelClone.offsetMin = new Vector2(0, 0);
        startPanelClone.offsetMax = new Vector2(0, 0);
    }
    private void createGamePanel()
    {
        RectTransform gamePanelClone = Instantiate(gamePanel, gamePanel.position, gamePanel.rotation);
        gamePanelClone.SetParent(transform);
        gamePanelClone.anchoredPosition3D = new Vector3(0, 0, 0);
        gamePanelClone.localScale = new Vector3(1, 1, 1);
        gamePanelClone.offsetMin = new Vector2(0, 0);
        gamePanelClone.offsetMax = new Vector2(0, 0);
    }

    public void showStartPanel()
    {
        createStartPanel();
    }

    public void showGamePanel()
    {
        createGamePanel();
    }
}
