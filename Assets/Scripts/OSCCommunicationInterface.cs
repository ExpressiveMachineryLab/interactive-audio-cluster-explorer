using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;

public class OSCCommunicationInterface : MonoBehaviour
{
    private OSCReceiver receiver;
    private OSCTransmitter transmitter;

    private VisualizeRecommendations visualizer;

    public int localPort = 5005;
    public int remotePort = 5006;
    public string remoteAddress = "127.0.0.1";
    public string samplesFilter = "/samples";
    public string clearFilter = "/clear";
    public string exitFilter = "/exit";
    public string modeFilter = "/mode";
    public string recommendationsFilter = "/recommendations";
    public string selectedSamplesFilter = "/selectedsamples";

    //samples, clear, shutdown, recommendations, selectedsamples

    // Use this for initialization
    void Start()
    {
        receiver = gameObject.AddComponent<OSCReceiver>();
        receiver.LocalPort = localPort;
        receiver.Bind(recommendationsFilter, HandleRecommendations);
        receiver.Bind(selectedSamplesFilter, HandleSelectedSamples);

        transmitter = gameObject.AddComponent<OSCTransmitter>();
        transmitter.RemoteHost = remoteAddress;
        transmitter.RemotePort = remotePort;

        visualizer = gameObject.GetComponent<VisualizeRecommendations>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HandleRecommendations(OSCMessage recommendationsMessage)
    {
        Debug.Log("HandleRecommendations: " + recommendationsMessage);

        //OSCValue[] values = recommendationsMessage.GetValues();
        //Debug.Log("Message has " + values.Length + " values.");

        //List<OSCValue> arrayValue = values[0].ArrayValue;
        List<OSCValue> arrayValue = recommendationsMessage.Values;
        Debug.Log("Message has " + arrayValue.Count + " values.");
        string[] recommendations = new string[arrayValue.Count];
        for (int index = 0; index < arrayValue.Count; index++)
        {
            recommendations[index] = arrayValue[index].StringValue;
            Debug.Log("Recommendation: " + arrayValue[index].StringValue);
        }

        visualizer.VisualizeRecommendation(recommendations);
    }

    public void HandleSelectedSamples(OSCMessage selectedSamplesMessage)
    {
        Debug.Log("HandleSelectedSamples: " + selectedSamplesMessage);
    }

    public void SendSamples(List<Point> samplePoints, string separatorString)
    {
        OSCValue[] messageStrings = new OSCValue[samplePoints.Count];
        for (int index = 0; index < samplePoints.Count; index++)
        {
            messageStrings[index] = OSCValue.String(samplePoints[index].audiokey);
        }

        OSCMessage message = new OSCMessage(samplesFilter);
        message.AddValue(OSCValue.Array(messageStrings));

        Debug.Log("Sent: " + message.ToString());

        transmitter.Send(message);
    }

    public void SendClear()
    {
        OSCMessage message = new OSCMessage(clearFilter);
        message.AddValue(OSCValue.String("clear"));

        Debug.Log("Sent: " + message.ToString());

        transmitter.Send(message);
    }

    public void SendExit()
    {
        OSCMessage message = new OSCMessage(exitFilter);
        message.AddValue(OSCValue.String("exit"));

        Debug.Log("Sent: " + message.ToString());

        transmitter.Send(message);
    }

    public void SendMode()
    {
        OSCMessage message = new OSCMessage(modeFilter);
        message.AddValue(OSCValue.String("mode"));

        Debug.Log("Sent: " + message.ToString());

        transmitter.Send(message);
    }
}
