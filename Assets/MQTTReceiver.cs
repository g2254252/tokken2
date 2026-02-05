using System;
using System.Text;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class MQTTReceiver : MonoBehaviour
{
    public string brokerAddress = "test.mosquitto.org";
    public int brokerPort = 1883;
    public string topic = "unity/solarSync";

    public Transform mainCamera;
    public Transform earth;
    public Transform moon;
    public Transform moonOrbit;

    private MqttClient client;

    [Serializable]
    public class ObjectTransform
    {
        public string name;
        public float px, py, pz;
        public float rx, ry, rz, rw;
    }

    [Serializable]
    public class SyncData
    {
        public ObjectTransform[] objects;
    }

    private SyncData latestData;
    private bool newMessage = false;

    void Start()
    {
        client = new MqttClient(brokerAddress);
        client.MqttMsgPublishReceived += OnMessageReceived;
        client.Connect(Guid.NewGuid().ToString());
        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

        Debug.Log("MQTT Connected (Receiver)");
    }

    void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string json = Encoding.UTF8.GetString(e.Message);
        latestData = JsonUtility.FromJson<SyncData>(json);
        newMessage = true;
    }

    void Update()
    {
        if (!newMessage || latestData == null) return;

        foreach (var obj in latestData.objects)
        {
            ApplyTransform(obj);
        }

        newMessage = false;
    }

    void ApplyTransform(ObjectTransform obj)
    {
        Transform target = null;

        switch (obj.name)
        {
            case "Main Camera": target = mainCamera; break;
            case "Earth": target = earth; break;
            case "Moon": target = moon; break;
            case "MoonOrbit": target = moonOrbit; break;
        }

        if (target != null)
        {
            target.position = new Vector3(obj.px, obj.py, obj.pz);
            target.rotation = new Quaternion(obj.rx, obj.ry, obj.rz, obj.rw);
        }
    }
}

