using log4net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;

namespace PeriodicConnectionTest
{
    class Program
    {
        #region Fields

        private static readonly ILog log = LogManager.GetLogger("PeriodicConnectionTest");

        #endregion

        static void Main(string[] args)
        {
            try
            {
                CrmServiceClient client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["MSCRMConnection"].ConnectionString);
                var result = client.RetrieveMultiple(new QueryExpression()
                {
                    EntityName = "contact",
                    NoLock = true,
                    Criteria = {
                    Conditions = {
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                    }
                },
                    TopCount = 1
                });

                log.InfoFormat("Successfully fetched {0} contact from the CRM system", result.Entities.Count);

            }
            catch (Exception ex) {
                log.Error(BuildErrorMessage(ex),ex);
            }

        }

        private static string BuildErrorMessage(Exception exception)
        {
            List<string> result = new List<string>();

            //result.Add("Es ist ein Fehler aufgetreten!");

            if (exception.GetType() == typeof(InvalidPluginExecutionException))
            {
                InvalidPluginExecutionException ex = (InvalidPluginExecutionException)exception;
                result.Add(ex.Message);
            }
            else if (exception.GetType() == typeof(FaultException<OrganizationServiceFault>))
            {
                FaultException<OrganizationServiceFault> ex = (FaultException<OrganizationServiceFault>)exception;
                result.Add(ex.Message);
            }
            else if (exception.GetType() == typeof(TimeoutException))
            {
                TimeoutException ex = (TimeoutException)exception;
                result.Add(string.Format("Message: {0}", ex.Message));
                result.Add(string.Format("Stack Trace: {0}", ex.StackTrace));
                result.Add(string.Format("Inner Fault: {0}",
                    null == ex.InnerException.Message ? "Has Inner Fault" : "No Inner Fault"));
            }
            else
            {
                Exception ex = exception;
                result.Add(string.Format(ex.Message));

                // Display the details of the inner exception.
                if (ex.InnerException != null)
                {
                    result.Add(string.Format(ex.InnerException.Message));

                    FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> fe = ex.InnerException
                        as FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>;
                    if (fe != null)
                    {
                        result.Add(string.Format("Timestamp: {0}", fe.Detail.Timestamp));
                        result.Add(string.Format("Code: {0}", fe.Detail.ErrorCode));
                        result.Add(string.Format("Message: {0}", fe.Detail.Message));
                        result.Add(string.Format("Trace: {0}", fe.Detail.TraceText));
                        result.Add(string.Format("Inner Fault: {0}",
                            null == fe.Detail.InnerFault ? "Has Inner Fault" : "No Inner Fault"));
                    }//if
                }//if
            }//if

            return string.Join(Environment.NewLine, result.ToArray());
        }//BuildErrorMessage
    }
}
