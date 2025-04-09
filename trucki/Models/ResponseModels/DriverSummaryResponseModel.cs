namespace trucki.Models.ResponseModels;

public class DriverSummaryResponseModel
{
    public WeeklyStats WeeklyStats { get; set; }
    public MonthlyStats MonthlyStats { get; set; }
    public double TotalEarnings { get; set; }
    public int TotalTripsCompleted { get; set; }
    public decimal AverageRating { get; set; }
}

public class WeeklyStats
{
    public int CompletedTrips { get; set; }
    public double Earnings { get; set; }
    public List<DailyTrip> DailyTrips { get; set; }
}

public class MonthlyStats
{
    public int CompletedTrips { get; set; }
    public double Earnings { get; set; }
    public List<WeeklyTrip> WeeklyTrips { get; set; }
}

public class DailyTrip
{
    public DateTime Date { get; set; }
    public int TripCount { get; set; }
    public double Earnings { get; set; }
}

public class WeeklyTrip
{
    public int WeekNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TripCount { get; set; }
    public double Earnings { get; set; }
}
