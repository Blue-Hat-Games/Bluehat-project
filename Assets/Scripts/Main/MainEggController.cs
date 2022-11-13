using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace BluehatGames
{
    public class MainEggController : MonoBehaviour
    {
        public Text eggText;
        public Button eggButton;
        public GameObject eggResultPanel;

        public GameObject eggAlertPanel;

        public Camera overUICamera;
        public Transform newAnimalTr;

        public Button resultExitButton;
        public Text resultAnimalText;

        private GameObject myNewAnimal;

        private int egg;

        void Start()
        {
            eggAlertPanel.SetActive(false);

            eggButton.onClick.AddListener(() =>
            {
                if (eggAlertPanel.activeSelf)
                {
                    return;
                }
                if (eggResultPanel.activeSelf)
                {
                    eggResultPanel.SetActive(false);
                    GameObject.Destroy(myNewAnimal);
                    return;
                }
                if (UserRepository.GetEgg() <= 0)
                {
                    StartCoroutine(ShowAlertPanel());
                    return;
                }

                else
                {
                    StartCoroutine(GetNewAnimalFromServer(ApiUrl.postAnimalNew));
                    UserRepository.SetEgg(UserRepository.GetEgg() - 1);
                    eggText.text = UserRepository.GetEgg().ToString();
                }

            });


            resultExitButton.onClick.AddListener(() =>
            {
                eggResultPanel.SetActive(false);
                GameObject.Destroy(myNewAnimal);
            });
        }


        public IEnumerator GetNewAnimalFromServer(string URL)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(URL, ""))
            {
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                // send access token to server
                request.SetRequestHeader(ApiUrl.AuthGetHeader, AccessToken.GetAccessToken());

                yield return request.SendWebRequest();

                // error
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(request.error);
                }
                // success
                else
                {
                    string responseText = request.downloadHandler.text;
                    string responseType = JsonUtility.FromJson<ResponseAnimalNew>(responseText).type;

                    Debug.Log(request.downloadHandler.text);

                    string animalName = responseType;
                    resultAnimalText.text = $"{animalName.ToString()}!";

                    LoadAnimalPrefab(animalName);
                    ShowResultPanel();
                }
            }
        }

        private void LoadAnimalPrefab(string animalName)
        {
            var path = $"Prefab/Animals/{animalName}";
            GameObject obj = Resources.Load(path) as GameObject;
            GameObject animal = Instantiate(obj, newAnimalTr.transform.position, Quaternion.identity);
            int overUILayer = LayerMask.NameToLayer("OverUI");
            animal.GetComponentInChildren<Renderer>().gameObject.layer = overUILayer;
            ResetAnimalState(animal);
            animal.transform.LookAt(overUICamera.transform);

            myNewAnimal = animal;
        }

        void ResetAnimalState(GameObject animal)
        {
            animal.GetComponent<Rigidbody>().useGravity = false;
            animal.GetComponent<Rigidbody>().isKinematic = true;
            animal.GetComponent<CapsuleCollider>().enabled = false;
        }

        void ShowResultPanel()
        {
            eggResultPanel.SetActive(true);
        }

        IEnumerator ShowAlertPanel()
        {
            eggAlertPanel.SetActive(true);
            yield return new WaitForSeconds(3);
            eggAlertPanel.SetActive(false);
        }
    }
}
