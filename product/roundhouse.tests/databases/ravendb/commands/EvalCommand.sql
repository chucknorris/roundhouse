EVAL http://localhost:8080/bulk_docs/Condor/DocumentsByEntityName?query=Tag%253ACareProviders
-H "Content-Type: application/json; charset=utf-8"
-d "{
	  'Script': 'if(this.RizivIdentification.RizivNumber.length == 6){var checkDigit = new String(97 - (this.RizivIdentification.RizivNumber  % 97));if(checkDigit .length < 2) { checkDigit = \"0\" +  checkDigit; }this.RizivIdentification.RizivNumber = this.RizivIdentification.RizivNumber + checkDigit;}',
	  'Values': {}
	}"