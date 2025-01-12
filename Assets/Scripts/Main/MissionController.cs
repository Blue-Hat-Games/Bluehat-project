using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace BluehatGames
{
    public class MissionController : MonoBehaviour
    {
        public Button btn_mission;
        public Image img_btn_mission_alert;
        public GameObject missionPanel;
        public Button btn_mission_pannel_close;
        public AudioClip upperButtonSound;

        public Transform missionContent;
        public GameObject missionCardPrefab;

        private void Start()
        {
            missionPanel.SetActive(false);
            btn_mission.onClick.AddListener(() =>
            {
                SoundManager.instance.PlayEffectSound(upperButtonSound);
                LoadMission();
                missionPanel.SetActive(true);
            });

            btn_mission_pannel_close.onClick.AddListener(() => { missionPanel.SetActive(false); });
        }

        public void LoadMission()
        {
            StartCoroutine(GetMissionFromSever());
        }


        private IEnumerator GetMissionFromSever()
        {
            using var webRequest = UnityWebRequest.Get(ApiUrl.getQuestList);
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
                var questList = JsonHelper.FromJson<Quest>(jsonData);
                StartCoroutine(createQuestCard(questList));
            }
        }

        private IEnumerator createQuestCard(Quest[] questList)
        {
            for (var i = 0; i < questList.Length; i++)
            {
                var quest = questList[i];
                Debug.Log($"quest = {quest.title}");
                var questCard = Instantiate(missionCardPrefab, missionContent, false);
                questCard.transform.Find("Title").GetComponent<Text>().text = quest.title;
                questCard.transform.Find("Description").GetComponent<Text>().text = quest.description;
                questCard.transform.Find("CoinText").GetComponent<Text>().text = quest.reward_coin.ToString();
                questCard.transform.Find("EggText").GetComponent<Text>().text = quest.reward_egg.ToString();
                if (quest.status) questCard.transform.Find("Button").GetComponent<Button>().interactable = false;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}