﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Hides items when clicked/toggled
    /// </summary>
    public class UiCollapsible : MonoBehaviour
    {
        public TMP_Text title;
        public List<GameObject> items;
        public bool visible = true;
        public RectTransform checkmarkIcon;
        
        /// <summary>
        /// Add an item to the group, visibility is immediately set
        /// </summary>
        public void AddItem(GameObject item)
        {
            items.Add(item);
            item.SetActive(visible);
        }

        /// <summary>
        /// Change the visibility, callable from UI Event OnClick
        /// </summary>
        public void SetVisibility(bool value)
        {
            if (value != visible)
            {
                foreach (var item in items)
                    item.SetActive(value);

                if (checkmarkIcon)
                    checkmarkIcon.Rotate(new Vector3(0, 0, value ? -90 : 90));
            }
            visible = value;
        }
    }
}
