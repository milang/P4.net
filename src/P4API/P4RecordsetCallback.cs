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

namespace P4API
{
    /// <summary>
    /// A P4Callback class that populates a P4Recordset.
    /// </summary>
    /// <remarks>
    /// Subclassing P4RecordsetCallback can be usefull when changing code that used the P4Connection.Run method.
    /// By subclassing P4RecordsetCallback, you can intercept calls to provide status updates, but still have access 
    /// to the populated recordset when the command completes.  Be sure to call the base methods in overriden methods to 
    /// ensure the recordset is correctly poulated.
    /// </remarks>
    public class P4RecordsetCallback : P4Callback
    {
        private P4BaseRecordSet _P4Result;

        // instance variable for the super class when this is created externally
        private P4RecordSet _P4ResultRecordset;

        internal P4RecordsetCallback(P4BaseRecordSet p4Result)
        {
            _P4Result = p4Result;
            _P4ResultRecordset = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="P4RecordsetCallback"/> class.
        /// </summary>
        public P4RecordsetCallback()
        {
            _P4ResultRecordset = new P4RecordSet();
            _P4Result = _P4ResultRecordset;
        }

        /// <summary>
        /// Gets the recordset.
        /// </summary>
        /// <value>The recordset.</value>
        /// <remarks>
        /// The recordset that has been populated when the command completes.
        /// </remarks>
        public P4RecordSet Recordset
        { 
            get 
            {
                return _P4ResultRecordset;
            } 
        }


        #region P4Callback Members

        /// <summary>
        /// Edits the specified f1.
        /// </summary>
        /// <param name="f1">The f1.</param>
        public override void Edit(System.IO.FileInfo f1)
        {
            throw new P4API.Exceptions.FormCommandException();
        }

        /// <summary>
        /// Prompts the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="rsp">The RSP.</param>
        public override void Prompt(string msg, ref string rsp)
        {
            rsp = _P4Result.RaiseOnPromptEvent(msg);
        }

        /// <summary>
        /// Resolves the file.
        /// </summary>
        /// <param name="mergeData">The merge data.</param>
        /// <returns></returns>
        public override MergeAction ResolveFile(MergeData mergeData)
        {
            // don't handle a merge action here
            throw new Exceptions.MergeNotImplemented();
        }

        /// <summary>
        /// Finisheds this instance.
        /// </summary>
        public override void Finished()
        {
            _P4Result.SpecDef = base.SpecDef;
            _P4Result.Finished();
        }

        /// <summary>
        /// Outputs the info.
        /// </summary>
        /// <param name="data">The data.</param>
        public override void OutputInfo(string data)
        {
            _P4Result.AddInfo(data);
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        /// <returns></returns>
        public override bool Cancel()
        {
            return false;
        }

        /// <summary>
        /// Outputs the content.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="IsText">if set to <c>true</c> [is text].</param>
        public override void OutputContent(byte[] buffer, bool IsText)
        {
            if (IsText)
            {
                string data = base.ContentEncoding.GetString(buffer);
                _P4Result.AddInfo(data);
            }
            else
            {
                _P4Result.BinaryOutput = buffer;
            }
            
        }

        /// <summary>
        /// Inputs the data.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public override void InputData(System.Text.StringBuilder buffer)
        {
            buffer.Append(_P4Result.InputData);
        }

        /// <summary>
        /// Outputs the record.
        /// </summary>
        /// <param name="record">The record.</param>
        public override void OutputRecord(P4Record record)
        {
            _P4Result.AddRecord(record);
        }

        /// <summary>
        /// Outputs the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void OutputMessage(P4Message message)
        {
            _P4Result.AddP4Message(message);

            //Console.WriteLine("{0}: {1}", (int)message.Severity, message.Identity);
            switch (message.Severity)
            {
                case P4MessageSeverity.Empty: // E_EMPTY (0) | no error 
                    _P4Result.AddString(message.Format());
                    break;
                case P4MessageSeverity.Info: // E_INFO  (1) | information, not necessarily an error 
                    _P4Result.AddInfo(message.Format());
                    break;
                case P4MessageSeverity.Warning: // E_WARN  (2) | a minor error occurred 
                    _P4Result.AddWarning(message.Format());
                    break;
                case P4MessageSeverity.Failed: // E_FAILED(3) | the command was used incorrectly 
                    _P4Result.AddError(message.Format());
                    break;
                case P4MessageSeverity.Fatal: // E_FATAL (4) | fatal error, the command can't be processed 
                    _P4Result.AddError(message.Format());
                    break;
                default:
                    //TODO throw an error... unknown severity
                    break;
            }
        }

        #endregion
    }
}
