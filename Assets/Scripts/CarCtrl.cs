using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Vehicles.Car;

public enum GameState
{
    Start,
    Wait
}
public class CarCtrl : MonoBehaviour
{
    public Button[] GearBtn;            //��� ��ư //���Ŀ� �巡�� ������� ���� ���
    //public Image[] KeyImg;            //�ʱ⿡ Ű �Է� �ǳ� Ȯ�ο� ���� �Ⱦ�

    public static GameState g_State = GameState.Wait;       //���� ������� Ȯ�ο�

    [Header("Wheels")]
    public GameObject[] Wheels;         //Ÿ�̾� ������Ʈ
    public WheelCollider[] wheelColliders;
    [SerializeField] private WheelEffects[] m_WheelEffects = new WheelEffects[4];
    private float m_OldRotation;
    [Range(0, 1)][SerializeField] private float m_SteerHelper; // 0 is raw physics , 1 the car will grip in the direction it is facing
    float m_MaximumSteerAngle = 30;
    float WheelAnlge = 0;

    [Header("Acc")]
    public Transform GoFor;             //���ư� ���⿡ ��ġ�� ������ ������Ʈ(���� ����)
    Vector3 GoForVec = Vector3.zero;    //��������
    Rigidbody rb;                       //velocity ���
    [Range(0, 1)][SerializeField] private float m_TractionControl; // 0 is no traction control, 1 is full interference
    [SerializeField] private float m_SlipLimit;
    [SerializeField] private float m_FullTorqueOverAllWheels;
    private float m_CurrentTorque;
    float footbrake;
    [SerializeField] private float m_BrakeTorque;
    float Ac = 0.0f;
    float SideBrake;
    public Scrollbar SideBrake_Bar;

    [Header("Light")]
    //�׻� 0���� ���� ����
    public Light[] Backlights;      //�������õ�
    public Light[] SubLights;       //��������
    public Light[] BrakeLights;     //�극��ũ��
    float Bright = 0.0f;            //��� ����
    float LightTime = 0.0f;         //�ð��� ���� ��Ⱚ ������
    bool isLeftLightOn = false;     //���� ������ on/off
    bool isRIghtLightOn = false;    //���� ������ on/off

    float m_LvSpeed = 0.0f;
    Vector3 m_LvVec = Vector3.zero;
    bool isNomal = true;
    bool isSlip = false;

    //Vector3 cac1, cac2;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        SideBrake = 1000;

