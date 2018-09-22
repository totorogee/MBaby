using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Shooter
{

    public class Hp : MonoBehaviour
    {

        public float zPos = -5f;
        public bool setSize = true;
        public Vector2 hpBarSize = new Vector2(1.5f, 0.3f);
        public bool follow = true;

        public Vector2 offset = Vector2.zero;
        public Transform hpBar;

        public List<HpSet> HpSets;

        // Use this for initialization
        void Start()
        {
            if (setSize)
                hpBar.localScale = new Vector3(hpBarSize.x, hpBarSize.y, 1f);
        }

        // Update is called once per frame
        void Update()
        {
            UpdateOffset();
            UpdateHpUI();
        }

        public void UpdateOffset(bool byDebug = false)
        {
            hpBar.transform.position = this.transform.position + (Vector3)offset + new Vector3(0, 0, zPos);
            if ((byDebug) && (setSize)) hpBar.localScale = new Vector3(hpBarSize.x, hpBarSize.y, 1f);
        }

        public void UpdateHpUI()
        {
            if(HpSets != null)
            {
                for (int i = 0; i < HpSets.Count; i++)
                {
                    HpSets[i].UpdateUI();
                }
            }
        }

    }

}
