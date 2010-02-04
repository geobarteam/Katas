using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using System.Collections.Generic;

namespace Visitor
{
    //Change on the master branch
    #region Test
    [TestFixture]
    public class VisitorTest
    {

        [Test]
        public void Visit_WithValidateBelgiumPhoneNumber_SetIsValidTrue()
        {
            //Arrenge
            var  phoneNumber = new PhoneNumber("32", "0495204340");
            var subject = new ValidateBelgiumPhoneNumber();
            
            //Act
            var result = subject.BrokenRuleMessage(phoneNumber);
            
            //Assert
            Assert.AreEqual(string.Empty, result);
        }


        [Test]
        public void Visit_WithValidateBelgiumPhoneNumber_SetIsValidFalse()
        {
            //Arrenge
            var phoneNumber = new PhoneNumber("32", "654");
            var subject = new ValidateBelgiumPhoneNumber();

            //Act
            var result = subject.BrokenRuleMessage(phoneNumber);

            //Assert
            Assert.AreEqual("+32.654 is not a valid Belgium phone number", result);
        }

        

        [Test]
        public void Visit_WithBadFormatedEmail_ContactListIsInValid()
        {
            //Arrenge
            var emailAddress = new EmailAddress("badAemail.com");
            var subject = new EmailAddressValidator();

            //Act
            var result = subject.BrokenRuleMessage(emailAddress);

            //Assert
            Assert.AreEqual("badAemail.com is not a valid e-mail", result);
        }

        [Test]
        public void Validate_WithValidArgs_ContactListIsValid()
        {
            //Arrenge
            var contactList = new ContactList();
            contactList.Add(new EmailAddress("good@email.com"));
            contactList.Add(new PhoneNumber("32","022989878"));

            //Act
            contactList.AddValidationRule(new EmailAddressValidator());
            contactList.AddValidationRule(new ValidateBelgiumPhoneNumber());
            contactList.Validate();

            //Assert
            Assert.IsTrue(contactList.IsValid);
        }

        [Test]
        public void Validate_WithInValidArgs_ContactListIsNotValid()
        {
            //Arrenge
            var contactList = new ContactList();
            contactList.Add(new EmailAddress("good@email.com"));
            contactList.Add(new PhoneNumber("32", "02552989878"));
            contactList.Add(new PhoneNumber("32", "0255245878"));

            //Act
            contactList.AddValidationRule(new EmailAddressValidator());
            contactList.AddValidationRule(new ValidateBelgiumPhoneNumber());
            contactList.Validate();
            //Assert
            Assert.IsFalse(contactList.IsValid);
        }

        [Test]
        public void Validate_WithInValidArgs_ContactListHasBrokenRuleMessage()
        {
            //Arrenge
            var contactList = new ContactList();
            contactList.Add(new EmailAddress("good@email.com"));
            contactList.Add(new PhoneNumber("32", "02552989878"));
            contactList.Add(new PhoneNumber("32", "0255245878"));

            //Act
            contactList.AddValidationRule(new EmailAddressValidator());
            contactList.AddValidationRule(new ValidateBelgiumPhoneNumber());
            contactList.Validate();
            //Assert
            Assert.IsFalse(contactList.ErrorMessages.Count() == 0);
        }


    }
    # endregion

    #region Extensions

    public class ValidateBelgiumPhoneNumber : IValidationRule
    {
        public string BrokenRuleMessage(Contact contact)
        {
            var phoneNumber = contact as PhoneNumber;
            if (phoneNumber == null || phoneNumber.CountryCode != "32") return string.Empty;
            var strRegex = @"^0[1-9]\d{7,8}$";
            var regex = new Regex(strRegex);
            if (!regex.IsMatch(phoneNumber.Number))
                return phoneNumber.ToString() + " is not a valid Belgium phone number";

            return string.Empty;
        }
    }

    public class ValidateFrenchPhoneNumber : IValidationRule
    {
        public string BrokenRuleMessage(Contact contact)
        {
            var phoneNumber = contact as PhoneNumber;
            if (phoneNumber == null || phoneNumber.CountryCode != "33") return string.Empty;
            var strRegex = @"^0[1-6]{1}(([0-9]{2}){4})|((\s[0-9]{2}){4})|((-[0-9]{2}){4})$";
            var regex = new Regex(strRegex);
            if (!regex.IsMatch(phoneNumber.Number))
                return phoneNumber.ToString() + " is not a valid French phone number";

            return string.Empty;
        }
    }

    public class OnlyAcceptBelgiumAndFrenchCountryCode : IValidationRule
    {
        public string BrokenRuleMessage(Contact contact)
        {
            var phoneNumber = contact as PhoneNumber;
            if (phoneNumber == null) return string.Empty;

            if (
                phoneNumber.CountryCode != "32" &&
                phoneNumber.CountryCode != "33"
                )
            {
                return phoneNumber.ToString() + " is not a valid Belgium or French phone number";
            }

            return string.Empty;
        }
    }

    public class EmailAddressValidator : IValidationRule
    {
        public string BrokenRuleMessage(Contact contact)
        {
            var emailAddress = contact as EmailAddress;
            if (emailAddress == null) return string.Empty;

            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                     @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + 
                     @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            
            Regex re = new Regex(strRegex);
            if(!re.IsMatch(emailAddress.ToString()))
                return emailAddress + " is not a valid emailAddress";

            return string.Empty;
        }
    }

    
    #endregion

    #region DomainModel
    public class ContactList : List<Contact>
    {
        IList<IValidationRule> _validationRules;
        IEnumerable<string> _errorMessages;

        public ContactList()
        {
            _validationRules = new List<IValidationRule>();
        }

        public bool IsValid
        {
            get
            {
                return ErrorMessages.Count()==0;
            }
        }

        public IEnumerable<string> ErrorMessages
        {
            get
            {
                return _errorMessages;
            }
        }
        
        public void AddValidationRule(IValidationRule visitor)
        {
            _validationRules.Add(visitor);
        }

        public void Validate()
        {
            _errorMessages = getBrokenRules();
        }

        private IEnumerable<string> getBrokenRules()
        {
            foreach (var item in this)
            {
                foreach (var rule in _validationRules)
                {
                    yield return rule.BrokenRuleMessage(item);
                }
            }
        }
    }

    public class EmailAddress : Contact
    {
        public string _email;
        public EmailAddress(string email)
        {
            _email = email;
        }

        public override string ToString()
        {
            return _email;
        }
    }

    public class PhoneNumber:Contact 
    {

        public string CountryCode { get; set;}

        public string Number { get; set; }

        public PhoneNumber(string countryCode, string number)
        {
            CountryCode = countryCode;
            Number = number;
        }

        public override string ToString()
        {
            return CountryCode + "/" + Number; 
        }
    }

    public abstract class Contact
    {
        private bool _isValid = true;  //this is the default
        public bool IsValid
        {
            get
            { return _isValid; }
            set
            { _isValid = value; }
        }
    }

    public interface IValidationRule
    {
        string BrokenRuleMessage(Contact contact);
    }
    #endregion

}

