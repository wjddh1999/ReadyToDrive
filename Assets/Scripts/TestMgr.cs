using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestMgr : MonoBehaviour
{
    string Log;
    public List<string> LogList;
    public Text LogText;
    public GameObject End_Panel;
    public Button RePlay_Btn;

    // Start is called before the first frame update
    void Start()
    {
        End_Panel.SetActive(false);

        if (RePlay_Btn != null)
            RePlay_Btn.onClick.AddListener(() => 
            {
                SceneManager.LoadScene("PlayScene");
            });
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LogTextPrint(string a_Log = "감점")
    {
        LogText.text = "";
        Log = "[" + DateTime.Now.ToString("hh:mm:ss") + "] " + a_Log + "\n";
        Debug.Log(Log);
        LogList.Add(Log);
        if (10 < LogList.Count)
        {
            LogList.RemoveAt(0);
        }
        foreach (string log in LogList)
        {
            LogText.text += log;
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "OutLine")
        {
            LogTextPrint("감점");
        }
        if (coll.tag == "DeadZone")
        {
            CarCtrl.g_State = GameState.Wait;
            Time.timeScale = 0.0f;
            CarState.inst.SoundCtrl(CarState.inst.EngineSound, "", false);
            End_Panel.SetActive(true);
            LogTextPrint("실격");
        }
    }
}