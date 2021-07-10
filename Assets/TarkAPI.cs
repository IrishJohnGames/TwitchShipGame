using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// api implementation found at https://github.com/tarkadal/playerlevelapi
/// </summary>
public class TarkAPI
{
    string _baseUrl = string.Empty;
    public TarkAPI(string baseUrl)
    {
        _baseUrl = baseUrl;
    }
    
    /// <summary>
    /// wrapper to make get request
    /// </summary>
    IEnumerator Get(string resource, Action<UnityWebRequest> callback)
    {
        var www = UnityWebRequest.Get(_baseUrl + resource);
        yield return www.SendWebRequest();
        callback?.Invoke(www);
    }

    /// <summary>
    /// wrapper to make post request
    /// </summary>
    IEnumerator Post(string resource, Action<UnityWebRequest> callback)
    {
        //        MonoBehaviour.print(_baseUrl + resource);

        var reqUrl = _baseUrl + resource;
        var www = UnityWebRequest.Post(reqUrl, (string)null);
        //www.method = "POST";
        //JsonConvert.SerializeObject(new { randombody = true })
        yield return www.SendWebRequest();
        callback?.Invoke(www);
    }

    void OnInvalidResponse(UnityWebRequest req)
    {
        Debug.LogError($"request to {req.url} failed with status code {req.responseCode}");
    }

    /// <summary>
    /// fetches all players stored in database
    /// </summary>
    /// <returns>array of users with data model that looks like { name:"", level: 0}</returns>
    public IEnumerator GetAllPlayers(Action<TarkUserModel[]> callback)
    {
        return Get("/players/all", (req) =>
        {
            if (req.result == UnityWebRequest.Result.Success)
            {
                var data = JsonConvert.DeserializeObject<TarkUserModel[]>(req.downloadHandler.text);
                callback(data);
            }
            else
            {
                OnInvalidResponse(req);
            }
        });
    }

    /// <summary>
    /// fetches user stored in database by name
    /// </summary>
    /// <returns>array of users with data model that looks like { name:"", level: 0}</returns>
    public IEnumerator GetPlayer(string name, Action<TarkUserModel[]> callback)
    {
        return Get($"/players/get?name={name}", (req) =>
        {
            if (req.result == UnityWebRequest.Result.Success)
            {

                var data = JsonConvert.DeserializeObject<TarkUserModel[]>(req.downloadHandler.text);
                callback(data);
            }
            else
            {
                OnInvalidResponse(req);
            }
        });
    }

    /// <summary>
    /// adds player to the database
    /// </summary>
    /// <returns>{success:true or false}</returns>
    public IEnumerator AddPlayer(string name, int level, Action<TarkPost> callback)
    {
        //
        return Post($"/players/add?name={name}&level={level}", (req) =>
        {
            if (req.result == UnityWebRequest.Result.Success)
            {

                var data = JsonConvert.DeserializeObject<TarkPost>(req.downloadHandler.text);
                callback(data);
            }
            else
            {
                OnInvalidResponse(req);
            }
        });
    }

    /// <summary>
    /// deletes player from the database
    /// </summary>
    /// <returns>{success:true or false}</returns>
    public IEnumerator DeletePlayer(string name, Action<TarkPost> callback)
    {
        return Post($"/players/delete?name={name}&level=1", (req) =>
        {
            if (req.result == UnityWebRequest.Result.Success)
            {

                var data = JsonConvert.DeserializeObject<TarkPost>(req.downloadHandler.text);
                callback(data);
            }
            else
            {
                OnInvalidResponse(req);
            }
        });
    }

    /// <summary>
    /// updates player in the database
    /// </summary>
    /// <returns>{success:true or false}</returns>
    public IEnumerator UpdatePlayer(string name, int level, Action<TarkPost> callback)
    {
        return Post($"/players/update?name={name}&level={level}", (req) =>
        {
            if (req.result == UnityWebRequest.Result.Success)
            {

                var data = JsonConvert.DeserializeObject<TarkPost>(req.downloadHandler.text);
                callback(data);
            }
            else
            {
                OnInvalidResponse(req);
            }
        });
    }

    /// <summary>
    /// clears the database
    /// </summary>
    /// <returns>{success:true or false}</returns>
    public IEnumerator ResetAllPlease(Action<TarkPost> callback)
    {
        return Post($"/players/resetallplease", (req) =>
        {
            if (req.result == UnityWebRequest.Result.Success)
            {

                var data = JsonConvert.DeserializeObject<TarkPost>(req.downloadHandler.text);
                callback(data);
            }
            else
            {
                OnInvalidResponse(req);
            }
        });
    }
}

public class TarkPost
{
    public bool success;
}

public class TarkUserModel
{
    public int level;
    public string name;

}