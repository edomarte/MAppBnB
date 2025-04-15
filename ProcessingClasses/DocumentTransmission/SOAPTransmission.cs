using AlloggiatiService;
using MAppBnB;
using MAppBnB.Models;

// Class implementing the Classes and Methods form the AlloggiatiWebWSDL.
public class SOAPTransmission
{
    public static async Task<string> SendBookingToSOAP(string username, string password, string wskey, List<string> bookings, string accommodationId)
    {
        // Create a new object of class ServiceSoapClient.
        ServiceSoapClient ssc = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);

        // Create a new object of class EsitoOperazioneServizio (Service Operation Result).
        EsitoOperazioneServizio esito = new EsitoOperazioneServizio();
        // Get the autentication token info.
        TokenInfo tokenInfo = ssc.GenerateToken(username, password, wskey, ref esito);
        // If esito (retult) of the autentication is false (fails), return the error.
        if (!esito.esito)
        {
            return esito.ErroreCod + "; " + esito.ErroreDes + ": " + esito.ErroreDettaglio;// If error found, return
        }

        // Create the array of strings adding the formatted string list of bookings.
        ArrayOfString aos = [.. bookings];
        // Create a new ElencoSchedineEsito object for capturing the result of the data sending.
        ElencoSchedineEsito result = new ElencoSchedineEsito();

        // Check if the accommodation is registered as an apartment for the Italian Police or not.
        if (accommodationId.Equals(""))
            await ssc.SendAsync(username, tokenInfo.token, aos, result); // Check and send lines.
            // For TESTS: await ssc.TestAsync(username, tokenInfo.token, aos, ref result); Only check if lines valid.
        else
            // For TESTS: await ssc.GestioneAppartamenti_TestAsync(username, tokenInfo.token, aos, accommodationId, result); // Only check if lines for users with category "Gestione Appartamenti".
            await ssc.GestioneAppartamenti_SendAsync(username, tokenInfo.token, aos, Convert.ToInt32(accommodationId), result); // Check and send lines for users with category "Gestione Appartamenti".
        
        foreach (EsitoOperazioneServizio booking in result.Dettaglio)
        {   
            // For each schedina (booking) check if esito (result) fails return the error.
            if (!booking.esito)
            {
                return booking.ErroreCod + "; " + booking.ErroreDes + ": " + booking.ErroreDettaglio;
            }
        }

        // If no error, return booking sent message.
        return "Schedine sent correctly.";
    }

    // Method to send the docs to the police.
    public static string SendDocsToPolice(Booking booking, List<Person> persons, Document document, Configuration configuration, string AWIDAppartamento)
    {
        // Prepare the Schedine (formatted list of bookings)
        List<string> soapString = prepareSOAPstring(booking, persons, document, configuration, AWIDAppartamento);
        // Return the result of the data transmission.
        return SendBookingToSOAP(configuration.AlloggiatiWebUsername, configuration.AlloggiatiWebPassword, configuration.AlloggiatiWebWSKey, soapString, configuration.IsGestioneAppartamenti ? AWIDAppartamento : "").ToString();
    }

    // Method to prepare the list of strings with the bookings (schedine) as per Alloggiati Web documentation.
    public static List<string> prepareSOAPstring(Booking booking, List<Person> persons, Document document, Configuration configuration, string AWIDAppartamento)
    {
        List<string> schedine = new List<string>();

        foreach (Person person in persons)
        {
            // Foreach person in persons, populate the string as following.
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

            // If a main type of guest (16, 17, 18), populate the document part of the schedina.
            if (Convert.ToInt32(person.RoleRelation) > 15 && Convert.ToInt32(person.RoleRelation) < 19)
            {
                schedina += document.DocumentType
                        + document.SerialNumber.PadRight(20)
                        + document.IssuingCountry.PadRight(9); //Country o comune
            }
            else
            {
                // Otherwise, pad it with blanks as per documentation.
                schedina += "     " // Document type
                        + "                    " // serial number
                        + "         "; //Country o comune
            }
            // Add a new line.
            schedina += "\r\n";

            // If the host has multiple apartments (Gestione Appartamenti) as per Configuration, add the apartment id.
            if (configuration.IsGestioneAppartamenti)
                schedina += AWIDAppartamento.PadRight(6);

            // Add the formatted schedina to the list.
            schedine.Add(schedina);
        }
        // Return the list of schedine.
        return schedine;
    }
}