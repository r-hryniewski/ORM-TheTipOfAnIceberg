namespace RowLevelSecurityPolicyDemo
{
    public class Company : ICompany
    {
        public long Id { get; }

        public Company(long id)
        {
            Id = id;
        }

    }
}