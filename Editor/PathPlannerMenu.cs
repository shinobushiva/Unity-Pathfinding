using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class PathPlannerMenu : Editor
{
    public static LinkObject linkPrefab;
    public static NodeObject nodePrefab;
  
    static void Setup ()
    {
        if (nodePrefab == null) {
            nodePrefab = (NodeObject)AssetDatabase.LoadAssetAtPath ("Assets/PathFinding/Prefabs/NodePrefab.prefab", typeof(NodeObject));
            Debug.Log ("Load Node Prefab : " + nodePrefab);
        }
        if (linkPrefab == null) {
            linkPrefab = (LinkObject)AssetDatabase.LoadAssetAtPath ("Assets/PathFinding/Prefabs/LinkPrefab.prefab", typeof(LinkObject));
            Debug.Log ("Load Link Prefab : " + linkPrefab);
        }
    
    }
  
    [MenuItem("PathPlanner/Adjust")]
    static void Adjust ()
    {
        Setup ();
    
        NodeObject[] ns = FindObjectsOfType (typeof(NodeObject)) as NodeObject[];
        foreach (NodeObject n in ns) {
            n.Adjust ();
        }
    
        LinkObject[] ls = FindObjectsOfType (typeof(LinkObject)) as LinkObject[];
        foreach (LinkObject l in ls) {
            l.Adjust ();
        }
    }
  
    [MenuItem("PathPlanner/CreateNode")]
    static void CreateNode ()
    {
        Setup ();

        Undo.RegisterSceneUndo ("Add NodeObject Prefab");
        NodeObject g = (NodeObject)PrefabUtility.InstantiatePrefab (nodePrefab);
        g.transform.position = Vector3.zero;
        g.name = "Change This Name";

      
    }
  
    [MenuItem("PathPlanner/MakeLink")]
    static void MakeLink ()
    {
        Setup ();
    
        GameObject[] gs = Selection.gameObjects;
        if (gs.Length != 2) {
            Debug.Log ("Select Two NodeObjects");
            return;
        }
    
        if (gs [0].GetComponent<NodeObject> () && gs [1].GetComponent<NodeObject> ()) {
            NodeObject n1 = gs [0].GetComponent<NodeObject> ();
            NodeObject n2 = gs [1].GetComponent<NodeObject> ();
            if (n1.id == null || n1.id.Length == 0) {
                n1.id = n1.gameObject.name;
            }
            if (n2.id == null || n2.id.Length == 0) {
                n2.id = n2.gameObject.name;
            }
      
            Vector3 pos1 = n1.transform.position;
            Vector3 pos2 = n2.transform.position;
    
            Undo.RegisterSceneUndo ("Add LinkObject Prefab");
            LinkObject g = (LinkObject)PrefabUtility.InstantiatePrefab (linkPrefab);
            g.transform.position = pos1;
      
            g.head = n1;
            g.tail = n2;
            g.link = new Link (n1.node, n2.node);
            g.link.Id = n1.id + "-" + n2.id;
            g.name = "" + g.link.Id;
            g.Adjust ();

        } else {
            Debug.Log ("Select NodeObjects Only");
        }
    }
  
    private static string OpenFilePanel (string text, string type)
    {
        string openDirectory = EditorPrefs.GetString ("Last Open Directory", "");
    
        string path = EditorUtility.OpenFilePanel (text, openDirectory, type);
        if (path.Length != 0) {
      
            int li = path.LastIndexOf ("/");
            EditorPrefs.SetString ("Last Open Directory", path.Substring (0, li));
        }
        return path;
    }

    [MenuItem("PathPlanner/Load/Node and Link from CSV")]
    static void LoadFromCSV ()
    {
        Setup ();

        if (nodePrefab == null || linkPrefab == null) {
            Debug.Log ("Prefabs were not found. Aborting.");
            return;
        }
    
        float mag = 1000;
    
        GameObject pathObject = new GameObject ("Path from CSV");
		pathObject.AddComponent<HideRendererAtRuntime> ();
        Undo.RegisterSceneUndo ("Add Path from CSV");
    
    
        Dictionary<string, NodeObject> nDict = new Dictionary<string, NodeObject> ();
    
        EditorUtility.DisplayDialog ("Selecting Node CSV File!", "First, please select a Node CSV file.", "OK");
        {
            string path = OpenFilePanel ("Select a Node CSV file", "csv");
            if (path.Length != 0) {
        
                string url = "file://" + path;
        
                WWW www = new WWW (url);
                while (!www.isDone) {
                }
                string txt = www.text;
                Debug.Log (txt);
        
                StringReader r = new StringReader (txt);
                string line = null;
                while ((line = r.ReadLine()) != null) {
                    Debug.Log (line);

                    if (line.Replace (",", "").Trim ().Length == 0)
                        continue;

                    string[] ss = line.Split (',');
                    if (ss.Length < 2) {
                        continue;
                    } 
          
                    int id = ToI (ss [0]);
                    if (ss [0].Trim ().Length == 0 || id == int.MinValue)
                        continue;

                    float x = ToF (ss [1]) / mag;
                    float y = ToF (ss [2]) / mag;
                    float z = ToF (ss [3]) / mag;
          
                    NodeObject g = (NodeObject)PrefabUtility.InstantiatePrefab (nodePrefab);
                    g.name = "NODE_" + id;
                    g.transform.position = new Vector3 (x, z, y);
                    g.transform.parent = pathObject.transform;

                    g.gameObject.AddComponent<SphereCollider> ();
                    g.gameObject.GetComponent<Collider>().isTrigger = true;
                    g.gameObject.AddComponent<SpawningPoint> ();


                    nDict [ss [0].Trim ()] = g;
          
                }
            } 
        }
        foreach (string s in nDict.Keys) {
            Debug.Log (s);
        }
    
        EditorUtility.DisplayDialog ("Selecting a Link CSV file", "Next, please select a Link CSV file.", "OK");
        {
            string path = OpenFilePanel ("Select a Link CSV file", "csv");
            if (path.Length != 0) {
        
                string url = "file://" + path;
                WWW www = new WWW (url);
                while (!www.isDone) {
                }
                string txt = www.text;
                Debug.Log (txt);
        
                StringReader r = new StringReader (txt);
                string line = null;
                int counter = 0;
                LinkObject g = null;
                LinkAttribute la = null;
                string[] prevSS = null;
                while ((line = r.ReadLine()) != null) {
                    Debug.Log (line);
          
                    if (line.Replace (",", "").Trim ().Length == 0)
                        continue;
          
                    string[] ss = line.Split (',');
                    int linkNo = ToI (ss [0]);
                    if (ss [0].Trim ().Length == 0 || linkNo == int.MinValue)
                        continue;

                    if (prevSS != null) {
                        string[] prevLinkIds = prevSS [1].Trim ().Split ('-');
                        string[] curLinkIds = ss [1].Trim ().Split ('-');
                        if (prevLinkIds [0].Equals (curLinkIds [1]) && prevLinkIds [1].Equals (curLinkIds [0])) {
                            for (int i=0; i<ss.Length; i++) {
                                if (ss [i].Trim ().Length == 0) {
                                    ss [i] = prevSS [i];
                                }
                            }
                        }
                    } 


                    //2014.10.22
                    g = (LinkObject)PrefabUtility.InstantiatePrefab (linkPrefab);
                    la = g.gameObject.AddComponent<LinkAttribute> ();
                    la.linkId = ss [1];
                    la.length = ToF (ss [2]);
                    la.minWidth = ToF (ss [3]);
                    la.hasPedestrianRoad = (ToI (ss [4]) == 1);
                    la.numPedstriansWeekday = ToI (ss [5]);
                    la.numPedstriansHoliday = ToI (ss [6]);
                    la.roofCoverage = ToF (ss [7]);
                    la.material = ToI (ss [8]);
                    la.numHoles700 = ToI (ss [9]);
                    la.numHoles1400 = ToI (ss [10]);
                    la.numHolesMax = ToI (ss [11]);
                    la.numGratings700 = ToI (ss [12]);
                    la.numGratings1400 = ToI (ss [13]);
                    la.numGratingsMax = ToI (ss [14]);
                    la.numStuddedPavings = ToI (ss [15]);
                    la.numLights700 = ToI (ss [16]);
                    la.numLights1400 = ToI (ss [17]);
                    la.numLightsMax = ToI (ss [18]);
                    la.numElectricityBoxes700 = ToI (ss [19]);
                    la.numElectricityBoxes1400 = ToI (ss [20]);
                    la.numElectricityBoxesMax = ToI (ss [21]);
                    la.numTrees700 = ToI (ss [22]);
                    la.numTrees1400 = ToI (ss [23]);
                    la.numTreesMax = ToI (ss [24]);
                    la.numMovableObstacles700 = ToI (ss [25]);
                    la.numMovableObstacles1400 = ToI (ss [26]);
                    la.numMovableObstaclesMax = ToI (ss [27]);
                    la.numStaticObstacles700 = ToI (ss [28]);
                    la.numStaticObstacles1400 = ToI (ss [29]);
                    la.numStaticObstaclesMax = ToI (ss [30]);
                    la.numPowerPoles700 = ToI (ss [31]);
                    la.numPowerPoles1400 = ToI (ss [32]);
                    la.numPowerPolesMax = ToI (ss [33]);
                    la.numSignPoles700 = ToI (ss [34]);
                    la.numSignPoles1400 = ToI (ss [35]);
                    la.numSignPolesMax = ToI (ss [36]);
                    la.numUnMaintenanced700 = ToI (ss [37]);
                    la.numUnMaintenanced1400 = ToI (ss [38]);
                    la.numUnMaintenancedMax = ToI (ss [39]);
                    la.goProTime = ToF (ss [40]);

                    la.rightBrachioradialis = ToF (ss [41]);
                    la.rightBicepsBrachii = ToF (ss [42]);
                    la.rightTricepsBrachii = ToF (ss [43]);
                    la.rightDeltoid = ToF (ss [44]);

                    la.leftBrachioradialis = ToF (ss [45]);
                    la.leftBicepsBrachii = ToF (ss [46]);
                    la.leftTricepsBrachii = ToF (ss [47]);
                    la.leftDeltoid = ToF (ss [48]);

                    la.stepUp = (ToI (ss[49])==1);
                    la.stepDown = (ToI (ss[50])==1);

                    la.vibrationRate = ToF (ss[51]);

                    /*
                    la.bicepsBrachii = ToF (ss [41]);
                    la.tricepsBrachii =  ToF (ss [42]);
                    la.deltoidAnterior = ToF (ss [43]);
                    la.deltoidMiddle = ToF (ss [44]);
                    la.deltoidPosterior = ToF (ss [45]);
                    la.brachioradialis = ToF (ss [46]); 

                    la.imageId = ss [44].Trim ();

                    la.crossGrade = (ToI (ss [45]) == 1);
                    la.crossDegree = ToI (ss [46]);
                    la.crossType = ToI (ss [47]);
                    la.inclineDirection = ToI (ss [48]);
                    la.carRode = (ToI (ss [49]) == 1);
                    la.parkingOnStreet = (ToI (ss [50]) == 1);
                    la.vibration = (ToI (ss [51]) == 1); 
                    */
					la.values = ss;

                    prevSS = ss;

                    string[] nids = la.linkId.Split ('-');
                    NodeObject n1 = nDict [nids [0].Trim ()];
                    NodeObject n2 = nDict [nids [1].Trim ()]; 
          
                    g.name = "LINK_" + la.linkId;
                    g.head = n1;
                    g.tail = n2;
                    g.transform.parent = pathObject.transform;
          
                    Adjust (g);
                    Adjust (g);
          
                }
            }
        }
    
        //EditorUtility.SetDirty (pathObject);
    }
  
    private static void Adjust (LinkObject lo)
    {
    
        Vector3 pos1 = lo.head.transform.position;
        Vector3 pos2 = lo.tail.transform.position;
    
        float d = Vector3.Distance (pos1, pos2);
        lo.transform.localScale = new Vector3 (1, 1, d);
        lo.transform.LookAt (pos2, Vector3.up);
        lo.transform.position = pos1 + ((pos2 - pos1).normalized * d / 2);
    
    }
  
    private static int ToI (string f)
    {
        if (f.Trim ().Length == 0)
            return 0;

        int result = 0;
        bool b = int.TryParse (f, out result);
        if (b)
            return result;
        else
            return int.MinValue;
    }
  
    private static float ToF (string f)
    {
        if (f.Trim ().Length == 0)
            return 0;

        float result = 0;
        bool b = float.TryParse (f, out result);
    
        if (b)
            return result;
        else
            return float.NaN;
    }

}
