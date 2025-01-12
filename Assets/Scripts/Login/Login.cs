using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BluehatGames
{
    public class LoginBtn
    {
        public enum LoginBtnStatus
        {
            SendEmail,
            Login
        }

        private readonly Button btn_login;
        private readonly Button btn_resend_email;
        private LoginBtnStatus btn_status;

        public LoginBtn(Button btn_login, Button btn_resend_email)
        {
            this.btn_login = btn_login;
            this.btn_resend_email = btn_resend_email;
        }

        public LoginBtnStatus GetBtnStatus()
        {
            return btn_status;
        }

        public void SetBtnSendEmail()
        {
            btn_status = LoginBtnStatus.SendEmail;
            btn_login.GetComponentInChildren<Text>().text = "Send Email";
            btn_resend_email.gameObject.SetActive(false);
        }

        public void SetBtnLogin()
        {
            btn_status = LoginBtnStatus.Login;
            btn_login.GetComponentInChildren<Text>().text = "Login";
            btn_resend_email.gameObject.SetActive(true);
        }
    }

    public class PlayerInfo
    {
        public string email;

        public PlayerInfo(string email)
        {
            this.email = email;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }


    public class Login : MonoBehaviour
    {
        [Header("Buttons")] public Button btn_login;

        public Button btn_resend_email;

        [Header("InputFields")] public InputField inputEmail;

        [Header("Alert Popup")] public GameObject alertPopup;

        public Text alertText;


        [Header("Control Variables")] public int popupShowTime;

        private readonly string authCompleted = "인증에 성공했습니다!";
        private readonly string emailMessage = "이메일을 보냈습니다.\n 메일함에서 인증을 완료해주세요!";

        private Coroutine popupCoroutine;
        private readonly string warnEmailMessage = "유효한 이메일이 아닙니다.";
        private string warnWalletMessage = "Wallet Address Not OK.";

        private void Start()
        {
            var loginBtn = new LoginBtn(btn_login, btn_resend_email);
            loginBtn.SetBtnSendEmail();

            // If click resend email button, login btn status change
            btn_resend_email.onClick.AddListener(() => { loginBtn.SetBtnSendEmail(); });

            // Login Btn Click
            btn_login.onClick.AddListener(() =>
            {
                var email = inputEmail.text;
                if (false == IsValidInputData(email)) return; // Input Data is not Valid

                // If Login Btn Status Send email
                if (loginBtn.GetBtnStatus() == LoginBtn.LoginBtnStatus.SendEmail)
                    StartCoroutine(RequestAuthToServer(ApiUrl.emailLoginVerify, email, request =>
                    {
                        StartCoroutine(ShowAlertPopup(emailMessage));

                        // json text from server response
                        var response = JsonUtility.FromJson<ResponseLogin>(request.downloadHandler.text);

                        if (response.msg == "fail")
                        {
                            StartCoroutine(ShowAlertPopup("이메일을 보내지 못했습니다.\n 다시 시도해주세요."));
                            return;
                        }

                        Debug.Log("이메일을 보냈습니다.\n 메일함에서 인증을 완료해주세요!");
                        loginBtn.SetBtnLogin();
                    }));

                // If Login Btn status is Login
                else
                    StartCoroutine(RequestAuthToServer(ApiUrl.login, email, request =>
                    {
                        var response = JsonUtility.FromJson<ResponseLogin>(request.downloadHandler.text);
                        if (response.msg is "Register Success" or "Login Success")
                        {
                            if (null != popupCoroutine) StopCoroutine(popupCoroutine);
                            StartCoroutine(ShowAlertPopup(authCompleted));
                            if (response.msg == "Register Success")
                                SaveClientInfo(PlayerPrefsKey.key_authStatus, AuthStatus._JOIN_COMPLETED);
                            else
                                SaveClientInfo(PlayerPrefsKey.key_authStatus, AuthStatus._LOGIN_COMPLETED);
                            AccessToken.SetAccessToken(response.access_token);
                            SceneManager.LoadScene(SceneName._03_Main);
                        }
                        else
                        {
                            Debug.LogError("Server: Email not Verified.");
                        }
                    }));
            });
        }

        private void SaveClientInfo(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        private int GetClientInfo(string key)
        {
            return PlayerPrefs.GetInt(key);
        }


        private IEnumerator ShowAlertPopup(string text)
        {
            alertText.text = text;
            alertPopup.SetActive(true);
            yield return new WaitForSeconds(popupShowTime);
            alertPopup.SetActive(false);
        }

        private IEnumerator RequestAuthToServer(string URL, string inputEmail, Action<UnityWebRequest> action)
        {
            Debug.Log($"RequestAuthToServer | URL: {URL}, inputEmail: {inputEmail}");
            var playerInfo = new PlayerInfo(inputEmail);
            var jsonData = playerInfo.ToJson();
            Debug.Log("Resutlt = " + playerInfo.ToJson());

            // 'emailLoginVerify' or 'login'
            if (URL == ApiUrl.emailLoginVerify)
            {
                if (null != popupCoroutine) StopCoroutine(popupCoroutine);

                // alert popup 
                StartCoroutine(ShowAlertPopup(emailMessage));
            }

            // byteEmail 
            var byteEmail = Encoding.UTF8.GetBytes(jsonData);

            using (var request = UnityWebRequest.Post(URL, jsonData))
            {
                request.uploadHandler = new UploadHandlerRaw(byteEmail);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();
                if (request.responseCode == 409)
                {
                    Debug.Log("Email Not Verified");
                    StartCoroutine(ShowAlertPopup("Email Not Verified"));
                }
                else if (request.result is UnityWebRequest.Result.ConnectionError
                         or UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("request Error!");
                    Debug.Log(request.responseCode);
                    Debug.Log(request.error + " | " + request);
                }
                else
                {
                    Debug.Log("request Success!");
                    action.Invoke(request);

                    // URL -> 'emailLoginVerify' or 'login'
                    if (URL == ApiUrl.emailLoginVerify)
                        SaveClientInfo(PlayerPrefsKey.key_authStatus, AuthStatus._EMAIL_AUTHENTICATING);
                    else
                        SaveClientInfo(PlayerPrefsKey.key_authStatus, AuthStatus._JOIN_COMPLETED);
                }
            }
        }


        private bool IsValidInputData(string inputEmail)
        {
            // warn email
            if (false == IsValidEmail(inputEmail))
            {
                popupCoroutine = StartCoroutine(ShowAlertPopup(warnEmailMessage));
                return false;
            }

            return true;
        }

        // check valid email
        private bool IsValidEmail(string email)
        {
            var valid = Regex.IsMatch(email,
                @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?");
            return valid;
        }
    }
}