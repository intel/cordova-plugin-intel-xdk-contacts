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


    var commandProxy = require('cordova/windows8/commandProxy');

    module.exports = {
        busy: false,

        getContactInfo: function (successCallback, errorCallback, params) {
            intelxdk = {};
            intelxdk.people = [];
            successCallback();
        },

        addContact: function (successCallback, errorCallback, params) {
            var e = document.createEvent('Events');
            e.initEvent('intel.xdk.contacts.add', true, true);
            e.success = false;
            e.message = 'Add Contact not supported for Windows 8.';
            document.dispatchEvent(e);
        },

        chooseContact: function (successCallback, errorCallback, params) {
            var e = document.createEvent('Events');
            e.initEvent('intel.xdk.contacts.choose', true, true);
            e.success = false;
            e.message = 'Choose Contact not supported for Windows 8.';
            document.dispatchEvent(e);

            //var me = module.exports;

            //me.busy = true;

            //var contactPicker = Windows.ApplicationModel.Contacts.ContactPicker();
            //contactPicker.commitButtonText = "Select";

            //contactPicker.pickSingleContactAsync().done(function (contact) {
            //    if (contact !== null) {
            //        // Display the name
            //        //contactElement.appendChild(createTextElement("h3", contact.displayName));
                        
            //        var e = document.createEvent('Events');
            //        e.initEvent('intel.xdk.contacts.choose',true,true);
            //        e.success=true;
            //        e.contactid = contact.Name;
            //        document.dispatchEvent(e);
            //    } else {
            //        var ev = document.createEvent('Events');
            //        ev.initEvent('intel.xdk.contacts.choose', true, true);
            //        ev.success = false;
            //        ev.message = 'User canceled';
            //        document.dispatchEvent(ev);
            //    }

            //    me.busy = false;
            //});
        },

        editContact: function (successCallback, errorCallback, params) {
            var e = document.createEvent('Events');
            e.initEvent('intel.xdk.contacts.edit', true, true);
            e.success = false;
            e.message = 'Edit Contact not supported for Windows 8.';
            document.dispatchEvent(e);
        },

        getContacts: function (successCallback, errorCallback, params) {
            var e = document.createEvent('Events');
            e.initEvent('intel.xdk.contacts.get', true, true);
            e.success = false;
            e.message = 'Get Contacts not supported for Windows 8.';
            document.dispatchEvent(e);
        },

        removeContact: function (successCallback, errorCallback, params) {
            var e = document.createEvent('Events');
            e.initEvent('intel.xdk.contacts.remove', true, true);
            e.success = false;
            e.message = 'Remove Contact not supported for Windows 8.';
            document.dispatchEvent(e);
        }
    };

    commandProxy.add('IntelXDKContacts', module.exports);

