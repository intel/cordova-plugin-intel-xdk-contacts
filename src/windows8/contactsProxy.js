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


    // This try/catch is temporary to maintain backwards compatibility. Will be removed and changed to just 
    // require('cordova/exec/proxy') at unknown date/time.
    var commandProxy;
    try {
        commandProxy = require('cordova/windows8/commandProxy');
    } catch (e) {
        commandProxy = require('cordova/exec/proxy');
    }

    module.exports = {
        busy: false,

        getContactInfo: function (successCallback, errorCallback, params) {
            intelxdk = {};
            intelxdk.people = [];
            successCallback();
        },

        addContact: function (successCallback, errorCallback, params) {
            var me = module.exports;
            /*var e = document.createEvent('Events');
            e.initEvent('intel.xdk.contacts.add', true, true);
            e.success = false;
            e.message = 'Add Contact not supported for Windows 8.';
            document.dispatchEvent(e);*/
            me.createAndDispatchEvent("intel.xdk.contacts.add",
                {
                    success: false,
                    message: "Add Contact not supported for Windows 8"
                });
        },

        chooseContact: function (successCallback, errorCallback, params) {
            var me = module.exports;
            /*var e = document.createEvent('Events');
            e.initEvent('intel.xdk.contacts.choose', true, true);
            e.success = false;
            e.message = 'Choose Contact not supported for Windows 8.';
            document.dispatchEvent(e);*/
            me.createAndDispatchEvent("intel.xdk.contacts.choose",
                {
                    success: false,
                    message: "Choose Contact not supported for Windows 8"
                });
        },

        editContact: function (successCallback, errorCallback, params) {
            var me = module.exports;
            /*var e = document.createEvent('Events');
            e.initEvent('intel.xdk.contacts.edit', true, true);
            e.success = false;
            e.message = 'Edit Contact not supported for Windows 8.';
            document.dispatchEvent(e);*/
            me.createAndDispatchEvent("intel.xdk.contacts.edit",
                {
                    success: false,
                    message: "Edit Contact not supported for Windows 8"
                });
        },

        getContacts: function (successCallback, errorCallback, params) {
            var me = module.exports;
            /*var e = document.createEvent('Events');
            e.initEvent('intel.xdk.contacts.get', true, true);
            e.success = false;
            e.message = 'Get Contacts not supported for Windows 8.';
            document.dispatchEvent(e);*/
            me.createAndDispatchEvent("intel.xdk.contacts.get",
                {
                    success: false,
                    message: "Get Contact not supported for Windows 8"
                });
        },

        removeContact: function (successCallback, errorCallback, params) {
            var me = module.exports;
            /*var e = document.createEvent('Events');
            e.initEvent('intel.xdk.contacts.remove', true, true);
            e.success = false;
            e.message = 'Remove Contact not supported for Windows 8.';
            document.dispatchEvent(e);*/
            me.createAndDispatchEvent("intel.xdk.contacts.remove",
                {
                    success: false,
                    message: "Remove Contact not supported for Windows 8"
                });
        },

        createAndDispatchEvent: function (name, properties) {
            var e = document.createEvent('Events');
            e.initEvent(name, true, true);
            if (typeof properties === 'object') {
                for (key in properties) {
                    e[key] = properties[key];
                }
            }
            document.dispatchEvent(e);
        }
    };

    commandProxy.add('IntelXDKContacts', module.exports);

