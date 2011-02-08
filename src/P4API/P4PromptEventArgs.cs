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

namespace P4API
{
    /// <summary>
    /// Arguments provided for an OnPrompt event.
    /// </summary>
    public class P4PromptEventArgs : EventArgs
    {
        private string _message = string.Empty;
        private string _response = string.Empty;
        
        //don't allow to be created outside this assembly
        internal P4PromptEventArgs(string Message)
        {
            _message = Message;
        }

        /// <summary>
        /// The message from Perforce.
        /// </summary>
        /// <value>The prompt message from Perforce.</value>
        public string Message
        {
            get
            {
                return _message;
            }
        }

        /// <summary>
        /// The value to respond to Perforce with.
        /// </summary>
        /// <value>The response to the Perforce command.</value>
        public string Response
        {
            get
            {
                return _response;
            }
            set
            {
                _response = value;
            }
        }
    }
}
