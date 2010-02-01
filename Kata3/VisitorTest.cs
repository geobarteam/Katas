using System;
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
            subject.Visit(phoneNumber);
            
            //Assert
            Assert.IsTrue(phoneNumber.IsValid);
        }


        [Test]
        public void Visit_WithValidateBelgiumPhoneNumber_SetIsValidFalse()
        {
            //Arrenge
            var phoneNumber = new PhoneNumber("32", "654");
            var subject = new ValidateBelgiumPhoneNumber();

            //Act
            subject.Visit(phoneNumber);

            //Assert
            Assert.IsFalse(phoneNumber.IsValid);
        }

        

        [Test]
        public void Visit_WithBadFormatedEmail_ContactListIsInValid()
        {
            //Arrenge
            var emailAddress = new EmailAddress("badAemail.com");
            var subject = new EmailAddressValidator();

            //Act
            subject.Visit(emailAddress);

            //Assert
            Assert.IsFalse(emailAddress.IsValid);
        }

        [Test]
        public void Validate_WithValidEmail_ContactListIsValid()
        {
            //Arrenge
            var contactList = new ContactList();
            contactList.Add(new EmailAddress("good@email.com"));

            //Act
            contactList.Attach(new EmailAddressValidator());

            //Assert
            Assert.IsTrue(contactList.IsValid);
        }



    }
    # endregion

    #region Extensions

    public class ValidateBelgiumPhoneNumber : IVisitor
    {
        public void Visit(Contact contact)
        {
            var phoneNumber = contact as PhoneNumber;
            if (phoneNumber == null || phoneNumber.CountryCode != "32") return;
            var strRegex = @"^0[1-9]\d{7,8}$";
            var regex = new Regex(strRegex);
            phoneNumber.IsValid = regex.IsMatch(phoneNumber.Number);
        }
    }

    public class ValidateFrenchPhoneNumber : IVisitor
    {
        public void Visit(Contact contact)
        {
            var phoneNumber = contact as PhoneNumber;
            if (phoneNumber == null || phoneNumber.CountryCode != "33") return;
            var strRegex = @"^0[1-6]{1}(([0-9]{2}){4})|((\s[0-9]{2}){4})|((-[0-9]{2}){4})$";
            var regex = new Regex(strRegex);
            phoneNumber.IsValid = regex.IsMatch(phoneNumber.Number);
        }
    }

    public class OnlyAcceptBelgiumAndFrenchCountryCode : IVisitor
    {
        public void Visit(Contact contact)
        {
            var phoneNumber = contact as PhoneNumber;
            if (phoneNumber == null) return;

            phoneNumber.IsValid = (
                phoneNumber.CountryCode == "32" ||
                phoneNumber.CountryCode == "33"
                );
        }
    }

    public class EmailAddressValidator : IVisitor
    {
        public void Visit(Contact contact)
        {
            var emailAddress = contact as EmailAddress;
            if (emailAddress == null) return;

            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                     @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" + 
                     @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            
            Regex re = new Regex(strRegex);
            emailAddress.IsValid = re.IsMatch(emailAddress.ToString());
        }
    }

    
    #endregion

    #region DomainModel
    public class ContactList : List<Contact>
    {
        public bool IsValid
        {
            get
            {
                foreach (var item in this)
                {
                    if (!item.IsValid)
                        return false;
                }
                return true;
            }
        }

        public void Attach(IVisitor visitor)
        {
            foreach (var item in this)
            {
                visitor.Visit(item);
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

    public interface IVisitor
    {
        void Visit(Contact contact);
    }

    #endregion

}

