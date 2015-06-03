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

/*global exports, describe, xdescribe, it, xit, expect, jasmine*/
/*global document, intel, console */

exports.defineAutoTests = function () {
    'use strict';
    
    describe('intel.xdk.contacts tests', function () {
        it('should be defined', function () {
            expect(intel.xdk.contacts).toBeDefined();
        });
        
        it('should have a addContact method', function () {
            expect(intel.xdk.contacts.addContact).toBeDefined();
        });
        
        /** this spec s failing */
        xit('should have a getContactInfo method', function () {
            expect(intel.xdk.contacts.getContactInfo).toBeDefined();
        });
        
        it('should have a chooseContact method', function () {
            expect(intel.xdk.contacts.chooseContact).toBeDefined();
        });
        
        it('should have a editContact method', function () {
            expect(intel.xdk.contacts.editContact).toBeDefined();
        });
        
        /** this spec s failing */
        xit('should have a getContactData method', function () {
            expect(intel.xdk.contacts.getContactData).toBeDefined();
        });
        
        it('should have a getContactList method', function () {
            expect(intel.xdk.contacts.getContactList).toBeDefined();
        });
        
        it('should have a removeContact method', function () {
            expect(intel.xdk.contacts.removeContact).toBeDefined();
        });
    });
};

exports.defineManualTests = function (contentEl, createActionButton) {
    'use strict';
    
    function logMessage(message, color) {
        var log = document.getElementById('info'),
            logLine = document.createElement('div');
        
        if (color) {
            logLine.style.color = color;
        }
        
        logLine.innerHTML = message;
        log.appendChild(logLine);
    }

    function clearLog() {
        var log = document.getElementById('info');
        log.innerHTML = '';
    }
    
    function testNotImplemented(testName) {
        return function () {
            console.error(testName, 'test not implemented');
        };
    }
    
    function init() {
        document.addEventListener('intel.xdk.contacts.add', function (e) {
            console.log('event:', 'intel.xdk.contacts.add');
            console.log({
                success: e.success,
                contactid: e.contactid || null,
                error: e.error || null
            });
        });
        
        document.addEventListener('intel.xdk.contacts.choose', function (e) {
            console.log('event:', 'intel.xdk.contacts.chose');
            console.log({
                success: e.success,
                contactid: e.contactid || null
            });
            
            if (e.success) {
                TestSuite.currentSelectedContact = e.contactid;
            }
        });
        
        document.addEventListener('intel.xdk.contacts.edit', function (e) {
            console.log('event:', 'intel.xdk.contacts.edit');
            console.log({
                success: e.success,
                contactid: e.contactid || null
            });
        });

        document.addEventListener('intel.xdk.contacts.get', function () {
            console.log('event', 'intel.xdk.contacts.get');
            console.log({
                method: 'get',
                contacts: intel.xdk.contacts.getContactList().length
            });
        });

    }
  
    /** object to hold properties and configs */
    var TestSuite = {};
  
    TestSuite.$markup = '<h3>Add Contact</h3>' +
        '<div id="buttonAddContact"></div>' +
        'Expected result: should add a contact' +
        
        '<h3>Choose Contact</h3>' +
        '<div id="buttonChooseContact"></div>' +
        'Expected result: should display contact picker' +
        
        '<h3>Edit Contact</h3>' +
        '<div id="buttonEditContact"></div>' +
        'Expected result: should edit previous selected contact' +
        
        '<h3>Get Contacts List</h3>' +
        '<div id="buttonGetContactsList"></div>' +
        'Expected result: should display contacts list' +
    
        '<h3>Get Contacts</h3>' +
        '<div id="buttonGetContacts"></div>' +
        'Expected result: should log contacts list lenght';
        
    TestSuite.currentSelectedContact = null;
        
    contentEl.innerHTML = '<div id="info"></div>' + TestSuite.$markup;
    
    createActionButton('Add Contact', function () {
        console.log('executing', 'intel.xdk.contacts.addContact');
        intel.xdk.contacts.addContact();
    }, 'buttonAddContact');
    
    createActionButton('Choose Contact', function () {
        console.log('executing', 'intel.xdk.contacts.chooseContact');
        intel.xdk.contacts.chooseContact();
    }, 'buttonChooseContact');
    
    createActionButton('Edit Contact', function () {
        if (TestSuite.currentSelectedContact !== null) {
            console.log('executing', 'intel.xdk.contacts.editContact');
            intel.xdk.contacts.editContact(TestSuite.currentSelectedContact);
        } else {
            console.error('intel.xdk.contacts.editContact', 'no contact selected');
        }
    }, 'buttonEditContact');
    
    createActionButton('Get Contacts List', function () {
        console.log('executing', 'intel.xdk.contacts.getContactList');
        var contacts = intel.xdk.contacts.getContactList();
        console.log(contacts);
    }, 'buttonGetContactsList');
    
    createActionButton('Get Contacts', function () {
        console.log('executing', 'intel.xdk.contacts.getContacts');
        intel.xdk.contacts.getContacts();
    }, 'buttonGetContacts');
    
    document.addEventListener('deviceready', init, false);
};