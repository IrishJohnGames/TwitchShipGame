using TwitchLib.Client.Models;
using TwitchLib.PubSub.Events;
using TwitchLib.Unity;
using UnityEngine;

namespace CoreTwitchLibSetup
{
	public class TwitchLibCtrl : MonoBehaviour
	{
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

		private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
		{
			Debug.Log("Redemption!!!: " + e.Message + " " + e.RewardTitle);
		}
		private void OnChannelPointsReceived(object sender, TwitchLib.PubSub.Events.OnChannelPointsRewardRedeemedArgs e)
		{
			Debug.Log("Redemption: " + e.RewardRedeemed.Redemption.Reward.Title + " " + e.RewardRedeemed.Redemption.User.DisplayName + " " + e.RewardRedeemed.Redemption.Status);
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

		private void OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e) =>
			Debug.Log($"Message received from {e.ChatMessage.Username}: {e.ChatMessage.Message}");

		private void OnChatCommandReceived(object sender, TwitchLib.Client.Events.OnChatCommandReceivedArgs e)
		{
			switch (e.Command.CommandText)
			{
				case "hello":
				case "ahoy":
					_client.SendMessage(e.Command.ChatMessage.Channel, $"Ahoy {e.Command.ChatMessage.DisplayName}!");
					break;
				case "about":
					_client.SendMessage(e.Command.ChatMessage.Channel, "I be a Twitch bot running on the TwitchLib vessel!");
					break;
					//default:
					//	_client.SendMessage(e.Command.ChatMessage.Channel, $"Unknown chat command: {e.Command.CommandIdentifier}{e.Command.CommandText}");
					//	break;
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
				_client.SendMessage(_channelToConnectTo, "I pressed the space key within Unity.");
		}
	}
}