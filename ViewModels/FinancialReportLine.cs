using System.Threading.Channels;
using MAppBnB;

public class FinancialReportLine{
        public Booking Booking{get;set;}
        public Person MainPerson{get;set;}
        public Room Room{get;set;}
        public BookChannel Channel{get;set;}
        public int GuestCount{get;set;}
    }