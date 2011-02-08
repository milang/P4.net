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
using p4dn;
using P4API.Exceptions;
using System.IO;
using System.Collections.Generic;

namespace P4API
{
    /// <summary>
    /// Delegate to handle the OnPrompt event.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="args">P4PromptEventArgs</param>
    /// <remarks>Handle Perforce "prompts".  Prompts are from commands that ask the user to respond, such as:
    /// <list>
    /// <li>login</li>
    /// <li>passwd</li>
    /// <li>resolve (without an -a* switch)</li>
    /// </list>
    /// </remarks>
    /// <seealso  cref="P4API.P4Connection.OnPrompt"/>
    public delegate void OnPromptEventHandler(object sender, P4PromptEventArgs args);

    /// <summary>
    /// A connection to a Perforce server instance.
    /// </summary>
    /// <include file='CodeDocuments.xml' path='//P4Connection/remarks' />
    public class P4Connection : IDisposable
    {

        #region Private Variables
        private ClientApi m_ClientApi;
        private bool _tagged = true;
        private bool _Initialized;

        private System.Collections.Specialized.StringDictionary cachedSpecDefs;
        private string _CallingProgram;
        private string _CallingProgramVersion;
        private string _Client = null;
        private string _Port = null;
        private string _User = null;
        private string _Host = null;
        private string _CWD = null;
        private string _Charset = null;
        private string _Password = null;
        private string _TicketFile = null;
        private DateTime _p4epoch = new DateTime(1970, 1, 1);
        private P4ExceptionLevels _exceptionLevel = P4ExceptionLevels.NoExceptionOnWarnings ;

        private int _maxScanRows = 0;
        private int _maxResults = 0;
        private int _maxLockTime = 0;
        private int _ApiLevel = 0;
        #endregion

        #region Events
        /// <summary>
        /// Raised when Perforce is prompting for a response.
        /// </summary>
        /// <remarks>Handle Perforce "prompts".  Prompts are from commands that ask the user to respond, such as:
        /// <list>
        /// <li>login</li>
        /// <li>passwd</li>
        /// <li>resolve (without an -a* switch)</li>
        /// </list>
        /// </remarks>
        /// <include file='CodeDocuments.xml' path='//OnPrompt/example' />
        public event OnPromptEventHandler OnPrompt;

        /// <summary>
        /// Raised from P4PrintStreamEvents before a file is printed.
        /// </summary>
        /// <remarks> Handle this event to initialize a stream that P4API will write to.</remarks>
        public event OnPrintStreamEventHandler OnPrintStream;

        /// <summary>
        /// Raised from P4PrintStreamEvents after a file is printed.
        /// </summary>
        /// <remarks>
        /// Use this event to close any streams that were created by the OnPrintStreamEventHandler event.
        /// </remarks>
        public event OnPrintEndEventHandler OnPrintEndFile;

        #endregion

