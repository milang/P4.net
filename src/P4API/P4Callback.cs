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
using System.IO;

namespace P4API
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class P4Callback
    {
        private Encoding _encoding = null;
        internal void SetEncoding(Encoding encoding)
        {
            _encoding = (Encoding)encoding.Clone();
        }

        private string _specDef = null;
        internal void SetSpecDef(string specDef)
        {
            _specDef = specDef;
        }
        /// <summary>
        /// Gets the specdef for a form if it exists.
        /// </summary>
        /// <value>
        /// A specdef is a specially coded character string that Perforce uses internally to define the 
        /// structure of fields in a form.
        /// </value>
        protected string SpecDef
        {
            get
            {
                return _specDef;
            }
        }

        /// <summary>
        /// Gets the content encoding.
        /// </summary>
        /// <value>The content encoding is either UTF-8 for Unicode-enabled servers or ASCII otherwise.</value>
        protected Encoding ContentEncoding
        {
            get
            {
                return _encoding;
            }
        }

        /// <summary>
        /// Executed when a command requests an editor to launch.
        /// </summary>
        /// <param name="f1">The file on the local system to launch in the editor.</param>
        public virtual void Edit(FileInfo f1)
        {
        }

        /// <summary>
        /// Executed when the Perforce command needs to "prompt" the user for a response. 
        /// </summary>
        /// <param name="message">The message output from the Perforce command.</param>
        /// <param name="response">The response sent back to Perforce.</param>
        public virtual void Prompt(string message, ref string response)
        {
        }

        /// <summary>
        /// Executes when the command is finished.
        /// </summary>
        public virtual void Finished()
        {
            // do nothing
        }
        /// <summary>
        /// Allows consumers to cancel the current command.
        /// </summary>
        /// <returns>Return true to kill the current command.</returns>
        /// <remarks>
        /// The cancel method is called periodically during long-running commands.  
        /// You can't always guarantee when it will be called, or how quickly it will cancel the command.
        /// </remarks>
        public virtual bool Cancel()
        {
            return false;
        }

        /// <summary>
        /// Executed when a command outputs file content.
        /// </summary>
        /// <param name="buffer">A buffer containing a chunk of data.</param>
        /// <param name="IsText">If set to <c>true</c> [is text].</param>
        /// <remarks>
        /// OutputContent is typically called from the 'print' command, but can also be called from 'describe', 
        /// 'diff', or 'diff2'.
        /// Output content may be called several times for a single file if it is large.  You must orchestrate 
        /// between calls to OutputMessage and OutputContent to know which file you are working with.
        /// If the content is textual, you can use the ContentEncoding property to convert to text.
        /// </remarks>
        public virtual void OutputContent(byte[] buffer, bool IsText)
        {
        }

        /// <summary>
        /// Executed when a command expects a file buffer input.
        /// </summary>
        /// <param name="buffer">A reference to a buffer.  Append strings to the buffer to send file content to the server.</param>
        /// <remarks>
        /// InputData is called from form input commands.  In general it's easier to use the P4Form class to manipulate forms than 
        /// handling through the P4Callback interface.
        /// </remarks>
        public virtual void InputData(StringBuilder buffer)
        {
        }
        /// <summary>
        /// Executed when tagged output is streamed from the server.
        /// </summary>
        /// <param name="record">The tagged output in the form of a P4Record.</param>
        public virtual void OutputRecord(P4Record record)
        {
        }
        /// <summary>
        /// Executed when a textual message is streamed from the server.
        /// </summary>
        /// <param name="message">The message.</param>
        public abstract void OutputMessage(P4Message message);

        /// <summary>
        /// Executed when a textual message is output from the underlying API.
        /// </summary>
        /// <param name="data">The textual message.</param>
        /// <remarks>
        /// The majority of commands output messages from OutputMessage, but there are some commands that ocasionally
        /// call OutputInfo without the full data of the message string.
        /// </remarks>
        public virtual void OutputInfo(string data)
        {
        }
        /// <summary>
        /// Executed when a 'resolve' command seeks input on how to resolve a file.
        /// </summary>
        /// <param name="mergeData">The merge data containing information on the files to resolve.</param>
        /// <returns>Merge action for the file.</returns>
        /// <remarks>
        /// mergeData is disposed after ResolveFile completes.  Do not persist a reference to the mergeData object.
        /// </remarks>
        public virtual MergeAction ResolveFile(MergeData mergeData)
        {
            return MergeAction.Quit;
        }
    }
}
