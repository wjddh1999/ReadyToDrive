using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarState : MonoBehaviour
{
    public static CarState inst;

    Rigidbody rb;                       //velocity ���

    bool RPMChange = false;             //��� ���ӽ� rpm �ٲٱ�뵵
    [HideInInspector] public float RPM = 0.0f;                   //��� ����� �䱸ġ �Ǵ�
    [HideInInspector] public float speed = 20.0f;                //��꿡 ����� �� �ӵ�
    [HideInInspector] public float m_Topspeed = 10.0f;           //�ְ� �ӵ�
    [HideInInspector] public float m_LowSpeed;                   //�� �� ���� �ӷ�
    [HideInInspector] public float m_AddTorque;                   //�� �� �߰���ũ

    [HideInInspector] public bool isRun = false;                 //�޸��� �ִ� ���¿����� ����� ���� ���п� //�����ÿ� ���� ������ ������� �ʰ� �ϱ� ����
    [HideInInspector] public bool isBrake = false;                 //�޸��� �ִ� ���¿����� ����� ���� ���п� //�����ÿ� ���� ������ ������� �ʰ� �ϱ� ����

    [HideInInspector] public bool isClutch = false;              //Ŭ��ġ�� ��Ҵ°�
    [HideInInspector] public int Gear = -1;                      //�õ��� �������� �� -1�� ���� �߸�(N)���� 0 1 2 3 4 5 ����(R)�� 6
    [HideInInspector] public int ShiftGear = 0;                  //�ٲٷ��� ��� Ȯ��
    [HideInInspector] public int CacGear = 0;                    //Ŭ��ġ�� ������� ���������� ���ִ� ���
    float CalcLowAcc;                                            //���� �����ӵ�
    public float CurrentSpeed { get { return rb.velocity.magnitude * 3.6f; } }

    public Text SpeedText;              //���� �ӵ� ǥ�� �ؽ�Ʈ
    public Text GearText;               //���� ��� ǥ�� �ؽ�Ʈ
    public Text RPMText;                //���� rpm ǥ�� �ؽ�Ʈ
    public GameObject RpmImg;           //rpm ���Ͱ�
    public GameObject SpeedImg;         //�ӵ� ���Ͱ�

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
        GearTable();    //�� ���̺�, ��� ���� ���
        GearShift();
        RpmDiffuse();   //��� ����� rpm ����
        MeterCtrl();    //���Ͱ� ����
    }

    void MeterCtrl()
    {//���Ͱ� ����
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

    void GearTable()        //ũ�ƾ� Ŭ��ġ �극��ũ �����ϱ�
    {//��� ���� ���̺�  //��� ���� ���
        if (Gear != -1 && Gear != 6 && Gear != 0)
        {
            GearText.text = "��� : " + Gear.ToString();
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

            GearText.text = "��� : N";

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
        {//����
            m_LowSpeed = 4;

            m_Topspeed = 8;

            GearText.text = "��� : R";
        }
    }
    void GearShift()
    {//��� ���� ���
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
            {//������ �׻� ����
                Gear = ShiftGear;

                GearSound.PlayOneShot(GearSound.clip, 0.7f);

                return;
            }

            if (ShiftGear - CacGear == 0)
            {//���� ��� ��ư ������
                Gear = ShiftGear;

                return;
            }
            if ((ShiftGear == 1 || ShiftGear == 2) && (CacGear == 0 || CacGear == 1))
            {//N,1,2�� ���̿����� �����Ӱ� ����
                RPMChange = true;

                Gear = ShiftGear;

                GearSound.PlayOneShot(GearSound.clip, 0.7f);

                return;
            }
            else if (ShiftGear - CacGear == 1)
            {//�ٷ� �Ѵ� �� ��� ������ 
                if (2000 <= RPM)
                {//rpm 2000 �Ѿ��
                    RPMChange = true;

                    Gear = ShiftGear;

                    GearSound.PlayOneShot(GearSound.clip, 0.7f);
                }
                else
                {//���߿� �õ������� 
                    Gear = CacGear;
                }

                return;
            }
            else if (2 <= ShiftGear - CacGear)
            {//2�� �̻� ���̳��� ��� ������ 
                if (2000 + (500 * (ShiftGear - CacGear)) <= RPM)
                {//�ܼ� ���̸��� �䱸 rpm ���
                    RPMChange = true;

                    Gear = ShiftGear;

                    GearSound.PlayOneShot(GearSound.clip, 0.7f);
                }
                else if (RPM < 2000 + (500 * (ShiftGear - CacGear)))
                {//�ȵǸ� ����
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
    {//��� ����� rpm ����
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