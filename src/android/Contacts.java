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

package com.intel.xdk.contacts;

import org.apache.cordova.CallbackContext;
import org.apache.cordova.CordovaActivity;
import org.apache.cordova.CordovaInterface;
import org.apache.cordova.CordovaPlugin;
import org.apache.cordova.CordovaWebView;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.app.Activity;
import android.content.ContentResolver;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.provider.ContactsContract;

public class Contacts extends CordovaPlugin{

	static boolean busy = false;
	static String contactBeingEdited = "";
	private Activity activity;
	private CordovaWebView webView;
	
	final private static int CONTACT_ADDER_RESULT = 0;
	
	final private static int CONTACT_CHOOSER_RESULT = 1;
	
	final private static int CONTACT_EDIT_RESULT = 2;
	
	/**
	 * Constructor
	 */
	public Contacts(){
		
	}

	@Override
	public void initialize(CordovaInterface cordova, CordovaWebView webView){
		super.initialize(cordova, webView);
		
		this.activity = cordova.getActivity();
		this.webView = webView;
	}
	
	@Override
	public boolean execute(String action, JSONArray args, CallbackContext callbackContext) throws JSONException{
		if(action.equals("addContact")){
			this.addContact();
		}
		else if(action.equals("getContactInfo")){
            JSONObject r = new JSONObject();
            callbackContext.success(r);
		}
		else if(action.equals("chooseContact")){
			this.chooseContact();
		}
		else if(action.equals("editContact")){
			this.editContact(args.getString(0));
		}
		else if(action.equals("getContactData")){
			//this.getContactData(args.getString(0));
		}
		else if(action.equals("getContactList")){
			this.getContactList();
		}
		else if(action.equals("getContacts")){
			this.getContacts();
		}
		else if(action.equals("removeContact")){
			this.removeContact(args.getString(0));
		}
		else{
			return false;
		}
		return true;
	}
	
	public void addContact(){
		addContact("", "", "", "", "", "", "", "", "");
	}
	
	public void addContact(String first, String last, String street, String city, String state, String zip, String country, String phone, String email)
	{
		if( busy == true ) {
			String js = "javascript: var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.busy',true,true);e.success=false;e.message='busy';document.dispatchEvent(e);";
			injectJS(js);
			return;
		}
		
		try {
			busy = true;
			Intent intent = new Intent(Intent.ACTION_INSERT);
			intent.setType(ContactsContract.Contacts.CONTENT_TYPE);  
			intent.putExtra(ContactsContract.Intents.Insert.FULL_MODE, true);

			cordova.setActivityResultCallback(this);
			activity.startActivityForResult(intent, CONTACT_ADDER_RESULT);           
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} 
	}
	
	//CONTACT ADDER RESULT HANDLER
	@SuppressWarnings("deprecation")
	public void contactsAdderActivityResult(int requestCode, int resultCode, Intent intent) 
	{       
			
		 //Contact Added
	     if(resultCode == Activity.RESULT_OK)
	     {
		     Cursor cursor =  activity.managedQuery(intent.getData(), null, null, null, null);
		     cursor.moveToNext();
		     String contactId = cursor.getString(cursor.getColumnIndex(ContactsContract.Contacts.LOOKUP_KEY));
		    
		     
		     getAllContacts();
		     String js = String.format("var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.add',true,true);e.success=true;e.contactid='%s';document.dispatchEvent(e);", contactId);
			  
		     injectJS("javascript:"+js);
		  	 busy = false;
	  	 }
		 //Contact not Added
		 else
		 {
		   	 String js = "javascript: var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.add',true,true);e.success=false;e.cancelled=true;document.dispatchEvent(e);";
		     injectJS(js);
		   	 busy = false;
		 }
		  
	}
	
	public void chooseContact(){
		try {
			if( busy == true ) {
				String js = "javascript: var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.busy',true,true);e.success=false;e.message='busy';document.dispatchEvent(e);";
				injectJS(js);
				return;
			}
			busy = true;
			
			Intent intent = new Intent(Intent.ACTION_PICK);           
			intent.setType(ContactsContract.Contacts.CONTENT_TYPE);
			cordova.setActivityResultCallback(this);
			activity.startActivityForResult(intent, CONTACT_CHOOSER_RESULT);           
			//activity.setLaunchedChildActivity(true);
		} catch (Exception e) {
			e.printStackTrace();
		}      
	}
	
