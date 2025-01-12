using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BluehatGames
{
    public class MainManager : MonoBehaviour
    {
        public Button btn_synthesis;
        public Button btn_multiplay;
        public Button btn_nftMarket;

        [Header("Setting Button")] public Button btn_setting;

        public GameObject settingPanel;
        public Button btn_setting_close;
        public Button btn_logout;
        public Toggle toggle_music;
        public Toggle toggle_sound_effect;
        public Button bnt_change_name;
        public InputField input_name;

        [Header("Alert Panel")] public Text text_fistAnimal;

        public Button AlertDoneBtn;
        public GameObject AlertPanel;

        [Header("Music")] public AudioSource audioSource;

        public AudioClip multiplayButtonSound;
        public AudioClip upperButtonSound;
        public AudioClip mainButtonSound;

        [Header("Wallet Info alert")] public Image img_btn_wallet_alert;

        [Header("User Egg")] public Text eggText;

        public GameObject Mission;
        private DataManager dataManager;
        private SoundUtil soundUtil;

        private void Start()
        {
            StartCoroutine(GetUserInfo());
            dataManager = FindObjectOfType<DataManager>();
            AlertPanel.SetActive(false);
            soundUtil = new SoundUtil();

            if (soundUtil.isbackgroundMusicOn())
            {
                audioSource.Play();
                toggle_music.isOn = true;
            }
            else
            {
                audioSource.Stop();
                toggle_music.isOn = false;
            }

            btn_synthesis.onClick.AddListener(() =>
            {
                SoundManager.instance.PlayEffectSound(mainButtonSound);
                SceneManager.LoadScene(SceneName._04_Synthesis);
            });

            btn_multiplay.onClick.AddListener(() =>
            {
                SoundManager.instance.PlayEffectSound(multiplayButtonSound);
                SceneManager.LoadScene(SceneName._05_Multiplay);
            });

            btn_nftMarket.onClick.AddListener(() =>
            {
                SoundManager.instance.PlayEffectSound(mainButtonSound);
                SceneManager.LoadScene(SceneName._06_Market);
            });


            settingPanel.SetActive(false);
            btn_logout.onClick.AddListener(() =>
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.SetInt(PlayerPrefsKey.key_authStatus, AuthStatus._INIT);
                SceneManager.LoadScene(SceneName._01_Title);
            });

            btn_setting.onClick.AddListener(() =>
            {
                SoundManager.instance.PlayEffectSound(upperButtonSound);
                settingPanel.SetActive(true);
            });

            btn_setting_close.onClick.AddListener(() => { settingPanel.SetActive(false); });

            AlertDoneBtn.onClick.AddListener(() => { AlertPanel.SetActive(false); });

            toggle_music.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    audioSource.Play();
                    soundUtil.turnOnBackgroundMusic();
                }
                else
                {
                    audioSource.Stop();
                    soundUtil.turnOffBackgroundMusic();
                }
            });

            toggle_sound_effect.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    soundUtil.turnOnSoundEffect();
                    SoundManager.instance.PlayEffectSound(upperButtonSound);
                }
                else
                {
                    soundUtil.turnOffSoundEffect();
                }
            });

            bnt_change_name.onClick.AddListener(() => { StartCoroutine(ChangeUserName(input_name.text)); });
        }

        private void ShowUserInfo(bool showWalletAlert)
        {
            Debug.Log(UserRepository.GetUsername());
            img_btn_wallet_alert.gameObject.SetActive(showWalletAlert);
            if (UserRepository.GetUsername() != null) input_name.text = UserRepository.GetUsername();
            eggText.text = UserRepository.GetEgg().ToString();
        }

        public IEnumerator GetUserInfo()
        {
            using var webRequest = UnityWebRequest.Get(ApiUrl.getUserInfo);
            webRequest.SetRequestHeader(ApiUrl.AuthGetHeader, AccessToken.GetAccessToken());
            yield return webRequest.SendWebRequest();
            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"Error: {webRequest.error}");
            }
            else
            {
                Debug.Log($"Received: {webRequest.downloadHandler.text}");
                var jsonData = webRequest.downloadHandler.text;
                var user = JsonUtility.FromJson<User>(jsonData);
                UserRepository.SetUserInfo(user.username, user.coin, user.egg);
                var noWallet = user.wallet_address is "" or null;
                ShowUserInfo(noWallet);
            }
        }


        public IEnumerator ChangeUserName(string new_name)
        {
            using (var webRequest = UnityWebRequest.Post(ApiUrl.ChangeUserName, ""))
            {
                webRequest.SetRequestHeader(ApiUrl.AuthGetHeader, AccessToken.GetAccessToken());
                webRequest.SetRequestHeader("Content-Type", "application/json");

                var json = "{\"username\":\"" + new_name + "\"}";
                Debug.Log(json);

                var bodyRaw = Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();

                yield return webRequest.SendWebRequest();

                if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log($"Error: {webRequest.error}");
                }
                else
                {
                    var responseText = webRequest.downloadHandler.text;
                    Debug.Log(responseText);
                    var missionResult = Mission.GetComponent<MissionUtils>();
                    missionResult.createWalletEvent();
                }
            }
        }
    }
}