using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/**
 * 経路区間を示すノードを表すクラスです。
 * 
 * @author Shinobu Izumi (Kyushu Institute of Technology)
 * 
 */
[System.Serializable]
public class Node : Element
{
	[System.NonSerialized]
	protected List<Link> neighborLinks;

	public List<Link> NeighbourLinks {
		get {
			return neighborLinks;
		}
	}

	/** The position of this node */
	protected Vector3 position;

	public Vector3 Position {
		get {
			return position;
		}
		set {
			position = value;
		}
	}
	
	

	/** Creates a new instance of Node */
	public Node ()
	{ 
		_Construct (0, 0, 0);
	}

	/**
	 * Creates a new instance of Node
	 * 
	 * @param position
	 */
	public Node (Vector3  position)
	{
		_Construct (position.x, position.y, position.z);
	}

	/**
	 * Creates a new instance of Node
	 * 
	 * @param position
	 */
	public Node (Vector2 position)
	{
		_Construct (position.x, position.y, 0);
	}

	/**
	 * Creates a new instance of Node
	 * 
	 * @param x
	 * @param y
	 */
	public Node (float x, float y, float z)
	{
		_Construct (x, y, z);
	}

	void _Construct (float x, float y, float z)
	{
		this.position = new Vector3 (x, y, z);
		neighborLinks = new List<Link> ();
	}


	/**
	 * Add new Link.
	 */
	public void AddLink (Link newLink)
	{
		int index = neighborLinks.IndexOf (newLink);
		if (index >= 0) {
			throw new SystemException ("newLink already exist: " + newLink);
		} else {
			neighborLinks.Add (newLink);
		}
	}

	/**
	 * @param index
	 * @param newLink
	 */
	public void AddLink (int index, Link newLink)
	{
		int oldIndex = neighborLinks.IndexOf (newLink);
		if (oldIndex >= 0) {
			throw new SystemException ("newLink already exist: " + newLink);
		} else {
			neighborLinks.Insert (index, newLink);
		}
	}

	/**
	 * Returns the link element at the specified position in this list.
	 * 
	 * @param index
	 *            index of element to return.
	 * @return the link element at the specified position in this list.
	 * 
	 * @throws IndexOutOfBoundsException
	 *             if the index is out of range (index &lt; 0 || index &gt;=
	 *             size()).
	 */
	public Link GetLink (int index)
	{
		return neighborLinks [index];
	}

	/**
	 * Remove Link.
	 */
	public bool RemoveLink (Link newLink)
	{
		return neighborLinks.Remove (newLink);
	}

	/**
	 * Add Links.
	 */
	public void AddAllLinks (Node node)
	{
		neighborLinks.AddRange (node.neighborLinks);
	}

	/**
	 * Add Links.
	 */
	public void AddAllLinks (int index, Node node)
	{
		neighborLinks.InsertRange (index, node.neighborLinks);
	}

	public override string ToString ()
	{
		// return super.toString() + "[" + id + "] : " + position + " : "
		// + neighborLinks;
		return base.ToString () + "[" + id + "] ";
	}
}
