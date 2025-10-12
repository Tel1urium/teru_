using System;
using System.Collections.Generic;
using UnityEngine;
using static EventBus;
public enum HandType
{
    Main, // �E��
    Sub   // ����
}
[System.Serializable]
public class WeaponInstance
{
    public WeaponItem template;
    public int currentDurability;

    public WeaponInstance(WeaponItem weapon)
    {
        template = weapon;
        currentDurability = 5;
    }
    public WeaponInstance(WeaponItem weapon, int durability)
    {
        template = weapon;
        currentDurability = Mathf.Clamp(durability, 0, weapon != null ? weapon.maxDurability : durability);
    }
    public WeaponInstance Clone()
    {
        return new WeaponInstance(template, currentDurability);
    }

    public bool Use(int cost)
    {
        if (currentDurability < cost) return false;
        currentDurability -= Mathf.CeilToInt(cost);
        currentDurability = Mathf.Max(0, currentDurability);
        currentDurability = Mathf.Min(currentDurability, template.maxDurability);

        return true;
    }

    public bool IsBroken => currentDurability < 0;
}


[Serializable]
public class PlayerWeaponInventory
{
    [Tooltip("��������i�C���f�b�N�X��UI��̔ԍ��ƈ�v�j")]
    public List<WeaponInstance> weapons = new List<WeaponInstance>();

    [Tooltip("�E��iMain�j�̌��ݑ����C���f�b�N�X�B-1 �͖�����")]
    public int mainIndex = -1;

    [Tooltip("����iSub�j�̌��ݑ����C���f�b�N�X�B-1 �͖�����")]
    public int subIndex = -1;

    private Dictionary<WeaponType,int> typeToIndex = new Dictionary<WeaponType, int>();

    public void ApplyLoadoutInstances(List<WeaponInstance> instances, int mainIndexToEquip)
    {
        weapons.Clear();
        typeToIndex.Clear();
        mainIndex = -1;
        subIndex = -1;

        if (instances != null)
        {
            for (int i = 0; i < instances.Count; ++i)
            {
                var src = instances[i];
                if (src == null || src.template == null) continue;

                var clone = src.Clone();
                clone.currentDurability = Mathf.Clamp(clone.currentDurability, 0, clone.template.maxDurability);

                weapons.Add(clone);
                typeToIndex[clone.template.weaponType] = weapons.Count - 1;
            }
        }

        int prev = mainIndex;
        int target = (mainIndexToEquip >= 0 && mainIndexToEquip < weapons.Count) ? mainIndexToEquip : -1;

        if (prev != target)
        {
            UIEvents.OnRightWeaponSwitch?.Invoke(weapons, prev, target);
        }
        mainIndex = target;
    }

    // ==== �������[�e�B���e�B ====
    private bool IsUsableIndex(int idx)
    {
        return idx >= 0 && idx < weapons.Count && weapons[idx] != null && !weapons[idx].IsBroken;
    }

    // dir: +1 = �O�����ɏ���, -1 = ������ɏ���
    private int FindNextUsable(int startIdx, int exclude, int dir = 1)
    {
        // �� ���S�K�[�h
        int n = weapons.Count;
        if (n == 0) return -1;
        if (dir == 0) dir = +1;

        // startIdx �� [-1, n-1] �ɐ��K���i-1 �́u���̈ʒu�̒��O�v�݂����Ɉ����j
        int start = Mathf.Clamp(startIdx, -1, n - 1);

        // n ��܂Ō��ɂ���
        for (int step = 1; step <= n; ++step)
        {
            // ����C���f�b�N�X�v�Z�i�����␳����j
            int i = (start + step * dir) % n;
            if (i < 0) i += n;

            if (exclude >= 0 && i == exclude) continue;
            if (IsUsableIndex(i)) return i;
        }
        return -1;
    }

    private int GetHandIndex(HandType hand) => (hand == HandType.Main) ? mainIndex : subIndex;

    // ��̃C���f�b�N�X��ݒ肵�AUI �C�x���g�ɓ]��
    private void SetHandIndex(HandType hand, int to)
    {
        int from = GetHandIndex(hand);
        if (hand == HandType.Main) mainIndex = to; else subIndex = to;

        if (from != to)
        {
            // --- ������UIEvents�֒ʒm ---
            if (hand == HandType.Main)
            {
                UIEvents.OnRightWeaponSwitch?.Invoke(weapons, from, to);
            }
            else
            {
                UIEvents.OnLeftWeaponSwitch?.Invoke(weapons, from, to);
            }
        }
    }

    // ==== �����ؑ� ====
    public bool TrySwitchRight(int index = 1)
    {
        int next = -1;
        if (weapons.Count > 0) next = FindNextUsable(mainIndex, exclude: -1, index);
        SetHandIndex(HandType.Main, next);

        if (next == -1) return false;
        return true;
    }

    public void Unequip(HandType hand)
    {
        SetHandIndex(hand, -1);
    }

    // ==== �����Ǘ� ====
    public void AddWeapon(WeaponItem weapon)
    {
        if (weapon == null) return;
        if(typeToIndex.ContainsKey(weapon.weaponType))
        {
            int idx = typeToIndex[weapon.weaponType];
            weapons[idx].currentDurability += weapons[idx].template.addDurabilityOnPickup;
            weapons[idx].currentDurability = Mathf.Min(weapons[idx].currentDurability, weapons[idx].template.maxDurability);
            UIEvents.OnDurabilityChanged?.Invoke(HandType.Main, idx, weapons[idx].currentDurability, weapons[idx].template.maxDurability);
            return;
        }
        else
        {
            weapons.Add(new WeaponInstance(weapon));
            typeToIndex[weapon.weaponType] = weapons.Count - 1;
            UIEvents.OnRightWeaponSwitch?.Invoke(weapons, mainIndex, mainIndex); // UI�X�V
        }
        
    }

    // ==== �ϋv����i�j�󎞂͎��� Remove & Recover�j====
    // �d�l�ύX�F����󂹂��ɑϋv0�ŕ��u�ł���悤
    public bool ConsumeDurability(HandType hand, int cost)
    {
        int idx = GetHandIndex(hand);
        bool res = true;
        if (!IsUsableIndex(idx)) res = false;

        WeaponInstance inst = weapons[idx];
        if(res) res = inst.Use(cost);

        // --- �ϋv�x�X�V�C�x���g ---
        if (res)
        {
            UIEvents.OnDurabilityChanged?.Invoke(
            hand, idx, inst.currentDurability, inst.template.maxDurability
        );
        } 
        else { UIEvents.OnWeaponUseFailed?.Invoke(); }

        return res;
    }

    public WeaponInstance GetWeapon(HandType hand)
    {
        int idx = GetHandIndex(hand);
        //return IsUsableIndex(idx) ? weapons[idx] : null;
        if(idx >= 0 && idx < weapons.Count)
        {
            return weapons[idx];
        }
        else
        {
            return null;
        }
    }
}




