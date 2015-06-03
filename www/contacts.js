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

var channel = require('cordova/channel'),
    utils = require('cordova/utils'),
    exec = require("cordova/exec");

module.exports = {
    people: [],

    /** Initialize the plugin internal contact list from the system contacts database.
     *
     *  This plugin maintains an internal copy of the system contacts database. The 
     *  getContactList() and getContactData() functions retrieve information from the internal
     *  copy, not directly from the system database. Therefore, the internal copy must be
     *  initialized from the system database before getContactList() or getContactData() is
     *  called. That initialization can occur explicitly with a call to this getContacts(), or
     *  implicitly with a successful call to addContact(), removeContact(), chooseContact(), or
     *  editContact().
     *
     *  Events fired:
	 *
	 *  -   intel.xdk.contacts.permissionDenied: The application does not have permission to 
	 *      access the system contacts database. The internal list has not been initialized.
	 *  -   intel.xdk.contacts.get(success): The internal list has been initialized. success 
	 *      will always be true.
	 */
	getContacts : function(){
		exec(null, null, "IntelXDKContacts", "getContacts", []);
	},

	/** Get the internal contact list.
	 *
	 *  @return An array of contact ids. Contact ids are described in the description
	 *          of getContactData().
     *              
     *  @note   This function retrieves data from an internal copy of the system contact
     *          database. If the internal copy has not been initialized from the system
     *          database, then this function will return an empty array. See the description
     *          of getContacts() for more details.
     */
	getContactList : function(){
		var contactList = [];
		var contacts = this.people;
		if(contacts != null && contacts.length > 0){
			for(var i = 0 ; i < contacts.length ; i ++){
				contactList.push(contacts[i].id);
			}
		}
		return contactList;
	},

    /** Get a single contact object from the internal contact list.
     *
     *  @param  contactID The id field of the contact object to be returned.
     *
     *  @return The contact object in the internal contact list whose id field matches the
     *          @a contactID argument, or @c null if there is no such contact.
     *
     *  The properties of a contact object are:
     *
     *  -   id:         An identifier which is unique to this contact, and which can be used to
     *                  refer to it in other function calls. It may be either a number or a
     *                  string, depending on the platform.
     *  -   last:       A string containing the last name of the person.
     *  -   first:      A string containing the first name of the person.
     *  -   name:       A string containing the entire name of the person. (Not necessarily
     *                  just the concatenation of the first and last names.)
     *  -   emails:     An array of strings containing email addresses.
     *  -   phones:     An array of strings containing phone numbers.
     *  -   addresses:  An array of objects representing addresses.
     *
     *  The properties of an address object are:
     *
     *  -   street:     A string containing the street address. (May contain multiple lines
     *                  separated by line-feed characters.)
     *  -   city:       A string containing the city name.
     *  -   state:      A string containing a state or province name or abbreviation.
     *  -   zip:        A string containing a zip or postal code.
     *  -   country:    A string containing a country name or abbreviation.
     *              
     *  @note   This function retrieves data from an internal copy of the system contact
     *          database. If the internal copy has not been initialized from the system
     *          database, then this function will always return null. See the description
     *          of getContacts() for more details.
     */
	getContactData : function(contactID){
	    var contacts = this.people;
		if(contacts != null && contacts.length > 0){
			for(var i = 0 ; i < contacts.length ; i ++){
				if(contacts[i].id == contactID){
					return contacts[i];
				}
			}
		}
		return null;
	},

	/** Add a contact to the system contact database.
	 *
	 *  Invokes the system "add contact" dialog.
	 *
	 *  Events fired:
	 *
	 *  -   intel.xdk.contacts.permissionDenied: The application does not have permission to 
	 *      access the system contacts database.
	 *  -   intel.xdk.contacts.busy: Some other contacts action has been initiated and is not 
	 *      yet complete.
	 *  -   intel.xdk.contacts.add(success, contactid): If success=true, then the user added
	 *      a new contact whose id is contactid, and the internal contact list has been updated.
	 *      If success=false, the user cancelled the dialog without adding a contact,
	 *      contactid is undefined, and the internal contact list is unchanged.
	 *
	 *  @note   If the internal contact list was uninitialized before calling addContact(), then
	 *          it will still be uninitialized if the user canceled the add operation (i.e.,
	 *          if success=false in the intel.xdk.contacts.add event).
	 */
	addContact : function(){
		exec(null, null, "IntelXDKContacts", "addContact", []);
	},

	/** Choose a contact from the system contact database.
	 *
	 *  Invokes the system "choose contact" dialog.
	 *
	 *  Events fired:
	 *
	 *  -   intel.xdk.contacts.permissionDenied: The application does not have permission to 
	 *      access the system contacts database.
	 *  -   intel.xdk.contacts.busy: Some other contacts action has been initiated and is not 
	 *      yet complete.
	 *  -   intel.xdk.contacts.choose(success, contactid): If success=true, then the user
	 *      selected the contact whose id is contactid. If success=false, the user cancelled
	 *      the dialog without choosing a contact, and contactid is undefined. In either
	 *      case, the internal contact list is unchanged.
	 *
	 *  @note   If the internal contact list was uninitialized before calling chooseContact(),
	 *          then it will still be uninitialized if the user canceled the choose operation
	 *          (i.e., if success=false in the intel.xdk.contacts.choose event).
	 */
	chooseContact : function(){
		exec(null, null, "IntelXDKContacts", "chooseContact", []);
	},

	/** Edit a contact in the system contact database.
	 *
	 *  Invokes the system "edit contact" dialog.
	 *
	 *  @param contactID    The id for the contact to be edited.
	 *
	 *  Events fired:
	 *
	 *  -   intel.xdk.contacts.permissionDenied: The application does not have permission to 
	 *      access the system contacts database.
	 *  -   intel.xdk.contacts.busy: Some other contacts action has been initiated and is not 
	 *      yet complete.
	 *  -   intel.xdk.contacts.edit(success, contactid): If success=true, then the user edited
	 *      the contact with the specified id and the internal contacts list has been updated.
	 *      If success=false, the user cancelled editing and the internal contacts list is
	 *      unchanged. In either case, contactid is the same as the contactID parameter.
	 *
	 *  @note   If the internal contact list was uninitialized before calling addContact(), then
	 *          it will still be uninitialized if the user canceled the edit operation (i.e.,
	 *          if success=false in the intel.xdk.contacts.edit event).
	 */
	editContact : function(contactID){
		exec(null, null, "IntelXDKContacts", "editContact", [contactID]);
	},

	/** Remove a contact from the system contact database.
	 *
	 *  @param contactID    The id for the contact to be removed.
	 *
	 *  Events fired:
	 *
	 *  -   intel.xdk.contacts.permissionDenied: The application does not have permission to 
	 *      access the system contacts database.
	 *  -   intel.xdk.contacts.busy: Some other contacts action has been initiated and is not 
	 *      yet complete.
	 *  -   intel.xdk.contacts.remove(success, contactid, error): If success=true, then the 
	 *      contact with the specified id was removed from the system contacts database,
	 *      error is undefined, and the contacts list has been updated. If success=false,
	 *      the system contacts database and the contacts list are unchanged and error is a
	 *      string describing the reason the contact was not removed. In either case, contactid
	 *      is the same as the contactID parameter.
	 *
	 *  @note   If the internal contact list was uninitialized before calling removeContact(),
	 *          then it will still be uninitialized if the remove operation failed for any
	 *          reason (i.e., if success=false in the intel.xdk.contacts.remove event).
	 */
	removeContact : function(contactID){
		exec(null, null, "IntelXDKContacts", "removeContact", [contactID]);
	}
}


//intelxdk.people maintenance
var me = module.exports;

document.addEventListener('intel.xdk.contacts.internal.get', function (e) {
    me.people = e.contacts;
}, false);
