using System;
using NUnit.Framework;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;
using net.authorize.sample;
using System.IO;
using System.Reflection;
using AuthorizeNet;
using net.authorize.sample.PaymentTransactions;
using System.Threading;
using net.authorize.sample.CustomerProfiles;
using net.authorize.sample.MobileInappTransactions;

namespace SampleCodeTest
{
    [TestFixture]
    public class Outout
    {



        string apiLoginId = Constants.API_LOGIN_ID;
        string transactionKey = Constants.TRANSACTION_KEY;
        string TransactionID = Constants.TRANSACTION_ID;
        string payerID = Constants.PAYER_ID;

        static CryptoRandom r = new CryptoRandom();
        ANetApiResponse response = null;

        private static string GetEmail()
        {
            return r.Next(1000, 89999999) + "@test.com";
        }

        private static decimal GetAmount()
        {
            return r.Next(10, 200);
        }

        private static short GetMonth()
        {
            return (Int16)r.Next(7, 365);
        }

        customerAddressType billingAddress = new customerAddressType
        {
            firstName = "John",
            lastName = "Doe",
            address = "123 My St",
            city = "OurTown",
            zip = "98004"
        };

        static creditCardType creditCard = new creditCardType
        {
            cardNumber = "4111111111111111",
            expirationDate = "0718",
            cardCode = "123"
        };

        //standard api call to retrieve response
        paymentType paymentType = new paymentType { Item = creditCard };



        transactionRequestType transactionRequest = new transactionRequestType
        {
            transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),    // charge the card

            amount = GetAmount(),
            payment = new paymentType { Item = creditCard },
            billTo = new customerAddressType
            {
                firstName = "John",
                lastName = "Doe",
                address = "123 My St",
                city = "OurTown",
                zip = "98004"
            }
        };

        public string[] GenerateRandomString(int min, int max)
        {
            Random random = new Random();
            string[] array = new string[2];
            int i;

            string r = "";
            for (i = 0; i < max; i++)
            {
                r += random.Next(0, 9).ToString();
            }
            array[0] = r; //max value

            if (min > 0)
            {
                r = "";
                for (i = 0; i < min; i++)
                {
                    r += random.Next(1, 9).ToString();
                }
                array[1] = r; //minimum value
            }
            else { array[1] = null; }
            return array;
        }

        public string[] GenerateNegativeString(int min, int max)
        {
            Random random = new Random();
            string[] array = new string[2];
            int i;

            string r = "";
            for (i = 0; i <= max; i++)
            {
                r += random.Next(1, 9).ToString();
            }
            array[0] = r; //max value excedding

            if (min >= 1)
            {
                r = "";
                for (i = 1; i < min; i++)
                {
                    r += random.Next(1, 9).ToString();
                }
                array[1] = r; // less than minimum value
            }
            else { array[1] = null; }
          
            return array;
        }


        [Test]
        public void TestChargeCreditCardAmount()
        {
            foreach (string values in GenerateRandomString(1, 5))
            {
                transactionRequest.amount = Convert.ToDecimal(values);
                response = ChargeCreditCard.Run(apiLoginId, transactionKey, GetAmount(), transactionRequest);
                Assert.IsNotNull(response);
                Assert.AreEqual(response.messages.resultCode, messageTypeEnum.Ok);
            }
            foreach (string values in GenerateNegativeString(1, 5))
            {

                if (values != "")
                {
                    transactionRequest.amount = Convert.ToDecimal(values);
                }
                else {
                    transactionRequest.amount = 0;
                }
                
                response = ChargeCreditCard.Run(apiLoginId, transactionKey, GetAmount(), transactionRequest);
                Assert.IsNotNull(response);
                Assert.AreNotEqual(response.messages.resultCode, messageTypeEnum.Ok);
            }
        }


        [Test]
        public void TestChargeCreditCardTerminalNumber()
        {
            foreach (string values in GenerateRandomString(4, 5000))
            {
                transactionRequest.terminalNumber=values;

                response = ChargeCreditCard.Run(apiLoginId, transactionKey, GetAmount(), transactionRequest);
                Assert.IsNotNull(response);
                Assert.AreEqual(response.messages.resultCode, messageTypeEnum.Ok);
            }
            foreach (string values in GenerateNegativeString(1, 50))
            {

                transactionRequest.solution.id = values;

                response = ChargeCreditCard.Run(apiLoginId, transactionKey, GetAmount(), transactionRequest);
                Assert.IsNotNull(response);
                Assert.AreNotEqual(response.messages.resultCode, messageTypeEnum.Ok);
            }
        }

        [Test]
        public void TestChargeCreditCardCreditCardNumber()
        {
            string[] possibleValues = { "41111111111111111", "411111111111" };
            foreach (string values in possibleValues)
            {

                transactionRequest.payment = new paymentType
                {
                    Item = new creditCardType
                    {
                        cardNumber = values,
                        expirationDate = "0718",
                        cardCode = "123"
                    }
                };

                response = ChargeCreditCard.Run(apiLoginId, transactionKey, GetAmount(), transactionRequest);
                Assert.IsNotNull(response);
                Assert.AreNotEqual(response.messages.resultCode, messageTypeEnum.Ok);
            }
        }

        [Test]
        public void TestChargeCreditInvoiceNumber()
        {
            foreach (string values in GenerateRandomString(2, 20))
            {
                transactionRequest.order = new orderType { invoiceNumber = values };
                response = ChargeCreditCard.Run(apiLoginId, transactionKey, GetAmount(), transactionRequest);
                Assert.IsNotNull(response);
                Assert.AreEqual(response.messages.resultCode, messageTypeEnum.Ok);
            }
            foreach (string values in GenerateNegativeString(1, 20))
            {

                transactionRequest.order = new orderType { invoiceNumber = values };
                response = ChargeCreditCard.Run(apiLoginId, transactionKey, GetAmount(), transactionRequest);
                Assert.IsNotNull(response);
                Assert.AreNotEqual(response.messages.resultCode, messageTypeEnum.Ok);
            }
        }



    }
}