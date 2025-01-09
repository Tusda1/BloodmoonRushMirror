using System;

namespace Utilities {
    public class IndexedArray<T> {
        private readonly T[] _items;
        public int CurrentIndex { get; private set; }
        public int PreviousIndex { get; private set; }

        public T CurrentItem => _items[CurrentIndex];
        public T PreviousItem => _items[PreviousIndex];

        public event Action<T> OnIndexChanged = delegate { };
        public event Action OnMinIndex = delegate { };
        public event Action NotOnMinIndex = delegate { };
        public event Action OnMaxIndex = delegate { };
        public event Action NotOnMaxIndex = delegate { };

        public IndexedArray(T[] items) {
            if (items == null || items.Length == 0) {
                throw new ArgumentException("Array cannot be null or empty.");
            }

            _items = items;
            CurrentIndex = 0;
        }

        public void SetIndex(int index) {
            if (index < 0 || index >= _items.Length) {
                throw new IndexOutOfRangeException("Index is out of bounds.");
            }

            PreviousIndex = CurrentIndex;
            CurrentIndex = index;

            OnIndexChanged.Invoke(_items[CurrentIndex]);

            if (index == 0) {
                OnMinIndex.Invoke();
            }
            else {
                NotOnMinIndex.Invoke();
            }

            if (index == _items.Length - 1) {
                OnMaxIndex.Invoke();
            }
            else {
                NotOnMaxIndex.Invoke();
            }
        }
        
        public void DecreaseIndex() {
            SetIndex(CurrentIndex - 1);
        }

        public void IncreaseIndex() {
            SetIndex(CurrentIndex + 1);
        }
    }
}