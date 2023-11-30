namespace DocPlanner.UnitTests
{
    public class MockHttpMessageHandler : DelegatingHandler
    {
        private readonly HttpResponseMessage _mockResponse;

        public MockHttpMessageHandler(HttpResponseMessage responseMessage)
        {
            _mockResponse = responseMessage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_mockResponse);
        }
    }

}
