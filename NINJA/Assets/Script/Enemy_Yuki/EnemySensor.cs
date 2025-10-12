using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnemySensor : MonoBehaviour
{
    [SerializeField]
    private SphereCollider search = default;
    [SerializeField]
    private GameObject ctrl;
    private EnemyController ec;
    private IceShot iceShot;
    public int maxTime;
    InField inField;
    private void Start()
    {
        ec = transform.parent.GetComponent<EnemyController>();
        inField = new InField();
        maxTime = 80;
    }
    private void OnTriggerStay(Collider other)
    {
        if(inField.GetInField()) {
            var playerDir = other.transform.position - transform.position;
            var angle = Vector3.Angle(transform.forward, playerDir);//ÉvÉåÉCÉÑÅ[Ç∆é©êgÇ∆ÇÃäpìx
            var dis = Vector3.Distance(other.transform.position, transform.position);//ãóó£
            if (other.gameObject.tag == "Player")
            {
                ctrl.transform.position = Vector3.Lerp(ctrl.transform.position, other.gameObject.transform.position, 0.1f);
                ec.SetTarget(other.transform);
                ec.SetLookPlayer(other.transform.position);
                if (EnemyController.now / maxTime == 1)
                {
                    maxTime = Random.Range(1, 4) * 80;

                    if (dis <= search.radius * 0.15f)//îºåaÇÃÇO.1Å`ÇOÅD15
                    {
                        if (Probability(5))
                        {
                            ec.SetState(EnemyController.EnemyState.Chase);
                        }
                        else if (Probability(15))
                        {
                            ec.SetState(EnemyController.EnemyState.Atk3);
                        }
                        else if (Probability(30))
                        {
                            ec.SetState(EnemyController.EnemyState.Atk2);
                        }
                        else if (Probability(40))
                        {
                            ec.SetState(EnemyController.EnemyState.Atk1);
                        }
                        else if (Probability(75))
                        {
                            ec.SetState(EnemyController.EnemyState.Atk4);
                        }
                        else
                        {
                            ec.SetState(EnemyController.EnemyState.Leave);
                        }
                    }
                    else if (dis <= search.radius * 0.7f && dis > search.radius * 0.15f)//ÇªÇÍà»ç~
                    {
                        if (Probability(10))
                        {
                              ec.SetState(EnemyController.EnemyState.Chase);
                        }
                        else if (Probability(30))
                        {
                            ec.SetState(EnemyController.EnemyState.Atk3);
                        }
                        else if (Probability(60))
                        {
                            ec.SetState(EnemyController.EnemyState.Atk2);
                        }
                        else if (Probability(80))
                        {
                            ec.SetState(EnemyController.EnemyState.Atk1);
                        }
                        else
                        {
                            ec.SetState(EnemyController.EnemyState.Leave);
                        }
                    }
                    else if (dis <= search.radius * 1f && dis > search.radius * 0.7f)//ÇªÇÍà»ç~
                    {
                        ec.SetState(EnemyController.EnemyState.Chase);
                    }
                }

            }

        }
    }
    private void OnTriggerExit(Collider other)//îÕàÕäO
    {
        ctrl.transform.position = transform.position + transform.forward * 0.8f;
        ec.SetState(EnemyController.EnemyState.Idle);
    }
    public static bool Probability(float fPersent)//ämóßîªíËópÉÅÉ\ÉbÉh
    {
        float fProbabilityRate = UnityEngine.Random.value * 100;
        if (fPersent == 100f && fProbabilityRate == fPersent)
        {
            return true;
        }
        else if (fPersent > fProbabilityRate)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
