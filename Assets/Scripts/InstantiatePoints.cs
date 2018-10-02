using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstantiatePoints : MonoBehaviour
{
    private List<string> filenames;
    private List<string> names;
    private List<Vector3> coordinates;
    private System.Random random;
    private string streamingAssetsPath;
    private GameObject setObject;
    private GameObject panelObject;
    private GameObject camObject;
    private Camera cam;
    private GameObject refSpriteObject;
    private Sprite referenceSprite;
    private GameObject audioSourceObject;
    //private AudioClip sound;
    private AudioSource bufferSource;
    private AudioSource playingSource;
    private List<GameObject> points;
    private int coordinatesPathIndex;

    private string filenamesPath = "sounds/filenames.txt";
    private string namesPath = "sounds/name.tsv";
    private string coordinatesPath;
    public string[] coordinatesPaths;

    public int coordinateScaleFactor = 500;
    public float radiusScaleFactor = 0.5f;
    public float centroidRadiusScaleFactor = 1f;

    public int numberOfDataPointsLoadedPerFrame = 10;

    private Vector3 centroid;

    // Use this for initialization
    void Start()
    {
        random = new System.Random();
        streamingAssetsPath = Application.streamingAssetsPath;
        points = new List<GameObject>();

        coordinatesPaths = Directory.GetFiles(streamingAssetsPath + "/sounds/tsne/", "*.3d.tsv");
        coordinatesPathIndex = 0;

        setObject = GameObject.Find("Set");
        panelObject = GameObject.Find("Panel");
        camObject = GameObject.Find("Cam");
        cam = camObject.GetComponent<Camera>();
        audioSourceObject = GameObject.Find("Sound");
        //sound = audioClipObject.AddComponent<AudioClip>();
        bufferSource = audioSourceObject.GetComponents<AudioSource>()[0];
        playingSource = audioSourceObject.GetComponents<AudioSource>()[1];
        refSpriteObject = GameObject.Find("ReferenceSprite");
        referenceSprite = refSpriteObject.GetComponent<Sprite>();
        filenames = LoadString(streamingAssetsPath + "/" + filenamesPath);
        Debug.Log(filenames[random.Next(filenames.Count)]);
        names = LoadString(streamingAssetsPath + "/" + namesPath);
        Debug.Log(names[random.Next(names.Count)]);

        //centroid = CalculateCentroid(coordinates);
        //GameObject centroidObject = UnityEngine.Object.Instantiate(refSpriteObject);
        //centroidObject.name = "Centroid";
        //centroidObject.transform.localScale = new Vector3(centroidRadiusScaleFactor, centroidRadiusScaleFactor, centroidRadiusScaleFactor);
        //centroidObject.transform.position = cam.ViewportToWorldPoint(new Vector3(centroid.x, centroid.y, 10));
        ////centroidObject.transform.position = cam.ViewportToWorldPoint(new Vector3(centroid.x, centroid.y, 10)) + cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        //centroidObject.GetComponent<Image>().color = Color.HSVToRGB(centroid.z, 1, 1);
        //centroidObject.transform.SetParent(panelObject.transform);

        //camObject.transform.LookAt(centroidObject.transform);
        InitPoints();
    }

    public void InitPoints()
    {
        refSpriteObject.SetActive(true);
        coordinatesPath = coordinatesPaths[coordinatesPathIndex];
        coordinates = LoadVector3(coordinatesPath);
        Debug.Log(coordinates[random.Next(coordinates.Count)]);
        StartCoroutine(InstantiateDataPoints());
    }

    public void ResetPoints()
    {
        while(points.Count > 0)
        {
            GameObject point = points[0];
            points.RemoveAt(0);
            Destroy(point);
        }

        InitPoints();
    }

    private Vector3 CalculateCentroid(List<Vector3> coords)
    {
        Vector3 ctroid = new Vector3();
        foreach(Vector3 coord in coords)
        {
            ctroid += coord;
        }
        ctroid /= coords.Count;

        return ctroid;
    }

    // Update is called once per frame
    private void Update()
    {
        if(Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Clicked mouse");
            //source = MouseInteraction.source;
            if(bufferSource.clip != null)
            {
                if(playingSource.isPlaying)
                {
                    playingSource.Stop();
                }
                playingSource.clip = bufferSource.clip;
                playingSource.Play();
                //bufferSource.PlayOneShot(bufferSource.clip, 1);
                Debug.Log("Played source");
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Clicked mouse");
            //source = MouseInteraction.source;
            if (bufferSource.clip != null)
            {
                playingSource.PlayOneShot(bufferSource.clip, 1);
                Debug.Log("Played source");
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            coordinatesPathIndex = (coordinatesPathIndex < coordinatesPaths.Length - 1) ? coordinatesPathIndex + 1 : 0;
            StopAllCoroutines();
            ResetPoints();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            coordinatesPathIndex = (coordinatesPathIndex > 0) ? coordinatesPathIndex - 1 : coordinatesPaths.Length - 1;
            StopAllCoroutines();
            ResetPoints();
        }
    }

    private IEnumerator<List<Vector3>> InstantiateDataPoints()
    {
        //MouseInteraction.source = source;
        MouseInteraction.sourceObject = audioSourceObject;
        for (int i = 0; i < coordinates.Count; i++)
        {
            Vector3 coordinate = coordinates[i];
            GameObject point = UnityEngine.Object.Instantiate(refSpriteObject);
            point.name = "point" + i;
            point.transform.localScale = new Vector3(radiusScaleFactor, radiusScaleFactor, radiusScaleFactor);
            point.transform.position = cam.ViewportToWorldPoint(new Vector3(coordinate.x, coordinate.y, 10));
            point.GetComponent<Image>().color = Color.HSVToRGB(coordinate.z, 1, 1);
            point.AddComponent<MouseInteraction>();
            MouseInteraction interaction = point.GetComponent<MouseInteraction>();
            interaction.filename = streamingAssetsPath + "/" + filenames[i];
            //interaction.filename = filenames[i];
            point.transform.SetParent(panelObject.transform);

            points.Add(point);

            if(i % numberOfDataPointsLoadedPerFrame == 0)
            {
                yield return null;
            }
        }

        refSpriteObject.SetActive(false);
        Debug.Log("Done displaying points.");
    }

    private List<string> LoadString (string fileName)
    {
        // Handle any problems that might arise when reading the text
        try
        {
            List<string> list = new List<string>();
            string line;
            // Create a new StreamReader, tell it which file to read and what encoding the file
            // was saved as
            StreamReader reader = new StreamReader(fileName, Encoding.Default);
            // Immediately clean up the reader after this block of code is done.
            // You generally use the "using" statement for potentially memory-intensive objects
            // instead of relying on garbage collection.
            // (Do not confuse this with the using directive for namespace at the 
            // beginning of a class!)
            using (reader)
            {
                // While there's lines left in the text file, do this:
                do
                {
                    line = reader.ReadLine();

                    if (line != null)
                    {
                        list.Add(line);
                    }
                }
                while (line != null);
                // Done reading, close the reader and return true to broadcast success    
                reader.Close();
                return list;
            }
        }
        // If anything broke in the try block, we throw an exception with information
        // on what didn't work
        catch (Exception e)
        {
            Console.WriteLine("{0}\n", e.Message);
            return null;
        }
    }

    private List<Vector3> LoadVector3(string fileName)
    {
        // Handle any problems that might arise when reading the text
        try
        {
            List<Vector3> list = new List<Vector3>();
            string line;
            // Create a new StreamReader, tell it which file to read and what encoding the file
            // was saved as
            StreamReader reader = new StreamReader(fileName, Encoding.Default);
            // Immediately clean up the reader after this block of code is done.
            // You generally use the "using" statement for potentially memory-intensive objects
            // instead of relying on garbage collection.
            // (Do not confuse this with the using directive for namespace at the 
            // beginning of a class!)
            using (reader)
            {
                // While there's lines left in the text file, do this:
                do
                {
                    line = reader.ReadLine();

                    if (line != null)
                    {
                        string[] entries = line.Split('\t');
                        list.Add(new Vector3(Single.Parse(entries[0]), Single.Parse(entries[1]), Single.Parse(entries[2])));
                    }
                }
                while (line != null);
                // Done reading, close the reader and return true to broadcast success    
                reader.Close();
                return list;
            }
        }
        // If anything broke in the try block, we throw an exception with information
        // on what didn't work
        catch (Exception e)
        {
            Console.WriteLine("{0}\n", e.Message);
            return null;
        }
    }
}