using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Abstract class that culculate a shortest path by Dijestra algorithem.<br>
 * <br>
 * You must specify the way to culculate logical distance for a link
 * 
 * @author Shinobu Izumi (Kyushu Institute of Technology)
 * 
 */
public abstract class AbstractPathPlanner
{

	/**
	 *  Holds the shortest path distance to the goal node
	 */
	private float minDistance;

	/**
	 *  Holds the start node
	 */
	private Node start;

	/**
	 *  Holds the goal node
	 */
	private Node goal;

	/**
	 *  Holds the logically shortest path from start to goal node
	 */
	private List<Element> path = new List<Element> ();



	/**
	 *  Holds physical distance to each links
	 */
	private Dictionary<Link, float> physicalDistMap = new Dictionary<Link, float> ();

	/**
	 *  Holds logical distance to each links
	 */
	private Dictionary<Link, float> logicalDistMap = new Dictionary<Link, float> ();
	private float physicalDistance = 0.0f;

	/**
	 *  Holds reachable paths from start to goal nodes
	 * 
	 * TODO: Currently this in not used. It may be used when we want to have more than one path in the future.
	 */
	// private List<Object> paths = new List<Object>();
	
	/**
	 *  Holds if it divides the distance by link number
	 */
	private bool divByNum = false;

	public bool DivByNum {
		get {
			return divByNum;
		}
		set {
			divByNum = value;
		}
	}


	/**
	 *  Hold if it uses the directed graph.
	 * 
	 *  The direction of the graph is from HeadNode to TailNode
	 */
	private bool useDAG = false;

	public bool UseDAG {
		get {
			return useDAG;
		}
		set {
			useDAG = value;
		}
	}

	/**
	 *  Get the node at the end or this link
	 * 
	 * @param l
	 *            The link
	 * @param n
	 *            Current node
	 * @return Next node (if not return null)
	 */
	public Node GetNextNode (Link l, Node n)
	{

		Node hn = l.HeadNode;
		Node tn = l.TailNode;
		if (hn == null || tn == null) {
			return null;
		}
		if (hn == n) {
			return l.TailNode;
		} else {
			return l.HeadNode;
		}
	}

	void CollectNodes (Node n, List<Node> nodes)
	{
		foreach (Link l in n.NeighbourLinks) {
			if (!nodes.Contains (l.HeadNode)) {
				nodes.Add (l.HeadNode);
				CollectNodes (l.HeadNode, nodes);
			}

			if (!nodes.Contains (l.TailNode)) {
				nodes.Add (l.TailNode);
				CollectNodes (l.TailNode, nodes);
			}
		}
	}

	/**
	 * Run the path finding
	 * 
	 * @param s
	 *            Start node
	 * @param g
	 *            Goal Node
	 * 
	 * @return If it can not find the shortest path then return false
	 */
	public bool Search (Node s, Node g)
	{
		
		if (s == null || g == null) {
			return false;
		}


		//need to collect all nodes
		List<Node> nodes = new List<Node> ();
		CollectNodes (s, nodes);

		Dictionary<Node, Link> linkCommingToNode = new Dictionary<Node, Link> ();
		List<Node> visited = new List<Node> ();
		Dictionary<Node, Node> previousMap = new Dictionary<Node, Node> ();
		Dictionary<Node, float> distMap = new Dictionary<Node, float> ();

		// init
		physicalDistMap.Clear ();
		logicalDistMap.Clear ();
		path.Clear ();

		foreach (Node n in nodes) {
			distMap.Add (n, float.PositiveInfinity);
			previousMap.Add (n, null);
		}

		start = s;
		goal = g;

		Node next = start;
		distMap [next] = 0.0f;

		Node current = null;

		while (nodes.Count > 0) {
			
			float min = float.PositiveInfinity;
			Node minNode = null;
			Node toMinNode = null;
			next = null;
			foreach (Node n in nodes) {
				if (!visited.Contains (n)) {
					float distJ = distMap [n];
					if (distJ < min) {
						min = distJ;
						next = n;
					}
				}
			}
			current = next;
			visited.Add (current);
			nodes.Remove (current);

			List<Link> connectedLinkFromCurrent = new List<Link> ();
			// Collect and add candidate nodes
			foreach (Link l in current.NeighbourLinks) {
				// for directed graph
				if (useDAG && current == l.TailNode) {
					continue;
				}
				Node neighbourNode = GetNextNode (l, current);
				if (visited.Contains (neighbourNode)) {
					continue;
				}
				connectedLinkFromCurrent.Add (l);
			}

			foreach (Link l in connectedLinkFromCurrent) {
				Node toNode  = GetNextNode(l, current);
				float ll = _GetOrCalcLinkLength (l, current, toNode);
				
				// update the shortest path if needed
				float distI = float.PositiveInfinity;
				float distJ = float.PositiveInfinity;
				distI = distMap [current];
				distJ = distMap [toNode];
				if (distI + ll < distJ) {
					distMap [toNode] = distI + ll;
					previousMap [toNode] = current;
					linkCommingToNode[toNode] = l;
				}
			}
		}

		//  Routes for all nodes are fixed
		minDistance = 0;
		physicalDistance = 0;

		path = new List<Element> ();
		current = goal;
		path.Add (current);

		int count = 0;
		while (previousMap [current] != null) {
			Link link = linkCommingToNode [current];
			path.Add (link);
			Node pre = previousMap [current];
			path.Add (pre);
			current = pre;

			minDistance += logicalDistMap [link];
			physicalDistance += physicalDistMap [link];
			count++;
		}

		if (!distMap.ContainsKey (goal))
			return false;

		minDistance = distMap [goal];

		if (current != start) {
			return false;
		}

		if (divByNum) {
			minDistance /= count;
		}

		return true;
	}

