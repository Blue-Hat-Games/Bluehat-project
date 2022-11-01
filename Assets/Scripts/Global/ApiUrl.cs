namespace BluehatGames
{
    public class ApiUrl
    {
        public enum ApiCategory
        {
            emailLoginVerify,
            userLogin,
            postAnimalNew,
            getAnimalList
        }

        public string liveServer = "https://api.bluehat.games";
        public string testServer = "";

        //Login
        public const string emailLoginVerify = "https://api.bluehat.games/auth";
        public const string login = "https://api.bluehat.games/user";

        //Animal
        public const string postAnimalNew = "https://api.bluehat.games/animal/make-animal";
        public const string getAnimalList = "https://api.bluehat.games/animal";
        public const string getUserAnimal = "https://api.bluehat.games/animal/get-user-animal";

        //Synthesis
        public const string postChangeColor = "https://api.bluehat.games/animal/change-color";

        //Get Header Authorization
        public const string AuthGetHeader = "Authorization";

        // 해당되는 값이 없을 때 리턴할까 싶어서 만들어 본 변수인데 또 굳이..? 싶기도? 
        public const string failAddress = "failedAddress";

        public string GetLiveServerApiUrl(ApiCategory apiCategory)
        {
            string url = "";

            switch (apiCategory)
            {
                case ApiCategory.emailLoginVerify:
                    url = $"{liveServer}/{emailLoginVerify}";
                    break;
                case ApiCategory.userLogin:
                    url = $"{liveServer}/{login}";
                    break;
                case ApiCategory.postAnimalNew:
                    url = $"{liveServer}/{postAnimalNew}";
                    break;
                case ApiCategory.getAnimalList:
                    url = $"{liveServer}/{getAnimalList}";
                    break;
                default:

                    break;
            }
            return url;
        }

        public string GetTestServerApiUrl(ApiCategory apiCategory)
        {
            string url = "";

            switch (apiCategory)
            {
                case ApiCategory.emailLoginVerify:
                    url = $"{testServer}/{emailLoginVerify}";
                    break;
                case ApiCategory.userLogin:
                    url = $"{testServer}/{login}";
                    break;
                case ApiCategory.postAnimalNew:
                    url = $"{testServer}/{postAnimalNew}";
                    break;
                case ApiCategory.getAnimalList:
                    url = $"{testServer}/{getAnimalList}";
                    break;
                default:

                    break;
            }
            return url;
        }
    }

    public class ResponseLogin
    {
        public string msg;
        public string access_token;
    }

    public class ResponseAnimalNew
    {
        public string id;
        public string type;
    }

    public class ResponseResult
    {
        public string msg;
    }

    public class RequestColorChangeAnimalFormat
    {
        public string animalId;
    }
}
