using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common.Track
{
    [System.Serializable]
    public class Session
    {
        public float lenght = 1;
        public MoveType moveType;
        public TrackDirection direction;
    }
}
