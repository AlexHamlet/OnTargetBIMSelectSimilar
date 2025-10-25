using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Net;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace SelectSimilar
{
    class Entitlements : IExternalCommand
    {

        //Set values specific to the environment public const string
        private const string _baseApiUrl = @"https://apps.autodesk.com/";
        private const string _appId = @"955852388506253137";
        private static bool bEntitled = false;

        private bool Entitlement(string appId, string userId)
        {

            //(1) Build request
            var client = new RestClient();
            client.BaseUrl = new System.Uri(_baseApiUrl);

            //Set resource/end point
            var request = new RestRequest();
            request.Resource = "webservices/checkentitlement";
            request.Method = Method.GET;

            //Add parameters
            request.AddParameter("userid", userId);
            request.AddParameter("appid", appId);

            //(2) Execute request and get response
            IRestResponse response = client.Execute(request);

            //Get the entitlement status.
            bool isValid = false;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JsonDeserializer deserial = new JsonDeserializer();

                try
                {
                    EntitlementResponse entitlementResponse =
                    deserial.Deserialize<EntitlementResponse>(response);
                    isValid = entitlementResponse.IsValid;

                    Properties.Settings.Default.OTBENTFILE = SimpleJson.SerializeObject(new EntitlementCache() { PrevEntitled = isValid });
                    Properties.Settings.Default.Save();
                    bEntitled = isValid;
                    return isValid;
                }
                catch
                {
                    EntitlementCache entitlementCache = SimpleJson.DeserializeObject<EntitlementCache>(Properties.Settings.Default.OTBENTFILE);
                    if (entitlementCache.PrevEntitled == false)
                    {
                        return false;
                    }
                    else
                    {
                        if (entitlementCache.Time == null)
                        {
                            Properties.Settings.Default.OTBENTFILE = SimpleJson.SerializeObject(new EntitlementCache() { PrevEntitled = true, Time = DateTime.UtcNow });
                            Properties.Settings.Default.Save();
                            MessageBox.Show("Your license could not be verified at this time.\nYou have been given a 24 hour grace period.\n", "Grace Period");
                            bEntitled = true;
                            return true;
                        }
                        else
                        {
                            if (entitlementCache.Time is DateTime dateTime)
                                if (Math.Abs(dateTime.Subtract(DateTime.UtcNow).TotalDays) < 1)
                                {
                                    MessageBox.Show("Your license could not be verified at this time.\nYou have been given a 24 hour grace period.\n", "Grace Period");
                                    bEntitled = true;
                                    return true;
                                }
                                else
                                {
                                    MessageBox.Show("Your grace period has ended.", "Grace Period");
                                    return false;
                                }
                        }
                    }
                }
            }

            return isValid;
        }

        public bool Entitled(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                //Check if already entitled
                if (bEntitled)
                    return true;

                //Run Entitlement method
                if ((int)Execute(commandData, ref message, elements) == (int)Result.Succeeded)
                {
                    bEntitled = true;
                    return true;
                }

                //If all else failed, the are not Entitled
                return false;
            }
            catch
            {
                bEntitled = true;
                return true;
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                //Get the top elements
                UIApplication uiApp = commandData.Application;
                Application rvtApp = uiApp.Application;

                //Check to see if the user is logged in.
                if (!Application.IsLoggedIn)
                {
                    MessageBox.Show("Please login to your Autodesk account to use Select Similar.\n", "Entitlement API");
                    return Result.Failed;
                }

                //Get the user id, and check entitlement
                string userId = rvtApp.LoginUserId;
                bool isValid = Entitlement(_appId, userId);

                if (isValid)
                {
                    return Result.Succeeded;
                }

                return Result.Failed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
                return Result.Succeeded;
            }
        }
    }
}
