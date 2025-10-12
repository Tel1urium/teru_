using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// GameState �� BGM �̑Ή��֌W
/// </summary>
[System.Serializable]
public struct StateToBGM
{
    public GameState state;
    public string bgmName;
}

/// <summary>
/// �������i���O�AAudioClip�A���ʁj
/// </summary>
[System.Serializable]
public struct soundAudio
{
    public string name;
    public AudioClip audioClip;
    public float volume;
}

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;

    [Header("BGM��`���X�g")]
    public List<soundAudio> soundList = new List<soundAudio>();

    [Header("BGM�Đ��pAudioSource")]
    public AudioSource audioSource;

    [Header("GameState���Ƃ�BGM�}�b�s���O")]
    public List<StateToBGM> bgmMappings = new List<StateToBGM>();

    private Dictionary<GameState, string> stateToBgmMap = new Dictionary<GameState, string>();

    private string currentPlayingName = null;
    private GameState nextState = GameState.Startup; // ����GameState
    public float fadeDuration = 1f; // �t�F�[�h����

    /// <summary>
    /// �V���O���g���̏�����
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // GameState��BGM�̃}�b�s���O�������ɓo�^
        foreach (var mapping in bgmMappings)
        {
            if (!stateToBgmMap.ContainsKey(mapping.state))
                stateToBgmMap.Add(mapping.state, mapping.bgmName);
        }
    }

    /// <summary>
    /// AudioSource�̏�����
    /// </summary>
    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    /// <summary>
    /// �C�x���g�w��
    /// </summary>
    private void OnEnable()
    {
        EventBus.SystemEvents.OnGameStateChange += PrepareToEnterState;
    }

    /// <summary>
    /// �C�x���g����
    /// </summary>
    private void OnDisable()
    {
        EventBus.SystemEvents.OnGameStateChange -= PrepareToEnterState;
    }

    /// <summary>
    /// ���O����T�E���h�����擾
    /// </summary>
    public soundAudio GetClipByName(string name)
    {
        foreach (var s in soundList)
        {
            if (s.name == name)
            {
                return s;
            }
        }

        soundAudio nu = new soundAudio();
        return nu;
    }

    /// <summary>
    /// GameState�ɑΉ�����BGM�����擾
    /// </summary>
    private string GetBGMNameForState(GameState state)
    {
        return stateToBgmMap.TryGetValue(state, out string bgmName) ? bgmName : null;
    }

    /// <summary>
    /// �V�[���֑ؑO�ɌĂ΂��֐��B���̏�Ԃ�����BGM��ύX����K�v�����邩�m�F�B
    /// </summary>
    private void PrepareToEnterState(GameState nextState)
    {
        this.nextState = nextState;
        string nextBgmName = GetBGMNameForState(nextState);

        if (nextBgmName == null || nextBgmName == currentPlayingName)
            return;

        StartCoroutine(FadeOutThenPlay(nextBgmName));
    }

    /// <summary>
    /// �t�F�[�h�A�E�g���Ă���V����BGM���Đ�
    /// </summary>
    private IEnumerator FadeOutThenPlay(string nextBgmName)
    {
        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
        currentPlayingName = null;

        PlayBGMForState(); // �t�F�[�h��ɐV����BGM���Đ�
    }

    /// <summary>
    /// ����GameState�ɑΉ�����BGM���Đ�
    /// </summary>
    public void PlayBGMForState()
    {
        string bgmName = GetBGMNameForState(nextState);

        if (bgmName == null)
            return;

        if (currentPlayingName == bgmName)
            return;

        soundAudio s = GetClipByName(bgmName);
        if (s.audioClip == null)
            return;

        audioSource.clip = s.audioClip;
        audioSource.volume = 0f;
        audioSource.loop = true;
        audioSource.Play();
        currentPlayingName = bgmName;
        StartCoroutine(FadeIn(s.volume));
    }

    /// <summary>
    /// �t�F�[�h�C������
    /// </summary>
    private IEnumerator FadeIn(float targetVolume)
    {
        float time = 0f;
        float duration = fadeDuration / 2;
        while (time < duration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, time / duration);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }
}
