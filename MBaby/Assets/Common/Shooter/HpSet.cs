using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Shooter
{
    [System.Serializable]
    public class HpSet
    {
        public string name = "HP";
        public float hp = 1;
        public int hpMax = 1;
        public Text hpText;
        public Image hpImage;

        public bool isTimer;
        public float regen;

        public HpSet(int _hpMax)
        {
            hpMax = _hpMax;
            hp = (float) hpMax;
        }

        public void UpdateUI()
        {
            if (hpText != null) hpText.text = (Mathf.CeilToInt(hp)).ToString();
            if (hpImage != null) hpImage.fillAmount = Mathf.Clamp01 ( hp / (float)hpMax);
        }
    }
}
