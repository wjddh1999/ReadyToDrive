using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMgr : MonoBehaviour
{
    public GameObject Flight;
    public GameObject FlightUI;

    public GameObject Car;
    public GameObject CarUI;

    public Button Btn1;
    public Button Btn2;

    public GameObject StartPanel;
    public Button StartBtn;
    public Button HelpBtn;

    public Texture2D Mouse;

    // Start is called before the first frame update
    void Start()
    {
        StartPanel.SetActive(true);
        Flight.SetActive(false);
        FlightUI.SetActive(false);
        Car.SetActive(true);
        CarUI.SetActive(true);

        if (Btn1 != null)
            Btn1.onClick.AddListener(() =>
            {
                Flight.SetActive(true);
                FlightUI.SetActive(true);
                Car.SetActive(false);
                CarUI.SetActive(false);
            });

        if (Btn2 != null)
            Btn2.onClick.AddListener(() =>
            {
                Flight.SetActive(false);
                FlightUI.SetActive(false);
                Car.SetActive(true);
                CarUI.SetActive(true);
            });

        if (StartBtn != null)
            StartBtn.onClick.AddListener(() => 
            {
                StartPanel.SetActive(false);
                CarCtrl.g_State = GameState.Start;
                Time.timeScale = 1.0f;
            });

        if (HelpBtn != null)
            HelpBtn.onClick.AddListener(() =>
            {
                StartPanel.SetActive(true);
                CarCtrl.g_State = GameState.Wait;
                Time.timeScale = 0.0f;
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}