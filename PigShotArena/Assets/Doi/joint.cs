using UnityEngine;

public class joint : MonoBehaviour
{
    public GameObject targetObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("targetObjectが設定されていません！");
            return;
        }

        Rigidbody thisRb = GetComponent<Rigidbody>();
        Rigidbody targetRb = targetObject.GetComponent<Rigidbody>();

        // FixedJointを追加して接続
        FixedJoint joint = gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = targetRb;

        // 必要に応じてジョイントのパラメータを調整可能
        joint.breakForce = Mathf.Infinity;  // ジョイントの破断力（無限に設定）
        joint.breakTorque = Mathf.Infinity; // 破断トルク（無限に設定）

        Debug.Log("オブジェクト同士をFixedJointで接続しました！");
    }
}

    