using System;
using System.Collections;
using System.Collections.Generic;
using CurvedUI;
using Assert = UnityEngine.Assertions.Assert;

namespace Fractalscape
{
    public sealed class PrimaryWindow : Window
    {
        private List<MenuItem> _items;
        private int _indx = 0;
        private int _offset = 1;
        private const int ItemsPerFrame = 3;
        private List<MenuItem> _currentItems;
        private bool _moveItems;

        public enum RefreshDirection
        {
            Left,
            Right
        }

        public override AppSession.BackButtonBehavior SetBackButtonBehavior()
        {
            return AppSession.BackButtonBehavior.ExitApp;
        }

        private void Awake()
        {
            _items = new List<MenuItem>();
            _currentItems = new List<MenuItem>();
        }

        public override void Setup()
        {
            AppSession.Skybox.LoadTexture(AppData.Ref.DefaultSkyBox);
            RenderNewItems();
        }

        public override void Cleanup()
        {
            if (WindowManager.Instance.NewWindow.GetType() != typeof(Menu))
            {
                HidePreviousItems(_currentItems);
                _indx = (_items.Count > ItemsPerFrame ? ItemsPerFrame : _items.Count) - 1;
                if (_items.Count > 1) _currentItems = LoadNewItems(_indx);
                RepositionItems();
                RenderNewItems(_currentItems);
                FlattenItems();
            }
        }

        private void HidePreviousItems(List<MenuItem> items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i] == null) continue;
                items[i].Hide();
            }
        }

        public void RenderNewItems(List<MenuItem> items)
        {
            if (_currentItems == null) _items = LoadNewItems(_indx);
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i] == null) continue;
                items[i].gameObject.SetActive(true);
            }
        }

        public void RenderNewItems()
        {
            for (var i = 0; i < _currentItems.Count; i++)
            {
                if (_currentItems[i] == null) continue;
                _currentItems[i].Reveal();
            }
        }

        public void Refresh(RefreshDirection direction)
        {
            switch (direction)
            {
                case RefreshDirection.Right:
                    if (_indx == _items.Count - 1) return;
                    _indx++;
                    break;
                case RefreshDirection.Left:
                    if (_indx - (ItemsPerFrame - 1) <= 0) return;
                    _indx--;
                    break;
            }
            HidePreviousItems(_currentItems);
            _currentItems = LoadNewItems(_indx);
            RepositionItems();
            RenderNewItems(_currentItems);
        }

        private List<MenuItem> LoadNewItems(int newIndx)
        {
            var upper = newIndx;
            var diff = upper - (ItemsPerFrame - 1);
            var lower = diff < 0 ? 0 : diff;
            var list = new List<MenuItem>();

            try
            {
                for (var j = lower; j <= upper; j++)
                {
                    list.Add(_items[j]);
                }
                return list;
            }
            catch (IndexOutOfRangeException e)
            {
                return list;
            }
        }

        public void AddItem(MenuItem item)
        {
            _items.Add(item);
            item.Setup(_items.Count > ItemsPerFrame
                ? (_items.Count - 1) % ItemsPerFrame
                : _items.Count - 1, transform);
        }

        public void AddItem(MenuItem item, int indx)
        {
            Assert.IsNotNull(item);

            _items.Insert(indx, item);
            item.Setup(_items.Count > ItemsPerFrame
                ? (indx) % ItemsPerFrame
                : indx, transform);
        }

        public void RemoveItem(MenuItem item)
        {
            if (_indx == _items.Count - 1) _indx--;
            _items.Remove(item);
            _currentItems = LoadNewItems(_indx);
            RepositionItems();
        }

        private void RepositionItems()
        {
            for (var i = 0; i < _currentItems.Count; i++)
            {
                if (!_currentItems[i].gameObject.activeSelf) _currentItems[i].gameObject.SetActive(true);
                _currentItems[i].UpdatePosition(i);
                _currentItems[i].GetComponent<CurvedUIVertexEffect>().SetDirty();
            }
        }

        public IEnumerator Populate(List<string> fractals)
        {
            for (var i = 0 ; i < fractals.Count; i++) //no it cannot be converted you lying piece of....
            {
                var go = AppData.Ref.Fractals[fractals[i]];
                AddItem(go.GetComponent<MenuItem>());
                yield return null;
            }
            _indx = ItemsPerFrame - 1 > _items.Count - 1 ? _items.Count - 1 : ItemsPerFrame - 1;
            yield return null;
            _currentItems = LoadNewItems(_indx);
            yield return null;
            RepositionItems();
        }

        public override string PartnerNavigationWindow()
        {
            return "DefaultNavigationWindow";
        }

        public override bool UseBlackBackground()
        {
            return false;
        }

        public MenuItem GetMenuItem(string item)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Sku != item || _items[i] == null) continue;
                return _items[i];
            }
            return null;
        }

        public List<T> CleanItems<T>(List<T> l)
        {
            var newL = new List<T>();

            for (var z = 0; z < l.Count; z++)
            {
                if (l[z] == null) continue;
                newL.Add(l[z]);
            }
            return newL;
        }

        public void InverseAdd(MenuItem item)
        {
            if (!_items.Contains(item)) return;
            var inverseWindow = WindowManager.Instance.GetWindow<PrimaryWindow>(WindowName == WindowNames.StoreWindow
                ? WindowNames.LibraryWindow
                : WindowNames.StoreWindow);
            item.gameObject.SetActive(false);

            if (inverseWindow.WindowName == WindowNames.LibraryWindow)
            {
                var iIndx = inverseWindow._items.Count - 1;
                inverseWindow.AddItem(item, iIndx);
            }
            else
            {
                inverseWindow.AddItem(item);
            }
            RemoveItem(item);
            RenderNewItems(_currentItems);
            inverseWindow.RenderNewItems();
            inverseWindow.RepositionItems();
        }

        public void FlattenItems()
        {
            foreach (var item in _currentItems)
            {
                item.GetComponent<CUI_ZChangeOnHover>().FlattenObj();
            }
        }
    }
}