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


using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace P4API
{
    /// <summary>
    /// P4Record is a dictionary-like object that provides a means of interpreting Perforce results.
    /// </summary>
    /// <remarks>
    /// The Perforce api provides "parsed" output in the form of key-value pairs.  
    /// Keys are always strings (case sensitive).  From the raw api, the values come in 3 flavors:
    /// <list>
    ///     <li>Simple strings.</li>
    ///     <li>List of strings.</li>
    ///     <li>Sparse multi-demensional list of strings.</li>    
    /// </list>
    /// P4.Net does not yet handle the third type (which only comes from certain output of p4 filelog).
    /// <para>To access simple string values by key, use the Fields property (or the default indexer).</para>
    /// <para>To access lists of strings by key, use the ArrayFields property.</para>
    /// </remarks>
    public class P4Record
    {
        internal FieldDictionary _Fields = null;
        internal ArrayFieldDictionary _ArrayFields = null;
        char[] digits  = {'0','1','2','3','4','5','6','7','8','9'};
        char[] digits_coma = { ',', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        private Dictionary<string, string> _allFields = null;
        private Dictionary<string, object> _allVars = null;

        internal P4Record(Dictionary<string, string> sd)
        {
            Reset(sd);
        }

        private bool isDigit(char c)
        {
            return (c >= '0' && c <= '9');
        }

        private bool isArray(string value, out string baseName)
        {
            baseName = value.TrimEnd(digits);
            if (isDigit(value[value.Length - 1]))
            {
                if (_allFields.ContainsKey(baseName + "0"))
                {
                    return true;
                }
                else if (baseName == "fileSize")
                {
                    // fileSize has some stange behavior.  In a filelog command, a file that is deleted
                    // at the head revision will not have a fileSize0 field.  This blows up my logic,
                    // so I handle it here as a special case
                    _allFields.Add("fileSize0", "");
                    return true;
                }
            }
            return false;
        }

        private bool isList(string value, Dictionary<string, string> sd,  out string baseName)
        {
            baseName = value.TrimEnd(digits_coma);
            string suffix = value.Substring(baseName.Length);
            if (isDigit(value[value.Length - 1]))
            {
                if (sd.ContainsKey(baseName + "0"))
                {
                    return true;
                }
                else if (baseName == "fileSize")
                {
                    // fileSize has some stange behavior.  In a filelog command, a file that is deleted
                    // at the head revision will not have a fileSize0 field.  This blows up my logic,
                    // so I handle it here as a special case
                    sd.Add("fileSize0", "");
                    return true;
                }
            }
            return false;
        }

        private void processDictionary(Dictionary<string, string> sd)
        {
            _allVars = new Dictionary<string, object>();
            // clone the keys array, b/c we may be changing the hashtable within the loop
            string[] keys = new string[sd.Keys.Count];
            sd.Keys.CopyTo(keys, 0);

            foreach (string key in keys)
            {
                string baseName;
                baseName = key.TrimEnd(digits_coma);
                if (!_allVars.ContainsKey(baseName))
                {
                    if (baseName != key)
                    {
                        string suffix = key.Substring(baseName.Length);
                        if (suffix.Contains(","))
                        {
                        }

                        processArray(baseName);
                    }
                    else
                    {
                        _allVars.Add(key, sd[key]);
                    }
                }
            }
        }

        private void getBaseName(string key)
        {

        }

        internal void Reset(Dictionary<string, string> sd)
        {
            _allFields = sd;
            _Fields = new FieldDictionary();
            _ArrayFields = new ArrayFieldDictionary();

            // clone the keys array, b/c we may be changing the hashtable within the loop
            string[] keys = new string[_allFields.Keys.Count];
            _allFields.Keys.CopyTo(keys, 0);

            foreach (string s in keys)
            {
                string baseName;
                if (isArray(s, out baseName))
                {
                    processArray(baseName);
                }
                else
                {
                    _Fields.Add(s, _allFields[s]);
                }
            }
        }

        private List<string> parseList(string baseName, Dictionary<string, string> sd)
        {
            List<string> list = new List<string>();
            for (int i = 0; ; i++)
            {
                string key = string.Format("{0}{1}", baseName, i);

                // fileSize will get us here too.  If a file is deleted and re-added,
                // the a filelog command will skip some numbers on the fileSize                    
                if (!sd.ContainsKey(key))
                {
                    // peek ahead one file to see if one happend to be skipped
                    string key2 = string.Format("{0}{1}", baseName, i + 1);
                    if (sd.ContainsKey(key2))
                    {
                        list.Add(null);
                    }
                    else
                    {
                        break;
                    }
                }
                list.Add(sd[key]);
            }
            return list;
        }

        private List<List<string>> parseList2(string baseName, Dictionary<string, string> sd)
        {
            List<List<string>> list = new List<List<string>>();
            for (int i = 0; ; i++)
            {
                List<string> innerList = new List<string>();
                for (int j = 0; ; j++)
                {
                    string key = string.Format("{0}{1},{2}", baseName, i,j);
              
                    if (!sd.ContainsKey(key))
                    {
                        break;
                    }
                    innerList.Add(sd[key]);
                }
                list.Add(innerList);
            }
            return list;
        }

        private void processArray(string baseName)
        {
            
            if (!_ArrayFields.ContainsKey(baseName))
            {
                List<string> list = new List<string>();
                for (int i = 0; ; i++)
                {
                    string key = string.Format("{0}{1}", baseName, i);

                    // fileSize will get us here too.  If a file is deleted and re-added,
                    // the a filelog command will skip some numbers on the fileSize                    
                    if (!_allFields.ContainsKey(key))
                    {
                        // peek ahead one file to see if one happend to be skipped
                        string key2 = string.Format("{0}{1}", baseName, i + 1);
                        if (_allFields.ContainsKey(key2))
                        {
                            list.Add("");
                        }
                        else
                        { 
                            break;
                        }
                    }
                    list.Add(_allFields[key]);
                }
                _ArrayFields.Add(baseName, list.ToArray());
            }            
        }

        /// <summary>
        /// Gets the FieldDictionary returned from the Perforce command.
        /// </summary>
        /// <value>The single-value fields returned for the current record.</value>
        public FieldDictionary Fields
        {
            get
            {
                return _Fields;
            }
        }

        //public T GetVar<T>(string key) where T:Object
        //{
        //    return _allFields[key] as T;
        //}

        /// <summary>
        /// Gets the ArrayFieldDictionary returned from the Perforce command.
        /// </summary>
        /// <value>The array-value fields returned for the current record.</value>
        public ArrayFieldDictionary ArrayFields
        {
            get
            {
                return _ArrayFields;
            }
        }

        /// <summary>
        /// Returns the value of of the Field by key.  This is the same as Fields[key].
        /// </summary>
        /// <param name="key">The key for </param>
        /// <returns>String value for the associated key.</returns>
        /// <value>The value for field 'key'.</value>
        public string this[string key]
        {
            get
            {
                return _Fields[key];
            }
            set
            {
                _Fields[key] = value;
            }
        }

        internal Dictionary<string, string> AllFieldDictionary
        {
            get 
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();

                // Add the scalar fields
                foreach (string key in _Fields.Keys)
                {
                    ret.Add(key, _Fields[key]);
                }

                // Add the array fields
                foreach (string key in _ArrayFields.Keys)
                {
                    for (int i = 0; i < _ArrayFields[key].Length; i++)
                    {
                        ret.Add(string.Format("{0}{1}", key, i), _ArrayFields[key][i]);
                    }
                    
                }
                return ret;
            }

        }


    }
}
