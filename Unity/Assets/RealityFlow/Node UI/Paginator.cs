using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RealityFlow.NodeUI
{
    public class Paginator : MonoBehaviour
    {
        public GameObject content;
        public StatefulInteractable leftButton;
        public StatefulInteractable rightButton;
        public TMP_Text pageCounter;
        [SerializeField]
        GameObject pagedElementPrefab;
        public GameObject PagedElement
        {
            get => pagedElementPrefab;
            set
            {
                pagedElementPrefab = value;
                InitPool();
            }
        }
        [SerializeField]
        int itemCount;
        public int ItemCount
        {
            get => itemCount;
            set
            {
                itemCount = value;
                InitPool();
            }
        }
        [SerializeField]
        int countPerPage = 10;
        public int CountPerPage
        {
            get => countPerPage;
            set
            {
                countPerPage = value;
                InitPool();
            }
        }

        Action<GameObject, int> onShow;
        public Action<GameObject, int> OnShow
        {
            get => onShow;
            set
            {
                onShow = value;
                InitPool();
            }
        }

        readonly List<GameObject> pool = new();
        int page = 0;
        int totalPages = 0;

        public void NextPage()
        {
            if (page + 1 >= totalPages)
                return;

            page += 1;

            ShowPage();
        }

        public void PrevPage()
        {
            if (page == 0)
                return;

            page -= 1;

            ShowPage();
        }

        void ShowPage()
        {
            if (pagedElementPrefab == null || onShow == null)
                return;

            int count = CountPerPage;
            int start = page * count;
            for (int i = 0; i < count; i++)
            {
                GameObject element = pool[i];
                int index = start + i;

                if (index < ItemCount)
                {
                    element.SetActive(true);
                    onShow(element, index);
                }
                else
                    pool[i].SetActive(false);
            }

            SetPageText();
            SetButtonInteractable();
        }

        void OnEnable()
        {
            leftButton.enabled = false;
            rightButton.enabled = false;
        }

        void InitPool()
        {
            if (pagedElementPrefab == null)
                return;

            for (int i = 0; i < pool.Count; i++)
                Destroy(pool[i]);
            pool.Clear();
            for (int i = 0; i < CountPerPage; i++)
                pool.Add(Instantiate(pagedElementPrefab, content.transform));

            totalPages = Mathf.CeilToInt((float)ItemCount / CountPerPage);

            if (page >= totalPages)
                page = Math.Max(0, totalPages - 1);

            ShowPage();
        }

        void SetPageText()
        {
            pageCounter.text = $"{page + 1} / {totalPages}";
        }

        void SetButtonInteractable()
        {
            leftButton.enabled = page != 0;
            rightButton.enabled = page < totalPages - 1;
        }
    }
}