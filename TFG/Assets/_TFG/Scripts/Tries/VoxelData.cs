using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelData
{
    public List<Vector3> dataVec;
    public VoxelData(List<Vector3> _d)
    {
        dataVec = _d;
    }

    public int GetNeighbor(Vector3 pos, Direction dir)
    {
        //DataCoordinate offsetToCheck = offsets[(int)dir];
        //DataCoordinate neighborCoord = new DataCoordinate((int)pos.x + offsetToCheck.x, 0 + offsetToCheck.y, (int)pos.z + offsetToCheck.z);

        //if (neighborCoord.x < 0 || neighborCoord.x >= Width || neighborCoord.y != 0 || neighborCoord.z < 0 || neighborCoord.z >= Depth)
        //{
        //    return 0;
        //}
        //else
        //    return 1;
        
        DataCoordinate offsetToCheck = offsets[(int)dir];

        Vector3 reference =  pos + offsetToCheck.position;
        for (int i = 0; i < dataVec.Count; i++)
        {
            if (dataVec[i] == reference)
                return 1;
        }
        return 0;
    }
    struct DataCoordinate
    {
        public int x;
        public int y;
        public int z;

        public Vector3 position;

        public DataCoordinate(int x, int y,int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;

            position = new Vector3(x,y,z);
        }
    }
    DataCoordinate[] offsets =
    {
        new DataCoordinate(0,0,1),
        new DataCoordinate(1,0,0),
        new DataCoordinate(0,0,-1),
        new DataCoordinate(-1,0,0),
        new DataCoordinate(0,1,0),
        new DataCoordinate(0,-1,0)

    };
}


public enum Direction
{
    NORTH,
    EAST,
    SOUTH,
    WEST,
    UP,
    DOWN
}
