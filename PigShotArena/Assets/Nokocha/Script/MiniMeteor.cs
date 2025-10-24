using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class minimeteor : MonoBehaviour
{

    MapGimmick mapGimmick;

    bool HitCheck = false;
    public GameObject BoomEfe;      //�����G�t�F�N�g
    EStateMachine<minimeteor> stateMachine; 
    enum state
    {
        Move,
        Boom,
        End
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stateMachine = new EStateMachine<minimeteor>(this);
        stateMachine.Add<MoveState>((int)state.Move);
        stateMachine.Add<BoomState>((int)state.Boom);
        stateMachine.Add<EndState>((int)state.End);
        stateMachine.OnStart((int)state.Move);

        mapGimmick = GameObject.Find("GimmickManager").GetComponent<MapGimmick>();

    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.OnUpdate();
    }
    public class MoveState : EStateMachine<minimeteor>.StateBase
    {
        public override void OnStart()
        {
            //�G�t�F�N�g�̍Đ�
        }
        public override void OnUpdate()
        {
            //���������A�Ԃ�������������
            Owner.transform.Translate(Vector3.down * 5f * Time.deltaTime);

            if(Owner.HitCheck)
            {
                StateMachine.ChangeState((int)state.Boom);      //���̃X�e�[�g�Ɉڍs
            }
        }
        public override void OnEnd()
        {
            //�G�t�F�N�g�̏I��
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag=="map")
        {
            Debug.Log("meteor��map�ɗ���");
            mapGimmick.DestroyDanger();
            HitCheck = true;
        }
        var player = other.GetComponent<Player>();
        if(player != null)
        {
            player.OnHit();
            
            Debug.Log("playerHit");
        }
    }
    public class BoomState : EStateMachine<minimeteor>.StateBase
    {

        public override void OnStart()
        {
            //�G�t�F�N�g�̍Đ�
            Instantiate(Owner.BoomEfe, Owner.transform.position, Quaternion.identity);
        }
        public override void OnUpdate()
        {
            StateMachine.ChangeState((int)state.End);      //���̃X�e�[�g�Ɉڍs
        }
        public override void OnEnd()
        {

            //�G�t�F�N�g�̍Đ�
        }
    }

    

    public class EndState : EStateMachine<minimeteor>.StateBase
    {
        public override void OnStart()
        {
            Destroy(Owner.gameObject);
        }
        public override void OnUpdate()
        {

        }
        public override void OnEnd()
        {

        }
    }
}
