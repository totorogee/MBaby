using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common.Track;

namespace Common.Track
{
    [System.Serializable]
    public enum TrainDirection { In, Out, Forward, Backward }

    [RequireComponent(typeof(Effect))]
    public class Train : MonoBehaviour
    {
        public Track track;
        public bool onTrack = true;
        public bool facing = true;
        public bool movingBack = false;

        public List<TrainParts> partsList = new List<TrainParts>();
        public float speed = 20f;
        public float nodeNumber = 0;

        public int rotateSelected = 0;

        public int nodeI = 0;
        protected Effect eff;

        // Use this for initialization
        void Start()
        {

            if (track == null) Debug.LogWarning("Train Need Track reference");

            if (partsList.Count == 0)
                partsList.Add(new TrainParts { part = transform, facing = TrainDirection.Out });

            if (partsList[0].part == null)
                partsList[0].part = this.transform;

            if (this.transform.GetComponent<Effect>() != null)
                eff = this.transform.GetComponent<Effect>();

        }

        // Update is called once per frame
        void Update()
        {

            if (movingBack) BackTrack();
            if (!movingBack) nodeNumber += Time.deltaTime * speed;
            nodeI = Mathf.CeilToInt(nodeNumber);

            if (nodeI > track.nodes.Count - 1)
            {
                nodeNumber = 0;
                nodeI = 0;
            }

            if (onTrack)
            {
                eff.NoFollow(true);

                if (!movingBack)
                    if (nodeI > 0)
                    {
                        transform.position = new Vector3(
                            Mathf.Lerp(track.nodes[nodeI - 1].position.x, track.nodes[nodeI].position.x, 1f - nodeI + nodeNumber),
                            Mathf.Lerp(track.nodes[nodeI - 1].position.y, track.nodes[nodeI].position.y, 1f - nodeI + nodeNumber),
                            transform.position.z);
                    }

                if (facing)
                    for (int i = 0; i < partsList.Count; i++)
                    {
                        float dir = 0f;

                        switch (partsList[i].facing)
                        {
                            case TrainDirection.Out:
                                break;
                            case TrainDirection.In:
                                dir = 180f;
                                break;
                            case TrainDirection.Backward:
                                if (track.filpNormal) dir = 270;
                                else dir = 90;
                                break;
                            case TrainDirection.Forward:
                                if (track.filpNormal) dir = 90;
                                else dir = 270;
                                break;

                            default:
                                break;
                        }

                        if (nodeI > 0)
                        {
                            Vector3 vectorToTarget = (Vector3)(track.nodes[nodeI].normals);
                            float a = (Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg) - 90;

                            vectorToTarget = (Vector3)(track.nodes[nodeI - 1].normals);
                            float b = (Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg) - 90;

                            a = Mathf.LerpAngle(b, a, 1f - nodeI + nodeNumber);
                            Quaternion q = Quaternion.AngleAxis(a + dir, Vector3.forward);
                            partsList[i].part.rotation = q;
                        }
                    }

            }
        }

        public void OnTrackToggle()
        {
            if (onTrack)
            {
                onTrack = false;
                facing = false;
                movingBack = false;
                eff.NoFollow(false);
            }
            else
            {
                onTrack = true;
                facing = true;
                movingBack = true;
                nodeNumber = track.NearestNode((Vector2)transform.position);
                eff.NoFollow(true);
            }
        }

        public void BackTrack()
        {
            float dis = track.lenghtOfNode * speed;
            float tarDis = ((Vector3)track.nodes[nodeI].position - transform.position).sqrMagnitude;
            transform.position = transform.position + ((Vector3)track.nodes[nodeI].position - transform.position).normalized * dis * Time.fixedDeltaTime;
            if (((Vector3)track.nodes[nodeI].position - transform.position).sqrMagnitude >= tarDis)
            {
                movingBack = false;
            }

        }
    }
}