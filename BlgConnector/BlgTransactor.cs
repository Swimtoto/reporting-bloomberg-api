using Bloomberglp.Blpapi;
using NeutralObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlgConnector
{

    public class BlgTransactor : IDisposable
    {
        private Service service;
        private Session session;
        public int counter;
        public int datecounter;

        public Dictionary<string[], HistoData> res2;
        public Dictionary<string[], string> res1;
        public Dictionary<string, string> errors;

        private static readonly Name SECURITY_DATA = new Name("securityData");
        private static readonly Name SECURITY = new Name("security");
        private static readonly Name SECURITIES = new Name("securities");
        private static readonly Name FIELD_DATA = new Name("fieldData");
        private static readonly Name RESPONSE_ERROR = new Name("responseError");
        private static readonly Name HISTORY_RESPONSE_ERROR = new Name("errorInfo");
        private static readonly Name SECURITY_ERROR = new Name("securityError");
        private static readonly Name FIELD_EXCEPTIONS = new Name("fieldExceptions");
        private static readonly Name FIELD_ID = new Name("fieldId");
        private static readonly Name FIELDS = new Name("fields");
        private static readonly Name ERROR_INFO = new Name("errorInfo");
        private static readonly Name CATEGORY = new Name("category");
        private static readonly Name MESSAGE = new Name("message");
        private static readonly Name LAST_PRICE = new Name("PX_LAST");

        protected BlgOptions options;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_options">Options list</param>
        public BlgTransactor(BlgOptions _options = null)
        {
            if (_options == null)
                options = new BlgOptions();
            else
                options = _options;
            open_session();
            counter = 0;
        }

        /// <summary>
        /// Opens a Bloomberg session
        /// </summary>
        private void open_session()
        {
            if (this.service == null)
            {
                if (this.session == null)
                {
                    string serverHost = "localhost";
                    int serverPort = 8194;

                    SessionOptions sessionOptions = new SessionOptions();
                    sessionOptions.ServerHost = serverHost;
                    sessionOptions.ServerPort = serverPort;
                    session = new Session(sessionOptions);
                    bool sessionStarted = session.Start();
                    if (!sessionStarted)
                    {
                        System.Console.Error.WriteLine("Failed to start session.");
                        return;
                    }
                    if (!session.OpenService("//blp/refdata"))
                    {
                        System.Console.Error.WriteLine("Failed to open //blp/refdata");
                        return;
                    }
                }
                service = session.GetService("//blp/refdata");
            }
        }

        /// <summary>
        /// Sends the request and processes received data
        /// </summary>
        public void ProcessData()
        {
            open_session();
            bool done = false;

            counter = 0;
            datecounter = 0;

            res2 = new Dictionary<string[], HistoData>();
            res1 = new Dictionary<String[], String>();
            errors = new Dictionary<String, String>();

            try
            {
                sendRefDataRequest(session);
            }
            catch (InvalidRequestException e)
            {
                System.Console.WriteLine(e.ToString());
            }

            while (!done)
            {
                Event eventObj = session.NextEvent();
                if (eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                {
                    System.Console.WriteLine("Processing Partial Response");
                    processResponseEvent(eventObj);
                }
                else if (eventObj.Type == Event.EventType.RESPONSE)
                {
                    System.Console.WriteLine("Processing Response");
                    processResponseEvent(eventObj);
                    done = true;
                }
                else
                {
                    foreach (Message msg in eventObj)
                    {
                        System.Console.WriteLine(msg.AsElement);
                        if (eventObj.Type == Event.EventType.SESSION_STATUS)
                        {
                            if (msg.MessageType.Equals("SessionTerminated"))
                            {
                                done = true;
                            }
                        }
                    }
                }
            }
            close_connection();
        }

        /// <summary>
        /// Processes data received from Blg
        /// </summary>
        /// <param name="eventObj">Blg Event</param>
        private void processResponseEvent(Event eventObj)
        {
            foreach (Message msg in eventObj)
            {
                if (msg.HasElement(RESPONSE_ERROR))
                {
                    printErrorInfo("REQUEST FAILED: ", msg.GetElement(RESPONSE_ERROR));
                    continue;
                }
                if (options.reqType == BlgRequests.reference)
                {
                    Element security = msg.GetElement(SECURITY_DATA);
                    for (int j = 0; j < security.NumValues; ++j)
                    {
                        string ticker = "";
                        Element temp;
                        if (!CheckBBRunning())
                            temp = ((Element)security.Elements.ElementAt(j));
                        else
                            temp = ((Element)security[j]);

                        ticker = temp.GetElementAsString(SECURITY);
                        Element fieldDataArray = temp.GetElement(FIELD_DATA);

                        foreach (string s in options.fields)
                        {
                            string tempres;
                            try
                            {
                                tempres = fieldDataArray.GetElementAsString(s);
                                counter++;
                            }
                            catch (NotFoundException)
                            {
                                continue;
                            }
                            catch (Exception)
                            {
                                tempres = fieldDataArray.GetElementAsFloat64(s).ToString();
                            }

                            res1.Add(new string[2] { s, ticker }, tempres);
                        }
                    }
                }
                else
                {
                    Element security = msg.GetElement(SECURITY_DATA);
                    string ticker = "";
                    System.Console.WriteLine("Processing securities:");

                    try
                    {
                        ticker = security.GetElementAsString(SECURITY);
                    }
                    catch (Exception)
                    {
                        security = (Element)security[0];
                        ticker = security.GetElementAsString(SECURITY).Remove(0, 6);
                    }

                    System.Console.WriteLine("\nTicker: " + ticker);
                    if (security.HasElement(SECURITY_ERROR))
                    {
                        printErrorInfo("\tSECURITY FAILED: ",
                            security.GetElement(SECURITY_ERROR));
                        errors.Add(ticker, security.GetElement(SECURITY_ERROR).GetElementAsString(MESSAGE));
                        continue;
                    }

                    try
                    {
                        Element fieldexc = ((Element)security.GetElement(FIELD_EXCEPTIONS)[0]);
                        printErrorInfo("\tFIELD EXCEPTION: ",
                            fieldexc.GetElement(HISTORY_RESPONSE_ERROR));
                        errors.Add(ticker, fieldexc.GetElement(HISTORY_RESPONSE_ERROR).GetElementAsString(MESSAGE));
                        continue;
                    }
                    catch (Exception) { }

                    Element fieldDataArray = security.GetElement(FIELD_DATA);

                    DateTime date = new DateTime();
                    double value = 0;

                    System.Console.WriteLine("FIELD\t\tVALUE");
                    System.Console.WriteLine("-----\t\t-----");


                    for (int j = 0; j < fieldDataArray.NumValues; ++j)
                    {
                        Element fieldData = fieldDataArray.GetValueAsElement(j);

                        Datetime temp = fieldData.GetElementAsDatetime("date");
                        counter++;
                        datecounter++;
                        double valtemp = 0;
                        foreach (string s in options.fields)
                        {
                            string tempres;
                            counter++;
                            try
                            {
                                tempres = fieldData.GetElementAsString(s);
                            }
                            catch (NotFoundException)
                            {
                                continue;
                            }
                            if (!Double.TryParse(tempres, out valtemp))
                                break;
                            else
                            {
                                date = temp.ToSystemDateTime();
                                value = valtemp;
                                if (date != null && value != 0)
                                    res2.Add(new string[2] { s, ticker }, new HistoData(date, value));
                            }
                        }
                    }
                    System.Console.WriteLine(date.ToString() + "\t\t" +
                                value.ToString());
                }
            }
        }

        /// <summary>
        /// Sends the request
        /// </summary>
        /// <param name="session">Blg Session</param>
        private void sendRefDataRequest(Session session)
        {
            Service refDataService = session.GetService("//blp/refdata");
            Request request = refDataService.CreateRequest(StringEnum.GetStringValue(options.reqType));

            if (options.reqType == BlgRequests.historical)
                request.Set(StringEnum.GetStringValue(options.periods.Key), StringEnum.GetStringValue(options.periods.Value));

            foreach (KeyValuePair<BlgMiscOpt, dynamic> kvp in options.options)
                request.Set(StringEnum.GetStringValue(kvp.Key), kvp.Value);

            // Add securities to request
            Element securities = request.GetElement(SECURITIES);
            foreach (string s in options.tickers)
                securities.AppendValue(s);

            // Add fields to request
            Element fields = request.GetElement(FIELDS);
            foreach (string s in options.fields)
                fields.AppendValue(s);

            System.Console.WriteLine("Sending Request: " + request);
            session.SendRequest(request, null);
        }

        /// <summary>
        /// Writes error into the console
        /// </summary>
        /// <param name="leadingStr">Error keyword</param>
        /// <param name="errorInfo">Blg Element containing the error</param>
        private void printErrorInfo(string leadingStr, Element errorInfo)
        {
            System.Console.WriteLine(leadingStr + errorInfo.GetElementAsString(CATEGORY) +
                " (" + errorInfo.GetElementAsString(MESSAGE) + ")");
        }

        /// <summary>
        /// Close Blg connection
        /// </summary>
        private void close_connection()
        {
            this.Dispose();
        }

        /// <summary>
        /// Destroy this object
        /// </summary>
        public void Dispose()
        {
            session = null;
            service = null;
        }

        private bool CheckBBRunning()
        {
            if (Environment.UserDomainName == Environment.MachineName)
                return false;
            else
                return true;
        }
    }
}