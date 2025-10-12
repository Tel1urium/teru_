using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static EventBus;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class TempUI : MonoBehaviour
{
    [Header("�E��UI�i�T���v���j")]
    // �� ��: public TextMeshProUGUI weaponDurableText; // �e�L�X�g�\���͔p�~
    public float weaponIconLength = 100f;     // �ׂ̃A�C�R���܂ł̃��[�J��X����
    public GameObject weaponPrefab;           // 1�̕���A�C�R��UI�v���n�u�iImage��z��j
    public GameObject weaponList;             // �E���̋�I�u�W�F�N�g�i�S�A�C�R���̐e�j

    public Sprite weaponDurMaxImg;            // �ϋvMAX���̃X�v���C�g
    public Sprite weaponDurCurImg;            // �ϋv�ʏ펞�̃X�v���C�g

    [Header("�_�b�V��UI")]
    public GameObject dashEnableIcon;       // �_�b�V���\�A�C�R��
    public GameObject dashDisableIcon;

    [Header("HPUI")]
    public GameObject HPUIFront;       // HPUI�O�i
    public float HPUILength = 500f;    // HPUI�̒���

    [Header("���b�N�I��UI")]
    public GameObject AimIcon;
    private Transform lockTarget = null;

    [Header("Menu UI")]
    public GameObject mapObj;
    public GameObject tutorial;
    private bool isMenuOpen = false;

    [Header("�����\���p")]
    public GameObject firsSelectbutton;

    public InputSystem_Actions inputActions;

    private Coroutine slideCo;

    // �E�茻�݃C���f�b�N�X�i�����ʒu�̌v�Z�ɂ̂ݎg�p�j
    private int currentRightIndex = -1;

    // ���߂̏������탊�X�g�Q��
    private List<WeaponInstance> lastWeaponsRef;

    // ������������A�C�R���̎Q�Ɓiindex -> �A�C�R���j
    // �E�f��(-1)�͕ێ����Ȃ��i�ϋv�\�����s�v�̂��߁j
    private readonly Dictionary<int, TempUI_WeaponIcon> rightIconByIndex = new Dictionary<int, TempUI_WeaponIcon>(16);

    private void OnEnable()
    {
        UIEvents.OnRightWeaponSwitch += ChangeRightWeapon;
        UIEvents.OnDurabilityChanged += OnDurabilityChanged;   // �ʕ���̑ϋvUI���f
        //UIEvents.OnWeaponDestroyed += OnWeaponDestroyed;     // �K�v�Ȃ牉�o�g���K

        UIEvents.OnAimPointChanged += SwitchLockIcon;
        UIEvents.OnDashUIChange += SwitchDashIcon;
        UIEvents.OnPlayerHpChange += SetHpBar;

        UIEvents.OnAttackHoldProgress += OnAttacHolding;
        UIEvents.OnAttackHoldUI += OnAttackHoldChange;
        UIEvents.OnAttackHoldDenied += OnAttackHoldDenied;


        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            inputActions.UI.Enable();
            inputActions.UI.Menu.performed += ctx => SwitchMenu();
        }

        if (AimIcon != null)
            AimIcon.SetActive(false);
        ClearRightIcons();
    }

    private void OnDisable()
    {
        UIEvents.OnRightWeaponSwitch -= ChangeRightWeapon;
        UIEvents.OnDurabilityChanged -= OnDurabilityChanged;
        //UIEvents.OnWeaponDestroyed -= OnWeaponDestroyed;
        UIEvents.OnAimPointChanged -= SwitchLockIcon;
        UIEvents.OnDashUIChange -= SwitchDashIcon;

        UIEvents.OnPlayerHpChange -= SetHpBar;
        UIEvents.OnAttackHoldProgress -= OnAttacHolding;
        UIEvents.OnAttackHoldUI -= OnAttackHoldChange;
        UIEvents.OnAttackHoldDenied -= OnAttackHoldDenied;

        if (inputActions != null)
        {
            inputActions.UI.Menu.performed -= ctx => SwitchMenu();
            inputActions.UI.Disable();
            inputActions = null;
        }
        if (slideCo != null) StopCoroutine(slideCo);
        if (isMenuOpen)
        {
            SystemEvents.OnGameResume?.Invoke();
            isMenuOpen = false;
        }
        
    }

    private void Update()
    {
        // --- ���b�N�I���A�C�R���̃X�N���[���Ǐ] ---
        if (lockTarget != null && AimIcon != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(lockTarget.position + Vector3.up * -0f);
            AimIcon.transform.position = screenPos;
        }
    }

    public void SwitchMenu()
    {
        if (!isMenuOpen)
        {
            if (mapObj != null) mapObj.SetActive(true);
            if (tutorial != null) tutorial.SetActive(true);
            isMenuOpen = true;
            if (firsSelectbutton != null) EventSystem.current.SetSelectedGameObject(firsSelectbutton);
            GameObject player = PlayerEvents.GetPlayerObject();
            player.GetComponent<PlayerMovement>().DisablePlayerInput();
            GameManager.Instance?.PauseGame();
            
        }
        else
        {
            if (mapObj != null) mapObj.SetActive(false);
            if (tutorial != null) tutorial.SetActive(false);
            isMenuOpen = false;
            GameManager.Instance?.ResumeGame();
            SystemEvents.OnGameResume?.Invoke();

            GameObject player = PlayerEvents.GetPlayerObject();
            player.GetComponent<PlayerMovement>().EnablePlayerInput();
        }
    }

    // ===== ���b�N�I���A�C�R���\�� =====
    private void SwitchLockIcon(Transform target)
    {
        lockTarget = target;
        if (AimIcon != null)
            AimIcon.SetActive(lockTarget != null);
    }

    private void SwitchDashIcon(bool enable)
    {
        if (dashEnableIcon != null) dashEnableIcon.SetActive(enable);
        if (dashDisableIcon != null) dashDisableIcon.SetActive(!enable);
    }

    private void SetHpBar(int amount, int max)
    {
        if (HPUIFront != null)
        {
            var rt = HPUIFront.transform as RectTransform;
            if (rt != null)
            {
                float len = HPUILength * ((float)amount / (float)max);
                rt.sizeDelta = new Vector2(len, rt.sizeDelta.y);
            }
        }
    }
    private void OnAttacHolding(float amount)
    {
        var weapon = rightIconByIndex.TryGetValue(currentRightIndex, out var icon) ? icon : null;
        if(weapon == null || weapon.skillImgObj == null) return;
        weapon.skillImgObj.enabled = true;
        weapon.skillImgObj.fillAmount = amount;
    }
        
    private void OnAttackHoldChange(bool isHolding)
    {
        var weapon = rightIconByIndex.TryGetValue(currentRightIndex, out var icon) ? icon : null;
        if (weapon == null || weapon.skillImgObj == null) return;
        weapon.skillImgObj.enabled = isHolding;
        if (!isHolding)
            weapon.skillImgObj.fillAmount = 0f;
    }
    private void OnAttackHoldDenied()
    {
        var weapon = rightIconByIndex.TryGetValue(currentRightIndex, out var icon) ? icon : null;
        if (weapon == null || weapon.skillImgObj == null) return;
        Color ori = weapon.skillImgObj.color;
        weapon.skillImgObj.color = Color.red;
        StartCoroutine(ResetColor(weapon.skillImgObj, ori, 0.35f));
    }

    private IEnumerator ResetColor(Image img, Color to, float duration)
    {
        if (img == null) yield break;
        Color from = img.color;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            img.color = Color.Lerp(from, to, k);
            yield return null;
        }
        img.color = to;
        img.enabled = false;
    }
    // ===== �����\���i�f��̂݁j=====
    private void TryRenderFistOnly()
    {
        if (weaponList == null || weaponPrefab == null) return;

        var listRT = weaponList.transform as RectTransform;
        if (listRT == null) return;

        // �����A�C�R���N���A
        for (int i = listRT.childCount - 1; i >= 0; --i)
            Destroy(listRT.GetChild(i).gameObject);
        rightIconByIndex.Clear();

        // �f��A�C�R���iindex = -1�j
        var go = Instantiate(weaponPrefab, weaponList.transform);
        go.name = "Icon_-1_Fist";
        var rt = go.transform as RectTransform;
        if (rt != null)
        {
            // ���S�� -1�i�f��j�ɍ��킹��BX=0 �������B
            rt.anchoredPosition = new Vector2(0f, 0f);
        }

        // �f��͑ϋv�\���Ȃ��FTempUI_WeaponIcon ���t���Ă���Ȃ�Q�[�W���\��
        var fistIcon = go.GetComponent<TempUI_WeaponIcon>();
        if (fistIcon != null && fistIcon.durImgObj != null)
        {
            fistIcon.durImgObj.fillAmount = 0f;
            fistIcon.durImgObj.enabled = false;
        }
        if (fistIcon != null && fistIcon.skillImgObj != null)
        {
            fistIcon.skillImgObj.fillAmount = 0f;
        }

        // ���݉E��C���f�b�N�X = �f��
        currentRightIndex = -1;
    }

    // ===== �ؑփn���h���i�E��j=====
    private void ChangeRightWeapon(List<WeaponInstance> weapons, int from, int to)
    {
        // --- �Q�ƃK�[�h ---
        if (weaponList == null) { Debug.LogWarning("TempUI: No UI weaponList"); return; }
        if (weaponPrefab == null) { Debug.LogWarning("TempUI: No UI weaponPrefab"); return; }

        lastWeaponsRef = weapons; // �Q�ƕێ�

        RectTransform listRT = weaponList.transform as RectTransform;
        if (listRT == null) { Debug.LogWarning("TempUI: weaponList is not RectTransform"); return; }

        // �����A�C�R���S�폜
        for (int i = listRT.childCount - 1; i >= 0; --i)
            Destroy(listRT.GetChild(i).gameObject);
        rightIconByIndex.Clear();

        // ��̏ꍇ�͉����o�����I��
        if (weapons == null || weapons.Count == 0)
        {
            currentRightIndex = -1;
            return;
        }

        // �X���C�h�p���W
        var childRTs = new List<RectTransform>();
        var fromXs = new List<float>();
        var toXs = new List<float>();

        // --- ����A�C�R���Q�iindex: 0..Count-1�j---
        for (int i = 0; i < weapons.Count; i++)
        {
            GameObject go = Instantiate(weaponPrefab, weaponList.transform);
            go.name = $"Icon_{i}";
            RectTransform rt = go.transform as RectTransform;
            if (rt != null)
            {
                // �� ������ center = from / to �����̂܂܎g�p����i�f��I�t�Z�b�g�����j
                float sx = IndexToLinear(i, from) * weaponIconLength;
                float ex = IndexToLinear(i, to) * weaponIconLength;
                rt.anchoredPosition = new Vector2(sx, 0f);

                childRTs.Add(rt);
                fromXs.Add(sx);
                toXs.Add(ex);
            }

            // ���C���̃A�C�R���摜
            var img = go.GetComponent<Image>();
            var spr = weapons[i]?.template?.icon;
            if (img != null) img.sprite = spr;

            // �ϋv/�X�L��UI�̏�����
            var weaponIcon = go.GetComponent<TempUI_WeaponIcon>();
            if (weaponIcon != null)
            {
                // --- �ϋv�o�[������ ---
                if (weaponIcon.durImgObj != null)
                {
                    int cur = Mathf.Max(0, weapons[i].currentDurability);
                    int max = Mathf.Max(1, weapons[i].template.maxDurability);
                    float ratio = (float)cur / (float)max;

                    weaponIcon.durImgObj.fillAmount = ratio;
                    weaponIcon.durImgObj.enabled = true;
                    weaponIcon.durImgObj.sprite = (cur >= max) ? weaponDurMaxImg : weaponDurCurImg;
                }

                // --- �X�L���Q�[�W�������i0%�j ---
                if (weaponIcon.skillImgObj != null)
                    weaponIcon.skillImgObj.fillAmount = 0f;

                // �C���f�b�N�X���A�C�R���Q��
                rightIconByIndex[i] = weaponIcon;
            }
        }

        // --- �q�I�u�W�F�N�g���X���C�h�i�e�͌Œ�j---
        if (slideCo != null) StopCoroutine(slideCo);
        slideCo = StartCoroutine(SlideChildrenX(childRTs, fromXs, toXs, 0.2f, () =>
        {
            currentRightIndex = to;
            RefreshAllRightDurability(); // �O�̂��ߓ���
        }));
    }

    // ===== �ʑϋv�ω��i�E��j=====
    private void OnDurabilityChanged(HandType hand, int index, int current, int max)
    {
        // �E�E��̂ݔ��f
        if (hand != HandType.Main) return;

        // �E�����ς݂̊Y���C���f�b�N�X�̃A�C�R��������΍X�V
        if (rightIconByIndex.TryGetValue(index, out var icon) && icon != null && icon.durImgObj != null)
        {
            // --- fillAmount ��ϋv�����ɍX�V ---
            int safeMax = Mathf.Max(1, max);
            int safeCur = Mathf.Clamp(current, 0, safeMax);
            float ratio = (float)safeCur / (float)safeMax;

            icon.durImgObj.fillAmount = ratio;
            icon.durImgObj.enabled = true;
            icon.durImgObj.sprite = (safeCur >= safeMax) ? weaponDurMaxImg : weaponDurCurImg;
        }
        // �E������Ȃ��ꍇ�͑S�̍X�V�Ńt�H�[���o�b�N�i�Ⴆ�΃X���b�g�\���ω�����j
        else
        {
            RefreshAllRightDurability();
        }
    }

    // ===== �j��ʒm�i�K�v�Ȃ牉�o�p�j=====
    private void OnWeaponDestroyed(int removedIndex, WeaponItem item)
    {
        // �E�����ł͉��o�g���K�������Ă��悢�i�_��/SE���j
        Debug.Log($"TempUI: Weapon destroyed @index={removedIndex} ({item?.weaponName})");
    }

    // ===== �C���f�b�N�X�������ʒu�ւ̕ϊ� =====
    private int IndexToLinear(int idx, int centerIndex)
    {
        // ��FcenterIndex=2 �̏ꍇ�Aidx=2 �� 0�i�����j�Aidx=1 �� -1�Aidx=3 �� +1
        return idx - centerIndex;
    }

    // ===== �R���[�`���F�X���C�h =====
    private IEnumerator SlideChildrenX(
        List<RectTransform> items,
        List<float> fromXs,
        List<float> toXs,
        float duration,
        System.Action onDone = null)
    {
        int count = Mathf.Min(items.Count, Mathf.Min(fromXs.Count, toXs.Count));
        if (count <= 0) { onDone?.Invoke(); yield break; }

        float t = 0f;
        while (t < duration)
        {
            // --- Time.timeScale=0 �ł����삳���邽�� unscaledDeltaTime ���g�p ---
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);

            for (int i = 0; i < count; i++)
            {
                var rt = items[i];
                if (rt == null) continue;

                float x = Mathf.Lerp(fromXs[i], toXs[i], k);
                var ap = rt.anchoredPosition;
                rt.anchoredPosition = new Vector2(x, ap.y);
            }
            yield return null;
        }

        // --- �I�[�ňʒu�덷���z�� ---
        for (int i = 0; i < count; i++)
        {
            var rt = items[i];
            if (rt == null) continue;
            var ap = rt.anchoredPosition;
            rt.anchoredPosition = new Vector2(toXs[i], ap.y);
        }

        onDone?.Invoke();
    }

    // ===== �S�A�C�R���̑ϋv�������f�[�^����ē��� =====
    private void RefreshAllRightDurability()
    {
        // �E�������X�g��������Ή������Ȃ�
        if (lastWeaponsRef == null) return;

        for (int i = 0; i < lastWeaponsRef.Count; i++)
        {
            if (!rightIconByIndex.TryGetValue(i, out var icon) || icon == null || icon.durImgObj == null)
                continue;

            int cur = Mathf.Max(0, lastWeaponsRef[i].currentDurability);
            int max = Mathf.Max(1, lastWeaponsRef[i].template.maxDurability);
            float ratio = (float)cur / (float)max;

            icon.durImgObj.fillAmount = ratio;
            icon.durImgObj.enabled = true;
            icon.durImgObj.sprite = (cur >= max) ? weaponDurMaxImg : weaponDurCurImg;
        }
    }
    private void ClearRightIcons()
    {
        if (weaponList == null) return;

        // �q��S�j��
        var listRT = weaponList.transform as RectTransform;
        if (listRT == null) return;

        for (int i = listRT.childCount - 1; i >= 0; --i)
            Destroy(listRT.GetChild(i).gameObject);

        rightIconByIndex.Clear();

        // ���݃C���f�b�N�X
        currentRightIndex = -1;
    }
}
