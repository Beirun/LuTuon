using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageManager : MonoBehaviour
{
    [Serializable]
    public class Page
    {
        public List<GameObject> pages;
        public Button previous;
        public Button next;

        [NonSerialized] public int idx;
    }

    [Header("Pages")]
    public List<Page> pageSets = new();

    void Start()
    {
        for (int i = 0; i < pageSets.Count; i++)
            Init(pageSets[i]);
    }

    void Init(Page p)
    {
        if (p.pages == null || p.pages.Count == 0)
            return;

        p.idx = 0;

        for (int i = 0; i < p.pages.Count; i++)
            p.pages[i].SetActive(i == 0);

        p.previous.onClick.AddListener(() => Prev(p));
        p.next.onClick.AddListener(() => Next(p));

        UpdateButtons(p);
    }

    void Next(Page p)
    {
        if (p.idx >= p.pages.Count - 1)
            return;

        p.pages[p.idx].SetActive(false);
        p.idx++;
        p.pages[p.idx].SetActive(true);

        UpdateButtons(p);
    }

    void Prev(Page p)
    {
        if (p.idx <= 0)
            return;

        p.pages[p.idx].SetActive(false);
        p.idx--;
        p.pages[p.idx].SetActive(true);

        UpdateButtons(p);
    }

    void UpdateButtons(Page p)
    {
        p.previous.interactable = p.idx > 0;
        p.next.interactable = p.idx < p.pages.Count - 1;
    }
}
