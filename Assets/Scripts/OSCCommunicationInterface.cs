using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;

public class OSCCommunicationInterface : MonoBehaviour
{
    private OSCReceiver receiver;
    private OSCTransmitter transmitter;

    public int localPort = 5005;
    public int remotePort = 5006;
    public string remoteAddress = "127.0.0.1";
    public string samplesFilter = "/samples";
    public string clearFilter = "/clear";
    public string exitFilter = "/exit";
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
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HandleRecommendations(OSCMessage recommendationsMessage)
    {
        Debug.Log("HandleRecommendations: " + recommendationsMessage.ToString());
    }

    public void HandleSelectedSamples(OSCMessage selectedSamplesMessage)
    {
        Debug.Log("HandleSelectedSamples: " + selectedSamplesMessage.ToString());
    }

    public void SendSamples(List<Point> samplePoints, string separatorString)
    {
        string messageString = "";
        foreach(Point point in samplePoints)
        {
            messageString += point.name + separatorString;
        }
        messageString = messageString.Substring(0, messageString.Length - separatorString.Length);
        OSCMessage message = new OSCMessage(samplesFilter);
        message.AddValue(OSCValue.String(messageString));

        transmitter.Send(message);
    }

    public void SendClear()
    {
        OSCMessage message = new OSCMessage(clearFilter);
        message.AddValue(OSCValue.String("clear"));

        transmitter.Send(message);
    }

    public void SendExit()
    {
        OSCMessage message = new OSCMessage(exitFilter);
        message.AddValue(OSCValue.String("exit"));

        transmitter.Send(message);
    }
}
