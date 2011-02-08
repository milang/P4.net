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
using System.Collections.Generic;
using System.Text;
using p4dn;

namespace P4API
{
    /// <summary>
    /// Represents a Perforce Map object.
    /// </summary>
    public class P4Map : IDisposable
    {
        private P4MapMaker      _map;

        /// <summary>
        /// Creates an empty Map.
        /// </summary>
        public P4Map()
        {
            // always use UTF8
            _map = new P4MapMaker(Encoding.UTF8);
        }

        /// <summary>
        /// Creates a P4Map object populated with the view defined by lines.
        /// </summary>
        /// <param name="lines">Lines in the Map view.</param>
        public P4Map(params string[] lines)
        {
            // always use UTF8
            _map = new P4MapMaker(Encoding.UTF8);
            Insert(lines);
        }        

        private P4Map(P4MapMaker mapmaker)
        {
            _map = mapmaker;
        }

        /// <summary>
        /// Joins two map objects to form the intersection of the two views.
        /// </summary>
        /// <param name="left">Left-hand map.</param>
        /// <param name="right">Right-hand map.</param>
        /// <returns>P4Map object of the intersection of the two maps.</returns>
        public static P4Map Join(P4Map left, P4Map right)
        {
            return new P4Map(P4MapMaker.Join(left._map, right._map));
        }

        /// <summary>
        /// Clears all view lines from the map.
        /// </summary>
        public void Clear()
        {
            _map.Clear();
        }

        /// <summary>
        /// Returns the number of view lines in the map.
        /// </summary>
        public int Count
        {
            get
            {
                return _map.Count();
            }
        }

        /// <summary>
        /// Returns true if the map has no lines.
        /// </summary>
        /// <returns>True if there are no view lines.</returns>
        public bool IsEmpty()
        {
            return (_map.Count() == 0);
        }

        /// <summary>
        /// Inserts a new view line at the end of the map.
        /// </summary>
        /// <param name="lines"></param>
        public void Insert(params string[] lines)
        {
            foreach (string line in lines)
            {
                _map.Insert(line);
            }
        }

        /// <summary>
        /// Inserts a new view line at the end of the map.
        /// </summary>
        /// <param name="left">Left-hand path of the view line.</param>
        /// <param name="right">Right-hand path of the view line.</param>
        public void Insert2(string left, string right)
        {
            _map.Insert(left, right);
        }

        /// <summary>
        /// Inserts a new view line at the end of the map.
        /// </summary>
        /// <param name="left">Left-hand paths of the view lines.</param>
        /// <param name="right">Right-hand paths of the view lines.</param>
        public void Insert2(IList<string> left, IList<string> right)
        {
            if (left.Count != right.Count) throw new ArgumentException("Left and Right arguments must be the same length.");
            for (int i = 0; i < left.Count; i++)
            {
                _map.Insert(left[i], right[i]);
            }
        }

        /// <summary>
        /// Translates a list of paths through the view.
        /// </summary>
        /// <param name="paths">Paths to be translated.</param>
        /// <returns>Translated paths.</returns>
        public IList<string> Translate(params string[] paths)
        {
            List<string> output = new List<string>();
            foreach (string path in paths)
            {
                string t = Translate(path);
                if (t != null) output.Add(t);
            }
            return output;
        }

        /// <summary>
        /// Translates a list of paths through the reverse view (left-to-right).
        /// </summary>
        /// <param name="paths">Paths to be translated.</param>
        /// <returns>Translated paths.</returns>
        public IList<string> TranslateReverse(params string[] paths)
        {
            List<string> output = new List<string>();
            foreach (string path in paths)
            {
                string t = TranslateReverse(path);
                if (t != null) output.Add(t);
            }
            return output;
        }

        /// <summary>
        /// Translates a path through the view.
        /// </summary>
        /// <param name="path">Path to be translated.</param>
        /// <returns>Translated path.</returns>
        public string Translate(string path)
        {
            return _map.Translate(path, true);
        }

        /// <summary>
        /// Translates a path through the reverse view (left-to-right).
        /// </summary>
        /// <param name="path">Path to be translated.</param>
        /// <returns>Translated path.</returns>
        public string TranslateReverse(string path)
        {
            return _map.Translate(path, false);
        }

        /// <summary>
        /// Determines if the path is contained in the view.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns>True when the view includes the path.</returns>
        public bool Includes(string path)
        {
            return (Translate(path) != null);
        }

        /// <summary>
        /// Determines if the view contains any of the specified paths.
        /// </summary>
        /// <param name="paths">Paths to check.</param>
        /// <returns>True if any of the paths are contained in the view.</returns>
        public bool IncludesAny(params string[] paths)
        {
            foreach (string path in paths)
            {
                if (Translate(path) != null) return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if the view contains all of the specified paths.
        /// </summary>
        /// <param name="paths">Paths to check.</param>
        /// <returns>True if all of the paths are contained in the view.</returns>
        public bool IncludesAll(params string[] paths)
        {
            foreach (string path in paths)
            {
                if (Translate(path) == null) return false;
            }
            return true;
        }

        /// <summary>
        /// Reverses the view lines.
        /// </summary>
        /// <returns>A P4Map object containing the reversed view.</returns>
        public P4Map Reverse()
        {
            P4Map map = new P4Map();
            map.Insert(_map.ToA());
            map._map.Reverse();
            return map;
        }

        /// <summary>
        /// Gets the left-hand side of the view lines.
        /// </summary>
        /// <returns>List containing the left-hand side of the view.</returns>
        public IList<string> Lhs()
        {
            return _map.Lhs();
        }

        /// <summary>
        /// Gets the right-hand side of the view lines.
        /// </summary>
        /// <returns>List containing the right-hand side of the view.</returns>
        public IList<string> Rhs()
        {
            return _map.Rhs();
        }

        /// <summary>
        /// Gets a list of the view lines.
        /// </summary>
        /// <returns>List fo strings containing each view line.</returns>
        public IList<string> ToArray()
        {
            return _map.ToA();            
        }

        /// <summary>
        /// Cleans up unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _map.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns string representation of the view.
        /// </summary>
        /// <returns>String representing the view.</returns>
        public override string ToString()
        {
            return _map.Inspect();
        }

        /// <summary>
        /// Calls dispose method if not already disposed.
        /// </summary>
        ~P4Map()
        {
            _map.Dispose();
        }
    }
}
