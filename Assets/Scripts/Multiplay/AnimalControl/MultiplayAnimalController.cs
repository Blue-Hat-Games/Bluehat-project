using Photon.Pun;
using UnityEngine;

public class MultiplayAnimalController : MonoBehaviourPun, IPunObservable
{
    public float rotSpeed = 10;

    private readonly string ANIM_PARAMETER_JUMP = "Jump";
    private readonly string ANIM_PARAMETER_MOTIONSPEED = "MotionSpeed";
    private readonly float jumpPower = 5;

    private readonly float moveSpeed = 10;
    private Animator animator;
    private float camAngle;

    private bool isGround = true;
    protected Joybutton joybutton;

    protected Joystick joystick;
    private Quaternion remoteCamRot = Quaternion.identity;

    // 받은 데이터 기억 변수 (보간처리하기 위해서)
    private Vector3 remotePos = Vector3.zero;
    private Quaternion remoteRot = Quaternion.identity;
    private Vector3 remoteScale = Vector3.zero;

    private Rigidbody rigid;

    private void Start()
    {
        joystick = FindObjectOfType<Joystick>();
        joybutton = FindObjectOfType<Joybutton>();

        rigid = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // 리모트 캐릭터 처리
        if (photonView.IsMine == false)
        {
            ControlRemotePlayer();
            return;
        }

        // 애니메이터 파라미터 설정 
        animator.SetFloat(ANIM_PARAMETER_MOTIONSPEED, joystick.InputScale);
        // 이동 방향으로 회전  
        rigid.velocity = new Vector3(joystick.Horizontal * moveSpeed, rigid.velocity.y, joystick.Vertical * moveSpeed);

        var h = joystick.Horizontal;
        var v = joystick.Vertical;

        var dir = new Vector3(h, 0, v);

        if (!(h == 0 && v == 0))
            // 바라보는 방향으로 회전
            transform.rotation =
                Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed);

        // Jump에 대한 처리
        if (isGround && joybutton.Pressed)
        {
            rigid.velocity = Vector3.up * jumpPower;
            animator.SetTrigger(ANIM_PARAMETER_JUMP);
            isGround = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 부딪힌 물체의 태그가 "Ground"라면
        if (collision.gameObject.CompareTag("Ground"))
            // isGround를 true로 변경
            isGround = true;
    }

    // IPunObservable 상속 시 꼭 구현해야 하는 것
    // - 데이터를 네트워크 사용자 간에 보내고 받고 하게 하는 콜백 함수
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 내가 데이터를 보내는 중이라면
        if (stream.IsWriting) // 내꺼보내는 거
        {
            // 이 방안에 있는 모든 사용자에게 브로드캐스트 
            // - 내 포지션 값을 보내보자
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(transform.localScale);
            //stream.SendNext(camera.rotation);
        }
        // 내가 데이터를 받는 중이라면 
        else // 원격에 있는 나 
        {
            // 순서대로 보내면 순서대로 들어옴. 근데 타입캐스팅 해주어야 함
            remotePos = (Vector3)stream.ReceiveNext();
            remoteRot = (Quaternion)stream.ReceiveNext();
            remoteScale = (Vector3)stream.ReceiveNext();
            //remoteCamRot = (Quaternion)stream.ReceiveNext();
        }
    }

    private void ControlRemotePlayer()
    {
        transform.position = Vector3.Lerp(transform.position, remotePos, 10 * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, remoteRot, 10 * Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, remoteScale, 10 * Time.deltaTime);
        //camera.rotation = Quaternion.Lerp(camera.rotation, remoteCamRot, 10 * Time.deltaTime);
    }
}