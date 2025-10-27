using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class MapGimmick : MonoBehaviour
{
    //���e�I�I�u�W�F�N�g
    public GameObject minimeteor;
    //�Q�[��Ingame�̃X�e�[�g�Ǘ�
    GameManager gameManager;
    //Ingame�̌o�ߎ���
    private float gameTime = 0;
    //Meteor���~�点��^�C�~���O
    private float MeteorTime = 30;
    //�{�[�����~�点��^�C�~���O
    private float BallTime = 10;

    
    public GameObject[] ballPrefabs;
    //�{�[���𗎂Ƃ���
    public int SpawnBall = 3;
    //�{�[�����~�点�鍂��
    public float PosY = 10.0f;
    //�I�u�W�F�N�g�𐶐�����X�͈̔�
    public float AreaX = 1.0f;
    //�{�[���̃t���O
    public bool ballTrigger = true;
    //���e�I�t���O
    public bool meteorTrigger = true;
    //�C���X�y�N�^�[����e�[�u���̃R���C�_�[��ݒ�
    public Collider tableCollider;
    //�C���X�y�N�^�[���烁�e�I�̏o���ʒu��ݒ�
    public Transform[] meteorSpawnPoint;
    //ray�̐i�s����
    Vector3 direction;
    //�q�b�g�����ꏊ
    RaycastHit hit;
    //ray�̊J�n�n�_
    Vector3 startPoint;
    //�\��
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
            //�Q�[���̌o�ߎ��Ԃ��v��
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

    //���e�I
    async public void Meteorgimmick()
    {
        //0����z��̗v�f��-1�܂ł̃����_���Ȑ������擾
        int randomIndex = UnityEngine.Random.Range(0, meteorSpawnPoint.Length);
        //�����_���ɑI�΂ꂽ�o���ʒu��tranceform���擾
        Transform spawnpoint = meteorSpawnPoint[randomIndex];
        //ray�̐i�s�����̎擾
        direction = -spawnpoint.transform.up;
        //ray�̐����ꏊ�̎擾
        startPoint = spawnpoint.position;
        //ray�Ăяo��
        Rayhit();
        //�Q�[�������ԃ��Z�b�g
        gameTime = 0;
        //�{�[���g���K�[����
        ballTrigger = true;
        //���b��ɑI�΂ꂽ�ꏊ�̈ʒu���e�I�𐶐�
        await UniTask.Delay(TimeSpan.FromSeconds(3));
        Instantiate(minimeteor, spawnpoint.position, spawnpoint.rotation);
        
    }

    //�{�[��
    public void BallFallgimmick()
    {
        Debug.Log("�Q�[���J�n����10�b�o�߁A�{�[�����~�点�܂�");
        //�e�[�u���̋��E�����擾
        Bounds tableBounds = tableCollider.bounds;

        foreach (var ball in ballPrefabs)
        {
            for (int i = 0; i < SpawnBall; i++)//SpawnBall�̓{�[�����o����
            {
                //�����_����X�̈ʒu
                float randomX = UnityEngine.Random.Range(tableBounds.min.x, tableBounds.max.x);
                //�����_����Z�̈ʒu
                float randomZ = UnityEngine.Random.Range(tableBounds.min.z, tableBounds.max.z);
                //��������ʒu�̌���
                Vector3 spawnposition = new Vector3(randomX, tableBounds.max.y + PosY, randomZ);
                //�{�[���̐���
                Instantiate(ball, spawnposition, Quaternion.identity);
                //���e�I�g���K�[����
                meteorTrigger = true;
            }
        }
    }

    private void Rayhit()
    {
        Ray ray = new Ray(startPoint, direction);
        if (Physics.Raycast(ray.origin, ray.direction, out hit))
        {
            Debug.Log("ray�Փ�" + hit.collider.name);

            if (hit.collider.CompareTag("map"))
            {
                Debug.Log("ray��map�ɏՓ�" + hit.point);
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
