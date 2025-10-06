using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarState : MonoBehaviour
{
    public static CarState inst;

    Rigidbody rb;                       //velocity 계산

    bool RPMChange = false;             //기어 변속시 rpm 바꾸기용도
    [HideInInspector] public float RPM = 0.0f;                   //기어 변경시 요구치 판단
    [HideInInspector] public float speed = 20.0f;                //계산에 기반이 될 속도
    [HideInInspector] public float m_Topspeed = 10.0f;           //최고 속도
    [HideInInspector] public float m_LowSpeed;                   //각 기어별 최저 속력
    [HideInInspector] public float m_AddTorque;                   //각 기어별 추가토크

    [HideInInspector] public bool isRun = false;                 //달리고 있는 상태에서만 적용될 내용 구분용 //정지시에 제한 사항이 적용되지 않게 하기 위해
    [HideInInspector] public bool isBrake = false;                 //달리고 있는 상태에서만 적용될 내용 구분용 //정지시에 제한 사항이 적용되지 않게 하기 위해

    [HideInInspector] public bool isClutch = false;              //클러치를 밟았는가
    [HideInInspector] public int Gear = -1;                      //시동이 꺼졌을때 기어를 -1로 설정 중립(N)부터 0 1 2 3 4 5 후진(R)이 6
    [HideInInspector] public int ShiftGear = 0;                  //바꾸려는 기어 확인
    [HideInInspector] public int CacGear = 0;                    //클러치를 밟았을때 마지막으로 들어가있던 기어
    float CalcLowAcc;                                            //계산용 최저속도
    public float CurrentSpeed { get { return rb.velocity.magnitude * 3.6f; } }

    public Text SpeedText;              //현재 속도 표시 텍스트
    public Text GearText;               //현재 기어 표시 텍스트
    public Text RPMText;                //현재 rpm 표시 텍스트
    public GameObject RpmImg;           //rpm 미터계
    public GameObject SpeedImg;         //속도 미터계

    public AudioSource audioSource;
    public AudioSource EngineSound;
    public AudioSource GearSound;
    public AudioSource Warning;

    public GameObject RbCenter;

    private void Awake()
    {
        inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = RbCenter.transform.localPosition;
        audioSource = GetComponent<AudioSource>();
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        GearTable();    //기어값 테이블, 기어 변경 계산
        GearShift();
        RpmDiffuse();   //기어 변경시 rpm 변경
        MeterCtrl();    //미터계 조절
    }

    void MeterCtrl()
    {//미터계 조절
        Vector3 rpmangle = RpmImg.GetComponent<RectTransform>().transform.localEulerAngles;

        rpmangle.z = 90 - (RPM / 4000) * 135.0f;

        RpmImg.GetComponent<RectTransform>().transform.localEulerAngles = rpmangle;

        Vector3 speedangle = SpeedImg.GetComponent<RectTransform>().transform.localEulerAngles;

        speedangle.z = 90 - (rb.velocity.magnitude / 150) * 135.0f;

        if (speedangle.z < -75.0f)
            speedangle.z = -75.0f;

        SpeedImg.GetComponent<RectTransform>().transform.localEulerAngles = speedangle;

        SpeedText.text = rb.velocity.magnitude.ToString("N0") + " Km/h";
        RPMText.text = RPM.ToString("N0");
    }

    void GearTable()        //크아악 클러치 브레이크 구현하기
    {//기어 변속 테이블  //기어 변경 계산
        if (Gear != -1 && Gear != 6 && Gear != 0)
        {
            GearText.text = "기어 : " + Gear.ToString();
        }

        if (Gear == -1)
        {
            GearText.gameObject.SetActive(false);

            isRun = false;

            RPM -= 200.0f * Time.deltaTime;
            if (RPM <= 0)
                RPM = 0;
        }
        else if (Gear == 0)
        {
            GearText.gameObject.SetActive(true);

            GearText.text = "기어 : N";

            m_LowSpeed = 0.0f;

            m_AddTorque = 0.0f;
        }
        else if (Gear == 1)
        {
            m_LowSpeed = 5;

            m_Topspeed = 7.0f;

            m_AddTorque = 50;
            //if (15 <= Acc)
            //    Acc -= Time.deltaTime * speed * 20.0f;
        }
        else if (Gear == 2)
        {
            m_LowSpeed = 5;

            m_Topspeed = 25.0f;

            m_AddTorque = 50;
            //if (25 <= Acc)
            //    Acc -= Time.deltaTime * speed * 20.0f;
        }
        else if (Gear == 3)
        {
            m_LowSpeed = 20;

            m_Topspeed = 45.0f;

            m_AddTorque = 100;
            //if (45 <= Acc)
            //    Acc -= Time.deltaTime * speed * 20.0f;
        }
        else if (Gear == 4)
        {
            m_LowSpeed = 40;

            m_Topspeed = 65.0f;

            m_AddTorque = 500;
            //if (65 <= Acc)
            //    Acc -= Time.deltaTime * speed * 20.0f;
        }
        else if (Gear == 5)
        {
            m_LowSpeed = 60;

            m_Topspeed = 200.0f;

            m_AddTorque = 1000;
            //if (200 <= Acc)
            //    Acc -= Time.deltaTime * speed * 20.0f;
        }
        else if (Gear == 6)
        {//후진
            m_LowSpeed = 4;

            m_Topspeed = 8;

            GearText.text = "기어 : R";
        }
    }
    void GearShift()
    {//기어 변경 계산
        if (Input.GetKeyDown(KeyCode.LeftShift) && Gear != -1)
        {
            ShiftGear = Gear;
            CacGear = Gear;
            CalcLowAcc = m_LowSpeed;
            Gear = 0;
            isClutch = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && Gear != -1)
        {
            isClutch = false;

            if (ShiftGear == 6)
            {//후진은 항상 가능
                Gear = ShiftGear;

                GearSound.PlayOneShot(GearSound.clip, 0.7f);

                return;
            }

            if (ShiftGear - CacGear == 0)
            {//동일 기어 버튼 누를때
                Gear = ShiftGear;

                return;
            }
            if ((ShiftGear == 1 || ShiftGear == 2) && (CacGear == 0 || CacGear == 1))
            {//N,1,2단 사이에서는 자유롭게 변속
                RPMChange = true;

                Gear = ShiftGear;

                GearSound.PlayOneShot(GearSound.clip, 0.7f);

                return;
            }
            else if (ShiftGear - CacGear == 1)
            {//바로 한단 윗 기어 넣을때 
                if (2000 <= RPM)
                {//rpm 2000 넘어야
                    RPMChange = true;

                    Gear = ShiftGear;

                    GearSound.PlayOneShot(GearSound.clip, 0.7f);
                }
                else
                {//나중에 시동꺼지게 
                    Gear = CacGear;
                }

                return;
            }
            else if (2 <= ShiftGear - CacGear)
            {//2단 이상 차이나는 기어 넣을때 
                if (2000 + (500 * (ShiftGear - CacGear)) <= RPM)
                {//단수 차이마다 요구 rpm 상승
                    RPMChange = true;

                    Gear = ShiftGear;

                    GearSound.PlayOneShot(GearSound.clip, 0.7f);
                }
                else if (RPM < 2000 + (500 * (ShiftGear - CacGear)))
                {//안되면 끄기
                    Gear = -1;
                    m_LowSpeed = 0;
                    ShiftGear = 0;

                    SoundCtrl(audioSource, "Vehicle_Car_Stop_Engine_Exterior", false);
                }

                return;
            }
            else if (ShiftGear - CacGear < 0)
            {
                if (CacGear == 6)
                {
                    if ((ShiftGear == 1 || ShiftGear == 0))
                    {
                        GearSound.PlayOneShot(GearSound.clip, 0.7f);

                        Gear = ShiftGear;

                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    GearSound.PlayOneShot(GearSound.clip, 0.7f);

                    RPMChange = true;

                    Gear = ShiftGear;

                    return;
                }
            }
        }
    }

    void RpmDiffuse()
    {//기어 변경시 rpm 변경
        if (RPMChange == true)
        {
            if (CacGear < ShiftGear)
            {
                if (RPM <= 1000.0f)
                    RPMChange = false;
                RPM = Mathf.Lerp(RPM, 0.0f, Time.deltaTime * 2.0f);
            }
            else if (CacGear > ShiftGear)
            {
                if (RPM >= 2700.0f + (CacGear - ShiftGear) * 300.0f)
                    RPMChange = false;
                if (ShiftGear == 0)
                    RPMChange = false;
                if (ShiftGear == 1 && CacGear == 2)
                    RPMChange = false;

                RPM = Mathf.Lerp(RPM, 2700.0f + (CacGear - ShiftGear) * 500.0f, Time.deltaTime * 2.0f);
            }
        }
        else if (RPMChange == false)
            return;
    }

    public void SoundCtrl(AudioSource audioSource, string name, bool isLoop)
    {
        audioSource.clip = Resources.Load(name) as AudioClip;
        audioSource.loop = isLoop;
        audioSource.Play();
    }
}