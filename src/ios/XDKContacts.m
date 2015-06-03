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

#import "XDKContacts.h"
#import <AddressBook/ABAddressBook.h>
#import <AddressBookUI/AddressBookUI.h>

// "if(1)" turns OFF XDKog logging.
// "if(0)" turns ON XDKog logging.
#define XDKLog if(1); else NSLog

@interface XDKContacts ()   < ABNewPersonViewControllerDelegate
                            , ABPersonViewControllerDelegate
                            , ABPeoplePickerNavigationControllerDelegate
                            >


//! The address book reference to be used throughout the plugin, or nil, if we do not have
//! permission to access the address book. Initialized in pluginInitialize.
@property (nonatomic)   ABAddressBookRef    addressBook;

//! Flag indicating whether the plugin is busy.
@property (nonatomic)   BOOL                busy;

//! The record ID of the contact record currently being edited.
@property (nonatomic)   NSString*           idForEditedPerson;

//! The active contact editor dialog.
@property (nonatomic)   id                  editingDoneTarget;

//! The active contact editor dialog.
@property (nonatomic)   SEL                 editingDoneAction;;

@end

@implementation XDKContacts

#pragma mark Commands

- (void) addContact:(CDVInvokedUrlCommand*)command
{
    if (! [self addressBookIsAvailable]) return;
    if ([self inUse]) return;

	ABNewPersonViewController* newPersonController = [ABNewPersonViewController new];
	newPersonController.addressBook = self.addressBook;
	newPersonController.newPersonViewDelegate = self;
    
    UINavigationController* navigationController = [[UINavigationController alloc]
                                                    initWithRootViewController:newPersonController];
	
    [self.viewController presentViewController:navigationController
                                      animated:YES
                                    completion:nil];
}


- (void) chooseContact:(CDVInvokedUrlCommand*)command
{
    if (! [self addressBookIsAvailable]) return;
    if ([self inUse]) return;
    
    ABPeoplePickerNavigationController* picker = [ABPeoplePickerNavigationController new];
    picker.peoplePickerDelegate = self;
    [self.viewController presentViewController:picker animated:YES completion:nil];
}


- (void) editContact:(CDVInvokedUrlCommand*)command
{
    ABRecordID recordID = (ABRecordID) [[command argumentAtIndex:0
                                                     withDefault:@(kABRecordInvalidID)
                                                        andClass:[NSNumber class]]
                                        integerValue];
    if (recordID == kABRecordInvalidID) {
        [self fireEvent:@"contacts.edit"
                success:NO
             components:@{ @"error": @"Record ID argument omitted" }];
        return;
    }
    
    if (! [self addressBookIsAvailable]) return;
    if ([self inUse]) return;
    
	ABRecordRef record = ABAddressBookGetPersonWithRecordID(self.addressBook, recordID);
	if (record == nil) {
        [self fireEvent:@"contacts.edit"
                success:NO
             components:@{ @"error": @"contact not found",
                           @"contactid": [NSString stringWithFormat:@"%d", recordID] }];
        self.busy = NO;
        return;
	}
    
    self.idForEditedPerson = idForPerson(record);

	ABPersonViewController* personController = [ABPersonViewController new];
	personController.displayedPerson = record;
	personController.addressBook = self.addressBook;
	personController.personViewDelegate = self;
	personController.allowsEditing = YES;
	personController.allowsActions = YES;
    personController.displayedProperties = @[ @(kABPersonFirstNameProperty),
                                              @(kABPersonLastNameProperty),
                                              @(kABPersonEmailProperty),
                                              @(kABPersonPhoneProperty),
                                              @(kABPersonAddressProperty) ];

    UINavigationController* navController = [[UINavigationController alloc]
                                             initWithRootViewController:personController];

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Warc-performSelector-leaks"
    // When the person controller has been presented, trigger its "Edit" button action. This
    // way, the user will only see the person view edit interface.
    [self.viewController presentViewController:navController
                                      animated:YES
                                    completion:^{
                                        UIBarButtonItem* editButton = personController.navigationItem.rightBarButtonItem;
                                        [editButton.target performSelector:editButton.action withObject:editButton];
#pragma clang diagnostic pop
                                    }];
    
    // Create a repeating task that will patch the Cancel and Done buttons on the person
    // controller navigation bar once they come into existence.
    [NSTimer scheduledTimerWithTimeInterval:0.1
                                     target:self
                                   selector:@selector(patchNavigationBarActions:)
                                   userInfo:personController.navigationItem
                                    repeats:YES];
}


- (void) getContacts:(CDVInvokedUrlCommand*)command
{
    if ([self addressBookIsAvailable]) {
        [self deliverContacts];
        [self fireEvent:@"contacts.get" success:YES components:nil];
    }
}


