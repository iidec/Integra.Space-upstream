//-----------------------------------------------------------------------
// <copyright file="TestObject1.cs" company="Integra.Space.UnitTests">
//     Copyright (c) Integra.Space. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.UnitTests
{
    /// <summary>
    /// Simulates a Visa message.
    /// </summary>
    public class TestObject1 : EventBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestObject1"/> class.
        /// </summary>
        /// <param name="messageType">Message type.</param>
        /// <param name="primaryAccountNumber">Primary account number.</param>
        /// <param name="processingCode">Processing code.</param>
        /// <param name="transactionAmount">Transaction amount.</param>
        /// <param name="dateTimeTransmission">Date time transmission.</param>
        /// <param name="systemTraceAuditNumber">System trace audit number.</param>
        /// <param name="localTransactionTime">Local transaction time.</param>
        /// <param name="localTransactionDate">Local transaction date.</param>
        /// <param name="setElementDate">Set element date.</param>
        /// <param name="merchantType">Merchant type.</param>
        /// <param name="acquiringInstitutionCountryCode">Acquiring institution country code.</param>
        /// <param name="pointOfServiceEntryMode">Point of service entry mode.</param>
        /// <param name="pointOfServiceConditionCode">Point of service condition code.</param>
        /// <param name="acquiringInstitutionIdentificationCode">Acquiring institution identification code.</param>
        /// <param name="track2Data">Track 2 data.</param>
        /// <param name="retrievalReferenceNumber">Retrieval reference number.</param>
        /// <param name="cardAcceptorTerminalIdentification">Card acceptor terminal identification.</param>
        /// <param name="cardAcceptorIdentificationCode">Card acceptor identification code.</param>
        /// <param name="cardAcceptorNameLocation">Card acceptor name location.</param>
        /// <param name="transactionCurrencyCode">Transaction currency code.</param>
        /// <param name="accountIdentification1">Account identification 1</param>
        /// <param name="campo104">Additional integer field for test.</param>
        /// <param name="campo105">Additional unsigned integer field for test.</param>
        public TestObject1(string messageType = "0100", string primaryAccountNumber = "9999941616073663", string processingCode = "302000", decimal transactionAmount = 1m, string dateTimeTransmission = "0508152549", string systemTraceAuditNumber = "212868", string localTransactionTime = "152549", string localTransactionDate = "0508", string setElementDate = "0508", string merchantType = "6011", string acquiringInstitutionCountryCode = "320", string pointOfServiceEntryMode = "051", string pointOfServiceConditionCode = "02", string acquiringInstitutionIdentificationCode = "491381", string track2Data = "9999941616073663D18022011583036900000", string retrievalReferenceNumber = "412815212868", string cardAcceptorTerminalIdentification = "2906", string cardAcceptorIdentificationCode = "Shell El Rodeo ", string cardAcceptorNameLocation = "Shell El Rodeo1GUATEMALA    GT", string transactionCurrencyCode = "320", string accountIdentification1 = "00001613000000000001", int campo104 = -1, uint campo105 = 1)
        {
            this.MessageType = messageType;
            this.PrimaryAccountNumber = primaryAccountNumber;
            this.ProcessingCode = processingCode;
            this.TransactionAmount = transactionAmount;
            this.DateTimeTransmission = dateTimeTransmission;
            this.SystemTraceAuditNumber = systemTraceAuditNumber;
            this.LocalTransactionTime = localTransactionTime;
            this.LocalTransactionDate = localTransactionDate;
            this.SetElementDate = setElementDate;
            this.MerchantType = merchantType;
            this.AcquiringInstitutionCountryCode = acquiringInstitutionCountryCode;
            this.PointOfServiceConditionCode = pointOfServiceEntryMode;
            this.PointOfServiceConditionCode = pointOfServiceConditionCode;
            this.AcquiringInstitutionIdentificationCode = acquiringInstitutionIdentificationCode;
            this.Track2Data = track2Data;
            this.RetrievalReferenceNumber = retrievalReferenceNumber;
            this.CardAcceptorTerminalIdentification = cardAcceptorTerminalIdentification;
            this.CardAcceptorIdentificationCode = cardAcceptorIdentificationCode;
            this.CardAcceptorNameLocation = cardAcceptorNameLocation;
            this.TransactionCurrencyCode = transactionCurrencyCode;
            this.AccountIdentification1 = accountIdentification1;
            this.Campo104 = campo104;
            this.Campo105 = campo105;
        }

        /// <summary>
        /// Gets the message type.
        /// </summary>
        public string MessageType { get; private set; }

        /// <summary>
        /// Gets the primary account number.
        /// </summary>
        public string PrimaryAccountNumber { get; private set; }

        /// <summary>
        /// Gets the processing code.
        /// </summary>
        public string ProcessingCode { get; private set; }

        /// <summary>
        /// Gets the transaction amount.
        /// </summary>
        public decimal TransactionAmount { get; private set; }

        /// <summary>
        /// Gets the date time transmission.
        /// </summary>
        public string DateTimeTransmission { get; private set; }

        /// <summary>
        /// Gets the system trace audit number.
        /// </summary>
        public string SystemTraceAuditNumber { get; private set; }

        /// <summary>
        /// Gets the local transaction time.
        /// </summary>
        public string LocalTransactionTime { get; private set; }

        /// <summary>
        /// Gets the local transaction date.
        /// </summary>
        public string LocalTransactionDate { get; private set; }

        /// <summary>
        /// Gets the set element date field.
        /// </summary>
        public string SetElementDate { get; private set; }

        /// <summary>
        /// Gets the merchant type.
        /// </summary>
        public string MerchantType { get; private set; }

        /// <summary>
        /// Gets the acquiring institution country code.
        /// </summary>
        public string AcquiringInstitutionCountryCode { get; private set; }

        /// <summary>
        /// Gets the point of service entry mode.
        /// </summary>
        public string PointOfServiceEntryMode { get; private set; }

        /// <summary>
        /// Gets point of service condition code.
        /// </summary>
        public string PointOfServiceConditionCode { get; private set; }

        /// <summary>
        /// Gets acquiring institution identification code.
        /// </summary>
        public string AcquiringInstitutionIdentificationCode { get; private set; }

        /// <summary>
        /// Gets the track 2 data.
        /// </summary>
        public string Track2Data { get; private set; }

        /// <summary>
        /// Gets the retrieval reference number.
        /// </summary>
        public string RetrievalReferenceNumber { get; private set; }

        /// <summary>
        /// Gets the card acceptor terminal identification.
        /// </summary>
        public string CardAcceptorTerminalIdentification { get; private set; }

        /// <summary>
        /// Gets the card acceptor identification code.
        /// </summary>
        public string CardAcceptorIdentificationCode { get; private set; }

        /// <summary>
        /// Gets the card acceptor name location.
        /// </summary>
        public string CardAcceptorNameLocation { get; private set; }

        /// <summary>
        /// Gets the transaction currency code.
        /// </summary>
        public string TransactionCurrencyCode { get; private set; }

        /// <summary>
        /// Gets the account identification 1.
        /// </summary>
        public string AccountIdentification1 { get; private set; }

        /// <summary>
        /// Gets the additional integer field for test.
        /// </summary>
        public int Campo104 { get; private set; }

        /// <summary>
        /// Gets the additional unsigned integer field for test.
        /// </summary>
        public uint Campo105 { get; private set; }
    }
}
