using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using System.Collections.Generic;

namespace Visitor
{
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
            bool result = subject.Validate(phoneNumber);
            
            //Assert
            Assert.IsTrue(result);
        }


        [Test]
        public void Visit_WithValidateBelgiumPhoneNumber_SetIsValidFalse()
        {
            //Arrenge
            var phoneNumber = new PhoneNumber("32", "654");
            var subject = new ValidateBelgiumPhoneNumber();

            //Act
            bool result = subject.Validate(phoneNumber);

            //Assert
            Assert.IsFalse(result);
        }

        

        [Test]
        public void Visit_WithBadFormatedEmail_ContactListIsInValid()
        {
            //Arrenge
            var emailAddress = new EmailAddress("badAemail.com");
            var subject = new EmailAddressValidator();

            //Act
            bool result = subject.Validate(emailAddress);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Validate_WithValidArgs_ContactListIsValid()
        {
            //Arrenge
            var contactList = new ContactList();
            contactList.Add(new EmailAddress("good@email.com"));
            contactList.Add(new PhoneNumber("32","022989878"));

            //Act
            contactList.Attach(new EmailAddressValidator());
            contactList.Attach(new ValidateBelgiumPhoneNumber());

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
            contactList.Attach(new EmailAddressValidator());
            contactList.Attach(new ValidateBelgiumPhoneNumber());

            //Assert
            Assert.IsFalse(contactList.IsValid);
        }


    }
    # endregion

    #region Extensions

    public class ValidateBelgiumPhoneNumber : IValidator
    {
        public bool Validate(Contact contact)
        {
            var phoneNumber = contact as PhoneNumber;
            if (phoneNumber == null || phoneNumber.CountryCode != "32") return true;
            var strRegex = @"^0[1-9]\d{7,8}$";
            var regex = new Regex(strRegex);
            return regex.IsMatch(phoneNumber.Number);
        }
    }

    public class ValidateFrenchPhoneNumber : IValidator
    {
        public bool Validate(Contact contact)
        {
            var phoneNumber = contact as PhoneNumber;
            if (phoneNumber == null || phoneNumber.CountryCode != "33") return true;
            var strRegex = @"^0[1-6]{1}(([0-9]{2}){4})|((\s[0-9]{2}){4})|((-[0-9]{2}){4})$";
            var regex = new Regex(strRegex);
            return regex.IsMatch(phoneNumber.Number);
        }
    }

    public class OnlyAcceptBelgiumAndFrenchCountryCode : IValidator
    {
        public bool Validate(Contact contact)
        {
            var phoneNumber = contact as PhoneNumber;
            if (phoneNumber == null) return true;

            return (
                phoneNumber.CountryCode == "32" ||
                phoneNumber.CountryCode == "33"
                );
        }
    }

    public class EmailAddressValidator : IValidator
    {
        public bool Validate(Contact contact)
        {
            var emailAddress = contact as EmailAddress;
            if (emailAddress == null) return true;

            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                     @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + 
                     @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            
            Regex re = new Regex(strRegex);
            return re.IsMatch(emailAddress.ToString());
        }
    }

    
    #endregion

    #region DomainModel
    public class ContactList : List<Contact>
    {
        bool _isValid = true;
        
        public bool IsValid
        {
            get
            {
                return _isValid;
            }

        }

        public void Attach(IValidator visitor)
        {
            foreach (var item in this)
            {
               _isValid = visitor.Validate(item) && _isValid;
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

    public interface IValidator
    {
        bool Validate(Contact contact);
    }

    #endregion

}

