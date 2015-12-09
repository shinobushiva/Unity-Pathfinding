using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NodeRandomSimpleWalkBehavior : ABehavior
{
	private List<Element> resultPath;
	private List<Element> nodeResult;
	private Node currentTarget;
	private LinkObject[] links;
	private NodeObject[] nodes;
	//
	private Dictionary<Node, NodeObject> nodeMap;
	private Dictionary<Link, LinkObject> linkMap;
	
	//
	private  SpeedDirectionBehavior speedDirectionBehavior;
	
	//
	private int numRecentPositions = 10;
	private Queue<Vector3> recentPositions = new Queue<Vector3> ();
	
	void Start ()
	{
		speedDirectionBehavior = GetComponent<SpeedDirectionBehavior> ();
	}
	
	public float UpdateRecentPositions (Vector3 cur)
	{
		if (recentPositions.Count >= numRecentPositions) {
			recentPositions.Dequeue ();
		
			recentPositions.Enqueue (cur);
		
			float d = 0;
			Vector3[] rps = recentPositions.ToArray ();
			for (int i=0; i<recentPositions.Count-1; i++) {
				d += Vector3.Distance (rps [i], rps [i + 1]);
			}
			return d;
		} else {
			recentPositions.Enqueue (cur);
			
			return float.MaxValue;
		}
	}
	
	override public void Begin ()
	{
		//print ("NodeRandomWalkBehavior#Begin");
		nodes = FindObjectsOfType (typeof(NodeObject)) as NodeObject[];
		nodeMap = new Dictionary<Node, NodeObject> ();
		foreach (NodeObject n in nodes) {
			nodeMap [n.node] = n;
		}
		
		links = FindObjectsOfType (typeof(LinkObject)) as LinkObject[];
		linkMap = new Dictionary<Link, LinkObject> ();
		foreach (LinkObject l in links) {
			linkMap [l.link] = l;
		}
		
		currentTarget = FindNearest ();
		
	}
	
	private Node FindNearest ()
	{
		NodeObject[] ns = nodes.OrderBy (
			n => Vector3.Distance (n.transform.position, transform.position)
		).ToArray ();
		if (ns [0].node == currentTarget)
			return ns [Random.Range (0, ns.Length)].node;
		else
			return ns [0].node;
	}

	public override void Step ()
	{
		
		/*
		RaycastHit[] hits = Physics.RaycastAll (transform.position, speedDirectionBehavior.Direction, 
			Vector3.Distance (transform.position, currentTarget.Position));
		foreach (RaycastHit h in hits) {
			if (h.collider.gameObject.tag == "Wall") {
				currentTarget = FindNearest ();
				//print ("Found Nearest : " + currentTarget.Id);
		
				Vector3 p1 = transform.position;
				Vector3 p2 = currentTarget.Position;
				speedDirectionBehavior.Direction = (p2 - p1).normalized;
				
				return;
			}
		}
		*/
		
		/*
		float dist = UpdateRecentPositions (transform.position);
		if (dist < 0.1f) {
			//Stack;
			currentTarget = FindNearest ();
			//print ("Stack and Found Nearest : " + currentTarget.Id);
		
			Vector3 p1 = transform.position;
			Vector3 p2 = currentTarget.Position;
			speedDirectionBehavior.Direction = (p2 - p1).normalized;
				
			return;
		}
		*/
		
		if (Vector3.Distance (currentTarget.Position, transform.position) < 2f) {
			List<LinkObject> links = nodeMap [currentTarget].connectedLinks;
			LinkObject lo = links [Random.Range (0, links.Count)];
			if (lo.head == nodeMap [currentTarget]) {
				currentTarget = lo.tail.node;
			} else {
				currentTarget = lo.head.node;
			}
			
			Vector3 p1 = transform.position;
			Vector3 p2 = currentTarget.Position;
			speedDirectionBehavior.Direction = (p2 - p1).normalized;
				
			return;
		}
		
		
		//List<NodeObject> nl = GetAgentsAroundPosition<NodeObject> (AttachedAgent.World, transform.position, 1f);
		
		//print ("Count : " + nl.Count);
		//print ("Current : " + currentTarget.Id);
		
		/*
		List<NodeObject> nl = nodes.OrderBy (n => Vector3.Distance (transform.position, n.transform.position)).ToList ();
		if (nl.Count > 0 && nl [0].node == currentTarget) {
			
			List<Node> candidates = new List<Node> ();
			foreach (LinkObject lo in links) {
				if (lo.head.node == currentTarget) {
					if (!candidates.Contains (lo.tail.node)) {
						candidates.Add (lo.tail.node);
					}
				} else if (lo.tail.node == currentTarget) {
					if (!candidates.Contains (lo.head.node)) {
						candidates.Add (lo.head.node);
					}
				}
			}
			candidates.Remove (currentTarget);
			
			Node next = candidates [Random.Range (0, candidates.Count)];
			Vector3 p1 = transform.position;
			Vector3 p2 = next.Position;
			speedDirectionBehavior.Direction = (p2 - p1).normalized;
			currentTarget = next;
			
		} else {
			Vector3 p1 = transform.position;
			Vector3 p2 = currentTarget.Position;
			speedDirectionBehavior.Direction = (p2 - p1).normalized;
		}
		*/
	}
	
	void OnDrawGizmos ()
	{
		
		Gizmos.color = Color.white;
		if (currentTarget != null)
			Gizmos.DrawLine (transform.position, currentTarget.Position);
		
		
		if (speedDirectionBehavior) {
			Gizmos.color = Color.green;
			Gizmos.DrawLine (transform.position, transform.position + speedDirectionBehavior.Direction);
		}
	}
}
