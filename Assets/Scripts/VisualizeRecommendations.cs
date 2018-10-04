using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualizeRecommendations : MonoBehaviour
{
    public string recommendationFilePath = "/sounds/recommendedSamples.json";
    private string streamingAssetsPath;

    private bool isRecommenderVizActive;
    private bool isVizInitialized;
    private List<Point> pointsList;
    private Dictionary<string, Point> pointsDictionary;
    private List<Point> selectedPointsList;


    private Queue<string[]> recommendationQueue;

    /// <summary>
    /// Start this instance.
    /// </summary>
    public void Start()
    {
        isRecommenderVizActive = false;
        isVizInitialized = false;
        streamingAssetsPath = Application.streamingAssetsPath;
        pointsList = new List<Point>();
        pointsDictionary = new Dictionary<string, Point>();
        selectedPointsList = new List<Point>();
        recommendationQueue = new Queue<string[]>();

        StartCoroutine(ReadRecommendationFile());
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    public void Update()
    {
        if (isRecommenderVizActive && isVizInitialized)
        {
            ProcessQueue();
        }
    }

    public void AddPoints(List<Point> points)
    {
        pointsList = points;
        foreach(Point point in pointsList)
        {
            if(!pointsDictionary.ContainsKey(point.name))
            {
                pointsDictionary.Add(point.name, point);
            }
        }
        isVizInitialized = true;
    }

    public void ClearPoints()
    {
        pointsList.Clear();
        pointsDictionary.Clear();
        DeselectPoints();
        selectedPointsList.Clear();
        isVizInitialized = false;
    }

    private IEnumerator<UnityEngine.WaitForSeconds> ReadRecommendationFile()
    {
        List<string> lines;
        string[] recommendations;
        string line;
        while (true)
        {
            if(isRecommenderVizActive && isVizInitialized)
            {
                try
                {
                    //Read something.
                    lines = FileIO.LoadStringList(streamingAssetsPath + recommendationFilePath);
                    if (lines != null && lines.Count > 0)
                    {
                        line = lines[0];
                        Debug.Log("Read: " + line);
                        //recommendations = JsonUtility.FromJson<string[]>(line);
                        recommendations = line.Substring(1, line.Length - 2).Replace("\"", "").Split(new string[] {", "}, StringSplitOptions.RemoveEmptyEntries);
                        Debug.Log("String Array Length: " + recommendations.Length);
                        foreach(string recommendation in recommendations)
                        {
                            
                            Debug.Log("Recommended: " + recommendation);
                        }
                        recommendationQueue.Enqueue(recommendations);
                        File.Create(streamingAssetsPath + recommendationFilePath).Dispose();
                    }
                    else
                    {
                        //Nothing is available to read.
                    }
                }
                catch(Exception e)
                {
                    Debug.Log("Exception during File read or write: " + e.Message);
                }
            }

            yield return new WaitForSeconds(.1f);
        }
    }

    public void ProcessQueue()
    {
        if(recommendationQueue.Count > 0)
        {
            VisualizeRecommendation(recommendationQueue.Dequeue());
        }
    }

    public void VisualizeRecommendation(string[] sampleNames)
    {
        Debug.Log("String Count: " + sampleNames.Length);
        DeselectPoints();
        selectedPointsList.Clear();
        int count = 0;
        foreach (string sample in sampleNames)
        {
            Debug.Log("Sample: " + sample);
            if(pointsDictionary.ContainsKey(sample))
            {
                Point point = pointsDictionary[sample];
                selectedPointsList.Add(point);
                count++;
            }
        }
        Debug.Log("Loaded Count: " + count);
        SelectPoints();
    }

    public void SelectPoints()
    {
        foreach (Point point in selectedPointsList)
        {
            point.pointObject.GetComponent<Image>().color = Color.HSVToRGB(point.coordinate.z, 1f, 1f);
        }
    }

    public void DeselectPoints()
    {
        foreach(Point point in selectedPointsList)
        {
            point.pointObject.GetComponent<Image>().color = Color.HSVToRGB(point.coordinate.z, 0.25f, 0.25f);
        }
    }

    public void SelectAllPoints()
    {
        foreach (Point point in pointsList)
        {
            point.pointObject.GetComponent<Image>().color = Color.HSVToRGB(point.coordinate.z, 1f, 1f);
        }
    }

    public void DeselectAllPoints()
    {
        foreach (Point point in pointsList)
        {
            point.pointObject.GetComponent<Image>().color = Color.HSVToRGB(point.coordinate.z, 0.25f, 0.25f);
        }
    }

    public void WriteSelectedSampleFile()
    {
        //TODO: Write Selected Samples to File.
    }

    public void ActivateVisualizer()
    {
        isRecommenderVizActive = true;
        DeselectAllPoints();
    }

    public void DeactivateVisualizer()
    {
        isRecommenderVizActive = false;
        SelectAllPoints();
    }

    public void ToggleVisualizerActivation()
    {
        if(isRecommenderVizActive)
        {
            DeactivateVisualizer();
        }
        else
        {
            ActivateVisualizer();
        }
    }
}
