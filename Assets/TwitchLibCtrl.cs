using TwitchLib.Client.Models;
using TwitchLib.PubSub.Events;
using TwitchLib.Unity;
using UnityEngine;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;

namespace CoreTwitchLibSetup
{
	public class TwitchLibCtrl : MonoBehaviour
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