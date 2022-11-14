using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;


namespace BluehatGames
{
    public class AccessoryManager : MonoBehaviour
    {
        private SynthesisManager synthesisManager;
        public AnimalDataFormat selectedAnimalData;
        public GameObject selectedAnimalObject;

        public GameObject hatParticle;
        private GameObject resultAnimal;

        public void SetSynthesisManager(SynthesisManager instance)
        {
            synthesisManager = instance;
        }

        public void SendRandomHatAPI()
        {
            StartCoroutine(GetRandomHatResultFromServer(ApiUrl.getRandomHat));
        }

        public IEnumerator GetRandomHatResultFromServer(string URL)
        {

            UnityWebRequest request = UnityWebRequest.Get(URL);

            using (UnityWebRequest webRequest = UnityWebRequest.Post(URL, ""))
            {
                webRequest.SetRequestHeader(ApiUrl.AuthGetHeader, AccessToken.GetAccessToken());
                webRequest.SetRequestHeader("Content-Type", "application/json");

                RequestRandomHatFormat requestData = new RequestRandomHatFormat();
                requestData.animalId = selectedAnimalData.id;

                string json = JsonUtility.ToJson(requestData);
                Debug.Log(json);

                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Debug.Log($"Error: {webRequest.error}");
                }
                else
                {
                    string responseText = webRequest.downloadHandler.text;

                    var new_item = JsonUtility.FromJson<ResponseHatResult>(responseText).new_item;
                    Debug.Log($"AccessoryManager | [{URL}] - new_item = {new_item}");
                    LoadHatItemPrefab(new_item);
                    synthesisManager.SetResultLoadingPanel(false);

                    // refresh data
                    synthesisManager.SendRequestRefreshAnimalData(selectedAnimalData.id, false);
                }
                webRequest.Dispose();
            }

        }

        private Transform curHatPoint;
        private void LoadHatItemPrefab(string itemName)
        {
            var path = $"Prefab/Hats/{itemName}";
            GameObject obj = Resources.Load(path) as GameObject;
            GameObject hatObj = Instantiate(obj);
            Transform[] allChildren = selectedAnimalObject.GetComponentsInChildren<Transform>();
            Transform hatPoint = null;

            foreach (Transform childTr in allChildren)
            {
                if (childTr.name == "HatPoint")
                {
                    hatPoint = childTr;
                }
            }

            curHatPoint = hatPoint;

            if (hatPoint.childCount > 0)
            {
                Destroy(hatPoint.GetChild(0).gameObject);
            }
            hatObj.transform.SetParent(hatPoint);
            hatObj.transform.localPosition = Vector3.zero;
            hatObj.transform.localEulerAngles = Vector3.zero;
        }

        private GameObject tempParticle;
        public void CreateHatParticle()
        {
            Vector3 newPos = new Vector3(-2, curHatPoint.position.y, 0);
            tempParticle = Instantiate(hatParticle, newPos, Quaternion.identity);
            tempParticle.GetComponent<ParticleSystem>().Play();
            Invoke("DestroyParticle", 2.0f);
        }

        private void DestroyParticle()
        {
            GameObject.Destroy(tempParticle);
        }

        public void SetCurSelectedAnimal(AnimalDataFormat animalData, GameObject animalObject)
        {
            selectedAnimalData = animalData;
            selectedAnimalObject = animalObject;
        }

        void Start()
        {

        }

        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                if (selectedAnimalObject != null)
                {
                    selectedAnimalObject
                    .transform
                    .Rotate(0f, -Input.GetAxis("Mouse X") * 10, 0f, Space.World);
                }
            }

            if (resultAnimal != null)
            {
                resultAnimal.transform.Rotate(0f, -Input.GetAxis("Mouse X") * 10, 0f, Space.World);
            }

        }
    }
}
