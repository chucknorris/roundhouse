namespace roundhouse.runners
{
    public class RoundhouseNHibernateCompareRunner{
        private readonly string server;
        private readonly string database_name;

        public RoundhouseNHibernateCompareRunner(string server, string database_name)
        {
            this.server = server;
            this.database_name = database_name;
        }

        public void run()
        {
            
        }
    }
}