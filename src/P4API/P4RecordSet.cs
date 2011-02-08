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
using System.Collections;
using System.Text;

namespace P4API
{
    /// <summary>
    /// Provides output of a Perforce Command in "tagged" moded.
    /// </summary>
    /// <remarks>
    /// P4Recordset is an enumerable collection of P4Records that supply keyed access to 
    /// information returned from Perforce.
    /// </remarks>
    public class P4RecordSet : P4BaseRecordSet, IEnumerable
    {
        internal P4RecordSet() { }
        private P4Record[] m_results;
        private P4Record[] _Results
        {
            get
            {
                if (m_results == null)
                {
                    m_results = TaggedOutputs.ToArray();
                }
                return m_results;
            }
        }

        /// <summary>
        /// Gets an array of records returned from the Perforce command.
        /// </summary>
        /// <value>Array of P4Records.</value>
        public P4Record[] Records
        {
            get
            {
                return (P4Record[])_Results.Clone();
            }
        }

        /// <summary>
        /// Gets an array of string messages returned from the Perforce command.
        /// </summary>
        /// <value>Array of informational messages returned from a Perforce command.</value>
        public new string[] Messages
        {
            get
            {
                return (base.StringOutputs.ToArray());
            }
        }

        /// <summary>
        /// Gets the record at the specified index.
        /// </summary>
        /// <param name="Index">Index of the record to get.</param>
        /// <returns>P4Record</returns>
        /// <value>Returns the record at 'Index'.</value>
        public P4Record this[int Index] //Indexer
        {
            get
            {
                return _Results[Index];
            }
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Results.GetEnumerator();
        }

        #endregion
    }
}
