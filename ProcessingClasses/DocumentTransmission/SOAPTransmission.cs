using System.Data;
using System.Threading.Tasks;
using AlloggiatiService;
using MAppBnB;
using MAppBnB.Models;

public class SOAPTransmission
{
    public static async Task<string> SendBookingToSOAP(string username, string password, string wskey, List<string> bookings, string accommodationId)
    {

        ServiceSoapClient ssc = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);

        EsitoOperazioneServizio esito = new EsitoOperazioneServizio();
        TokenInfo tokenInfo = ssc.GenerateToken(username, password, wskey, ref esito);
        if (!esito.esito)
        {
            return esito.ErroreCod + "; " + esito.ErroreDes + ": " + esito.ErroreDettaglio;// If error found, return
        }

        ArrayOfString aos = [.. bookings];
        ElencoSchedineEsito result = new ElencoSchedineEsito();

        // Check if the accommodation is registered as an apartment for the Italian Police or not.
        if (accommodationId.Equals(""))
            await ssc.SendAsync(username, tokenInfo.token, aos, result); // Check and send lines.
            //await ssc.TestAsync(username, tokenInfo.token, aos, ref result); Only check if lines valid.
        else
            //await ssc.GestioneAppartamenti_TestAsync(username, tokenInfo.token, aos, accommodationId, result); // Only check if lines for users with category "Gestione Appartamenti".
            await ssc.GestioneAppartamenti_SendAsync(username, tokenInfo.token, aos, Convert.ToInt32(accommodationId), result); // Check and send lines for users with category "Gestione Appartamenti".
        
        foreach (EsitoOperazioneServizio booking in result.Dettaglio)
        {
            if (!booking.esito)
            {
                return booking.ErroreCod + "; " + booking.ErroreDes + ": " + booking.ErroreDettaglio;// If error found, return
            }
        }

        return "Schedine sent correctly.";
    }

    internal static string SendDocsToPolice(Booking booking, List<Person> persons, Document document, Configuration configuration, string AWIDAppartamento)
    {
        List<string> soapString = prepareSOAPstring(booking, persons, document, configuration, AWIDAppartamento);
        return SendBookingToSOAP(configuration.AlloggiatiWebUsername, configuration.AlloggiatiWebPassword, configuration.AlloggiatiWebWSKey, soapString, configuration.IsGestioneAppartamenti ? AWIDAppartamento : "").ToString();
    }

    internal static List<string> prepareSOAPstring(Booking booking, List<Person> persons, Document document, Configuration configuration, string AWIDAppartamento)
    {
        List<string> schedine = new List<string>();
        foreach (Person person in persons)
        {
            string schedina = "";
            schedina += person.RoleRelation
                    + booking.CheckinDateTime.Date.ToString("dd/MM/yyyy")
                    + (booking.CheckOutDateTime.Date - booking.CheckinDateTime.Date).TotalDays
                    + person.Surname.PadRight(50)
                    + person.Name.PadRight(30)
                    + person.Sex
                    + person.BirthDate.Value.ToString("dd/MM/yyyy")
                    + (person.BirthPlace.Equals("ES") ? "         " : person.BirthPlace)
                    + (person.BirthPlace.Equals("ES") ? "  " : person.BirthProvince)
                    + person.BirthCountry
                    + person.Citizenship;

            if (Convert.ToInt32(person.RoleRelation) > 15 && Convert.ToInt32(person.RoleRelation) < 19)
            {
                schedina += document.DocumentType
                        + document.SerialNumber.PadRight(20)
                        + document.IssuingCountry.PadRight(9); //Country o comune
            }
            else
            {
                schedina += "     " // Document type
                        + "                    " // serial number
                        + "         "; //Country o comune
            }
            schedina += "\r\n";

            if (configuration.IsGestioneAppartamenti)
                schedina += AWIDAppartamento.PadRight(6);

            schedine.Add(schedina);
        }
        return schedine;
    }
}