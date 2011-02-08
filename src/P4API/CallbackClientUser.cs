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
using System.Collections.Generic;

namespace P4API
{
    internal class CallbackKeepAlive : p4dn.KeepAlive
    {
        private CallbackClientUser callbackClient;
        public CallbackKeepAlive(CallbackClientUser callback)
        {
            this.callbackClient = callback;
        }

        public override bool IsAlive()
        {
            return callbackClient.IsAlive();
        }
    }
    internal class CallbackClientUser : p4dn.ClientUser
    {
        private P4Callback _callback = null;
        // throwing an exception in one of the overriden methods causes all kinds of problems.
        // this variable stores an exception, which can be used later to throw an exception when we're 
        // all done communicating with Perforce
        public Exception DeferedException = null;

        public bool IsAlive()
        {
            if (DeferedException != null) return false;
            try
            {
                if (_callback.Cancel())
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                DeferedException = e;
                return false;
            }
            return true;

        }

        public CallbackClientUser(P4Callback callback)
        {
            this._callback = callback;
        }

        #region Un-implemented callbacks... throw an exception
        //public override void Diff(System.IO.FileInfo f1, System.IO.FileInfo f2, int doPage, string diffFlags, p4dn.Error err)
        //{
        //    if (DeferedException != null) return;
        //    try
        //    {
        //        _callback.Diff(f1, f2, diffFlags);
        //    }
        //    catch (Exception e)
        //    {
        //        DeferedException = e;
        //    }

        //}

        public override void Edit(System.IO.FileInfo f1, p4dn.Error err)
        {
            // don't give the consumer any more messages if we're in an error state
            if (DeferedException != null) return;

            // this only happens when the user is fetching a form w/o using FetchForm method.
            DeferedException = new P4API.Exceptions.FormCommandException();
        }

        public override void ErrorPause(string errBuf, p4dn.Error err)
        {
            if (DeferedException != null) return;

            // don't know how this would be called.  AFIK, the only way to get here is to fill out a form
            // incorrectly, and other code should deal with that.
            DeferedException = new P4API.Exceptions.ErrorPauseCalled();
        }

        public override void Prompt(string msg, ref string rsp, bool noEcho, p4dn.Error err)
        {
            if (DeferedException != null) return;
            try
            {
                _callback.Prompt(msg, ref rsp);
            }
            catch (Exception e)
            {
                DeferedException = e;
            }
        }

        #endregion

        public override p4dn.P4MergeStatus Resolve(p4dn.P4MergeData mergeData)
        {
            if (DeferedException != null) return p4dn.P4MergeStatus.CMS_QUIT;
            try
            {
                using (MergeData merger = new MergeData(mergeData))
                {
                    MergeAction action = _callback.ResolveFile(merger);
                    switch (action)
                    {
                        case MergeAction.Quit:
                            return p4dn.P4MergeStatus.CMS_QUIT;
                        case MergeAction.Skip:
                            return p4dn.P4MergeStatus.CMS_SKIP;
                        case MergeAction.AcceptEdit:
                            return p4dn.P4MergeStatus.CMS_EDIT;
                        case MergeAction.AcceptMerged:
                            return p4dn.P4MergeStatus.CMS_MERGED;
                        case MergeAction.AcceptTheirs:
                            return p4dn.P4MergeStatus.CMS_THEIRS;
                        case MergeAction.AcceptYours:
                            return p4dn.P4MergeStatus.CMS_YOURS;
                    }
                }
            }
            catch (Exception e)
            {
                DeferedException = e;
            }
            return p4dn.P4MergeStatus.CMS_QUIT;
        }

        public override void Finished()
        {
            if (DeferedException != null) return;
            try
            {
                _callback.Finished();
            }
            catch (Exception e)
            {
                DeferedException = e;
            }
        }

        public override void SetSpecDef(string specdef)
        {
            _callback.SetSpecDef(specdef);
        }

        public override void Message(p4dn.Error err)
        {
            if (DeferedException != null) return;
            try
            {
                _callback.OutputMessage(new P4Message(err));
            }
            catch (Exception e)
            {
                DeferedException = e;
            }
        }
        public override void HandleError(p4dn.Error err)
        {
            if (DeferedException != null) return;
            try
            {
                _callback.OutputMessage(new P4Message(err));
            }
            catch(Exception e)
            {
                DeferedException = e;
            }
        }

        public override void OutputContent(byte[] b, bool IsText)
        {
            if (DeferedException != null) return;
            try
            {
                _callback.OutputContent(b, IsText);
            }
            catch (Exception e)
            {
                DeferedException = e;
            }            
        }

        public override void OutputStat(Dictionary<string, string> varList)
        {
            if (DeferedException != null) return;
            try
            {
                _callback.OutputRecord(new P4Record(varList));
            }
            catch (Exception e)
            {
                DeferedException = e;
            }
            
        }
        
        public override void OutputInfo(char level, string data)
        {
            if (DeferedException == null)
            {
                try
                {
                    _callback.OutputInfo(data);
                }
                catch (Exception e)
                {
                    DeferedException = e;
                }
            }
        }

        public override void InputData(ref string buff, p4dn.Error err)
        {
            System.Text.StringBuilder sb = new StringBuilder();
            if (DeferedException == null) 
            {            
                try
                {
                    
                    _callback.InputData(sb);
                }
                catch (Exception e)
                {
                    DeferedException = e;
                }
            }
            buff = sb.ToString();
        }
    }
}