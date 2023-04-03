using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPointPairs
{
    public EndPointPairs(Vector3 StartPoint, Vector3 EndPoint)
    {
        this.StartPoint = StartPoint;
        this.EndPoint = EndPoint;
    }


    public Vector3 StartPoint { get; private set; }
    public Vector3 EndPoint { get; private set; }

}
