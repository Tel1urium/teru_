using UnityEngine;

public class joint : MonoBehaviour
{
    public GameObject targetObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("targetObject���ݒ肳��Ă��܂���I");
            return;
        }

        Rigidbody thisRb = GetComponent<Rigidbody>();
        Rigidbody targetRb = targetObject.GetComponent<Rigidbody>();

        // FixedJoint��ǉ����Đڑ�
        FixedJoint joint = gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = targetRb;

        // �K�v�ɉ����ăW���C���g�̃p�����[�^�𒲐��\
        joint.breakForce = Mathf.Infinity;  // �W���C���g�̔j�f�́i�����ɐݒ�j
        joint.breakTorque = Mathf.Infinity; // �j�f�g���N�i�����ɐݒ�j

        Debug.Log("�I�u�W�F�N�g���m��FixedJoint�Őڑ����܂����I");
    }
}

    