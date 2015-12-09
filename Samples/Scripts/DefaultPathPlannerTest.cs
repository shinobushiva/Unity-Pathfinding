using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DefaultPathPlannerTest : MonoBehaviour
{
	public GameObject nodePrefab;
	public GameObject linkPrefab;
	public Dictionary<string, GameObject> dict = new Dictionary<string, GameObject> ();
	public List<Node> nodes;
	public List<Link> links;
	public Rect windowRect = new Rect (20, 20, 300, 50);

	void OnGUI ()
	{
		windowRect = GUILayout.Window (0, windowRect, DoMyWindow, "My Window");
	}
	
	int selectedS = 0;
	int selectedG = 0;

	void DoMyWindow (int windowID)
	{
		
		GUILayout.BeginVertical ();
		{
			
		
			GUILayout.BeginHorizontal ();
			{
		
				GUILayout.BeginVertical ();
				{
					GUIContent[] cs = new GUIContent[nodes.Count];
					for (int i=0; i<nodes.Count; i++) {
						cs [i] = new GUIContent ();
						cs [i].text = "" + nodes [i].Id;
					}
					selectedS = ToggleList (selectedS, cs);
				}
		
				GUILayout.EndVertical ();
			
				GUILayout.BeginVertical ();

				{
					GUIContent[] cs = new GUIContent[nodes.Count];
					for (int i=0; i<nodes.Count; i++) {
						cs [i] = new GUIContent ();
						cs [i].text = "" + nodes [i].Id;
					}
					selectedG = ToggleList (selectedG, cs);
				}
		
				GUILayout.EndVertical ();
			}
			GUILayout.EndHorizontal ();
			
			if (GUILayout.Button ("Search")) {
				Node s = nodes [selectedS];
				Node g = nodes [selectedG];
				Search (s, g);
				
			}
			
		}
		GUILayout.EndVertical ();
        
	}
	
	public static int ToggleList (int selected, GUIContent[] items)
	{

		// Keep the selected index within the bounds of the items array

		selected = selected < 0 ? 0 : selected >= items.Length ? items.Length - 1 : selected;

		for (int i = 0; i < items.Length; i++) {
			// Display toggle. Get if toggle changed.
			bool change = GUILayout.Toggle (selected == i, items [i]);
			// If changed, set selected to current index.
			if (change)
				selected = i;
		}

		// Return the currently selected item's index

		return selected;

	}
	
	public void Create (Node n)
	{
		float size = 5;
		GameObject g = (GameObject)Instantiate (nodePrefab, Vector3.zero, Quaternion.identity);
		g.transform.localScale = new Vector3 (size, size, size);
		g.name = "" + n.Id;
		dict ["" + n.Id] = g;
		g.GetComponent<NodeObject> ().node = n;
		g.GetComponent<NodeObject> ().id = "" + n.Id;
		g.transform.position = g.GetComponent<NodeObject> ().node.Position;

		nodeMap.Add (n, g.GetComponent<NodeObject> ());
	}
	
	public void Create (Link l)
	{
		Vector3 pos1 = l.HeadNode.Position;
		Vector3 pos2 = l.TailNode.Position;
		
		GameObject g = (GameObject)Instantiate (linkPrefab, pos1, Quaternion.identity);
		g.name = "" + l.Id;
		dict ["" + l.Id] = g;
		
		g.GetComponent<LinkObject> ().link = l;
		g.GetComponent<LinkObject> ().head = nodeMap [l.HeadNode];
		g.GetComponent<LinkObject> ().tail = nodeMap [l.TailNode];


		linkMap.Add (l, g.GetComponent<LinkObject> ());
	}
	
	Node CreateNode (float x, float y, float z, int id)
	{
		Node n = new Node (x, y, z);
		n.Id = "" + id;
		Create (n);
		nodes.Add (n);
		
		return n;
	}
	
	Link CreateLink (Node n1, Node n2, int id)
	{

		Link l = new Link (n1, n2);
		l.Id = "" + id;
		Create (l);
		links.Add (l);
		
		
		return l;
	}

	Link[] CreateLinkBoth (Node n1, Node n2, int id)
	{
		Link l1;
		Link l2;
		{
			l1 = new Link (n1, n2);
			l1.Id = "" + id;
			Create (l1);
			links.Add (l1);


		}
		{
			l2 = new Link (n2, n1);
			l2.Id = "_" + id;
			Create (l2);
			links.Add (l2);
		}
		
		return new Link[]{l1, l2};
	}

	private List<Element> vec;
	
	public void Search (Node s, Node g)
	{
		
		//Color initializeing
		Color trans = Color.white;
		trans.a = 0.5f;
		foreach (Node e in nodes) {
			
			Renderer[] rs = dict ["" + e.Id].GetComponentsInChildren<Renderer> () as Renderer[];
			foreach (Renderer r in rs) {
				r.material.color = trans;
			}
		}
		foreach (Link e in links) {
			
			Renderer[] rs = dict ["" + e.Id].GetComponentsInChildren<Renderer> () as Renderer[];
			foreach (Renderer r in rs) {
				r.material.color = trans;
			}
		}
		
		//Create Planner
		AbstractPathPlanner dp = new DefaultPathPlanner ();
		dp.UseDAG = false;

		
		//Search
		dp.Search (s, g);
		
		//Get Result
		vec = dp.GetPath ();
		
		//Coloring
		foreach (Element e in vec) {
			print ("" + e);
			print ("|");
			
			Renderer[] rs = dict ["" + e.Id].GetComponentsInChildren<Renderer> () as Renderer[];
			foreach (Renderer r in rs) {
				r.material.color = Color.green;
			}
		}
		dict ["" + s.Id].GetComponent<Renderer> ().material.color = Color.red;
		dict ["" + g.Id].GetComponent<Renderer> ().material.color = Color.blue;
		
		print (dp.GetDistance ());
		print (dp.GetPhysicalDistance ());
		
		
		Vector3[] pps = dp.GetPathPoints ();
		foreach (Vector3 v in pps) {
			print (v);
		}
	}

	Dictionary<Node, NodeObject> nodeMap;
	Dictionary<Link, LinkObject> linkMap;

	// Use this for initialization
	void Start ()
	{

		nodeMap = new Dictionary<Node, NodeObject> ();
		linkMap = new Dictionary<Link, LinkObject> ();	
		
		nodes = new List<Node> ();
		links = new List<Link> ();
		
		Node n1 = CreateNode (-56, 0, 52, 101);
		Node n2 = CreateNode (18, 0, 16, 102);
		Node n3 = CreateNode (4, 0, 48, 103);
		Node n4 = CreateNode (-30, 0, -60, 104);
		Node n5 = CreateNode (-56, 0, -56, 105);
		Node n6 = CreateNode (-74, 0, -18, 106);
		Node n7 = CreateNode (-52, 0, -16, 107);
		Node n8 = CreateNode (-44, 0, -26, 108);
		Node n9 = CreateNode (-24, 0, -38, 109);
		Node n10 = CreateNode (-4, 0, -30, 110);
		Node n11 = CreateNode (-6, 0, 0, 111);
		Node n12 = CreateNode (-42, 0, 6, 112);
		
		CreateLinkBoth (n1, n10, 201);
		CreateLinkBoth (n11, n2, 202);
		CreateLinkBoth (n3, n10, 203);
		CreateLinkBoth (n4, n9, 204);
		CreateLinkBoth (n5, n8, 205);
		CreateLinkBoth (n6, n7, 206);
		CreateLinkBoth (n7, n12, 207);
		CreateLinkBoth (n7, n8, 208);
		CreateLinkBoth (n8, n9, 209);
		CreateLinkBoth (n9, n10, 210);
		CreateLinkBoth (n10, n11, 211);	
		CreateLinkBoth (n1, n12, 212);
		CreateLinkBoth (n12, n2, 213);
		CreateLinkBoth (n11, n12, 213);
	
		Node s = n1;
		Node g = n5;
		Search (s, g);
			
		

	}

	void OnDrawGizmos ()
	{

		if (nodeMap == null)
			return;

		Gizmos.color = Color.white;
		#if UNITY_EDITOR
		foreach (Node e in nodeMap.Keys) {
			UnityEditor.Handles.Label(e.Position, e.id);
		}
		#endif

		Gizmos.color = Color.green;

		foreach (Link e in linkMap.Keys) {
			Gizmos.DrawLine (e.HeadNode.Position, e.TailNode.Position);
		}


		if (vec == null)
			return;

		Gizmos.color = Color.red;

		foreach (Element e in vec) {
			if (e is Link) {
				Link l = e as Link;
				Gizmos.DrawLine (l.HeadNode.Position, l.TailNode.Position);
			}
		}

	}
	
}