        #region Contructors
        /// <summary>
        /// Initializes a new instance of the P4Connection class.
        /// </summary>
        public P4Connection()
        {
            this._Initialized = false;
            this._CallingProgram = "P4.Net API";
            this._CallingProgramVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            cachedSpecDefs = new System.Collections.Specialized.StringDictionary();

        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/Sets a value that overrides the defined MaxScanRows.
        /// </summary>
        /// <remarks>Servers may set a limit for MaxScanRows for certain users.  Set this value to override.  
        /// A value of 0 indicates no override.</remarks>
        /// <value>Overrides the MaxScanRows defined at the server.</value>
        public int MaxScanRows
        {
            get
            {
                return _maxScanRows ;
            }
            set
            {
                _maxScanRows  = value;
                //_ClientAPI.SetMaxScanRows(value);
            }
        }

        /// <summary>
        /// Gets/Sets a value that overrides the defined MaxLockTime.
        /// </summary>
        /// <remarks>Servers may set a limit for MaxLockTime for certain users.  Set this value to override.  
        /// A value of 0 indicates no override.</remarks>
        /// <value>Overrides the MaxLockTime defined at the server.</value>
        public int MaxLockTime
        {
            get
            {
                return _maxLockTime;
            }
            set
            {
                _maxLockTime = value;
                //_ClientAPI.SetMaxScanRows(value);
            }
        }        

        /// <summary>
        /// Gets/Sets a value that overrides the defined MaxResults.
        /// </summary>
        /// <remarks>Servers may set a limit for MaxResults for certain users.  Set this value to override.  
        /// A value of 0 indicates no override.</remarks>
        /// <value>Overrides the MaxResults defined at the server.</value>
        public int MaxResults
        {
            get
            {
                return _maxResults;
            }
            set
            {
                _maxResults = value;
                //_ClientAPI.SetMaxResults(value);
            }
        }

        /// <summary>
        /// Gets/Sets the Host-name of the client.
        /// </summary>
        /// <remarks>Forcing the Host to a different name is useful when using a client that
        /// was defined on a different host.  Of course, that can cause issues with the client's
        /// have list, so use with caution.</remarks>
        /// <value>The client Host name.</value>
        public string Host
        {
            get
            {
                return _ClientAPI.Host;
            }
            set
            {
                _Host = value;
                _ClientAPI.SetHost(value);
            }
        }

        /// <summary>
        /// Gets/Sets the Perforce Server port.
        /// </summary>
        /// <value>The port.</value>
        /// <remarks>Defaults to the value set in your Perforce environment.  Port can not be
        /// changed after running Connect (not even after a Disconnect).  
        /// To connect to a server on another port, create a new instance of P4Connection.
        /// </remarks>
        public string Port
        {
            get
            {
                return _ClientAPI.Port;
            }
            set
            {
                if (_Initialized) throw new ServerAlreadyConnected();
                _Port = value;
                _ClientAPI.SetPort(value);
            }
        }

        /// <summary>
        /// Gets/Sets the User login used to connect to Perforce.
        /// </summary>
        /// <value>The user.</value>
        /// <remarks>Defaults to the value set in your Perforce environment.  
        /// After you've set User, you can unset (revert to the default) by setting to User to null.</remarks>
        public string User
        {
            get
            {
                return _ClientAPI.User;
            }
            set
            {
                _User = value;
                _ClientAPI.SetUser(value);
            }
        }

        /// <summary>
        /// Gets/Sets the client character set.
        /// </summary>
        /// <remarks>Defaults to the value set in your Perforce environment.  
        /// After you've set Charset, you can unset (revert to the default) by setting to Charset to null.</remarks>
        /// <value>The client's charset.</value>
        public string Charset
        {
            get
            {
                return _ClientAPI.Charset;
            }
            set
            {
                _Charset = value;
                _ClientAPI.SetCharset(value);
            }
        }

        /// <summary>
        /// Get/Sets the name of the calling program.
        /// </summary>
        /// <remarks>This value is seen in p4 monitor and in the server log files.</remarks>
        /// <value>Defaults to "P4.Net API".</value>
        public string CallingProgram
        {
            get
            {
                return _CallingProgram;
            }
            set
            {
                if (_Initialized) throw new ServerNotConnected_SetVar_AfterInit();
                _CallingProgram = value;
                // Set these in the initialize section
            }
        }

        /// <summary>
        /// Gets/Sets the version of the calling program's version.
        /// </summary>
        /// <remarks>This value is seen in p4 monitor and the server log files.</remarks>
        /// <value>Defaults to the assembly version of the P4API.dll.</value>
        public string CallingVersion
        {
            get
            {
                return _CallingProgramVersion;
            }
            set
            {
                if (_Initialized) throw new ServerNotConnected_SetVar_AfterInit();
                _CallingProgramVersion = value;
                // Set these in the initialize section
            }
        }

        /// <summary>
        /// Sets the password to conenct with.
        /// </summary>
        /// <remarks>
        /// Do not set the literal password with a server running security level 2 or higher.  
        /// For those servers, Use the Login method and/or a ticket in place of the Password.
        /// </remarks>
        /// <value>The user's password.</value>
        public string Password
        {
            get
            {
                return _ClientAPI.Password;
            }
            set
            {
                _Password = value;
                _ClientAPI.SetPassword(value);
            }
        }

        /// <summary>
        /// Sets the ticket file used for Authentication.
        /// </summary>
        /// <remarks>
        /// Use this to override the default location of the TicketFile.
        /// </remarks>
        /// <value>Overrided the ticket file location.</value>
        public string TicketFile
        {
            get
            {
                return _TicketFile;
            }
            set
            {
                _TicketFile = value;
                _ClientAPI.SetTicketFile(value);
            }
        }

        /// <summary>
        /// Sets the Language for message translations.
        /// </summary>
        /// <remarks>
        /// Use this to override the default location of the TicketFile.
        /// </remarks>
        /// <value>Overrided the ticket file location.</value>
        public string Language
        {
            get
            {
                return _ClientAPI.Language;
            }
            set
            {
                _ClientAPI.SetLanguage(value);
            }
        }


        /// <summary>
        /// Gets/Sets the client workspace.
        /// </summary>
        /// <remarks>Many Perforce commands require a valid client spec to be set in order to run.  
        /// Defaults to the value set in your Perforce environment.  After you've set Client, you
        /// can unset (revert to the default) by setting to Client to null.</remarks>
        /// <value>The client name.</value>
        public string Client
        {
            get
            {
                return _ClientAPI.Client;
            }
            set
            {
                _Client = value;
                _ClientAPI.SetClient(value);
            }
        }

        /// <summary>
        /// Gets/Sets the current working directory.
        /// </summary>
        /// <remarks>Setting CWD can be used so that relative paths are specified.  Keep in mind that changing
        /// the CWD also might cause P4CONFIG settings to change.</remarks>
        /// <value>The current working directory for the Perforce connection.</value>
        public string CWD
        {
            get
            {
                return _ClientAPI.Cwd;
            }
            set
            {
                _CWD = value;
                _ClientAPI.SetCwd(value);
            }
        }

        /// <summary>
        /// Sets the client API protocol level.
        /// </summary>
        /// <remarks> Sets the API compatibility level desired. 
        /// This is useful when writing scripts using Perforce commands that do not yet support tagged output. 
        /// In these cases, upgrading to a later server that supports tagged output for the commands in question 
        /// can break your script. Using this method allows you to lock your script to the 
        /// output format of an older Perforce release and facilitate seamless upgrades. 
        /// Note that this method must be called prior to calling Connect.
        /// <br/>
        /// See the http://kb.perforce.com/P4dServerReference/ProtocolLevels/PerforceClientLevels for the API integer levels that correspond to each Perforce release.
        /// </remarks>
        /// <value>The Perforce API level.</value>
        public int Api
        {
            get
            {
                return _ApiLevel;
            }
            set
            {
                _ApiLevel = value;
                _ClientAPI.SetProtocol("api", value.ToString());
            }
        }

        /// <summary>
        /// Gets/Sets the Exception level when running Perforce commands.
        /// </summary>
        /// <remarks>
        /// <para>This property controls when P4.Net will throw exceptions
        /// when the underlying Perforce commands raise errors and warnings.</para>
        /// <para>The default is NoExceptionOnWarnings</para></remarks>
        /// <value>The exception level for the connection.</value>
        public P4ExceptionLevels ExceptionLevel
        {
            get
            {
                return _exceptionLevel;
            }
            set
            {
                _exceptionLevel = value;
            }
        }

        /// <summary>
        /// Checks the server level (version) of the Perforce server.
        /// </summary>
        /// <returns>The server's version level.</returns>
        /// <remarks>See http://kb.perforce.com/P4dServerReference/ProtocolLevels/PerforceServerLevels for more informantion.</remarks>
        /// <value>The server's version level.</value>
        public int ServerLevel
        {
            get
            {
                if (!_Initialized) throw new P4API.Exceptions.ServerNotConnected();

                string serverLevel = _ClientAPI.GetProtocol("server2");
                if (serverLevel == null)
                {
                    return 0;
                }
                else
                {
                    return int.Parse(serverLevel);
                }
            }
        }
           

        #endregion

        #region Public Methods

        #region Date Converion Methods
        /// <summary>
        /// Converts Perforce date (integer) to .Net DateTime.
        /// </summary>
        /// <remarks>
        /// The Perforce API returns most dates as an integer representing the number of seconds
        /// since 1/1/1970 in UTC.  The command line client generally returns dates as strings
        /// in the Perforce <b>server's</b> time zone.  The ConvertDate methods use the <b>client's</b> time zone for
        /// the offset.
        /// </remarks>
        /// <returns>DateTime in .Net format.</returns>
        public DateTime ConvertDate(int p4Date)
        {
            DateTime utc = _p4epoch.AddSeconds(p4Date);
            return TimeZone.CurrentTimeZone.ToLocalTime(utc);
        }

        /// <summary>
        /// Converts Perforce date (integer value as a string) to .Net DateTime.
        /// </summary> 
        /// <remarks>
        /// The Perforce API returns most dates as an integer representing the number of seconds
        /// since 1/1/1970 in UTC.  The command line client generally returns dates as strings
        /// in the Perforce <b>server's</b> time zone.  The ConvertDate methods use the <b>client's</b> time zone for
        /// the offset.
        /// </remarks>
        public DateTime ConvertDate(string p4Date)
        {
            return ConvertDate(int.Parse(p4Date));
        }

        /// <summary>
        /// Converts .Net DateTime to Perforce date (integer).
        /// </summary>
        /// <remarks>
        /// The Perforce API returns most dates as an integer representing the number of seconds
        /// since 1/1/1970 in UTC.  The command line client generally returns dates as strings
        /// in the Perforce <b>server's</b> time zone.  The ConvertDate methods use the <b>client's</b> time zone for
        /// the offset.
        /// </remarks>
        public int ConvertDate(DateTime date)
        {
            DateTime utc = TimeZone.CurrentTimeZone.ToUniversalTime(date);
            TimeSpan ts = utc.Subtract(_p4epoch);
            return (int) ts.TotalSeconds;
        }

        #endregion

        #region General Methods
        /// <summary>
        /// Checks the case-sensitivity of the Perforce server.
        /// </summary>
        /// <returns>True when the connected server is case-sensitive.</returns>
        /// <remarks>This must be called after connecting to the server.</remarks>
        public bool IsServerCaseSensitive()
        {
            if (!_Initialized) throw new P4API.Exceptions.ServerNotConnected();

            string serverCase = _ClientAPI.GetProtocol("nocase");
            if (serverCase == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Connect to the Perforce server
        /// </summary>
        public void Connect()
        {
        
            // reset the cached SpecDefs
            cachedSpecDefs = new System.Collections.Specialized.StringDictionary();

            EstablishConnection(_tagged);

        }

        /// <summary>
        /// Disconnect from the Perforce Server
        /// </summary>
        public void Disconnect()
        {
            CloseConnection();
         }

        /// <summary>
        /// Login to the Perforce Server
        /// </summary>
        /// <param name="password">The password.</param>
        public void Login(string password)
        {
            if (password == null) throw new ArgumentNullException();
            
            // if password is empty, then don't do anything
            if (password == string.Empty) return;

            P4ExceptionLevels oldLevel = _exceptionLevel;
            try
            {
                P4UnParsedRecordSet r = new P4UnParsedRecordSet();
                P4RecordsetCallback cb = new P4RecordsetCallback(r);
                r.LoginPassword = password;
                string[] Args = { };
                _exceptionLevel = P4ExceptionLevels.NoExceptionOnErrors;
                
               
                RunCallbackUnparsed(cb, "login", Args);

                //for good measure delete the password
                r.LoginPassword = null;

                if (r.HasErrors())
                {
                    throw new P4API.Exceptions.InvalidLogin(r.ErrorMessage);
                }
            }
            finally
            {
                _exceptionLevel = oldLevel;
            }

        }

        /// <summary>
        /// Creates a new pending changelist.
        /// </summary>
        /// <param name="Description">The description.</param>
        /// <returns>P4PendingChangelist object representing the named pending changelist</returns>
        public P4PendingChangelist CreatePendingChangelist(string Description)
        {
            if (Description == null) throw new ArgumentNullException();
            if (Description == string.Empty) throw new ArgumentException("Descrition can not be empty!");
            
            EstablishConnection(false);
            return new P4PendingChangelist(Description, this);

        }

        /// <summary>
        /// Determines if the Perforce connection is valid.
        /// </summary>
        /// <param name="checkLogin">if set to <c>true</c> it will check if the user is logged in.</param>
        /// <param name="checkClient">if set to <c>true</c> it will verify the client exists.</param>
        /// <returns>
        /// 	<c>true</c> if the connection is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// IsValidConnection performs the following 3 checks (depending on the arguments passed).
        /// <list type="numbered">
        /// <li>Runs p4 info, and verifies the server is valid.</li>
        /// <li>If checkClient is true, verifies that p4 info returns a clientName that is not "*unknown*.</li>
        /// <li>If checkLogin is true, verifies that p4 login -s does not have errors.</li>
        /// </list>
        /// </remarks>
        public bool IsValidConnection(bool checkLogin, bool checkClient)
        {
            P4ExceptionLevels oldExLevel = this.ExceptionLevel;
            try
            {
                this.ExceptionLevel = P4ExceptionLevels.NoExceptionOnErrors;
                P4RecordSet r = Run("info");
                if (r.HasErrors()) return false;
                if (r.Records.Length != 1) return false;
                if (checkClient)
                {
                    if (!r.Records[0].Fields.ContainsKey("clientName")) return false;
                    if (r.Records[0].Fields["clientName"] == "*unknown*") return false;
                }
                if (checkLogin)
                {
                    P4UnParsedRecordSet ur = RunUnParsed("login", "-s");
                    if (ur.HasErrors()) return false;
                }

            }
            catch
            {
                // something went way wrong
                return false;
            }
            finally
            {
                // set the ExceptionLevel back
                this.ExceptionLevel = oldExLevel;
            }
            return true;
        }

        /// <summary>
        /// Executes a Perforce command in tagged mode.
        /// </summary>
        /// <param name="Command">The command.</param>
        /// <param name="Args">The arguments to the Perforce command.  Remember to use a dash (-) in front of all switches</param>
        /// <returns>A P4Recordset containing the results of the command.</returns>
        public P4RecordSet Run(string Command, params string[] Args)
        {
            EstablishConnection(true);
            P4RecordSet r = new P4RecordSet();
            P4RecordsetCallback cb = new P4RecordsetCallback(r);

            RunCallback(cb, Command, Args);
            if (((_exceptionLevel == P4ExceptionLevels.ExceptionOnBothErrorsAndWarnings
                 || _exceptionLevel == P4ExceptionLevels.NoExceptionOnWarnings)
                 && r.HasErrors())
                ||
                  (_exceptionLevel == P4ExceptionLevels.ExceptionOnBothErrorsAndWarnings
                   && r.HasWarnings())
                )
            {
                throw new RunException(r);
            }


            return r;
        }

        /// <summary>
        /// Runs the specified command, calling the appropriate callback methods as Perforce returns information.
        /// </summary>
        /// <param name="Command">The Perforce command to run.</param>
        /// <param name="Callback">A callback instance to recieve information as the command is run.</param>
        /// <param name="Args">Arguments to the Perforce command</param>
        public void RunCallback(P4Callback Callback, string Command, params string[] Args)
        {
            CallbackClientUser cu = new CallbackClientUser(Callback);
            CallbackKeepAlive keepAlive = new CallbackKeepAlive(cu);
            EstablishConnection(true, keepAlive);
            Callback.SetEncoding(m_ClientApi.Encoding);
            RunIt(Command, Args, cu);

            // always throw a defered exception (means something went WAY wrong)
            if (cu.DeferedException != null)
            {
                throw cu.DeferedException;
            }
        }

        /// <summary>
        /// Runs the callback unparsed.
        /// </summary>
        /// <param name="Command">The command.</param>
        /// <param name="Callback">The callback.</param>
        /// <param name="Args">The args.</param>
        public void RunCallbackUnparsed(P4Callback Callback, string Command, params string[] Args)
        {
            CallbackClientUser cu = new CallbackClientUser(Callback);
            CallbackKeepAlive keepAlive = new CallbackKeepAlive(cu);
            EstablishConnection(false, keepAlive);
            Callback.SetEncoding(m_ClientApi.Encoding);
            RunIt(Command, Args, cu);

            // always throw a defered exception (means something went WAY wrong)
            if (cu.DeferedException != null)
            {
                throw cu.DeferedException;
            }
        }

        /// <summary>
        /// Executes a Perforce command in non-tagged mode.
        /// </summary>
        /// <param name="Command">The command.</param>
        /// <param name="Args">The args.</param>
        /// <returns></returns>
        public P4UnParsedRecordSet RunUnParsed(string Command, params string[] Args)
        {
            P4UnParsedRecordSet r = new P4UnParsedRecordSet();
            P4RecordsetCallback cb = new P4RecordsetCallback(r);
            P4BaseRecordSet.OnPromptEventHandler handler = new P4BaseRecordSet.OnPromptEventHandler(this.HandleOnPrompt);
            r.OnPrompt += handler;
            RunCallbackUnparsed(cb, Command, Args);
            r.OnPrompt -= handler;

            if (((_exceptionLevel == P4ExceptionLevels.ExceptionOnBothErrorsAndWarnings
                 || _exceptionLevel == P4ExceptionLevels.NoExceptionOnWarnings)
                 && r.HasErrors())
                ||
                  (_exceptionLevel == P4ExceptionLevels.ExceptionOnBothErrorsAndWarnings
                   && r.HasWarnings())
                )
            {
                throw new RunUnParsedException(r);
            }

            return r;
        }
        #endregion

        #region Form Methods
        /// <summary>
        /// Parse the raw text of a Perforce form into a P4Form object
        /// </summary>
        /// <param name="formCommand">The form command.</param>
        /// <param name="formContents">Raw contents of the form spec.</param>
        /// <returns>
        /// A P4Form object.  The fields of the form can be read or updated.  If you update a filed, you can save it with Save_Form.
        /// </returns>
        /// <include file='CodeDocuments.xml' path='//Forms/remarks' />
        public P4Form Parse_Form(string formCommand, string formContents)
        {
            // logic stolen from P4Ruby.  Cache the spec defs, and load a form from the cached specdefs.

            // If we don't have a cached Spec def, we need to create a dummy form to get it in the cache
            if (!cachedSpecDefs.ContainsKey(formCommand))
            {
                string bogusSpec = "__p4net_bogus_spec__";
                P4Form outputForm;

                // 
                // For specs of the following types we need the bogus spec name
                //
                if (formCommand == "branch" || formCommand == "label" || formCommand == "depot" || formCommand == "group")
                {
                    outputForm = Fetch_Form(formCommand, bogusSpec);
                }
                else
                {
                    outputForm = Fetch_Form(formCommand);
                }                
            }

            return P4Form.LoadFromSpec(formCommand, cachedSpecDefs[formCommand], formContents, m_ClientApi.Encoding);
        }

        /// <summary>
        /// Fetch a form object from Perforce.
        /// </summary>
        /// <param name="FormCommand">The form command.</param>
        /// <param name="Args">The args.</param>
        /// <returns>
        /// A Form object.  The fields of the form can be read or updated.  If you update a filed, you can save it with Save_Form.
        /// </returns>
        /// <include file='CodeDocuments.xml' path='//Forms/remarks' />
        public P4Form Fetch_Form(string FormCommand, params string[] Args)
        {
            if (FormCommand == null) throw new ArgumentNullException();
            if (Args == null) throw new ArgumentNullException();

            string[] AllArgs = new string[Args.Length + 1];
            AllArgs[0] = "-o";

            for (int i = 0; i < Args.Length ; i++)
            {
                if (Args[i] == null)
                {
                    throw new ArgumentNullException();
                }
                if (Args[i] == "-o")
                {
                    throw new InvalidFormArgument();
                }
                AllArgs[i+1] = Args[i];
            }

            EstablishConnection(true);

            P4FormRecordSet r = new P4FormRecordSet(FormCommand, m_ClientApi.Encoding);
            P4RecordsetCallback cb = new P4RecordsetCallback(r);

            //RunIt(FormCommand, AllArgs, r.ResultClientUser);
            RunCallback(cb, FormCommand, AllArgs);

            // The Fetch_Form command must always throw an error
            // because there will be no form to return if there's a 
            // problem
            if (r.HasErrors())
            {
                throw new FormFetchException(FormCommand, r.ErrorMessage);
            }

            // save the spec def, in case Parse_Form is called in the future
            cachedSpecDefs[FormCommand] = r.Form.SpecDef;

            return r.Form;
        }

        /// <summary>
        /// Saves the form to Perforce.
        /// </summary>
        /// <param name="Form">The P4Form object retrieved from Fetch_Form.</param>
        /// <include file='CodeDocuments.xml' path='//Forms/remarks' />
        /// <returns>P4UnParsedRecordSet.  Output can be parsed to verify the form was processed correctly.</returns>
        public P4UnParsedRecordSet Save_Form(P4Form Form)
        {
            return Save_Form(Form, false);
        }

        /// <summary>
        /// Saves the form to Perforce.
        /// </summary>
        /// <param name="formCommand">The form command to run.</param>
        /// <param name="formSpec">The formatted spec.</param>
        /// <param name="args">Arguments to the form command.</param>
        /// <returns>P4UnParsedRecordSet.  Output can be parsed to verify the form was processed correctly.</returns>
        public P4UnParsedRecordSet Save_Form(string formCommand, string formSpec, params string[] args)
        {
            if (formCommand == null) throw new ArgumentNullException("formCommand");
            if (formCommand == string.Empty) throw new ArgumentException("Parameter 'formCommand' can not be empty");
            if (formSpec == null) throw new ArgumentNullException("formSpec");
            if (formSpec == string.Empty) throw new ArgumentException("Parameter 'formSpec' can not be empt");

            P4UnParsedRecordSet r = new P4UnParsedRecordSet();
            P4RecordsetCallback cb = new P4RecordsetCallback(r);
            r.InputData = formSpec;
            List<string> formargs = new List<string>();
            foreach (string arg in args)
            {
                if (arg == "-i" || arg == "-o")
                {
                    throw new InvalidFormArgument();
                }
                formargs.Add(arg);
            }
            formargs.Add("-i");

            RunCallbackUnparsed(cb, formCommand, formargs.ToArray());
        
            if (((_exceptionLevel == P4ExceptionLevels.ExceptionOnBothErrorsAndWarnings
                 || _exceptionLevel == P4ExceptionLevels.NoExceptionOnWarnings)
                 && r.HasErrors())
                ||
                  (_exceptionLevel == P4ExceptionLevels.ExceptionOnBothErrorsAndWarnings
                   && r.HasWarnings())
                )
            {
                throw new RunUnParsedException(r);
            }
            return r;
        }

        /// <summary>
        /// Saves the form to Perforce.
        /// </summary>
        /// <param name="formCommand">The form command to run.</param>
        /// <param name="formSpec">The formatted spec.</param>
        /// <param name="Force">True to pass the '-f' flag when saving.</param>
        /// <returns>P4UnParsedRecordSet.  Output can be parsed to verify the form was processed correctly.</returns>
        public P4UnParsedRecordSet Save_Form(string formCommand, string formSpec, bool Force)
        {
            if (formCommand == null) throw new ArgumentNullException("formCommand");
            if (formCommand == string.Empty) throw new ArgumentException("Parameter 'formCommand' can not be empty");
            if (formSpec == null) throw new ArgumentNullException("formSpec");
            if (formSpec == string.Empty) throw new ArgumentException("Parameter 'formSpec' can not be empt");

            if (Force)
            {
                return Save_Form(formCommand, formSpec, "-f");
            }
            return Save_Form(formCommand, formSpec);
        }

        /// <summary>
        /// Saves the form to Perforce.
        /// </summary>
        /// <param name="Form">The P4Form object retrieved from Fetch_Form.</param>
        /// <param name="Force">True to pass the '-f' flag when saving.</param>
        /// <include file='CodeDocuments.xml' path='//Forms/remarks' />
        /// <returns>P4UnParsedRecordSet.  Output can be parsed to verify the form was processed correctly.</returns>
        public P4UnParsedRecordSet Save_Form(P4Form Form, bool Force)
        {
            if (Form == null) throw new ArgumentNullException("Form");
            return Save_Form(Form.FormCommand, Form.FormatSpec(), Force);
        }

        /// <summary>
        /// Saves the form to Perforce.
        /// </summary>
        /// <param name="Form">The P4Form object retrieved from Fetch_Form.</param>
        /// <param name="args">Arguments passed to the form command.</param>
        /// <include file='CodeDocuments.xml' path='//Forms/remarks' />
        /// <returns>P4UnParsedRecordSet.  Output can be parsed to verify the form was processed correctly.</returns>
        public P4UnParsedRecordSet Save_Form(P4Form Form, params string[] args)
        {
            if (Form == null) throw new ArgumentNullException("Form");
            return Save_Form(Form.FormCommand, Form.FormatSpec(), args);
        }

        #endregion

        #endregion

        #region Print Methods
        /// <summary>
        /// Print the contents of a file in Perforce to a byte array.
        /// </summary>
        /// <param name="depotPath">Perforce path to the file.</param>
        /// <returns>
        /// Contents of the file.
        /// Text files will be encoded with ANSI encodinge.  
        /// Unicode files will be encoded as UTF-8 (regardless of P4CHARSET setting).
        ///</returns>
        public byte[] PrintBinary(string depotPath)
        {
            // use utf-16 encoding.  In theory this will be more effient, b/c 
            // it will do only one conversion.
            MemoryStream ms = new MemoryStream();
            PrintStream(ms, depotPath);
            byte[] ret = new byte[ms.Position];
            ms.Position = 0;
            ms.Read(ret, 0, ret.Length);
            ms.Close();
            return ret;
        }

        /// <summary>
        /// Print the contents of a file in Perforce to a string.
        /// </summary>
        /// <param name="depotPath">Perforce path to the file.</param>
        /// <returns>Contents of the file.</returns>
        /// <remarks>
        /// Attempting to call PrintText on a binary file will likely cause corruption.<br/>
        /// If a file is does not exist, a FileNotFound exception will be thrown (regardless
        /// of the ExceptionLevel setting).<br/>
        /// Depot Path, Client Path or Local Path can generally be used for the argument
        /// depotPath (so long as the argument is valid for the <code>p4 print</code> command line.
        ///</remarks>
        public string PrintText(string depotPath)
        {
            // use utf-16 encoding.  In theory this will be more effient, b/c 
            // it will do only one conversion.
            MemoryStream ms = new MemoryStream();
            StreamReader sr = new StreamReader(ms, Encoding.Unicode);
            PrintStream(ms, depotPath, Encoding.Unicode);
            ms.Position = 0;
            string ret = sr.ReadToEnd();
            sr.Close();
            ms.Close();
            return ret;
        }

        /// <summary>
        /// Prints the contents of a Perforce file to a Stream.
        /// </summary>
        /// <param name="stream">Writable stream to write the contents to.</param>
        /// <param name="depotPath">Perforce path of the file to print.</param>
        /// <param name="encoding">Text encoding of the Stream.</param>
        /// <remarks>
        /// If a file is does not exist, a FileNotFound exception will be thrown (regardless
        /// of the ExceptionLevel setting).<br/>
        /// Depot Path, Client Path or Local Path can generally be used for the argument
        /// depotPath (so long as the argument is valid for the <code>p4 print</code> command line.<br/>
        /// Encoding will only be applied to files with a Perforce type of 'text' or 'unicode'.<br/>
        /// The stream argument can be any valid stream, so long as it is initialized and writable.<br/>
        ///</remarks>
        public void PrintStream(Stream stream, string depotPath, Encoding encoding)
        {
            if (stream == null) throw new ArgumentNullException();
            if (depotPath == null) throw new ArgumentNullException();
            if (depotPath == string.Empty) throw new ArgumentException("Argument depotPath can not be empty.");
            if (!stream.CanWrite) throw new StreamNotWriteable();

            PrintStreamHelper ph = new PrintStreamHelper(this);
            int filesPrinted = ph.PrintStream(stream, depotPath, encoding);
            if (filesPrinted < 1)
            {
                throw new Exceptions.FileNotFound(depotPath);
            }
        }

        /// <summary>
        /// Prints the contents of a Perforce file to a Stream.
        /// </summary>
        /// <param name="stream">Writable stream to write the contents to.</param>
        /// <param name="depotPath">Perforce path of the file to print.</param>        
        /// <remarks>
        /// If a file is does not exist, a FileNotFound exception will be thrown (regardless
        /// of the ExceptionLevel setting).<br/>
        /// Depot Path, Client Path or Local Path can generally be used for the argument
        /// depotPath (so long as the argument is valid for the <code>p4 print</code> command line.<br/>
        /// The stream argument can be any valid stream, so long as it is initialized and writable.<br/>
        ///</remarks>
        public void PrintStream(Stream stream, string depotPath)
        {
            PrintStream(stream, depotPath, null);
        }

        /// <summary>
        /// Runs a print command, and raises events for each file printed.
        /// </summary>
        /// <param name="args">Arguments to the <code>p4 print</code> command.</param>
        /// <event>OnPrintStream</event>
        /// <event>OnPrintEndFile</event>
        /// <remarks>
        /// This method is useful when you need to print several files with a single call
        /// to the Perforce server.
        /// </remarks>
        /// <example><code language='c#'></code></example>
        public void PrintStreamEvents(params string[] args)
        {
            P4PrintCallback pcb = new P4PrintCallback(this, OnPrintStream, OnPrintEndFile);
            RunCallback(pcb, "print", args);
        }

        #endregion

        #region Private Helper Methods
        private ClientApi _ClientAPI
        {
            get
            {
                if (m_ClientApi == null)
                {
                    m_ClientApi = new ClientApi();
                }
                return m_ClientApi;
            }
        }

        private void EstablishConnection(bool tagged)
        {
            EstablishConnection(tagged, null);
        }
        private void EstablishConnection(bool tagged, KeepAlive keepAlive)
        {
            if (m_ClientApi != null && m_ClientApi.Dropped() != 0)
            {
                // I can't figure out how to force this artificially, so currently untested :-(
                CloseConnection();
            }
            if (m_ClientApi == null)
            {
                m_ClientApi = new ClientApi();
            }
            if (!_Initialized)
            {
                Error err = null;
                try
                {
                    _tagged = tagged;
                    err = m_ClientApi.CreateError();
                   
                    // Always use the specstring protocol.  We'll controll form output via SetTag
                    // before each run
                    m_ClientApi.SetProtocol("specstring", "");                    
                    
                    //May have lost our settings... reset here
                    if (_Client != null) m_ClientApi.SetClient(_Client);
                    if (_User != null) m_ClientApi.SetUser(_User);
                    if (_CWD != null) m_ClientApi.SetCwd(_CWD);
                    if (_Charset != null) m_ClientApi.SetCharset(_Charset);
                    if (_Host != null) m_ClientApi.SetHost(_Host); 
                    if (_Port != null) m_ClientApi.SetPort(_Port);
                    if (_Password != null) m_ClientApi.SetPassword(_Password);
                    if (_TicketFile != null) m_ClientApi.SetTicketFile(_TicketFile);
                    if (_maxResults != 0) m_ClientApi.SetMaxResults(_maxResults);
                    if (_maxScanRows != 0) m_ClientApi.SetMaxScanRows(_maxResults);
                    if (_maxLockTime != 0) m_ClientApi.SetMaxLockTime(_maxLockTime);
                    if (_ApiLevel != 0) m_ClientApi.SetProtocol("api", _ApiLevel.ToString());
                    
                    m_ClientApi.Init(err);
                    if (err.Severity == Error.ErrorSeverity.Failed || err.Severity == Error.ErrorSeverity.Fatal)
                    {
                        throw new Exception("Unable to connect to Perforce!");
                    }
                    _Initialized = true;
                    err.Dispose();
                }
                catch (Exception e)
                {
                    string message = "Perforce connection error.";
                    try
                    {
                        m_ClientApi.Final(err);
                        message = e.Message;
                        err.Dispose();
                        m_ClientApi.Dispose();
                        m_ClientApi = null;
                    }
                    catch { }
                    throw new PerforceInitializationError(message);
                }
            }
            if (tagged) m_ClientApi.SetTag();
            if (_CallingProgram != null) m_ClientApi.SetProg(_CallingProgram);
            if (_CallingProgramVersion != null) m_ClientApi.SetVersion(_CallingProgramVersion);
            m_ClientApi.SetBreak(keepAlive);
        }
        private void CloseConnection()
        {
            // Need to reset the connection
            if (_Initialized)
            {
                Error err = m_ClientApi.CreateError();
                m_ClientApi.Final(err);
                err.Dispose();
            }
            if (m_ClientApi != null)
            {
                m_ClientApi.Dispose();
                m_ClientApi = null;
            }
            _Initialized = false;            
        }

        private void RunIt(string command, string[] args, ClientUser cu)
        {
            // validate that none of the args are null
            if (args == null)
            {
                throw new System.ArgumentNullException();
            }

            foreach (string arg in args)
            {
                if (arg == null)
                {
                    throw new System.ArgumentNullException();
                }
            }

            m_ClientApi.SetArgv(args);
            m_ClientApi.Run(command, cu);

        }
        private void HandleOnPrompt(object sender, P4PromptEventArgs e)
        {
            e.Response = RaiseOnPromptEvent(e.Message);
        }
        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        private string RaiseOnPromptEvent(string Message)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnPromptEventHandler handler = OnPrompt;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                P4PromptEventArgs e = new P4PromptEventArgs(Message);
                
                // Use the () operator to raise the event.
                handler(this, e);
                return e.Response; 
            }
            else
            {
                return string.Empty;
            }
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Calls Disconnect.
        /// </summary>
        public void Dispose()
        {
            this.Disconnect();
        }

        #endregion

    }
}
