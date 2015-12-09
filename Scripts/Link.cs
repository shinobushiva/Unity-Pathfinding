using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/**
 *  A class representing a link
 * 
 * @author Shinobu Izumi (Kyushu Institute of Technology)
 * 
 */
[System.Serializable]
public class Link : Element
{

	protected Node headNode;

	public Node HeadNode {
		get {
			return headNode;
		}
		set {
			headNode = value;
		}
	}

	protected Node tailNode;
	
	public Node TailNode {
		get {
			return tailNode;
		}
		set {
			tailNode = value;
		}
	}
	
	
	//Without head and tail
	protected Vector3[] points;

	public Vector3[] Points {
		get {
			return points;
		}
		set {
			points = value;
		}
	}
	
	/** Creates a new instance of Link */
	public Link ()
	{
		_Construct (null, null, new Vector3[0]);
	}

	/**
	 * Creates a new instance of Link
	 * 
	 * @param headNode
	 * @param tailNode
	 */
	public Link (Node headNode, Node tailNode)
	{
		_Construct (headNode, tailNode, new Vector3[0]);
	}

	private void _Construct (Node headNode, Node tailNode, Vector3[] content)
	{
		points = new Vector3[(content != null ? content.Length : 0)];
		for (int i = 0; i < (content != null ? content.Length : 0); i++) {
			// by copy
			points [i] = new Vector3 (content [i].x, content [i].y, content [i].z);
		}
		
		HeadNode = headNode;
		if (headNode != null) {
			headNode.AddLink (this);
		}
		TailNode = tailNode;
		if (tailNode != null) {
			tailNode.AddLink (this);
		}
	}
	
	public Vector3 GetPoint (int index)
	{
		Vector3 ret;
		int beginIndex = (headNode != null ? 1 : 0);

		if (headNode != null && index == 0) {
			ret = headNode.Position;
		} else if (tailNode != null && index == points.Length + beginIndex) {
			ret = tailNode.Position;
		} else {
			ret = points [index - beginIndex];
		}
		return ret;
	}
	
	public int GetPointNum ()
	{
		return (points != null ? points.Length : 0)
				+ (headNode != null ? 1 : 0) + (tailNode != null ? 1 : 0);
	}
	
	public override string ToString ()
	{
		return "" + id;
//		"[" + id + "] : (head=" + headNode.Id
//				+ ", tail=" + tailNode.Id + ")";

	}
	
	

}
