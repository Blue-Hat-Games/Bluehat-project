using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace BluehatGames {

public class AnimalFactory : MonoBehaviour
{
    private string pngSavePath = "";

    [System.Serializable]
    public class AnimalDataFromServer
    {
        public string name;
        public int tier;
        public string color;
        public string id;
        public string animalType;
        public string headItem;
        public string pattern;
    }

    void Start()
    {
        pngSavePath = $"{Application.dataPath}/RuntimeImages";
    }

    public Dictionary<string, GameObject> ConvertJsonToAnimalObject(string jsonData)
    {
        Dictionary<string, GameObject> animalObjectDictionary = new Dictionary<string, GameObject>();

        Animal[] animalList = GetAnimalDataFromJson(jsonData);
        foreach (Animal data in animalList)
        {
            GameObject animalObj = GetAnimalGameObject(data);
            animalObjectDictionary.Add(data.id, animalObj);
        }

        return animalObjectDictionary;
    }


    private Animal[] GetAnimalDataFromJson(string txt)
    {
        var animalData = JsonHelper.FromJson<AnimalDataFormat>(txt);
        Debug.Log($"animalData = {animalData.Length}");
        Animal[] animalList = new Animal[animalData.Length];

        for (int i = 0; i < animalData.Length; i++)
        {
            Animal animal = new Animal(animalData[i]);
            animalList[i] = animal;
        }

        return animalList;
    }

    public void ChangeTextureAnimalObject(GameObject animalObj, Animal animalData)
    {
        Texture2D meshTex = animalData.getAnimalTexture();
        animalObj.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", meshTex);
    }

    // synthesis manager 같은 곳에서 하나의 동물을 불러올 때 사용 
    public GameObject GetAnimalGameObject(Animal animalData)
    {
        // Debug.Log($"type = {animalData.animalType}");
        GameObject animalPrefab = animalData.getAnimalPrefab();

        GameObject animalObj = GameObject.Instantiate(animalPrefab);
        // animalObj.transform.LookAt(Camera.main.transform);
        animalObj.name = $"{animalData.animalType}_{animalData.id}";

        Texture2D meshTex = animalData.getAnimalTexture();
        animalObj.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", meshTex);

        return animalObj;
    }

}
}