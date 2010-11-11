namespace roundhouse.infrastructure.persistence
{
    using System;
    using System.Security.Principal;
    using NHibernate.Event;
    using NHibernate.Persister.Entity;
    using roundhouse.model;

    public class AuditEventListener : IPreInsertEventListener, IPreUpdateEventListener
    {
        public string get_identity()
        {
            string identity_of_updater = WindowsIdentity.GetCurrent() != null ? WindowsIdentity.GetCurrent().Name : string.Empty;

            return identity_of_updater;
        }

        //http://ayende.com/Blog/archive/2009/04/29/nhibernate-ipreupdateeventlistener-amp-ipreinserteventlistener.aspx
        public bool OnPreInsert(PreInsertEvent event_item)
        {
            Auditable audit = event_item.Entity as Auditable;
            if (audit == null)
            {
                return false;
            }

            DateTime? entry_date = DateTime.Now;
            DateTime? modified_date = DateTime.Now;
            string identity_of_updater = get_identity();

            store(event_item.Persister, event_item.State, "entry_date", entry_date);
            store(event_item.Persister, event_item.State, "modified_date", modified_date);
            store(event_item.Persister, event_item.State, "entered_by", identity_of_updater);
            audit.entry_date = entry_date;
            audit.modified_date = modified_date;
            audit.entered_by = identity_of_updater;

            return false;
        }

        public bool OnPreUpdate(PreUpdateEvent event_item)
        {
            Auditable audit = event_item.Entity as Auditable;
            if (audit == null)
            {
                return false;
            }

            DateTime? modified_date = DateTime.Now;
            string identity_of_updater = get_identity();

            store(event_item.Persister, event_item.State, "modified_date", modified_date);
            store(event_item.Persister, event_item.State, "entered_by", identity_of_updater);
            audit.modified_date = modified_date;
            audit.entered_by = identity_of_updater;

            //insert auditing object here

            return false;
        }

        public void store(IEntityPersister persister, object[] state, string property_name, object value)
        {
            int index = Array.IndexOf(persister.PropertyNames, property_name);
            if (index == -1)
            {
                return;
            }
            state[index] = value;
        }
    }
}