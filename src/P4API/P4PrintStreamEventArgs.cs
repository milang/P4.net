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
using System.IO;
using System.Text;

namespace P4API
{
    /// <summary>
    /// Delegate to handle the OnPrintStream event.
    /// </summary>
    /// <param name="args">Arguments describing the printed file.</param>
    /// <param name="outputStream">The stream to write the file.</param>
    /// <seealso cref="P4API.P4Connection.OnPrintStream"/>
    public delegate void OnPrintStreamEventHandler(P4PrintStreamEventArgs args, out Stream outputStream);

    /// <summary>
    /// Delegate to handle the OnPrintEndFile event.
    /// </summary>
    /// <param name="args">Arguments describing the printed file.</param>
    /// <param name="stream">The stream that was written.</param>
    /// <seealso cref="P4API.P4Connection.OnPrintEndFile"/>
    public delegate void OnPrintEndEventHandler(P4PrintStreamEventArgs args, Stream stream);

    /// <summary>
    /// EventArgs class to supply file details.
    /// </summary>
    public class P4PrintStreamEventArgs : EventArgs
    {
        private Encoding _textEncoding = Encoding.GetEncoding(1252);
        private Encoding _unicodeEncoding = Encoding.UTF8;
        private string _depotFile;
        private string _action;
        private string _type;
        private DateTime _fileTime;
        private int _change;

        internal P4PrintStreamEventArgs(string depotFile, string action, string fileType,
            DateTime fileTime, int change)
        {
            _depotFile = depotFile;
            _action = action;
            _type = fileType;
            _fileTime = fileTime;
            _change = change;
        }

        /// <summary>
        /// The Perforce depot path to the file being printed.
        /// </summary>
        /// <value>The depot path to the file.</value>
        public string DepotFile
        {
            get
            {
                return _depotFile;
            }
        }

        /// <summary>
        /// Encoding to be used for files with a Perforce type of 'text'.
        /// </summary>
        /// <value>The encoding for Text Files being printed.</value>
        public Encoding TextEncoding
        {
            get
            {
                return _textEncoding;
            }
            set
            {
                _textEncoding = value;
            }
        }

        /// <summary>
        /// Encoding to be used for files with a Perforce type of 'unicode'.
        /// </summary>
        /// <value>The encoding form unicode files being printed.</value>
        public Encoding UnicodeEncoding
        {
            get
            {
                return _unicodeEncoding;
            }
            set
            {
                _unicodeEncoding = value;
            }
        }

        /// <summary>
        /// Submit action for the file being printed.
        /// </summary>
        /// <value>The last submit action for the file being printed.</value>
        public string Action
        {
            get
            {
                return _action;
            }
        }

        /// <summary>
        /// The Perforce file type of the file being printed.
        /// </summary>
        /// <value>The Perforce filetype of the file being printed.</value>
        public string FileType
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// The timestamp of the file being printed.
        /// </summary>
        /// <value>The timestamp of the file being printed.</value>
        public DateTime FileTime
        {
            get
            {
                return _fileTime;
            }
        }

        /// <summary>
        /// The Perforce changelist number for the file being printed.
        /// </summary>
        /// <value>The Perforce changelist number for the file being printed.</value>
        public int Change
        {
            get
            {
                return _change;
            }
        }

    }
}
