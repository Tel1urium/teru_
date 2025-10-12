using UnityEngine;

public class minimeteor : MonoBehaviour
{

    bool HitCheck = false;
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
            HitCheck = true;
        }
    }

    public class BoomState : EStateMachine<minimeteor>.StateBase
    {
        public override void OnStart()
        {
            //�G�t�F�N�g�̍Đ�
        }
        public override void OnUpdate()
        {
            //�v���C���[�ɏ���n��
            StateMachine.ChangeState((int)state.End);      //���̃X�e�[�g�Ɉڍs
        }
        public override void OnEnd()
        {

        }
    }
    public class EndState : EStateMachine<minimeteor>.StateBase
    {
        public override void OnStart()
        {

        }
        public override void OnUpdate()
        {

        }
        public override void OnEnd()
        {

        }
    }

    


}
