using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class GoalControler : MonoBehaviour
{
    public Rigidbody player;
    public PlayerMovement pm;
    public GrapplingGun gg;
    public TextMeshProUGUI text;
    public Image crosshair;
    double timer;
    private bool ended;
    private void Start()
    {
        ended = false;
        crosshair.enabled = true;
        text.enabled = false;
        timer = 0;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Player")
        {
            EndLevel();
            DisplayStats();
        }
    }
    private void DisplayStats()
    {
        Debug.Log(timer.ToString("F2") + " seconds to complete");
        text.enabled = true;
        text.text = timer.ToString("F2");
    }
    private void EndLevel()
    {
        player.useGravity = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        player.velocity = new Vector3(0, 0, 0);
        crosshair.enabled = false;
        gg.enabled = false;
        pm.enabled = false;
        ended = true;
        
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if (ended && Input.GetMouseButtonDown(0))
        {
            GoToMainMenu();
        }
    }
    private void GoToMainMenu()
    {
        SceneManager.LoadScene(1);
    }
}
