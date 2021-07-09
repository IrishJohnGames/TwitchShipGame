using System;
using System.Collections;
using TwitchLib.Api.V5.Models.Users;
using TwitchLib.Client.Models;
using TwitchLib.PubSub.Events;
using TwitchLib.Unity;
using UnityEngine;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace CoreTwitchLibSetup
{
	public class TwitchLibCtrl : ManagerBase<TwitchLibCtrl>
	{
		ConcurrentBag<Spawnables> ToSpawn = new ConcurrentBag<Spawnables>();

		const int RESET_TIMER_CLEAR_MEM = 1, BUFFER_TIME_INCREMENT = 1;
		List<MessageCache> MessagesReceivedIRC = new List<MessageCache>();
		float TimeToResetMessagesReceived = 0;

		bool DoingShit = false;
		float bufferTime;

		public class MessageCache
        {
			public int index;
			public string shipName;
			public string captain;
		}

		public class Spawnables
        {
			public string shipName;
			public string captain;
        }

		[SerializeField]
		private string _channelToConnectTo = "irishjohngames";

		private Client _client;
		private PubSub _pubSub;
		private Api _api;

		Secrets auth;

		private void Start()
		{
			Application.runInBackground = true;

			auth = new Secrets();

			ConnectionCredentials credentials = new ConnectionCredentials("irishjerngaming", auth.bot_access_token);

			_client = new Client();
			_client.Initialize(credentials, _channelToConnectTo);
			_client.OnConnected += OnConnected;
			_client.OnJoinedChannel += OnJoinedChannel;
			_client.OnMessageReceived += OnMessageReceived;
			_client.OnChatCommandReceived += OnChatCommandReceived;
			_client.Connect();

			_pubSub = new PubSub();
			// _pubSub.OnWhisper += OnWhisper;
			_pubSub.OnPubSubServiceConnected += OnPubSubServiceConnected;
			_pubSub.OnListenResponse += OnListenResponse;
			_pubSub.OnChannelPointsRewardRedeemed += OnChannelPointsReceived;
			_pubSub.Connect();

			_api = new Api();
			_api.Settings.ClientId = "FETCH THIS FROM SOMEWHERE...";
		}

		private void OnPubSubServiceConnected(object sender, System.EventArgs e)
		{
			// _pubSub.ListenToWhispers(auth.john_id);
			_pubSub.ListenToChannelPoints(auth.john_id);

			_pubSub.SendTopics(auth.oauth_redemption);
		}

		private void OnWhisper(object sender, OnWhisperArgs e) => Debug.Log($"{e.Whisper.Data}");

		private void OnChannelPointsReceived(object sender, OnChannelPointsRewardRedeemedArgs e)
		{
			if (e.RewardRedeemed.Redemption.Reward.Title == "TwitchGameTest")
			{
				Debug.Log($"StartCrew for player: {e.RewardRedeemed.Redemption.User.DisplayName}. ShipName: {e.RewardRedeemed.Redemption.Reward}");
				ToSpawn.Add(new Spawnables()
				{
					shipName = e.RewardRedeemed.Redemption.Reward.Prompt,
					captain = e.RewardRedeemed.Redemption.User.DisplayName
				});
				// PlayerManager.Instance.Spawn("Hello", "there");//();
			}
		private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
		{
			Debug.Log("Redemption!!!: " + e.Message + " " + e.RewardTitle);
		}
		private void OnChannelPointsReceived(object sender, TwitchLib.PubSub.Events.OnChannelPointsRewardRedeemedArgs e)
		{
			Debug.Log("Redemption: " + e.RewardRedeemed.Redemption.Reward.Title + " " + e.RewardRedeemed.Redemption.User.DisplayName + " " + e.RewardRedeemed.Redemption.Status);
		
		}

		/// <summary>
		/// Coroutine to Fetch twitch user profile image
		/// </summary>
		/// <param name="userLogin">twitch username</param>
		/// <param name="callback">callback for when the image is ready, wont be called if the requests fail</param>
		public IEnumerator GetUserProfileIcon(string userLogin, Action<Sprite> callback)
		{
			//not huge fan of this flow but it was in the twitch lib examples ¯\_(?)_/¯
			Users getUsersResponse = null;

			//helix requires access tokens in the header... cba, using kraken for now, even if its deprecated
			yield return _api.InvokeAsync(_api.V5.Users.GetUserByNameAsync(userLogin),
				((response) => { getUsersResponse = response; })

			);

			var users = getUsersResponse.Matches;
			//for (int i = 0; i < response.Users.Length; i++)
			if (users.Length > 0)
			{
				var user = users[0];//.Users[0];
				var imageUrl = user.Logo;//.ProfileImageUrl;

				var www = UnityWebRequestTexture.GetTexture(imageUrl);
				yield return www.SendWebRequest();

				if (www.result == UnityWebRequest.Result.Success)
				{
					var texture = DownloadHandlerTexture.GetContent(www);

					var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
					callback(sprite);
				}
			}
		}

		private void OnListenResponse(object sender, OnListenResponseArgs e)
		{
			if (e.Successful) Debug.Log("Listening"); // Debug.Log($"Successfully verified listening to topic: {e.Topic}");
			else Debug.Log($"Failed to listen! Error: {e.Response.Error}");
		}

		private void OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
		{
			Debug.Log($"The bot {e.BotUsername} succesfully connected to Twitch.");
			if (!string.IsNullOrWhiteSpace(e.AutoJoinChannel))
				Debug.Log($"The bot will now attempt to automatically join the channel provided when the Initialize method was called: {e.AutoJoinChannel}");
		}

		private void OnJoinedChannel(object sender, TwitchLib.Client.Events.OnJoinedChannelArgs e) =>
			_client.SendMessage(e.Channel, "Yarrr! It be time for the slaughtarrr!");

		private void OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
		{
			MessagesReceivedIRC.Add(new MessageCache()
			{
				index = MessagesReceivedIRC.Count,
				captain = e.ChatMessage.Username,
				shipName = e.ChatMessage.Message
			});

			bufferTime = Time.time + BUFFER_TIME_INCREMENT;
			// Debug.Log($"Message received from {e.ChatMessage.Username}: {e.ChatMessage.Message} : {e.ChatMessage.TmiSentTs}");
		}

		private void OnChatCommandReceived(object sender, TwitchLib.Client.Events.OnChatCommandReceivedArgs e)
		{
			switch (e.Command.CommandText)
			{
				//case "hello":
				//case "ahoy":
				//	_client.SendMessage(e.Command.ChatMessage.Channel, $"Ahoy {e.Command.ChatMessage.DisplayName}!");
				//	//example of how to spawn a player 
				//	PlayerManager.Instance.Spawn(e.Command.CommandText, e.Command.ChatMessage.DisplayName);
				//	break;
				case "about":
					_client.SendMessage(e.Command.ChatMessage.Channel, "I be a Twitch bot running on the TwitchLib vessel!");
					break;
					//default:
					//	_client.SendMessage(e.Command.ChatMessage.Channel, $"Unknown chat command: {e.Command.CommandIdentifier}{e.Command.CommandText}");
					//	break;
			}
		}

		
		private void FixedUpdate()
        {
			if(MessagesReceivedIRC.Any() && ToSpawn.Any() & !DoingShit && Time.time > bufferTime)
            {
				DoingShit = true;
				foreach (Spawnables s in ToSpawn)
				{
					s.shipName = MessagesReceivedIRC.OrderBy(o => o.index).LastOrDefault(o => o.captain.ToLower() == s.captain.ToLower())?.shipName;
					PlayerManager.Instance.Spawn(s.shipName, s.captain);
				}

				ToSpawn = new ConcurrentBag<Spawnables>();
				DoingShit = false;
            }

			if(MessagesReceivedIRC.Count > 1000) MessagesReceivedIRC = new List<MessageCache>();
		}
    }
}