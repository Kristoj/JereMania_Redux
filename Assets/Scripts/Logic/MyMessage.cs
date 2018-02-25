using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class MyMessage : MessageBase {
	public NetworkInstanceId playerId;
	public NetworkInstanceId objectId;
	public string message;
}