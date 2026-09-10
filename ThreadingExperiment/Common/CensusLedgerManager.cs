namespace Common;

public class CensusLedgerManager
{
    public async Task<List<EmailTask>> DetermineWhoAndWhatToSendAsync(string collection)
    {
        // Mock
        return EmailTask.Collection(1000);
    }
}