        m_CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl * m_FullTorqueOverAllWheels);

        //�ʱ� �� ��� 0���� ����
        Backlights[0].intensity = 0;
        Backlights[1].intensity = 0;

        SubLights[0].intensity = 0;
        SubLights[1].intensity = 0;

        BrakeLights[0].intensity = 0;
        BrakeLights[1].intensity = 0;
        //�ʱ� �� ��� 0���� ����

        //��ư�� ��� �Ҵ� //���� �巡�� ������� ���� ���
        if (GearBtn[0] != null)
        {//�߸����
            GearBtn[0].onClick.AddListener(() => 
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                    CarState.inst.ShiftGear = 0;
            });
        }
        if (GearBtn[1] != null)
        {//1�ܱ��
            GearBtn[1].onClick.AddListener(() =>
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                {
                    CarState.inst.ShiftGear = 1;
                }
            });
        }
        if (GearBtn[2] != null)
        {//2�ܱ��
            GearBtn[2].onClick.AddListener(() =>
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                {
                    CarState.inst.ShiftGear = 2;
                }
            });
        }
        if (GearBtn[3] != null)
        {//3�ܱ��
            GearBtn[3].onClick.AddListener(() =>
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                {
                    CarState.inst.ShiftGear = 3;
                }
                    
            });
        }
        if (GearBtn[4] != null)
        {//4�ܱ��
            GearBtn[4].onClick.AddListener(() =>
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                {
                    CarState.inst.ShiftGear = 4;
                }
            });
        }
        if (GearBtn[5] != null)
        {//5�ܱ��
            GearBtn[5].onClick.AddListener(() =>
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                {
                    CarState.inst.ShiftGear = 5;
                }
            });
        }
        if (GearBtn[6] != null)
        {//�������
            GearBtn[6].onClick.AddListener(() =>
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                {
                    CarState.inst.ShiftGear = 6;
                }
            });
        }
        //��ư�� ��� �Ҵ� //���� �巡�� ������� ���� ���
    }

    // Update is called once per frame
    void Update()
    {
        if (g_State == GameState.Wait)
            return;

        Brake();
        Move();         //�̵� ó��
        BackLight();    //�������õ�, �극��ũ�� ����
    }

    void Move()
    {//���� ���� ����
        GoForVec = GoFor.transform.position - transform.position;

        if (Input.GetKeyUp(KeyCode.Q) &&
            Input.GetKey(KeyCode.Space) &&
            Input.GetKey(KeyCode.LeftShift)
            && CarState.inst.Gear == -1)
        {//�õ��ɱ�
            //Debug.Log("�õ� ��������");
            CarState.inst.Gear = 0;
            CarState.inst.SoundCtrl(CarState.inst.audioSource, "Vehicle_Car_Start_Engine_Exterior", false);
            CarState.inst.SoundCtrl(CarState.inst.EngineSound, "Vehicle_Car_Engine_1000_RPM_Rear_Exterior_Loop", true);
        }
        else if (Input.GetKeyUp(KeyCode.Q) &&
            Input.GetKey(KeyCode.Space) &&
            Input.GetKey(KeyCode.LeftShift)
            && CarState.inst.Gear != -1)
        {//�õ��ɱ�
            //Debug.Log("�õ� ��������");
            CarState.inst.Gear = -1;
            CarState.inst.m_LowSpeed = 0;
            CarState.inst.ShiftGear = 0;
            CarState.inst.SoundCtrl(CarState.inst.audioSource, "Vehicle_Car_Stop_Engine_Exterior", false);
            CarState.inst.EngineSound.Stop();
        }

        //�¿� ������ȯ
        WheelAnlge = Input.GetAxis("Horizontal");

        for (int i = 0; i < 4; i++)
        {
            Quaternion quat;
            Vector3 position;
            wheelColliders[i].GetWorldPose(out position, out quat);
            Wheels[i].transform.position = position;
            Wheels[i].transform.rotation = quat;
        }

        //Ÿ�̾� �ִ밢 ����
        WheelAnlge = Mathf.Clamp(WheelAnlge, -1, 1);
        wheelColliders[0].steerAngle = WheelAnlge * m_MaximumSteerAngle;
        wheelColliders[1].steerAngle = WheelAnlge * m_MaximumSteerAngle;

        SteerHelper();

        float thrustTorque;
        Ac = Input.GetAxis("Vertical");
        Ac = Mathf.Clamp(Ac, 0, 1);

        if (CarState.inst.isClutch == true)
            Ac = 0;

        // �ӵ� ���� �� ����   //�ȹٷ� ���������� 
        if(isRight() == true 
            && isAerial() == true)
            SpeedCtrl();

        if (CarState.inst.Gear == 6)
        {
            thrustTorque = (Ac * (m_CurrentTorque / 2f));
            wheelColliders[2].motorTorque = wheelColliders[3].motorTorque = -thrustTorque;
        }
        else if (CarState.inst.Gear != 0 && CarState.inst.Gear != -1)
        {
            thrustTorque = (Ac * (m_CurrentTorque / 2f)) + CarState.inst.m_AddTorque;
            wheelColliders[2].motorTorque = wheelColliders[3].motorTorque = thrustTorque;
        }
        else if ((CarState.inst.Gear == 0 || CarState.inst.Gear == -1))
        {
            Ac = 0;
            thrustTorque = Ac * (m_CurrentTorque / 2f) + 0.0001f;
            wheelColliders[2].motorTorque = wheelColliders[3].motorTorque = thrustTorque;
        }
        TractionControl();
        

        CarState.inst.RPM -= Time.deltaTime * 300.0f;

        if (CarState.inst.RPM <= 800 && CarState.inst.Gear != -1)
        {
            CarState.inst.RPM += Time.deltaTime * 1500;
        }

        if (Input.GetKey(KeyCode.W))
        {//�Ǽ�
            if (CarState.inst.Gear != -1)
            {
                if (CarState.inst.RPM <= 2700)
                {//rpm ��
                    CarState.inst.RPM += 750.0f * Time.deltaTime;
                }
                else if (CarState.inst.RPM <= 3600)
                {
                    CarState.inst.RPM += 350.0f * Time.deltaTime;
                }
                else if (CarState.inst.RPM <= 4000)
                {
                    CarState.inst.RPM += 100.0f * Time.deltaTime;
                }
                else
                {
                    CarState.inst.RPM += 15.0f * Time.deltaTime;
                }
            }
        }
    }

    bool isRight()
    {//�ȹٷ� ���ִ���
        if(Mathf.Abs(Vector3.Dot(transform.up, Vector3.up)) < 0.5f)
            return false;

        return true;
    }

    bool isAerial()
    {//�޹����� �Ѵ� �پ� �ִ��� 
        WheelHit wheelHit;

        if(wheelColliders[3].GetGroundHit(out wheelHit) == false 
            && wheelColliders[2].GetGroundHit(out wheelHit) == false)
            return false;
        else
            return true;
    }

    void SpeedCtrl()
    {
        WheelHit wheelHit;
        wheelColliders[0].GetGroundHit(out wheelHit);

        Vector3 CacForDir = wheelHit.forwardDir.normalized;
        Vector3 CacRbDir = rb.velocity.normalized;

        if (CarState.inst.Gear == 6)
        {
            if (rb.velocity.magnitude < CarState.inst.m_LowSpeed)
                Ac = 0.5f;
            else
            {
                float speed = Mathf.SmoothDamp(rb.velocity.magnitude, CarState.inst.m_LowSpeed, ref m_LvSpeed, 0.5f);
                Vector3 Dir = Vector3.SmoothDamp(rb.velocity.normalized, -wheelHit.forwardDir.normalized, ref m_LvVec, 0.1f);

                rb.velocity = speed * Dir;
            }
        }
        else
        {
            //if (Mathf.Abs(Vector3.Dot(CacForDir, CacRbDir)) < 0.5f
            if (Mathf.Abs(wheelHit.forwardSlip) >= m_SlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= m_SlipLimit
            && (CarState.inst.Gear != 0 || CarState.inst.Gear != -1))
            {//�и���
                if (rb.velocity.magnitude <= 5.0f
                        && CarState.inst.isBrake == false
                        && CarState.inst.Gear != 0)
                {//�극��ũ, Ŭ��ġ �ȹ�� ������ 5Ű�ε� �ȵǸ� �ϴ� Ÿ�̾� ȸ����Ű��
                    Ac = 0.5f;
                }
                else
                {
                    //Debug.Log("����ȭ��");
                    float speed = Mathf.SmoothDamp(rb.velocity.magnitude, CarState.inst.m_LowSpeed, ref m_LvSpeed, 2.5f);
                    Vector3 Dir = Vector3.SmoothDamp(rb.velocity.normalized, wheelHit.forwardDir.normalized, ref m_LvVec, 0.5f);

                    rb.velocity = speed * Dir;
                }
            }
            else if (Mathf.Abs(wheelHit.forwardSlip) < m_SlipLimit && Mathf.Abs(wheelHit.sidewaysSlip) < m_SlipLimit)
            {//�ȹи���
                if (rb.velocity.magnitude > CarState.inst.m_Topspeed)
                {
                    // ���� �ִ� �ӵ� ����
                    float speed = Mathf.SmoothDamp(rb.velocity.magnitude, CarState.inst.m_Topspeed, ref m_LvSpeed, 0.5f);
                    Vector3 Dir = Vector3.SmoothDamp(rb.velocity.normalized, wheelHit.forwardDir.normalized, ref m_LvVec, 0.5f);

                    rb.velocity = speed * Dir; // ���� �����ϸ� �ӵ� ����
                }
                else if (rb.velocity.magnitude <= CarState.inst.m_LowSpeed
                        && CarState.inst.isBrake == false
                        && CarState.inst.Gear != 0)
                {//�극��ũ, Ŭ��ġ �ȹ�� ������ �����ӵ�
                    Ac = 0.5f;
                }
            }
        }
    }

    private void SteerHelper()
    {
        isSlip = false;
        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelhit;
            wheelColliders[i].GetGroundHit(out wheelhit);
            if (wheelhit.normal == Vector3.zero)
                return; // wheels arent on the ground so dont realign the rigidbody velocity
        }

        // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
        if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
        {
            isSlip = true;
            var turnadjust = (transform.eulerAngles.y - m_OldRotation) * m_SteerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            rb.velocity = velRotation * rb.velocity;
        }
        m_OldRotation = transform.eulerAngles.y;
    }

    private void AdjustTorque(WheelHit hit)
    {
        if (Mathf.Abs(hit.forwardSlip) >= m_SlipLimit || Mathf.Abs(hit.sidewaysSlip) >= m_SlipLimit)
        {
            // ������ ���̱�
            m_CurrentTorque -= 10f * m_TractionControl;
            if (m_CurrentTorque <= 0)
                m_CurrentTorque = 0;
        }
        else
        {
            m_CurrentTorque += 10 * m_TractionControl;
            if (m_CurrentTorque > m_FullTorqueOverAllWheels)
            {
                m_CurrentTorque = m_FullTorqueOverAllWheels;
            }
        }
    }

    private void TractionControl()
    {
        WheelHit wheelHit;

        for (int i = 0; i < wheelColliders.Length; i++) 
        {
            wheelColliders[i].GetGroundHit(out wheelHit);

            if (Mathf.Abs(wheelHit.sidewaysSlip) > m_SlipLimit)
            {//Mathf.Abs(wheelHit.forwardSlip) > m_SlipLimit || 
                if (!m_WheelEffects[i].skidding)
                {
                    StartCoroutine(m_WheelEffects[i].StartSkidTrail());
                }

                if (!AnySkidSoundPlaying())
                {
                    m_WheelEffects[i].PlayAudio();
                }
                continue;
            }

            // if it wasnt slipping stop all the audio
            if (m_WheelEffects[i].PlayingAudio)
            {
                m_WheelEffects[i].StopAudio();
            }

            m_WheelEffects[i].EndSkidTrail();
            AdjustTorque(wheelHit);
        }
    }
    private bool AnySkidSoundPlaying()
    {
        for (int i = 0; i < 4; i++)
        {
            if (m_WheelEffects[i].PlayingAudio)
            {
                return true;
            }
        }
        return false;
    }

    void Brake()
    {//�극��ũ �۵�
        footbrake = Input.GetAxis("Jump");
        footbrake = Mathf.Clamp(footbrake, 0, 1);

        SideBrake = SideBrake_Bar.value * 1000;

        for (int i = 0; i < 4; i++)
        {
            wheelColliders[i].brakeTorque = m_BrakeTorque * footbrake + SideBrake;
        }

        if (footbrake != 0)
        {// �����̽� �Է½�
            CarState.inst.isBrake = true;
        }
        else if (footbrake == 0)
        {
            CarState.inst.isBrake = false;
        }

        if (rb.velocity.magnitude < CarState.inst.m_LowSpeed - 5.0f 
            && CarState.inst.isClutch == false
            && CarState.inst.isBrake == true)
        {//�õ� ������ ��� �� �ʱ�ȭ
            CarState.inst.Gear = -1;
            CarState.inst.m_LowSpeed = 0;
            CarState.inst.ShiftGear = 0;
            CarState.inst.SoundCtrl(CarState.inst.audioSource, "Vehicle_Car_Stop_Engine_Exterior", false);
            CarState.inst.EngineSound.Stop();
        }
    }

    void BackLight()
    {//�������õ�, �극��ũ�� ����
        if (CarState.inst.isBrake == true)
        {
            BrakeLights[0].intensity = 5f;
            BrakeLights[1].intensity = 5f;
        }
        else
        {
            BrakeLights[0].intensity = 0.0f;
            BrakeLights[1].intensity = 0.0f;
        }

        LightTime += Time.deltaTime * 3.0f;

        if (isRIghtLightOn == true && isLeftLightOn == false)
        {//���������� �Ѱ� ��ȸ���� ��������
            if (Input.GetKeyUp(KeyCode.D))
            {
                isRIghtLightOn = false;
                CarState.inst.Warning.Stop();
            }
        }

        if (isRIghtLightOn == false && isLeftLightOn == true)
        {//���������� �Ѱ� ��ȸ���� ��������
            if (Input.GetKeyUp(KeyCode.A))
            {
                isLeftLightOn = false;
                CarState.inst.Warning.Stop();
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {//���� ������
            if (isRIghtLightOn == true && isLeftLightOn == true)
            {
                isRIghtLightOn = false;
                return;
            }
            else if (isRIghtLightOn == true && isLeftLightOn == false)
            {
                isRIghtLightOn = !isRIghtLightOn;
            }

            isLeftLightOn = !isLeftLightOn;
            Bright = 4.5f;

            if (isLeftLightOn == true)
                CarState.inst.SoundCtrl(CarState.inst.Warning, "Vehicle_Car_Warning_Interior_Loop_01", true);
            else
            {
                CarState.inst.Warning.Stop();
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {//���� ������
            if (isRIghtLightOn == true && isLeftLightOn == true)
            {
                isLeftLightOn = !isLeftLightOn;
                return;
            }
            else if (isLeftLightOn == true && isRIghtLightOn == false)
            {
                isLeftLightOn = !isLeftLightOn;
            }

            isRIghtLightOn = !isRIghtLightOn;

            Bright = 4.5f;

            if (isRIghtLightOn == true)
                CarState.inst.SoundCtrl(CarState.inst.Warning, "Vehicle_Car_Warning_Interior_Loop_01", true);
            else
            {
                CarState.inst.Warning.Stop();
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {//����
            if (isRIghtLightOn == true && isLeftLightOn == true)
            {
                CarState.inst.Warning.Stop();
                isRIghtLightOn = false;
                isLeftLightOn = false;
                return;
            }
            CarState.inst.SoundCtrl(CarState.inst.Warning, "Vehicle_Car_Warning_Interior_Loop_01", true);
            isRIghtLightOn = true;
            isLeftLightOn = true;

            Bright = 4.5f;
        }

        if (isLeftLightOn == true)
        {
            Bright = Mathf.Cos(LightTime) * 4.5f;

            Backlights[0].intensity = Mathf.Abs(Bright);
            SubLights[0].intensity = Mathf.Abs(Bright);
        }
        else
        {
            Backlights[0].intensity = 0.0f;
            SubLights[0].intensity = 0.0f;
        }

        if (isRIghtLightOn == true)
        {
            Bright = Mathf.Cos(LightTime) * 4.5f;
            
            Backlights[1].intensity = Mathf.Abs(Bright);
            SubLights[1].intensity = Mathf.Abs(Bright);
        }
        else
        {
            Backlights[1].intensity = 0.0f;
            SubLights[1].intensity = 0.0f;
        }
    }
}