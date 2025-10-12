using UnityEngine;

public class ColliderFollower : MonoBehaviour
{
    [SerializeField] private Transform model;        // Animator �����������f��
    [SerializeField] private Transform colliderObj;  // �Ǐ]���������R���C�_�[�I�u�W�F�N�g

    private Vector3 initialOffset;
    private Quaternion initialRotationOffset;

    void Start()
    {
        // �����̈ʒu�E��]�̍���ێ�
        initialOffset = colliderObj.position - model.position;
        initialRotationOffset = Quaternion.Inverse(model.rotation) * colliderObj.rotation;
    }

    void LateUpdate()
    {
        // ���f���ɒǏ]
        colliderObj.position = model.position;
        colliderObj.rotation = model.rotation * initialRotationOffset;
    }
}