- (void) removeContact:(CDVInvokedUrlCommand*)command
{
    ABRecordID recordID = (ABRecordID) [[command argumentAtIndex:0
                                                     withDefault:@(kABRecordInvalidID)
                                                        andClass:[NSNumber class]]
                                        integerValue];
    if (recordID == kABRecordInvalidID) {
        [self fireEvent:@"contacts.remove"
                success:NO
             components:@{ @"error": @"Record ID argument omitted" }];
        return;
    }

    if (! [self addressBookIsAvailable]) return;
    if ([self inUse]) return;

	ABRecordRef record = ABAddressBookGetPersonWithRecordID(self.addressBook, recordID);
	if (record == nil) {
        [self fireEvent:@"contacts.remove"
                success:NO
             components:@{ @"error": @"contact not found",
                           @"contactid": [NSString stringWithFormat:@"%d", recordID] }];
        self.busy = NO;
        return;
	}
	
	CFErrorRef error;
	if (ABAddressBookRemoveRecord(self.addressBook, record, &error)  &&
        ABAddressBookSave(self.addressBook, &error))
    {
        [self deliverContacts];
        [self fireEvent:@"contacts.remove"
                success:YES
             components:@{ @"contactid": [NSString stringWithFormat:@"%d", recordID] }];
    }
    else {
        [self fireEvent:@"contacts.remove"
                success:NO
             components:@{ @"error": @"error deleting contact",
                           @"contactid": [NSString stringWithFormat:@"%d", recordID] }];
    }
	self.busy = NO;
}


#pragma mark - Utility methods

//! Fire a JavaScript event.
//!
//! Generates a string of JavaScript code to create and dispatch an event.
//! @param eventName    The name of the event (not including the @c "intel.xdk." prefix).
//! @param success      The boolean value to assign to the @a success field in the
//!                     event object.
//! @param components   Each key/value pair in this dictionary will be incorporated.
//!                     (Note that the value must be a string which is the JavaScript
//!                     representation of the value - @c "true" for a boolean value,
//!                     @c "'Hello'" for a string, @c "20" for a number, etc.)
//!
//! @see fireEvent:success:components:internal:
//!
- (void) fireEvent:(NSString*)eventName
           success:(BOOL)success
        components:(NSDictionary*)components
{
    NSMutableString* eventComponents = [NSMutableString string];
    for (NSString *eachKey in components) {
        [eventComponents appendFormat:@"e.%@ = %@;", eachKey, components[eachKey]];
    }
    NSString* script = [NSString stringWithFormat:@"var e = document.createEvent('Events');"
                        "e.initEvent('intel.xdk.%@', true, true);"
                        "e.success = %@;"
                        "%@"
                        "document.dispatchEvent(e);",
                        eventName,
                        (success ? @"true" : @"false"),
                        eventComponents];
    XDKLog(@"%@", script);
    [self.commandDelegate evalJs:script];
}


//! Test whether the system address book has been successfully accessed.
//!
//! If not, then fire a @c contacts.permissionDenied event.
//!
//! @return YES if the address book is available, NO otherwise.
//!
- (BOOL) addressBookIsAvailable
{
    if (self.addressBook) {
        return YES;
    } else {
        [self fireEvent:@"contacts.permissionDenied" success:NO components:nil];
        return NO;
    }
}


//! Is there already an action in progress which can't be run safely in parallel with another
//! action?
//!
//! If so, fire a @c contacts.busy event; if not, then there is now.
//!
//! @return YES if already in use, NO otherwise.
//!
- (BOOL) inUse
{
    if (self.busy) {
        [self fireEvent:@"contacts.busy"
                success:NO
             components:@{ @"message": @"'busy'" }];
        return YES;
        
    }
    else {
        self.busy = YES;
        return NO;
    }
}


//! Return a string containing the ID field of a person object.
static NSString* idForPerson(ABRecordRef person)
{
    return [NSString stringWithFormat:@"%d", ABRecordGetRecordID(person)];
}


#pragma mark Address book marshalling and delivery

//! Create an NSString containinng a copy of the value of an address field.
//! @param dict An address dictionary from an address book person record.
//! @param key  The key for an address field.
//! @return     An NSString which is a copy of the string value of the field in the
//!             address dictionary identified by the key.
//!
static NSString* addressField(CFDictionaryRef dict, const void* key)
{
    CFTypeRef value = CFDictionaryGetValue(dict, key);
    return value == nil ? @"" : [NSString stringWithString:(__bridge NSString*)value];
}


