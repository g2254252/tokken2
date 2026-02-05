using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using System;

public class MQTTSender : MonoBehaviour
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

    void Start()
    {
        client = new MqttClient(brokerAddress);
        client.Connect(Guid.NewGuid().ToString());
        Debug.Log("MQTT Connected (Sender)");
    }

    void Update()
    {
        SendTransforms();
    }

    void SendTransforms()
    {
        SyncData data = new SyncData();
        data.objects = new ObjectTransform[]
        {
            CreateData("Main Camera", mainCamera),
            CreateData("Earth", earth),
            CreateData("Moon", moon),
            CreateData("MoonOrbit", moonOrbit)
        };

        string json = JsonUtility.ToJson(data);
        client.Publish(topic, Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
    }

    ObjectTransform CreateData(string name, Transform t)
    {
        return new ObjectTransform
        {
            name = name,
            px = t.position.x,
            py = t.position.y,
            pz = t.position.z,
            rx = t.rotation.x,
            ry = t.rotation.y,
            rz = t.rotation.z,
            rw = t.rotation.w
        };
    }
}
