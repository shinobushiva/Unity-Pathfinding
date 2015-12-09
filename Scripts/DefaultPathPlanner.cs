/**
 * PathPlanner implementation to culculate physically shortest path
 * 
 * @author Shinobu Izumi (Kyushu Institute of Technology)
 */
public class DefaultPathPlanner : AbstractPathPlanner
{
	
	/**
	 * Culculate a link logical length
	 * 
	 * @return link logical length
	 */
	public override float CalcLinkLength (Link l, Node baseNode, Node targetNode,
	                                      float physicalLength)
	{
		return physicalLength;
	}
	
	public DefaultPathPlanner ()
	{
		DivByNum = false;
	}
	
	
}