	//CONTACT CHOOSER RESULT HANDLER
	@SuppressWarnings("deprecation")
	public void contactsChooserActivityResult(int requestCode, int resultCode, Intent intent) 
	{
	          
	    if(resultCode == Activity.RESULT_OK){
	    	Cursor cursor =  activity.managedQuery(intent.getData(), null, null, null, null);
		    cursor.moveToNext();
		    String contactId = cursor.getString(cursor.getColumnIndex(ContactsContract.Contacts.LOOKUP_KEY));
			String name = cursor.getString(cursor.getColumnIndexOrThrow(ContactsContract.Contacts.DISPLAY_NAME)); 
				
			getAllContacts();
			String js =String.format(" var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.choose',true,true);e.success=true;e.contactid='%s';document.dispatchEvent(e);",contactId );
			injectJS("javascript:" + js);
			busy = false;
	    }
	    else{
	    	String js = "var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.choose',true,true);e.success=false;e.message='User canceled';document.dispatchEvent(e);";
			injectJS("javascript:" + js);
	    	busy = false;
	    }
	    
		    
	}
	
	@SuppressWarnings("deprecation")
	private String getAllContacts()
	{ 	
		
	    // Run query
	    Uri uri = ContactsContract.Contacts.CONTENT_URI;
	    String[] projection = new String[] {
	    		ContactsContract.Contacts._ID,
	            ContactsContract.Contacts.DISPLAY_NAME,
	            ContactsContract.Contacts.LOOKUP_KEY
	    };
	    String selection = ContactsContract.Contacts.IN_VISIBLE_GROUP + " = '" +
	            "1" + "'";
	    String[] selectionArgs = null;
	    String sortOrder = ContactsContract.Contacts.DISPLAY_NAME + " COLLATE LOCALIZED ASC";

	    Cursor cursor =  activity.managedQuery(uri, projection, selection, selectionArgs, sortOrder);
	    //Cursor cursor = activity.getContentResolver().query(uri, null, null, null, null);
	    
	    String jsContacts = "[";

	    if (cursor.getCount() > 0)
	    {
	    	while (cursor.moveToNext())
	        {
		    	//GRAB CURRENT LOOKUP_KEY;	
				String lk = cursor.getString(cursor.getColumnIndex(ContactsContract.Contacts.LOOKUP_KEY));
				String jsPerson = JSONValueForPerson(lk);
				jsContacts += jsPerson ;
	        }
	    }

	    jsContacts += "]";
        String js = " var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.internal.get',true,true);e.success=true;e.contacts="+jsContacts+";document.dispatchEvent(e);";
        injectJS("javascript:" + js);

	    return jsContacts;
	}
	
