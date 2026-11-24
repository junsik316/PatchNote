using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomData
{
    public MapTree.Node node;

    // 방 생성 여부
    public bool isGenerated = false;
    public bool isCleared = false;

    // 방의 플랫폼 정보
    public List<RoomMaker.Platform> platforms =
        new List<RoomMaker.Platform>();
}