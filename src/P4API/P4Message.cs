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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace P4API
{
    /// <summary>
    /// Perforce Message Severity
    /// </summary>
    public enum P4MessageSeverity
    {
        /// <summary>
        /// No Error
        /// </summary>
        Empty   = 0,
        /// <summary>
        /// Informational Message
        /// </summary>
        Info    = 1,
        /// <summary>
        /// Warning Message
        /// </summary>
        Warning = 2,
        /// <summary>
        /// Error Message
        /// </summary>
        Failed  = 3,
        /// <summary>
        /// Fatal Message
        /// </summary>
        Fatal   = 4
    }
    /// <summary>
    /// 
    /// </summary>
    public class P4Message
    {
        private SortedDictionary<string, string> _vars = null;
        private string _format = null;
        private int _id = 0;
        private P4MessageSeverity _severity;

        internal P4Message(p4dn.Error error)
        {
            // cache all the data from the error so we don't have to implement IDisposable
            _vars = new SortedDictionary<string, string>();
            error.GetVariables(_vars);
            _id = error.GetErrorID().code;
            _severity = (P4MessageSeverity) error.GetErrorID().Severity();
            _format = error.Fmt();

        }


        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return this.Format();
        }

        /// <summary>
        /// Gets the severity.
        /// </summary>
        /// <value>The severity.</value>
        public P4MessageSeverity Severity
        {
            get
            {
                return _severity;
            }

        }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <returns></returns>
        public string Format()
        {
            return _format;
        }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>The variables.</value>
        public string[] Variables
        {
            get
            {
                string[] ret = new string[_vars.Keys.Count];
                int i = 0;
                foreach (string s in _vars.Keys)
                {
                    ret[i] = s;
                    i++;
                }
                return ret;
            }
        }

        /// <summary>
        /// Dumps this instance.
        /// </summary>
        public void Dump()
        {
            switch (this.Severity)
            {
                case P4MessageSeverity.Info:
                    Trace.Write("... info ");
                    break;

                case P4MessageSeverity.Warning:
                    Trace.Write("... warn ");
                    break;

                case P4MessageSeverity.Fatal:
                    Trace.Write("... erro ");
                    break;

                case P4MessageSeverity.Failed:
                    Trace.Write("... erro ");
                    break;
            }
            Trace.Write(string.Format("<{0}> {1}\n", this.Identity, this.ToString()));
        }

        /// <summary>
        /// Gets the identity.
        /// </summary>
        /// <value>The identity.</value>
        public int Identity
        {
            get
            {
                return _id;
            }
        }
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="var">The var.</param>
        /// <returns></returns>
        public string GetValue(string var)
        {
            return _vars[var];
        }
    }
}
