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
    /// This class represents the "unparsed" output from a Perforce command.
    /// </summary>
    /// <remarks>
    /// In general P4UnParsedRecordSet will return an array of strings which is analagous 
    /// to a line printed to stdout in p4.exe.  Errors and Warnings are stored in the 
    /// Errors and Warnings properties respectively.
    /// </remarks>
    public class P4UnParsedRecordSet : P4BaseRecordSet, IEnumerable
    {
        internal P4UnParsedRecordSet() { }
        private string[] m_outputs;
        private string[] _Outputs
        {
            get
            {
                if (m_outputs == null)
                {
                    m_outputs = StringOutputs.ToArray();
                }
                return m_outputs;
            }
        }
        /// <summary>
        /// Gets array of messages returned from the Perforce command.
        /// </summary>
        /// <value>Array of messages returned from Perforce.</value>
        public new string[] Messages
        {
            get
            {
                return _Outputs;
            }
        }

        /// <summary>
        /// Gets message at the specified index.
        /// </summary>
        /// <remarks>
        /// This is the same as running record.Messages[0];
        /// </remarks>
        /// <param name="index">Index of the message.</param>
        /// <returns>String message</returns>
        /// <value>Message at 'index'.</value>
        public string this[int index]  //indexer
        {
            get
            {
                return _Outputs[index];
                
            }
        }


        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Outputs.GetEnumerator();
        }

        #endregion
    }
}
