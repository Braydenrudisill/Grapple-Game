using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelector : MonoBehaviour
{
    public GameObject levelHolder;
    public GameObject levelIcon;
    public GameObject thisCanvas;
    public int numberOfLevels;
    private Rect panelDimensions;
    private Rect iconDimensions;
    private int amountPerPage;
    private int currentLevelCount;
    public int totalPages;
    // Start is called before the first frame update
    void Start()
    {
        LoadLevels();
        panelDimensions = levelHolder.GetComponent<RectTransform>().rect;
        iconDimensions = levelIcon.GetComponent<RectTransform>().rect;
        int maxInARow = Mathf.FloorToInt(panelDimensions.width / (iconDimensions.width+50));
        int maxInACol = Mathf.FloorToInt(panelDimensions.height / (iconDimensions.height+50));
        amountPerPage = maxInACol * maxInARow;
        Debug.Log((maxInACol,maxInARow,amountPerPage,panelDimensions.width, iconDimensions.width, panelDimensions.height / iconDimensions.height));
        totalPages = Mathf.CeilToInt((float)numberOfLevels / amountPerPage);
        LoadPanels(totalPages);
    }

    void LoadLevels()
    {

    }

    void LoadPanels(int numberOfPanels)
    {
        GameObject panelClone = Instantiate(levelHolder) as GameObject;

        for (int i = 1; i <= numberOfPanels; i++) 
        {
            GameObject panel = Instantiate(panelClone) as GameObject;
            panel.transform.SetParent(thisCanvas.transform, false);
            panel.transform.SetParent(levelHolder.transform);
            panel.name = "Page-" + i;
            panel.GetComponent<RectTransform>().localPosition = new Vector2(panelDimensions.width * (i - 1), 0);
            panel.GetComponent<PageSwiper>().enabled = false;
            SetUpGrid(panel);
            int numberOfIcons = i == numberOfPanels ? numberOfLevels - currentLevelCount : amountPerPage;
            LoadIcons(numberOfIcons, panel);
        }
        Destroy(panelClone);
    }

    void SetUpGrid(GameObject panel)
    {
        GridLayoutGroup grid = panel.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(iconDimensions.width, iconDimensions.height);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.spacing = new Vector2(50, 50);
        grid.padding.right = 50;

    }

    void LoadIcons(int numberOfIcons, GameObject parentObject )
    {
        for (int i = 1; i <= numberOfIcons; i++)
        {
            currentLevelCount++;
            GameObject icon = Instantiate(levelIcon) as GameObject;
            icon.transform.SetParent(thisCanvas.transform, false);
            icon.transform.SetParent(parentObject.transform);
            icon.name = "Level " + i;
            icon.GetComponentInChildren<TextMeshProUGUI>().text = "Level " + currentLevelCount;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