//! Create a dictionary from an address book person record which mirrors the
//! Javascript contact object that will represent that record.
//!
- (NSDictionary*) contactForPerson:(ABRecordRef)person
{
    NSMutableDictionary* contact = [NSMutableDictionary dictionary];

    contact[@"id"] = @(ABRecordGetRecordID(person));
    NSString* first = (__bridge_transfer NSString*)ABRecordCopyValue(person, kABPersonFirstNameProperty);
    contact[@"first"] = first ? first : @"";
    NSString* last = (__bridge_transfer NSString*)ABRecordCopyValue(person, kABPersonLastNameProperty);
    contact[@"last"] = last ? last : @"";
    NSString* name = (__bridge_transfer NSString*)ABRecordCopyCompositeName(person);
    contact[@"name"] = name ? name : @"";

	ABMultiValueRef emails = ABRecordCopyValue(person, kABPersonEmailProperty);
    NSArray* emailsArray = (__bridge_transfer NSArray*)ABMultiValueCopyArrayOfAllValues(emails);
    // ABMultiValueCopyArrayOfAllValues returns nil, not @[], if there are no values!
    contact[@"emails"] = emailsArray ? emailsArray :@[];
	if (emails) CFRelease(emails);
	
    ABMultiValueRef phones = ABRecordCopyValue(person, kABPersonPhoneProperty);
    NSArray* phonesArray = (__bridge_transfer NSArray*)ABMultiValueCopyArrayOfAllValues(phones);
    // ABMultiValueCopyArrayOfAllValues returns nil, not @[], if there are no values!
    contact[@"phones"] = phonesArray ? phonesArray : @[];
	if (phones) CFRelease(phones);
	
	ABMultiValueRef addresses = ABRecordCopyValue(person, kABPersonAddressProperty);
	CFIndex count = ABMultiValueGetCount(addresses);
    NSMutableArray* addrs = [NSMutableArray arrayWithCapacity:count];
    for (CFIndex i = 0; i != count; i++) {
        CFDictionaryRef address = ABMultiValueCopyValueAtIndex(addresses, i);
        [addrs addObject:@{@"street":  addressField(address, kABPersonAddressStreetKey),
                           @"city":    addressField(address, kABPersonAddressCityKey),
                           @"state":   addressField(address, kABPersonAddressStateKey),
                           @"zip":     addressField(address, kABPersonAddressZIPKey),
                           @"country": addressField(address, kABPersonAddressCountryKey)
                           }];
        CFRelease(address);
    }
    contact[@"addresses"] = addrs;
    if (addresses) CFRelease(addresses);

    return contact;
}


//! Send the JSON-encoded address book to the Javascript code.
//!
- (void) deliverContacts
{
    // Create an array of contact dictionaries, one for each person in the address book.
    
    CFArrayRef people = ABAddressBookCopyArrayOfAllPeople(self.addressBook);
    NSMutableArray* contacts = [NSMutableArray array];
    CFIndex count = CFArrayGetCount(people);
    for (CFIndex i = 0; i != count; ++i) {
        [contacts addObject:[self contactForPerson:CFArrayGetValueAtIndex(people, i)]];
    }
    CFRelease(people);
    
    // Convert the array of contact dictionaries into a JSON string.
    
    NSError* err;
    NSData* jsonData = [NSJSONSerialization dataWithJSONObject:contacts options:0 error:&err];
    NSString* jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    
    // Send it back to the Javascript.
    
    [self fireEvent:@"contacts.internal.get"
            success:YES
         components:@{ @"contacts": jsonString }];
}


#pragma mark Helper methods for editContact

//! Patch the buttons in the person controller's navigation bar so they will report back to
//! the Javascript.
//!
//! This function will execute every 0.1 seconds until it finds a left button (the Cancel
//! button) on the person controller navigation bar; then it will patch the target and
//! action of the Cancel and Done buttons so they will call the contactEditorCancel and
//! contactDone methods, respectively.
//!
//! @param timer The NSTimer that is executing this method.
//!
- (void) patchNavigationBarActions:(NSTimer*)timer
{
    UINavigationItem* navBar = timer.userInfo;
    UIBarButtonItem* cancelButton = navBar.leftBarButtonItem;
    UIBarButtonItem* doneButton = navBar.rightBarButtonItem;
    if (cancelButton != nil) {
        [timer invalidate];     // The timer has served its purpose.
        cancelButton.target = self;
        cancelButton.action = @selector(contactEditorCancel:);
        self.editingDoneTarget = doneButton.target;
        self.editingDoneAction = doneButton.action;
        doneButton.target = self;
        doneButton.action = @selector(contactEditorDone:);
    }
}


