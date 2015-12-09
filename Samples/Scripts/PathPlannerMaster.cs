using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shiva.PathPlanning;

public class PathPlannerMaster : SingletonMonoBehaviour<PathPlannerMaster>
{
	[HideInInspector]
	public PathPlannerWorker[] workers;
	[HideInInspector]
	public PathPlannerWorker mainWorker;

	public PathPlanWorker[] planWorkers;

	void Start(){

		List<PathPlannerWorker> list = new List<PathPlannerWorker> ();
		
		foreach (PathPlanWorker ppw in planWorkers) {
			list.Add (new PathPlannerWorker (ppw));
		}
		workers = list.ToArray ();
		
		mainWorker = workers [0];
	}

	public class PathPlannerWorker
	{
		public string name;
		public PathPlanWorker worker;
		
		//
		public List<Element> resultPath;
		public float physicalDistance;
		public float logicalDistance;
		public Color displayColor;
		public bool isShowing = true;
		public Texture2D tex;
		
		public PathPlannerWorker (PathPlanWorker worker)
		{
			this.name = worker.displayName;
			this.worker = worker;
			this.displayColor = worker.color;

			tex = new Texture2D (1, 1);
			tex.SetPixel (0, 0, displayColor);
			tex.Apply ();

		}
		
		public void Search (Node s, Node g)
		{
			//Search
			worker.SetupForSeraching ();
			worker.Planner.Search (s, g); 
			
			//Get Result
			resultPath = worker.Planner.GetPath ();
			resultPath.Reverse ();

			if (resultPath.Count > 1) {
				physicalDistance = worker.Planner.GetPhysicalDistance ();
				logicalDistance = worker.Planner.GetDistance ();
			} else {
				physicalDistance = float.MaxValue;
				logicalDistance = float.MaxValue;

			}
		}
		
		public void OnGUI (int d=0, int w=1)
		{
			if (resultPath == null)
				return;

//			print (this.name);
			foreach (Element e in resultPath) {
//				print (e.id);
				if (e is Link) {
					Link l = (Link)e;
					Vector3 p1 = Camera.main.WorldToScreenPoint (l.HeadNode.Position);
					p1.y = Screen.height - p1.y;
					
					Vector3 p2 = Camera.main.WorldToScreenPoint (l.TailNode.Position);
					p2.y = Screen.height - p2.y;
					
					GuiHelper.DrawLine (new Vector2 (p1.x + d, p1.y + d), new Vector2 (p2.x + d, p2.y + d), displayColor, w);
				}
			}
		}
	}

	public void OnAriscoGUI (List<Element> resultPath, Color displayColor, int d=0, int w=1)
	{
		if (resultPath == null)
			return;
		

		foreach (Element e in resultPath) {

			if (e is Link) {
				Link l = (Link)e;
				Vector3 p1 = Camera.main.WorldToScreenPoint (l.HeadNode.Position);
				p1.y = Screen.height - p1.y;
				
				Vector3 p2 = Camera.main.WorldToScreenPoint (l.TailNode.Position);
				p2.y = Screen.height - p2.y;
				
				GuiHelper.DrawLine (new Vector2 (p1.x + d, p1.y + d), new Vector2 (p2.x + d, p2.y + d), displayColor, w);
			}
		}
	}



	// Use this for initialization
	public void Setup (World world)
	{


	
	}
}