	public String JSONValueForPerson(String idlk)
    {	
		ContentResolver cr = activity.getContentResolver();
    	//PROCESS NAME ELEMENTS FOR CURRENT CONTACT ID
	    String firstName = "",lastName = "", compositeName = "", id="";
	    String nameWhere = ContactsContract.Data.LOOKUP_KEY + " = ? AND " + ContactsContract.Data.MIMETYPE + " = ?";
	    String[] params = new String[]{idlk, ContactsContract.CommonDataKinds.StructuredName.CONTENT_ITEM_TYPE};
	    Cursor nameCur = cr.query(ContactsContract.Data.CONTENT_URI, null, nameWhere ,params , null); 
	    if(nameCur.getCount() > 0)
	    {
	    	nameCur.moveToFirst();
	    	id = nameCur.getString(nameCur.getColumnIndex(ContactsContract.Data.CONTACT_ID));

		    firstName = nameCur.getString(nameCur.getColumnIndex(ContactsContract.CommonDataKinds.StructuredName.GIVEN_NAME));
		    lastName = nameCur.getString(nameCur.getColumnIndex(ContactsContract.CommonDataKinds.StructuredName.FAMILY_NAME));
		    compositeName = nameCur.getString(nameCur.getColumnIndex(ContactsContract.CommonDataKinds.StructuredName.DISPLAY_NAME));
					
			firstName = (firstName == null)?"":escapeStuff(firstName);
			lastName = (lastName == null)?"":escapeStuff(lastName);
			compositeName = (compositeName == null)?"":escapeStuff(compositeName);
	    }
		
		//PROCESS EMAIL ADDRESES FOR CURRENT CONTACT ID
		Cursor emailCur = cr.query(ContactsContract.CommonDataKinds.Email.CONTENT_URI, null, ContactsContract.CommonDataKinds.Email.CONTACT_ID + " = ?", new String[]{id}, null); 
		String emailAddresses = "[]";
		if(emailCur.getCount() > 0)
		{
			emailAddresses = "[";
			while (emailCur.moveToNext())
			{ 
				String email = emailCur.getString(emailCur.getColumnIndex(ContactsContract.CommonDataKinds.Email.DATA));
		 	   	email = escapeStuff(email);
			    //String emailType = emailCur.getString(emailCur.getColumnIndex(ContactsContract.CommonDataKinds.Email.TYPE)); 
		 	   
		 	   emailAddresses += "'"+email+"', ";

			} 
			emailAddresses += "]";
		}
		 	emailCur.close();

		 
		//PROCESS PHONE NUMBERS FOR CURRENT CONTACT ID
		Cursor phoneCur = cr.query(ContactsContract.CommonDataKinds.Phone.CONTENT_URI, null, ContactsContract.CommonDataKinds.Phone.CONTACT_ID + " = ?", new String[]{id}, null); 
		
		String phoneNumbers = "[]";
		if(phoneCur.getCount() > 0)
		{
			phoneNumbers = "[";
			while (phoneCur.moveToNext())
			{ 
				String phoneNum = phoneCur.getString(phoneCur.getColumnIndex(ContactsContract.CommonDataKinds.Phone.NUMBER));
				phoneNum = escapeStuff(phoneNum);
		 	   phoneNumbers += "'"+phoneNum+"', ";
			} 
			phoneNumbers += "]";
		}
			phoneCur.close();

		
		//PROCESS STREET ADDRESSES FOR CURRENT CONTACT ID
		String addrWhere = ContactsContract.Data.CONTACT_ID + " = ? AND " + ContactsContract.Data.MIMETYPE + " = ?"; 
		String[] addrWhereParams = new String[]{id, ContactsContract.CommonDataKinds.StructuredPostal.CONTENT_ITEM_TYPE};
		Cursor addressCur = cr.query(ContactsContract.CommonDataKinds.StructuredPostal.CONTENT_URI, null, addrWhere, addrWhereParams, null); 
		
		String streetAddresses = "[]";
		if(addressCur.getCount() > 0)
		{
			streetAddresses = "[";
			while (addressCur.moveToNext())
			{ 
				
				String street = addressCur.getString(addressCur.getColumnIndex(ContactsContract.CommonDataKinds.StructuredPostal.STREET));
				String city = addressCur.getString(addressCur.getColumnIndex(ContactsContract.CommonDataKinds.StructuredPostal.CITY));
				String state = addressCur.getString(addressCur.getColumnIndex(ContactsContract.CommonDataKinds.StructuredPostal.REGION));
				String zip = addressCur.getString(addressCur.getColumnIndex(ContactsContract.CommonDataKinds.StructuredPostal.POSTCODE));
				String country = addressCur.getString(addressCur.getColumnIndex(ContactsContract.CommonDataKinds.StructuredPostal.COUNTRY));
			
				
				street = escapeStuff(street);
				city = escapeStuff(city);
				state = escapeStuff(state);
				zip = escapeStuff(zip);
				country = escapeStuff(country);
				
				String addressstr = String.format("{ street:'%s', city:'%s', state:'%s', zip:'%s', country:'%s' }, ", 
										street, city, state, zip, country);
				streetAddresses += addressstr;
			
			} 
			streetAddresses += "]";
		}
		addressCur.close();
		
		 	
		 	
		String jsPerson =  String.format("{ id:'%s', name:'%s', first:'%s', last:'%s', phones:%s, emails:%s, addresses:%s }, ",
								idlk, compositeName, firstName, lastName, phoneNumbers, emailAddresses, streetAddresses);
		return  jsPerson.replaceAll("\\r\\n|\\r|\\n", "\\\\n");
		
    }
	
	public String escapeStuff(String myString)
	{
		if(myString == null)
			return "";
		myString = myString.replaceAll("\\\\", "\\\\\\\\");
		myString = myString.replaceAll("'", "\\\\'");
		return myString;
	}
	
