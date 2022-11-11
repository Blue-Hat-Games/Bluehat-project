using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace BluehatGames
{
    public class ColorChangeManager : MonoBehaviour
    {
        public bool isTest;
        public string tempAccessToken = "0000";

        public AnimalDataFormat selectedAnimalData;
        public GameObject resultAnimalParticle;
        public Transform thumbnailSpot;

        private GameObject resultAnimal;

        private GameObject selectedAnimalObject;

        private SynthesisManager synthesisManager;

        private GameObject tempParticle;

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                if (selectedAnimalObject != null)
                    selectedAnimalObject
                        .transform
                        .Rotate(0f, -Input.GetAxis("Mouse X") * 10, 0f, Space.World);

                if (resultAnimal != null)
                    resultAnimal.transform.Rotate(0f, -Input.GetAxis("Mouse X") * 10, 0f, Space.World);
            }
        }

        public void SetSynthesisManager(SynthesisManager instance)
        {
            synthesisManager = instance;
        }

        public void ClearResultAnimal()
        {
            if (resultAnimal)
            {
                resultAnimal.SetActive(false);
                DestroyParticle();
            }

            if (selectedAnimalObject) selectedAnimalObject.SetActive(false);
        }

        public void SetCurSelectedAnimal(AnimalDataFormat animalData, GameObject animalObject)
        {
            selectedAnimalData = animalData;
            selectedAnimalObject = animalObject;
        }

        public void SendColorChangeAPI()
        {
            StartCoroutine(GetColorChangeResultFromServer(ApiUrl.postChangeColor));
        }

        public IEnumerator GetColorChangeResultFromServer(string URL)
        {
            // Access Token
            var access_token = PlayerPrefs.GetString(PlayerPrefsKey.key_accessToken);

            // TODO: 테스트이면 0000 으로 
            if (isTest) access_token = tempAccessToken;
            Debug.Log($"access_token = {access_token}");
            using var webRequest = UnityWebRequest.Post(URL, "");
            webRequest.SetRequestHeader(ApiUrl.AuthGetHeader, access_token);
            webRequest.SetRequestHeader("Content-Type", "application/json");

            var requestData = new RequestColorChangeAnimalFormat();
            requestData.animalId = selectedAnimalData.id;

            var json = JsonUtility.ToJson(requestData);
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

                var responseMsg = JsonUtility.FromJson<ResponseResult>(responseText).msg;
                Debug.Log($"ColorChangeManager | [{URL}] - {responseMsg}");

                // refresh data
                synthesisManager.SendRequestRefreshAnimalData(selectedAnimalData.id, true);
            }
        }

        public void OnRefreshAnimalDataAfterColorChange()
        {
            var obj = synthesisManager.GetAnimalObject(selectedAnimalData.id);
            obj.transform.position = thumbnailSpot.transform.position;

            obj.SetActive(true);
            resultAnimal = obj;

            // obj.transform.position = new Vector3(-2, -1, obj.transform.position.z);
            CreateResultAnimalParticle(obj.transform);
            obj.transform.LookAt(Camera.main.transform);
            obj.transform.eulerAngles = new Vector3(0, obj.transform.eulerAngles.y, 0);

            synthesisManager.SetResultLoadingPanel(false);
        }

        private void CreateResultAnimalParticle(Transform particlePoint)
        {
            var newPos = new Vector3(particlePoint.position.x, particlePoint.position.y + 0.5f,
                particlePoint.position.z);
            tempParticle = Instantiate(resultAnimalParticle, newPos, Quaternion.identity);
            tempParticle.GetComponent<ParticleSystem>().Play();
        }

        private void DestroyParticle()
        {
            Destroy(tempParticle);
        }

        // EncodeToPNG 함수를 사용하기 위해 원래 에셋의 텍스처 포맷을 변경해야 함
        // 그리고 byte를 생성해서 string값을 서버와 주고 받기 위한 함수

        private void ChangeTextureFormat(Texture2D testTexture)
        {
            var tex =
                new Texture2D(testTexture.width,
                    testTexture.height,
                    TextureFormat.RGB24,
                    false);
            var sourcePixels = testTexture.GetPixels();

            for (var h = 0; h < testTexture.height; h++)
            for (var w = 0; w < testTexture.width; w++)
            {
                var color = sourcePixels[h * testTexture.width + w];
                tex.SetPixel(w, h, color);
            }

            tex.Apply();

            var myTextureBytes = tex.EncodeToPNG();

            var myTextureBytesEncodedAsBase64 =
                Convert.ToBase64String(myTextureBytes);
            Debug
                .Log("myTextureBytesEncodedAsBase64: " +
                     myTextureBytesEncodedAsBase64);

            // 만들어진 string을 통해 텍스처를 로드해서 다시 적용
            var loadImg =
                new Texture2D(testTexture.width, testTexture.height);
            loadImg
                .LoadImage(Convert
                    .FromBase64String(myTextureBytesEncodedAsBase64));

            //testAnimal.material.SetTexture("_MainTex", loadImg);
        }
    }
}