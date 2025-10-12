using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    private Renderer renderer;
    private Material instanceMaterial;

    void Start()
    {
        // Renderer�R���|�[�l���g���擾
        renderer = GetComponent<Renderer>();

        // ���̃}�e���A���̃C���X�^���X���쐬
        instanceMaterial = new Material(renderer.sharedMaterial);

        // �C���X�^���X�������}�e���A����Renderer�ɓK�p
        renderer.material = instanceMaterial;

        // �F��ύX�i�Ⴆ�ΐԂɕύX�j
        instanceMaterial.SetColor("_Color", Color.red);
    }
}
