using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMessage
{
    public OpCode Code { get; set; }

    public virtual void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }

    public virtual void Deserialize(ref DataStreamReader reader)
    {
        // Code = (OpCode)reader.ReadByte();
    }

    public virtual void ReceivedOnClient()
    {
        Debug.Log("Received message on client: " + Code);
    }

    public virtual void ReceivedOnServer(NetworkConnection connection)
    {
        Debug.Log("Received message on server: " + Code);
    }
}