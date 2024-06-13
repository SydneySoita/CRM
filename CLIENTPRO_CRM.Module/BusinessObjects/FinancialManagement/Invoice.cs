﻿using CLIENTPRO_CRM.Module.BusinessObjects.AccountingManagement;
using CLIENTPRO_CRM.Module.BusinessObjects.ActivityStreamManagement;
using CLIENTPRO_CRM.Module.BusinessObjects.Basics;
using CLIENTPRO_CRM.Module.BusinessObjects.OrderManagement;
using CLIENTPRO_CRM.Module.BusinessObjects.PipelineManagement;
using CLIENTPRO_CRM.Module.BusinessObjects.Settings;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System.ComponentModel;

namespace CLIENTPRO_CRM.Module.BusinessObjects.FinancialManagement
{
    [DefaultClassOptions]
    [DefaultProperty("InvoiceNumber")]
    [ImageName("BO_Invoice")]
    [NavigationItem("Financials")]

    public class Invoice : BaseObject
    {
        public Invoice(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
            InvoiceDate = DateTime.Now;

            // Retrieve the existing CompanyInformation instance
            CompanyInformation = Session.Query<CompanyInformation>().FirstOrDefault();
        }


        [Association("Account-Invoices")]
        public Account Account { get => account; set => SetPropertyValue(nameof(Account), ref account, value); }

        CompanyInformation companyInformation;
        PurchaseOrder fromPurchaseOrder;
        DateTime invoiceDueDate;
        Quote fromQuoteNo;
        Account account;
        DateTime invoiceDate;
        string invoiceNumber;
        private bool taxExempt;
        private Opportunity opportunity;

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [VisibleInDetailView(false)]
        public string InvoiceNumber
        {
            get { return invoiceNumber; }
            set { SetPropertyValue(nameof(InvoiceNumber), ref invoiceNumber, value); }
        }

        public DateTime InvoiceDate
        {
            get { return invoiceDate; }
            set { SetPropertyValue(nameof(InvoiceDate), ref invoiceDate, value); }
        }


        public DateTime InvoiceDueDate
        {
            get => invoiceDueDate;
            set => SetPropertyValue(nameof(InvoiceDueDate), ref invoiceDueDate, value);
        }

        [Association("Products-Invoices")]
        public XPCollection<Product> Products { get { return GetCollection<Product>(nameof(Products)); } }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        [Association("Invoice-PurchaseOrders")]
        public XPCollection<PurchaseOrder> PurchaseOrders
        {
            get { return GetCollection<PurchaseOrder>(nameof(PurchaseOrders)); }
        }

        [Association("PurchaseOrder-Invoices")]
        public PurchaseOrder FromPurchaseOrder
        {
            get => fromPurchaseOrder;
            set => SetPropertyValue(nameof(FromPurchaseOrder), ref fromPurchaseOrder, value);
        }


        [Association("Quote-Invoices")]
        public Quote FromQuoteNo
        {
            get => fromQuoteNo;
            set => SetPropertyValue(nameof(FromQuoteNo), ref fromQuoteNo, value);
        }


        [Association("Opportunity-Invoices")]
        public Opportunity Opportunity
        {
            get => opportunity;
            set => SetPropertyValue(nameof(Opportunity), ref opportunity, value);
        }

        public BasicAddress BillingAddress { get; set; }

        public BasicAddress ShippingAddress { get; set; }

        public bool TaxExempt { get => taxExempt; set => SetPropertyValue(nameof(TaxExempt), ref taxExempt, value); }

        [VisibleInDetailView(false)]
        [Association("CompanyInformation-Invoices")]
        public CompanyInformation CompanyInformation
        {
            get => companyInformation;
            set => SetPropertyValue(nameof(CompanyInformation), ref companyInformation, value);
        }

        public PaymentCurrencyType CurrencyType { get; set; }

        public ShippingProviderType ShippingProvider { get; set; }


        private void GenerateInvoiceNumber()
        {
            const string InvoiceNumberFormat = "INV{0}{1}{2:0000}";
            var lastInvoice = Session.Query<Invoice>()?.OrderByDescending(i => i.InvoiceDate).FirstOrDefault();
            if (lastInvoice != null)
            {
                var year = lastInvoice.InvoiceDate.Year;
                var month = lastInvoice.InvoiceDate.Month;
                var sequence = int.Parse(lastInvoice.InvoiceNumber[7..]);
                sequence++;
                var newInvoiceNumber = string.Format(InvoiceNumberFormat, year, month, sequence);
                InvoiceNumber = newInvoiceNumber;
            }
            else
            {
                InvoiceNumber = string.Format(InvoiceNumberFormat, DateTime.Today.Year, DateTime.Today.Month, 1);
            }
        }

        DateTime modifiedOn;
        DateTime createdOn;

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        public DateTime CreatedOn
        {
            get => createdOn;
            set => SetPropertyValue(nameof(CreatedOn), ref createdOn, value);
        }


        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        public DateTime ModifiedOn
        {
            get => modifiedOn;
            set => SetPropertyValue(nameof(ModifiedOn), ref modifiedOn, value);
        }

        protected override void OnSaving()
        {
            if (Session.IsNewObject(this))
            {
                CreatedOn = DateTime.Now;
                AddActivityStreamEntry("created", SecuritySystem.CurrentUser as ApplicationUser);
            }
            else
            {
                AddActivityStreamEntry("modified", SecuritySystem.CurrentUser as ApplicationUser);
            }
            ModifiedOn = DateTime.Now;

            if (Session.IsNewObject(this))
            {
                GenerateInvoiceNumber();
            }
            base.OnSaving();
        }

        private void AddActivityStreamEntry(string action, ApplicationUser applicationUser)
        {
            var activityStreamEntry = new MyActivityStream(Session)
            {
                AccountName = InvoiceNumber,
                Action = action,
                Date = DateTime.Now,
                CreatedBy = applicationUser?.UserName
            };
            activityStreamEntry.Save(GetType().Name); // Pass the class name as a parameter
        }
    }
}