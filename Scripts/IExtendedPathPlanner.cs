using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * 
 * @author Shinobu Izumi (Kyushu Institute of Technology)
 * 
 */
public interface IExtendedPathPlanner
{

	float GetMin();

	float GetMax();

 	float Calc (LinkAttribute la, LinkObject lo);

	Color GetColor (LinkObject lo, 
	                      Color cDef, 
	                      Color c1,
	                      Color c2, 
	                      Color c3,
	                       float alpha = 0.8f);
	void CalcMinMax();
}