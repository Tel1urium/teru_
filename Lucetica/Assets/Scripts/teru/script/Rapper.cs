using UnityEngine;

public class Rapper: MonoBehaviour
{
    public Enemy parentController; // Cube �ɕt�����X�N���v�g
    public void OnAttackSet()
    {
        parentController.OnAttackSet();
    }

    public void OnAttackEnd()
    {
        parentController.OnAttackEnd();
    }
    
    public void OnSumon()
    {
        parentController.OnSumon();
    }
}
