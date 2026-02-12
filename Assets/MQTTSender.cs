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
        public float timestamp;
    }

    [Serializable]
    public class SyncData
    {
        public ObjectTransform[] objects;
    }

    void Start()
    {
        client = new MqttClient(brokerAddress);
        // ★追加：生徒から戻ってきたデータを受け取って計算する
        client.MqttMsgPublishReceived += (sender, e) => {
            try {
                string json = Encoding.UTF8.GetString(e.Message);
                SyncData echoed = JsonUtility.FromJson<SyncData>(json);
            if (echoed.objects != null && echoed.objects.Length > 0) {
                // 現在時刻 - 送信時の時刻 = 往復時間(RTT)
                float rtt = Time.time - echoed.objects[0].timestamp;
                // 片道時間はその半分として計算
                Debug.Log($"[計測データ] 反映時間: {(rtt / 2f) * 1000f:F1} ms");
            }
        } catch { /* 解析エラーは無視 */ }
    };

        client.Connect(Guid.NewGuid().ToString());

        // ★追加：返信用トピック(topic + "/echo")を購読
        client.Subscribe(new string[] { topic + "/echo" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

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
            rw = t.rotation.w,
            timestamp = Time.time
        };
    }
}
