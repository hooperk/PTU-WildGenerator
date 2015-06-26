using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Generator
{
    /// <summary>
    /// Base class of DoubleMap class, contains static methods relevant to the class for access without type denotation
    /// </summary>
    public abstract class DoubleMap : ICollection
    {
        private object _mapLock = new object();

        /// <summary>
        /// Static method to swap all of the key,value pairs in a dictionary and return a dictionary mapping the values to the original keys
        /// </summary>
        /// <typeparam name="TKey">Type of the keys in the original dictionary</typeparam>
        /// <typeparam name="TValue">Type of the values in the original dictionary</typeparam>
        /// <param name="target">The dictionary to create a mirror of</param>
        /// <returns>A dictionary mapping in the opposite direction to the original</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if any of the values in the original dictionary are null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the values in the original dictionary are not unique</exception>
        public static Dictionary<TValue, TKey> Swap<TKey,TValue>(Dictionary<TKey, TValue> target)
        {
            Dictionary<TValue, TKey> result = new Dictionary<TValue, TKey>(target.Count);
            foreach (TKey key in target.Keys)
            {
                result.Add(target[key], key);//use add instead of indexing so ArgumentException can be thrown for non-unique values
            }
            return result;
        }

        /// <summary>
        /// Overrides object.ToString
        /// </summary>
        /// <returns>Key:Value pairs of the dictionary</returns>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        /// Prints the list, optionally formatting the dictionary
        /// </summary>
        /// <param name="pretty">Whethr to format the output</param>
        /// <returns>String representation of the Key:Value pairs of the dictionary</returns>
        public abstract string ToString(bool pretty);

        /// <summary>
        /// Get the enumerator for the map
        /// </summary>
        /// <returns>Enumerator for the class</returns>
        public IEnumerator GetEnumerator()
        {
            return ((IDictionary)this).GetEnumerator();
        }

        /// <summary>
        /// Copy the elements of the collection to an array
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public abstract void CopyTo(Array array, int index);

        /// <summary>
        /// Return the number of pairs in the Dictionary
        /// </summary>
        public abstract int Count
        {
            get;
        }

        /// <summary>
        /// DoubleMap is not thread-safe
        /// </summary>
        public bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// A Lock for maintaining thread safety with the Dictionary
        /// </summary>
        public object SyncRoot
        {
            get { return _mapLock; }
        }

        /// <summary>
        /// Add a key:value pair to the dictionary
        /// </summary>
        /// <param name="key">Key of the pair to add</param>
        /// <param name="value">Value of the pair to add</param>
        public abstract void Add(object key, object value);

        /// <summary>
        /// Removes all items from the Dictioanry
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Check if the Dictionary contains a key
        /// </summary>
        /// <param name="key">Key to check the dictionary for</param>
        /// <returns>True if the key is present and false if it is not</returns>
        public abstract bool Contains(object key);

        /// <summary>
        /// The Dictionary is not fixed length
        /// </summary>
        public bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// The Dictionary can be written to
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Remove a matching item from the dictionary
        /// </summary>
        /// <param name="key">Key of the item to be removed</param>
        public abstract void Remove(object key);

        /// <summary>
        /// Index accessor for the dictionary
        /// </summary>
        public abstract object this[object key]
        { get; set; }
    }

    /// <summary>
    /// Class which provides 1:1 mapping of two keys of differing types
    /// </summary>
    /// <typeparam name="L">Refered to as the left key, maps to right value</typeparam>
    /// <typeparam name="R">Refered to as the right key, maps to left value</typeparam>
    [Serializable]
    public class DoubleMap<L,R> : DoubleMap, IDictionary<L, R>, ISerializable, IDeserializationCallback
    {
        private Dictionary<L,R> _left;
        [NonSerialized] private Dictionary<R,L> _right;

        /// <summary>
        /// Create a blank doubly mapped dictionary 
        /// </summary>
        public DoubleMap() : this(new Dictionary<L, R>()) { }

        /// <summary>
        /// Create the map from an existing Dictionary
        /// </summary>
        /// <param name="source">Dictionary object to mirror to make a doubly mapped dictionary</param>
        public DoubleMap(Dictionary<L,R> source){
            _left = source;
            _right = DoubleMap.Swap(_left);
        }

        /// <summary>
        /// Create the map from an existing Dictionary
        /// </summary>
        /// <param name="source">Dictionary object to mirror to make a doubly mapped dictionary</param>
        public DoubleMap(Dictionary<R, L> source)
        {
            _right = source;
            _left = DoubleMap.Swap(_right);
        }

        /// <summary>
        /// Make a map from another map
        /// </summary>
        /// <param name="source">Source map to take from</param>
        public DoubleMap(DoubleMap<L, R> source)
        {
            _left = source.GetLeft();
            _right = source.GetRight();
        }

        /// <summary>
        /// Deserialise the DoubleMap
        /// </summary>
        /// <param name="info">The SerializationInfo containing data.</param>
        /// <param name="context">The source <see cref="System.Runtime.Serialization.StreamingContext"/> for this serialization.</param>
        protected DoubleMap(SerializationInfo info, StreamingContext context)
        {
            _left = (Dictionary<L, R>)info.GetValue("dictionary", typeof(Dictionary<L, R>));
        }

        /// <summary>
        /// Populates a <see cref="System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination <see cref="System.Runtime.Serialization.StreamingContext"/> for this serialization.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("dictionary", _left, typeof(string));
        }

        void IDeserializationCallback.OnDeserialization(Object sender)
        {
            _right = DoubleMap.Swap(_left);
        }

        /// <summary>
        /// Add a pair to the map
        /// </summary>
        /// <param name="lkey">left key of mapping</param>
        /// <param name="rkey">right key of mapping</param>
        public void Add(L lkey, R rkey)
        {
            if (lkey == null)
                throw new ArgumentNullException("lkey is null.");
            if (rkey == null)
                throw new ArgumentNullException("rkey is null.");
            if (_left.ContainsKey(lkey))
                throw new ArgumentException("An element with the same lkey already exists in the DoubleMap object.");
            if (_right.ContainsKey(rkey))
                throw new ArgumentException("An element with the same rkey already exists in the DoubleMap object.");
            ActualAdd(lkey, rkey);
        }

        /// <summary>
        /// Add a Key:Value to the dictionary
        /// </summary>
        /// <param name="lkey">left portion of the pair</param>
        /// <param name="rkey">right portion of the pair</param>
        protected void ActualAdd(L lkey, R rkey)
        {
            _left.Add(lkey, rkey);
            _right.Add(rkey, lkey);
        }

        /// <summary>
        /// Remove existing pairs from both dictionaries if the keys are already used
        /// </summary>
        /// <param name="lkey">left portion of the pair</param>
        /// <param name="rkey">right portion of the pair</param>
        protected void AddExisting(L lkey, R rkey)
        {
            if (_left.ContainsKey(lkey))
                Remove(lkey);
            if (_right.ContainsKey(rkey))
                Remove(rkey);
            ActualAdd(lkey, rkey);
        }

        /// <summary>
        /// Remove a pair which matches the given left key
        /// </summary>
        /// <param name="key">key of item to be removed, found from left table</param>
        /// /// <returns>true if an item is removed</returns>
        public bool Remove(L key)
        {
            if (!LeftContains(key))
                return false;
            bool removed = _right.Remove(_left[key]);
            return _left.Remove(key) || removed;
        }

        /// <summary>
        /// Remove a pair which matches the given right key
        /// </summary>
        /// <param name="key">key of item to be removed, found from right table</param>
        /// <returns>true if an item is removed</returns>
        public bool Remove(R key)
        {
            return RemoveRight(key);
        }

        /// <summary>
        /// Remove an item which the right key matches
        /// </summary>
        /// <param name="key">key in the right dictionary to match</param>
        /// <returns>true if an item is removed</returns>
        public bool RemoveRight(R key)
        {
            if(!RightContains(key))
                return false;
            bool removed = _left.Remove(_right[key]);
            return _right.Remove(key) || removed;
        }
        /// <summary>
        /// Remove a given pair if and only if it exists
        /// </summary>
        /// <param name="lkey">left key to check</param>
        /// <param name="rkey">right key to check</param>
        /// <returns>true if the pair existed, false if it was not in the map</returns>
        public bool Remove(L lkey, R rkey)
        {
            if (LeftContains(lkey) && RightContains(rkey) && lkey.Equals(_right[rkey]))
            {
                _left.Remove(lkey);
                _right.Remove(rkey);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove all pairs matching the predicate
        /// </summary>
        /// <param name="pred">Predicate to check if a pair should be removed</param>
        public void RemoveSome(Predicate<KeyValuePair<L,R>> pred)
        {
            List<L> toRemove = new List<L>();
            foreach (KeyValuePair<L, R> pair in _left)
                if (pred(pair))
                    toRemove.Add(pair.Key);
            RemoveSome(toRemove);
        }

        /// <summary>
        /// Remove all of the matching keys from the collection
        /// </summary>
        /// <param name="keys">Collection of keys to be removed</param>
        public void RemoveSome(params L[] keys)
        {
            RemoveSome((IEnumerable<L>)keys);
        }

        /// <summary>
        /// Remove all of the matching keys from the collection
        /// </summary>
        /// <param name="keys">Collection of keys to be removed</param>
        public void RemoveSome(IEnumerable<L> keys)
        {
            foreach (L key in keys)
                Remove(key);
        }

        /// <summary>
        /// Remove all of the matching keys from the collection
        /// </summary>
        /// <param name="keys">Collection of keys to be removed</param>
        public void RemoveSome(params R[] keys)
        {
            RemoveSomeRight((IEnumerable<R>)keys);
        }

        /// <summary>
        /// Remove all of the matching keys from the collection
        /// </summary>
        /// <param name="keys">Collection of keys to be removed</param>
        public void RemoveSome(IEnumerable<R> keys)
        {
            RemoveSomeRight(keys);
        }

        /// <summary>
        /// Remove all of the matching right keys from the collection
        /// </summary>
        /// <param name="keys">Collection of keys to be removed</param>
        public void RemoveSomeRight(params R[] keys)
        {
            RemoveSomeRight((IEnumerable<R>)keys);
        }

        /// <summary>
        /// Remove all of the matching right keys from the collection
        /// </summary>
        /// <param name="keys">Collection of keys to be removed</param>
        public void RemoveSomeRight(IEnumerable<R> keys)
        {
            foreach (R key in keys)
                Remove(key);
        }

        /// <summary>
        /// Accesser to access from a left key, using to set calls the add method
        /// </summary>
        /// <param name="key">key to look up against right table</param>
        /// <returns>The R which the key maps to</returns>
        public R Left(L key) { return _left[key]; }

        /// <summary>
        /// Accesser to access from a right key, using to set calls the add method
        /// </summary>
        /// <param name="key">key to look up against right table</param>
        /// <returns>The L which the key maps to</returns>
        public L Right(R key) { return _right[key]; }

        /// <summary>
        /// Checks if the given key is in the left table
        /// </summary>
        /// <param name="key">key to check against</param>
        /// <returns>true if the key is in the table already</returns>
        public bool LeftContains(L key) { return _left.ContainsKey(key); }

        /// <summary>
        /// Checks if the given key is in the right table
        /// </summary>
        /// <param name="key">key to check against</param>
        /// <returns>true if the key is in the table already</returns>
        public bool RightContains(R key) { return _right.ContainsKey(key); }

        /// <summary>
        /// Accessor for the keys in the left table
        /// </summary>
        public Dictionary<L,R>.KeyCollection LeftKeys { get { return _left.Keys; } }

        /// <summary>
        /// Accessor for the keys in the right table
        /// </summary>
        public Dictionary<R,L>.KeyCollection RightKeys { get { return _right.Keys; } }

        /// <summary>
        /// Indexer to access from a left key, using to set calls the add method
        /// </summary>
        /// <param name="key">key to look up against left table</param>
        /// <returns>The R which the key maps to</returns>
        public R this[L key]
        {
            get { return Left(key); }
            set { AddExisting(key, value); }
        }

        /// <summary>
        /// Indexer to access from a right key, using to set calls the add method
        /// </summary>
        /// <param name="key">key to look up against right table</param>
        /// <returns>The L which the key maps to</returns>
        public L this[R key]
        {
            get { return Right(key); }
            set { AddExisting(value, key); }//key = R, value = L
        }

        /// <summary>
        /// Get the Left Dictionary from the mapping
        /// </summary>
        /// <returns>A Dictionary mapping left keys to right values</returns>
        public Dictionary<L,R> GetLeft()
        {
            return _left;
        }

        /// <summary>
        /// Get the Right Dictionary from the mapping
        /// </summary>
        /// <returns>A Dictionary mapping right keys to left values</returns>
        public Dictionary<R,L> GetRight(){
            return _right;
        }

        /// <summary>
        /// Tostring method to print the key value pairs as if left were the primary key
        /// </summary>
        /// <param name="pretty">To seperate each with a newline or a comma</param>
        /// <returns>String represnetation of the mapping</returns>
        public override string ToString(bool pretty = false)
        {
            StringBuilder output = new StringBuilder();
            int i = 0;
            foreach (L key in LeftKeys)
            {
                output.Append("(" + key + ":" + this[key] + ")");
                if (i < LeftKeys.Count - 1) output.Append((pretty ? Environment.NewLine : ", "));
                i++;
            }
            return output.ToString();
        }

        /// <summary>
        /// Check for equality based on both maps containing the exact same mappings, in any order
        /// </summary>
        /// <param name="obj">the other DoubleMap</param>
        /// <returns>true if both maps contain the same keys that map to the same values</returns>
        public override bool Equals(object obj)
        {
            DoubleMap<L, R> other = obj as DoubleMap<L, R>;
            if (other == null || this.LeftKeys.Count != other.LeftKeys.Count)
                return false;
            foreach (L key in this.LeftKeys)
                if (!this[key].Equals(other[key]))
                    return false;
            return true;
        }

        /// <summary>
        /// Create a Hashcode based on hashing all of the contents
        /// </summary>
        /// <returns>Somewhat unique hash code for the Map</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 27;
                foreach (L key in this.LeftKeys)
                    hashCode = (((13 * hashCode) + key.GetHashCode()) * 13) + this[key].GetHashCode();//cause a pair to create a code of its own
                return hashCode;//result is sum with overflow keeping length static
            }
        }

        /// <summary>
        /// Cast a doublemap to a Dictionary
        /// </summary>
        /// <param name="source">DoubleMap to becoem a normal dictionary</param>
        /// <returns>Dictionary of only one key to value mapping direction</returns>
        public static implicit operator Dictionary<L, R>(DoubleMap<L, R> source)
        {
            return source.GetLeft();
        }

        /// <summary>
        /// Cast a doublemap to a Dictionary
        /// </summary>
        /// <param name="source">DoubleMap to becoem a normal dictionary</param>
        /// <returns>Dictionary of only one key to value mapping direction</returns>
        public static implicit operator Dictionary<R,L>(DoubleMap<L,R> source){
            return source.GetRight();
        }

        /// <summary>
        /// Explicitly convert a Dictioanry to a DoubleMap in the same way as the constructor
        /// </summary>
        /// <param name="source">Dictionary to be mirrored for double mapping</param>
        /// <returns>DoubleMap mapping all pairs in the dictionary</returns>
        public static explicit operator DoubleMap<L, R>(Dictionary<L, R> source)
        {
            return new DoubleMap<L, R>(source);
        }

        /// <summary>
        /// Explicitly convert a Dictioanry to a DoubleMap in the same way as the constructor
        /// </summary>
        /// <param name="source">Dictionary to be mirrored for double mapping</param>
        /// <returns>DoubleMap mapping all pairs in the dictionary</returns>
        public static explicit operator DoubleMap<L, R>(Dictionary<R, L> source)
        {
            return new DoubleMap<L, R>(source);
        }
        /// <summary>
        /// Get the iterator for the Map
        /// </summary>
        /// <returns>Iterator to go over the Key:Value pairs</returns>
        public new IEnumerator<KeyValuePair<L, R>> GetEnumerator()
        {
            return _left.GetEnumerator();
        }

        /// <summary>
        /// Copy the elements of the collection to an array
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public override void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array is null.");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index is less than zero.");
            if (array.Rank > 1)
                throw new ArgumentException("array is multidimensional.");
            if (this.Count > array.Length - index)
                throw new ArgumentException("The number of elements in the source ICollection is greater than the available space from index to the end of the destination array.");
            foreach (KeyValuePair<L, R> pair in _left)
            {
                array.SetValue(pair, index++);
            }
        }

        /// <summary>
        /// Return the number of pairs in the Dictionary
        /// </summary>
        public override int Count
        {
            get { return _left.Count; }
        }

        /// <summary>
        /// Add a key:value pair to the dictionary
        /// </summary>
        /// <param name="item">pair to be added</param>
        public void Add(KeyValuePair<L, R> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes all items from the Dictioanry
        /// </summary>
        public override void Clear()
        {
            _left.Clear();
            _right.Clear();
        }

        /// <summary>
        /// Check to see if a given pair is in the Dictionary
        /// </summary>
        /// <param name="item">Pair to check the dictionary for</param>
        /// <returns>True if the pair is in the dictionary</returns>
        public bool Contains(KeyValuePair<L, R> item)
        {
            return LeftContains(item.Key) && this[item.Key].Equals(item.Value);
        }

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection. The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(KeyValuePair<L, R>[] array, int arrayIndex)
        {
            CopyTo((Array)array, arrayIndex);
        }

        /// <summary>
        /// Remove the given pair from the dictionary
        /// </summary>
        /// <param name="item">The pair to remove</param>
        /// <returns>True if the pair existed and was removed</returns>
        public bool Remove(KeyValuePair<L, R> item)
        {
            return Remove(item.Key, item.Value);
        }

        /// <summary>
        /// Add a key:value pair to the dictionary
        /// </summary>
        /// <param name="key">Key of the pair to add</param>
        /// <param name="value">Value of the pair to add</param>
        public override void Add(object key, object value)
        {
            Add((L)key, (R)value);
        }

        /// <summary>
        /// Check if the Dictionary contains the given key
        /// </summary>
        /// <param name="key">Key to check both sides for</param>
        /// <returns>True if either side contains the key</returns>
        public override bool Contains(object key)
        {
            if (key is L)
            {
                if (key is R)
                {

                    return LeftContains((L)key) || RightContains((R)key);
                }
                return LeftContains((L)key);
            }
            if (key is R)
            {
                return RightContains((R)key);
            }
            return false;
        }

        /// <summary>
        /// Remove the given key from the dictioanry
        /// </summary>
        /// <param name="key">Key to remove</param>
        public override void Remove(object key)
        {
            Remove((L)key);
        }

        /// <summary>
        /// Accessor for the map
        /// </summary>
        public override object this[object key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException("key is null.");
                if (key is L)
                    return this[(L)key];
                if (key is R)
                    return this[(R)key];
                throw new KeyNotFoundException("Key is not of valid type for this dictionary");
            }
            set
            {
                if (key == null) throw new ArgumentNullException("key is null.");
                if (key is L)
                    this[(L)key] = (R)value;
                if (key is R)
                    this[(R)key] = (L)value;
                throw new KeyNotFoundException("Key is not of valid type for this dictionary");
            }
        }

        /// <summary>
        /// Checks the left side for a key like this is a regular dictionary
        /// </summary>
        /// <param name="key">key to check for</param>
        /// <returns>True if the key is present</returns>
        public bool ContainsKey(L key)
        {
            return LeftContains(key);
        }

        /// <summary>
        /// Left side as if this was a regular dictionary
        /// </summary>
        public ICollection<L> Keys
        {
            get { return LeftKeys; }
        }

        /// <summary>
        /// Try to get a value from the dictioanry
        /// </summary>
        /// <param name="key">key to look for</param>
        /// <param name="value">Output value, default if key not found</param>
        /// <returns>true if the value is found and false otherwise</returns>
        public bool TryGetValue(L key, out R value)
        {
            try
            {
                if (LeftContains(key))
                {
                    value = Left(key);
                    return true;
                }
                value = default(R);
                return false;
            }
            catch (ArgumentNullException)
            {
                value = default(R);
                return false;
            }
            catch (KeyNotFoundException)
            {
                value = default(R);
                return false;
            }
        }

        /// <summary>
        /// Right side as if this was a regular dictionary
        /// </summary>
        public ICollection<R> Values
        {
            get { return RightKeys; }
        }

        /// <summary>
        /// Checks the rightt side for a key like this is a regular dictioanry
        /// </summary>
        /// <param name="value">key to check for</param>
        /// <returns>True if the key is present</returns>
        public bool ContainsValue(R value)
        {
            return RightContains(value);
        }
    }

    /// <summary>
    /// Class which provides 1:1 mapping of two keys of the same type
    /// </summary>
    /// <typeparam name="T">The type of item used i nthis map</typeparam>
    [Serializable]
    public class DoubleMap<T> : DoubleMap, IDictionary<T, T>, ISerializable, IDeserializationCallback
    {
        private Dictionary<T,T> _left;
        private Dictionary<T,T> _right;

        /// <summary>
        /// Create the tables
        /// </summary>
        public DoubleMap() : this(new Dictionary<T, T>()) { }

        /// <summary>
        /// Create the map from an existing Dictionary
        /// </summary>
        /// <param name="source">Dictionary object to mirror to make a doubly mapped dictionary</param>
        public DoubleMap(Dictionary<T, T> source)
        {
            _left = source;
            _right = DoubleMap.Swap(_left);
        }

        /// <summary>
        /// Make a map from another map
        /// </summary>
        /// <param name="source">Source map to take from</param>
        public DoubleMap(DoubleMap<T, T> source)
        {
            _left = source.GetLeft();
            _right = source.GetRight();
        }

        /// <summary>
        /// Deserialise the DoubleMap
        /// </summary>
        /// <param name="info">The SerializationInfo containing data.</param>
        /// <param name="context">The source <see cref="System.Runtime.Serialization.StreamingContext"/> for this serialization.</param>
        protected DoubleMap(SerializationInfo info, StreamingContext context)
        {
            _left = (Dictionary<T, T>)info.GetValue("dictionary", typeof(Dictionary<T, T>));
        }

        /// <summary>
        /// Populates a <see cref="System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination <see cref="System.Runtime.Serialization.StreamingContext"/> for this serialization.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("dictionary", _left, typeof(string));
        }

        void IDeserializationCallback.OnDeserialization(Object sender)
        {
            _right = DoubleMap.Swap(_left);
        }

        /// <summary>
        /// Add a pair to the map
        /// </summary>
        /// <param name="lkey">left key of mapping</param>
        /// <param name="rkey">right key of mapping</param>
        public void Add(T lkey, T rkey)
        {
            if(lkey == null)
                throw new ArgumentNullException("lkey is null.");
            if(rkey == null)
                throw new ArgumentNullException("rkey is null.");
            if (_left.ContainsKey(lkey))
                throw new ArgumentException("An element with the same lkey already exists in the DoubleMap object.");
            if (_right.ContainsKey(rkey))
                throw new ArgumentException("An element with the same rkey already exists in the DoubleMap object.");
            ActualAdd(lkey, rkey);
        }

        /// <summary>
        /// Add a Key:Value to the dictionary
        /// </summary>
        /// <param name="lkey">left portion of the pair</param>
        /// <param name="rkey">right portion of the pair</param>
        protected void ActualAdd(T lkey, T rkey)
        {
            _left.Add(lkey, rkey);
            _right.Add(rkey, lkey);
        }

        /// <summary>
        /// Remove existing pairs from both dictionaries if the keys are already used
        /// </summary>
        /// <param name="lkey">left portion of the pair</param>
        /// <param name="rkey">right portion of the pair</param>
        protected void AddExisting(T lkey, T rkey)
        {
            if (_left.ContainsKey(lkey))
                Remove(lkey);
            if (_right.ContainsKey(rkey))
                RemoveRight(rkey);
            ActualAdd(lkey, rkey);
        }

        /// <summary>
        /// Remove a pair which matches the given left key
        /// </summary>
        /// <param name="key">key of item to be removed, found from left table</param>
        public bool Remove(T key)
        {
            if (!LeftContains(key))
                return false;
            bool removed = _right.Remove(_left[key]);
            return _left.Remove(key) || removed;
        }

        /// <summary>
        /// Remove a given pair if and only if it exists
        /// </summary>
        /// <param name="lkey">left key to check</param>
        /// <param name="rkey">right key to check</param>
        /// <returns>true if the pair existed, false if ti was not in the map</returns>
        public bool Remove(T lkey, T rkey)
        {
            if (LeftContains(lkey) && RightContains(rkey) && lkey.Equals(_right[rkey]))
            {
                _left.Remove(lkey);
                _right.Remove(rkey);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove a pair which matches the given right key
        /// </summary>
        /// <param name="key">key of item to be removed, found from right table</param>
        public bool RemoveRight(T key)
        {
            if (!RightContains(key))
                return false;
            bool removed = _left.Remove(_right[key]);
            return _right.Remove(key) || removed;
        }

        /// <summary>
        /// Remove all pairs matching the predicate
        /// </summary>
        /// <param name="pred">Predicate to check if a pair should be removed</param>
        public void RemoveSome(Predicate<KeyValuePair<T, T>> pred)
        {
            List<T> toRemove = new List<T>();
            foreach (KeyValuePair<T, T> pair in _left)
                if (pred(pair))
                    toRemove.Add(pair.Key);
            RemoveSome(toRemove);
        }

        /// <summary>
        /// Remove all of the matching keys from the collection
        /// </summary>
        /// <param name="keys">Collection of keys to be removed</param>
        public void RemoveSome(params T[] keys)
        {
            RemoveSome((IEnumerable<T>)keys);
        }

        /// <summary>
        /// Remove all of the matching keys from the collection
        /// </summary>
        /// <param name="keys">Collection of keys to be removed</param>
        public void RemoveSome(IEnumerable<T> keys)
        {
            foreach (T key in keys)
                Remove(key);
        }

        /// <summary>
        /// Remove all of the matching right keys from the collection
        /// </summary>
        /// <param name="keys">Collection of keys to be removed</param>
        public void RemoveSomeRight(params T[] keys)
        {
            RemoveSomeRight((IEnumerable<T>)keys);
        }

        /// <summary>
        /// Remove all of the matching right keys from the collection
        /// </summary>
        /// <param name="keys">Collection of keys to be removed</param>
        public void RemoveSomeRight(IEnumerable<T> keys)
        {
            foreach (T key in keys)
                Remove(key);
        }

        /// <summary>
        /// Accesser to access from a left key, using to set calls the add method
        /// </summary>
        /// <param name="key">key to look up against right table</param>
        /// <returns>The T which the key maps to</returns>
        public T Left(T key) { return (T)_left[key]; }

        /// <summary>
        /// Accesser to access from a right key, using to set calls the add method
        /// </summary>
        /// <param name="key">key to look up against right table</param>
        /// <returns>The L which the key maps to</returns>
        public T Right(T key) { return (T)_right[key]; }

        /// <summary>
        /// Checks if the given key is in the left table
        /// </summary>
        /// <param name="key">key to check against</param>
        /// <returns>true if the key is in the table already</returns>
        public bool LeftContains(T key) { return _left.ContainsKey(key); }

        /// <summary>
        /// Checks if the given key is in the right table
        /// </summary>
        /// <param name="key">key to check against</param>
        /// <returns>true if the key is in the table already</returns>
        public bool RightContains(T key) { return _right.ContainsKey(key); }

        /// <summary>
        /// Accessor for the keys in the left table
        /// </summary>
        public Dictionary<T,T>.KeyCollection LeftKeys { get { return _left.Keys; } }

        /// <summary>
        /// Accessor for the keys in the right table
        /// </summary>
        public Dictionary<T,T>.KeyCollection RightKeys { get { return _right.Keys; } }

        /// <summary>
        /// Indexer to access from a right key, using to set calls the add method
        /// </summary>
        /// <param name="key">key to look up against right table</param>
        /// <returns>The L which the key maps to</returns>
        public T this[T key]
        {
            get { return Left(key); }
            set { AddExisting(key, value); }//key = R, value = L
        }

        /// <summary>
        /// Get the Left Dictionary from the mapping
        /// </summary>
        /// <returns>A Dictionary mapping left keys to right values</returns>
        public Dictionary<T, T> GetLeft()
        {
            return _left;
        }

        /// <summary>
        /// Get the Right Dictionary from the mapping
        /// </summary>
        /// <returns>A Dictionary mapping right keys to left values</returns>
        public Dictionary<T, T> GetRight()
        {
            return _right;
        }

        /// <summary>
        /// Tostring method to print the key value pairs as if left were the primary key
        /// </summary>
        /// <param name="pretty">To seperate each with a newline or a comma</param>
        /// <returns>String represnetation of the mapping</returns>
        public override string ToString(bool pretty = false)
        {
            StringBuilder output = new StringBuilder();
            int i = 0;
            foreach (T key in LeftKeys)
            {
                output.Append("(" + key + ":" + this[key] + ")");
                if (i < LeftKeys.Count - 1) output.Append((pretty ? Environment.NewLine : ", "));
                i++;
            }
            return output.ToString();
        }

        /// <summary>
        /// Check for equality based on both maps containing the exact same mappings, in any order
        /// </summary>
        /// <param name="obj">the other DoubleMap</param>
        /// <returns>true if both maps contain the same keys that map to the same values</returns>
        public override bool Equals(object obj)
        {
            DoubleMap<T> other = obj as DoubleMap<T>;
            if (other == null || this.LeftKeys.Count != other.LeftKeys.Count)
                return false;
            foreach (T key in this.LeftKeys)
                if (!this[key].Equals(other[key]))
                    return false;
            return true;
        }

        /// <summary>
        /// Create a Hashcode based on hashing all of the contents
        /// </summary>
        /// <returns>Somewhat unique hash code for the Map</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 27;
                foreach (T key in this.LeftKeys)
                    hashCode = (((13 * hashCode) + key.GetHashCode()) * 13) + this[key].GetHashCode();//cause a pair to create a code of its own
                return hashCode;//result is sum with overflow keeping length static
            }
        }

        /// <summary>
        /// Cast a doublemap to a Dictionary
        /// </summary>
        /// <param name="source">DoubleMap to becoem a normal dictionary</param>
        /// <returns>Dictionary of only one key to value mapping direction</returns>
        public static implicit operator Dictionary<T, T>(DoubleMap<T> source)
        {
            return source.GetLeft();
        }

        /// <summary>
        /// Explicitly convert a Dictioanry to a DoubleMap in the same way as the constructor
        /// </summary>
        /// <param name="source">Dictionary to be mirrored for double mapping</param>
        /// <returns>DoubleMap mapping all pairs in the dictionary</returns>
        public static explicit operator DoubleMap<T>(Dictionary<T, T> source)
        {
            return new DoubleMap<T>(source);
        }

        /// <summary>
        /// Get the iterator for the Map
        /// </summary>
        /// <returns>Iterator to go over the Key:Value pairs</returns>
        public new IEnumerator<KeyValuePair<T, T>> GetEnumerator()
        {
            return _left.GetEnumerator();
        }

        /// <summary>
        /// Copy the elements of the collection to an array
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection. The Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public override void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array is null.");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index is less than zero.");
            if (array.Rank > 1)
                throw new ArgumentException("array is multidimensional.");
            if (this.Count > array.Length - index)
                throw new ArgumentException("The number of elements in the source ICollection is greater than the available space from index to the end of the destination array.");
            foreach (KeyValuePair<T, T> pair in _left)
            {
                array.SetValue(pair, index++);
            }
        }

        /// <summary>
        /// Return the number of pairs in the Dictionary
        /// </summary>
        public override int Count
        {
            get { return _left.Count; }
        }

        /// <summary>
        /// Add a key:value pair to the dictionary
        /// </summary>
        /// <param name="item">pair to be added</param>
        public void Add(KeyValuePair<T, T> item)
        {
            AddExisting(item.Key, item.Value);
        }

        /// <summary>
        /// Removes all items from the Dictioanry
        /// </summary>
        public override void Clear()
        {
            _left.Clear();
            _right.Clear();
        }

        /// <summary>
        /// Check to see if a given pair is in the Dictionary
        /// </summary>
        /// <param name="item">Pair to check the dictionary for</param>
        /// <returns>True if the pair is in the dictionary</returns>
        public bool Contains(KeyValuePair<T, T> item)
        {
            return LeftContains(item.Key) && this[item.Key].Equals(item.Value);
        }

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection. The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(KeyValuePair<T, T>[] array, int arrayIndex)
        {
            CopyTo((Array)array, arrayIndex);
        }

        /// <summary>
        /// Remove the given pair from the dictionary
        /// </summary>
        /// <param name="item">The pair to remove</param>
        /// <returns>True if the pair existed and was removed</returns>
        public bool Remove(KeyValuePair<T, T> item)
        {
            return Remove(item.Key, item.Value);
        }

        /// <summary>
        /// Add a pair to the Dictionary
        /// </summary>
        /// <param name="key">Left value to add</param>
        /// <param name="value">Rigth value to add</param>
        public override void Add(object key, object value)
        {
            AddExisting((T)key, (T)value);
        }

        /// <summary>
        /// Check if the Dictionary contains the given key
        /// </summary>
        /// <param name="key">Key to check both sides for</param>
        /// <returns>True if either side contains the key</returns>
        public override bool Contains(object key)
        {
            if(key is T)
                return LeftContains((T)key) || RightContains((T)key);
            return false;
        }

        /// <summary>
        /// Remove the given key from the dictioanry
        /// </summary>
        /// <param name="key">Key to remove</param>
        public override void Remove(object key)
        {
            if(key is T)
                Remove((T)key);
        }

        /// <summary>
        /// Accessor for the map
        /// </summary>
        public override object this[object key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException("key is null.");
                if (key is T)
                    return this[(T)key];
                throw new KeyNotFoundException("Key is not of valid type for this dictionary");
            }
            set
            {
                if (key is T)
                    this[(T)key] = (T)value;
                throw new KeyNotFoundException("Key is not of valid type for this dictionary");
            }
        }

        /// <summary>
        /// Checks the left side for a key like this is a regular dictioanry
        /// </summary>
        /// <param name="key">key to check for</param>
        /// <returns>True if the key is present</returns>
        public bool ContainsKey(T key)
        {
            return LeftContains(key);
        }

        /// <summary>
        /// Left side as if this was a regular dictionary
        /// </summary>
        public ICollection<T> Keys
        {
            get { return LeftKeys; }
        }

        /// <summary>
        /// Try to get a value from the dictioanry
        /// </summary>
        /// <param name="key">key to look for</param>
        /// <param name="value">Output value, default if key not found</param>
        /// <returns>true if the value is found and false otherwise</returns>
        public bool TryGetValue(T key, out T value)
        {
            try
            {
                if (LeftContains(key))
                {
                    value = Left(key);
                    return true;
                }
                value = default(T);
                return false;
            }
            catch (ArgumentNullException)
            {
                value = default(T);
                return false;
            }
            catch (KeyNotFoundException)
            {
                value = default(T);
                return false;
            }
        }

        /// <summary>
        /// Right side as if this was a regular dictionary
        /// </summary>
        public ICollection<T> Values
        {
            get { return RightKeys; }
        }

        /// <summary>
        /// Checks the rightt side for a key like this is a regular dictioanry
        /// </summary>
        /// <param name="value">key to check for</param>
        /// <returns>True if the key is present</returns>
        public bool ContainsValue(T value)
        {
            return RightContains(value);
        }
    }
}
