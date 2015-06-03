/*
Copyright 2015 Intel Corporation

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
except in compliance with the License. You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the 
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
either express or implied. See the License for the specific language governing permissions 
and limitations under the License
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.PersonalInformation;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.UserData;
using System.Windows;
using WPCordovaClassLib.Cordova;
using WPCordovaClassLib.Cordova.Commands;
using WPCordovaClassLib.CordovaLib;


namespace Cordova.Extension.Commands
{
    public class IntelXDKContacts: BaseCommand
    {
        private ContactStore store;
        AddressChooserTask addressChooserTask;
        private bool busy;
        private const string ContactStoreLocalInstanceIdKey = "LocalInstanceId";
        private bool isGetContacts = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public IntelXDKContacts()
        {
        }

        #region Public Methods
        public void getContactInfo(string parameters)
        {
        }

        /// <summary>
        /// Address Chooser
        /// </summary>
        /// <param name="parameters"></param>
        public void chooseContact(string parameters)
        {
            // WP8 does not allow the editing of the contact.  Just return NOT AVAILABLE IN WINDOES PHONE 8 message
            string js = "javascript: var e = document.createEvent('Events');" +
                "e.initEvent('intel.xdk.contacts.choose',true,true);e.success=false;" +
                "e.error='NOT AVAILABLE IN WINDOES PHONE 8';document.dispatchEvent(e);";
            //InjectJS("javascript:" + js);
            InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true);
            return;


            try
            {
                if (busy == true)
                {
                    js = "javascript: var e = document.createEvent('Events');" +
                        "e.initEvent('intel.xdk.contacts.busy',true,true);e.success=false;" +
                        "e.message='busy';document.dispatchEvent(e);";
                    //InjectJS("javascript:" + js);
                    InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true); 
                    return;
                }
                busy = true;

                addressChooserTask = new AddressChooserTask();
                addressChooserTask.Completed += new EventHandler<AddressResult>(addressChooserTask_Completed);
                addressChooserTask.Show();
            }
            catch (Exception e)
            {
                js = string.Format(" var e = document.createEvent('Events');" +
                    "e.initEvent('intel.xdk.contacts.choose',true,true);" +
                    "e.success=false;e.error='{0}';document.dispatchEvent(e);", "There was a problem choosing a contact.");
                //InjectJS("javascript:" + js);
                InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true);
            }

        }

        /// <summary>
        /// Add Contact (Launcher)
        /// </summary>
        /// <param name="parameters"></param>
        public void addContact(string parameters)
        {
            string[] args = WPCordovaClassLib.Cordova.JSON.JsonHelper.Deserialize<string[]>(parameters);

            string givenName = (args.Length>1) ? HttpUtility.UrlDecode(args[0]) : "";
            string familyName =  (args.Length>1) ? HttpUtility.UrlDecode(args[1]) : "";
            string street =  (args.Length>1) ? HttpUtility.UrlDecode(args[2]) : "";
            string city =  (args.Length>1) ? HttpUtility.UrlDecode(args[3]) : "";
            string state =  (args.Length>1) ? HttpUtility.UrlDecode(args[4]) : "";
            string zip =  (args.Length>1) ? HttpUtility.UrlDecode(args[5]) : "";
            string country =  (args.Length>1) ? HttpUtility.UrlDecode(args[6]) : "";
            string phone =  (args.Length>1) ? HttpUtility.UrlDecode(args[7]) : "";
            string email =  (args.Length>1) ? HttpUtility.UrlDecode(args[8]) : "";

            SaveContactTask addContactTask = new SaveContactTask();
            addContactTask.Completed += new EventHandler<SaveContactResult>(addContactTask_Completed);
            addContactTask.FirstName = givenName;
            addContactTask.LastName = familyName;
            addContactTask.HomePhone = phone;
            addContactTask.PersonalEmail = email;
            addContactTask.HomeAddressCity = city;
            addContactTask.HomeAddressCountry = country;
            addContactTask.HomeAddressState = state;
            addContactTask.HomeAddressZipCode = zip;
            addContactTask.HomeAddressStreet = street;
            addContactTask.Show();
        }

        /// <summary>
        /// Retrieve All Contacts
        /// </summary>
        /// <param name="parameters"></param>
        public void getContacts(string parameters)
        {
            isGetContacts = true;

            getAllContacts();
        }

        /// <summary>
        /// Edit Contact
        /// </summary>
        /// <param name="parameters"></param>
        public void editContact(string parameters)
        {
            EditContact(parameters);

        }

        /// <summary>
        /// Remove Contact
        /// </summary>
        /// <param name="parameters"></param>
        async public void removeContact(string parameters)
        {
            // WP8 does not allow the editing of the contact.  Just return NOT AVAILABLE IN WINDOES PHONE 8 message
            string js = "javascript: var e = document.createEvent('Events');" +
                "e.initEvent('intel.xdk.contacts.remove',true,true);e.success=false;" +
                "e.error='NOT AVAILABLE IN WINDOES PHONE 8';document.dispatchEvent(e);";
            //InjectJS("javascript:" + js);
            InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true);
            return;


            string[] args = WPCordovaClassLib.Cordova.JSON.JsonHelper.Deserialize<string[]>(parameters);

            string contactId = HttpUtility.UrlDecode(args[0]);

            if( busy == true ) {
                js = "javascript: var e = document.createEvent('Events');" +
                    "e.initEvent('intel.xdk.contacts.busy',true,true);e.success=false;" +
                    "e.message='busy';document.dispatchEvent(e);";
                //InjectJS(js);
                InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true);
                return;
            }

            try{
                ContactStore store = await ContactStore.CreateOrOpenAsync(ContactStoreSystemAccessMode.ReadWrite, ContactStoreApplicationAccessMode.ReadOnly);
                StoredContact contact = await store.FindContactByIdAsync(contactId);
                
                await store.DeleteContactAsync(contactId);

                getAllContacts();

                js = string.Format("var e = document.createEvent('Events');" +
                    "e.initEvent('intel.xdk.contacts.remove',true,true);e.success=true;" +
                    "e.contactid='{0}';document.dispatchEvent(e);", contactId);
                //InjectJS("javascript: "+js);
                InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true); 

                busy = false;
            }
            catch(Exception e)
            {
                js = string.Format("var e = document.createEvent('Events');" +
                    "e.initEvent('intel.xdk.contacts.remove',true,true);e.success=false;" +
                    "e.error='contact not found';e.contactid='{0}';document.dispatchEvent(e);", contactId);
                //InjectJS("javascript: "+errjs);
                InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true); 

                busy = false;
                return;
            }
        }
        #endregion

        #region Chooser/Launcher Handlers
        /// <summary>
        /// Choose Address Launcher Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addressChooserTask_Completed(object sender, AddressResult e)
        {
            string js = "";

            getAllContacts();

            switch (e.TaskResult)
            {
                case TaskResult.OK:
                    AddressResult address = (AddressResult)e;

                    js = string.Format("javascript: var e = document.createEvent('Events');" +
                        "e.initEvent('intel.xdk.contacts.choose',true,true);" +
                        "e.success=true;e.contactid='{0}';document.dispatchEvent(e);", "");
                    busy = false;
                    break;
                case TaskResult.Cancel:
                    //js = new CommandResponse(new JsEvent { CommandType = CommandTypeEnum.APPMOBI_CONTACTS_CHOOSE, Cancelled = true, ErrorMessage = "Choose contact cancelled." }).ToString();
                    js = string.Format("javascript: var e = document.createEvent('Events');" +
                        "e.initEvent('intel.xdk.contacts.choose',true,true);e.cancelled=true;" +
                        "e.success=false;e.error='{0}';document.dispatchEvent(e);", "There was a problem choosing a contact.");
                    busy = false;
                    break;
                case TaskResult.None:
                    //js = new CommandResponse(new JsEvent { CommandType = CommandTypeEnum.APPMOBI_CONTACTS_CHOOSE, ErrorMessage = "There was a problem choosing the contact." }).ToString();
                    js = string.Format(" var e = document.createEvent('Events');" +
                        "e.initEvent('intel.xdk.contacts.choose',true,true);e.cancelled=false;" +
                        "e.success=false;e.error='{0}';document.dispatchEvent(e);", "There was a problem choosing the contact.");
                    break;
            }

            //InjectJS("javascript:" + js);
            InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true);
        }

        /// <summary>
        /// Add Contact Chooser Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addContactTask_Completed(object sender, SaveContactResult e)
        {
            string js = "";

            getAllContacts();

            switch (e.TaskResult)
            {
                case TaskResult.OK:
                    js = string.Format("var e = document.createEvent('Events');" + 
                        "e.initEvent('intel.xdk.contacts.add',true,true);e.success=true;" +
                        "e.contactid='{0}';document.dispatchEvent(e);", ((Microsoft.Phone.Tasks.SaveContactTask)(sender)).FirstName + " " +((Microsoft.Phone.Tasks.SaveContactTask)(sender)).LastName) ; //contactId);
                    break;

                case TaskResult.Cancel:
                    //js = new CommandResponse(new JsEvent { CommandType = CommandTypeEnum.APPMOBI_CONTACTS_ADD, Cancelled = true, ErrorMessage = "Add contact cancelled." }).ToString();
                    js = string.Format(" var e = document.createEvent('Events');" +
                        "e.initEvent('intel.xdk.contacts.add',true,true);e.cancelled=true;" +
                        "e.success=false;e.error='{0}';document.dispatchEvent(e);", "Add contact cancelled.");
                    break;

                case TaskResult.None:
                    //js = new CommandResponse(new JsEvent { CommandType = CommandTypeEnum.APPMOBI_CONTACTS_ADD, ErrorMessage = "There was a problem adding the contact." }).ToString();
                    js = string.Format(" var e = document.createEvent('Events');" +
                        "e.initEvent('intel.xdk.contacts.add',true,true);e.cancelled=false;" +
                        "e.success=false;e.error='{0}';document.dispatchEvent(e);", "There was a problem adding the contact.");
                    break;
            }
            //InjectJS("javascript:" + js);
            InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true);
            busy = false;
        }

        /// <summary>
        /// Search Contact Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Contacts_SearchCompleted(object sender, ContactsSearchEventArgs e)
        {
            string jsContacts = "[";
            bool first = true;

            foreach (Contact con in e.Results)
            {
                if (!first) jsContacts += ",";

                //GRAB CURRENT LOOKUP_KEY;  
                string jsPerson = JSONValueForPerson(con);
                jsContacts += jsPerson;
                first = false;
            }

            jsContacts += "];";

            string js = "javascript: var e = document.createEvent('Events');" +
                "e.initEvent('intel.xdk.contacts.internal.get',true,true);e.success=true;" +
                "e.contacts=" + jsContacts + ";" +
                "document.dispatchEvent(e);";

            if (isGetContacts)
            {
                isGetContacts = false;
                js += "e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.get',true,true);e.success=true;document.dispatchEvent(e);";
            }

            //InjectJS("javascript:" + jsContacts + js);
            InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true);

        }
        #endregion


        //async public void AddContactFromStore(string parameters)
        //{
        //    string[] tempParams = HttpUtility.UrlDecode(parameters).Split('~');

        //    string remoteId = tempParams[0];
        //    string givenName = tempParams[1];
        //    string familyName = tempParams[2];
        //    string street = tempParams[3];
        //    string city = tempParams[4];
        //    string state = tempParams[5];
        //    string zip = tempParams[6];
        //    string country = tempParams[7];
        //    string phone = tempParams[8];
        //    string email = tempParams[9];

        //    store = await ContactStore.CreateOrOpenAsync(ContactStoreSystemAccessMode.ReadWrite, ContactStoreApplicationAccessMode.ReadOnly);

        //    StoredContact contact = new StoredContact(store);

        //    RemoteIdHelper remoteIDHelper = new RemoteIdHelper();
        //    await remoteIDHelper.SetRemoteIdGuid(store);
        //    contact.RemoteId = await remoteIDHelper.GetTaggedRemoteId(store, remoteId);

        //    contact.GivenName = givenName;
        //    contact.FamilyName = familyName;

        //    string address = street + Environment.NewLine + city + ", " + state + " " + zip + Environment.NewLine + country;

        //    IDictionary<string, object> props = await contact.GetPropertiesAsync();
        //    props.Add(KnownContactProperties.Email, email);
        //    //props.Add(KnownContactProperties.Address, address);
        //    props.Add(KnownContactProperties.Telephone, phone);

        //    //IDictionary<string, object> extprops = await contact.GetExtendedPropertiesAsync();
        //    //extprops.Add("Codename", codeName);

        //    await contact.SaveAsync();
        //}

        #region Private methods
        async private void EditContact(string parameters)
        {
            string js = "";

            // WP8 does not allow the editing of the contact.  Just return NOT AVAILABLE IN WINDOES PHONE 8 message
            js = "javascript: var e = document.createEvent('Events');" +
                "e.initEvent('intel.xdk.contacts.edit',true,true);e.success=false;" +
                "e.error='NOT AVAILABLE IN WINDOES PHONE 8';document.dispatchEvent(e);";
            //InjectJS("javascript:" + js);
            InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true);
            return;

            string[] args = WPCordovaClassLib.Cordova.JSON.JsonHelper.Deserialize<string[]>(parameters);

            getAllContacts();

            string contactId = HttpUtility.UrlDecode(args[0]);

            if (busy == true)
            {
                js = "javascript: var e = document.createEvent('Events');" +
                    "e.initEvent('intel.xdk.contacts.busy',true,true);e.success=false;" +
                    "e.message='busy';document.dispatchEvent(e);";
                //InjectJS("javascript:" + js);
                InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true);
                return;
            }

            try
            {
                ContactStore store = await ContactStore.CreateOrOpenAsync(ContactStoreSystemAccessMode.ReadWrite, ContactStoreApplicationAccessMode.ReadOnly);

                StoredContact contact = await store.FindContactByRemoteIdAsync(contactId);
                if (contact != null)
                {
                }
                else
                {
                    js = string.Format("javascript: var e = document.createEvent('Events');" +
                        "e.initEvent('intel.xdk.contacts.edit',true,true);e.success=false;" +
                        "e.error='contact not found';e.contactid='{0}';document.dispatchEvent(e);", contactId);
                    //InjectJS("javascript:" + js);
                    InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true);
                    return;
                }
            }
            catch (Exception e)
            {
                js = string.Format("javascript: var e = document.createEvent('Events');" +
                    "e.initEvent('intel.xdk.contacts.edit',true,true);e.success=false;" +
                    "e.error='{0}';e.contactid='{1}';document.dispatchEvent(e);", e.Message, contactId);
                //InjectJS("javascript:" + js);
                InvokeCustomScript(new ScriptCallback("eval", new string[] { js }), true);
                return;
            }
        }

        /// <summary>
        /// Update Contact Information
        /// </summary>
        /// <param name="remoteId"></param>
        /// <param name="givenName"></param>
        /// <param name="familyName"></param>
        /// <param name="email"></param>
        /// <param name="codeName"></param>
        async private void UpdateContact(string remoteId, string givenName, string familyName, string email, string codeName)
        {
            store = await ContactStore.CreateOrOpenAsync(ContactStoreSystemAccessMode.ReadWrite, ContactStoreApplicationAccessMode.ReadOnly);

            string taggedRemoteId = await GetTaggedRemoteId(store, remoteId);
            StoredContact contact = await store.FindContactByRemoteIdAsync(taggedRemoteId);

            if (contact != null)
            {
                contact.GivenName = givenName;
                contact.FamilyName = familyName;

                IDictionary<string, object> props = await contact.GetPropertiesAsync();
                props[KnownContactProperties.Email] = email;

                IDictionary<string, object> extprops = await contact.GetExtendedPropertiesAsync();
                extprops["Codename"] = codeName;

                await contact.SaveAsync();
            }
        }

        /// <summary>
        /// Delete Contact
        /// </summary>
        /// <param name="id"></param>
        async private void DeleteContact(string id)
        {

            store = await ContactStore.CreateOrOpenAsync(ContactStoreSystemAccessMode.ReadWrite, ContactStoreApplicationAccessMode.ReadOnly);
            StoredContact contact = await store.FindContactByIdAsync(id);
           
            
            await store.DeleteContactAsync(id);
        }

        /// <summary>
        /// Retrieve list of contacts from the store
        /// </summary>
        /// <param name="parameters"></param>
        async public void GetContactsFromStore(string parameters)
        {
            store = await ContactStore.CreateOrOpenAsync(ContactStoreSystemAccessMode.ReadWrite, ContactStoreApplicationAccessMode.ReadOnly);
            ContactQueryOptions options = new ContactQueryOptions();
            options.DesiredFields.Add(KnownContactProperties.Email);
            options.DesiredFields.Add(KnownContactProperties.Address);
            options.DesiredFields.Add(KnownContactProperties.Telephone);

            ContactQueryResult result = store.CreateContactQuery(options);

            IReadOnlyList<StoredContact> contacts = await result.GetContactsAsync();
            string jsContacts = "AppMobi.people = [";

            foreach (StoredContact con in contacts)
            {
                IDictionary<string, object> temps = await con.GetPropertiesAsync();
                string displayName = "";
                Windows.Phone.PersonalInformation.ContactAddress address;
                string familyName = "";
                string givenName = "";
                string email = "";
                string telephone = "";

                if (temps.ContainsKey("DisplayName"))
                    displayName = (string)temps["DisplayName"];

                if (temps.ContainsKey("Address"))
                    address = (Windows.Phone.PersonalInformation.ContactAddress)temps["Address"];

                if (temps.ContainsKey("FamilyName"))
                    familyName = (string)temps["FamilyName"];

                if (temps.ContainsKey("GivenName"))
                    givenName = (string)temps["GivenName"];

                if (temps.ContainsKey("Email"))
                    email = (string)temps["Email"];

                if (temps.ContainsKey("Telephone"))
                    telephone = (string)temps["Telephone"];
            }

            jsContacts += "];";
        }

        /// <summary>
        /// Get a list of all Contacts in the store
        /// </summary>
        private void getAllContacts()
        {
            Contacts cons = new Contacts();

            cons.SearchCompleted += new EventHandler<ContactsSearchEventArgs>(Contacts_SearchCompleted);
            cons.SearchAsync(String.Empty, FilterKind.None, "Get All Contacts");
        }

        /// <summary>
        /// Convert the Contact class to a JSON representation
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        private string JSONValueForPerson(Contact contact)
        {   
            //PROCESS NAME ELEMENTS FOR CURRENT CONTACT ID
            String firstName = "",lastName = "", compositeName = "", id="";

            if (contact != null)
            {
                id = "Temp";

                if (contact.CompleteName != null)
                {
                    firstName = contact.CompleteName.FirstName;
                    lastName = contact.CompleteName.LastName;
                }
                compositeName = firstName + " " + lastName;

                firstName = (firstName == null)?"":EscapeStuff(firstName);
                lastName = (lastName == null)?"":EscapeStuff(lastName);
                compositeName = (compositeName == null)?"":EscapeStuff(compositeName);
            }

            //PROCESS EMAIL ADDRESES FOR CURRENT CONTACT ID
            String emailAddresses = "[]";
            if (contact.EmailAddresses.Count() > 0)
            {
                bool first = true;

                emailAddresses = "[";
                foreach (var emailAdddress in contact.EmailAddresses)
                {
                    if (!first) emailAddresses += ",";

                    String email = emailAdddress.EmailAddress;
                    email = EscapeStuff(email);
                    //String emailType = emailCur.getString(emailCur.getColumnIndex(ContactsContract.CommonDataKinds.Email.TYPE)); 

                    emailAddresses += "\"" + email + "\"";
                   first = false;
                } 
                emailAddresses += "]";
            }


            //PROCESS PHONE NUMBERS FOR CURRENT CONTACT ID
            String phoneNumbers = "[]";
            if (contact.PhoneNumbers.Count() > 0)
            {
                bool first = true;
                
                phoneNumbers = "[";
                foreach (var phoneNumber in contact.PhoneNumbers)
                {
                    if (!first) phoneNumbers += ",";

                    String phoneNum = phoneNumber.PhoneNumber;
                    phoneNum = EscapeStuff(phoneNum);
                    phoneNumbers += "\"" + phoneNum + "\"";
                    first = false;
                } 
                phoneNumbers += "]";
            }


            //PROCESS STREET ADDRESSES FOR CURRENT CONTACT ID
            String streetAddresses = "[]";
            if (contact.Addresses.Count() > 0)
            {
                bool first = true;
                
                streetAddresses = "[";
                foreach (var address in contact.Addresses)
                {
                    if (!first) streetAddresses += ",";

                    String street = address.PhysicalAddress.AddressLine1.Replace("\n", " ").Replace("\r", "");
                    String city = address.PhysicalAddress.City;
                    String state = address.PhysicalAddress.StateProvince;
                    String zip = address.PhysicalAddress.PostalCode;
                    String country = address.PhysicalAddress.CountryRegion;


                    street = EscapeStuff(street);
                    city = EscapeStuff(city);
                    state = EscapeStuff(state);
                    zip = EscapeStuff(zip);
                    country = EscapeStuff(country);

                    String addressstr = string.Format("{{ \"street\":\"{0}\", \"city\":\"{1}\", \"state\":\"{2}\", \"zip\":\"{3}\", \"country\":\"{4}\" }}", 
                                            street, city, state, zip, country);
                    streetAddresses += addressstr;
                    first = false;
                } 
                streetAddresses += "]";
            }

            string jsPerson = string.Format("{{ \"id\":\"{0}\", \"name\":\"{1}\", \"first\":\"{2}\", \"last\":\"{3}\", \"phones\":{4}, \"emails\":{5}, \"addresses\":{6} }}",
                                    contact.GetHashCode(), compositeName, firstName, lastName, phoneNumbers, emailAddresses, streetAddresses);
            return jsPerson.Replace("\r\n|\\r\\n|\\r|\\n", "\\\\n");

        }

        /// <summary>
        /// Escape the string
        /// </summary>
        /// <param name="myString"></param>
        /// <returns></returns>
        private string EscapeStuff(string myString)
        {
            if(myString == null)
                return "";
            myString = myString.Replace("\\\\", "\\\\\\\\");
            myString = myString.Replace("'", "\\\\'");
            return myString;
        }
        
        public async Task<string> GetTaggedRemoteId(ContactStore store, string remoteId)
        {
            string taggedRemoteId = string.Empty;

            System.Collections.Generic.IDictionary<string, object> properties;
            properties = await store.LoadExtendedPropertiesAsync().AsTask<System.Collections.Generic.IDictionary<string, object>>();
            if (properties.ContainsKey(ContactStoreLocalInstanceIdKey))
            {
                taggedRemoteId = string.Format("{0}_{1}", properties[ContactStoreLocalInstanceIdKey], remoteId);
            }
            else
            {
                // handle error condition
            }

            return taggedRemoteId;
        }
        #endregion
    }
}