//! Action method for the Cancel button in the contact editor interface.
- (void) contactEditorCancel:(id)sender
{
    [self fireEvent:@"contacts.edit"
            success:NO
         components:@{ @"contactid": self.idForEditedPerson }];
    [self.viewController dismissViewControllerAnimated:YES completion:nil];
    self.idForEditedPerson = nil;
    self.busy = NO;
}


//! Action method for the Done button in the contact editor interface.
- (void) contactEditorDone:(id)sender
{
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Warc-performSelector-leaks"
    [self.editingDoneTarget performSelector:self.editingDoneAction withObject:sender];
#pragma clang diagnostic pop
    [self.viewController dismissViewControllerAnimated:YES completion:nil];
    [self deliverContacts];
    [self fireEvent:@"contacts.edit"
            success:YES
         components:@{ @"contactid": self.idForEditedPerson }];
    self.idForEditedPerson = nil;
    self.busy = NO;
}


#pragma mark - ABNewPersonViewControllerDelegate (completion for addContact)

- (void) newPersonViewController:(ABNewPersonViewController *)newPersonView
        didCompleteWithNewPerson:(ABRecordRef)person
{
    [self.viewController dismissViewControllerAnimated:YES completion:nil];
    [self addedPerson:person];
}

- (void) addedPerson:(ABRecordRef)person
{
    if (person) {
        [self deliverContacts];
        [self fireEvent:@"contacts.add"
                success:YES
             components:@{ @"contactid": idForPerson(person)}];
    }
    else {
        [self fireEvent:@"contacts.add" success:NO components:nil];
    }
	self.busy = NO;
}


#pragma mark ABPeoplePickerNavigationControllerDelegate (completion for chooseContact)

// In iOS7 and before, tells the controller to return control to the app rather than displaying
// the selected contact. Deprecated in iOS8. (See below.)
- (BOOL) peoplePickerNavigationController:(ABPeoplePickerNavigationController *)peoplePicker
       shouldContinueAfterSelectingPerson:(ABRecordRef)person
{
    [self.viewController dismissViewControllerAnimated:YES completion:nil];
    [self chosePerson:person];
    return NO;
}


// In iOS8, because this method is implemented and peoplePickerNavigationController:
// didSelectPerson:property:identifier: is not implemented, the controller returns control to
// the app rather than displaying the contact.
- (void)peoplePickerNavigationController:(ABPeoplePickerNavigationController *)peoplePicker 
                         didSelectPerson:(ABRecordRef)person
{
    [self chosePerson:person];
}


- (void) peoplePickerNavigationControllerDidCancel:(ABPeoplePickerNavigationController *)peoplePicker
{
    [self.viewController dismissViewControllerAnimated:YES completion:nil];
    [self chosePerson:nil];
}


- (void) chosePerson:(ABRecordRef)person
{
    if (person) {
        [self deliverContacts];
        [self fireEvent:@"contacts.choose"
                success:YES
             components:@{ @"contactid": idForPerson(person) }];
    }
    else {
        [self fireEvent:@"contacts.choose" success:NO components:@{ @"cancelled": @"true" }];
    }
    self.busy = NO;
}


#pragma mark ABPersonViewControllerDelegate (used in editContact)

- (BOOL) personViewController:(ABPersonViewController *)personViewController
shouldPerformDefaultActionForPerson:(ABRecordRef)person
                     property:(ABPropertyID)property
                   identifier:(ABMultiValueIdentifier)identifier
{
    return NO;
}


#pragma mark - CDVPlugin

- (void)pluginInitialize
{
    [super pluginInitialize];
    
    // Get a reference to the system address book if possible.
    
    switch (ABAddressBookGetAuthorizationStatus()) {
        case kABAuthorizationStatusDenied:
        case kABAuthorizationStatusRestricted:
            // Not authorized. addressBook will be nil.
            break;
        case kABAuthorizationStatusAuthorized:
        case kABAuthorizationStatusNotDetermined: {
            CFErrorRef err;
            ABAddressBookRef ab = ABAddressBookCreateWithOptions(nil, &err);
            if (ab) {
                // We got an address book ... but can we use it?
                dispatch_semaphore_t sema = dispatch_semaphore_create(0);
                ABAddressBookRequestAccessWithCompletion(ab, ^(bool granted, CFErrorRef error) {
                    if (granted) {
                        self.addressBook = ab;
                    }
                    else {
                        CFRelease(ab);
                    }
                    dispatch_semaphore_signal(sema);
                });
                // Wait for the access request to complete.
                dispatch_semaphore_wait(sema, DISPATCH_TIME_FOREVER);
            }
            
        }
        default:
            // There are no other options.
            break;
    }
}


- (void)dealloc
{
    if (self.addressBook) CFRelease(self.addressBook);
}

@end
