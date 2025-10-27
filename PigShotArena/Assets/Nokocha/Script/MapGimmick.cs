using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class MapGimmick : MonoBehaviour
{
    //メテオオブジェクト
    public GameObject minimeteor;
    //ゲームIngameのステート管理
    GameManager gameManager;
    //Ingameの経過時間
    private float gameTime = 0;
    //Meteorを降らせるタイミング
    private float MeteorTime = 30;
    //ボールを降らせるタイミング
    private float BallTime = 10;

    
    public GameObject[] ballPrefabs;
    //ボールを落とす数
    public int SpawnBall = 3;
    //ボールを降らせる高さ
    public float PosY = 10.0f;
    //オブジェクトを生成するXの範囲
    public float AreaX = 1.0f;
    //ボールのフラグ
    public bool ballTrigger = true;
    //メテオフラグ
    public bool meteorTrigger = true;
    //インスペクターからテーブルのコライダーを設定
    public Collider tableCollider;
    //インスペクターからメテオの出現位置を設定
    public Transform[] meteorSpawnPoint;
    //rayの進行方向
    Vector3 direction;
    //ヒットした場所
    RaycastHit hit;
    //rayの開始地点
    Vector3 startPoint;
    //予兆
    public GameObject Danger;
    private GameObject dangerPoint;

    void Awake()
    {
        gameManager = GameObject.Find("SceneManagerObj").gameObject.GetComponent<GameManager>();
        
    }

    // Update is called once per frame
    void Update()
    {
        gimmick();
    }

    public void gimmick()
    {
        if (gameManager != null&& gameManager.state ==GameManager.State.Ingame)
        {
            //ゲームの経過時間を計測
            gameTime+=Time.deltaTime;

            if(gameTime > BallTime)
            {
                if(ballTrigger)
                {
                    BallFallgimmick();
                    ballTrigger = false;
                }
            }
            if(gameTime > MeteorTime)
            {
                Meteorgimmick();
                meteorTrigger = false;
            }

        }
    }

    //メテオ
    async public void Meteorgimmick()
    {
        //0から配列の要素数-1までのランダムな整数を取得
        int randomIndex = UnityEngine.Random.Range(0, meteorSpawnPoint.Length);
        //ランダムに選ばれた出現位置のtranceformを取得
        Transform spawnpoint = meteorSpawnPoint[randomIndex];
        //rayの進行方向の取得
        direction = -spawnpoint.transform.up;
        //rayの生成場所の取得
        startPoint = spawnpoint.position;
        //ray呼び出し
        Rayhit();
        //ゲーム内時間リセット
        gameTime = 0;
        //ボールトリガー復活
        ballTrigger = true;
        //数秒後に選ばれた場所の位置メテオを生成
        await UniTask.Delay(TimeSpan.FromSeconds(3));
        Instantiate(minimeteor, spawnpoint.position, spawnpoint.rotation);
        
    }

    //ボール
    public void BallFallgimmick()
    {
        Debug.Log("ゲーム開始から10秒経過、ボールを降らせます");
        //テーブルの境界情報を取得
        Bounds tableBounds = tableCollider.bounds;

        foreach (var ball in ballPrefabs)
        {
            for (int i = 0; i < SpawnBall; i++)//SpawnBallはボールを出す数
            {
                //ランダムなXの位置
                float randomX = UnityEngine.Random.Range(tableBounds.min.x, tableBounds.max.x);
                //ランダムなZの位置
                float randomZ = UnityEngine.Random.Range(tableBounds.min.z, tableBounds.max.z);
                //生成する位置の決定
                Vector3 spawnposition = new Vector3(randomX, tableBounds.max.y + PosY, randomZ);
                //ボールの生成
                Instantiate(ball, spawnposition, Quaternion.identity);
                //メテオトリガー復活
                meteorTrigger = true;
            }
        }
    }

    private void Rayhit()
    {
        Ray ray = new Ray(startPoint, direction);
        if (Physics.Raycast(ray.origin, ray.direction, out hit))
        {
            Debug.Log("ray衝突" + hit.collider.name);

            if (hit.collider.CompareTag("map"))
            {
                Debug.Log("rayがmapに衝突" + hit.point);
                Vector3 hitPoint = hit.point + new Vector3(0,0.1f,0);
                dangerPoint=Instantiate(Danger,hitPoint, Quaternion.identity);                
            }
        }
    }
    public void DestroyDanger()
    {
        Destroy(dangerPoint);
    }
}
