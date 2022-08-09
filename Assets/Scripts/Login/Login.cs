using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BluehatGames
{


    public class PlayerJoinInfo
    {
        public string email;
        public string wallet_address;
    }
    public class PlayerInfo
    {
        public string email;
        public string wallet_address;
    }


    public class Login : MonoBehaviour
    {
        [Header("Buttons")]
        public Button btn_login;
        public Button btn_refresh;
        public Button btn_play;

        [Header("InputFields")]
        public InputField inputEmail;
        public InputField inputWallet;
        public string URL;

        [Header("Alert Popup")]
        public GameObject alertPopup;
        public Text alertText;
        private string emailMessage = "Email OK.";
        private string authCompleted = "Auth OK";
        private string warnEmailMessage = "Email Not OK.";
        private string warnWalletMessage = "Wallet Address Not OK.";


        [Header("Control Variables")]
        public int popupShowTime;


        private Coroutine popupCoroutine;

        // PlayerPref? ????¥?  ê²?
        // 1. ?´ë©ì¼ ?¸ì¦ì ë³´ë´? ?ë£ë§ ?ë©? ??ì§?
        // - Login ë²í¼? ?´ë©ì¼ ?¤? ë³´ë´ê¸? ë²í¼?¼ë¡? ë³?ê²? 
        // 2. ?´ë©ì¼ ?¸ì¦ì ?ë£í?ì§?
        // - ? ??ê¸? ë²í¼?¼ë¡? ë³?ê²?

        [Header("ForTest")]
        public bool isCompletedAuth;

        void SaveClientInfo(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        int GetClientInfo(string key) {
            return PlayerPrefs.GetInt(key);
        }

        void Start()
        {
            // Áö±Ý ¾î¶² °èÁ¤À¸·Î µÇ¾î ÀÖ´ÂÁö µð¹ö±ë 
            SaveData loadData = SaveSystem.LoadUserInfoFile();
            if (loadData != null)
            {
                Debug.Log($"Load Success! -> Email: {loadData.email} | walletAdd: {loadData.wallet_address}");
            }

            Debug.Log($"Client Current Status => {GetClientInfo(PlayerPrefsKey.key_authStatus)}");
            // ë¡ê·¸?¸ ë²í¼ onClick
            btn_login.onClick.AddListener(() =>
            {
                
                if (false == IsValidInputData(inputEmail.text, inputWallet.text))
                    return;

                btn_refresh.gameObject.SetActive(true);
                StartCoroutine(RequestAuthToServer(ApiUrl.emailLoginVerify, inputEmail.text, inputWallet.text, (UnityWebRequest request) =>
                {
                    StartCoroutine(ShowAlertPopup(emailMessage));

                    // json text from server response
                    var response = JsonUtility.FromJson<ResponseLogin>(request.downloadHandler.text);
                    Debug.Log($"response => {response.msg}");

                    if (response.msg != "fail")
                    {
                        SaveData user = new SaveData(inputEmail.text, inputWallet.text);
                        SaveSystem.SaveUserInfoFile(user);
                    }
                }));
            });

            // ë¦¬í? ? ë²í¼ onClick 
            btn_refresh.onClick.AddListener(() =>
            {
                SaveData loadData = SaveSystem.LoadUserInfoFile();
                if(loadData != null)
                {
                    Debug.Log($"Load Success! -> Email: {loadData.email} | walletAdd: {loadData.wallet_address}");
                }

                // ÀÌ¸ÞÀÏ ÀÎÁõÇÒ ¶§ ÀÔ·ÂÇÑ °ªÀ» ·ÎÄÃ¿¡ ÀúÀåµÈ °É ºÒ·¯¿Í¼­ ÀÌ¸ÞÀÏ ÀÎÁõ ¿Ï·á ¿©ºÎ¿¡ ´ëÇØ ¼­¹ö¿¡ Ã¼Å©ÇÔ
                StartCoroutine(RequestAuthToServer(ApiUrl.login, loadData.email, loadData.wallet_address, (UnityWebRequest request) =>
                {                  
                    // ?¹?ë²ë¡ë¶??° ë°ì?? ??µ ?´?© ì¶ë ¥
                    Debug.Log(request.downloadHandler.text);
                    var response = JsonUtility.FromJson<ResponseLogin>(request.downloadHandler.text);
                    Debug.Log($"response => {response} | response.msg = {response.msg}");
                    if(response.msg == "Register Success" || response.msg == "Login Success")
                    {
                        if (null != popupCoroutine)
                        {
                            // ê¸°ì¡´ ì½ë£¨?´?´ ???¤ë©? ? ì§???¤ê³? ?ë¡ì´ ì½ë£¨?´?´ ?¤???ë¡? ?¨ 
                            StopCoroutine(popupCoroutine);
                        }

                        StartCoroutine(ShowAlertPopup(authCompleted));

                        SetJoinCompletedSetting();
                        SaveClientInfo(PlayerPrefsKey.key_authStatus, AuthStatus._JOIN_COMPLETED);
                        PlayerPrefs.SetString(PlayerPrefsKey.key_accessToken, response.access_token);
                        Debug.Log("Login access_token response => " + PlayerPrefs.GetString(PlayerPrefsKey.key_accessToken));
                        
                    } else
                    {
                        Debug.LogError("Server: Email not Verified.");
                    }
                }));
            });



            // ? ??ê¸? ë²í¼
            // 1. ê¸°ë³¸??? ë¹í?±?
            // 2. ??±? ?? ??  ?
            // - Refresh ë²í¼? ??¬? ?´ë©ì¼ ?¸ì¦ë ê²? ??¸??? ê²½ì°
            // - 
            btn_play.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(SceneName._03_Main);
            });


            // ì²ì?? ?? ?´ ë²í¼, ë¦¬í? ? ë²í¼ ë¹í?±?
            btn_play.gameObject.SetActive(false);
            btn_refresh.gameObject.SetActive(false);


            var clientAuthInfo = GetClientInfo(PlayerPrefsKey.key_authStatus);
            // ?¸ì¦? ??? ?°?¼ ë¶ê¸° 
            // ?´ë©ì¼? ë³´ë¸ ???´ë©? ?´ë©ì¼ ?¬? ?¡ ë²í¼ ??
            if (clientAuthInfo == AuthStatus._EMAIL_AUTHENTICATING)
            {
                SetEmailAuthenticatingSetting();
            }
            else if (clientAuthInfo == AuthStatus._JOIN_COMPLETED)
            {

                SetJoinCompletedSetting();
            }
            else
            {
                // ?ë¬´ë° ? ë³´ë ??¼ë©? ì´ê¸°?
                SaveClientInfo(PlayerPrefsKey.key_authStatus, AuthStatus._INIT);
            }
        }

        public void SetEmailAuthenticatingSetting()
        {
            btn_login.GetComponentInChildren<Text>().text = "Resend\n Email";
            btn_refresh.gameObject.SetActive(true);
        }

        public void SetJoinCompletedSetting()
        {
            btn_play.gameObject.SetActive(true);

        }


        IEnumerator ShowAlertPopup(string text)
        {
            alertText.text = text;
            alertPopup.SetActive(true);
            yield return new WaitForSeconds(popupShowTime);
            alertPopup.SetActive(false);
        }

        IEnumerator RequestAuthToServer(string URL, string inputEmail, string inputWallet, Action<UnityWebRequest> action)
        {
            Debug.Log($"RequestAuthToServer -> URL: {URL}");
            string jsonData = "";
            // ?´ë©ì¼ê³? ì§?ê°ì£¼?ë¥? Json ???¼ë¡? ë³?? 
            if (URL == ApiUrl.emailLoginVerify)
            {
                jsonData = SetPlayerJoinInfoToJsonData(inputEmail, inputWallet);
                // TEST; ?´ë©ì¼ ?¸ì¦? ?ë£ë ê²½ì°? ?ë¡ì°
                if (null != popupCoroutine)
                {
                    // ê¸°ì¡´ ì½ë£¨?´?´ ???¤ë©? ? ì§???¤ê³? ?ë¡ì´ ì½ë£¨?´?´ ?¤???ë¡? ?¨ 
                    StopCoroutine(popupCoroutine);
                }

                StartCoroutine(ShowAlertPopup(emailMessage));
            } 
            else if (URL == ApiUrl.login)
            {
                // ÀÎÁõ Á¤º¸¸¦ ¿äÃ»ÇÏ·Á´Â µ¥ÀÌÅÍ¸¦ JsonÀ¸·Î ¹Ù²Þ 
                jsonData = SetPlayerInfoToJsonData(inputEmail, inputWallet);

            }
            Debug.Log(jsonData);


            btn_login.GetComponentInChildren<Text>().text = "Resend Email";

            byte[] byteEmail = Encoding.UTF8.GetBytes(jsonData);
            // ?¹?ë²ë¡ Post ?ì²?? ë³´ë
            using (UnityWebRequest request = UnityWebRequest.Post(URL, jsonData))
            {
                request.uploadHandler = new UploadHandlerRaw(byteEmail); // ?ë¡ë ?¸?¤?¬
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer(); // ?¤?´ë¡ë ?¸?¤?¬
                                                                                        // ?¤?ë¥? Json?¼ë¡? ?¤? 
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("request Error!");
                    Debug.Log(request.error + " | " + request);
                }
                else
                {
                    Debug.Log("request Success! Action Invoke");
                    action.Invoke(request);
                    // ÀÌ¸ÞÀÏ Àü¼Û¿¡ ¼º°øÇÑ °æ¿ì¿¡¸¸ ÀÎÁõ»óÅÂ¸¦ º¯°æ
                    if(URL == ApiUrl.emailLoginVerify)
                    {
                        SaveClientInfo(PlayerPrefsKey.key_authStatus, AuthStatus._EMAIL_AUTHENTICATING);  
                    }
                    else
                    {
                        SaveClientInfo(PlayerPrefsKey.key_authStatus, AuthStatus._JOIN_COMPLETED);  
                    }
                }
            }
        }


        string SetPlayerJoinInfoToJsonData(string inputEmail, string inputWallet)
        {
            // ?ë²ë¡ ë³´ë¼ Json ?°?´?° ??
            PlayerJoinInfo playerInfo = new PlayerJoinInfo();

            playerInfo.email = inputEmail;
            playerInfo.wallet_address = inputWallet;

            return JsonUtility.ToJson(playerInfo);

        }

        string SetPlayerInfoToJsonData(string inputEmail, string inputWallet)
        {
            // ?ë²ë¡ ë³´ë¼ Json ?°?´?° ??
            PlayerInfo playerInfo = new PlayerInfo();

            playerInfo.email = inputEmail;
            playerInfo.wallet_address = inputWallet;

            return JsonUtility.ToJson(playerInfo); ;

        }

        bool IsValidInputData(string inputEmail, string inputWallet)
        {
            // 1. ? ?¨ ?°?´?°?¸ì§? ê²??¬ 
            // - ?´ë©ì¼ ì£¼ìê°? ?ë§ì?? ???¸ê°?
            if (false == IsValidEmail(inputEmail))
            {
                popupCoroutine = StartCoroutine(ShowAlertPopup(warnEmailMessage));
                return false;
            }
            // - ì§?ê°? ì£¼ìê°? ë¹ì´??ê°?
            if ("" == inputWallet)
            {
                popupCoroutine = StartCoroutine(ShowAlertPopup(warnWalletMessage));
                return false;
            }

            return true;
        }
        private bool IsValidEmail(string email)
        {
            bool valid = Regex.IsMatch(email, @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?");
            return valid;
        }
    }
}