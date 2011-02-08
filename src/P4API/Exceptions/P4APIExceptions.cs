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

namespace P4API.Exceptions
{
    /// <summary>
    /// Base exception for P4API.
    /// </summary>
    public class P4APIExceptions : System.Exception 
    {
        internal P4APIExceptions(){}
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Generic P4API exception!";
            }
        }
    }
    /// <summary>
    /// Exception: Attempting to run a command while the Perforce Server is not connected!
    /// </summary>
    public class ServerNotConnected : P4APIExceptions
    {
        internal ServerNotConnected() { }
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Attempting to run a command while the Perforce Server is not connected!\n";
            }
        }
    }

    /// <summary>
    /// Exception: Changing the port when a server is connected is not allowed!
    /// </summary>
    public class ServerAlreadyConnected : P4APIExceptions
    {
        internal ServerAlreadyConnected() { }

        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Changing the port when a server is connected is not allowed!\n";
            }
        }
    }

    /// <summary>
    /// Exception: Invalid login credentials.
    /// </summary>
    public class InvalidLogin : P4APIExceptions
    {
        private string _message;
        internal InvalidLogin(string Message) 
        {
            _message = Message;
        }
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("Invalid Login!\n{0}", _message) ;
            }
        }
    }

    /// <summary>
    /// Exception:  Attempting to set a property that can not be set after the server is connected!
    /// </summary>
    public class ServerNotConnected_SetVar_AfterInit : P4APIExceptions
    {
        internal ServerNotConnected_SetVar_AfterInit() { }
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Attempting to set a property that can not be set after the server is connected!\n";
            }
        }
    }

    /// <summary>
    /// Exception: You can not use the '-o' and '-i' flags in Fetch_Form and Save_Form.
    /// </summary>
    public class InvalidFormArgument : P4APIExceptions
    {
        internal InvalidFormArgument() { }
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return "You can not use the '-o' and '-i' flags in Fetch_Form and Save_Form.\n";
            }
        }
    }

    /// <summary>
    /// Exception:  Unable to connect to the Perforce Server!
    /// </summary>
    public class PerforceInitializationError : P4APIExceptions
    {
        private string _P4Msg;
        internal PerforceInitializationError(string msg)
        {
            _P4Msg = msg;
        }
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("Unable to connect to the Perforce Server!\n{0}", _P4Msg);
            }
        }
    }

    /// <summary>
    /// Error parsing the form spec.
    /// </summary>
    public class FormParseException : P4APIExceptions
    {
        private string _P4Msg;
        private string _P4FormName;
        internal FormParseException(string formName, string msg)
        {
            _P4Msg = msg;
            _P4FormName = formName;
        }
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("Error attempting to parse form: {0}!\n{1}", _P4FormName, _P4Msg);
            }
        }
    }

    
    /// <summary>
    /// Support for diff (without -s* flag) not yet implemented
    /// </summary>
    public class DiffNotImplemented : P4APIExceptions
    {
        internal DiffNotImplemented()
        {
        }
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Support for diff (without -s* flag) not yet implemented";
            }
        }
    }

    /// <summary>
    /// Support for merging not yet implemented
    /// </summary>
    public class MergeNotImplemented : P4APIExceptions
    {
        internal MergeNotImplemented()
        {
        }
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Support for merging not yet implemented";
            }
        }
    }

    /// <summary>
    /// Exception: Error attempting to fetch form.
    /// </summary>
    public class FormFetchException : P4APIExceptions
    {
        private string _P4Msg;
        private string _P4FormName;
        internal FormFetchException(string formName, string msg)
        {
            _P4Msg = msg;
            _P4FormName = formName;
        }
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("Error attempting to fetch form: {0}!\n{1}",_P4FormName, _P4Msg);
            }
        }
    }

    /// <summary>
    /// Exception: Interactive 'Form' commands are unsupported in P4API.
    /// Use the 'Fetch_Form' and 'Save_Form' methods of the P4Connection object.
    /// </summary>
    public class FormCommandException : P4APIExceptions
    {
        internal FormCommandException()
        {
        }
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Interactive 'Form' commands are unsupported in P4API.\nUse the 'Fetch_Form' and 'Save_Form' methods of the P4Connection object.";
            }
        }
    }

    /// <summary>
    /// Exception: Error running Perforce command!
    /// </summary>
    public class RunUnParsedException : P4APIExceptions
    {
        private P4UnParsedRecordSet _rs;
        internal RunUnParsedException(P4UnParsedRecordSet rs)
        {
            _rs = rs;
        }
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("Error running Perforce command!\n{0}",_rs.ErrorMessage);
            }
        }

        /// <summary>
        /// Gets the P4UnParsedRecordSet that would have been returned if an exception was not thrown.
        /// </summary>
        public P4UnParsedRecordSet Result
        {
            get
            {
                return _rs;
            }
        }
    }

    /// <summary>
    /// Exception: Error running Perforce command!
    /// </summary>
    public class RunException : P4APIExceptions
    {
        private P4RecordSet _rs;
        internal RunException(P4RecordSet rs)
        {
            _rs = rs;
        }
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("Error running Perforce command!\n{0}", _rs.ErrorMessage);
            }
        }
    }
    
    /// <summary>
    /// ClientUser ErrorPause method was called.  This in unsupported in P4.Net.
    /// </summary>
    /// <remarks>If you see this, please let the curator know the steps to reproduce.  This should not be reachable code.</remarks>
    public class ErrorPauseCalled : P4APIExceptions
    {
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("If you see this, please let the curator know the steps to reproduce.  This should not be reachable code.");
            }
        }
    }

    /// <summary>
    /// Exception: File not found!
    /// </summary>
    public class FileNotFound : P4APIExceptions
    {
        private string _depotPath;
        internal FileNotFound(string depotPath)
        {
            _depotPath = depotPath;
        }
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("Perforce file not found: {0}!\n", _depotPath);
            }
        }
    }

    /// <summary>
    /// Exception: Can not write to stream.
    /// </summary>
    public class StreamNotWriteable : P4APIExceptions
    {   
        /// <summary>
        /// Gets the error message for the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("Unable to write to stream!\n");
            }
        }
    }
}
