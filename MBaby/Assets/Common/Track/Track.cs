using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common.Track
{
    public enum MoveType { Line, Arc };
    public enum TrackDirection { Left, Up, Right, Down }

    public class Track : MonoBehaviour
    {

        public List<Session> sessions = new List<Session>();
        public bool filpNormal = false;
        [Range(0.05f, 0.5f)]
        public float lenghtOfNode = 0.1f;
        public List<Nodes> nodes = new List<Nodes>();
        public bool autoFinish = false;
        public int positionSelected = 0;

        private float upLimit = 0;
        private float downLimit = 0;
        private float leftLimit = 0;
        private float rightLimit = 0;

        // Use this for initialization
        void Start()
        {
            SetNodes();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void SetNodes()
        {
            nodes = new List<Nodes>
        {
            new Nodes { position = Vector2.zero }
        };

            for (int i = 0; i < sessions.Count; i++) SetNodes(i);

            if (autoFinish) AutoFinishNodes();

            ScaleAndMoveToLocal();
            CheckLimit();
            SetNormal();
        }

        void SetNodes(int sessionNumber)
        {
            int numberOfNode = 0;
            float tempDeg = 0;
            Vector2 tempPos = Vector2.zero;
            Vector2 tempCen = Vector2.zero;
            Vector2 tempDir = Vector2.zero;

            if (nodes.Count > 0)
                tempPos = nodes[nodes.Count - 1].position;

            switch (sessions[sessionNumber].direction)
            {
                case TrackDirection.Left:
                    tempDir = Vector2.left;
                    tempDeg = 270f;
                    tempCen = Vector2.up;
                    break;
                case TrackDirection.Right:
                    tempDir = Vector2.right;
                    tempDeg = 90f;
                    tempCen = Vector2.down;
                    break;
                case TrackDirection.Down:
                    tempDir = Vector2.down;
                    tempDeg = 180f;
                    tempCen = Vector2.left;
                    break;
                case TrackDirection.Up:
                    tempDir = Vector2.up;
                    tempDeg = 0f;
                    tempCen = Vector2.right;
                    break;
                default:
                    Debug.LogError("direction not defined");
                    break;
            }

            if (sessions[sessionNumber].moveType == MoveType.Line)
            {
                numberOfNode = Mathf.CeilToInt(sessions[sessionNumber].lenght / lenghtOfNode);
                for (int i = 1; i < numberOfNode; i++)
                {
                    nodes.Add(new Nodes());
                    nodes[nodes.Count - 1].position = tempPos + i * tempDir * lenghtOfNode;
                }
            }

            if (sessions[sessionNumber].moveType == MoveType.Arc)
            {
                numberOfNode = Mathf.CeilToInt(sessions[sessionNumber].lenght * Mathf.PI / 2 / lenghtOfNode);
                for (int i = 1; i < numberOfNode; i++)
                {
                    nodes.Add(new Nodes());
                    nodes[nodes.Count - 1].position = new Vector2(
                        tempPos.x - Mathf.Cos((tempDeg + (i * 90f / (numberOfNode - 1))) * Mathf.PI / 180f) * sessions[sessionNumber].lenght,
                        tempPos.y + Mathf.Sin((tempDeg + (i * 90f / (numberOfNode - 1))) * Mathf.PI / 180f) * sessions[sessionNumber].lenght);
                    nodes[nodes.Count - 1].position += sessions[sessionNumber].lenght * tempCen;
                }
            }
        }

        void AutoFinishNodes()
        {
            Vector3 tempPos = Vector3.zero;
            float distant = (nodes[0].position - nodes[nodes.Count - 1].position).magnitude;
            Vector3 dir = (nodes[0].position - nodes[nodes.Count - 1].position).normalized;
            int numberOfNode = Mathf.CeilToInt(distant / lenghtOfNode);

            if (nodes.Count > 0)
                tempPos = nodes[nodes.Count - 1].position;

            for (int i = 1; i < numberOfNode; i++)
            {
                nodes.Add(new Nodes());
                nodes[nodes.Count - 1].position = tempPos + i * dir * lenghtOfNode;
            }
        }

        void ScaleAndMoveToLocal()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].position = new Vector2(
                    nodes[i].position.x * transform.localScale.x,
                    nodes[i].position.y * transform.localScale.y);
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].position = transform.InverseTransformDirection(nodes[i].position);
                nodes[i].position = nodes[i].position + (Vector2)transform.position;
            }
        }

        void CheckLimit()
        {
            if ((nodes.Count > 0) && (nodes[0] != null))
            {
                leftLimit = nodes[0].position.x;
                rightLimit = nodes[0].position.x;
                upLimit = nodes[0].position.y;
                downLimit = nodes[0].position.y;

                for (int i = 0; i < nodes.Count; i++)
                {
                    if ((nodes[i].position.x) > rightLimit)
                        rightLimit = nodes[i].position.x;
                    else if ((nodes[i].position.x) < leftLimit)
                        leftLimit = nodes[i].position.x;

                    if ((nodes[i].position.y) > upLimit)
                        upLimit = nodes[i].position.y;
                    else if ((nodes[i].position.y) < downLimit)
                        downLimit = nodes[i].position.y;
                }
            }

            Vector2 offset = new Vector2(
                (leftLimit + rightLimit) / 2,
                (upLimit + downLimit) / 2);

            switch (positionSelected)
            {
                case 1:
                    offset = new Vector2(leftLimit, downLimit); break;
                case 2:
                    offset = new Vector2(offset.x, downLimit); break;
                case 3:
                    offset = new Vector2(rightLimit, downLimit); break;
                case 4:
                    offset = new Vector2(leftLimit, offset.y); break;
                case 6:
                    offset = new Vector2(rightLimit, offset.y); break;
                case 7:
                    offset = new Vector2(leftLimit, upLimit); break;
                case 8:
                    offset = new Vector2(offset.x, upLimit); break;
                case 9:
                    offset = new Vector2(rightLimit, upLimit); break;

                case 0:
                case 5:
                default:
                    break;
            }

            offset = offset - nodes[0].position;

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].position = nodes[i].position - offset;
            }
        }

        void SetNormal()
        {
            if (!filpNormal)
            {
                nodes[0].normals = V2Rotate((nodes[1].position - nodes[nodes.Count - 1].position), 90f);
                nodes[nodes.Count - 1].normals = V2Rotate((nodes[0].position - nodes[nodes.Count - 2].position), 90f);
            }
            else
            {
                nodes[0].normals = V2Rotate((nodes[1].position - nodes[nodes.Count - 1].position), -90f);
                nodes[nodes.Count - 1].normals = V2Rotate((nodes[0].position - nodes[nodes.Count - 2].position), -90f);
            }

            for (int i = 1; i < nodes.Count - 1; i++)
            {
                if (!filpNormal)
                {
                    nodes[i].normals = V2Rotate((nodes[i + 1].position - nodes[i - 1].position), 90f);
                }
                else
                {
                    nodes[i].normals = V2Rotate((nodes[i + 1].position - nodes[i - 1].position), -90f);
                }
            }
        }

        public int NearestNode(Vector2 pos)
        {
            int temp = 0;
            float dis = Mathf.Infinity;


            if (nodes.Count > 0)
                for (int i = 1; i < nodes.Count - 1; i++)
                {
                    if ((nodes[i].position - pos).sqrMagnitude < dis)
                    {
                        temp = i;
                        dis = (nodes[i].position - pos).sqrMagnitude;
                    }
                }
            return temp;
        }

        Vector2 V2Rotate(Vector2 aPoint, float a)
        {
            float rad = a * Mathf.Deg2Rad;
            float s = Mathf.Sin(rad);
            float c = Mathf.Cos(rad);
            return new Vector2(
                aPoint.x * c - aPoint.y * s,
                aPoint.y * c + aPoint.x * s);
        }

        public void DebugShowNodes()
        {
            SetNodes();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Gizmos.DrawLine(nodes[i].position, nodes[i + 1].position);
            }

            Gizmos.color = Color.blue;

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Gizmos.DrawLine(nodes[i].position, nodes[i].position + nodes[i].normals);
            }
        }
    }
}