	/**
	 * Get the list of Elements of the shortest path
	 * 
	 * @return if it's not found then  null
	 */
	public List<Element> GetPath ()
	{
		return path;
	}

	/**
	 * Get the shortest path's distance from start to goal node
	 * 
	 * @return if it's not found then - POSITIVE_INFINITY
	 */
	public float GetDistance ()
	{
		return minDistance;
	}

	/**
	 * Get the shortest path's physical distance from start to goal node
	 * 
	 * @return if it's not found then - POSITIVE_INFINITY
	 */
	public float GetPhysicalDistance ()
	{
		return physicalDistance;
	}


	/**
	 * Calculate the link length<br>
	 * <br>
	 *  This method is called in path finding method
	 * 
	 * @param l
	 *            target link
	 * @param baseNode
	 *            begin node
	 * @param targetNode
	 *            end node
	 * @param phisicalLength
	 *            physical distance
	 * 
	 * @return distance
	 */
	public virtual float CalcLinkLength (Link l, Node baseNode,
			Node targetNode, float phisicalLength)
	{
		return 0;
	}

	public virtual float CalcLinkConnection (Link l, Link prev, Node baseNode)
	{
		return 0;
	}

	/**
	 * Calculate the link length。
	 * 
	 * @param l
	 *            target link
	 * @param baseNode
	 * @param targetNode
	 * @param phisicalLength
	 * 
	 * @return distance
	 */
	private float _GetOrCalcLinkLength (Link l, Node baseNode, Node targetNode)
	{
		if (logicalDistMap.ContainsKey (l)) {
			return logicalDistMap [l];
		} else {

			float pl = CalcPhysicalLinkLength (l);
			float d = CalcLinkLength (l, baseNode, targetNode, pl);
			logicalDistMap [l] = d;
			return d;
		}

	}

	/**
	 *  Caluculate the physical distance of the given link
	 * 
	 * @return Euclid distance of the link
	 */
	private float CalcPhysicalLinkLength (Link l)
	{
		if (physicalDistMap.ContainsKey (l)) {
			
		} else {

			Vector3 p1;
			Vector3 p2;

			float dist = 0.0f;
			for (int i = 0; i < l.GetPointNum() - 1; i++) {
				p1 = l.GetPoint (i);
				p2 = l.GetPoint (i + 1);
				dist += Vector3.Distance (p1, p2);
			}
			physicalDistMap [l] = dist;
		}
		return physicalDistMap [l];
	}
	
	/**
	 *  Get physical positions of nodes that are contained in the found path
	 * 
	 * @return  list of nodes' position
	 */
	public Vector3[] GetPathPoints ()
	{
		Node node = null;
		Link link;

		List<Element> vec = GetPath ();
		List<Vector3> result = new List<Vector3> ();
		
		Vector3[] p3ds;
		
		foreach (Element o in vec) {
			if (o is Node) { 
				node = (Node)o;
				result.Add (node.Position);
			} else {
				link = (Link)o;
				p3ds = link.Points;
				if (link.HeadNode == node) {
					result.AddRange (p3ds);
				} else {
					for (int i = p3ds.Length; i > 0; i--) {
						result.Add (p3ds [i - 1]);
					}
				}
			}
		}
		
		return result.ToArray ();
	}


}