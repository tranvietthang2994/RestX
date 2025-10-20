namespace RestX.AdminWebApp.Models.HomeViewModels
{
    public class HomeViewModel
    {
        public List<Restaurants> Restaurants { get; set; }
    }

    public class Restaurants { 
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string OwnerName { get; set; }
    }

}
