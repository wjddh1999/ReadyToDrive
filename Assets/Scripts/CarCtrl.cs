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
    public Button[] GearBtn;            //기어 버튼 //추후에 드래그 방식으로 수정 희망
    //public Image[] KeyImg;            //초기에 키 입력 되나 확인용 이제 안씀

    public static GameState g_State = GameState.Wait;       //게임 진행상태 확인용

    [Header("Wheels")]
    public GameObject[] Wheels;         //타이어 오브젝트
    public WheelCollider[] wheelColliders;
    [SerializeField] private WheelEffects[] m_WheelEffects = new WheelEffects[4];
    private float m_OldRotation;
    [Range(0, 1)][SerializeField] private float m_SteerHelper; // 0 is raw physics , 1 the car will grip in the direction it is facing
    float m_MaximumSteerAngle = 30;
    float WheelAnlge = 0;

    [Header("Acc")]
    public Transform GoFor;             //나아갈 방향에 설치된 가상의 오브젝트(벡터 계산용)
    Vector3 GoForVec = Vector3.zero;    //전진벡터
    Rigidbody rb;                       //velocity 계산
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
    //항상 0번이 차량 좌측
    public Light[] Backlights;      //방향지시등
    public Light[] SubLights;       //보조조명
    public Light[] BrakeLights;     //브레이크등
    float Bright = 0.0f;            //밝기 변수
    float LightTime = 0.0f;         //시간에 따른 밝기값 조절용
    bool isLeftLightOn = false;     //좌측 깜빡이 on/off
    bool isRIghtLightOn = false;    //우측 깜빡이 on/off

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

        //초기 등 밝기 0으로 설정
        Backlights[0].intensity = 0;
        Backlights[1].intensity = 0;

        SubLights[0].intensity = 0;
        SubLights[1].intensity = 0;

        BrakeLights[0].intensity = 0;
        BrakeLights[1].intensity = 0;
        //초기 등 밝기 0으로 설정

        //버튼당 기어 할당 //추후 드래그 방식으로 수정 희망
        if (GearBtn[0] != null)
        {//중립기어
            GearBtn[0].onClick.AddListener(() => 
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                    CarState.inst.ShiftGear = 0;
            });
        }
        if (GearBtn[1] != null)
        {//1단기어
            GearBtn[1].onClick.AddListener(() =>
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                {
                    CarState.inst.ShiftGear = 1;
                }
            });
        }
        if (GearBtn[2] != null)
        {//2단기어
            GearBtn[2].onClick.AddListener(() =>
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                {
                    CarState.inst.ShiftGear = 2;
                }
            });
        }
        if (GearBtn[3] != null)
        {//3단기어
            GearBtn[3].onClick.AddListener(() =>
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                {
                    CarState.inst.ShiftGear = 3;
                }
                    
            });
        }
        if (GearBtn[4] != null)
        {//4단기어
            GearBtn[4].onClick.AddListener(() =>
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                {
                    CarState.inst.ShiftGear = 4;
                }
            });
        }
        if (GearBtn[5] != null)
        {//5단기어
            GearBtn[5].onClick.AddListener(() =>
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                {
                    CarState.inst.ShiftGear = 5;
                }
            });
        }
        if (GearBtn[6] != null)
        {//후진기어
            GearBtn[6].onClick.AddListener(() =>
            {
                if (CarState.inst.isClutch == true && CarState.inst.Gear != -1)
                {
                    CarState.inst.ShiftGear = 6;
                }
            });
        }
        //버튼당 기어 할당 //추후 드래그 방식으로 수정 희망
    }

    // Update is called once per frame
    void Update()
    {
        if (g_State == GameState.Wait)
            return;

        Brake();
        Move();         //이동 처리
        BackLight();    //방향지시등, 브레이크등 관리
    }

    void Move()
    {//차량 조작 전반
        GoForVec = GoFor.transform.position - transform.position;

        if (Input.GetKeyUp(KeyCode.Q) &&
            Input.GetKey(KeyCode.Space) &&
            Input.GetKey(KeyCode.LeftShift)
            && CarState.inst.Gear == -1)
        {//시동걸기
            //Debug.Log("시동 들어오세요");
            CarState.inst.Gear = 0;
            CarState.inst.SoundCtrl(CarState.inst.audioSource, "Vehicle_Car_Start_Engine_Exterior", false);
            CarState.inst.SoundCtrl(CarState.inst.EngineSound, "Vehicle_Car_Engine_1000_RPM_Rear_Exterior_Loop", true);
        }
        else if (Input.GetKeyUp(KeyCode.Q) &&
            Input.GetKey(KeyCode.Space) &&
            Input.GetKey(KeyCode.LeftShift)
            && CarState.inst.Gear != -1)
        {//시동걸기
            //Debug.Log("시동 나가세요");
            CarState.inst.Gear = -1;
            CarState.inst.m_LowSpeed = 0;
            CarState.inst.ShiftGear = 0;
            CarState.inst.SoundCtrl(CarState.inst.audioSource, "Vehicle_Car_Stop_Engine_Exterior", false);
            CarState.inst.EngineSound.Stop();
        }

        //좌우 방향전환
        WheelAnlge = Input.GetAxis("Horizontal");

        for (int i = 0; i < 4; i++)
        {
            Quaternion quat;
            Vector3 position;
            wheelColliders[i].GetWorldPose(out position, out quat);
            Wheels[i].transform.position = position;
            Wheels[i].transform.rotation = quat;
        }

        //타이어 최대각 제한
        WheelAnlge = Mathf.Clamp(WheelAnlge, -1, 1);
        wheelColliders[0].steerAngle = WheelAnlge * m_MaximumSteerAngle;
        wheelColliders[1].steerAngle = WheelAnlge * m_MaximumSteerAngle;

        SteerHelper();

        float thrustTorque;
        Ac = Input.GetAxis("Vertical");
        Ac = Mathf.Clamp(Ac, 0, 1);

        if (CarState.inst.isClutch == true)
            Ac = 0;

        // 속도 제한 및 보정   //똑바로 서있을때만 
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
        {//악셀
            if (CarState.inst.Gear != -1)
            {
                if (CarState.inst.RPM <= 2700)
                {//rpm 락
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
    {//똑바로 서있는지
        if(Mathf.Abs(Vector3.Dot(transform.up, Vector3.up)) < 0.5f)
            return false;

        return true;
    }

    bool isAerial()
    {//뒷바퀴가 둘다 붙어 있는지 
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
            {//밀릴때
                if (rb.velocity.magnitude <= 5.0f
                        && CarState.inst.isBrake == false
                        && CarState.inst.Gear != 0)
                {//브레이크, 클러치 안밟고 있을때 5키로도 안되면 일단 타이어 회전시키기
                    Ac = 0.5f;
                }
                else
                {
                    //Debug.Log("정상화중");
                    float speed = Mathf.SmoothDamp(rb.velocity.magnitude, CarState.inst.m_LowSpeed, ref m_LvSpeed, 2.5f);
                    Vector3 Dir = Vector3.SmoothDamp(rb.velocity.normalized, wheelHit.forwardDir.normalized, ref m_LvVec, 0.5f);

                    rb.velocity = speed * Dir;
                }
            }
            else if (Mathf.Abs(wheelHit.forwardSlip) < m_SlipLimit && Mathf.Abs(wheelHit.sidewaysSlip) < m_SlipLimit)
            {//안밀릴때
                if (rb.velocity.magnitude > CarState.inst.m_Topspeed)
                {
                    // 기어당 최대 속도 제한
                    float speed = Mathf.SmoothDamp(rb.velocity.magnitude, CarState.inst.m_Topspeed, ref m_LvSpeed, 0.5f);
                    Vector3 Dir = Vector3.SmoothDamp(rb.velocity.normalized, wheelHit.forwardDir.normalized, ref m_LvVec, 0.5f);

                    rb.velocity = speed * Dir; // 방향 유지하며 속도 감소
                }
                else if (rb.velocity.magnitude <= CarState.inst.m_LowSpeed
                        && CarState.inst.isBrake == false
                        && CarState.inst.Gear != 0)
                {//브레이크, 클러치 안밟고 있을때 최저속도
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
            // 구동력 줄이기
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
    {//브레이크 작동
        footbrake = Input.GetAxis("Jump");
        footbrake = Mathf.Clamp(footbrake, 0, 1);

        SideBrake = SideBrake_Bar.value * 1000;

        for (int i = 0; i < 4; i++)
        {
            wheelColliders[i].brakeTorque = m_BrakeTorque * footbrake + SideBrake;
        }

        if (footbrake != 0)
        {// 스페이스 입력시
            CarState.inst.isBrake = true;
        }
        else if (footbrake == 0)
        {
            CarState.inst.isBrake = false;
        }

        if (rb.velocity.magnitude < CarState.inst.m_LowSpeed - 5.0f 
            && CarState.inst.isClutch == false
            && CarState.inst.isBrake == true)
        {//시동 꺼질때 계산 다 초기화
            CarState.inst.Gear = -1;
            CarState.inst.m_LowSpeed = 0;
            CarState.inst.ShiftGear = 0;
            CarState.inst.SoundCtrl(CarState.inst.audioSource, "Vehicle_Car_Stop_Engine_Exterior", false);
            CarState.inst.EngineSound.Stop();
        }
    }

    void BackLight()
    {//방향지시등, 브레이크등 관리
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
        {//우측깜빡이 켜고 우회전을 끝냈을때
            if (Input.GetKeyUp(KeyCode.D))
            {
                isRIghtLightOn = false;
                CarState.inst.Warning.Stop();
            }
        }

        if (isRIghtLightOn == false && isLeftLightOn == true)
        {//좌측깜빡이 켜고 좌회전을 끝냈을때
            if (Input.GetKeyUp(KeyCode.A))
            {
                isLeftLightOn = false;
                CarState.inst.Warning.Stop();
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {//좌측 깜빡이
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
        {//우측 깜빡이
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
        {//비상등
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