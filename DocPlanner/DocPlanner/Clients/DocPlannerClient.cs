namespace DocPlanner.Clients
{
    public class DocPlannerClient
    {
        public HttpClient Client { get; }

        public DocPlannerClient(HttpClient httpClient)
        {
            Client = httpClient;
        }
    }
}