	public void editContact(String contactId)
	{
		if( busy == true ) {
			String js = "javascript: var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.busy',true,true);e.success=false;e.message='busy';document.dispatchEvent(e);";
			injectJS(js);
			return;
		}

		try {
			//Determine if Contact ID exists
			Uri res = null; 
			Uri lookupUri = Uri.withAppendedPath(ContactsContract.Contacts.CONTENT_LOOKUP_URI, contactId);
			
			try{res = ContactsContract.Contacts.lookupContact(activity.getContentResolver(), lookupUri);}catch(Exception e){e.printStackTrace();}
			if(res != null)
			{
				busy = true;
				Intent intent = new Intent(Intent.ACTION_EDIT);
				contactBeingEdited = contactId;
				intent.setType(ContactsContract.Contacts.CONTENT_TYPE);  
				intent.setData(res);
				//launch activity
				cordova.setActivityResultCallback(this);
			    activity.startActivityForResult(intent, CONTACT_EDIT_RESULT);           
			    //activity.setLaunchedChildActivity(true); 
			}
			else
			{
				contactBeingEdited = "";
				String errjs1 = String.format("var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.edit',true,true);e.success=false;e.error='contact not found';e.contactid='%s';document.dispatchEvent(e);", contactId);
				injectJS("javascript: "+errjs1);
			}
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	//CONTACT EDIT RESULT HANDLER
	public void contactsEditActivityResult(int requestCode, int resultCode, Intent intent) 
	{       
		 //Contact EDITED
	     if(resultCode == Activity.RESULT_OK)
	     {  
	        getAllContacts();
		    String js = String.format("var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.edit',true,true);e.success=true;e.contactid='%s';document.dispatchEvent(e);", contactBeingEdited);
			injectJS("javascript:"+js);
		  	busy = false;
	     }
	     //Contact not Added
	     else
	     {
	    	 String js = String.format("javascript: var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.edit',true,true);e.success=false;e.cancelled=true;e.contactid='%s';document.dispatchEvent(e);", contactBeingEdited);
	    	 injectJS(js);
	    	 busy = false;
	     }
	     contactBeingEdited = "";
	}
	
	public void getContactList(){
		;
	}
	
	public void getContacts()
	{
	    try {
			getAllContacts();
			String js ="javascript: var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.get',true,true);e.success=true;document.dispatchEvent(e);";
			injectJS(js);
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	
	public void removeContact(String contactId)
	{
		if( busy == true ) {
			String js = "javascript: var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.busy',true,true);e.success=false;e.message='busy';document.dispatchEvent(e);";
			injectJS(js);
			return;
		}
		
	        try{
	            Uri uri = Uri.withAppendedPath(ContactsContract.Contacts.CONTENT_LOOKUP_URI, contactId);
	            int rowsDeleted = activity.getContentResolver().delete(uri, null, null);
	          
	            if(rowsDeleted > 0)
	            {
	                getAllContacts();
		        	String js = String.format("var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.remove',true,true);e.success=true;e.contactid='%s';document.dispatchEvent(e);", contactId);
		        	injectJS("javascript: "+js);
	            }
	            else
	            {
	        		String js = String.format("var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.remove',true,true);e.success=false;e.error='error deleting contact';e.contactid='%s';document.dispatchEvent(e);", contactId);
	        		injectJS("javascript: "+js);
	            }
	            busy = false;
	        }
	        catch(Exception e)
	        {
	            System.out.println(e.getStackTrace());
		    	String errjs = String.format("var e = document.createEvent('Events');e.initEvent('intel.xdk.contacts.remove',true,true);e.success=false;e.error='contact not found';e.contactid='%s';document.dispatchEvent(e);", contactId);
		    	injectJS("javascript: "+errjs);
				busy = false;
				return;
	        }
	}
	
	protected void injectJS(final String js) {
		activity.runOnUiThread(new Runnable() {

			public void run() {
				webView.loadUrl(js);
			}

		});
	}
	
	@Override
	public void onActivityResult(int requestCode, int resultCode, Intent intent){
		switch(requestCode){
			case CONTACT_ADDER_RESULT : 
				this.contactsAdderActivityResult(requestCode, resultCode, intent);
				break;
			case CONTACT_CHOOSER_RESULT : 
				this.contactsChooserActivityResult(requestCode, resultCode, intent);
				break;
			case CONTACT_EDIT_RESULT : 
				this.contactsEditActivityResult(requestCode, resultCode, intent);
				break;
			default:
				break;
		}
	}
	
}
