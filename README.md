intel.xdk.contacts
==================

For access to the on-device contacts database.

Description
-----------

The contacts plugin gives programmers access to the default contacts database on
the native device. The application must be built with appropriate permissions in
order for this capability to work.

### Contact Objects

The plugin vends “contact objects” which represent entries in the native
contacts database. Each contact object has the following properties:

-   **id:** An identifier which is unique to this contact, and which can be used
    to refer to it in plugin method calls. It may be either a number or a
    string, depending on the platform.
-   **last:** A string containing the last name of the person.
-   **first:** A string containing the first name of the person.
-   **name:** A string containing the entire name of the person. (Not
    necessarily just the concatenation of the first and last names.)
-   **emails:** An array of strings containing email addresses.
-   **phones:** An array of strings containing phone numbers.
-   **addresses:**  An array of objects representing addresses.

An address object has the following properties:

-   **street:** A string containing the street address. (May contain multiple
    lines separated by line-feed characters.)
-   **city:** A string containing the city name.
-   **state:** A string containing a state or province name or abbreviation.
-   **zip:** A string containing a zip or postal code.
-   **country:** A string containing a country name or abbreviation.

### The Internal Contact List

The plugin keeps an internal contact list, which is a cached copy of the device
contacts database. The internal contact list is _not_ initialized automatically,
and a call to [getContactList](#getcontactlist) or
[getContactData](#getcontactdata) will return `null` if it has not been
initialized.

The internal contact list may be explicitly initialized by a call to
[getContacts](#getcontacts), or implicitly initialized or updated by a
successful call to [addContact](#addcontact), [chooseContact](#choosecontact),
[editContact](#editcontact), or [removeContact](#removecontact).

Note that a change to the device contacts database made other than through the
plugin will not be reflected in the internal contact list until it is
initialized or updated.

### Methods

Note that methods that access or manipulate the contacts database may present
the platform native interface for that functionality.

-   [addContact](#addcontact) — Allow the user to add a contact.
-   [chooseContact](#choosecontact) — Allow the user to choose a contact.
-   [editContact](#editcontact) — Allow the user to edit a contact.
-   [getContactData](#getcontactdata) — Return the contact object with a
    particular identifier.
-   [getContactList](#getcontactlist) — Return an array of the identifiers of
    all contacts in the database.
-   [getContacts](#getcontacts) — Fetch the device contact database into the
    plugin.
-   [removeContact](#removecontact) — Remove a specified contact.

### Events

-   [intel.xdk.contacts.add](#add) — The user has added a contact.
-   [intel.xdk.contacts.busy](#busy) — An operation was requested when another
    operation was already in progress.
-   [intel.xdk.contacts.choose](#choose) — The user has selected a contact.
-   [intel.xdk.contacts.edit](#edit) — The user has finished editing a contact.
-   [intel.xdk.contacts.get](#get) — The device contact database has been
    fetched into the plugin.
-   [intel.xdk.contacts.remove](#remove) — A contact has been removed.

Methods
-------

### addContact

Invokes the system “add contact” dialog.

```javascript
intel.xdk.contacts.addContact();
```

#### Description

This method displays the native device’s UI to allow the user to add a new
contact.

#### Platforms

-   Apple iOS
-   Google Android

#### Events

-   **[intel.xdk.contacts.permissionDenied](#permissiondenied):** The
    application does not have permission to access the system contacts database.
-   **[intel.xdk.contacts.busy](#busy):** Some other contacts action has been
    initiated and is not yet complete.
-   **[intel.xdk.contacts.add(_success_, _contactid_)](#add):** If _success_ is
    `true`, then the user added a new contact whose id is _contactid_. If
    _success_ is `false`, the user cancelled the dialog, and _contactid_ is
    undefined.

>   **Note:** If [the internal contact list](#the-internal-contact-list) was
>   uninitialized before the call and the user canceled the add operation (i.e.,
>   _success_ was `false`), then the internal contact list is still
>   uninitialized.

#### Example

```javascript
document.addEventListener('intel.xdk.contacts.add', onContactAdded);

intel.xdk.contacts.addContact();

function onChooseContact(evt) {
   if (evt.success == true)
   {
      alert("Contact "+evt.contactid+" successfully added");
   }
   else
   {
      alert("Add Contact Cancelled");
   }
```

### chooseContact

Invokes the system “choose contact” dialog.

```javascript
intel.xdk.contacts.chooseContact();
```

#### Description

This method displays the native device’s UI to allow the user to choose a
contact from the contact database.

#### Available Platforms

-   Apple iOS
-   Google Android
-   Microsoft Windows 8 - BETA
-   Microsoft Windows Phone 8 - BETA

#### Events

-   **[intel.xdk.contacts.permissionDenied](#permissiondenied):** The
    application does not have permission to access the system contacts database.
-   **[intel.xdk.contacts.busy](#busy):** Some other contacts action has been
    initiated and is not yet complete.
-   **[intel.xdk.contacts.choose(_success_, _contactid_)](#choose):** If
    _success_ is `true`, then the selected the contact whose id is
    _contactid_. If _success_ is `false`, the user cancelled the dialog, and
    _contactid_ is undefined.

>   **Note:** If [the internal contact list](#the-internal-contact-list) was
>   uninitialized before the call and the user canceled the choose operation
>   (i.e., _success_ was `false`), then the internal contact list is still
>   uninitialized.

#### Example

```javascript
document.addEventListener('intel.xdk.contacts.choose', onChooseContact);

intel.xdk.contacts.chooseContact();

function onChooseContact(evt) {
   if (evt.success == true)
   {
      intel.xdk.contacts.editContact(evt.contactid);
   }
   else
   {
      alert("Choose Contact Cancelled");
   }
```

### editContact

Invokes the system “edit contact” dialog.

```javascript
intel.xdk.contacts.editContact(contactID);
```

#### Description

This method displays the native device's contacts application to allow the user
to edit a specified contact.

#### Platforms

-   Apple iOS
-   Google Android

#### Parameters

-   **contactID:** The contact ID of the contact to be edited.

#### Events

-   **[intel.xdk.contacts.permissionDenied](#permissiondenied):** The
    application does not have permission to access the system contacts database.
-   **[intel.xdk.contacts.busy](#busy):** Some other contacts action has been
    initiated and is not yet complete.
-   **[intel.xdk.contacts.edit(_success_, _contactid_)](#choose):** If
    _success_ is `true`, then the edited the contact whose id is
    _contactid_. If _success_ is `false`, the user cancelled the dialog. In
    either case, _contactid_ is the same as the **contactID** from the `edit`
    call.

>   **Note:** If [the internal contact list](#the-internal-contact-list) was
>   uninitialized before the call and the user canceled the edit operation
>   (i.e., _success_ was `false`), then the internal contact list is still
>   uninitialized.


#### Example

```javascript
document.addEventListener('intel.xdk.contacts.edit', onContactEdit);

intel.xdk.contacts.editContact(contactID);

function onEditContact(evt) {
   if (evt.success == true)
   {
      alert("Contact "+evt.contactid+" successfully updated");
   }
   else
   {
      alert("Edit Contact Cancelled");
   }
```

### getContactData

Get a specified contact object.

```javascript
contactObj = intel.xdk.contacts.getContactData(contactID);
```

#### Description

This method gets the [contact object](#contact-objects) with a specified contact
id.

#### Available Platforms

-   Apple iOS
-   Google Android
-   Microsoft Windows 8 - BETA
-   Microsoft Windows Phone 8 - BETA

#### Parameters

-   **contactID:** The contact ID of the contact to be fetched.

#### Returns

-   The [contact object](#contact-objects) in the
    [internal contact list](#the-internal-contact-list) whose `id`
    property is **contactID**.
-   `null` if the [internal contact list](#the-internal-contact-list) has not been
    initialized, or does not contain a contact object with the specified contact
    ID.

#### Example

```javascript
function contactsReceived() {
    var table = document.getElementById("contacts");
    table.innerHTML = '';

    var myContacts = intel.xdk.contacts.getContactList();

    for(var i=0;i<myContacts.length;i++) {
        //add row to table
        var contactInfo =
            intel.xdk.contacts.getContactData(myContacts[i]);
        var tr = document.createElement("tr");
        tr.setAttribute('id', 'pnid'+contactInfo.id);
        tr.setAttribute('onClick',
            'document.getElementById("iden").value =
            '+contactInfo.id+';');
        tr.setAttribute('style', 'background-color:#B8BFD8');
        var id = document.createElement("td");
        id.innerHTML = contactInfo.id;
        tr.appendChild(id);
        var msg = document.createElement("td");
        msg.innerHTML = contactInfo.name;
        tr.appendChild(msg);
        table.appendChild(tr);
    }
}
```

### getContactList

Get all the contact IDs.

```javascript
contactListArray = intel.xdk.contacts.getContactList();
```

#### Description

This method gets an array containing the contact ID property of every
[contact object](#contact-objects).

#### Available Platforms

-   Apple iOS
-   Google Android
-   Microsoft Windows 8 - BETA
-   Microsoft Windows Phone 8 - BETA

#### Returns

-   An array containing the contact ID property of every
    [contact object](#contact-objects).
-   `null` if the [internal contact list](#the-internal-contact-list) has not been
    initialized.

#### Example
```javascript

document.addEventListener('intel.xdk.contacts.get', contactsReceived, false);

function contactsReceived() {
    var table = document.getElementById("contacts");
    table.innerHTML = '';

    var myContacts = intel.xdk.contacts.getContactList();

    for(var i=0;i<myContacts.length;i++) {
        //add row to table
        var contactInfo = intel.xdk.contacts.getContactData(myContacts[i]);
        var tr = document.createElement("tr");
        tr.setAttribute('id', 'pnid'+contactInfo.id);
        tr.setAttribute('onClick', 'document.getElementById("iden").value =
            '+contactInfo.id+';');
        tr.setAttribute('style', 'background-color:#B8BFD8');
        var id = document.createElement("td");
        id.innerHTML = contactInfo.id;
        tr.appendChild(id);
        var msg = document.createElement("td");
        msg.innerHTML = contactInfo.name;
        tr.appendChild(msg);
        table.appendChild(tr);
    }
}
```

### getContacts

Initialize the plugin [internal contact list](#the-internal-contact-list) from the
device contacts database.

```javascript
intel.xdk.contacts.getContacts();
```

#### Description

This plugin maintains an [internal copy](#the-internal-contact-list) of the device
contacts database. The [getContactList](#getcontactlist) and
[getContactData](#getcontactdata) methods retrieve information from the internal
contact list, not directly from the device database. Therefore, the internal
copy must be initialized from the system database before
[getContactList](#getcontactlist) or [getContactData](#getcontactdata) is
called. That initialization can occur explicitly with a call to this method, or
implicitly with a successful call to [addContact](#addcontact),
[removeContact](#removecontact), [chooseContact](#choosecontact), or
[editContact](#editcontact).

#### Available Platforms

-   Apple iOS
-   Google Android
-   Microsoft Windows 8 - BETA
-   Microsoft Windows Phone 8 - BETA

#### Events

-   **[intel.xdk.contacts.permissionDenied](#permissiondenied):** The
    application does not have permission to access the system contacts database.
    The internal list has not been initialized.
-   **[intel.xdk.contacts.get(_success_)](#get):** The internal contact list has
    been initialized. _success_ will always be true.

#### Example

```javascript

document.addEventListener('intel.xdk.contacts.get', contactsReceived, false);

intel.xdk.contacts.getContacts();

```

### removeContact

Remove a contact.

```javascript
intel.xdk.contacts.removeContact(contactID);
```

#### Description

This method removes a specified contact.

#### Platforms

-   Apple iOS
-   Google Android

#### Parameters

-   **contactID:** The contact ID of the contact to remove

#### Events

-   **[intel.xdk.contacts.permissionDenied](#permissiondenied):** The
    application does not have permission to access the system contacts database.
-   **[intel.xdk.contacts.busy](#busy):** Some other contacts action has been
    initiated and is not yet complete.
-   **[intel.xdk.contacts.remove(_success_, _contactid_, _error_)](#choose):**
    If _success_ is `true`, then the contact whose id is _contactid_ has been
    removed. If _success_ is `false`, the contacts database has not been
    changed, and _error_ is a string describing the reason the contact was not
    removed.

>   **Note:** If [the internal contact list](#the-internal-contact-list) was
>   uninitialized before the call and the remove operation failed for any reason
>   (i.e., _success_ was `false`), then the internal contact list is still
>   uninitialized.


#### Example

```javascript
document.addEventListener('intel.xdk.contacts.remove', onContactRemoved);

intel.xdk.contacts.removeContact(contactID);

function onEditContact(evt) {
   if (evt.success == true)
   {
      alert("Contact "+evt.contactid+" has been removed");
   }
}
```

### add

An [addContact](#addcontact) operation is complete.

#### Description

This event is fired in response to an [addContact](#addcontact) call when the
user either finishes adding a contact or cancels the operation.

#### Properties

-   **success:** `true` if the user added a contact; `false` if the
    user canceled the operation.
-   **contactid:** The `id` property of the added contact if **success** is
    `true`; undefined if **success** is false.

### busy

An operation could not be performed because another operation was already in
progress.

#### Description

This event is fired when the [addContact](#addcontact),
[chooseContact](#choosecontact), [editContact](#editcontact), or
[removeContact](#removecontact) method is called, but one of those methods has
previously been called and has not finished.

### choose

A [chooseContact](#choosecontact) operation is complete.

#### Description

This event is fired in response to a [chooseContact](#choosecontact) call when
the user either finishes choosing a contact or cancels the operation.

#### Properties

-   **success:** `true` if the user chose a contact; `false` if the
    user canceled the operation.
-   **contactid:** The `id` property of the chosen contact if **success** is
    `true`; undefined if **success** is false.

### edit

An [editContact](#editcontact) operation is complete.

#### Description

This event is fired in response to an [editContact](#editcontact) call when the
user either finishes editing the contact or cancels the operation.

#### Properties

-   **success:** `true` if the user edited the contact; `false` if the
    user canceled the operation.
-   **contactid:** The `id` property of the contact that was to be edited,
    whether or not it was actually edited.

### get

The [internal contact list](#the-internal-contact-list) has been initialized or
updated from the device contacts database.

#### Description

This event is fired in response to a [getContacts](#getcontacts) call when the
initialization or update of the internal contacts list is complete.

#### Properties

### permissionDenied

The application does not have permission to access the system contacts database.

#### Description

An operation that needs to access the device contacts database has failed,
either because the application was not built with the privileges required to
access the database, or because the user has refused to allow the application
to access the database.

### remove

A [removeContact](#removecontact) operation is complete.

#### Description

This event is fired in response to a [removeContact](#removecontact) call when
the operation completes, either successfully or unsuccessfully.

#### Properties

-   **success:** `true` if the contact was removed; `false` otherwise.
-   **contactid:** The `id` property of the contact that was to be removed,
    whether or not it was actually removed.
-   **error:** A string describing the reason the contact was not removed if
    **success** is `false`; undefined if **success** is `true`.
