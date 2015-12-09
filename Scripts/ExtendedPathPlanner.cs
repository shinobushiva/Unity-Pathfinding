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
public abstract class ExtendedPathPlanner : AbstractPathPlanner, IExtendedPathPlanner
{

	public float min;

	public float GetMin(){
		return min;
	}

	public float max;

	public float GetMax(){
		return max;
	}

	public virtual float Calc (LinkAttribute la, LinkObject lo)
	{
		return 0;
	}

	public virtual void CalcMinMax ()
	{
	}
	 
	public virtual Color GetColor (LinkObject lo, 
	                      Color cDef, 
	                      Color c1,
	                      Color c2, 
	                      Color c3,
	                      float alpha = 0.8f)
	{
		LinkObjectColor loc = lo.gameObject.GetComponent<LinkObjectColor> ();
		LinkAttribute la = lo.GetComponent<LinkAttribute> ();
		float f = Calc (la, lo);
		Color c;
		if (f < 0.01f) {
			c = cDef;
		} else {
			float val = (f - min) / (max - min);
			if (val < 0.5f) 
				c = Color.Lerp (c1, c2, val * 2);
			else
				c = Color.Lerp (c2, c3, (val - 0.5f) * 2);
		}
		c.a = alpha;

		return c;
	}


}