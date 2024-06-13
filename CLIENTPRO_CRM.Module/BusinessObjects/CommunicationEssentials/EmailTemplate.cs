﻿using CLIENTPRO_CRM.Module.BusinessObjects.ActivityStreamManagement;
using CLIENTPRO_CRM.Module.BusinessObjects.CustomerManagement;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System.ComponentModel;
using AssociationAttribute = DevExpress.Xpo.AssociationAttribute;

namespace CLIENTPRO_CRM.Module.BusinessObjects.CommunicationEssentials
{
    [ImageName("BO_Resume")]
    [DefaultProperty(nameof(Name))]

    public class EmailTemplate : BaseObject
    {
        /*int id;
        [Key(true)]

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        public int Id
        {
            get { return id; }
            set { SetPropertyValue(nameof(Id), ref id, value); }
        }*/
        public EmailTemplate(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }

        Lead lead;
        Communication email;
        Contact contact;
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetPropertyValue(nameof(Name), ref _name, value?.ToUpper()); }
        }

        private string _subject;
        [Size(200)]
        public string Subject
        {
            get { return _subject; }
            set { SetPropertyValue(nameof(Subject), ref _subject, value?.ToUpper()); }
        }

        [Size(SizeAttribute.Unlimited)]
        [Delayed(true)]
        public string Body
        {
            get { return GetDelayedPropertyValue<string>(nameof(Body)); }
            set { SetDelayedPropertyValue(nameof(Body), value); }
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        [Association("Contact-EmailTemplates")]
        public Contact Contact
        {
            get => contact;
            set => SetPropertyValue(nameof(Contact), ref contact, value);
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        [Association("Lead-EmailTemplates")]
        public Lead Lead
        {
            get => lead;
            set => SetPropertyValue(nameof(Lead), ref lead, value);
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        [Association("Communication-EmailTemplates")]
        public Communication Email
        {
            get => email;
            set => SetPropertyValue(nameof(Email), ref email, value);
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
            base.OnSaving();
        }

        private void AddActivityStreamEntry(string action, ApplicationUser applicationUser)
        {
            var activityStreamEntry = new MyActivityStream(Session)
            {
                AccountName = Name,
                Action = action,
                Date = DateTime.Now,
                CreatedBy = applicationUser?.UserName
            };
            activityStreamEntry.Save(GetType().Name); // Pass the class name as a parameter
        }
    }
}