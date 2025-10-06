using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlightCtrl : MonoBehaviour
{
    Rigidbody rb;

    public GameObject GoFor = null;
    Vector3 GoForVec = Vector3.zero;
    bool isGround = true;
    bool isAcceleration = false;
    float DropAcc = 0.0f;
    float speed = 20.0f;
    float Acc;
    float TurnSpeed = 20.0f;
    public GameObject Fire = null;
    public Text Speed;
    public Slider Height;
    public Text HText;
    public Image[] KeyImg;
    public Image AngleArrow;
    
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        KeyColor();
        Speed.text = rb.velocity.magnitude.ToString("N0");
        Height.value = transform.position.y / 2000;

        if (transform.position.y >= 1000)
        {
            HText.text = (transform.position.y / 1000).ToString("N2") + "km";
        }
        else if (transform.position.y < 1000)
        {
            HText.text = (transform.position.y).ToString("N0") + "m";
        }

        AngleArrow.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(GoForVec.y, GoForVec.z) * Mathf.Rad2Deg);
    }

    void Move()
    {
        if (GoForVec.magnitude >= 1.0f)
            GoForVec.Normalize();

        rb.velocity = GoForVec * Acc;

        if(transform.position.y > 15)
            isGround = false;
        
        if (isGround == true)                       //���� ������
        {
            if (rb.velocity.magnitude >= 2 && transform.position.y < 1)         //�ӵ��� ���� �پ��������� ������ȯ ����
            {
                if (Input.GetKey(KeyCode.A))            //������ȯ
                {
                    transform.Rotate(Vector3.down * Time.deltaTime * TurnSpeed, Space.Self);
                }
                if (Input.GetKey(KeyCode.D))            //������ȯ
                {
                    transform.Rotate(Vector3.up * Time.deltaTime * TurnSpeed, Space.Self);
                }
            }

            else
                Debug.Log("����");

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                isAcceleration = true;
                Fire.SetActive(true);
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                isAcceleration = false;
                Fire.SetActive(false);
            }

            if (isAcceleration == false)
            {
                GoForVec = GoFor.transform.position - transform.position;
                Acc -= Time.deltaTime * speed * 0.5f;
                if(Acc <= 0)
                    Acc = 0;
            }

            else if (isAcceleration == true)
            {
                Acc += 0.2f * speed * Time.deltaTime;

                if (Acc < 19)         //Ư���ӷº��� ���� ����ϴ� �� �ο�(���)
                {
                    GoForVec = GoFor.transform.position - transform.position;
                }
                
                if (Acc >= 19)
                {
                    transform.Rotate(0.3f * Vector3.left * Time.deltaTime * TurnSpeed, Space.Self);
                    GoForVec = GoFor.transform.position - transform.position + (Vector3.up * 0.2f);
                }
            }

            if (Acc >= 200.0f)
                Acc = 200.0f;
        }

        else if (isGround == false)                 //���߿� ������
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                isAcceleration = true;
                Fire.SetActive(true);
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                isAcceleration = false;
                Fire.SetActive(false);
            }
            
            if (isAcceleration == false)
            {
                GoForVec = GoFor.transform.position - transform.position;

                ChangAngle();

                Acc -= Time.deltaTime * speed * 0.1f;

                if(Acc < 200)
                    DropAngle();
                else
                    DropAngle(0.005f);

                if (Acc <= 0)
                    Acc = 0;
            }

            else if (isAcceleration == true)
            {
                GoForVec = GoFor.transform.position - transform.position;

                DropAngle(0.005f);

                ChangAngle();
                
                DropAcc -= Time.deltaTime * 0.015f;
                if (DropAcc <= 0.01f)
                    DropAcc = 0;

                Acc += speed * Time.deltaTime;
            }

            if (Acc > 400.0f)
                Acc = 400.0f;
        }
    }

    void ChangAngle()
    {
        if (Input.GetKey(KeyCode.A))            //��ü �¿� ���� ��ȯ
        {
            transform.Rotate(Vector3.forward * Time.deltaTime * TurnSpeed * 5.0f, Space.Self);

            DropAcc -= Time.deltaTime * 0.001f;
            if (DropAcc <= 0.01f)
                DropAcc = 0;
        }

        if (Input.GetKey(KeyCode.D))            //��ü �¿� ���� ��ȯ
        {
            transform.Rotate(Vector3.back * Time.deltaTime * TurnSpeed * 5.0f, Space.Self);

            DropAcc -= Time.deltaTime * 0.001f;
            if (DropAcc <= 0.01f)
                DropAcc = 0;
        }

        if (Input.GetKey(KeyCode.S))            //��ü �յ� ���� ��ȯ
        {
            transform.Rotate(Vector3.left * Time.deltaTime * TurnSpeed * 2.0f, Space.Self);

            DropAcc -= Time.deltaTime * 0.001f;
            if (DropAcc <= 0.01f)
                DropAcc = 0;
        }

        if (Input.GetKey(KeyCode.W))            //��ü �յ� ���� ��ȯ
        {
            transform.Rotate(Vector3.right * Time.deltaTime * TurnSpeed * 2.0f, Space.Self);

            DropAcc -= Time.deltaTime * 0.001f;
            if (DropAcc <= 0.01f)
                DropAcc = 0;
        }
    }

    void DropAngle(float DropValue = 0.0035f)
    {
        DropAcc += Time.deltaTime * DropValue;
        //(���۰�, ��ǥ��, ȸ�� �ӵ�)�� ���ڷ� �޾� ȸ�� ���� �������ִ� �޼���
        Quaternion rotateAmount = Quaternion.RotateTowards(transform.rotation, new Quaternion(0.7f, 0, 0, 0.7f), TurnSpeed * DropAcc);

        //ȸ���� ����
        transform.rotation = rotateAmount;

        if (DropAcc >= 0.05f)
            DropAcc = 0.05f;
    }

    void KeyColor()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            KeyImg[0].color = Color.green;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            KeyImg[0].color = Color.white;
        }

        if (Input.GetKey(KeyCode.W))
        {
            KeyImg[1].color = Color.green;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            KeyImg[1].color = Color.white;
        }

        if (Input.GetKey(KeyCode.A))            
        {
            KeyImg[2].color = Color.green;
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            KeyImg[2].color = Color.white;
        }

        if (Input.GetKey(KeyCode.S))            
        {
            KeyImg[3].color = Color.green;
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            KeyImg[3].color = Color.white;
        }

        if (Input.GetKey(KeyCode.D))            
        {
            KeyImg[4].color = Color.green;
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            KeyImg[4].color = Color.white;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (transform.eulerAngles.x < 350 && transform.eulerAngles.x > 10)
            Destroy(gameObject);
        if (transform.eulerAngles.z < 350 && transform.eulerAngles.z > 10)
            Destroy(gameObject);

        if (collision.gameObject.tag == "Ground")        //���� ����
            isGround = true;
    }

    //private void OnCollisionExit(Collision collision)
    //{
    //    if (collision.gameObject.tag == "Ground")       //�̷�
    //        isGround = false;
    //}
}