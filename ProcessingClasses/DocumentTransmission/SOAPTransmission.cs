using System.Data;
using System.Threading.Tasks;
using AlloggiatiService;

public class SOAPTransmission
{

    public async Task<bool> SendBookingToSOAP(string username, string password, string wskey, List<string> bookings, int accommodationId)
    {

        ServiceSoapClient ssc = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap);

        EsitoOperazioneServizio esito = new EsitoOperazioneServizio();
        TokenInfo tokenInfo = ssc.GenerateToken(username, password, wskey, ref esito);
        if (esito.esito)
        {
            Console.Write("Authentication valid");
        }
        else
        {
            Console.Write(esito.ErroreCod + "; " + esito.ErroreDes + ": " + esito.ErroreDettaglio);
            return false;// If error found, return
        }
        
        ArrayOfString aos = [.. bookings];

        ElencoSchedineEsito result = new ElencoSchedineEsito();
        //await ssc.TestAsync(username, tokenInfo.token, aos, ref result); Only check if lines valid.
        //await ssc.SendAsync(username, tokenInfo.token, aos, ref result); // Check and send lines.
        //await ssc.GestioneAppartamenti_TestAsync(username, tokenInfo.token, aos, accommodationId, result); // Only check if lines for users with category "Gestione Appartamenti".
        await ssc.GestioneAppartamenti_SendAsync(username, tokenInfo.token, aos, accommodationId, result); // Check and send lines for users with category "Gestione Appartamenti".
        foreach (EsitoOperazioneServizio booking in result.Dettaglio)
        {
            if (booking.esito)
            {
                Console.Write("Booking valid");
            }
            else
            {
                Console.Write(booking.ErroreCod + "; " + booking.ErroreDes + ": " + booking.ErroreDettaglio);
                return false;// If error found, return
            }
        }

        return true;
    }
}