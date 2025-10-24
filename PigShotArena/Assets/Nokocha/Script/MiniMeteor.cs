using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class minimeteor : MonoBehaviour
{

    MapGimmick mapGimmick;

    bool HitCheck = false;
    public GameObject BoomEfe;      //爆発エフェクト
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
            //エフェクトの再生
        }
        public override void OnUpdate()
        {
            //落下処理、ぶつかった処理判定
            Owner.transform.Translate(Vector3.down * 5f * Time.deltaTime);

            if(Owner.HitCheck)
            {
                StateMachine.ChangeState((int)state.Boom);      //次のステートに移行
            }
        }
        public override void OnEnd()
        {
            //エフェクトの終了
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag=="map")
        {
            Debug.Log("meteorがmapに落下");
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
            //エフェクトの再生
            Instantiate(Owner.BoomEfe, Owner.transform.position, Quaternion.identity);
        }
        public override void OnUpdate()
        {
            StateMachine.ChangeState((int)state.End);      //次のステートに移行
        }
        public override void OnEnd()
        {

            //エフェクトの再生
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
