using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstantiatePoints : MonoBehaviour
{
    private VisualizeRecommendations visualizer;
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
    private AudioSource bufferSource;
    private AudioSource playingSource;
    private List<Point> points;
    private int coordinatesPathIndex;
    private Point currentPoint;

    public string filenamesPath = "/sounds/filenames.txt";
    public string namesPath = "/sounds/name.tsv";
    public string tsnePath = "/sounds/tsne/";
    public string tsneFileNameFilter = "*.3d.tsv";
    private string coordinatesPath;
    private string[] coordinatesPaths;

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

        visualizer = gameObject.GetComponent<VisualizeRecommendations>();

        points = new List<Point>();

        coordinatesPaths = Directory.GetFiles(streamingAssetsPath + tsnePath, tsneFileNameFilter);
        coordinatesPathIndex = 0;

        setObject = GameObject.Find("Set");
        panelObject = GameObject.Find("Panel");
        camObject = GameObject.Find("Cam");
        cam = camObject.GetComponent<Camera>();
        audioSourceObject = GameObject.Find("Sound");
        bufferSource = audioSourceObject.GetComponents<AudioSource>()[0];
        playingSource = audioSourceObject.GetComponents<AudioSource>()[1];
        refSpriteObject = GameObject.Find("ReferenceSprite");
        referenceSprite = refSpriteObject.GetComponent<Sprite>();
        filenames = FileIO.LoadStringList(streamingAssetsPath + filenamesPath);
        Debug.Log(filenames[random.Next(filenames.Count)]);
        names = FileIO.LoadStringList(streamingAssetsPath + namesPath);
        Debug.Log(names[random.Next(names.Count)]);

        InitPoints();
    }

    public void InitPoints()
    {
        refSpriteObject.SetActive(true);
        coordinatesPath = coordinatesPaths[coordinatesPathIndex];
        coordinates = FileIO.LoadVector3List(coordinatesPath);
        Debug.Log(coordinates[random.Next(coordinates.Count)]);
        StartCoroutine(InstantiateDataPoints());
    }

    public void ResetPoints()
    {
        while(points.Count > 0)
        {
            GameObject point = points[0].pointObject;
            points.RemoveAt(0);
            Destroy(point);
        }

        visualizer.ClearPoints();

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
            Debug.Log("Clicked left mouse button");

            if (bufferSource.clip != null)
            {
                if (playingSource.isPlaying)
                {
                    playingSource.Stop();
                }
                playingSource.clip = bufferSource.clip;
                playingSource.Play();
                Debug.Log("Played source");
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Clicked right mouse button");

            if (visualizer.IsVizActive())
            {
                visualizer.AddCuedRecommenderPoint(currentPoint);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if(visualizer.IsVizActive())
            {
                visualizer.SendRecommendations();
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

        if (Input.GetKeyDown(KeyCode.V))
        {
            visualizer.ToggleVisualizerActivation();
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(visualizer.IsVizActive())
            {
                visualizer.ClearRecommendations();
            }
        }
    }

    private IEnumerator<List<Vector3>> InstantiateDataPoints()
    {
        MouseInteraction.sourceObject = audioSourceObject;
        for (int i = 0; i < coordinates.Count; i++)
        {
            Point point = new Point();
            point.coordinate = coordinates[i];
            point.filename = streamingAssetsPath + "/" + filenames[i];
            point.name = names[i];
            point.pointObject = UnityEngine.Object.Instantiate(refSpriteObject);
            point.pointObject.name = "point" + i;
            point.pointObject.transform.localScale = new Vector3(radiusScaleFactor, radiusScaleFactor, radiusScaleFactor);
            point.pointObject.transform.position = cam.ViewportToWorldPoint(new Vector3(point.coordinate.x, point.coordinate.y, 10));
            point.pointObject.GetComponent<Image>().color = Color.HSVToRGB(point.coordinate.z, 1f, 1f);
            point.pointObject.AddComponent<MouseInteraction>();
            point.pointObject.GetComponent<MouseInteraction>().point = point;
            point.pointObject.GetComponent<MouseInteraction>().instantiatePointsComponent = this;
            point.pointObject.transform.SetParent(panelObject.transform);
            //Debug.Log(point.name);
            points.Add(point);

            if(i % numberOfDataPointsLoadedPerFrame == 0)
            {
                yield return null;
            }
        }

        refSpriteObject.SetActive(false);
        Debug.Log("Done displaying points.");

        visualizer.AddPoints(points);
    }

    public void SetCurrentPoint(Point point)
    {
        this.currentPoint = point;
    }
}