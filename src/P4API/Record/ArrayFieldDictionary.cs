/*
 * P4.Net *
Copyright (c) 2007-2010 Shawn Hladky

Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
and associated documentation files (the "Software"), to deal in the Software without 
restriction, including without limitation the rights to use, copy, modify, merge, publish, 
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the 
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or 
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */


using System.Collections;

namespace P4API
{
    /// <summary>
    /// Strongly typed dictionary to represent array-valued fields returned from Perforce commands.
    /// </summary>
    /// <remarks>
    /// The FieldDictionary only contains fields that contain a string array values.  Fields that return a single string
    /// values are stored in <see cref="FieldDictionary"/>.<br/>
    /// If a value is read from a key that does not exist, an empty array will be returned.
    /// If a value is set with a key that does not exist, it will automatically be added.
    /// This behavior is to support many Perforce commands that omit a key, rather than supply a default value.
    /// </remarks>
    public class ArrayFieldDictionary
    {
        private Hashtable _ht;

        internal ArrayFieldDictionary()
        {
            _ht = new Hashtable();
        }

        internal void Add(string key, string[] value)
        {
            _ht.Add(key, value);
        }

        /// <summary>
        /// Clears all elements of the dictionary.
        /// </summary>
        public void Clear()
        {
            _ht.Clear();
        }

        /// <summary>
        /// Tests if the key exists in the dictionary.
        /// </summary>
        /// <param name="key">The key to test</param>
        /// <returns>True if the key is defined in the dictionary.</returns>
        public bool ContainsKey(string key)
        {
            return _ht.Contains(key);
        }

        /// <summary>
        /// Gets all keys contained in the dictionary.
        /// </summary>
        /// <value>Keys in the ArrayFieldDictionary.</value>
        public string[] Keys
        {
            get
            {
                string[] ret = new string[_ht.Count];
                int i = 0;
                foreach (string s in _ht.Keys)
                {
                    ret[i] = s;
                    i++;
                }
                return ret;
                
            }
        }

        /// <summary>
        /// Removes elements from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        public void Remove(string key)
        {
            _ht.Remove(key);
        }

        /// <summary>
        /// Gets the number of elements in the dictionary
        /// </summary>
        /// <value>Number of items.</value>
        public int Count
        {
            get
            {
                return _ht.Count;
            }
        }

        /// <summary>
        /// Returns the value assocatied to the key.
        /// </summary>
        /// <param name="key">The key to search on.</param>
        /// <returns>String array value associated to the key.</returns>
        public string[] this[string key]
        {
            get
            {
                return (string[]) _ht[key];
            }
            set
            {
                //Many p4 form commands do not have all the fields by default.
                //this will auto-add that key when you try to set a value.
                if (_ht.ContainsKey(key))
                {
                    _ht[key] = value;
                }
                else
                {
                    _ht.Add(key, value);
                }
            }
        }
    }
}
